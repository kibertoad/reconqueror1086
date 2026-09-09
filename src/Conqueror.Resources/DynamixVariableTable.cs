using System.Buffers.Binary;

namespace Conqueror.Resources;

public sealed record DynamixVariableTable(int ElementSize, int ElementKind, IReadOnlyList<int> InitialValues);

public static class DynamixVariableTableDecoder
{
    private const int HeaderSize = 12;
    private const int MaximumVariables = 100_000;

    public static DynamixVariableTable Decode(ReadOnlySpan<byte> source)
    {
        if (source.Length < HeaderSize) throw new InvalidDataException("Variable table header is truncated.");
        var count = BinaryPrimitives.ReadInt32LittleEndian(source);
        var elementSize = BinaryPrimitives.ReadInt32LittleEndian(source[4..]);
        var elementKind = BinaryPrimitives.ReadInt32LittleEndian(source[8..]);
        if (count is < 0 or > MaximumVariables || elementSize != 4 || elementKind != 5
            || source.Length != HeaderSize + checked(count * elementSize))
            throw new InvalidDataException("Variable table shape is invalid.");
        var values = new int[count];
        for (var index = 0; index < count; index++)
            values[index] = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(HeaderSize + index * 4, 4));
        return new DynamixVariableTable(elementSize, elementKind, values);
    }
}
