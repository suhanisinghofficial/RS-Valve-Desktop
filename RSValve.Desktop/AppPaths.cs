namespace RSValve.Desktop;

internal static class AppPaths
{
    private static readonly Lazy<string> DataDir = new(() =>
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            AppConstants.ApplicationName);
        Directory.CreateDirectory(dir);
        return dir;
    });

    public static string DataDirectory => DataDir.Value;
    public static string SettingsFile => Path.Combine(DataDirectory, "settings.json");
    public static string LibraryDirectory => Path.Combine(DataDirectory, "Library");
    public static string VideosDirectory => Path.Combine(LibraryDirectory, "videos");
    public static string LibraryIndexFile => Path.Combine(LibraryDirectory, "library.json");
}
