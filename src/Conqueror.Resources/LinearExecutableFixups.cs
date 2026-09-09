using System.Buffers.Binary;

namespace Conqueror.Resources;

public sealed record LinearExecutableFixup(
    int SourceObject,
    uint SourceAddress,
    byte SourceType,
    int TargetObject,
    uint TargetOffset,
    bool Additive,
    bool Chained);

public static class LinearExecutableFixupReader
{
    private const int HeaderSize = 0x84;
    private const int ObjectDescriptorSize = 24;
    private const byte SourceList = 0x20;
    private const byte TargetTypeMask = 0x03;
    private const byte Additive = 0x04;
    private const byte Chained = 0x08;
    private const byte TargetOffset32 = 0x10;
    private const byte Additive32 = 0x20;
    private const byte Ordinal16 = 0x40;
    private const byte ImportOrdinal8 = 0x80;

    private sealed record ObjectInfo(int Number, uint VirtualSize, uint BaseAddress, uint PageIndex, uint PageCount);

    public static IReadOnlyList<LinearExecutableFixup> ReadInternalFixups(ReadOnlySpan<byte> source)
    {
        var header = FindHeader(source);
        var pageCount = U32(source, header + 0x14);
        var pageSize = U32(source, header + 0x28);
        var objectTableOffset = U32(source, header + 0x40);
        var objectCount = U32(source, header + 0x44);
        var fixupPageOffset = U32(source, header + 0x68);
        var fixupRecordOffset = U32(source, header + 0x6c);
        if (pageCount is 0 or > 1_000_000 || pageSize is < 512 or > 65536 || objectCount is 0 or > 65535)
            throw new InvalidDataException("Linear executable header limits are invalid.");

        var objects = new List<ObjectInfo>(checked((int)objectCount));
        for (var index = 0; index < objectCount; index++)
        {
            var descriptor = CheckedOffset(header, objectTableOffset, checked((int)index * ObjectDescriptorSize), source.Length, ObjectDescriptorSize);
            objects.Add(new ObjectInfo(checked((int)index + 1), U32(source, descriptor), U32(source, descriptor + 4),
                U32(source, descriptor + 12), U32(source, descriptor + 16)));
        }

        var pageTable = CheckedOffset(header, fixupPageOffset, 0, source.Length, checked(((int)pageCount + 1) * 4));
        var recordTable = CheckedOffset(header, fixupRecordOffset, 0, source.Length, 0);
        var recordBytes = U32(source, pageTable + checked((int)pageCount * 4));
        if (recordBytes > source.Length - recordTable)
            throw new InvalidDataException("Linear executable fixup records extend past the file.");

        var result = new List<LinearExecutableFixup>();
        for (uint logicalPage = 1; logicalPage <= pageCount; logicalPage++)
        {
            var start = U32(source, pageTable + checked((int)(logicalPage - 1) * 4));
            var end = U32(source, pageTable + checked((int)logicalPage * 4));
            if (start > end || end > recordBytes)
                throw new InvalidDataException("Linear executable fixup page range is invalid.");
            var cursor = checked(recordTable + (int)start);
            var limit = checked(recordTable + (int)end);
            while (cursor < limit)
                ReadRecord(source, ref cursor, limit, logicalPage, pageSize, objects, result);
            if (cursor != limit) throw new InvalidDataException("Linear executable fixup record crosses its page range.");
        }
        return result;
    }

