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
        return BuildBlits(state, draws, frameCount, frameWidth, frameHeight);
    }

    /// <summary>
    /// Converts <c>draw_hostile_markers</c> (RULE-STRATEGY-015), drawn after the
    /// player markers with each record's stored frame.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMarkerBlit> BuildMovementBlits(
        OriginalStrategicCampaignState state,
        int frameCount,
        int frameWidth,
        int frameHeight)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
        if (frameWidth <= 0) throw new ArgumentOutOfRangeException(nameof(frameWidth));
        if (frameHeight <= 0) throw new ArgumentOutOfRangeException(nameof(frameHeight));

        return BuildBlits(state, OriginalStrategicMapMarkerPresentation.BuildMovementDraws(state),
            frameCount, frameWidth, frameHeight);
    }

    /// <summary>
    /// Converts the brigand markers of RULE-STRATEGY-017, which use the shared
    /// marker projection with frame 3.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMarkerBlit> BuildTemporaryForceBlits(
        OriginalStrategicCampaignState state,
        int frameCount,
        int frameWidth,
        int frameHeight)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (frameCount <= 0) throw new ArgumentOutOfRangeException(nameof(frameCount));
        if (frameWidth <= 0) throw new ArgumentOutOfRangeException(nameof(frameWidth));
        if (frameHeight <= 0) throw new ArgumentOutOfRangeException(nameof(frameHeight));

        return BuildBlits(state, OriginalStrategicMapMarkerPresentation.BuildTemporaryForceDraws(state),
            frameCount, frameWidth, frameHeight);
    }

    /// <summary>Builds the marker.CSF route-input overlay after map markers.</summary>
    public static IReadOnlyList<OriginalStrategicMarkerBlit> BuildRoutePreviewBlits(
        OriginalStrategicCampaignState state,
        int framePhase,
        int frameCount,
        int frameWidth,
        int frameHeight)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (frameCount < OriginalStrategicRoutePreview.FrameCount)
            throw new InvalidDataException("Strategic route overlay requires all four source marker frames.");
        if (frameWidth <= 0) throw new ArgumentOutOfRangeException(nameof(frameWidth));
        if (frameHeight <= 0) throw new ArgumentOutOfRangeException(nameof(frameHeight));

        return BuildBlits(state, OriginalStrategicRoutePreview.BuildDraws(state, framePhase),
            frameCount, frameWidth, frameHeight);
    }

    private static IReadOnlyList<OriginalStrategicMarkerBlit> BuildBlits(
        OriginalStrategicCampaignState state,
        IReadOnlyList<OriginalStrategicMapMarkerDraw> draws,
        int frameCount,
        int frameWidth,
        int frameHeight)
    {
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
