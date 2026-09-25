namespace Conqueror.Core;

/// <summary>
/// The two immutable side lanes used by resolver RULE-BATTLE-002's interactive
/// unit records. Their values are the original record field values, rather
/// than screen positions.
/// </summary>
public enum OriginalStrategicInteractiveEncounterSide
{
    Player = 0,
    Enemy = 0x168,
}

/// <summary>
/// PLACEHOLDER: FMT-BATTLE-001. The spec names category 0 halberdiers and 0x78
/// swordsmen (FND-BATTLE-007); these names keep the older reading.
/// </summary>
public enum OriginalStrategicInteractiveEncounterCategory
{
    Swordsmen = 0,
    Halberdiers = 0x78,
    Knights = 0xF0,
}

/// <summary>
/// The observable input-code branches in interactive dispatcher
/// RULE-BATTLE-008. The original event producer has not yet been recovered, so
/// these describe dispatch destinations rather than semantic user actions.
/// </summary>
public enum OriginalStrategicInteractiveEncounterInputRoute
{
    Ignored,
    PlayerSelection,
    ControlStrip,
    DestinationOrder,
}

/// <summary>
/// The outcome of the control-strip selector that dispatcher RULE-BATTLE-008
/// runs only for input code three. Its first control arms the mapped retreat
/// confirmation; the other controls retain their literal record mutations.
/// </summary>
public enum OriginalStrategicInteractiveEncounterControlStripRoute
{
    UnitSelectionFallback,
    RetreatConfirmation,
    SetControlCodeOneForSelectedRecords,
    SetControlCodeOneForLivingRecords,
}

/// <summary>
/// The terminal result returned by interactive resolver RULE-BATTLE-003 to
/// its caller. The original returns one for either a dispatcher exit or an
/// exhausted player lane and two for an exhausted enemy lane; its caller
/// separates the two return-one cases by inspecting the player count.
/// </summary>
public enum OriginalStrategicInteractiveEncounterOutcome
{
    InProgress,
    PlayerWithdrew,
    PlayerDefeated,
    EnemyDefeated,
}

/// <summary>
/// The short source-owned status line chosen before the resolver dispatches
/// input at RULE-BATTLE-008. A blank point shows the aggregate result
/// only for the one pass immediately following a unit hover.
/// </summary>
public enum OriginalStrategicInteractiveEncounterHoverKind
{
    None,
    PlayerStrength,
    Foe,
    Winning,
    Losing,
}

/// <summary>
/// One source status-line selection. <see cref="Strength"/> is meaningful
/// only for <see cref="OriginalStrategicInteractiveEncounterHoverKind.PlayerStrength"/>.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterHoverPresentation(
    OriginalStrategicInteractiveEncounterHoverKind Kind,
    int Strength = 0)
{
    public string Text => Kind switch
    {
        OriginalStrategicInteractiveEncounterHoverKind.PlayerStrength => $"OUR {Strength,3}%",
        OriginalStrategicInteractiveEncounterHoverKind.Foe => "FOE",
        OriginalStrategicInteractiveEncounterHoverKind.Winning => "WINNING",
        OriginalStrategicInteractiveEncounterHoverKind.Losing => "LOSING",
        _ => string.Empty,
    };
}

/// <summary>
/// Observable result of one explicit interactive-resolver frame. The result
/// keeps input, terminal dialog, and fixed tactical cadence distinct so hosts
/// do not accidentally make simulation rate depend on rendering throughput.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterFrameResult(
    OriginalStrategicInteractiveEncounterInputRoute InputRoute,
    bool ViewportScrolled,
    bool TacticalPassAdvanced,
    bool ResolverEnded)
{
    public OriginalStrategicInteractiveEncounterHoverPresentation HoverPresentation { get; init; }
    public OriginalStrategicInteractiveEncounterOutcome Outcome { get; init; }
}

