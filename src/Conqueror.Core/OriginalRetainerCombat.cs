namespace Conqueror.Core;

public static class OriginalRetainerCombat
{
    public const int SoldiersPerCampaignRetainer = 3;
    public const int MaximumRetainersPerUnitType = 3;

    public static int CampaignRetainerCapFor(Army army)
    {
        ArgumentNullException.ThrowIfNull(army);
        var cap = Enum.GetValues<UnitType>().Sum(type =>
            Math.Min(army.Units[type] / SoldiersPerCampaignRetainer, MaximumRetainersPerUnitType));
        return Math.Max(1, cap);
    }

    public static void ApplyCampaignLosses(Army army, int retainerLosses)
    {
        ArgumentNullException.ThrowIfNull(army);
        if (retainerLosses < 0) throw new ArgumentOutOfRangeException(nameof(retainerLosses));

        var roster = Enum.GetValues<UnitType>().ToDictionary(type => type,
            type => Math.Min(army.Units[type] / SoldiersPerCampaignRetainer, MaximumRetainersPerUnitType));
        if (roster.Values.Sum() == 0)
        {
            var firstPresent = Enum.GetValues<UnitType>().FirstOrDefault(type => army.Units[type] > 0);
            if (army.Units[firstPresent] > 0) roster[firstPresent] = 1;
        }

        var remaining = retainerLosses;
        foreach (var type in Enum.GetValues<UnitType>())
        {
            var losses = Math.Min(remaining, roster[type]);
            army.Units[type] -= losses;
            remaining -= losses;
            if (remaining == 0) break;
        }
    }
}
