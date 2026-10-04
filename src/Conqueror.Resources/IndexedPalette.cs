namespace Conqueror.Resources;

public sealed record IndexedPalette(byte[] Rgb)
{
    public const int ColorCount = 256;
    public const int ByteSize = ColorCount * 3;
}

// FMT-VIEW-009 and FMT-MEDIA-004 palette layout.
public static class IndexedPaletteDecoder
{
    public static IndexedPalette Decode(ReadOnlySpan<byte> source) =>
        new(RefurbishedDinosaurs.Core.Imaging.IndexedPaletteDecoder.Decode(source).Rgb);
}
