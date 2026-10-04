using RefurbishedDinosaurs.Core.Presentation;

namespace Conqueror.Game;

public static class PresentationScaling
{
    public const int VirtualWidth = 1024;
    public const int VirtualHeight = 768;

    public static UiBounds Destination(int viewportWidth, int viewportHeight, bool integerScaling)
    {
        var rectangle = ViewportScaler.Destination(viewportWidth, viewportHeight,
            VirtualWidth, VirtualHeight, integerScaling);
        return new UiBounds(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    public static (int X, int Y) ToVirtual(int x, int y, UiBounds destination) =>
        ToLogical(x, y, destination, VirtualWidth, VirtualHeight);

    public static (int X, int Y) ToLogical(int x, int y, UiBounds destination, int logicalWidth, int logicalHeight) =>
        ViewportScaler.ToLogical(x, y,
            new IntRectangle(destination.X, destination.Y, destination.Width, destination.Height),
            logicalWidth, logicalHeight);
}
