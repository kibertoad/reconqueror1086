namespace Conqueror.Core;

/// <summary>
/// RULE-JOUST-001 lance motion with the RULE-JOUST-002 practice rows and
/// p = 120. One pass runs per decoded movie frame (DEV-JOUST-001).
/// </summary>
public sealed class OriginalPracticeJoustLance
{
    public const int MinimumX = 50;
    public const int MaximumX = 400;
    public const int MinimumY = 20;
    public const int MaximumY = 280;
    public const int MovieTop = 90;
    public const int HorizontalBandWidth = (MaximumX - MinimumX + 2) / 5;
    public const int SourceFrameMilliseconds = 71;
    private const int JoustParameter = 120;
    private const int VerticalKick8 = (150 - JoustParameter) * 50;
    private const int KickFrameInterval = 500 / SourceFrameMilliseconds;

    private int _x8 = 225 << 8;
    private int _y8 = 150 << 8;
    private int _velocityX8;
    private int _velocityY8;

    public int X { get; private set; } = 225;
    public int Y { get; private set; } = 150;
    public int VelocityX8 => _velocityX8;
    public int VelocityY8 => _velocityY8;
    public int Frame => FrameFor(X, Y);

    public static int FrameFor(int x, int y)
    {
        x = Math.Clamp(x, MinimumX, MaximumX);
        y = Math.Clamp(y, MinimumY, MaximumY);
        var verticalBand = y >= 150 ? 0 : y >= 98 ? 1 : y >= 46 ? 2 : y >= 24 ? 3 : 4;
        var horizontalBand = (x - MinimumX) / HorizontalBandWidth;
        return Math.Clamp(5 * (verticalBand + 1) - 1 - horizontalBand, 0, 24);
    }

    public void Advance(int sourceFrame, int rawPointerX, int rawPointerY)
    {
        _x8 += _velocityX8;
        _y8 += _velocityY8;
        var x = _x8 / 256;
        var y = _y8 / 256;
        X = Math.Clamp(x, MinimumX, MaximumX);
        Y = Math.Clamp(y, MinimumY, MaximumY);
        // Only a crossed bound resets the fixed-point remainder.
        if (X != x) _x8 = X << 8;
        if (Y != y) _y8 = Y << 8;

        _velocityX8 = _velocityX8 * 80 / 100
            + ((rawPointerX - X) / 10) * 0x3200 / JoustParameter;
        _velocityY8 = _velocityY8 * 80 / 100
            + ((rawPointerY - Y) / 10) * 0x3200 / JoustParameter;
        if (sourceFrame % KickFrameInterval == 0) _velocityY8 -= VerticalKick8;
    }
}
