using Conqueror.Core;

namespace Conqueror.Game;

public readonly record struct PracticeJoustLanceBlit(UiBounds Source, UiBounds Destination);

/// <summary>
/// RULE-JOUST-002 lance drawing: the frame is cropped to the 640-by-300 movie
/// region and painted at local y plus 90.
/// </summary>
public static class OriginalPracticeJoustLancePresentation
{
    public const int ClipRight = 639;
    public const int ClipBottom = 299;
    public const int MovieTop = OriginalPracticeJoustLance.MovieTop;

    public static PracticeJoustLanceBlit? Clip(int x, int y, int width, int height)
    {
        if (width <= 0 || height <= 0) return null;
        var left = Math.Max(0, x);
        var top = Math.Max(0, y);
        var right = Math.Min(ClipRight, checked(x + width));
        var bottom = Math.Min(ClipBottom, checked(y + height));
        if (right <= left || bottom <= top) return null;
        var clippedWidth = right - left;
        var clippedHeight = bottom - top;
        return new(new UiBounds(left - x, top - y, clippedWidth, clippedHeight),
            new UiBounds(left, top + MovieTop, clippedWidth, clippedHeight));
    }
}
