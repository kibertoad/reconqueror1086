using System.Buffers.Binary;
using System.Text;

namespace Conqueror.Resources;

public sealed record DynamixSceneViewer(int X, int Y, int Elevation, int Heading)
{
    public int CellX => X >> 8;
    public int CellY => Y >> 8;
}

public sealed record DynamixSceneColorMapping(bool Enabled, int MapCount, int DistanceShift, int BlendTarget);

public enum DynamixSceneFace
{
    // CONQUER.EXE 0x44CC0-0x44F61 emits these hit masks; the selector at
    // 0x46739-0x467F7 maps them to block offsets 44, 48, 52, and 56.
    North = 0x100,
    East = 0x200,
    South = 0x400,
    West = 0x800
}

public sealed record DynamixSceneBlock(
    int Index,
    int Kind,
    int Behavior,
    int Flags,
    int ColorMapOffset,
    int Width,
    int Height,
    int Surface0,
    int Surface1,
    int Surface2,
    int Surface3,
    int EffectDefinitionIndex,
    int StateTarget,
    int ActorTemplate,
    int ActorCombatRow,
    string Name)
{
    public (int TextureIndex, bool FlipHorizontally) TextureForBillboardHeading(int relativeHeading)
    {
        if (Kind != 4 || Surface0 < 0 || Surface2 is <= 0 or > 256)
            return (Surface0, false);

        var divisions = Surface2;
        var heading = relativeHeading & 0xff;
        var sector = ((((heading + 0x80 / divisions) & 0xff) * divisions) >> 8);
        var flip = (Behavior & 4) != 0 && sector > divisions / 2;
        if (flip) sector = divisions - sector;
        return (Surface0 + sector, flip);
    }

    public int TextureForFace(DynamixSceneFace face) => face switch
    {
        DynamixSceneFace.North => Surface0,
        DynamixSceneFace.East => Surface1,
        DynamixSceneFace.South => Surface2,
        _ => Surface3
    };

    public IEnumerable<int> TextureReferences()
    {
        // Kind 4 is a camera-facing sprite: offset 44 is its image while the
        // following fields carry orientation/state data rather than wall faces.
        if (Kind == 4)
        {
            yield return Surface0;
            yield break;
        }

        foreach (var texture in new[] { Surface0, Surface1, Surface2, Surface3 })
            yield return texture;
    }
}

public sealed class DynamixScene
{
    public const int MapWidth = 128;
    public const int MapHeight = 128;
    private readonly ushort[] _cells;

    internal DynamixScene(
        DynamixSceneViewer viewer,
        DynamixSceneColorMapping colorMapping,
        int textureCount,
        int effectDefinitionCount,
        DynamixSceneBlock[] blocks,
        ushort[] cells,
        IReadOnlyList<DynamixSceneEffectDefinition> effectDefinitions)
    {
        Viewer = viewer;
        ColorMapping = colorMapping;
        TextureCount = textureCount;
        EffectDefinitionCount = effectDefinitionCount;
        Blocks = blocks;
        EffectDefinitions = effectDefinitions;
        _cells = cells;
    }

    public DynamixSceneViewer Viewer { get; }
    public DynamixSceneColorMapping ColorMapping { get; }
    public int TextureCount { get; }
    public int EffectDefinitionCount { get; }
    public IReadOnlyList<DynamixSceneEffectDefinition> EffectDefinitions { get; }
    public IReadOnlyList<DynamixSceneBlock> Blocks { get; }

    // The original resource stores complete Y columns consecutively.
    public ushort BlockIndexAt(int x, int y)
    {
        if (x is < 0 or >= MapWidth || y is < 0 or >= MapHeight)
            throw new ArgumentOutOfRangeException(x is < 0 or >= MapWidth ? nameof(x) : nameof(y));
        return _cells[x * MapHeight + y];
    }

    public DynamixSceneBlock BlockAt(int x, int y) => Blocks[BlockIndexAt(x, y)];
}

public static class DynamixSceneDecoder
{
    public const int ViewerSize = 108;
    public const int ScenarioSize = 568;
    public const int MapSize = DynamixScene.MapWidth * DynamixScene.MapHeight * sizeof(ushort);
    public const int BlockSize = 96;
    private const int BlockNameOffset = 78;
    private const int BlockNameSize = 16;

    public static DynamixScene Decode(
        ReadOnlySpan<byte> viewer,
        ReadOnlySpan<byte> scenario,
        ReadOnlySpan<byte> map,
        ReadOnlySpan<byte> blocks) => Decode(viewer, scenario, map, blocks, default, requireEffectDefinitions: false);

    public static DynamixScene Decode(
        ReadOnlySpan<byte> viewer,
        ReadOnlySpan<byte> scenario,
        ReadOnlySpan<byte> map,
        ReadOnlySpan<byte> blocks,
        ReadOnlySpan<byte> effectDefinitions) =>
        Decode(viewer, scenario, map, blocks, effectDefinitions, requireEffectDefinitions: true);

