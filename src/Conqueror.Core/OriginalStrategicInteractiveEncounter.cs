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
/// immutable category/side fields; later tactical simulation may update its
/// remaining strength without losing that classification.
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
    public OriginalStrategicInteractiveEncounterCategory Category { get; }

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
}

/// <summary>
/// Materializes and collapses the six-category interactive-resolver roster.
/// This is not a replacement combat algorithm; it preserves only the verified
/// constructor and terminal survivor write-back contract.
/// </summary>
public static class OriginalStrategicInteractiveEncounter
{
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
            units.Add(new(side, category, combatTypeCode, categoryValue));
    }
}
