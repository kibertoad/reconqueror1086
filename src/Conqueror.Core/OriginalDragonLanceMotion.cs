namespace Conqueror.Core;

/// <summary>
/// Dragon worker 0x1B644-0x1BA25, advanced once per source movie frame.
/// The original unrestricted worker loop has no stable wall-clock cadence.
/// </summary>
public sealed class OriginalDragonLanceMotion
{
    private int _x8 = 225 << 8;
    private int _y8 = 150 << 8;
    private int _velocityX8;
    private int _velocityY8;

    public int X { get; private set; } = 225;
    public int Y { get; private set; } = 150;
    public int VelocityX8 => _velocityX8;
    public int VelocityY8 => _velocityY8;

    public void Advance(int sourceFrame, int pointerX, int pointerY)
    {
        var nextX8 = _x8 + _velocityX8;
        var nextY8 = _y8 + _velocityY8;
        var nextX = nextX8 / 256;
        var nextY = nextY8 / 256;
        if (nextX < OriginalDragonLanceSelection.MinimumX)
        {
            nextX = OriginalDragonLanceSelection.MinimumX;
            nextX8 = nextX << 8;
        }
        else if (nextX > OriginalDragonLanceSelection.MaximumX)
        {
            nextX = OriginalDragonLanceSelection.MaximumX;
            nextX8 = nextX << 8;
        }
        if (nextY < OriginalDragonLanceSelection.MinimumY)
        {
            nextY = OriginalDragonLanceSelection.MinimumY;
            nextY8 = nextY << 8;
        }
        else if (nextY > OriginalDragonLanceSelection.MaximumY)
        {
            nextY = OriginalDragonLanceSelection.MaximumY;
            nextY8 = nextY << 8;
        }
        _x8 = nextX8;
        _y8 = nextY8;
        X = nextX;
        Y = nextY;

        _velocityX8 = _velocityX8 * 80 / 100 + ((pointerX - X) / 10) * 256;
        _velocityY8 = _velocityY8 * 80 / 100 + ((pointerY - Y) / 10) * 256;
        if (sourceFrame % (500 / OriginalDragonRunTimeline.FrameMilliseconds) == 0)
            _velocityY8 -= 5000;
    }
}
