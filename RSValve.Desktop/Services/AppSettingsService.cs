using System.Text.Json;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public sealed class AppSettingsService
{
    public AppSettings Load()
    {
        if (!File.Exists(AppPaths.SettingsFile))
            return new AppSettings();

        try
        {
            var json = File.ReadAllText(AppPaths.SettingsFile);
            return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var json = JsonSerializer.Serialize(settings, AppJson.Indented);
        File.WriteAllText(AppPaths.SettingsFile, json);
    }
}
