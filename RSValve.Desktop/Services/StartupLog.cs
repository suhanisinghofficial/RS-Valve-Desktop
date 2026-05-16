namespace RSValve.Desktop.Services;

internal static class StartupLog
{
    private static readonly object Lock = new();
    private static string? _logPath;

    public static string LogPath
    {
        get
        {
            if (_logPath != null) return _logPath;

            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                AppConstants.ApplicationName,
                "logs");
            Directory.CreateDirectory(dir);
            _logPath = Path.Combine(dir, "startup.log");
            return _logPath;
        }
    }

    public static void Write(string message)
    {
        try
        {
            var line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            lock (Lock)
                File.AppendAllText(LogPath, line);
        }
        catch
        {
        }
    }

    public static void Write(Exception ex) =>
        Write($"{ex.GetType().Name}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
}