    private static DynamixScene Decode(
        ReadOnlySpan<byte> viewer,
        ReadOnlySpan<byte> scenario,
        ReadOnlySpan<byte> map,
        ReadOnlySpan<byte> blocks,
        ReadOnlySpan<byte> effectDefinitions,
        bool requireEffectDefinitions)
    {
        if (viewer.Length != ViewerSize) throw new InvalidDataException($"Scene Viewer must be exactly {ViewerSize} bytes.");
        if (scenario.Length != ScenarioSize) throw new InvalidDataException($"Scene Scenario must be exactly {ScenarioSize} bytes.");
        if (map.Length != MapSize) throw new InvalidDataException($"Scene Map must be exactly {MapSize} bytes.");

        var textureCount = ReadInt32(scenario, 20);
        var blockCount = ReadInt32(scenario, 24);
        var effectDefinitionCount = ReadInt32(scenario, 28);
        var colorMappingEnabled = ReadInt32(scenario, 40);
        if (colorMappingEnabled is not (0 or 1))
            throw new InvalidDataException("Scene color-map enable flag is invalid.");
        var colorMapping = new DynamixSceneColorMapping(
            colorMappingEnabled != 0,
            ReadInt32(scenario, 44),
            ReadInt32(scenario, 48),
            ReadInt32(scenario, 52));
        if (textureCount is < 1 or > 4096) throw new InvalidDataException("Scene texture count is outside bounded limits.");
        if (blockCount is < 1 or > 4096) throw new InvalidDataException("Scene block count is outside bounded limits.");
        if (effectDefinitionCount is < 0 or > 4096) throw new InvalidDataException("Scene SFXDEFS count is outside bounded limits.");
        if (colorMapping.Enabled && (colorMapping.MapCount is < 1 or > DynamixSceneColorMaps.Count
            || colorMapping.DistanceShift is < 2 or > 30
            || colorMapping.BlendTarget is < 0 or >= DynamixSceneColorMaps.EntryCount))
            throw new InvalidDataException("Scene color-map parameters are outside bounded limits.");
        if (blocks.Length != checked(blockCount * BlockSize))
            throw new InvalidDataException("Scene Blocks length does not match the Scenario block count.");

        var decodedBlocks = new DynamixSceneBlock[blockCount];
        for (var index = 0; index < decodedBlocks.Length; index++)
        {
            var record = blocks.Slice(index * BlockSize, BlockSize);
            if (record[^2] != 0xcc || record[^1] != 0xcc)
                throw new InvalidDataException($"Scene block {index} is missing its record sentinel.");
            var decoded = new DynamixSceneBlock(
                index,
                ReadInt32(record, 0),
                ReadInt32(record, 4),
                ReadInt32(record, 8),
                ReadInt32(record, 12),
                ReadInt32(record, 24),
                ReadInt32(record, 32),
                BinaryPrimitives.ReadInt16LittleEndian(record.Slice(44, sizeof(short))),
                ReadInt32(record, 48),
                ReadInt32(record, 52),
                ReadInt32(record, 56),
                BinaryPrimitives.ReadInt16LittleEndian(record.Slice(46, sizeof(short))),
                ReadInt32(record, 64),
                BinaryPrimitives.ReadInt16LittleEndian(record.Slice(0x4a, sizeof(short))),
                BinaryPrimitives.ReadInt16LittleEndian(record.Slice(0x4c, sizeof(short))),
                DecodeName(record.Slice(BlockNameOffset, BlockNameSize), index));
            if (decoded.TextureReferences()
                .Any(surface => surface < -1 || surface >= textureCount))
                throw new InvalidDataException($"Scene block {index} references a texture outside the Scenario table.");
            if (requireEffectDefinitions &&
                (decoded.EffectDefinitionIndex < -1 || decoded.EffectDefinitionIndex >= effectDefinitionCount))
                throw new InvalidDataException($"Scene block {index} references an effect outside SFXDEFS.");
            decodedBlocks[index] = decoded;
        }

        var cells = new ushort[DynamixScene.MapWidth * DynamixScene.MapHeight];
        for (var index = 0; index < cells.Length; index++)
        {
            cells[index] = BinaryPrimitives.ReadUInt16LittleEndian(map.Slice(index * sizeof(ushort), sizeof(ushort)));
            if (cells[index] >= blockCount)
                throw new InvalidDataException($"Scene map cell {index} references missing block {cells[index]}.");
        }

        var decodedViewer = new DynamixSceneViewer(
            ReadInt32(viewer, 0), ReadInt32(viewer, 4), ReadInt32(viewer, 8), ReadInt32(viewer, 12));
        if (decodedViewer.CellX is < 0 or >= DynamixScene.MapWidth ||
            decodedViewer.CellY is < 0 or >= DynamixScene.MapHeight)
            throw new InvalidDataException("Scene Viewer starts outside the map.");
        if (decodedViewer.Heading is < 0 or > ushort.MaxValue)
            throw new InvalidDataException("Scene Viewer heading is outside its 16-bit turn range.");

        var decodedEffects = requireEffectDefinitions
            ? DynamixSceneEffectDecoder.Decode(effectDefinitions, effectDefinitionCount, blockCount)
            : Array.Empty<DynamixSceneEffectDefinition>();
        return new(decodedViewer, colorMapping, textureCount, effectDefinitionCount,
            decodedBlocks, cells, decodedEffects);
    }

    private static int ReadInt32(ReadOnlySpan<byte> bytes, int offset) =>
        BinaryPrimitives.ReadInt32LittleEndian(bytes.Slice(offset, sizeof(int)));

    private static string DecodeName(ReadOnlySpan<byte> bytes, int index)
    {
        var length = bytes.IndexOf((byte)0);
        if (length < 0) length = bytes.Length;
        var value = bytes[..length];
        if (value.IndexOfAnyExceptInRange((byte)32, (byte)126) >= 0)
            throw new InvalidDataException($"Scene block {index} name is not printable ASCII.");
        return Encoding.ASCII.GetString(value).Trim();
    }
}
