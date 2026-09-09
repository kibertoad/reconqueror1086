namespace Conqueror.Game;

public static class PresentationScaling
{
    public const int VirtualWidth = 1024;
    public const int VirtualHeight = 768;

    public static UiBounds Destination(int viewportWidth, int viewportHeight, bool integerScaling)
    {
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        var scale = Math.Min(viewportWidth / (double)VirtualWidth, viewportHeight / (double)VirtualHeight);
        if (integerScaling && scale >= 1) scale = Math.Floor(scale);
        var width = Math.Max(1, (int)Math.Round(VirtualWidth * scale));
        var height = Math.Max(1, (int)Math.Round(VirtualHeight * scale));
        return new UiBounds((viewportWidth - width) / 2, (viewportHeight - height) / 2, width, height);
    }

    public static (int X, int Y) ToVirtual(int x, int y, UiBounds destination) =>
        ToLogical(x, y, destination, VirtualWidth, VirtualHeight);

    public static (int X, int Y) ToLogical(int x, int y, UiBounds destination, int logicalWidth, int logicalHeight)
    {
        if (logicalWidth <= 0) throw new ArgumentOutOfRangeException(nameof(logicalWidth));
        if (logicalHeight <= 0) throw new ArgumentOutOfRangeException(nameof(logicalHeight));
        return ((int)Math.Floor((x - destination.X) * logicalWidth / (double)destination.Width),
            (int)Math.Floor((y - destination.Y) * logicalHeight / (double)destination.Height));
    }
}
