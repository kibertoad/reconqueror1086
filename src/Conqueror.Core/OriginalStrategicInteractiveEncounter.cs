namespace Conqueror.Core;

/// <summary>
/// The two immutable side lanes used by resolver <c>0x28C38</c>'s interactive
/// unit records. Their values are the original record field values, rather
/// than screen positions.
/// </summary>
public enum OriginalStrategicInteractiveEncounterSide
{
    Player = 0,
    Enemy = 0x168,
}

/// <summary>
/// The immutable category column used to classify a live interactive unit at
/// resolver completion.
/// </summary>
public enum OriginalStrategicInteractiveEncounterCategory
{
    Swordsmen = 0,
    Halberdiers = 0x78,
    Knights = 0xF0,
}

/// <summary>
/// One live unit materialized by resolver <c>0x28C38</c>. The original
/// terminal write-back reads only <see cref="RemainingStrength"/> and the
/// category/side fields while strength is positive. The mapped knight-death
/// completion mutates its category column only after zeroing strength.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterUnit
{
    internal OriginalStrategicInteractiveEncounterUnit(
        OriginalStrategicInteractiveEncounterSide side,
        OriginalStrategicInteractiveEncounterCategory category,
        int combatTypeCode,
        int categoryValue)
    {
        Side = side;
        Category = category;
        CombatTypeCode = combatTypeCode;
        CategoryValue = categoryValue;
    }

    public OriginalStrategicInteractiveEncounterSide Side { get; }
    public OriginalStrategicInteractiveEncounterCategory Category { get; private set; }

    /// <summary>
    /// Original record <c>+0x14</c>: 3 for player entries and 7 for enemy
    /// entries. Its tactical meaning has not yet been assigned a name.
    /// </summary>
    public int CombatTypeCode { get; }

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
    /// Original record <c>+0x0C/+0x10</c>. Constructor <c>0x28C38</c>
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
    /// Mirrors the record mutations at <c>0x26D29-0x26D46</c> after a unit's
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
    /// Mirrors the selected-record destination write at <c>0x26A7F-0x26B64</c>.
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
    /// Mirrors the player-hit selection append at <c>0x26999-0x26A77</c>.
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
    /// Applies the mapped player-prefix formation paths in <c>0x2904B</c>,
    /// <c>0x290BA</c>, and <c>0x29132</c>. Menu code 3 has its own method
    /// because its exact path also consumes a raw draw and the viewport width.
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
    /// Applies menu code 3's complete formation path at <c>0x29209-0x29643</c>.
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
    /// Mirrors <c>0x263C4-0x26477</c>: reset three category totals, then scan
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
        var combatTypeCode = side == OriginalStrategicInteractiveEncounterSide.Player ? 3 : 7;
        for (var index = 0; index < count; index++)
        {
            var unit = new OriginalStrategicInteractiveEncounterUnit(side, category, combatTypeCode, categoryValue)
            {
                ControlCode = side == OriginalStrategicInteractiveEncounterSide.Player ? 0 : 1,
            };
            units.Add(unit);
        }
    }
}
