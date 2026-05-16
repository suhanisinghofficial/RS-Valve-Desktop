using System.Diagnostics;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public static class VideoPlaybackService
{
    public static void Play(LibraryEntry entry)
    {
        var path = App.Library.TryGetStoredPath(entry.Id);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            throw new InvalidOperationException("Video file was not found on disk.");

        if (!LaunchExternal(path))
            throw new InvalidOperationException(
                "Could not open the video. Set a default video player on your system (e.g. QuickTime on Mac).");
    }

    public static bool TryOpenUrl(string url) => LaunchExternal(url);

    private static bool LaunchExternal(string target)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
                return true;
            }

            var fileName = OperatingSystem.IsMacOS() ? "open" : "xdg-open";
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = false
            };
            psi.ArgumentList.Add(target);
            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
