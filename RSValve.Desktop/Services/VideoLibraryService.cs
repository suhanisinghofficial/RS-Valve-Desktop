using System.Text.Json;
using RSValve.Desktop.Models;

namespace RSValve.Desktop.Services;

public sealed class VideoLibraryService
{
    private readonly object _lock = new();
    private LibraryIndex _index = new();

    public void Initialize()
    {
        Directory.CreateDirectory(AppPaths.VideosDirectory);

        lock (_lock)
        {
            if (File.Exists(AppPaths.LibraryIndexFile))
            {
                try
                {
                    var json = File.ReadAllText(AppPaths.LibraryIndexFile);
                    _index = JsonSerializer.Deserialize<LibraryIndex>(json) ?? new LibraryIndex();
                }
                catch
                {
                    _index = new LibraryIndex();
                }
            }

            PruneMissingFiles();
            SaveIndexUnlocked();
        }
    }

    public IReadOnlyList<LibraryEntry> GetAll()
    {
        lock (_lock)
        {
            return _index.Entries
                .OrderBy(e => e.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(e => new LibraryEntry
                {
                    Id = e.Id,
                    DisplayName = e.DisplayName,
                    StoredFileName = e.StoredFileName,
                    AddedUtc = e.AddedUtc
                })
                .ToList();
        }
    }

    public string? TryGetStoredPath(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        lock (_lock)
        {
            var entry = _index.Entries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return null;
            return Path.GetFullPath(Path.Combine(AppPaths.VideosDirectory, entry.StoredFileName));
        }
    }

    public string? TryAddCopy(string sourcePath)
    {
        if (!File.Exists(sourcePath) || !Mp4Validator.IsValidMp4Path(sourcePath))
            return null;

        var id = Guid.NewGuid().ToString("N");
        var storedName = id + ".mp4";
        var dest = Path.Combine(AppPaths.VideosDirectory, storedName);

        lock (_lock)
        {
            try
            {
                File.Copy(sourcePath, dest, overwrite: false);
                var display = VideoNameGenerator.FromFileName(
                    sourcePath,
                    _index.Entries.Select(e => e.DisplayName));
                _index.Entries.Add(new LibraryEntry
                {
                    Id = id,
                    DisplayName = display,
                    StoredFileName = storedName,
                    AddedUtc = DateTime.UtcNow
                });
                SaveIndexUnlocked();
            }
            catch
            {
                TryDeleteFile(dest);
                return null;
            }
        }

        return id;
    }

    public bool TryUpdateDisplayName(string id, string displayName)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        displayName = VideoNameGenerator.SanitizeDisplayName(displayName);
        if (string.IsNullOrEmpty(displayName)) return false;

        lock (_lock)
        {
            var entry = _index.Entries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return false;

            var others = _index.Entries.Where(e => e.Id != id).Select(e => e.DisplayName);
            entry.DisplayName = VideoNameGenerator.EnsureUniqueForRename(displayName, others);
            SaveIndexUnlocked();
            return true;
        }
    }

    public bool TryRemove(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;
        lock (_lock)
        {
            var entry = _index.Entries.FirstOrDefault(e => e.Id == id);
            if (entry == null) return false;

            var full = Path.Combine(AppPaths.VideosDirectory, entry.StoredFileName);
            _index.Entries.Remove(entry);
            SaveIndexUnlocked();
            TryDeleteFile(full);
            return true;
        }
    }

    private void PruneMissingFiles()
    {
        _index.Entries.RemoveAll(e =>
            !File.Exists(Path.Combine(AppPaths.VideosDirectory, e.StoredFileName)));
    }

    private static void TryDeleteFile(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch
        {
        }
    }

    private void SaveIndexUnlocked()
    {
        var json = JsonSerializer.Serialize(_index, AppJson.Indented);
        File.WriteAllText(AppPaths.LibraryIndexFile, json);
    }
}
