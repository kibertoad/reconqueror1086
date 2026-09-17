using Conqueror.Core;

namespace Conqueror.Game;

public readonly record struct SiegeFrameRun(int Start, int Count, bool Descending = false)
{
    public int EndExclusive => checked(Start + Count);
    public int First => Descending ? EndExclusive - 1 : Start;
    public int Last => Descending ? Start : EndExclusive - 1;

    public int Next(int frame)
    {
        if (frame < Start || frame >= EndExclusive) return -1;
        if (frame == Last) return -1;
        return frame + (Descending ? -1 : 1);
    }
}

public readonly record struct SiegeRetainerCommandButton(SiegeRetainerCommand Command, UiBounds Bounds);

public static class SiegeCombatPresentation
{
    private static readonly int[] ForegroundBasesByCombatRow =
    [
        41, 41, 41, 41, 39, 39, 39, 39, 39, 39, 39, 39,
        39, 39, 39, 27, 27, 27, 27, 33, 33, 36, 36, 30, 30
    ];
    public const int OriginalWidth = 320;
    public const int OriginalHeight = 200;
    public const int OriginalCursorCenterOffset = 9;
    public const double ProvisionalStepSeconds = 0.07;
    public static readonly UiBounds Viewport = new(26, 24, 167, 117);
    public static readonly UiBounds Radar = new(223, 111, 94, 84);
    public static readonly UiBounds HealthBar = new(60, 192, 137, 5);
    public static readonly UiBounds Message = new(5, 145, 205, 26);
    public static readonly UiBounds PrimaryStatus = new(225, 22, 90, 25);
    public static readonly UiBounds SecondaryStatus = new(225, 54, 90, 35);
    // CONQUER.EXE 0x55D20-0x55DAE divides (x - 4) by 56 in this y band.
    // The final button is clipped by the command region's exclusive x=210 bound.
    public static readonly IReadOnlyList<SiegeRetainerCommandButton> RetainerCommandButtons =
    [
        new(SiegeRetainerCommand.Attack, new UiBounds(4, 175, 56, 13)),
        new(SiegeRetainerCommand.Defend, new UiBounds(60, 175, 56, 13)),
        new(SiegeRetainerCommand.Follow, new UiBounds(116, 175, 56, 13)),
        new(SiegeRetainerCommand.Retreat, new UiBounds(172, 175, 38, 13))
    ];
    // CONQUER.EXE 0x5529B-0x55448 selects offset 2 on approach, offset 1
    // near contact, and offset 0 on return. Rows 23+ retain offset 2.
    public static readonly SiegeFrameRun AxeAttack = new(27, 3, true);
    public static readonly SiegeFrameRun CrossbowAttack = new(30, 3);
    public static readonly SiegeFrameRun HammerAttack = new(33, 3, true);
    public static readonly SiegeFrameRun MaceAttack = new(36, 3, true);
    public static readonly SiegeFrameRun SwordAttack = new(39, 3, true);
    public static readonly SiegeFrameRun DaggerAttack = new(42, 1);
    // CONQUER.EXE 0x55171-0x55189 chooses base 43 for a fatal strike and
    // base 48 otherwise; 0x552FB-0x55395 renders offsets 0 through 3.
    public static readonly SiegeFrameRun FatalHitBlood = new(43, 4);
    public static readonly SiegeFrameRun WoundingHitBlood = new(48, 4);

