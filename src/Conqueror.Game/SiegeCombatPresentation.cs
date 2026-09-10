using Conqueror.Core;

namespace Conqueror.Game;

public readonly record struct SiegeFrameRun(int Start, int Count)
{
    public int EndExclusive => checked(Start + Count);
}

public static class SiegeCombatPresentation
{
    private static readonly int[] CombatRowsByOriginalItemId =
    [
        4, 5, 12, 14, 6, 13, 11, 7, 9, 8, 10, 16,
        17, 18, 15, 3, 0, 1, 2, 19, 20, 21, 22
    ];
    private static readonly int[] ForegroundBasesByCombatRow =
    [
        41, 41, 41, 41, 39, 39, 39, 39, 39, 39, 39, 39,
        39, 39, 39, 27, 27, 27, 27, 33, 33, 36, 36, 30
    ];
    public const int OriginalWidth = 320;
    public const int OriginalHeight = 200;
    public const double FrameSeconds = 0.07;
    public static readonly UiBounds Viewport = new(26, 24, 167, 117);
    public static readonly UiBounds Radar = new(223, 111, 94, 84);
    public static readonly UiBounds HealthBar = new(60, 192, 137, 5);
    public static readonly UiBounds Message = new(5, 145, 205, 26);
    public static readonly UiBounds PrimaryStatus = new(225, 22, 90, 25);
    public static readonly UiBounds SecondaryStatus = new(225, 54, 90, 35);
    public static readonly SiegeFrameRun AxeAttack = new(27, 3);
    public static readonly SiegeFrameRun CrossbowAttack = new(30, 3);
    public static readonly SiegeFrameRun HammerAttack = new(33, 3);
    public static readonly SiegeFrameRun MaceAttack = new(36, 3);
    public static readonly SiegeFrameRun SwordAttack = new(39, 3);
    public static readonly SiegeFrameRun DaggerAttack = new(42, 1);
    // CONQUER.EXE 0x55171-0x55189 chooses base 43 for a fatal strike and
    // base 48 otherwise; 0x552FB-0x55395 renders offsets 0 through 3.
    public static readonly SiegeFrameRun FatalHitBlood = new(43, 4);
    public static readonly SiegeFrameRun WoundingHitBlood = new(48, 4);

    public static SiegeFrameRun AttackFramesFor(string? weapon)
    {
        if (weapon?.Contains("Crossbow", StringComparison.OrdinalIgnoreCase) == true) return CrossbowAttack;
        var definition = Balance.Equipment.FirstOrDefault(item =>
            item.Slot == EquipmentSlot.Weapon && string.Equals(item.Name, weapon, StringComparison.Ordinal));
        if (definition?.OriginalStoreRecord is { } itemId)
            return OriginalForegroundBaseFor(itemId) switch
            {
                27 => AxeAttack,
                33 => HammerAttack,
                36 => MaceAttack,
                39 => SwordAttack,
                41 => DaggerAttack,
                _ => SwordAttack
            };
        if (weapon?.Contains("Dagger", StringComparison.OrdinalIgnoreCase) == true) return DaggerAttack;
        if (weapon?.Contains("Axe", StringComparison.OrdinalIgnoreCase) == true) return AxeAttack;
        if (weapon?.Contains("Hammer", StringComparison.OrdinalIgnoreCase) == true) return HammerAttack;
        if (weapon?.Contains("Mace", StringComparison.OrdinalIgnoreCase) == true) return MaceAttack;
        return SwordAttack;
    }

    // CONQUER.EXE 0x588BB-0x58AC6 maps original item identifiers to ownership-mask
    // bits; 0x4CB20-0x4CCB0 maps the selected bit to its combat row.
    public static int OriginalCombatRowFor(int originalItemId)
    {
        if ((uint)originalItemId >= (uint)CombatRowsByOriginalItemId.Length)
            throw new ArgumentOutOfRangeException(nameof(originalItemId));
        return CombatRowsByOriginalItemId[originalItemId];
    }

    public static int OriginalForegroundBaseFor(int originalItemId) =>
        ForegroundBasesByCombatRow[OriginalCombatRowFor(originalItemId)];

    public static SiegeFrameRun BloodFramesFor(bool fatal) => fatal ? FatalHitBlood : WoundingHitBlood;
}
