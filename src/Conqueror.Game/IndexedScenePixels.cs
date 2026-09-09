using Conqueror.Resources;

namespace Conqueror.Game;

public static class IndexedScenePixels
{
    public static byte[] ToRgba(
        ReadOnlySpan<byte> indices, ReadOnlySpan<byte> palette, bool transparentZero = true,
        ReadOnlySpan<byte> colorMap = default)
    {
        if (palette.Length != IndexedPalette.ByteSize)
            throw new InvalidDataException("Scene palette is incomplete.");
        if (!colorMap.IsEmpty && colorMap.Length != DynamixSceneColorMaps.EntryCount)
            throw new InvalidDataException("Scene color map is incomplete.");
        var rgba = new byte[checked(indices.Length * 4)];
        for (var pixel = 0; pixel < indices.Length; pixel++)
        {
            var sourceIndex = indices[pixel];
            var mapped = colorMap.IsEmpty ? sourceIndex : colorMap[sourceIndex];
            var paletteOffset = mapped * 3;
            var target = pixel * 4;
            rgba[target] = palette[paletteOffset];
            rgba[target + 1] = palette[paletteOffset + 1];
            rgba[target + 2] = palette[paletteOffset + 2];
            rgba[target + 3] = transparentZero && sourceIndex == 0 ? (byte)0 : (byte)255;
        }
        return rgba;
    }
}
