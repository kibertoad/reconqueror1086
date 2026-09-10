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
        1, 0, 1, 3, 2, 1, 1, 3, 1, 3, 3, 6, 6
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

    public static int ContactDistanceFor(int itemId) =>
        checked(ContactDistancesByCombatRow[CombatRowFor(itemId)] + 0x40);

    public static int GridReachFor(int itemId) => Math.Max(1, ContactDistanceFor(itemId) >> 8);
}
