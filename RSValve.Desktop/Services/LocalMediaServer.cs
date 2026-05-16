using System.Net;
using System.Net.Sockets;
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

        app.MapGet("/videos", ListVideos);
        app.MapGet("/api/videos", ListVideos);
        app.MapGet("/media/{id}", ServeMedia);

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
