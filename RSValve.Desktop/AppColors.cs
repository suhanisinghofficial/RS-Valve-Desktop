using Avalonia.Media;

namespace RSValve.Desktop;

internal static class AppColors
{
    public static readonly SolidColorBrush StatusStarting = Hex("#94a3b8");
    public static readonly SolidColorBrush StatusConnected = Hex("#22c55e");
    public static readonly SolidColorBrush StatusFailure = Hex("#ef4444");
    public static readonly SolidColorBrush MessageSuccess = Hex("#1b7f3b");
    public static readonly SolidColorBrush MessageError = Hex("#b91c1c");
    public static readonly SolidColorBrush MessageSuccessBg = Hex("#f0fdf4");
    public static readonly SolidColorBrush MessageErrorBg = Hex("#fef2f2");

    public static SolidColorBrush Hex(string color) => new(Color.Parse(color));
}
