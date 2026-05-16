using System.Text.Json;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public sealed class UpdateCheckService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public async Task<UpdateCheckResult> CheckAsync(AppSettings settings, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(settings.MainUrl))
            return UpdateCheckResult.UpToDate();

        var baseUrl = settings.MainUrl.Trim().TrimEnd('/');
        var manifestUrl = $"{baseUrl}/releases/window/update.json";

        try
        {
            await using var stream = await Http.GetStreamAsync(manifestUrl, ct);
            var manifest = await JsonSerializer.DeserializeAsync<UpdateManifest>(
                stream,
                AppJson.Manifest,
                ct);

            if (manifest == null || string.IsNullOrWhiteSpace(manifest.Version))
                return UpdateCheckResult.UpToDate();

            if (!AppVersionInfo.IsRemoteNewer(manifest.Version))
                return UpdateCheckResult.UpToDate();

            var file = string.IsNullOrWhiteSpace(manifest.File) ? "RS-VALVE.exe" : manifest.File.Trim();
            return UpdateCheckResult.Available(manifest.Version.Trim(), $"{baseUrl}/releases/window/{file}");
        }
        catch
        {
            return UpdateCheckResult.UpToDate();
        }
    }
}
