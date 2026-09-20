using Conqueror.Core;

namespace Conqueror.Game;

/// <summary>
/// Converts the recovered strategic player-marker pass into clipped source
/// sprite blits. It has no authority over strategic input or simulation.
/// </summary>
public readonly record struct OriginalStrategicMarkerBlit(
    int Slot,
    int FrameIndex,
    UiBounds Destination,
    UiBounds Source);

public static class OriginalStrategicMarkerPresentation
{
    /// <summary>
    /// Retains the original physical player-slot order, color-derived frame
    /// base, coordinate projection, and blitter clipping before host scaling.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMarkerBlit> BuildBlits(
        OriginalStrategicCampaignState state,
        int characterColor,
        int frameCount,
        int frameWidth,
        int frameHeight)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
        if (frameWidth <= 0) throw new ArgumentOutOfRangeException(nameof(frameWidth));
        if (frameHeight <= 0) throw new ArgumentOutOfRangeException(nameof(frameHeight));

        var frameBases = OriginalStrategicMapMarkerPresentation
            .FrameBasesForCharacterColor(characterColor);
        var draws = OriginalStrategicMapMarkerPresentation.BuildDraws(state, frameBases);
        var viewport = new UiBounds(
            OriginalStrategicMapTerrainRendering.ViewportLeft,
            OriginalStrategicMapTerrainRendering.ViewportTop,
            OriginalStrategicMapTerrainRendering.ViewportRight
                - OriginalStrategicMapTerrainRendering.ViewportLeft,
            OriginalStrategicMapTerrainRendering.ViewportBottom
                - OriginalStrategicMapTerrainRendering.ViewportTop);
        var blits = new List<OriginalStrategicMarkerBlit>(draws.Count);
        foreach (var draw in draws)
        {
            if (draw.Frame >= frameCount)
                throw new InvalidDataException(
                    $"Strategic marker sequence contains {frameCount} frames but slot {draw.Slot} selected frame {draw.Frame}.");
            if (!OriginalStrategicMapMarkerProjection.TryProjectBlitOrigin(
                    state, draw.SourceX, draw.SourceY, frameHeight, out var origin))
                continue;

            var full = new UiBounds(origin.X, origin.Y, frameWidth, frameHeight);
            var visible = Intersect(full, viewport);
            if (visible.Width == 0 || visible.Height == 0) continue;
            blits.Add(new OriginalStrategicMarkerBlit(
                draw.Slot,
                draw.Frame,
                visible,
                new UiBounds(visible.X - full.X, visible.Y - full.Y,
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