    public static SiegeFrameRun AttackFramesFor(string? weapon)
    {
        var definition = Balance.Equipment.FirstOrDefault(item =>
            item.Slot == EquipmentSlot.Weapon && string.Equals(item.Name, weapon, StringComparison.Ordinal));
        if (definition?.OriginalWeaponItemId is { } itemId)
            return OriginalForegroundBaseFor(itemId) switch
            {
                27 => AxeAttack,
                30 => CrossbowAttack,
                33 => HammerAttack,
                36 => MaceAttack,
                39 => SwordAttack,
                41 => DaggerAttack,
                _ => SwordAttack
            };
        if (weapon?.Contains("Crossbow", StringComparison.OrdinalIgnoreCase) == true) return CrossbowAttack;
        if (weapon?.Contains("Dagger", StringComparison.OrdinalIgnoreCase) == true) return DaggerAttack;
        if (weapon?.Contains("Axe", StringComparison.OrdinalIgnoreCase) == true) return AxeAttack;
        if (weapon?.Contains("Hammer", StringComparison.OrdinalIgnoreCase) == true) return HammerAttack;
        if (weapon?.Contains("Mace", StringComparison.OrdinalIgnoreCase) == true) return MaceAttack;
        return SwordAttack;
    }

    public static int OriginalForegroundBaseFor(int originalItemId) =>
        ForegroundBasesByCombatRow[OriginalWeaponCombat.CombatRowFor(originalItemId)];

    public static int OriginalCombatRowFor(string? weapon)
    {
        var definition = Balance.Equipment.FirstOrDefault(item =>
            item.Slot == EquipmentSlot.Weapon && string.Equals(item.Name, weapon, StringComparison.Ordinal));
        if (definition?.OriginalWeaponItemId is { } itemId) return OriginalWeaponCombat.CombatRowFor(itemId);
        if (weapon?.Contains("Crossbow", StringComparison.OrdinalIgnoreCase) == true) return 23;
        if (weapon?.Contains("Dagger", StringComparison.OrdinalIgnoreCase) == true) return 0;
        if (weapon?.Contains("Axe", StringComparison.OrdinalIgnoreCase) == true) return 15;
        if (weapon?.Contains("Hammer", StringComparison.OrdinalIgnoreCase) == true) return 19;
        if (weapon?.Contains("Mace", StringComparison.OrdinalIgnoreCase) == true) return 21;
        return 4;
    }

    public static int SetupFrameForCombatRow(int combatRow, int baseFrame) => combatRow switch
    {
        <= 3 => baseFrame,
        <= 14 => baseFrame + 1,
        <= 22 => baseFrame + 2,
        _ => baseFrame
    };

    public static SiegeFrameRun BloodFramesFor(bool fatal) => fatal ? FatalHitBlood : WoundingHitBlood;

    public static UiBounds BackdropSource(Facing facing, int width, int height)
    {
        var panoramaRemainder = width - OriginalWidth;
        if (panoramaRemainder < 0 || panoramaRemainder % 3 != 0 || height < OriginalHeight)
            throw new InvalidDataException("The combat backdrop does not contain four overlapping screen views.");
        var facingStride = panoramaRemainder / 3;
        return new UiBounds(
            (int)facing * facingStride + Viewport.X,
            Viewport.Y,
            Viewport.Width,
            Viewport.Height);
    }

    public static SiegeRetainerCommand? RetainerCommandAt(int x, int y)
    {
        foreach (var button in RetainerCommandButtons)
            if (x >= button.Bounds.X && x < button.Bounds.X + button.Bounds.Width &&
                y >= button.Bounds.Y && y < button.Bounds.Y + button.Bounds.Height)
                return button.Command;
        return null;
    }

    // CONQUER.EXE 0x55DB8-0x55DE5 adds nine pixels to the stored cursor
    // top-left before raycasting; 0x5590C-0x5591D forwards that center point.
    public static (int X, int Y)? ForegroundTarget(int pointerX, int pointerY, UiBounds viewport)
    {
        var x = (int)Math.Floor((pointerX - viewport.X) * Viewport.Width / (double)viewport.Width)
            + OriginalCursorCenterOffset;
        var y = (int)Math.Floor((pointerY - viewport.Y) * Viewport.Height / (double)viewport.Height)
            + OriginalCursorCenterOffset;
        return x >= 0 && x < Viewport.Width && y >= 0 && y < Viewport.Height ? (x, y) : null;
    }
}
