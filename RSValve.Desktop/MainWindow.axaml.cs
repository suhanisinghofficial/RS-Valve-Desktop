using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Media;
using RSValve.Desktop.Models;
using RSValve.Desktop.Services;

namespace RSValve.Desktop;

public partial class MainWindow : Window
{
    private string? _updateDownloadUrl;
    private VideosWindow? _videosWindow;

    public MainWindow()
    {
        InitializeComponent();
        WindowIconHelper.Apply(this);
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        VersionBlock.Text = $"v{AppVersionInfo.Current}";
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
            UpdateMessageBlock.Text =
                $"Version {result.RemoteVersion} is available. You are on v{AppVersionInfo.Current}.";
            UpdateAvailableBorder.IsVisible = true;
            return;
        }

        _updateDownloadUrl = null;
        UpdateAvailableBorder.IsVisible = false;
    }

    private void OnDownloadUpdateClick(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(_updateDownloadUrl))
            VideoPlaybackService.TryOpenUrl(_updateDownloadUrl);
    }
}
