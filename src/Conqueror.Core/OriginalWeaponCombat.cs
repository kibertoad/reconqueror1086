namespace Conqueror.Core;

public static class OriginalWeaponCombat
{
    public const int CombatRowCount = 25;
    private static readonly int[] CombatRowsByItemId =
    [
        4, 5, 12, 14, 6, 13, 11, 7, 9, 8, 10, 16,
        17, 18, 15, 3, 0, 1, 2, 19, 20, 21, 22
    ];

    private static readonly int[] BreakRatingsByCombatRow =
    [
        4, 5, 6, 3, 6, 5, 3, 4, 2, 6, 0, 2,
        1, 0, 1, 3, 2, 1, 1, 3, 1, 3, 3, 6, 6
    ];

    // CONQUER.EXE object 2 offset 0xCE14, columns 0-2 of each 28-byte row.
    private static readonly int[] DiceCountsByCombatRow =
    [
        1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 2,
        2, 2, 2, 2, 2, 1, 3, 3, 1, 1, 2, 2
    ];

    private static readonly int[] DieSidesByCombatRow =
    [
        6, 5, 5, 4, 9, 8, 9, 8, 5, 7, 9, 8, 8,
        8, 7, 7, 6, 6, 12, 4, 4, 16, 12, 6, 7
    ];

    private static readonly int[] ArmorPenetrationByCombatRow =
    [
        4, 5, 6, 3, 6, 5, 3, 4, 2, 6, 0, 2, 1,
        0, 1, 3, 2, 1, 1, 3, 1, 3, 3, 6, 6
    ];

    // CONQUER.EXE object 2 offset 0xCE24 + 28 * row, column 4 of each combat row.
    // Contact processing at 0x558D4/0x559CA adds 0x40 before comparing range.
    private static readonly int[] ContactDistancesByCombatRow =
    [
        350, 350, 350, 350, 450, 440, 430, 420, 420, 420, 420, 420, 420,
        420, 420, 400, 400, 400, 400, 380, 380, 360, 360, 7000, 8192
    ];

    public static int CombatRowFor(int itemId)
    {
        if ((uint)itemId < (uint)CombatRowsByItemId.Length) return CombatRowsByItemId[itemId];
        return itemId switch
        {
            43 => 23,
            44 => 24,
            _ => throw new ArgumentOutOfRangeException(nameof(itemId))
        };
    }

    public static int? BreakRollRangeFor(int itemId)
    {
        var row = CombatRowFor(itemId);
        return row == 0 ? null : checked(200 + BreakRatingsByCombatRow[row] * 50);
    }

    public static int ContactDistanceFor(int itemId) => ContactDistanceForCombatRow(CombatRowFor(itemId));

    public static int GridReachFor(int itemId) => Math.Max(1, ContactDistanceFor(itemId) >> 8);

    public static int ContactDistanceForCombatRow(int combatRow)
    {
        if ((uint)combatRow >= (uint)CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        return checked(ContactDistancesByCombatRow[combatRow] + 0x40);
    }

    public static int GridReachForCombatRow(int combatRow) =>
        Math.Max(1, ContactDistanceForCombatRow(combatRow) >> 8);

    public static int DamageFor(int itemId, int targetArmor, Random random)
        => DamageForCombatRow(CombatRowFor(itemId), targetArmor, random);

    public static int DamageForCombatRow(int combatRow, int targetArmor, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if ((uint)combatRow >= (uint)CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        if (targetArmor < 0) throw new ArgumentOutOfRangeException(nameof(targetArmor));
        var rolled = 0;
        for (var die = 0; die < DiceCountsByCombatRow[combatRow]; die++)
            rolled += random.Next(DieSidesByCombatRow[combatRow]) + 1;
        var mitigation = Math.Max(targetArmor - ArmorPenetrationByCombatRow[combatRow], 0);
        return Math.Max(rolled - mitigation, 0);
    }
}
