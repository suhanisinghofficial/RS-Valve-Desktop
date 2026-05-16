using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;

namespace RSValve.Desktop.Services;

internal static class DialogHelper
{
    private static readonly IBrush PageBg = new SolidColorBrush(Color.Parse("#eef2f7"));
    private static readonly IBrush CardBg = new SolidColorBrush(Color.Parse("#ffffff"));
    private static readonly IBrush Ink = new SolidColorBrush(Color.Parse("#0f172a"));
    private static readonly IBrush InkMuted = new SolidColorBrush(Color.Parse("#64748b"));
    private static readonly IBrush HeaderBg = new SolidColorBrush(Color.Parse("#1e40af"));
    private static readonly IBrush InputBorder = new SolidColorBrush(Color.Parse("#cbd5e1"));

    public static async Task ShowInfoAsync(Window owner, string title, string message)
    {
        var dlg = CreateDialogShell(title, 400, null);
        var ok = CreateButton("OK", accent: true);
        ok.Click += (_, _) => dlg.Close();

        dlg.Content = BuildCard(
            title,
            null,
            new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    CreateBodyText(message),
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Children = { ok }
                    }
                }
            });

        await dlg.ShowDialog(owner);
    }

    public static async Task<bool> ConfirmAsync(Window owner, string title, string message)
    {
        bool? choice = null;
        var dlg = CreateDialogShell(title, 420, null);

        var yes = CreateButton("Yes", accent: true);
        var no = CreateButton("No", accent: false);
        yes.Margin = new Thickness(0, 0, 8, 0);
        yes.Click += (_, _) => { choice = true; dlg.Close(); };
        no.Click += (_, _) => { choice = false; dlg.Close(); };

        dlg.Content = BuildCard(
            title,
            null,
            new StackPanel
            {
                Spacing = 16,
                Children =
                {
                    CreateBodyText(message),
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Children = { yes, no }
                    }
                }
            });

        await dlg.ShowDialog(owner);
        return choice == true;
    }

    public static async Task<string?> PromptTextAsync(
        Window owner,
        string title,
        string label,
        string initialValue,
        string confirmText = "Save")
    {
        string? result = null;
        var dlg = CreateDialogShell(title, 440, null);

        var input = CreateInput(initialValue, label);
        var save = CreateButton(confirmText, accent: true);
        var cancel = CreateButton("Cancel", accent: false);
        save.Margin = new Thickness(0, 0, 8, 0);
        save.Click += (_, _) => { result = input.Text; dlg.Close(); };
        cancel.Click += (_, _) => dlg.Close();

        dlg.Content = BuildCard(
            title,
            "Names are shown in uppercase without special characters.",
            new StackPanel
            {
                Spacing = 14,
                Children =
                {
                    new TextBlock
                    {
                        Text = label,
                        FontSize = 12,
                        FontWeight = FontWeight.SemiBold,
                        Foreground = InkMuted
                    },
                    input,
                    new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 4, 0, 0),
                        Children = { save, cancel }
                    }
                }
            });

        await dlg.ShowDialog(owner);
        return result;
    }

    private static TextBox CreateInput(string initialValue, string watermark) =>
        new()
        {
            Text = initialValue,
            Watermark = watermark,
            FontSize = 14,
            Foreground = Ink,
            Background = CardBg,
            CaretBrush = Ink,
            BorderBrush = InputBorder,
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 10),
            MinHeight = 44
        };

    private static TextBlock CreateBodyText(string message) =>
        new()
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 13,
            Foreground = Ink,
            LineHeight = 20
        };

    private static Button CreateButton(string content, bool accent)
    {
        var btn = new Button
        {
            Content = content,
            MinWidth = 88,
            MinHeight = 36,
            FontSize = 13,
            FontWeight = FontWeight.SemiBold,
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(16, 8)
        };

        if (accent)
        {
            btn.Background = new SolidColorBrush(Color.Parse("#2563eb"));
            btn.Foreground = Brushes.White;
            btn.BorderThickness = new Thickness(0);
        }
        else
        {
            btn.Background = CardBg;
            btn.Foreground = Ink;
            btn.BorderBrush = InputBorder;
            btn.BorderThickness = new Thickness(1);
        }

        return btn;
    }

    private static Border BuildCard(string headerTitle, string? subtitle, Control body)
    {
        var headerContent = new StackPanel { Spacing = 2 };
        headerContent.Children.Add(new TextBlock
        {
            Text = headerTitle,
            FontSize = 16,
            FontWeight = FontWeight.Bold,
            Foreground = Brushes.White
        });

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            headerContent.Children.Add(new TextBlock
            {
                Text = subtitle,
                FontSize = 11,
                Foreground = new SolidColorBrush(Color.Parse("#bfdbfe")),
                TextWrapping = TextWrapping.Wrap
            });
        }

        return new Border
        {
            Background = PageBg,
            Padding = new Thickness(12),
            Child = new Border
            {
                Background = CardBg,
                CornerRadius = new CornerRadius(12),
                ClipToBounds = true,
                BoxShadow = BoxShadows.Parse("0 8 24 0 #22000000"),
                Child = CreateCardBody(headerContent, body)
            }
        };
    }

    private static DockPanel CreateCardBody(Control header, Control body)
    {
        var headerBorder = new Border
        {
            Background = HeaderBg,
            Padding = new Thickness(16, 14),
            Child = header
        };
        DockPanel.SetDock(headerBorder, Dock.Top);

        var bodyBorder = new Border
        {
            Padding = new Thickness(16, 14),
            Child = body
        };

        return new DockPanel
        {
            LastChildFill = true,
            Children = { headerBorder, bodyBorder }
        };
    }

    private static Window CreateDialogShell(string title, double width, double? height)
    {
        var dlg = new Window
        {
            Title = title,
            Width = width,
            CanResize = false,
            ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = PageBg,
            RequestedThemeVariant = ThemeVariant.Light,
            SizeToContent = height == null ? SizeToContent.Height : SizeToContent.Manual
        };

        if (height != null)
            dlg.Height = height.Value;

        return dlg;
    }
}
