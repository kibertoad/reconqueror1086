namespace Conqueror.Core;

/// <summary>
/// RULE-JOUST-001 frame selection with the dragon rows. Above y 92 the
/// original scan runs past its table and always ends on frame 24
/// (BUG-JOUST-001), which this reproduces.
/// </summary>
public static class OriginalDragonLanceSelection
{
    public const int MinimumX = 50;
    public const int MaximumX = 400;
    public const int MinimumY = 20;
    public const int MaximumY = 280;
    public const int HorizontalBandWidth = (MaximumX - MinimumX + 2) / 5;

    public static int FrameFor(int x, int y)
    {
        x = Math.Clamp(x, MinimumX, MaximumX);
        y = Math.Clamp(y, MinimumY, MaximumY);
        if (y < 92) return 24;
        var verticalBand = y >= 240 ? 0 : y >= 188 ? 1 : y >= 136 ? 2 : y >= 114 ? 3 : 4;
        var horizontalBand = (x - MinimumX) / HorizontalBandWidth;
        return Math.Clamp(5 * (verticalBand + 1) - 1 - horizontalBand, 0, 24);
    }
}
