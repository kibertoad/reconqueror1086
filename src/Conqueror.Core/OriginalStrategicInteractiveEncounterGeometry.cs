namespace Conqueror.Core;

/// <summary>
/// One eight-byte rectangle consumed by rectangle selector <c>0x6FF10</c>.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterRectangle(
    int X,
    int Y,
    int Width,
    int Height)
{
    /// <summary>
    /// Mirrors predicate <c>0x64164</c>: left/top are included, while the
    /// right and bottom edges are excluded.
    /// </summary>
    public bool Contains(int x, int y) => x >= X && x < checked(X + Width)
        && y >= Y && y < checked(Y + Height);
}

/// <summary>
/// Geometry owned by the interactive encounter's presentation and neighbor
/// query path. It keeps selector return values one-based because callers in
/// the original unit table subtract one before indexing the record.
/// </summary>
public static class OriginalStrategicInteractiveEncounterGeometry
{
    public const int RenderOffsetX = 15;
    public const int RenderOffsetY = 20;
    public const int RenderWidth = 25;
    public const int RenderHeight = 30;

    /// <summary>
    /// Mirrors <c>0x26BAF-0x26BFC</c>. A non-positive unit emits the zero
    /// rectangle; a positive unit maps its live formation coordinates to the
    /// rectangle consumed by the later selector.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterRectangle RenderRectangleFor(
        OriginalStrategicInteractiveEncounterUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (unit.RemainingStrength <= 0)
            return default;
        return new(
            checked(unit.PositionX - RenderOffsetX),
            checked(unit.PositionY - RenderOffsetY),
            RenderWidth,
            RenderHeight);
    }

    /// <summary>
    /// Mirrors <c>0x6FF10</c>: returns the first matching rectangle's one-based
    /// index, or zero when no rectangle contains the point.
    /// </summary>
    public static int FindFirstContainingOneBased(
        IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> rectangles,
        int x,
        int y)
    {
        ArgumentNullException.ThrowIfNull(rectangles);
        for (var index = 0; index < rectangles.Count; index++)
        {
            if (rectangles[index].Contains(x, y))
                return checked(index + 1);
        }

        return 0;
    }
}
