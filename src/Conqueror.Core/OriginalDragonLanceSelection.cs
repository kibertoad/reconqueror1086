namespace Conqueror.Core;

/// <summary>
/// Source lance-frame selection at 0x1B86C-0x1B8B4. The original vertical
/// scan has no end check below 92; clamp that input before selecting a band.
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
        var verticalBand = y >= 240 ? 0 : y >= 188 ? 1 : y >= 136 ? 2 : y >= 114 ? 3 : 4;
        var horizontalBand = (x - MinimumX) / HorizontalBandWidth;
        return Math.Clamp(5 * (verticalBand + 1) - 1 - horizontalBand, 0, 24);
    }
}
