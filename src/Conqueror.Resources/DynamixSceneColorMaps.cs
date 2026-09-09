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
