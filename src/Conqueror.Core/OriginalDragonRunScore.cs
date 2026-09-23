namespace Conqueror.Core;

/// <summary>
/// The dragon worker's final per-axis score. Its sample count depends on the
/// original main loop, so the session takes one sample per movie frame.
/// </summary>
public static class OriginalDragonRunScore
{
    public const int FullEquipmentBonus = 17;

    public static int EquipmentBonus(bool armor, bool shield, bool lance)
    {
        var bonus = (armor ? 4 : 0) + (shield ? 4 : 0) + (lance ? 4 : 0);
        return bonus == 12 ? 17 : bonus;
    }

    public static int Threshold(int lanceExperience, int equipmentBonus) =>
        26 * (Math.Clamp(lanceExperience, 0, 20) - 20 + equipmentBonus);

    public static bool Succeeds(int lanceExperience, int equipmentBonus,
        int horizontalError, int verticalError)
    {
        var threshold = Threshold(lanceExperience, equipmentBonus);
        return threshold > horizontalError && threshold > verticalError;
    }
}
