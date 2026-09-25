namespace Conqueror.Core;

/// <summary>
/// RULE-JOUST-003 dragon score. The session takes one sample per movie frame
/// (DEV-JOUST-001).
/// </summary>
public static class OriginalDragonRunScore
{
    public const int FullEquipmentBonus = 17;
    public const int LanceSlot = 0x36;
    public const int ArmorSlot = 0x40;
    public const int ShieldSlot = 0x3B;
    public const string LanceItem = "Dragon Slaying Lance";
    public const string ArmorItem = "Dragon Slaying Armor";
    public const string ShieldItem = "Shield of St. George";

    public static int EquipmentBonus(bool lance, bool armor, bool shield)
    {
        var bonus = (lance ? 4 : 0) + (armor ? 4 : 0) + (shield ? 4 : 0);
        return bonus == 12 ? 17 : bonus;
    }

    public static int EquipmentBonus(IReadOnlySet<string> items) => EquipmentBonus(
        items.Contains(LanceItem), items.Contains(ArmorItem), items.Contains(ShieldItem));

    public static int Threshold(int lanceExperience, int equipmentBonus) =>
        26 * (Math.Clamp(lanceExperience, 0, 20) - 20 + equipmentBonus);

    public static bool Succeeds(int lanceExperience, int equipmentBonus,
        int horizontalError, int verticalError)
    {
        var threshold = Threshold(lanceExperience, equipmentBonus);
        return threshold > horizontalError && threshold > verticalError;
    }
}
