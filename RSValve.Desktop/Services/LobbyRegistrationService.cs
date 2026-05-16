using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public sealed class LobbyRegistrationService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public async Task<RegistrationResult> RegisterAsync(AppSettings settings, string localVideoListUrl, CancellationToken ct = default)
    {
        var validationError = SettingsValidator.Validate(settings);
        if (validationError != null)
            return RegistrationResult.Fail(validationError);

        var baseUrl = settings.MainUrl.Trim().TrimEnd('/');
        var lobby = Uri.EscapeDataString(settings.Lobby.Trim());
        var url = $"{baseUrl}/reaction-stats/update-lobby-media/{lobby}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.TryAddWithoutValidation("X-Lobby-Trigger-Token", settings.Token.Trim());
        request.Content = JsonContent.Create(new UpdateLobbyMediaRequest(localVideoListUrl));

        try
        {
            using var response = await Http.SendAsync(request, ct);
            if (response.IsSuccessStatusCode)
                return RegistrationResult.Ok();

            var body = await response.Content.ReadAsStringAsync(ct);
            var detail = string.IsNullOrWhiteSpace(body)
                ? response.ReasonPhrase ?? "Request failed"
                : body.Trim();
            if (detail.Length > 200) detail = detail[..200] + "…";
            return RegistrationResult.Fail($"Server returned {(int)response.StatusCode}: {detail}");
        }
        catch (Exception ex)
        {
            return RegistrationResult.Fail(ex.Message);
        }
    }

    private sealed record UpdateLobbyMediaRequest(
        [property: JsonPropertyName("local_video_list_url")] string LocalVideoListUrl);
}

public readonly record struct RegistrationResult(bool Success, string? ErrorMessage)
{
    public static RegistrationResult Ok() => new(true, null);
    public static RegistrationResult Fail(string message) => new(false, message);
}
