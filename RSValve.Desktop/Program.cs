using System.Runtime.InteropServices;
using Avalonia;

namespace RSValve.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            Services.StartupLog.Write("Starting application…");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Services.StartupLog.Write(ex);
            ShowFatalError(
                "RS VALVE APPLICATION could not start.",
                $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Details were saved to:{Environment.NewLine}{Services.StartupLog.LogPath}");
            Environment.Exit(1);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void ShowFatalError(string title, string message)
    {
        if (OperatingSystem.IsWindows())
        {
            MessageBoxW(IntPtr.Zero, message, title, 0x10); // MB_ICONERROR
            return;
        }

        Console.Error.WriteLine($"{title}{Environment.NewLine}{message}");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "MessageBoxW")]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
