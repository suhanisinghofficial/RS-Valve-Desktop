using Avalonia.Controls;
using Avalonia.Platform;

namespace RSValve.Desktop;

internal static class WindowIconHelper
{
    private static WindowIcon? _icon;

    public static void Apply(Window window) =>
        window.Icon = _icon ??= new WindowIcon(AssetLoader.Open(new Uri("avares://RSValve/Assets/logo.ico")));
}
