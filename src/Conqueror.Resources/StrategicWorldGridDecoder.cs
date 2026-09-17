using System.Buffers.Binary;

namespace Conqueror.Resources;

public readonly record struct StrategicWorldCell(uint RawValue)
{
    public ushort TileId => (ushort)RawValue;
    public byte Auxiliary => (byte)(RawValue >> 16);
    public byte UpperByte => (byte)(RawValue >> 24);
}

public sealed class StrategicWorldGrid
{
    private readonly StrategicWorldCell[] _cells;

    internal StrategicWorldGrid(
        int cellWidth,
        int cellHeight,
        int rowCount,
        int columnCount,
        StrategicWorldCell[] cells)
    {
        CellWidth = cellWidth;
        CellHeight = cellHeight;
        RowCount = rowCount;
        ColumnCount = columnCount;
        _cells = cells;
    }

    public int CellWidth { get; }
    public int CellHeight { get; }
    public int RowCount { get; }
    public int ColumnCount { get; }
    public IReadOnlyList<StrategicWorldCell> Cells => _cells;

    public StrategicWorldCell this[int row, int column]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, RowCount);
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, ColumnCount);
            return _cells[checked(row * ColumnCount + column)];
        }
    }
}

/// <summary>
/// Decodes the original strategic <c>icon.jp</c>/<c>temp.jap</c> grid. The file
/// serializes columns outside rows even though the live grid is addressed row first.
/// </summary>
public static class StrategicWorldGridDecoder
{
    public const int HeaderSize = 16;
    public const int CellSize = 4;
    public const int CellWidth = 80;
    public const int CellHeight = 80;
    public const int RowCount = 200;
    public const int ColumnCount = 400;
    public const int EncodedLength = HeaderSize + RowCount * ColumnCount * CellSize;

    public static StrategicWorldGrid Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < HeaderSize)
            throw new InvalidDataException("Strategic world header is truncated.");

        var cellWidth = BinaryPrimitives.ReadInt32LittleEndian(data);
        var cellHeight = BinaryPrimitives.ReadInt32LittleEndian(data[4..]);
        var rowCount = BinaryPrimitives.ReadInt32LittleEndian(data[8..]);
        var columnCount = BinaryPrimitives.ReadInt32LittleEndian(data[12..]);
        if (cellWidth != CellWidth || cellHeight != CellHeight ||
            rowCount != RowCount || columnCount != ColumnCount)
            throw new InvalidDataException("Strategic world dimensions do not match the supported original grid.");
        if (data.Length != EncodedLength)
            throw new InvalidDataException("Strategic world length does not match its dimensions.");

        var cells = new StrategicWorldCell[checked(rowCount * columnCount)];
        var offset = HeaderSize;
        for (var column = 0; column < columnCount; column++)
        for (var row = 0; row < rowCount; row++)
        {
            cells[checked(row * columnCount + column)] = new(
                BinaryPrimitives.ReadUInt32LittleEndian(data[offset..]));
            offset += CellSize;
        }

        return new StrategicWorldGrid(cellWidth, cellHeight, rowCount, columnCount, cells);
    }
}
