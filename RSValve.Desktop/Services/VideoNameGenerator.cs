using System.Text.RegularExpressions;

namespace RSValve.Desktop.Services;

internal static partial class VideoNameGenerator
{
    public static string FromFileName(string sourcePath, IEnumerable<string> existingNames)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourcePath);
        var cleaned = NonAlphanumericRegex().Replace(baseName, " ");
        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var name = string.Join(" ", words.Select(w => w.ToUpperInvariant()));

        if (string.IsNullOrWhiteSpace(name))
            return GenerateNext(existingNames);

        return EnsureUnique(name, existingNames);
    }

    public static string GenerateNext(IEnumerable<string> existingNames)
    {
        var used = new HashSet<string>(
            existingNames.Select(NormalizeForCompare),
            StringComparer.OrdinalIgnoreCase);

        for (var n = 1; n <= 9999; n++)
        {
            var candidate = $"Video {n:D3}";
            if (!used.Contains(NormalizeForCompare(candidate)))
                return candidate;
        }

        return $"Video {Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    public static string SanitizeDisplayName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "";
        var trimmed = name.Trim();
        if (trimmed.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
            trimmed = trimmed[..^4].TrimEnd();
        trimmed = NonAlphanumericRegex().Replace(trimmed, " ");
        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Join(" ", words.Select(w => w.ToUpperInvariant()));
    }

    public static string EnsureUniqueForRename(string displayName, IEnumerable<string> otherDisplayNames) =>
        EnsureUnique(SanitizeDisplayName(displayName), otherDisplayNames);

    private static string EnsureUnique(string baseName, IEnumerable<string> existingNames)
    {
        var used = new HashSet<string>(
            existingNames.Select(NormalizeForCompare),
            StringComparer.OrdinalIgnoreCase);

        if (!used.Contains(NormalizeForCompare(baseName)))
            return baseName;

        for (var i = 2; i <= 999; i++)
        {
            var candidate = $"{baseName} {i}";
            if (!used.Contains(NormalizeForCompare(candidate)))
                return candidate;
        }

        return GenerateNext(existingNames);
    }

    private static string NormalizeForCompare(string name) =>
        SanitizeDisplayName(name).ToLowerInvariant();

    [GeneratedRegex(@"[^a-zA-Z0-9]+")]
    private static partial Regex NonAlphanumericRegex();
}
