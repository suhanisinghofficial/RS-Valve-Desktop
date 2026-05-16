using System.Runtime.InteropServices;
using Avalonia;

namespace RSValve.Desktop;

internal static class Program
{
    private const uint MbIconError = 0x10;

    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Services.StartupLog.Write(ex);
            ShowFatalError(
                "RS VALVE APPLICATION could not start.",
                $"{ex.Message}{Environment.NewLine}{Environment.NewLine}Log: {Services.StartupLog.LogPath}");
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
            MessageBoxW(IntPtr.Zero, message, title, MbIconError);
            return;
        }

        Console.Error.WriteLine($"{title}{Environment.NewLine}{message}");
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "MessageBoxW")]
    private static extern int MessageBoxW(IntPtr hWnd, string text, string caption, uint type);
}
