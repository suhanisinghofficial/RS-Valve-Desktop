using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using RSValve.Desktop.Models;
using RSValve.Desktop.Services;

namespace RSValve.Desktop;

public partial class MainWindow : Window
{
    private string? _updateDownloadUrl;
    private string? _updateRemoteVersion;
    private string? _downloadedInstallerPath;
    private CancellationTokenSource? _updateDownloadCts;
    private readonly UpdateDownloadService _updateDownloader = new();
    private VideosWindow? _videosWindow;

    public MainWindow()
    {
        InitializeComponent();
        WindowIconHelper.Apply(this);
        Loaded += OnLoaded;
        Closing += (_, _) => _updateDownloadCts?.Cancel();
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        VersionBlock.Text = $"v{AppVersionInfo.Display}";
        CopyrightBlock.Text = $"All rights reserved © {DateTime.Now.Year}";

        SetConnectionStatus("Starting…", AppColors.StatusStarting, showError: false);
        StartupErrorBorder.IsVisible = false;
        LobbyInfoBorder.IsVisible = false;

        Topmost = true;
        Activate();
        Topmost = false;

        await App.StartServicesAsync();
        RefreshDashboard();
        await CheckForUpdatesAsync();
    }

    private void OnOpenVideosWindow(object? sender, RoutedEventArgs e)
    {
        if (_videosWindow != null)
        {
            _videosWindow.Activate();
            return;
        }

        _videosWindow = new VideosWindow();
        _videosWindow.Closed += (_, _) =>
        {
            _videosWindow = null;
            RefreshDashboard();
        };
        _videosWindow.Show(this);
    }

    private async void OnOpenSettingsWindow(object? sender, RoutedEventArgs e)
    {
        var dialog = new SettingsWindow();
        await dialog.ShowDialog(this);
        RefreshDashboard();
        if (dialog.SavedSuccessfully)
            await CheckForUpdatesAsync();
    }

    private async void OnRetryConnectionClick(object? sender, RoutedEventArgs e)
    {
        RetryConnectionButton.IsEnabled = false;
        SetConnectionStatus("Retrying…", AppColors.StatusStarting, showError: false);

        await App.RetryConnectionAsync();

        RetryConnectionButton.IsEnabled = true;
        RefreshDashboard();
    }

    public void RefreshDashboard()
    {
        TotalVideosBlock.Text = App.Library.GetAll().Count.ToString();

        if (App.MediaServerError != null)
        {
            SetConnectionStatus("Failure", AppColors.StatusFailure, showError: true, App.MediaServerError.Message);
            UpdateLobbyDisplay(show: false);
            return;
        }

        if (App.IsConnected)
        {
            SetConnectionStatus("Connected", AppColors.StatusConnected, showError: false);
            UpdateLobbyDisplay(show: true);
            return;
        }

        SetConnectionStatus(
            "Failure",
            AppColors.StatusFailure,
            showError: true,
            App.ConnectionError ?? "Could not connect to the server. Check Settings.");
        UpdateLobbyDisplay(show: false);
    }

    private void UpdateLobbyDisplay(bool show)
    {
        LobbyInfoBorder.IsVisible = show;
        if (!show) return;

        var lobby = App.SettingsService.Load().Lobby;
        LobbyBlock.Text = string.IsNullOrWhiteSpace(lobby) ? "—" : lobby.Trim();
    }

    private void SetConnectionStatus(string text, IBrush dotBrush, bool showError, string? errorMessage = null)
    {
        StatusBlock.Text = text;
        StatusBlock.Foreground = dotBrush;
        StatusDot.Fill = dotBrush;
        StartupErrorBorder.IsVisible = showError;
        RetryConnectionButton.IsVisible = showError;
        if (showError && !string.IsNullOrWhiteSpace(errorMessage))
            StartupErrorBlock.Text = errorMessage;
    }

    private async Task CheckForUpdatesAsync()
    {
        var result = await App.UpdateChecker.CheckAsync();
        ApplyUpdateCheckResult(result);
    }

