using Conqueror.Resources;
using System.Text;

internal static class PcxPreview
{
    public static string Render(DynamixArchive archive, string resourceName, string artifactRoot)
    {
        var entry = archive.Entries.FirstOrDefault(candidate =>
            candidate.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"GOB PCX resource '{resourceName}' was not found.");
        var image = PcxDecoder.Decode(archive.ReadDecoded(entry));
        var previewRoot = Path.Combine(artifactRoot, "pcx-previews");
        Directory.CreateDirectory(previewRoot);
        var path = Path.Combine(previewRoot, $"{SafeName(resourceName)}.ppm");
        WriteIndexedPpm(path, image.Width, image.Height, image.Indices, image.PaletteRgb);
        return path;
    }

    private static string SafeName(string value) => string.Concat(value.Select(character =>
        Path.GetInvalidFileNameChars().Contains(character) || character is ':' or '/' or '\\' ? '_' : character));

    public static void WriteIndexedPpm(string path, int width, int height, ReadOnlySpan<byte> indices, ReadOnlySpan<byte> palette)
    {
        if (width <= 0 || height <= 0 || indices.Length != checked(width * height) || palette.Length != 768)
            throw new InvalidDataException("Indexed image buffers are inconsistent.");
        var header = Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
        var bytes = new byte[checked(header.Length + indices.Length * 3)];
        header.CopyTo(bytes, 0);
        var target = header.Length;
        foreach (var index in indices)
        {
            var color = index * 3;
            bytes[target++] = palette[color];
            bytes[target++] = palette[color + 1];
            bytes[target++] = palette[color + 2];
        }
        File.WriteAllBytes(path, bytes);
    }
}
