namespace Conqueror.Resources;

public sealed record IndexedPalette(byte[] Rgb)
{
    public const int ColorCount = 256;
    public const int ByteSize = ColorCount * 3;
}

public static class IndexedPaletteDecoder
{
    public static IndexedPalette Decode(ReadOnlySpan<byte> source)
    {
        if (source.Length != IndexedPalette.ByteSize)
            throw new InvalidDataException("An indexed RGB palette must contain exactly 256 RGB triples.");
        return new IndexedPalette(source.ToArray());
    }
}
