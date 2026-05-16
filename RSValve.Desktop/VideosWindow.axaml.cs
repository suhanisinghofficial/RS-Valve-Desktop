using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using RSValve.Desktop.Models;
using RSValve.Desktop.Services;

namespace RSValve.Desktop;

public partial class VideosWindow : Window
{
    public VideosWindow()
    {
        InitializeComponent();
        Loaded += (_, _) => RefreshList();
    }

    private void NotifyOwnerDashboard() =>
        (Owner as MainWindow)?.RefreshDashboard();

    private void RefreshList()
    {
        var rows = App.Library.GetAll()
            .Select((entry, index) =>
            {
                var path = App.Library.TryGetStoredPath(entry.Id);
                return LibraryEntryRow.FromEntry(index + 1, entry, path);
            })
            .ToList();

        VideoTable.ItemsSource = rows;
        EmptyVideosPanel.IsVisible = rows.Count == 0;
        VideoTable.IsVisible = rows.Count > 0;
        VideoCountBlock.Text = rows.Count == 1
            ? "1 video in library"
            : $"{rows.Count} videos in library";

        NotifyOwnerDashboard();
    }

    private async void OnAddClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Add video",
            AllowMultiple = true,
            FileTypeFilter =
            [
                new FilePickerFileType("MP4 video") { Patterns = ["*.mp4"] }
            ]
        });

        if (files.Count == 0) return;

        var added = 0;
        var skipped = 0;
        foreach (var f in files)
        {
            var path = f.TryGetLocalPath();
            if (string.IsNullOrEmpty(path) || App.Library.TryAddCopy(path) == null)
                skipped++;
            else
                added++;
        }

        RefreshList();

        if (skipped > 0)
        {
            await DialogHelper.ShowInfoAsync(
                this,
                "Add video",
                $"Added {added} file(s). Skipped {skipped} (not MP4 / invalid file / could not copy).");
        }
        else if (added > 0)
            await App.SyncLobbyIfConnectedAsync();
    }

    private async void OnEditVideoNameClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not Button btn || btn.Tag is not LibraryEntry entry) return;

        var raw = await DialogHelper.PromptTextAsync(
            this,
            "Edit video name",
            "Video name",
            entry.DisplayName);

        if (raw == null) return;

        var newName = VideoNameGenerator.SanitizeDisplayName(raw);
        if (string.IsNullOrEmpty(newName))
        {
            await DialogHelper.ShowInfoAsync(this, "Edit name", "Video name cannot be empty.");
            return;
        }

        if (!App.Library.TryUpdateDisplayName(entry.Id, newName))
            return;

        RefreshList();
        await App.SyncLobbyIfConnectedAsync();
    }

    private async void OnPlayRowClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not Button btn || btn.Tag is not LibraryEntry entry) return;

        try
        {
            VideoPlaybackService.Play(entry);
        }
        catch (Exception ex)
        {
            await DialogHelper.ShowInfoAsync(this, "Play video", ex.Message);
        }
    }

    private async void OnDeleteRowClick(object? sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (sender is not Button btn || btn.Tag is not LibraryEntry entry) return;

        var ok = await DialogHelper.ConfirmAsync(
            this,
            "Delete video",
            $"Delete “{entry.DisplayName}” from the library? The file will be removed from disk.");

        if (!ok) return;

        App.Library.TryRemove(entry.Id);
        RefreshList();
        await App.SyncLobbyIfConnectedAsync();
    }
}
