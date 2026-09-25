using System.Buffers.Binary;

namespace Conqueror.Resources;

/// <summary>
/// One 0x40-byte SFXDEFS descriptor (FMT-ASSAULT-003). Names are limited to
/// fields whose use the spec confirms; the remaining values stay addressable by offset.
/// </summary>
public sealed class DynamixSceneEffectDefinition
{
    private readonly int[] _fields;

    internal DynamixSceneEffectDefinition(int index, int[] fields)
    {
        Index = index;
        _fields = fields;
    }

    public int Index { get; }
    public int FrameCount => FieldAt(0x0c);
    public int IntervalMilliseconds => FieldAt(0x14);
    public int Flags => FieldAt(0x18);
    public int MapBlockIndex => FieldAt(0x28);
    public int MapXDeltaPerTick => FieldAt(0x1c);
    public int MapYDeltaPerTick => FieldAt(0x20);
    public int BlockIndexDeltaPerTick => FieldAt(0x2c);
    public int LoopBlockIndex => FieldAt(0x30);
    public int SurfaceIndexDeltaPerTick => FieldAt(0x34);
    public int TerminalSurfaceIndex => FieldAt(0x38);
    public int HeadingDeltaPerTick => FieldAt(0x3c);
    public long NominalCompletionMilliseconds => checked((long)FrameCount * IntervalMilliseconds);

    public int FieldAt(int byteOffset)
    {
        if (byteOffset < 0 || byteOffset >= DynamixSceneEffectDecoder.RecordSize || byteOffset % sizeof(int) != 0)
            throw new ArgumentOutOfRangeException(nameof(byteOffset));
        return _fields[byteOffset / sizeof(int)];
    }
}

public static class DynamixSceneEffectDecoder
{
    public const int RecordSize = 0x40;
    private const int FieldCount = RecordSize / sizeof(int);

    public static IReadOnlyList<DynamixSceneEffectDefinition> Decode(
        ReadOnlySpan<byte> payload, int expectedCount, int blockCount)
    {
        if (expectedCount is < 0 or > 4096)
            throw new InvalidDataException("Scene SFXDEFS count is outside bounded limits.");
        if (blockCount is < 1 or > 4096)
            throw new InvalidDataException("Scene block count is outside bounded limits.");
        if (payload.Length != checked(expectedCount * RecordSize))
            throw new InvalidDataException("Scene SFXDEFS length does not match the Scenario count.");

        var definitions = new DynamixSceneEffectDefinition[expectedCount];
        for (var index = 0; index < definitions.Length; index++)
        {
            var record = payload.Slice(index * RecordSize, RecordSize);
            var fields = new int[FieldCount];
            for (var field = 0; field < fields.Length; field++)
                fields[field] = BinaryPrimitives.ReadInt32LittleEndian(record.Slice(field * sizeof(int), sizeof(int)));

            var frameCount = fields[0x0c / sizeof(int)];
            var interval = fields[0x14 / sizeof(int)];
            var mapBlock = fields[0x28 / sizeof(int)];
            if (frameCount is < 0 or > 4096)
                throw new InvalidDataException($"Scene SFXDEFS record {index} has an invalid frame count.");
            if (interval is < 0 or > 60_000)
                throw new InvalidDataException($"Scene SFXDEFS record {index} has an invalid interval.");
            if (mapBlock < -1 || mapBlock >= blockCount)
                throw new InvalidDataException($"Scene SFXDEFS record {index} references a block outside the scene table.");
            definitions[index] = new DynamixSceneEffectDefinition(index, fields);
        }
        return definitions;
    }
}
