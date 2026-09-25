namespace Conqueror.Core;

// RULE-ESTATE-001: the screen stages its changes; OK writes them back and Cancel drops them.
public sealed class WarPlanningCheckpoint
{
    public const int CompanySize = 100;
    // PLACEHOLDER: RULE-ESTATE-001. SRC-MANUAL advises 60; the original does not enforce a limit.
    public const int MaximumCompaniesPerArmy = 60;

    private sealed record DivisionSnapshot(string Name, bool IsFielded, int Location,
        IReadOnlyDictionary<UnitType, int> Units);

    private readonly int _wealth;
    private readonly int _activeSpies;
    private readonly int? _joinedArmyIndex;
    private readonly DivisionSnapshot[] _divisions;
    private readonly string[] _journal;

    public WarPlanningCheckpoint(Campaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        var player = campaign.State.Player;
        player.EnsureArmyRoster();
        _wealth = player.Wealth;
        _activeSpies = player.ActiveSpies;
        _joinedArmyIndex = player.JoinedArmyIndex;
        _divisions = Enumerable.Range(0, Player.ArmyDivisionLimit).Select(index => new DivisionSnapshot(
            player.ArmyNameAt(index), player.ArmyIsFielded(index), player.ArmyLocationAt(index),
            new Dictionary<UnitType, int>(player.ArmyAt(index).Units))).ToArray();
        _journal = campaign.State.Journal.ToArray();
    }

    public void Restore(Campaign campaign)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        var player = campaign.State.Player;
        player.EnsureArmyRoster();
        player.Wealth = _wealth;
        player.ActiveSpies = _activeSpies;
        player.JoinedArmyIndex = _joinedArmyIndex;
        for (var index = 0; index < _divisions.Length; index++)
        {
            var snapshot = _divisions[index];
            if (index == 0) player.PrimaryArmyName = snapshot.Name;
            else player.AdditionalArmies[index - 1].Name = snapshot.Name;
            player.SetArmyFieldState(index, snapshot.IsFielded, snapshot.Location);
            RestoreDictionary(player.ArmyAt(index).Units, snapshot.Units);
        }
        campaign.State.Journal.Clear();
        campaign.State.Journal.AddRange(_journal);
    }

    private static void RestoreDictionary<TKey, TValue>(IDictionary<TKey, TValue> target,
        IReadOnlyDictionary<TKey, TValue> source) where TKey : notnull
    {
        target.Clear();
        foreach (var pair in source) target.Add(pair.Key, pair.Value);
    }
}