    private static void ReadRecord(ReadOnlySpan<byte> source, ref int cursor, int limit, uint logicalPage,
        uint pageSize, IReadOnlyList<ObjectInfo> objects, ICollection<LinearExecutableFixup> result)
    {
        var sourceFlags = Byte(source, ref cursor, limit);
        var targetFlags = Byte(source, ref cursor, limit);
        var sourceType = (byte)(sourceFlags & 0x0f);
        if (sourceType is not (0 or 2 or 3 or 5 or 6 or 7 or 8))
            throw new InvalidDataException("Linear executable fixup source type is invalid.");
        var sourceCount = (sourceFlags & SourceList) != 0 ? Byte(source, ref cursor, limit) : 1;
        var sourceOffsets = new short[sourceCount];
        if ((sourceFlags & SourceList) == 0) sourceOffsets[0] = I16(source, ref cursor, limit);

        var targetType = (byte)(targetFlags & TargetTypeMask);
        var ordinalBytes = (targetFlags & Ordinal16) != 0 ? 2 : 1;
        var targetNumber = ReadUnsigned(source, ref cursor, limit, ordinalBytes);
        uint targetOffset = 0;
        switch (targetType)
        {
            case 0:
                if (sourceType != 2)
                    targetOffset = ReadUnsigned(source, ref cursor, limit, (targetFlags & TargetOffset32) != 0 ? 4 : 2);
                break;
            case 1:
                _ = ReadUnsigned(source, ref cursor, limit,
                    (targetFlags & ImportOrdinal8) != 0 ? 1 : (targetFlags & TargetOffset32) != 0 ? 4 : 2);
                break;
            case 2:
                _ = ReadUnsigned(source, ref cursor, limit, (targetFlags & TargetOffset32) != 0 ? 4 : 2);
                break;
            case 3:
                break;
            default:
                throw new InvalidDataException("Linear executable fixup target type is invalid.");
        }

        if ((targetFlags & Additive) != 0)
            _ = ReadUnsigned(source, ref cursor, limit, (targetFlags & Additive32) != 0 ? 4 : 2);
        if ((sourceFlags & SourceList) != 0)
            for (var index = 0; index < sourceOffsets.Length; index++) sourceOffsets[index] = I16(source, ref cursor, limit);

        if (targetType != 0) return;
        if (targetNumber is 0 || targetNumber > objects.Count)
            throw new InvalidDataException("Linear executable fixup target object is invalid.");
        if (sourceType == 2) return;
        var sourceObject = objects.SingleOrDefault(item => logicalPage >= item.PageIndex
            && logicalPage - item.PageIndex < item.PageCount)
            ?? throw new InvalidDataException("Linear executable fixup page does not belong to an object.");
        foreach (var offset in sourceOffsets)
        {
            var address = checked((long)sourceObject.BaseAddress
                + (logicalPage - sourceObject.PageIndex) * pageSize + offset);
            if (address < sourceObject.BaseAddress || address >= (long)sourceObject.BaseAddress + sourceObject.VirtualSize)
                throw new InvalidDataException("Linear executable fixup source is outside its object.");
            result.Add(new LinearExecutableFixup(sourceObject.Number, checked((uint)address), sourceType,
                checked((int)targetNumber), targetOffset, (targetFlags & Additive) != 0, (targetFlags & Chained) != 0));
        }
    }

    private static int FindHeader(ReadOnlySpan<byte> source)
    {
        if (source.Length >= 0x40 && source[0] == (byte)'M' && source[1] == (byte)'Z')
        {
            var declared = U32(source, 0x3c);
            if (declared <= int.MaxValue && IsHeader(source, checked((int)declared))) return checked((int)declared);
        }
        for (var index = 0; index <= source.Length - HeaderSize; index++)
            if (IsHeader(source, index)) return index;
        throw new InvalidDataException("Linear executable header was not found.");
    }

    private static bool IsHeader(ReadOnlySpan<byte> source, int offset) => offset >= 0
        && offset <= source.Length - HeaderSize
        && source[offset] == (byte)'L' && source[offset + 1] is (byte)'E' or (byte)'X'
        && source[offset + 2] == 0 && source[offset + 3] == 0
        && U32(source, offset + 0x14) is > 0 and <= 1_000_000
        && U32(source, offset + 0x28) is >= 512 and <= 65536
        && U32(source, offset + 0x44) is > 0 and <= 65535;

    private static int CheckedOffset(int header, uint relative, int extra, int length, int required)
    {
        var value = checked((long)header + relative + extra);
        if (value < 0 || value > length - required) throw new InvalidDataException("Linear executable table is out of bounds.");
        return checked((int)value);
    }

    private static byte Byte(ReadOnlySpan<byte> source, ref int cursor, int limit)
    {
        if (cursor >= limit) throw new InvalidDataException("Linear executable fixup record is truncated.");
        return source[cursor++];
    }

    private static short I16(ReadOnlySpan<byte> source, ref int cursor, int limit) =>
        unchecked((short)(ushort)ReadUnsigned(source, ref cursor, limit, 2));

    private static uint ReadUnsigned(ReadOnlySpan<byte> source, ref int cursor, int limit, int count)
    {
        if (count is not (1 or 2 or 4) || cursor > limit - count)
            throw new InvalidDataException("Linear executable fixup field is truncated.");
        uint value = count switch
        {
            1 => source[cursor],
            2 => BinaryPrimitives.ReadUInt16LittleEndian(source.Slice(cursor, 2)),
            4 => BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(cursor, 4)),
            _ => throw new InvalidOperationException()
        };
        cursor += count;
        return value;
    }

    private static uint U32(ReadOnlySpan<byte> source, int offset)
    {
        if (offset < 0 || offset > source.Length - 4) throw new InvalidDataException("Linear executable field is out of bounds.");
        return BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset, 4));
    }
}
