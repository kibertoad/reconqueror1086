using System.Buffers.Binary;
using System.Text;

namespace Conqueror.Resources;

public sealed record DynamixSceneViewer(int X, int Y, int Elevation, int Heading)
{
    public int CellX => X >> 8;
    public int CellY => Y >> 8;
}

public sealed record DynamixSceneBlock(
    int Index,
    int Kind,
    int Behavior,
    int Flags,
    int Width,
    int Height,
    string Name);

public sealed class DynamixScene
{
    public const int MapWidth = 128;
    public const int MapHeight = 128;
    private readonly ushort[] _cells;

    internal DynamixScene(
        DynamixSceneViewer viewer,
        int textureCount,
        int soundEffectCount,
        DynamixSceneBlock[] blocks,
        ushort[] cells)
    {
        Viewer = viewer;
        TextureCount = textureCount;
        SoundEffectCount = soundEffectCount;
        Blocks = blocks;
        _cells = cells;
    }

    public DynamixSceneViewer Viewer { get; }
    public int TextureCount { get; }
    public int SoundEffectCount { get; }
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
        ReadOnlySpan<byte> blocks)
    {
        if (viewer.Length != ViewerSize) throw new InvalidDataException($"Scene Viewer must be exactly {ViewerSize} bytes.");
        if (scenario.Length != ScenarioSize) throw new InvalidDataException($"Scene Scenario must be exactly {ScenarioSize} bytes.");
        if (map.Length != MapSize) throw new InvalidDataException($"Scene Map must be exactly {MapSize} bytes.");

        var textureCount = ReadInt32(scenario, 20);
        var blockCount = ReadInt32(scenario, 24);
        var soundEffectCount = ReadInt32(scenario, 28);
        if (textureCount is < 1 or > 4096) throw new InvalidDataException("Scene texture count is outside bounded limits.");
        if (blockCount is < 1 or > 4096) throw new InvalidDataException("Scene block count is outside bounded limits.");
        if (soundEffectCount is < 0 or > 4096) throw new InvalidDataException("Scene sound-effect count is outside bounded limits.");
        if (blocks.Length != checked(blockCount * BlockSize))
            throw new InvalidDataException("Scene Blocks length does not match the Scenario block count.");

        var decodedBlocks = new DynamixSceneBlock[blockCount];
        for (var index = 0; index < decodedBlocks.Length; index++)
        {
            var record = blocks.Slice(index * BlockSize, BlockSize);
            if (record[^2] != 0xcc || record[^1] != 0xcc)
                throw new InvalidDataException($"Scene block {index} is missing its record sentinel.");
            decodedBlocks[index] = new(
                index,
                ReadInt32(record, 0),
                ReadInt32(record, 4),
                ReadInt32(record, 8),
                ReadInt32(record, 24),
                ReadInt32(record, 32),
                DecodeName(record.Slice(BlockNameOffset, BlockNameSize), index));
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

        return new(decodedViewer, textureCount, soundEffectCount, decodedBlocks, cells);
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
