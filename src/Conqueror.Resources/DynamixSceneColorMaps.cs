namespace Conqueror.Resources;

public sealed record DynamixSceneColorMap(int Index, byte[] Indices);

public sealed class DynamixSceneColorMaps
{
    public const int Count = 128;
    public const int EntryCount = 256;
    private readonly DynamixSceneColorMap[] _maps;

    public DynamixSceneColorMaps(IEnumerable<DynamixSceneColorMap> maps)
    {
        ArgumentNullException.ThrowIfNull(maps);
        _maps = maps.OrderBy(map => map.Index).ToArray();
        if (_maps.Length != Count || _maps.Select(map => map.Index).Distinct().Count() != Count ||
            _maps.Where((map, index) => map.Index != index).Any())
            throw new InvalidDataException($"Scene color maps must contain exactly Pal0 through Pal{Count - 1}.");
        if (_maps.Any(map => map.Indices.Length != EntryCount))
            throw new InvalidDataException($"Every scene color map must contain exactly {EntryCount} indices.");
    }

    public ReadOnlyMemory<byte> this[int index] => index is >= 0 and < Count
        ? _maps[index].Indices
        : throw new ArgumentOutOfRangeException(nameof(index));
}

public static class DynamixSceneColorMapDecoder
{
    public static DynamixSceneColorMap Decode(string resourceName, ReadOnlySpan<byte> payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        if (!resourceName.StartsWith("Pal", StringComparison.Ordinal) ||
            !int.TryParse(resourceName.AsSpan(3), out var index) ||
            index is < 0 or >= DynamixSceneColorMaps.Count || resourceName != $"Pal{index}")
            throw new InvalidDataException("Scene color-map name must be canonical Pal0 through Pal127.");
        if (payload.Length != DynamixSceneColorMaps.EntryCount)
            throw new InvalidDataException($"Scene color map must contain exactly {DynamixSceneColorMaps.EntryCount} indices.");
        return new DynamixSceneColorMap(index, payload.ToArray());
    }
}

public static class DynamixSceneColorMapGenerator
{
    // CONQUER.EXE 0x47914-0x47A84 blends RGB channels, rounds half-up,
    // and resolves the result through the lowest-index Manhattan match.
    public static DynamixSceneColorMaps RegenerateFirstFamily(
        DynamixSceneColorMaps stored, ReadOnlySpan<byte> palette, DynamixSceneColorMapping parameters)
    {
        ArgumentNullException.ThrowIfNull(stored);
        ArgumentNullException.ThrowIfNull(parameters);
        if (palette.Length != IndexedPalette.ByteSize)
            throw new InvalidDataException("Scene color-map generation requires a complete indexed RGB palette.");
        if (!parameters.Enabled) return stored;
        if (parameters.MapCount is < 1 or > DynamixSceneColorMaps.Count ||
            parameters.BlendTarget is < 0 or >= IndexedPalette.ColorCount)
            throw new ArgumentOutOfRangeException(nameof(parameters));

        var maps = new DynamixSceneColorMap[DynamixSceneColorMaps.Count];
        for (var mapIndex = 0; mapIndex < maps.Length; mapIndex++)
        {
            if (mapIndex >= parameters.MapCount)
            {
                maps[mapIndex] = new(mapIndex, stored[mapIndex].ToArray());
                continue;
            }

            var indices = new byte[DynamixSceneColorMaps.EntryCount];
            var sourceWeight = (double)(parameters.MapCount - mapIndex) / parameters.MapCount;
            var targetWeight = 1d - sourceWeight;
            for (var sourceIndex = 0; sourceIndex < indices.Length; sourceIndex++)
            {
                var red = Blend(palette, sourceIndex, parameters.BlendTarget, 0, sourceWeight, targetWeight);
                var green = Blend(palette, sourceIndex, parameters.BlendTarget, 1, sourceWeight, targetWeight);
                var blue = Blend(palette, sourceIndex, parameters.BlendTarget, 2, sourceWeight, targetWeight);
                indices[sourceIndex] = Nearest(palette, red, green, blue);
            }
            maps[mapIndex] = new(mapIndex, indices);
        }
        return new DynamixSceneColorMaps(maps);
    }

    private static int Blend(
        ReadOnlySpan<byte> palette, int source, int target, int channel,
        double sourceWeight, double targetWeight) =>
        (int)Math.Truncate(palette[source * 3 + channel] * sourceWeight
            + palette[target * 3 + channel] * targetWeight + 0.5d);

    private static byte Nearest(ReadOnlySpan<byte> palette, int red, int green, int blue)
    {
        // The original initializes the maximum possible RGB distance and only
        // replaces it on a strict improvement, preserving the first tied index.
        var bestDistance = 0x2fd;
        var bestIndex = 1;
        for (var candidate = 0; candidate < IndexedPalette.ColorCount; candidate++)
        {
            var offset = candidate * 3;
            var distance = Math.Abs(red - palette[offset])
                + Math.Abs(green - palette[offset + 1])
                + Math.Abs(blue - palette[offset + 2]);
            if (distance >= bestDistance) continue;
            bestDistance = distance;
            bestIndex = candidate;
        }
        return (byte)bestIndex;
    }
}
