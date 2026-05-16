namespace RSValve.Desktop.Models;

public sealed class LibraryEntryRow
{
    public int SerialNumber { get; init; }
    public required LibraryEntry Entry { get; init; }
    public string FileSizeText { get; init; } = "";

    public static LibraryEntryRow FromEntry(int serialNumber, LibraryEntry entry, string? videoPath)
    {
        var sizeText = "";
        if (!string.IsNullOrEmpty(videoPath) && File.Exists(videoPath))
            sizeText = FormatFileSize(new FileInfo(videoPath).Length);

        return new LibraryEntryRow
        {
            SerialNumber = serialNumber,
            Entry = entry,
            FileSizeText = sizeText
        };
    }

    private static string FormatFileSize(long bytes)
    {
        string[] units = ["B", "KB", "MB", "GB"];
        double size = bytes;
        var unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }

        return unit == 0 ? $"{bytes} {units[unit]}" : $"{size:0.#} {units[unit]}";
    }
}
