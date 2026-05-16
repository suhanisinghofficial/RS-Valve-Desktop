using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

internal static class SettingsValidator
{
    public static string? Validate(AppSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.MainUrl))
            return "Main URL is required.";

        if (!Uri.TryCreate(settings.MainUrl.Trim(), UriKind.Absolute, out var uri)
            || uri.Scheme is not ("http" or "https"))
            return "Main URL must be a valid http or https address.";

        if (string.IsNullOrWhiteSpace(settings.Lobby))
            return "Lobby is required.";

        if (string.IsNullOrWhiteSpace(settings.Token))
            return "Token is required.";

        return null;
    }
}
