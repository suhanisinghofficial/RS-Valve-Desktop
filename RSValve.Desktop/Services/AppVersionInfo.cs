using System.Reflection;

namespace RSValve.Desktop.Services;

public static class AppVersionInfo
{
    public static string Current
    {
        get
        {
            var asm = Assembly.GetExecutingAssembly();
            return asm.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? asm.GetName().Version?.ToString()
                   ?? "0.0.0";
        }
    }

    public static bool IsRemoteNewer(string remoteVersion, string? currentVersion = null)
    {
        currentVersion ??= Current;
        if (!TryParseVersion(remoteVersion, out var remote))
            return false;
        if (!TryParseVersion(currentVersion, out var current))
            return false;
        return remote > current;
    }

    private static bool TryParseVersion(string text, out Version version)
    {
        version = new Version(0, 0);
        if (string.IsNullOrWhiteSpace(text)) return false;

        var cleaned = text.Trim();
        var plus = cleaned.IndexOf('+');
        if (plus > 0) cleaned = cleaned[..plus];

        if (Version.TryParse(cleaned, out var parsed))
        {
            version = parsed;
            return true;
        }

        return false;
    }
}
