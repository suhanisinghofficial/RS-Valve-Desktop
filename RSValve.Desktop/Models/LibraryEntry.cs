namespace RSValve.Desktop.Models;

public sealed class LibraryEntry
{
    public required string Id { get; init; }
    public required string DisplayName { get; set; }
    public required string StoredFileName { get; init; }
    public DateTime AddedUtc { get; init; }
}

public sealed class LibraryIndex
{
    public List<LibraryEntry> Entries { get; set; } = new();
}

public sealed record VideoListItemDto(string Id, string Name, string Url);
