using System.Text.Json.Serialization;

namespace RSValve.Desktop.Models;

public sealed class UpdateManifest
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "";

    [JsonPropertyName("file")]
    public string File { get; set; } = "";
}

public readonly record struct UpdateCheckResult(
    bool UpdateAvailable,
    string? RemoteVersion,
    string? DownloadUrl)
{
    public static UpdateCheckResult UpToDate() => new(false, null, null);

    public static UpdateCheckResult Available(string version, string downloadUrl) =>
        new(true, version, downloadUrl);
}
