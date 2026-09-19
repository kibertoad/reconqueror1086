namespace Conqueror.Core;

/// <summary>
/// Frame selection owned by the interactive strategic resolver's
/// <c>MEN8.CSF</c> presentation at <c>0x28AD8</c>.
/// </summary>
public static class OriginalStrategicInteractiveEncounterPresentation
{
    public const int UnitSpriteWidth = 90;
    public const int UnitSpriteHeight = 90;
    public const int UnitSpriteHalfWidth = UnitSpriteWidth / 2;
    public const int UnitSpriteHalfHeight = UnitSpriteHeight / 2;
    public const int UnitFrameCount = 720;
    public const int SelectionOverlayFrame = 720;
    public const int PendingFirstControlFrame = 721;
    public const int ControlStripFrame = 722;

    /// <summary>
    /// Mirrors <c>0x28AF9-0x28B20</c>: lane plus category, five times the
    /// live heading octant, the signed divide-by-five remainder of the phase,
    /// and the live state code select one ordinary sprite frame.
    /// </summary>
    public static int FrameFor(OriginalStrategicInteractiveEncounterUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (unit.HeadingOctant is < 0 or > 7)
            throw new ArgumentOutOfRangeException(nameof(unit), "Heading must be an octant from zero through seven.");
        if (unit.PhaseCounter < 0)
            throw new ArgumentOutOfRangeException(nameof(unit), "Phase counter cannot be negative.");

        var frame = checked((int)unit.Side + (int)unit.Category
            + checked(unit.HeadingOctant * 5) + unit.PhaseCounter % 5 + unit.StateCode);
        if ((uint)frame >= UnitFrameCount)
            throw new ArgumentOutOfRangeException(nameof(unit), "Mapped unit frame is outside MEN8's ordinary frame range.");
        return frame;
    }

    /// <summary>
    /// Mirrors the draw coordinates at <c>0x28B20-0x28B3F</c>. The source
    /// centers the 90-by-90 frame on record <c>+0x04/+0x08</c> after applying
    /// its horizontal and vertical scroll globals. Painter ordering is owned
    /// by the unresolved comparator passed through <c>0x28A48</c>.
    /// </summary>
    public static (int X, int Y) DrawPositionFor(
        OriginalStrategicInteractiveEncounterUnit unit,
        int horizontalScrollOffset,
        int verticalScrollOffset)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (horizontalScrollOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(horizontalScrollOffset));
        if (verticalScrollOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(verticalScrollOffset));
        return (
            checked(unit.PositionX - horizontalScrollOffset - UnitSpriteHalfWidth),
            checked(unit.PositionY - verticalScrollOffset - UnitSpriteHalfHeight));
    }
}
