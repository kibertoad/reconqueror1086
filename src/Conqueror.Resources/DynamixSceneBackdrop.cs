using System.Buffers.Binary;

namespace Conqueror.Resources;

public sealed record DynamixSceneBackdrop(int Width, int Height, int Horizon, int Mode, byte[] Indices);

public static class DynamixSceneBackdropDecoder
{
    public const int DescriptorSize = 24;

    public static DynamixSceneBackdrop Decode(ReadOnlySpan<byte> descriptor, ReadOnlySpan<byte> image)
    {
        if (descriptor.Length != DescriptorSize)
            throw new InvalidDataException($"Scene Backdrop must be exactly {DescriptorSize} bytes.");
        if (ReadInt32(descriptor, 0) != 1)
            throw new InvalidDataException("Scene Backdrop kind is unsupported.");
        var width = ReadInt32(descriptor, 8);
        var height = ReadInt32(descriptor, 12);
        var horizon = ReadInt32(descriptor, 16);
        var mode = ReadInt32(descriptor, 20);
        if (width is <= 0 or > DynamixSceneTextureDecoder.MaximumDimension ||
            height is <= 0 or > DynamixSceneTextureDecoder.MaximumDimension ||
            horizon < 0 || horizon >= height)
            throw new InvalidDataException("Scene Backdrop geometry is outside bounded limits.");
        if ((long)width * height != image.Length)
            throw new InvalidDataException("Scene BackImage length does not match its Backdrop descriptor.");
        return new DynamixSceneBackdrop(width, height, horizon, mode, image.ToArray());
    }

    private static int ReadInt32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(offset, sizeof(int)));
}
