namespace Conqueror.Game;

public readonly record struct SiegeFrameRun(int Start, int Count)
{
    public int EndExclusive => checked(Start + Count);
}

public static class SiegeCombatPresentation
{
    public const double FrameSeconds = 0.07;
    public static readonly SiegeFrameRun AxeAttack = new(27, 3);
    public static readonly SiegeFrameRun CrossbowAttack = new(30, 3);
    public static readonly SiegeFrameRun HammerAttack = new(33, 3);
    public static readonly SiegeFrameRun MaceAttack = new(36, 3);
    public static readonly SiegeFrameRun SwordAttack = new(39, 3);
    public static readonly SiegeFrameRun DaggerAttack = new(42, 1);
    public static readonly SiegeFrameRun PlayerBlood = new(43, 5);
    public static readonly SiegeFrameRun EnemyBlood = new(48, 5);

    public static SiegeFrameRun AttackFramesFor(string? weapon)
    {
        if (weapon?.Contains("Crossbow", StringComparison.OrdinalIgnoreCase) == true) return CrossbowAttack;
        if (weapon?.Contains("Dagger", StringComparison.OrdinalIgnoreCase) == true) return DaggerAttack;
        if (weapon?.Contains("Axe", StringComparison.OrdinalIgnoreCase) == true) return AxeAttack;
        if (weapon?.Contains("Hammer", StringComparison.OrdinalIgnoreCase) == true) return HammerAttack;
        if (weapon?.Contains("Mace", StringComparison.OrdinalIgnoreCase) == true) return MaceAttack;
        return SwordAttack;
    }
}