    private void ApplyUpdateCheckResult(UpdateCheckResult result)
    {
        if (result.UpdateAvailable)
        {
            _updateDownloadUrl = result.DownloadUrl;
            _updateRemoteVersion = result.RemoteVersion;
            _downloadedInstallerPath = null;
            ResetUpdateDownloadUi();
            UpdateMessageBlock.Text =
                $"Version {result.RemoteVersion} is available. You are on v{AppVersionInfo.Display}.";
            UpdateAvailableBorder.IsVisible = true;
            return;
        }

        _updateDownloadUrl = null;
        _updateRemoteVersion = null;
        UpdateAvailableBorder.IsVisible = false;
    }

    private void ResetUpdateDownloadUi()
    {
        UpdateProgressBar.IsVisible = false;
        UpdateProgressBar.Value = 0;
        UpdateDownloadStatusBlock.IsVisible = false;
        UpdateDownloadStatusBlock.Text = "";
        DownloadUpdateButton.IsEnabled = true;
        DownloadUpdateButton.Content = UpdateDownloadService.CanAutoInstall ? "Update now" : "Download update";
    }

    private async void OnDownloadUpdateClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(_updateDownloadUrl) || string.IsNullOrEmpty(_updateRemoteVersion))
            return;

        if (!string.IsNullOrEmpty(_downloadedInstallerPath) && File.Exists(_downloadedInstallerPath))
        {
            if (UpdateDownloadService.CanAutoInstall)
            {
                BeginSilentInstall(_downloadedInstallerPath);
                return;
            }

            UpdateDownloadService.TryRevealInFileManager(_downloadedInstallerPath);
            return;
        }

        _updateDownloadCts?.Cancel();
        _updateDownloadCts = new CancellationTokenSource();
        var ct = _updateDownloadCts.Token;

        DownloadUpdateButton.IsEnabled = false;
        UpdateProgressBar.IsVisible = true;
        UpdateProgressBar.Value = 0;
        UpdateDownloadStatusBlock.IsVisible = true;
        UpdateDownloadStatusBlock.Text = "Downloading update…";

        var progress = new Progress<UpdateDownloadProgress>(p =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (p.Percent is int percent)
                {
                    UpdateProgressBar.Value = percent;
                    UpdateDownloadStatusBlock.Text = $"Downloading… {percent}%";
                }
                else
                {
                    var mb = p.BytesReceived / (1024.0 * 1024.0);
                    UpdateDownloadStatusBlock.Text = $"Downloading… {mb:0.0} MB";
                }
            });
        });

        var result = await _updateDownloader.DownloadAsync(
            _updateDownloadUrl,
            _updateRemoteVersion,
            progress,
            ct);

        if (ct.IsCancellationRequested)
            return;

        if (!result.Success || string.IsNullOrEmpty(result.FilePath))
        {
            DownloadUpdateButton.IsEnabled = true;
            UpdateProgressBar.IsVisible = false;
            UpdateDownloadStatusBlock.Text = result.ErrorMessage ?? "Download failed.";
            return;
        }

        _downloadedInstallerPath = result.FilePath;

        if (UpdateDownloadService.CanAutoInstall)
        {
            BeginSilentInstall(result.FilePath);
            return;
        }

        UpdateProgressBar.IsVisible = false;
        UpdateDownloadStatusBlock.Text =
            $"Update saved to:{Environment.NewLine}{result.FilePath}{Environment.NewLine}Install this file on Windows.";
        DownloadUpdateButton.IsEnabled = true;
        DownloadUpdateButton.Content = "Show in folder";
    }

    private void BeginSilentInstall(string installerPath)
    {
        DownloadUpdateButton.IsEnabled = false;
        UpdateProgressBar.IsVisible = false;
        UpdateDownloadStatusBlock.IsVisible = true;
        UpdateDownloadStatusBlock.Text = "Installing update… The app will close and restart.";

        if (UpdateDownloadService.TryStartSilentInstallAndExit(installerPath))
            return;

        DownloadUpdateButton.IsEnabled = true;
        UpdateDownloadStatusBlock.Text = "Could not start the installer.";
    }
}
