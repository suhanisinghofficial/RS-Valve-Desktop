using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using RSValve.Desktop.Models;
using RSValve.Desktop.Services;

namespace RSValve.Desktop;

public partial class SettingsWindow : Window
{
    private static readonly SolidColorBrush MessageSuccessBrush = new(Color.Parse("#1b7f3b"));
    private static readonly SolidColorBrush MessageErrorBrush = new(Color.Parse("#b91c1c"));
    private static readonly SolidColorBrush MessageSuccessBg = new(Color.Parse("#f0fdf4"));
    private static readonly SolidColorBrush MessageErrorBg = new(Color.Parse("#fef2f2"));

    private AppSettings? _editingSettings;

    public bool SavedSuccessfully { get; private set; }

    public SettingsWindow()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        SavedSuccessfully = false;
        _editingSettings = App.SettingsService.Load();
        MainUrlBox.Text = _editingSettings.MainUrl;
        LobbyBox.Text = _editingSettings.Lobby;
        TokenBox.Text = _editingSettings.Token;
        SettingsMessageBorder.IsVisible = false;
    }

    private void OnCloseClick(object? sender, RoutedEventArgs e) => Close();

    private async void OnSaveClick(object? sender, RoutedEventArgs e)
    {
        var settings = new AppSettings
        {
            MainUrl = MainUrlBox.Text?.Trim() ?? "",
            Lobby = LobbyBox.Text?.Trim() ?? "",
            Token = string.IsNullOrEmpty(TokenBox.Text)
                ? (_editingSettings?.Token ?? "")
                : TokenBox.Text
        };

        var validationError = SettingsValidator.Validate(settings);
        if (validationError != null)
        {
            ShowMessage(validationError, MessageErrorBrush, MessageErrorBg, "#fecaca");
            return;
        }

        SaveSettingsButton.IsEnabled = false;
        SettingsMessageBorder.IsVisible = false;

        var result = await App.SaveSettingsAndRegisterAsync(settings);
        _editingSettings = settings;
        SaveSettingsButton.IsEnabled = true;

        if (result.Success)
        {
            SavedSuccessfully = true;
            ShowMessage("Saved and connected successfully.", MessageSuccessBrush, MessageSuccessBg, "#bbf7d0");
            await Task.Delay(600);
            Close();
            return;
        }

        ShowMessage(result.ErrorMessage ?? "Connection failed.", MessageErrorBrush, MessageErrorBg, "#fecaca");
    }

    private void ShowMessage(string text, IBrush foreground, IBrush background, string borderColor)
    {
        SettingsMessageBlock.Text = text;
        SettingsMessageBlock.Foreground = foreground;
        SettingsMessageBorder.Background = background;
        SettingsMessageBorder.BorderBrush = new SolidColorBrush(Color.Parse(borderColor));
        SettingsMessageBorder.BorderThickness = new Thickness(1);
        SettingsMessageBorder.IsVisible = true;
    }
}
