using System.Net;
using System.Net.Sockets;
using RSValve.Desktop;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public sealed class LocalMediaServer
{
    private const int PortMin = 10240;
    private const int PortMax = 49151;
    private const string LoopbackBase = "http://127.0.0.1";

    private WebApplication? _app;
    private readonly VideoLibraryService _library;

    public LocalMediaServer(VideoLibraryService library) => _library = library;

    public int Port { get; private set; }

    public string VideoListUrl => Port > 0 ? $"{LoopbackBase}:{Port}/videos" : "";

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await StopAsync(cancellationToken);

        Port = PickRandomAvailablePort();

        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        builder.WebHost.UseSetting(WebHostDefaults.ServerUrlsKey, $"{LoopbackBase}:{Port}");
        builder.Logging.ClearProviders();

        var app = builder.Build();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
            await next();
        });

        app.MapGet("/", () => Results.Text(
            $"{AppConstants.ApplicationName} — local media. Try GET /videos",
            "text/plain; charset=utf-8"));

        app.MapGet("/videos", ListVideos);
        app.MapGet("/api/videos", ListVideos);

        app.MapGet("/media/{id}", ServeMedia);
        app.MapGet("/player/{id}", ServePlayerPage);

        _app = app;
        await _app.StartAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (_app == null) return;

        await _app.StopAsync(cancellationToken);
        await _app.DisposeAsync();
        _app = null;
        Port = 0;
    }

    private IResult ServeMedia(string id)
    {
        var path = _library.TryGetStoredPath(id);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return Results.NotFound();
        return Results.File(path, contentType: "video/mp4", enableRangeProcessing: true);
    }

    private IResult ServePlayerPage(string id)
    {
        var path = _library.TryGetStoredPath(id);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return Results.NotFound();

        var title = _library.GetAll().FirstOrDefault(v => v.Id == id)?.DisplayName ?? "Video";
        var encodedTitle = WebUtility.HtmlEncode(title);
        var html = $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>{{encodedTitle}}</title>
              <style>
                * { margin: 0; box-sizing: border-box; }
                html, body { height: 100%; background: #0f172a; color: #e2e8f0; font-family: system-ui, sans-serif; }
                .wrap { display: flex; flex-direction: column; height: 100%; }
                header { padding: 10px 14px; background: #1e293b; font-size: 14px; font-weight: 600; border-bottom: 1px solid #334155; }
                video { flex: 1; width: 100%; background: #000; object-fit: contain; }
              </style>
            </head>
            <body>
              <div class="wrap">
                <header>{{encodedTitle}}</header>
                <video src="/media/{{WebUtility.HtmlEncode(id)}}" controls autoplay playsinline></video>
              </div>
            </body>
            </html>
            """;
        return Results.Content(html, "text/html; charset=utf-8");
    }

    private IResult ListVideos()
    {
        var baseUrl = $"{LoopbackBase}:{Port}";
        var items = _library.GetAll()
            .Select(v => new VideoListItemDto(v.Id, v.DisplayName, $"{baseUrl}/media/{v.Id}"))
            .ToList();
        return Results.Json(items);
    }

    private static int PickRandomAvailablePort()
    {
        var rng = Random.Shared;
        var tried = new HashSet<int>();

        for (var attempt = 0; attempt < 80; attempt++)
        {
            var port = rng.Next(PortMin, PortMax + 1);
            if (!tried.Add(port)) continue;

            if (TryBindLoopback(port))
                return port;
        }

        for (var port = PortMin; port <= PortMax; port++)
        {
            if (TryBindLoopback(port))
                return port;
        }

        throw new InvalidOperationException("Could not find a free port for the local video server.");
    }

    private static bool TryBindLoopback(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
