using System.Net.Http.Headers;
using System.Text.Json;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public sealed class UpdateCheckService
{
    private static readonly HttpClient Http = CreateClient();

    public async Task<UpdateCheckResult> CheckAsync(CancellationToken ct = default)
    {
        var url =
            $"https://api.github.com/repos/{AppConstants.GitHubOwner}/{AppConstants.GitHubRepo}/releases/latest";

        try
        {
            using var response = await Http.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
                return UpdateCheckResult.UpToDate();

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            var release = await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, AppJson.Api, ct);
            if (release == null || string.IsNullOrWhiteSpace(release.TagName))
                return UpdateCheckResult.UpToDate();

            var remoteVersion = release.TagName.Trim();
            if (!AppVersionInfo.IsRemoteNewer(remoteVersion))
                return UpdateCheckResult.UpToDate();

            var downloadUrl = PickSetupDownloadUrl(release);
            if (string.IsNullOrEmpty(downloadUrl))
                downloadUrl = release.HtmlUrl;

            return UpdateCheckResult.Available(
                AppVersionInfo.NormalizeTag(remoteVersion),
                downloadUrl);
        }
        catch
        {
            return UpdateCheckResult.UpToDate();
        }
    }

    private static string? PickSetupDownloadUrl(GitHubRelease release)
    {
        foreach (var asset in release.Assets)
        {
            if (string.Equals(asset.Name, AppConstants.GitHubSetupAssetName, StringComparison.OrdinalIgnoreCase))
                return asset.BrowserDownloadUrl;
        }

        foreach (var asset in release.Assets)
        {
            var name = asset.Name;
            if (name.Contains("Setup", StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                return asset.BrowserDownloadUrl;
        }

        return null;
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("RS-Valve-Desktop", AppVersionInfo.Current));
        client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        return client;
    }
}
