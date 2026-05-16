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

    public static string NormalizeTag(string tag)
    {
        var cleaned = tag.Trim();
        if (cleaned.Length > 1 && cleaned[0] is 'v' or 'V' && char.IsDigit(cleaned[1]))
            cleaned = cleaned[1..];
        return cleaned;
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

        var cleaned = NormalizeTag(text);
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
