namespace Conqueror.Core;

public sealed class FiefManagementCheckpoint
{
    private readonly int _wealth;
    private readonly int _population;
    private readonly int _taxRate;
    private readonly int _houses;
    private readonly bool _church;
    private readonly bool _monastery;
    private readonly bool _steward;
    private readonly bool _beadle;
    private readonly bool _priest;
    private readonly bool _servantRoom;
    private readonly bool _woodward;
    private readonly bool _prospector;
    private readonly Dictionary<CropType, int> _crops;
    private readonly Dictionary<ForestIndustry, int> _forest;
    private readonly Dictionary<UnitType, int> _army;
    private readonly int _journalCount;

    private FiefManagementCheckpoint(CampaignState state)
    {
        var player = state.Player;
        var fief = player.Home;
        _wealth = player.Wealth;
        _population = fief.Population;
        _taxRate = fief.TaxRate;
        _houses = fief.Houses;
        _church = fief.Church;
        _monastery = fief.Monastery;
        _steward = fief.Steward;
        _beadle = fief.Beadle;
        _priest = fief.Priest;
        _servantRoom = fief.ServantRoom;
        _woodward = fief.Woodward;
        _prospector = fief.Prospector;
        _crops = new Dictionary<CropType, int>(fief.Crops);
        _forest = new Dictionary<ForestIndustry, int>(fief.Forest);
        _army = new Dictionary<UnitType, int>(player.Army.Units);
        _journalCount = state.Journal.Count;
    }

    public static FiefManagementCheckpoint Capture(CampaignState state) => new(state);

    public void Restore(CampaignState state)
    {
        var player = state.Player;
        var fief = player.Home;
        player.Wealth = _wealth;
        fief.Population = _population;
        fief.TaxRate = _taxRate;
        fief.Houses = _houses;
        fief.Church = _church;
        fief.Monastery = _monastery;
        fief.Steward = _steward;
        fief.Beadle = _beadle;
        fief.Priest = _priest;
        fief.ServantRoom = _servantRoom;
        fief.Woodward = _woodward;
        fief.Prospector = _prospector;
        RestoreDictionary(fief.Crops, _crops);
        RestoreDictionary(fief.Forest, _forest);
        RestoreDictionary(player.Army.Units, _army);
        if (state.Journal.Count > _journalCount)
            state.Journal.RemoveRange(_journalCount, state.Journal.Count - _journalCount);
    }

    private static void RestoreDictionary<TKey>(Dictionary<TKey, int> destination, IReadOnlyDictionary<TKey, int> source)
        where TKey : notnull
    {
        destination.Clear();
        foreach (var (key, value) in source) destination.Add(key, value);
    }
}
