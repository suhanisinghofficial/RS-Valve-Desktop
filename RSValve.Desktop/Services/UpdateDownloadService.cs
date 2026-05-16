using System.Diagnostics;
using System.Net.Http.Headers;

namespace RSValve.Desktop.Services;

public readonly record struct UpdateDownloadProgress(long BytesReceived, long? TotalBytes)
{
    public int? Percent =>
        TotalBytes is > 0 ? (int)Math.Clamp(BytesReceived * 100 / TotalBytes.Value, 0, 100) : null;
}

public readonly record struct UpdateDownloadResult(bool Success, string? FilePath, string? ErrorMessage)
{
    public static UpdateDownloadResult Ok(string path) => new(true, path, null);
    public static UpdateDownloadResult Fail(string message) => new(false, null, message);
}

public sealed class UpdateDownloadService
{
    private static readonly HttpClient Http = CreateClient();

    public async Task<UpdateDownloadResult> DownloadAsync(
        string downloadUrl,
        string version,
        IProgress<UpdateDownloadProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(downloadUrl))
            return UpdateDownloadResult.Fail("Download URL is missing.");

        try
        {
            var fileName = $"RS-Valve-Setup-v{AppVersionInfo.NormalizeTag(version)}.exe";
            var directory = GetDownloadDirectory();
            Directory.CreateDirectory(directory);
            var destinationPath = Path.Combine(directory, fileName);

            using var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl);
            using var response = await Http.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
                return UpdateDownloadResult.Fail($"Download failed ({(int)response.StatusCode}).");

            var totalBytes = response.Content.Headers.ContentLength;
            await using var input = await response.Content.ReadAsStreamAsync(cancellationToken);
            await using var output = File.Create(destinationPath);

            var buffer = new byte[81920];
            long received = 0;
            int read;

            while ((read = await input.ReadAsync(buffer, cancellationToken)) > 0)
            {
                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                received += read;
                progress?.Report(new UpdateDownloadProgress(received, totalBytes));
            }

            progress?.Report(new UpdateDownloadProgress(received, received));
            return UpdateDownloadResult.Ok(destinationPath);
        }
        catch (OperationCanceledException)
        {
            return UpdateDownloadResult.Fail("Download cancelled.");
        }
        catch (Exception ex)
        {
            return UpdateDownloadResult.Fail(ex.Message);
        }
    }

    public static bool TryRunInstaller(string installerPath)
    {
        if (!File.Exists(installerPath))
            return false;

        if (!OperatingSystem.IsWindows())
            return TryRevealInFileManager(installerPath);

        try
        {
            Process.Start(new ProcessStartInfo(installerPath) { UseShellExecute = true });
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool TryRevealInFileManager(string filePath)
    {
        if (!File.Exists(filePath))
            return false;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo("explorer.exe", $"/select,\"{filePath}\"")
                {
                    UseShellExecute = true
                });
                return true;
            }

            if (OperatingSystem.IsMacOS())
            {
                Process.Start(new ProcessStartInfo("open", $"-R \"{filePath}\"")
                {
                    UseShellExecute = false
                });
                return true;
            }

            var folder = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(folder))
                Process.Start(new ProcessStartInfo("xdg-open", folder) { UseShellExecute = false });
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string GetDownloadDirectory()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var downloads = Path.Combine(home, "Downloads");
        if (Directory.Exists(downloads))
            return downloads;

        var fallback = Path.Combine(AppPaths.DataDirectory, "updates");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private static HttpClient CreateClient()
    {
        var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
        client.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("RS-Valve-Desktop", AppVersionInfo.Display));
        return client;
    }
}
