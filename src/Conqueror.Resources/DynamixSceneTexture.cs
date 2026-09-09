namespace Conqueror.Resources;

public sealed record DynamixSceneTexture(int Index, int Width, int Height, byte[] Indices);

public static class DynamixSceneTextureDecoder
{
    public const int MaximumDimension = 4096;

    public static DynamixSceneTexture Decode(string resourceName, ReadOnlySpan<byte> payload)
    {
        ArgumentNullException.ThrowIfNull(resourceName);
        var fields = resourceName.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 3 || fields[0].Length != 6
            || !fields[0].StartsWith("TEX", StringComparison.OrdinalIgnoreCase)
            || !IsAsciiDigits(fields[0].AsSpan(3))
            || !IsAsciiDigits(fields[1]) || !IsAsciiDigits(fields[2])
            || !int.TryParse(fields[0].AsSpan(3), out var index)
            || !int.TryParse(fields[1], out var width) || !int.TryParse(fields[2], out var height)
            || width is <= 0 or > MaximumDimension || height is <= 0 or > MaximumDimension)
            throw new InvalidDataException("Scene texture name is invalid.");
        if ((long)width * height != payload.Length)
            throw new InvalidDataException("Scene texture dimensions are inconsistent with its payload.");
        return new DynamixSceneTexture(index, width, height, payload.ToArray());
    }

    private static bool IsAsciiDigits(ReadOnlySpan<char> value)
    {
        if (value.IsEmpty) return false;
        foreach (var character in value)
            if (character is < '0' or > '9') return false;
        return true;
    }
}
