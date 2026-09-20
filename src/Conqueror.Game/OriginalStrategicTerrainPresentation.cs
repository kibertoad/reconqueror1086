using Conqueror.Core;

namespace Conqueror.Game;

/// <summary>
/// Converts the executable-ordered strategic terrain pass into clipped atlas
/// blits. This is deliberately separate from the legacy estate overlay: it
/// has no authority over strategic input, markers, or simulation scheduling.
/// </summary>
public readonly record struct OriginalStrategicTerrainBlit(
    int FrameIndex,
    UiBounds Destination,
    UiBounds Source);

public static class OriginalStrategicTerrainPresentation
{
    public static string AtlasRoleFor(StrategicTerrainProfile profile) => profile switch
    {
        StrategicTerrainProfile.Spring or StrategicTerrainProfile.Summer => "Estate.Tiles.SpringSummer",
        StrategicTerrainProfile.Autumn => "Estate.Tiles.Autumn",
        StrategicTerrainProfile.Winter => "Estate.Tiles.Winter",
        _ => throw new ArgumentOutOfRangeException(nameof(profile))
    };

    /// <summary>
    /// Intersects each 80-by-80 source tile with the original logical viewport
    /// before scaling. The original blitter, rather than the traversal, owns
    /// the visible partial top, left, and right tiles.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicTerrainBlit> BuildBlits(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int frameCount)
    {
        if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));

        var tiles = OriginalStrategicMapTerrainRendering.BuildTileDraws(state, resources);
        var blits = new List<OriginalStrategicTerrainBlit>(tiles.Count);
        var viewport = new UiBounds(
            OriginalStrategicMapTerrainRendering.ViewportLeft,
            OriginalStrategicMapTerrainRendering.ViewportTop,
            OriginalStrategicMapTerrainRendering.ViewportRight
                - OriginalStrategicMapTerrainRendering.ViewportLeft,
            OriginalStrategicMapTerrainRendering.ViewportBottom
                - OriginalStrategicMapTerrainRendering.ViewportTop);
        foreach (var tile in tiles)
        {
            if (tile.TileId >= frameCount)
                throw new InvalidDataException(
                    $"Strategic atlas contains {frameCount} frames but grid selected tile {tile.TileId}.");

            var visible = Intersect(
                new UiBounds(tile.X, tile.Y,
                    OriginalStrategicMapTerrainRendering.TileSpan,
                    OriginalStrategicMapTerrainRendering.TileSpan), viewport);
            if (visible.Width == 0 || visible.Height == 0) continue;
            blits.Add(new OriginalStrategicTerrainBlit(
                tile.TileId,
                visible,
                new UiBounds(visible.X - tile.X, visible.Y - tile.Y,
                    visible.Width, visible.Height)));
        }
        return blits;
    }

    private static UiBounds Intersect(UiBounds left, UiBounds right)
    {
        var x = Math.Max(left.X, right.X);
        var y = Math.Max(left.Y, right.Y);
        var rightEdge = Math.Min(left.X + left.Width, right.X + right.Width);
        var bottomEdge = Math.Min(left.Y + left.Height, right.Y + right.Height);
        return new UiBounds(x, y, Math.Max(0, rightEdge - x), Math.Max(0, bottomEdge - y));
    }
}
