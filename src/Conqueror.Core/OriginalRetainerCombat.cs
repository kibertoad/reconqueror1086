namespace Conqueror.Core;

public static class OriginalRetainerCombat
{
    public const int SoldiersPerRetainer = 50;
    public const int MinimumRetainers = 2;
    public const int MaximumRetainers = 10;

    public static int RetainerCapFor(int armyTotal)
    {
        if (armyTotal < 0) throw new ArgumentOutOfRangeException(nameof(armyTotal));
        return Math.Clamp(armyTotal / SoldiersPerRetainer, MinimumRetainers, MaximumRetainers);
    }
}
