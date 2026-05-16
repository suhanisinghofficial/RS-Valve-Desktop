using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using RSValve.Desktop.Models;
using RSValve.Desktop.Services;

namespace RSValve.Desktop;

public partial class SettingsWindow : Window
{
    private AppSettings? _editingSettings;

    public bool SavedSuccessfully { get; private set; }

    public SettingsWindow()
    {
        InitializeComponent();
        WindowIconHelper.Apply(this);
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
        var settings = ReadSettingsFromForm();

        var validationError = SettingsValidator.Validate(settings);
        if (validationError != null)
        {
            ShowMessage(validationError, isError: true);
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
            ShowMessage("Saved and connected successfully.", isError: false);
            await Task.Delay(600);
            Close();
            return;
        }

        ShowMessage(result.ErrorMessage ?? "Connection failed.", isError: true);
    }

    private AppSettings ReadSettingsFromForm() => new()
    {
        MainUrl = MainUrlBox.Text?.Trim() ?? "",
        Lobby = LobbyBox.Text?.Trim() ?? "",
        Token = string.IsNullOrEmpty(TokenBox.Text) ? (_editingSettings?.Token ?? "") : TokenBox.Text
    };

    private void ShowMessage(string text, bool isError)
    {
        SettingsMessageBlock.Text = text;
        SettingsMessageBlock.Foreground = isError ? AppColors.MessageError : AppColors.MessageSuccess;
        SettingsMessageBorder.Background = isError ? AppColors.MessageErrorBg : AppColors.MessageSuccessBg;
        SettingsMessageBorder.BorderBrush = AppColors.Hex(isError ? "#fecaca" : "#bbf7d0");
        SettingsMessageBorder.BorderThickness = new Thickness(1);
        SettingsMessageBorder.IsVisible = true;
    }
}
