using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using RSValve.Desktop.Models;
using RSValve.Desktop.Services;

namespace RSValve.Desktop;

public partial class App : Application
{
    private static readonly SemaphoreSlim StartLock = new(1, 1);
    private static bool _servicesStarted;

    public static VideoLibraryService Library { get; private set; } = null!;
    public static LocalMediaServer MediaServer { get; private set; } = null!;
    public static AppSettingsService SettingsService { get; private set; } = null!;
    public static LobbyRegistrationService RegistrationService { get; private set; } = null!;
    public static UpdateCheckService UpdateChecker { get; private set; } = null!;

    public static Exception? MediaServerError { get; private set; }
    public static bool IsConnected { get; private set; }
    public static string? ConnectionError { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        try
        {
            Library = new VideoLibraryService();
            Library.Initialize();

            SettingsService = new AppSettingsService();
            RegistrationService = new LobbyRegistrationService();
            UpdateChecker = new UpdateCheckService();
            MediaServer = new LocalMediaServer(Library);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = new MainWindow();
                desktop.MainWindow = window;
                window.Show();
                window.Activate();
                desktop.Exit += (_, _) =>
                {
                    if (_servicesStarted)
                        MediaServer.StopAsync().GetAwaiter().GetResult();
                };
            }

            StartupLog.Write("UI initialized.");
            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception ex)
        {
            StartupLog.Write(ex);
            throw;
        }
    }

    public static async Task StartServicesAsync()
    {
        await StartLock.WaitAsync();
        try
        {
            if (_servicesStarted) return;

            MediaServerError = null;
            try
            {
                await MediaServer.StartAsync();
                var settings = SettingsService.Load();
                var reg = await RegistrationService.RegisterAsync(settings, MediaServer.VideoListUrl);
                ApplyRegistrationResult(reg);
            }
            catch (Exception ex)
            {
                MediaServerError = ex;
                IsConnected = false;
                ConnectionError = ex.Message;
            }

            _servicesStarted = true;
        }
        finally
        {
            StartLock.Release();
        }
    }

    public static async Task<RegistrationResult> SaveSettingsAndRegisterAsync(AppSettings settings)
    {
        if (!_servicesStarted || MediaServerError != null)
        {
            if (MediaServerError != null)
                return RegistrationResult.Fail(MediaServerError.Message);

            await StartServicesAsync();
        }

        if (MediaServerError != null)
            return RegistrationResult.Fail(MediaServerError.Message);

        var result = await RegistrationService.RegisterAsync(settings, MediaServer.VideoListUrl);
        if (result.Success)
            SettingsService.Save(settings);

        ApplyRegistrationResult(result);
        return result;
    }

    public static async Task RetryConnectionAsync()
    {
        await StartLock.WaitAsync();
        try
        {
            MediaServerError = null;
            ConnectionError = null;

            if (MediaServer.Port == 0)
                await MediaServer.StartAsync();

            var settings = SettingsService.Load();
            var result = await RegistrationService.RegisterAsync(settings, MediaServer.VideoListUrl);
            ApplyRegistrationResult(result);
        }
        catch (Exception ex)
        {
            MediaServerError = ex;
            IsConnected = false;
            ConnectionError = ex.Message;
        }
        finally
        {
            StartLock.Release();
        }
    }

    public static async Task SyncLobbyIfConnectedAsync()
    {
        if (!IsConnected || MediaServerError != null) return;
        await SaveSettingsAndRegisterAsync(SettingsService.Load());
    }

    private static void ApplyRegistrationResult(RegistrationResult result)
    {
        IsConnected = result.Success;
        ConnectionError = result.ErrorMessage;
    }
}
