namespace Conqueror.Core;

public static class OriginalWeaponCombat
{
    private static readonly int[] CombatRowsByItemId =
    [
        4, 5, 12, 14, 6, 13, 11, 7, 9, 8, 10, 16,
        17, 18, 15, 3, 0, 1, 2, 19, 20, 21, 22
    ];

    private static readonly int[] BreakRatingsByCombatRow =
    [
        4, 5, 6, 3, 6, 5, 3, 4, 2, 6, 0, 2,
        1, 0, 1, 3, 2, 1, 1, 3, 1, 3, 3
    ];

    public static int CombatRowFor(int itemId)
    {
        if ((uint)itemId >= (uint)CombatRowsByItemId.Length)
            throw new ArgumentOutOfRangeException(nameof(itemId));
        return CombatRowsByItemId[itemId];
    }

    public static int? BreakRollRangeFor(int itemId)
    {
        var row = CombatRowFor(itemId);
        return row == 0 ? null : checked(200 + BreakRatingsByCombatRow[row] * 50);
    }
}
