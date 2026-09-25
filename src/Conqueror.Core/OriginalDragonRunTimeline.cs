namespace Conqueror.Core;

/// <summary>
/// RULE-JOUST-003 dragon run movie timing and its 26-frame target track.
/// Coordinates are in the 640x300 movie; the movie begins at screen y=90.
/// </summary>
public static class OriginalDragonRunTimeline
{
    public const int FrameMilliseconds = 71;
    public const int FirstTargetFrame = 108;
    public const int EndFrame = 134;
    public const int MovieTop = 90;
    public const int ScreenWidth = 640;
    public const int ScreenHeight = 480;

    // RULE-JOUST-003 target tables for movie frames 108..133.
    private static readonly int[] TargetX =
    [
        277, 277, 277, 277, 277, 277, 277, 276, 276, 276, 276, 277, 276,
        274, 275, 274, 274, 272, 272, 270, 269, 267, 265, 262, 258, 254
    ];

    private static readonly int[] TargetY =
    [
        125, 124, 124, 123, 122, 122, 122, 120, 119, 117, 117, 115, 115,
        114, 111, 108, 108, 104, 101, 98, 95, 91, 85, 81, 75, 68
    ];

    public static int FrameAt(TimeSpan elapsed) => elapsed <= TimeSpan.Zero
        ? 0
        : (int)Math.Min(EndFrame, elapsed.Ticks / (FrameMilliseconds * TimeSpan.TicksPerMillisecond));

    public static (int X, int Y)? TargetAt(int frame)
    {
        if (frame < FirstTargetFrame || frame >= EndFrame) return null;
        var index = frame - FirstTargetFrame;
        return (TargetX[index], TargetY[index] + MovieTop);
    }
}
