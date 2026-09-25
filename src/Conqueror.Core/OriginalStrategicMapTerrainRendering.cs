namespace Conqueror.Core;

/// <summary>
/// One terrain-tile draw selected by the original strategic surface traversal.
/// The tile identity remains owned by the resource-backed application layer.
/// </summary>
public readonly record struct OriginalStrategicMapTerrainDraw(
    int Row,
    int Column,
    int X,
    int Y);

/// <summary>
/// A terrain draw with the exact low-word tile selector consumed by the
/// original blitter. Other source cell fields remain available to callers
/// through <see cref="OriginalStrategicTerrainCell"/>.
/// </summary>
public readonly record struct OriginalStrategicMapTerrainTileDraw(
    int Row,
    int Column,
    int X,
    int Y,
    ushort TileId);

/// <summary>
/// Reproduces the viewport traversal of <c>draw_terrain</c> (RULE-STRATEGY-015).
/// It deliberately exposes draw order and placement rather than choosing a
/// host renderer, texture atlas, or scaling policy.
/// </summary>
public static class OriginalStrategicMapTerrainRendering
{
    public const int ViewportLeft = 20;
    public const int ViewportTop = 7;
    public const int ViewportRight = 403;
    public const int ViewportBottom = 441;
    public const int TileSpan = 80;
    public const int ScanlineStep = 20;

    /// <summary>
    /// Enumerates source tiles in the original scanline and wrapped-row order.
    /// Each line begins at -20 or 20 according to its wrapped grid column;
    /// the viewport's partial top and right tiles are retained for clipping by
    /// the presentation layer.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMapTerrainDraw> BuildDraws(
        OriginalStrategicCampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();

        var draws = new List<OriginalStrategicMapTerrainDraw>();
        var column = state.CameraColumn;
        var y = ViewportTop - TileSpan / 2 - TileSpan / 4;
        var lastYExclusive = ViewportBottom - TileSpan / 2;
        while (y < lastYExclusive)
        {
            var row = state.CameraRow;
            var x = (column & 1) == 0 ? ViewportLeft - TileSpan / 2 : ViewportLeft;
            while (x < ViewportRight)
            {
                draws.Add(new(row, column, x, y));
                row = (row + 1) % OriginalStrategicCampaignState.WorldRowCount;
                x += TileSpan;
            }

            column = (column + 1) % OriginalStrategicCampaignState.WorldColumnCount;
            y += ScanlineStep;
        }
        return draws;
    }

    /// <summary>
    /// Resolves each draw's frame selector through the source grid, the cell's
    /// low word (RULE-STRATEGY-015).
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMapTerrainTileDraw> BuildTileDraws(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        var positions = BuildDraws(state);
        var draws = new OriginalStrategicMapTerrainTileDraw[positions.Count];
        for (var index = 0; index < positions.Count; index++)
        {
            var position = positions[index];
            if (!resources.TryGridCell(position.Row, position.Column, out var cell)
                || cell.Row != position.Row || cell.Column != position.Column)
                throw new InvalidDataException("Strategic terrain draw requires its exact source grid cell.");
            draws[index] = new(position.Row, position.Column, position.X, position.Y, cell.TileId);
        }
        return draws;
    }
}
