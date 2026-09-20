namespace Conqueror.Core;

/// <summary>
/// Maps a strategic marker's integer route-space coordinate to the origin
/// supplied to the original map blitter. Pixel clipping remains the blitter's
/// responsibility.
/// </summary>
public static class OriginalStrategicMapMarkerProjection
{
    /// <summary>
    /// Mirrors the bounds and origin arithmetic in <c>0x3F0A0-0x3F205</c> for
    /// the 80-by-80 strategic-grid geometry. The original checks its source
    /// bounds inclusively, subtracts the marker image height, and lets the
    /// eventual blitter crop the resulting image rectangle.
    /// </summary>
    public static bool TryProjectBlitOrigin(
        OriginalStrategicCampaignState state,
        int sourceX,
        int sourceY,
        int imageHeight,
        out OriginalStrategicMapBlitOrigin origin)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (imageHeight < 0) throw new ArgumentOutOfRangeException(nameof(imageHeight));
        state.Validate();

        var tileSpan = OriginalStrategicMapTerrainRendering.TileSpan;
        var cameraOriginX = checked(state.CameraRow * tileSpan + tileSpan / 2);
        var cameraOriginY = checked((state.CameraColumn + 1) * tileSpan / 4);
        var viewportWidth = OriginalStrategicMapTerrainRendering.ViewportRight
            - OriginalStrategicMapTerrainRendering.ViewportLeft;
        var viewportHeight = OriginalStrategicMapTerrainRendering.ViewportBottom
            - OriginalStrategicMapTerrainRendering.ViewportTop;
        var verticalLimit = checked(cameraOriginY + tileSpan / 2 + viewportHeight);
        if (sourceX < cameraOriginX || sourceX > checked(cameraOriginX + viewportWidth)
            || sourceY < cameraOriginY || sourceY > verticalLimit)
        {
            origin = default;
            return false;
        }

        var skew = (cameraOriginY + viewportHeight) / 2;
        origin = new OriginalStrategicMapBlitOrigin(
            checked(sourceX - cameraOriginX - skew
                + OriginalStrategicMapTerrainRendering.ViewportLeft),
            checked(sourceY - cameraOriginY - imageHeight
                + OriginalStrategicMapTerrainRendering.ViewportTop));
        return true;
    }
}

/// <summary>
/// The destination supplied by the marker pass to its image blitter before
/// that blitter crops against the current surface.
/// </summary>
public readonly record struct OriginalStrategicMapBlitOrigin(int X, int Y);