/// <summary>
/// One live unit materialized by resolver RULE-BATTLE-002. The original
/// terminal write-back reads only <see cref="RemainingStrength"/> and the
/// category/side fields while strength is positive. The mapped knight-death
/// completion mutates its category column only after zeroing strength.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterUnit
{
    internal OriginalStrategicInteractiveEncounterUnit(
        OriginalStrategicInteractiveEncounterSide side,
        OriginalStrategicInteractiveEncounterCategory category,
        int initialHeadingOctant,
        int categoryValue)
    {
        Side = side;
        Category = category;
        HeadingOctant = initialHeadingOctant;
        CategoryValue = categoryValue;
    }

    public OriginalStrategicInteractiveEncounterSide Side { get; }
    public OriginalStrategicInteractiveEncounterCategory Category { get; private set; }

    /// <summary>
    /// Original record <c>+0x14</c>: an eight-way heading, initialized to
    /// three for player entries and seven for enemy entries. Helper
    /// RULE-BATTLE-006 turns this field one octant at a time toward a stored
    /// target and enters contact state when it already faces that target.
    /// </summary>
    public int HeadingOctant { get; internal set; }

    /// <summary>
    /// Original record <c>+0x24</c>: 10, 20, or 40 by category. Its tactical
    /// role remains intentionally unlabelled pending further analysis.
    /// </summary>
    public int CategoryValue { get; }

    /// <summary>
    /// Original record <c>+0x20</c>, initialized to 100. Completion counts
    /// this unit only while the value is strictly positive.
    /// </summary>
    public int RemainingStrength { get; set; } = 100;

    /// <summary>
    /// Mutable original record <c>+0x04/+0x08</c> formation coordinates.
    /// They are assigned after materialization by the selected menu layout.
    /// </summary>
    public int PositionX { get; set; }
    public int PositionY { get; set; }

    /// <summary>
    /// Original record <c>+0x0C/+0x10</c>. Constructor RULE-BATTLE-002
    /// initializes both fields to -1; the interactive input path later writes
    /// them as a paired transient destination. Their presentation name is not
    /// yet established.
    /// </summary>
    public int AuxiliaryX { get; set; } = -1;
    public int AuxiliaryY { get; set; } = -1;

    /// <summary>
    /// Original record <c>+0x18</c>, initialized to zero and advanced modulo
    /// five by the mapped death-completion path.
    /// </summary>
    public int PhaseCounter { get; set; }

    /// <summary>
    /// Original record <c>+0x1C</c>, initialized to zero. State code
    /// <c>0x50</c> is the confirmed completed-death state; the remaining code
    /// meanings await the tactical-loop trace.
    /// </summary>
    public int StateCode { get; set; }

    /// <summary>
    /// Original record <c>+0x28</c>. It initializes to zero for player entries
    /// and one for enemy entries, then is overwritten by interactive control
    /// paths; its semantic label remains deliberately unresolved.
    /// </summary>
    public int ControlCode { get; set; }

    /// <summary>
    /// Original record <c>+0x30</c>, initialized to -1 and later used as a
    /// table-relative unit index by the tactical loop.
    /// </summary>
    public int TargetUnitIndex { get; set; } = -1;

    internal void CompleteMappedDeathAnimation()
    {
        RemainingStrength = 0;
        if (Category == OriginalStrategicInteractiveEncounterCategory.Knights)
            Category = OriginalStrategicInteractiveEncounterCategory.Halberdiers;
    }
}

/// <summary>
/// Materializes and collapses the six-category interactive-resolver roster.
/// This is not a replacement combat algorithm; it preserves only the verified
/// constructor and terminal survivor write-back contract.
/// </summary>
public static class OriginalStrategicInteractiveEncounter
{
    public const int FormationGridStep = 60;
    public const int FormationGridInset = 30;

    /// <summary>
    /// Mirrors the input-code comparisons at RULE-BATTLE-008 and
    /// RULE-BATTLE-008. Code two reaches player selection, code three reaches
    /// the control strip, and codes six and seven share destination ordering;
    /// every other code returns to the loop without a mapped record mutation.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterInputRoute RouteMappedInputCode(int inputCode) =>
        inputCode switch
        {
            2 => OriginalStrategicInteractiveEncounterInputRoute.PlayerSelection,
            3 => OriginalStrategicInteractiveEncounterInputRoute.ControlStrip,
            6 or 7 => OriginalStrategicInteractiveEncounterInputRoute.DestinationOrder,
            _ => OriginalStrategicInteractiveEncounterInputRoute.Ignored,
        };

    /// <summary>Maps RULE-BATTLE-009's raw category routes, independently of the small pointer/control dispatcher codes.</summary>
    public static OriginalStrategicInteractiveEncounterCategory? RouteMappedCategorySelectionInputCode(
        int rawInputCode) =>
        rawInputCode switch
        {
            0x48 or 0x68 => OriginalStrategicInteractiveEncounterCategory.Swordsmen,
            0x4B or 0x6B => OriginalStrategicInteractiveEncounterCategory.Knights,
            0x53 or 0x73 => OriginalStrategicInteractiveEncounterCategory.Halberdiers,
            _ => null,
        };

