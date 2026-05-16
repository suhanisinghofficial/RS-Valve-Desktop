namespace RSValve.Desktop.Models;

public readonly record struct UpdateCheckResult(
    bool UpdateAvailable,
    string? RemoteVersion,
    string? DownloadUrl)
{
    public static UpdateCheckResult UpToDate() => new(false, null, null);

    public static UpdateCheckResult Available(string version, string downloadUrl) =>
        new(true, version, downloadUrl);
}
