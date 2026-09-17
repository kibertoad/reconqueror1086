namespace Conqueror.Resources;

public readonly record struct StrategicWorldCellPosition(int Row, int Column);

/// <summary>
/// Reproduces the strategic world's staggered-cell coordinate helpers. The
/// original routines are <c>0x63E20</c> (cell to route-space anchor),
/// <c>0x629B0</c> (bring a route-space point into the current viewport), and
/// <c>0x6E5A0</c> (scan the rendered cell diamonds in draw order).
/// </summary>
public static class StrategicWorldProjection
{
    public const int ViewLeft = 20;
    public const int ViewTop = 7;
    public const int ViewRight = 403;
    public const int ViewBottom = 441;

    /// <summary>
    /// Returns the route-space anchor produced by original helper
    /// <c>0x63E20</c>. On odd columns this is the diamond center; on even
    /// columns it is the shared right/left vertex, whose ownership therefore
    /// follows the original viewport scan order.
    /// </summary>
    public static StrategicRoutePoint CellAnchor(int row, int column)
    {
        ValidateCell(row, column);
        return new StrategicRoutePoint(
            checked((row + 1) * StrategicWorldGridDecoder.CellWidth),
            checked((column + 1) * (StrategicWorldGridDecoder.CellHeight / 4)));
    }

    public static StrategicRoutePoint CellCenter(int row, int column)
    {
        ValidateCell(row, column);
        var halfWidth = StrategicWorldGridDecoder.CellWidth / 2;
        var centerX = checked(row * StrategicWorldGridDecoder.CellWidth +
            ((column & 1) == 0 ? halfWidth : StrategicWorldGridDecoder.CellWidth));
        var centerY = checked((column + 1) * (StrategicWorldGridDecoder.CellHeight / 4));
        return new StrategicRoutePoint(centerX, centerY);
    }

    /// <summary>
    /// Converts one original route-space point to its live grid address. Edge
    /// pixels deliberately retain camera-dependent ownership because
    /// <c>0x6E5A0</c> accepts both inclusive diamond edges and returns the
    /// first cell encountered from the current row/column.
    /// </summary>
    public static bool TryWorldToCell(
        int worldX,
        int worldY,
        int cameraRow,
        int cameraColumn,
        out StrategicWorldCellPosition position)
    {
        ValidateCell(cameraRow, cameraColumn);
        position = default;

        var row = cameraRow;
        var column = cameraColumn;
        int originX;
        int originY;
        // The original helper assumes coordinates generated inside the
        // strategic plane. A bounded loop turns malformed external points
        // into a clean miss instead of reproducing its non-progressing loop.
        for (var adjustment = 0; ; adjustment++)
        {
            if (adjustment > StrategicWorldGridDecoder.RowCount +
                StrategicWorldGridDecoder.ColumnCount) return false;

            originX = checked(row * StrategicWorldGridDecoder.CellWidth +
                StrategicWorldGridDecoder.CellWidth / 2);
            originY = checked(column * (StrategicWorldGridDecoder.CellHeight / 4) +
                StrategicWorldGridDecoder.CellHeight / 4);
            var maximumX = checked(originX + ViewRight - ViewLeft);
            var maximumY = checked(originY + ViewBottom - ViewTop);
            if (worldX < originX)
            {
                var previous = row;
                row -= (originX - worldX) / StrategicWorldGridDecoder.CellWidth;
                if (row > 0) row--;
                if (row == previous) return false;
                continue;
            }
            if (worldX > maximumX)
            {
                var previous = row;
                row += (worldX - originX) / StrategicWorldGridDecoder.CellWidth;
                if (row > 0) row--;
                if (row == previous) return false;
                continue;
            }
            if (worldY < originY)
            {
                var previous = column;
                column -= 2 * ((originY - worldY) /
                    (StrategicWorldGridDecoder.CellHeight / 2));
                if (column > 0) column--;
                if (column == previous) return false;
                continue;
            }
            if (worldY > maximumY)
            {
                var previous = column;
                column += 2 * ((worldY - originY) /
                    (StrategicWorldGridDecoder.CellHeight / 2));
                if (column > 0) column--;
                if (column == previous) return false;
                continue;
            }
            break;
        }

        var screenX = checked(worldX - originX + ViewLeft);
        var screenY = checked(worldY - originY + ViewTop);
        return TryPickVisibleCell(screenX, screenY, row, column, out position);
    }

    private static bool TryPickVisibleCell(
        int x,
        int y,
        int cameraRow,
        int cameraColumn,
        out StrategicWorldCellPosition position)
    {
        position = default;
        if (x < ViewLeft || x > ViewRight || y < ViewTop || y > ViewBottom)
            return false;

        var halfWidth = StrategicWorldGridDecoder.CellWidth / 2;
        var quarterHeight = StrategicWorldGridDecoder.CellHeight / 4;
        var halfHeight = StrategicWorldGridDecoder.CellHeight / 2;
        var lineY = ViewTop - halfHeight - quarterHeight;
        var initialColumnParity = cameraColumn & 1;
        var column = cameraColumn;
        var startX = initialColumnParity == 0 ? ViewLeft - halfWidth : ViewLeft;
        while (lineY < ViewBottom - halfHeight)
        {
            if (TryPickLine(x, y, lineY, startX, cameraRow, column, out position))
                return true;

            column = IncrementWrapped(column, StrategicWorldGridDecoder.ColumnCount);
            lineY += quarterHeight;
            startX = initialColumnParity == 0 ? ViewLeft : ViewLeft - halfWidth;
            if (TryPickLine(x, y, lineY, startX, cameraRow, column, out position))
                return true;

            column = IncrementWrapped(column, StrategicWorldGridDecoder.ColumnCount);
            lineY += quarterHeight;
            startX = initialColumnParity == 0 ? ViewLeft - halfWidth : ViewLeft;
        }
        return false;
    }

    private static bool TryPickLine(
        int x,
        int y,
        int lineY,
        int startX,
        int cameraRow,
        int column,
        out StrategicWorldCellPosition position)
    {
        position = default;
        var row = cameraRow;
        while (startX < ViewRight)
        {
            var centerX = startX + StrategicWorldGridDecoder.CellWidth / 2;
            var centerY = lineY + 3 * StrategicWorldGridDecoder.CellHeight / 4;
            var metric = Math.Abs(x - centerX) + 2 * Math.Abs(y - centerY);
            if (metric <= StrategicWorldGridDecoder.CellWidth / 2)
            {
                position = new StrategicWorldCellPosition(row, column);
                return true;
            }
            row = IncrementWrapped(row, StrategicWorldGridDecoder.RowCount);
            startX += StrategicWorldGridDecoder.CellWidth;
        }
        return false;
    }

    private static int IncrementWrapped(int value, int count) => value + 1 == count ? 0 : value + 1;

    private static void ValidateCell(int row, int column)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(row);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, StrategicWorldGridDecoder.RowCount);
        ArgumentOutOfRangeException.ThrowIfNegative(column);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, StrategicWorldGridDecoder.ColumnCount);
    }
}
