namespace Conqueror.Core;

/// <summary>
/// One eight-byte rectangle consumed by rectangle selector RULE-BATTLE-004.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterRectangle(
    int X,
    int Y,
    int Width,
    int Height)
{
    /// <summary>
    /// Mirrors predicate RULE-BATTLE-004: left/top are included, while the
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
    /// Builds the three control-strip rectangles appended directly after the
    /// live unit rectangle table by setup routine RULE-BATTLE-001. They retain
    /// their selector order: the first path is still unresolved, while the
    /// latter two are the two mapped control-code-one mutations.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle>
        CreateMappedControlStripRectangles(int controlStripMargin, int verticalSpan)
    {
        if (verticalSpan < 34)
            throw new ArgumentOutOfRangeException(nameof(verticalSpan));

        var top = checked(verticalSpan - 34);
        return [
            new(checked(controlStripMargin + 400), top, 84, 31),
            new(checked(controlStripMargin + 524), top, 41, 31),
            new(checked(controlStripMargin + 566), top, 41, 31),
        ];
    }

    /// <summary>
    /// Mirrors RULE-BATTLE-003. A non-positive unit emits the zero
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
    /// Mirrors RULE-BATTLE-004: returns the first matching rectangle's one-based
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

    /// <summary>
    /// Mirrors neighbor helper RULE-BATTLE-004. The source rectangle is
    /// temporarily replaced by a one-by-one offscreen rectangle, then its
    /// offset corners are probed in top-left, bottom-left, top-right,
    /// bottom-right order. A living hit returns one for the source lane or
    /// two for the other lane; every nonzero hit writes its zero-based record
    /// index to <paramref name="targetUnitIndex"/> before the living test.
    ///
    /// If the first corner hits a dead record, a miss at any later corner
    /// stops immediately. If the first corner misses, later misses instead
    /// continue to the next corner. PLACEHOLDER: RULE-BATTLE-004. The original
    /// also stops at a miss after a dead hit at a later corner, which this
    /// search does not.
    /// </summary>
    public static int ProbeMappedNeighborContact(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int sourceUnitIndex,
        int deltaX,
        int deltaY,
        int contentWidth,
        int contentHeight,
        ref int targetUnitIndex)
    {
        ArgumentNullException.ThrowIfNull(units);
        return ProbeMappedNeighborContact(
            units,
            units.Select(RenderRectangleFor).ToArray(),
            sourceUnitIndex,
            deltaX,
            deltaY,
            contentWidth,
            contentHeight,
            ref targetUnitIndex);
    }

    /// <summary>
    /// Runs <see cref="ProbeMappedNeighborContact(IReadOnlyList{OriginalStrategicInteractiveEncounterUnit}, int, int, int, int, int, ref int)"/>
    /// against the supplied live selector table. The table is a distinct
    /// source artifact: callers retain it when a tactical pass deliberately
    /// observes a non-positive record before its geometry is rebuilt.
    /// </summary>
    public static int ProbeMappedNeighborContact(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> rectangles,
        int sourceUnitIndex,
        int deltaX,
        int deltaY,
        int contentWidth,
        int contentHeight,
        ref int targetUnitIndex)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(rectangles);
        if ((uint)sourceUnitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(sourceUnitIndex));
        if (rectangles.Count != units.Count)
            throw new ArgumentException("Mapped selector rectangles must match the unit record count.",
                nameof(rectangles));

        var source = units[sourceUnitIndex];
        ArgumentNullException.ThrowIfNull(source);
        var sourceRectangle = rectangles[sourceUnitIndex];
        var probeRectangles = rectangles.ToArray();
        probeRectangles[sourceUnitIndex] = new(
            unchecked((short)(contentWidth + 1)),
            unchecked((short)(contentHeight + 1)),
            1,
            1);

        var corners = new[]
        {
            (X: checked(sourceRectangle.X + deltaX), Y: checked(sourceRectangle.Y + deltaY)),
            (X: checked(sourceRectangle.X + deltaX),
                Y: checked(sourceRectangle.Y + sourceRectangle.Height + deltaY)),
            (X: checked(sourceRectangle.X + sourceRectangle.Width + deltaX),
                Y: checked(sourceRectangle.Y + deltaY)),
            (X: checked(sourceRectangle.X + sourceRectangle.Width + deltaX),
                Y: checked(sourceRectangle.Y + sourceRectangle.Height + deltaY)),
        };

        var firstHit = FindFirstContainingOneBased(probeRectangles, corners[0].X, corners[0].Y);
        if (firstHit != 0)
        {
            if (TryReturnLivingContact(units, source, firstHit, ref targetUnitIndex, out var relation))
                return relation;

            for (var corner = 1; corner < corners.Length; corner++)
            {
                var hit = FindFirstContainingOneBased(probeRectangles, corners[corner].X, corners[corner].Y);
                if (hit == 0)
                    return 0;
                if (TryReturnLivingContact(units, source, hit, ref targetUnitIndex, out relation))
                    return relation;
            }

            return 0;
        }

        for (var corner = 1; corner < corners.Length; corner++)
        {
            var hit = FindFirstContainingOneBased(probeRectangles, corners[corner].X, corners[corner].Y);
            if (hit == 0)
                continue;
            if (TryReturnLivingContact(units, source, hit, ref targetUnitIndex, out var relation))
                return relation;
        }

        return 0;
    }

    /// <summary>
    /// Mirrors state-zero acquisition at RULE-BATTLE-007. It hides the
    /// source selector rectangle, then checks its four corners in top-left,
    /// bottom-left, top-right, bottom-right order. Unlike RULE-BATTLE-004, a
    /// selector hit is useful only for a living unit in the opposite lane and
    /// only that accepted hit writes <paramref name="targetUnitIndex"/>.
    ///
    /// The source has the same material first-corner split: after an initial
    /// hit (even a same-lane or dead one), a later selector miss ends the
    /// search; after an initial miss, later misses continue through remaining
    /// corners. PLACEHOLDER: RULE-BATTLE-007: the original also ends at a miss after any later hit.
    /// </summary>
    public static bool TryAcquireMappedOpposingTarget(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> rectangles,
        int sourceUnitIndex,
        int contentWidth,
        int contentHeight,
        ref int targetUnitIndex)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(rectangles);
        if ((uint)sourceUnitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(sourceUnitIndex));
        if (rectangles.Count != units.Count)
            throw new ArgumentException("Mapped selector rectangles must match the unit record count.",
                nameof(rectangles));

        var source = units[sourceUnitIndex];
        ArgumentNullException.ThrowIfNull(source);
        var rectangle = rectangles[sourceUnitIndex];
        var selector = rectangles.ToArray();
        selector[sourceUnitIndex] = new(
            unchecked((short)(contentWidth + 1)),
            unchecked((short)(contentHeight + 1)), 1, 1);
        var corners = new[]
        {
            (rectangle.X, rectangle.Y),
            (rectangle.X, checked(rectangle.Y + rectangle.Height)),
            (checked(rectangle.X + rectangle.Width), rectangle.Y),
            (checked(rectangle.X + rectangle.Width), checked(rectangle.Y + rectangle.Height)),
        };

        var firstHit = FindFirstContainingOneBased(selector, corners[0].Item1, corners[0].Item2);
        if (firstHit != 0)
        {
            if (TryAcceptOpposingTarget(units, source, firstHit, ref targetUnitIndex)) return true;
            for (var corner = 1; corner < corners.Length; corner++)
            {
                var hit = FindFirstContainingOneBased(selector, corners[corner].Item1, corners[corner].Item2);
                if (hit == 0) return false;
                if (TryAcceptOpposingTarget(units, source, hit, ref targetUnitIndex)) return true;
            }
            return false;
        }

        for (var corner = 1; corner < corners.Length; corner++)
        {
            var hit = FindFirstContainingOneBased(selector, corners[corner].Item1, corners[corner].Item2);
            if (hit != 0 && TryAcceptOpposingTarget(units, source, hit, ref targetUnitIndex)) return true;
        }
        return false;
    }

    private static bool TryReturnLivingContact(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        OriginalStrategicInteractiveEncounterUnit source,
        int oneBasedHit,
        ref int targetUnitIndex,
        out int relation)
    {
        var hitIndex = checked(oneBasedHit - 1);
        var candidate = units[hitIndex];
        ArgumentNullException.ThrowIfNull(candidate);
        targetUnitIndex = hitIndex;
        if (candidate.RemainingStrength <= 0)
        {
            relation = 0;
            return false;
        }

        relation = candidate.Side == source.Side ? 1 : 2;
        return true;
    }

    private static bool TryAcceptOpposingTarget(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        OriginalStrategicInteractiveEncounterUnit source,
        int oneBasedHit,
        ref int targetUnitIndex)
    {
        var hitIndex = checked(oneBasedHit - 1);
        var candidate = units[hitIndex];
        ArgumentNullException.ThrowIfNull(candidate);
        if (candidate.Side == source.Side || candidate.RemainingStrength <= 0) return false;
        targetUnitIndex = hitIndex;
        return true;
    }
}
