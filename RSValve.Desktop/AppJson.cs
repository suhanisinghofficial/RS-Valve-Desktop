using System.Text.Json;

namespace RSValve.Desktop;

internal static class AppJson
{
    public static readonly JsonSerializerOptions Indented = new() { WriteIndented = true };

    public static readonly JsonSerializerOptions Api = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
