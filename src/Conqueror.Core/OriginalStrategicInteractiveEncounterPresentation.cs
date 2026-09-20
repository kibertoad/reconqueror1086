namespace Conqueror.Core;

/// <summary>
/// One selected-unit overlay command emitted by the resolver presentation.
/// The source emits these after all ordinary unit sprites, retaining the
/// byte-list order in which selection was appended.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterOverlayDraw(
    int UnitIndex,
    int Frame,
    int X,
    int Y);

/// <summary>
/// One ordinary strategic-unit sprite command emitted before the selection
/// overlay layer.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterUnitDraw(
    int UnitIndex,
    int Frame,
    int X,
    int Y);

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
    public const int SelectionOverlayOffsetX = 5;
    public const int SelectionOverlayOffsetY = 7;
    public const int PendingFirstControlOffsetX = 15;
    public const int PendingFirstControlOffsetY = 6;
    public const int ControlStripOffsetX = 20;
    public const int ControlStripOffsetY = 6;
    public const int HoverStatusPanelOffsetX = 160;
    public const int HoverStatusPanelBottomOffset = 25;
    public const int HoverStatusPanelWidth = 83;
    public const int HoverStatusPanelHeight = 20;
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

    /// <summary>
    /// Mirrors pointer ordering at <c>0x28A48</c> and comparator
    /// <c>0x289CC-0x28A47</c>: non-positive-strength records first, then
    /// ascending live Y and X coordinates. The source comparator returns
    /// zero for exact ties; authored unit order is the deterministic
    /// compatibility tie-breaker rather than reproducing a library sort's
    /// unspecified equal-item rearrangement.
    /// </summary>
    public static IReadOnlyList<int> OrderedUnitIndicesFor(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        var indices = Enumerable.Range(0, units.Count).ToArray();
        Array.Sort(indices, (leftIndex, rightIndex) =>
        {
            var left = units[leftIndex] ?? throw new ArgumentException("Unit list cannot contain null.", nameof(units));
            var right = units[rightIndex] ?? throw new ArgumentException("Unit list cannot contain null.", nameof(units));
            var comparison = CompareForRenderer(left, right);
            return comparison != 0 ? comparison : leftIndex.CompareTo(rightIndex);
        });
        return indices;
    }

    /// <summary>
    /// Produces the confirmed ordinary-sprite pass. Selection overlays must
    /// be emitted separately afterwards through <see cref="SelectionOverlayDrawsFor"/>.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicInteractiveEncounterUnitDraw> UnitDrawsFor(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int horizontalScrollOffset,
        int verticalScrollOffset)
    {
        ArgumentNullException.ThrowIfNull(units);
        var ordered = OrderedUnitIndicesFor(units);
        var draws = new OriginalStrategicInteractiveEncounterUnitDraw[ordered.Count];
        for (var drawIndex = 0; drawIndex < ordered.Count; drawIndex++)
        {
            var unitIndex = ordered[drawIndex];
            var unit = units[unitIndex];
            var (x, y) = DrawPositionFor(unit, horizontalScrollOffset, verticalScrollOffset);
            draws[drawIndex] = new(unitIndex, FrameFor(unit), x, y);
        }
        return draws;
    }

    /// <summary>
    /// Mirrors <c>0x28B5C-0x28B9D</c>. Frame 720 is positioned relative to
    /// the selected unit's live coordinate, not centered within its 90-pixel
    /// ordinary sprite. The source applies its horizontal and vertical scroll
    /// globals before the separate five- and seven-pixel offsets.
    /// </summary>
    public static (int X, int Y) SelectionOverlayPositionFor(
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
            checked(unit.PositionX - horizontalScrollOffset - SelectionOverlayOffsetX),
            checked(unit.PositionY - verticalScrollOffset - SelectionOverlayOffsetY));
    }

    /// <summary>
    /// Produces the post-unit selection-overlay pass from the source's
    /// byte-indexed selected list. Ordinary unit sorting uses a separate,
    /// not-yet-recovered comparator; this method deliberately models only
    /// the confirmed overlay layer after that pass.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicInteractiveEncounterOverlayDraw>
        SelectionOverlayDrawsFor(
            IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
            IReadOnlyList<int> selectedUnitIndices,
            int horizontalScrollOffset,
            int verticalScrollOffset)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(selectedUnitIndices);

        var draws = new OriginalStrategicInteractiveEncounterOverlayDraw[selectedUnitIndices.Count];
        for (var drawIndex = 0; drawIndex < selectedUnitIndices.Count; drawIndex++)
        {
            var unitIndex = selectedUnitIndices[drawIndex];
            if ((uint)unitIndex >= (uint)units.Count)
                throw new ArgumentOutOfRangeException(nameof(selectedUnitIndices));
            var unit = units[unitIndex];
            var (x, y) = SelectionOverlayPositionFor(unit, horizontalScrollOffset, verticalScrollOffset);
            draws[drawIndex] = new(unitIndex, SelectionOverlayFrame, x, y);
        }

        return draws;
    }

    /// <summary>
    /// Mirrors the armed branch at <c>0x268C8-0x268E4</c>. It draws frame
    /// 721 against the first mapped control-strip rectangle, with the source
    /// offsets rather than treating this 39-by-13 frame as a centered sprite.
    /// </summary>
    public static (int X, int Y) PendingFirstControlDrawPositionFor(
        int controlStripMargin,
        int verticalSpan)
    {
        var rectangle = FirstMappedControlStripRectangle(controlStripMargin, verticalSpan);
        return (
            checked(rectangle.X + PendingFirstControlOffsetX),
            checked(rectangle.Y + PendingFirstControlOffsetY));
    }

    /// <summary>
    /// Mirrors setup's frame-722 draw at <c>0x25FCD-0x26000</c>. Its 57-by-14
    /// control-strip image is placed against the same first control rectangle
    /// but has a distinct horizontal source offset from the armed frame.
    /// </summary>
    public static (int X, int Y) ControlStripDrawPositionFor(
        int controlStripMargin,
        int verticalSpan)
    {
        var rectangle = FirstMappedControlStripRectangle(controlStripMargin, verticalSpan);
        return (
            checked(rectangle.X + ControlStripOffsetX),
            checked(rectangle.Y + ControlStripOffsetY));
    }

    /// <summary>
    /// Mirrors the status-panel placement at <c>0x2665E-0x2667D</c> and
    /// <c>0x2670E-0x2672D</c>. Hover labels use the same resolution margin as
    /// the control strip, but a separate fixed lower status-panel anchor.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterRectangle HoverStatusPanelBoundsFor(
        int controlStripMargin,
        int verticalSpan)
    {
        if (controlStripMargin < 0)
            throw new ArgumentOutOfRangeException(nameof(controlStripMargin));
        if (verticalSpan < HoverStatusPanelBottomOffset)
            throw new ArgumentOutOfRangeException(nameof(verticalSpan));
        return new(
            checked(controlStripMargin + HoverStatusPanelOffsetX),
            checked(verticalSpan - HoverStatusPanelBottomOffset),
            HoverStatusPanelWidth,
            HoverStatusPanelHeight);
    }

    private static OriginalStrategicInteractiveEncounterRectangle FirstMappedControlStripRectangle(
        int controlStripMargin,
        int verticalSpan) =>
        OriginalStrategicInteractiveEncounterGeometry.CreateMappedControlStripRectangles(
            controlStripMargin, verticalSpan)[0];

    private static int CompareForRenderer(
        OriginalStrategicInteractiveEncounterUnit left,
        OriginalStrategicInteractiveEncounterUnit right)
    {
        var leftIsNonPositive = left.RemainingStrength <= 0;
        var rightIsNonPositive = right.RemainingStrength <= 0;
        if (leftIsNonPositive != rightIsNonPositive)
            return leftIsNonPositive ? -1 : 1;
        var vertical = left.PositionY.CompareTo(right.PositionY);
        return vertical != 0 ? vertical : left.PositionX.CompareTo(right.PositionX);
    }
}
