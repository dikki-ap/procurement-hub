namespace ProcureHub.API.Security;

/// <summary>
/// Validates uploaded files by inspecting their magic bytes, preventing extension/MIME spoofing.
/// </summary>
public static class FileSignatureValidator
{
    private static readonly Dictionary<string, byte[][]> Signatures = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"]  = [[0x25, 0x50, 0x44, 0x46]],                                        // %PDF
        [".jpg"]  = [[0xFF, 0xD8, 0xFF]],                                               // JFIF / EXIF SOI
        [".jpeg"] = [[0xFF, 0xD8, 0xFF]],
        [".png"]  = [[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]],               // PNG
        [".xlsx"] = [[0x50, 0x4B, 0x03, 0x04]],                                        // ZIP (OOXML)
        [".xls"]  = [[0xD0, 0xCF, 0x11, 0xE0]],                                        // CFB / OLE2
    };

    /// <summary>
    /// Reads the first bytes of <paramref name="stream"/>, checks them against known signatures,
    /// then resets the stream position to 0 for subsequent reads.
    /// </summary>
    /// <returns>True when the bytes match at least one known signature for the extension.</returns>
    public static async Task<bool> IsValidAsync(Stream stream, string extension)
    {
        if (!Signatures.TryGetValue(extension, out var sigs))
            return false;

        int maxLen = sigs.Max(s => s.Length);
        byte[] header = new byte[maxLen];
        int bytesRead = await stream.ReadAsync(header.AsMemory(0, maxLen));

        if (stream.CanSeek)
            stream.Position = 0;

        return sigs.Any(sig =>
            bytesRead >= sig.Length &&
            header.AsSpan(0, sig.Length).SequenceEqual(sig));
    }
}