    /// <summary>
    /// Mirrors the zero-based branch after the three-rectangle selector at
    /// RULE-BATTLE-008. A selector miss falls through to player-unit
    /// selection; the first hit takes a separately unresolved path; hits two
    /// and three perform the literal control-code mutations represented by
    /// the final two route values.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterControlStripRoute RouteMappedControlStripHit(
        int oneBasedHit) =>
        oneBasedHit switch
        {
            0 => OriginalStrategicInteractiveEncounterControlStripRoute.UnitSelectionFallback,
            1 => OriginalStrategicInteractiveEncounterControlStripRoute.RetreatConfirmation,
            2 => OriginalStrategicInteractiveEncounterControlStripRoute.SetControlCodeOneForSelectedRecords,
            3 => OriginalStrategicInteractiveEncounterControlStripRoute.SetControlCodeOneForLivingRecords,
            _ => throw new ArgumentOutOfRangeException(nameof(oneBasedHit)),
        };

    public static IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> Materialize(
        OriginalStrategicEncounterForces playerForces,
        OriginalStrategicEncounterForces enemyForces)
    {
        playerForces.Validate(nameof(playerForces));
        enemyForces.Validate(nameof(enemyForces));

        var units = new List<OriginalStrategicInteractiveEncounterUnit>(
            checked(playerForces.Total + enemyForces.Total));
        Append(units, OriginalStrategicInteractiveEncounterSide.Player, playerForces);
        Append(units, OriginalStrategicInteractiveEncounterSide.Enemy, enemyForces);
        return units;
    }

    /// <summary>
    /// Mirrors the record mutations at RULE-BATTLE-003 after a unit's
    /// death animation has completed. Timing and target selection remain in
    /// the not-yet-modeled tactical loop; this method deliberately represents
    /// only the completed-record effect.
    /// </summary>
    public static void CompleteMappedDeathAnimation(OriginalStrategicInteractiveEncounterUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        unit.CompleteMappedDeathAnimation();
    }

    /// <summary>
    /// Mirrors the selected-record destination write at RULE-BATTLE-008.
    /// The source clamps only the local y input to <c>[45, verticalSpan - 85]</c>,
    /// adds the live viewport offsets, clears record <c>+0x28</c>, and writes
    /// the paired <c>+0x0C/+0x10</c> fields in selected-list order.
    /// </summary>
    public static void ApplyMappedDestinationOrder(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<int> selectedUnitIndices,
        int localX,
        int localY,
        int horizontalOffset,
        int verticalOffset,
        int verticalSpan)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(selectedUnitIndices);
        if (verticalSpan <= 0)
            throw new ArgumentOutOfRangeException(nameof(verticalSpan));

        var boundedY = localY < 45 ? 45 : localY;
        var maximumY = checked(verticalSpan - 85);
        if (maximumY < boundedY)
            boundedY = maximumY;
        var targetX = checked(localX + horizontalOffset);
        var targetY = checked(boundedY + verticalOffset);
        foreach (var index in selectedUnitIndices)
        {
            if ((uint)index >= (uint)units.Count)
                throw new ArgumentOutOfRangeException(nameof(selectedUnitIndices));
            var unit = units[index];
            ArgumentNullException.ThrowIfNull(unit);
            unit.ControlCode = 0;
            unit.AuxiliaryX = targetX;
            unit.AuxiliaryY = targetY;
        }
    }

    /// <summary>
    /// Mirrors the player-hit selection append at RULE-BATTLE-008.
    /// The original accepts only a living player-lane unit, searches the
    /// existing byte-indexed list, and appends the unit only when absent; it
    /// does not toggle an existing entry.
    /// </summary>
    public static bool TryAppendMappedPlayerSelection(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IList<int> selectedUnitIndices,
        int unitIndex)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(selectedUnitIndices);
        if ((uint)unitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(unitIndex));
        var unit = units[unitIndex];
        ArgumentNullException.ThrowIfNull(unit);
        if (unit.Side != OriginalStrategicInteractiveEncounterSide.Player
            || unit.RemainingStrength <= 0
            || selectedUnitIndices.Contains(unitIndex))
            return false;

        selectedUnitIndices.Add(unitIndex);
        return true;
    }

    /// <summary>
    /// Mirrors the three player-category selection loops at
    /// RULE-BATTLE-009. Each loop scans the whole table in authored
    /// order, accepts only living player-lane entries with its immutable
    /// category value, and appends entries absent from the current selection
    /// without clearing the entries already there.
    /// </summary>
    public static int AppendMappedPlayerCategorySelection(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IList<int> selectedUnitIndices,
        OriginalStrategicInteractiveEncounterCategory category)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(selectedUnitIndices);
        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));

        var appended = 0;
        for (var index = 0; index < units.Count; index++)
        {
            var unit = units[index] ?? throw new ArgumentException("Unit list cannot contain null.", nameof(units));
            if (unit.Category != category)
                continue;
            if (TryAppendMappedPlayerSelection(units, selectedUnitIndices, index))
                appended++;
        }
        return appended;
    }

    /// <summary>
    /// Writes control code one to every record named by the current selected
    /// list, matching RULE-BATTLE-008. The source does not recheck
    /// strength or side while processing the already-populated list.
    /// </summary>
    public static void SetMappedControlCodeOneForSelectedRecords(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<int> selectedUnitIndices)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(selectedUnitIndices);
        foreach (var index in selectedUnitIndices)
        {
            if ((uint)index >= (uint)units.Count)
                throw new ArgumentOutOfRangeException(nameof(selectedUnitIndices));
            var unit = units[index];
            ArgumentNullException.ThrowIfNull(unit);
            unit.ControlCode = 1;
        }
    }

    /// <summary>
    /// Writes control code one to every positive-strength record, matching
    /// RULE-BATTLE-008. This source path does not filter by side.
    /// </summary>
    public static void SetMappedControlCodeOneForLivingRecords(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units)
    {
        ArgumentNullException.ThrowIfNull(units);
        foreach (var unit in units)
        {
            ArgumentNullException.ThrowIfNull(unit);
            if (unit.RemainingStrength > 0)
                unit.ControlCode = 1;
        }
    }

    /// <summary>
    /// Returns the source-compatible octant chosen by RULE-BATTLE-006 for a
    /// unit at <paramref name="sourceX"/>, <paramref name="sourceY"/> toward
    /// a target coordinate. A target coordinate of minus one follows the
    /// helper's axis fallback rather than being treated as an absent order.
    /// </summary>
    public static int DetermineMappedHeadingOctant(
        int sourceX,
        int sourceY,
        int targetX,
        int targetY)
    {
        if (sourceX == targetX || targetX == -1)
            return sourceY < targetY ? 1 : 5;
        if (sourceY == targetY || targetY == -1)
            return sourceX < targetX ? 3 : 7;
        if (sourceY > targetY)
            return sourceX < targetX ? 4 : 6;
        return sourceX < targetX ? 2 : 0;
    }

    /// <summary>
    /// Mirrors target-turn helper RULE-BATTLE-006. It reads the indexed
    /// target's live coordinates from record <c>+0x30</c>, rotates record
    /// <c>+0x14</c> through the shorter cyclic route (ties decrement), and
    /// enters state <c>0x28</c> with phase zero only when it already faces the
    /// target at the start of this call.
    /// </summary>
    public static bool AdvanceMappedTargetHeading(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int unitIndex)
    {
        ArgumentNullException.ThrowIfNull(units);
        if ((uint)unitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(unitIndex));

        var unit = units[unitIndex];
        ArgumentNullException.ThrowIfNull(unit);
        var targetIndex = unit.TargetUnitIndex;
        if ((uint)targetIndex >= (uint)units.Count)
            throw new InvalidOperationException("Mapped target turn requires a target record index.");
        var target = units[targetIndex];
        ArgumentNullException.ThrowIfNull(target);
        if (unit.HeadingOctant is < 0 or > 7)
            throw new InvalidOperationException("Mapped target turn requires an octant heading from zero through seven.");

        var desiredHeading = DetermineMappedHeadingOctant(
            unit.PositionX, unit.PositionY, target.PositionX, target.PositionY);
        if (unit.HeadingOctant == desiredHeading)
        {
            unit.PhaseCounter = 0;
            unit.StateCode = OriginalStrategicInteractiveEncounterCombat.ContactStateCode;
            return true;
        }

        TurnMappedHeadingOneOctant(unit, desiredHeading);
        return false;
    }

    /// <summary>
    /// Mirrors the no-target destination path at RULE-BATTLE-007.
    /// A state-zero unit without a table target first turns toward its paired
    /// <c>+0x0C/+0x10</c> destination. Once aligned, it processes vertical
    /// movement before horizontal movement in that same tactical pass. Knights
    /// probe and move ten units per axis; the other categories use five. Each
    /// step retains RULE-BATTLE-004's ordered contact semantics through the
    /// supplied pre-pass selector table: an opposing contact updates the live
    /// coordinate and turns toward its selected record, while a same-lane
    /// contact switches control code to one and reverses the stored axis
    /// destination by five with the original 90-pixel boundary reset.
    ///
    /// This is intentionally the source's narrow state-zero/no-target branch,
    /// not a replacement tactical loop: the caller supplies the immutable
    /// selector table built at the start of that source pass, and automatic
    /// behavior after both destination axes are absent remains unmapped.
    /// </summary>
    public static bool AdvanceMappedDestinationOrder(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> rectangles,
        int unitIndex,
        int contentWidth,
        int contentHeight)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(rectangles);
        if ((uint)unitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(unitIndex));
        if (rectangles.Count != units.Count)
            throw new ArgumentException("Mapped selector rectangles must match the unit record count.",
                nameof(rectangles));

        var unit = units[unitIndex];
        ArgumentNullException.ThrowIfNull(unit);
        if (unit.StateCode != 0 || unit.TargetUnitIndex != -1)
            return false;
        if (unit.AuxiliaryX == -1 && unit.AuxiliaryY == -1)
            return false;
        if (unit.HeadingOctant is < 0 or > 7)
            throw new InvalidOperationException("Mapped destination movement requires an octant heading from zero through seven.");

        var desiredHeading = DetermineMappedHeadingOctant(
            unit.PositionX, unit.PositionY, unit.AuxiliaryX, unit.AuxiliaryY);
        if (unit.HeadingOctant != desiredHeading)
        {
            TurnMappedHeadingOneOctant(unit, desiredHeading);
            return true;
        }

        AdvanceMappedDestinationAxis(units, rectangles, unitIndex, vertical: true,
            contentWidth: contentWidth, contentHeight: contentHeight);
        AdvanceMappedDestinationAxis(units, rectangles, unitIndex, vertical: false,
            contentWidth: contentWidth, contentHeight: contentHeight);
        return true;
    }

    /// <summary>
    /// PLACEHOLDER: RULE-BATTLE-007. The original copies the chosen unit's stored
    /// x destination to the source here. A living control-code-one source runs it only
    /// while both original lane counters remain positive. It scans the record
    /// table in order, excludes itself and its own lane, and keeps a candidate
    /// only when its positive-strength truncated Euclidean distance is
    /// strictly smaller than the initial <c>0x7FFF</c> bound. Thus authored
    /// table order breaks equal distances.
    ///
    /// The selected record normally supplies its live X/Y coordinates as the
    /// source's paired destination. If its stored X destination exists and
    /// differs from the source's live X, the source instead writes its own X
    /// into that selected record and still adopts the selected Y destination
    /// when present. The executable's invalid global-count/no-candidate path
    /// would index stale register state; this clean-room boundary safely
    /// reports no assignment rather than reproducing that clear fault.
    /// </summary>
    public static bool TryAssignMappedAutomaticDestination(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int unitIndex,
        int playerLaneCount,
        int enemyLaneCount)
    {
        ArgumentNullException.ThrowIfNull(units);
        if ((uint)unitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(unitIndex));
        if (playerLaneCount < 0)
            throw new ArgumentOutOfRangeException(nameof(playerLaneCount));
        if (enemyLaneCount < 0)
            throw new ArgumentOutOfRangeException(nameof(enemyLaneCount));

        var source = units[unitIndex];
        ArgumentNullException.ThrowIfNull(source);
        if (source.StateCode != 0
            || source.TargetUnitIndex != -1
            || source.AuxiliaryX != -1
            || source.AuxiliaryY != -1
            || source.ControlCode != 1
            || source.RemainingStrength <= 0
            || playerLaneCount == 0
            || enemyLaneCount == 0)
            return false;

        var chosenIndex = -1;
        var chosenDistance = 0x7FFF;
        for (var index = 0; index < units.Count; index++)
        {
            if (index == unitIndex)
                continue;
            var candidate = units[index];
            ArgumentNullException.ThrowIfNull(candidate);
            if (candidate.Side == source.Side || candidate.RemainingStrength <= 0)
                continue;

            var distance = CalculateMappedTruncatedDistance(source, candidate);
            if (distance >= chosenDistance)
                continue;
            chosenIndex = index;
            chosenDistance = distance;
        }

        if (chosenIndex < 0)
            return false;

        var chosen = units[chosenIndex];
        if (chosen.AuxiliaryX == -1 || chosen.AuxiliaryX == source.PositionX)
            source.AuxiliaryX = chosen.PositionX;
        else
            chosen.AuxiliaryX = source.PositionX;
        source.AuxiliaryY = chosen.AuxiliaryY == -1
            ? chosen.PositionY
            : chosen.AuxiliaryY;
        return true;
    }

    /// <summary>
    /// Composes state-zero record handling in the exact order reached from
    /// RULE-BATTLE-003: first run the distinct opposing-corner acquisition,
    /// turn immediately when a target field exists, otherwise process a
    /// paired destination, then finally attempt control-code-one automatic
    /// destination assignment. The caller supplies the rectangle table built
    /// before the unit pass, just as RULE-BATTLE-003 does; this method does not
    /// create an unverified outer scheduler or input loop.
    /// </summary>
    public static bool AdvanceMappedStateZero(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> rectangles,
        int unitIndex,
        int contentWidth,
        int contentHeight,
        int playerLaneCount,
        int enemyLaneCount)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(rectangles);
        if ((uint)unitIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(unitIndex));
        if (rectangles.Count != units.Count)
            throw new ArgumentException("Mapped selector rectangles must match the unit record count.",
                nameof(rectangles));

        var unit = units[unitIndex];
        ArgumentNullException.ThrowIfNull(unit);
        if (unit.StateCode != 0)
            return false;

        var targetUnitIndex = unit.TargetUnitIndex;
        OriginalStrategicInteractiveEncounterGeometry.TryAcquireMappedOpposingTarget(
            units, rectangles, unitIndex, contentWidth, contentHeight, ref targetUnitIndex);
        unit.TargetUnitIndex = targetUnitIndex;
        if (unit.TargetUnitIndex != -1)
        {
            AdvanceMappedTargetHeading(units, unitIndex);
            return true;
        }

        if (unit.AuxiliaryX != -1 || unit.AuxiliaryY != -1)
            return AdvanceMappedDestinationOrder(units, rectangles, unitIndex, contentWidth, contentHeight);

        return TryAssignMappedAutomaticDestination(units, unitIndex, playerLaneCount, enemyLaneCount);
    }

    /// <summary>
    /// PLACEHOLDER: RULE-BATTLE-002. Formations 0 to 2 leave the foe unplaced and
    /// make no draw, and the wedge uses the smallest row count; the original places
    /// the foe after every formation and widens a wedge of a non-triangular count.
    /// </summary>
    public static void ApplyMappedMenuFormation(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int menuCode,
        int verticalSpan)
    {
        ArgumentNullException.ThrowIfNull(units);
        if (menuCode is < 0 or > 2)
            throw new ArgumentOutOfRangeException(nameof(menuCode));
        var rows = verticalSpan / FormationGridStep;
        if (rows <= 0)
            throw new ArgumentOutOfRangeException(nameof(verticalSpan));

        var playerCount = 0;
        var enemySeen = false;
        foreach (var unit in units)
        {
            ArgumentNullException.ThrowIfNull(unit);
            if (unit.Side == OriginalStrategicInteractiveEncounterSide.Enemy)
            {
                enemySeen = true;
                continue;
            }
            if (enemySeen)
                throw new ArgumentException("Interactive units must retain the original player-first order.",
                    nameof(units));
            playerCount++;
        }

        if (menuCode == 2)
        {
            ApplyMenuCodeTwoTriangle(units, playerCount, verticalSpan);
            return;
        }

        for (var index = 0; index < playerCount; index++)
        {
            var column = index / rows;
            units[index].PositionX = menuCode == 0
                ? checked(column * FormationGridStep + FormationGridStep)
                : checked((playerCount / rows) * FormationGridStep + FormationGridStep
                    - column * FormationGridStep);
            units[index].PositionY = checked((index % rows) * FormationGridStep + FormationGridInset);
        }
    }

    /// <summary>
    /// Applies menu code 3's complete formation path at RULE-BATTLE-002.
    /// It divides the player swordsmen into two four-unit rows, then places
    /// the other player categories and one of two reachable hostile grids.
    /// The executable takes one signed remainder by two from its raw generator;
    /// this replacement requires a non-negative raw source and preserves the
    /// two resulting layouts without exposing the source's dead switch cases.
    /// </summary>
    public static void ApplyMappedMenuCodeThreeFormation(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int horizontalSpan,
        int verticalSpan,
        IOriginalStrategicEncounterRandom random)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(random);
        if (horizontalSpan <= 0)
            throw new ArgumentOutOfRangeException(nameof(horizontalSpan));
        var rows = verticalSpan / FormationGridStep;
        if (rows <= 0)
            throw new ArgumentOutOfRangeException(nameof(verticalSpan));

        var playerCount = ValidateOriginalPlayerFirstOrder(units);
        var playerSwordsmen = 0;
        while (playerSwordsmen < playerCount
            && units[playerSwordsmen].Category == OriginalStrategicInteractiveEncounterCategory.Swordsmen)
            playerSwordsmen++;
        for (var index = playerSwordsmen; index < playerCount; index++)
        {
            if (units[index].Category == OriginalStrategicInteractiveEncounterCategory.Swordsmen)
                throw new ArgumentException("Player units must retain the original category order.", nameof(units));
        }

        var halfSwordsmen = playerSwordsmen / 2;
        var otherPlayerUnits = checked(playerCount - playerSwordsmen);
        var swordBaseX = checked((halfSwordsmen / 4 + 1) * FormationGridStep);
        var playerColumnBaseX = checked(swordBaseX
            + otherPlayerUnits / rows * FormationGridStep);

        for (var index = 0; index < halfSwordsmen; index++)
        {
            units[index].PositionX = checked(playerColumnBaseX - index / 4 * FormationGridStep + 120);
            units[index].PositionY = checked(index % 4 * FormationGridStep + FormationGridInset);
        }

        var latterSwordsmen = checked(playerSwordsmen - halfSwordsmen);
        for (var index = 0; index < latterSwordsmen; index++)
        {
            var unit = units[halfSwordsmen + index];
            unit.PositionX = checked(playerColumnBaseX - index / 4 * FormationGridStep + 120);
            unit.PositionY = checked(index % 4 * FormationGridStep + 480);
        }

        for (var index = 0; index < otherPlayerUnits; index++)
        {
            var unit = units[playerSwordsmen + index];
            unit.PositionX = checked(playerColumnBaseX - index / rows * FormationGridStep);
            unit.PositionY = checked(index % rows * FormationGridStep + FormationGridInset);
        }

        var enemyCount = checked(units.Count - playerCount);
        var raw = random.NextRaw();
        if (raw < 0)
            throw new InvalidOperationException("Encounter random source returned a negative raw value.");
        var mirroredEnemyColumns = raw % 2 == 0;
        var enemyColumnBaseX = checked(horizontalSpan
            - (enemyCount / rows * FormationGridStep + FormationGridStep));
        for (var index = 0; index < enemyCount; index++)
        {
            var unit = units[playerCount + index];
            unit.PositionX = mirroredEnemyColumns
                ? checked(horizontalSpan - (index / rows * FormationGridStep + FormationGridStep))
                : checked(enemyColumnBaseX + index / rows * FormationGridStep);
            unit.PositionY = checked(index % rows * FormationGridStep + FormationGridInset);
        }
    }

    private static void AdvanceMappedDestinationAxis(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> rectangles,
        int unitIndex,
        bool vertical,
        int contentWidth,
        int contentHeight)
    {
        var unit = units[unitIndex];
        var target = vertical ? unit.AuxiliaryY : unit.AuxiliaryX;
        if (target == -1)
            return;

        var current = vertical ? unit.PositionY : unit.PositionX;
        var difference = checked(target - current);
        var step = unit.Category == OriginalStrategicInteractiveEncounterCategory.Knights ? 10 : 5;
        var direction = difference >= 0 ? 1 : -1;
        var distance = Math.Abs(difference);
        var advance = distance < 10 ? difference : checked(direction * step);
        var targetUnitIndex = unit.TargetUnitIndex;
        var contact = OriginalStrategicInteractiveEncounterGeometry.ProbeMappedNeighborContact(
            units, rectangles, unitIndex,
            vertical ? 0 : advance,
            vertical ? advance : 0,
            contentWidth, contentHeight,
            ref targetUnitIndex);
        unit.TargetUnitIndex = targetUnitIndex;

        if (contact == 1)
        {
            unit.ControlCode = 1;
            if (distance < 10)
            {
                SetMappedAxisTarget(unit, vertical, -1);
                return;
            }

            var boundary = checked((vertical ? contentHeight : contentWidth) - 90);
            var reversedTarget = checked(target - direction * 5);
            SetMappedAxisTarget(unit, vertical,
                direction > 0
                    ? reversedTarget < 0 ? boundary : reversedTarget
                    : reversedTarget >= boundary ? 90 : reversedTarget);
            return;
        }

        IncrementMappedPhase(unit);
        SetMappedAxisPosition(unit, vertical, checked(current + advance));
        if (distance < 10 && contact == 0)
            SetMappedAxisTarget(unit, vertical, -1);

        if (contact == 2)
            AdvanceMappedTargetHeading(units, unitIndex);
    }

    private static void TurnMappedHeadingOneOctant(
        OriginalStrategicInteractiveEncounterUnit unit,
        int desiredHeading)
    {
        var incrementDistance = Math.Abs(desiredHeading - (unit.HeadingOctant + 1)) % 8;
        var decrementDistance = Math.Abs(desiredHeading - (unit.HeadingOctant - 1)) % 8;
        unit.HeadingOctant = incrementDistance < decrementDistance
            ? (unit.HeadingOctant + 1) % 8
            : unit.HeadingOctant == 0 ? 7 : unit.HeadingOctant - 1;
    }

    private static void IncrementMappedPhase(OriginalStrategicInteractiveEncounterUnit unit) =>
        unit.PhaseCounter = (unit.PhaseCounter + 1) % 5;

    private static void SetMappedAxisPosition(
        OriginalStrategicInteractiveEncounterUnit unit,
        bool vertical,
        int value)
    {
        if (vertical)
            unit.PositionY = value;
        else
            unit.PositionX = value;
    }

    private static void SetMappedAxisTarget(
        OriginalStrategicInteractiveEncounterUnit unit,
        bool vertical,
        int value)
    {
        if (vertical)
            unit.AuxiliaryY = value;
        else
            unit.AuxiliaryX = value;
    }

    private static int CalculateMappedTruncatedDistance(
        OriginalStrategicInteractiveEncounterUnit source,
        OriginalStrategicInteractiveEncounterUnit candidate)
    {
        var deltaX = checked(source.PositionX - candidate.PositionX);
        var deltaY = checked(source.PositionY - candidate.PositionY);
        var squaredDistance = checked((long)deltaX * deltaX + (long)deltaY * deltaY);
        return checked((int)Math.Truncate(Math.Sqrt(squaredDistance)));
    }

    private static void ApplyMenuCodeTwoTriangle(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int playerCount,
        int verticalSpan)
    {
        if (playerCount == 0)
            return;

        var rowLength = 1;
        while (checked(rowLength * (rowLength + 1) / 2) < playerCount)
            rowLength++;

        var x = checked(rowLength * 45);
        var rowY = verticalSpan / 2;
        var index = 0;
        for (var count = 1; index < playerCount; count++)
        {
            var y = rowY;
            for (var column = 0; column < count && index < playerCount; column++, index++)
            {
                units[index].PositionX = x;
                units[index].PositionY = y;
                y = checked(y + FormationGridStep);
            }

            x = checked(x - 45);
            rowY = checked(rowY - FormationGridInset);
        }
    }

    private static int ValidateOriginalPlayerFirstOrder(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units)
    {
        var playerCount = 0;
        var enemySeen = false;
        foreach (var unit in units)
        {
            ArgumentNullException.ThrowIfNull(unit);
            if (unit.Side == OriginalStrategicInteractiveEncounterSide.Enemy)
            {
                enemySeen = true;
                continue;
            }
            if (enemySeen)
                throw new ArgumentException("Interactive units must retain the original player-first order.",
                    nameof(units));
            playerCount++;
        }

        return playerCount;
    }

    /// <summary>
    /// Mirrors RULE-BATTLE-001: reset three category totals, then scan
    /// live units in table order and retain only a strictly positive strength.
    /// </summary>
    public static OriginalStrategicEncounterForces CountSurvivors(
        IEnumerable<OriginalStrategicInteractiveEncounterUnit> units,
        OriginalStrategicInteractiveEncounterSide side)
    {
        ArgumentNullException.ThrowIfNull(units);
        var swordsmen = 0;
        var halberdiers = 0;
        var knights = 0;
        foreach (var unit in units)
        {
            ArgumentNullException.ThrowIfNull(unit);
            if (unit.Side != side || unit.RemainingStrength <= 0)
                continue;

            switch (unit.Category)
            {
                case OriginalStrategicInteractiveEncounterCategory.Swordsmen:
                    swordsmen++;
                    break;
                case OriginalStrategicInteractiveEncounterCategory.Halberdiers:
                    halberdiers++;
                    break;
                case OriginalStrategicInteractiveEncounterCategory.Knights:
                    knights++;
                    break;
                default:
                    throw new InvalidOperationException("Encounter unit has an unknown category column.");
            }
        }

        return new(swordsmen, halberdiers, knights);
    }

    private static void Append(
        ICollection<OriginalStrategicInteractiveEncounterUnit> units,
        OriginalStrategicInteractiveEncounterSide side,
        OriginalStrategicEncounterForces forces)
    {
        AppendCategory(units, side, OriginalStrategicInteractiveEncounterCategory.Swordsmen,
            forces.Swordsmen, categoryValue: 10);
        AppendCategory(units, side, OriginalStrategicInteractiveEncounterCategory.Halberdiers,
            forces.Halberdiers, categoryValue: 20);
        AppendCategory(units, side, OriginalStrategicInteractiveEncounterCategory.Knights,
            forces.Knights, categoryValue: 40);
    }

    private static void AppendCategory(
        ICollection<OriginalStrategicInteractiveEncounterUnit> units,
        OriginalStrategicInteractiveEncounterSide side,
        OriginalStrategicInteractiveEncounterCategory category,
        int count,
        int categoryValue)
    {
        var initialHeadingOctant = side == OriginalStrategicInteractiveEncounterSide.Player ? 3 : 7;
        for (var index = 0; index < count; index++)
        {
            var unit = new OriginalStrategicInteractiveEncounterUnit(side, category, initialHeadingOctant, categoryValue)
            {
                ControlCode = side == OriginalStrategicInteractiveEncounterSide.Player ? 0 : 1,
            };
            units.Add(unit);
        }
    }
}
