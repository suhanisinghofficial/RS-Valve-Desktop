namespace RSValve.Desktop.Services;

internal static class Mp4Validator
{
    public static bool IsValidMp4Path(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (!Path.GetExtension(path).Equals(".mp4", StringComparison.OrdinalIgnoreCase)) return false;
        return HasFtypSignature(path);
    }

    private static bool HasFtypSignature(string path)
    {
        try
        {
            using var fs = File.OpenRead(path);
            var buf = new byte[12];
            if (fs.Read(buf, 0, buf.Length) < 12) return false;
            return buf[4] == (byte)'f' && buf[5] == (byte)'t' && buf[6] == (byte)'y' && buf[7] == (byte)'p';
        }
        catch
        {
            return false;
        }
    }
}
