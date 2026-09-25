namespace Conqueror.Core;

public sealed record CropBalance(int Cost, int NormalRevenueAt50, int HarvestRevenueAt50, int Serfs = 5);
public sealed record ForestBalance(int Cost, int RevenueAt50, int Serfs);
public sealed record UnitBalance(int PriceAt50, int PriceAt100, int UpkeepAt50, int UpkeepAt100);
public sealed record EquipmentBalance(string Name, int BuyPrice, int Armor, int Power, bool Shop = true,
    EquipmentSlot Slot = EquipmentSlot.Weapon, int? OriginalStoreRecord = null, int? OriginalItemId = null)
{
    public int? OriginalWeaponItemId => Slot == EquipmentSlot.Weapon ? OriginalItemId ?? OriginalStoreRecord : null;
}
public sealed record BuildingDefinition(BuildingKind Kind, string Name, int Cost, int ProductivityBonus, bool Repeatable = false);
public sealed record CourtshipReward(int Win, int Wealth = 0, string? Item = null);
public sealed record CourtshipDefinition(string Name, bool CourtAble, int MinHonor, int? MaxPiety, bool FameWaivesPiety, int MarriageWins, CourtshipReward[] Rewards);
public sealed record VictoryDefinition(VictoryKind Kind, int LocationIndex, int RequiredFiefs, int RequiredArmy, int RequiredStrength, string[] RequiredItems);
public sealed record GrowthBand(double MinimumCapacity, double MonthlyRate);
public sealed record StrategicDefinition(int SpyCost, int InterceptionPercent, int MinimumFieldArmy);
public sealed record TournamentOpponentDefinition(string Name, int Wager, int JoustTolerance, int Swordsmen, int Halberdiers, int Knights);

public static class Balance
{
    public const int StartingYear = 1086;
    public const int StartingAge = 18;
    public const int FinalAge = 30;
    public const int StartingCustomWealth = 240;
    // RULE-ESTATE-003: the ceiling comes from SRC-MANUAL; the interest is half the loan.
    public const int MaxLoan = 200;
    public const decimal LoanInterest = 0.50m;
    public static readonly StrategicDefinition Strategy = new(80, 98, 9);

    // PLACEHOLDER: RULE-TOURNEY-002. The original draws five character rows for each tournament;
    // these fixed opponents, stakes (RULE-TOURNEY-003, RULE-TOURNEY-004), tolerances and unit
    // mixes are guesses.
    public static readonly TournamentOpponentDefinition[] TournamentOpponents =
    [
        new("Simon", 20, 20, 3, 3, 2),
        new("Richard", 35, 18, 2, 3, 3),
        new("Gerard", 50, 16, 3, 2, 3),
        new("Gilbert", 65, 14, 3, 3, 2),
        new("Hugh", 80, 12, 2, 3, 3)
    ];

    // PLACEHOLDER: RULE-ESTATE-005. Costs and revenues at 50% productivity from SRC-GAMEFAQS-66730.
    public static readonly IReadOnlyDictionary<CropType, CropBalance> Crops = new Dictionary<CropType, CropBalance>
    {
        [CropType.Grain] = new(1, 2, 25), [CropType.Beans] = new(1, 2, 25),
        [CropType.Vegetables] = new(1, 10, 10), [CropType.Fruit] = new(5, 5, 5)
    };

    // PLACEHOLDER: RULE-ESTATE-005. Costs, serfs and revenues from SRC-GAMEFAQS-66730.
    public static readonly IReadOnlyDictionary<ForestIndustry, ForestBalance> Forest = new Dictionary<ForestIndustry, ForestBalance>
    {
        [ForestIndustry.Timber] = new(5, 5, 5), [ForestIndustry.IronMine] = new(400, 15, 10),
        [ForestIndustry.CoalMine] = new(400, 31, 10), [ForestIndustry.GoldMine] = new(400, 36, 10),
        [ForestIndustry.SilverMine] = new(400, 15, 10)
    };

    // PLACEHOLDER: RULE-ESTATE-001. The two sets match the original, which picks the cheaper one when
    // FAME is above 16; scaling between them by productivity is a guess.
    public static readonly IReadOnlyDictionary<UnitType, UnitBalance> Units = new Dictionary<UnitType, UnitBalance>
    {
        [UnitType.Swordsmen] = new(28, 20, 12, 8),
        [UnitType.Halberdiers] = new(23, 15, 8, 5),
        [UnitType.Knights] = new(32, 24, 16, 10)
    };

    public static readonly IReadOnlyDictionary<UnitType, UnitType> Counters = new Dictionary<UnitType, UnitType>
    {
        [UnitType.Swordsmen] = UnitType.Halberdiers,
        [UnitType.Halberdiers] = UnitType.Knights,
        [UnitType.Knights] = UnitType.Swordsmen
    };

    // PLACEHOLDER: RULE-ESTATE-006. Growth bands from SRC-GAMEFAQS-66730.
    public static readonly GrowthBand[] PopulationGrowth =
    [
        new(1.00, .07), new(.50, .06), new(.36, .05), new(.29, .04),
        new(.21, .03), new(double.Epsilon, .01), new(0, -.017)
    ];

    public static readonly CharacterTemplate[] Templates =
    [
        new("Chaunce Norman", new(8, 8, 8, 8, 8), 240),
        new("Ronald DeMille", new(9, 8, 8, 10, 10), 490),
        new("Hayward Tussle", new(20, 20, 8, 16, 10), 740),
        new("Spencer Goodman", new(18, 16, 18, 19, 20), 1240),
        new("Mordred Knatchbull", new(18, 17, 0, 15, 0), 1240),
        new("Simon Hakluyt", new(7, 4, 4, 7, 2), 240)
    ];

    // PLACEHOLDER: RULE-ESTATE-004. Bonuses from SRC-GAMEFAQS-66730; the costs the guide does not give are guesses.
    public static readonly IReadOnlyDictionary<BuildingKind, BuildingDefinition> Buildings = new Dictionary<BuildingKind, BuildingDefinition>
    {
        [BuildingKind.House] = new(BuildingKind.House, "House", 5, 0, true),
        [BuildingKind.Church] = new(BuildingKind.Church, "Church", 100, 15),
        [BuildingKind.Monastery] = new(BuildingKind.Monastery, "Monastery", 150, 15),
        [BuildingKind.Steward] = new(BuildingKind.Steward, "Steward", 20, 10),
        [BuildingKind.Beadle] = new(BuildingKind.Beadle, "Beadle", 10, 5),
        [BuildingKind.Priest] = new(BuildingKind.Priest, "Priest", 10, 5),
        [BuildingKind.ServantRoom] = new(BuildingKind.ServantRoom, "Servant Room", 50, 0),
        [BuildingKind.Woodward] = new(BuildingKind.Woodward, "Woodward", 10, 5)
    };

    public static readonly EquipmentBalance[] Equipment =
    [
        new("Kingslayer Sword", 4000, 0, 182, OriginalStoreRecord: 0), new("Mercenary's Sword", 2800, 0, 174, OriginalStoreRecord: 4),
        new("Bishop's Sword", 3200, 0, 171, OriginalStoreRecord: 1), new("Thruster's Sword", 1500, 0, 143, OriginalStoreRecord: 7),
        new("Armor-Ripping Sword", 850, 0, 144, OriginalStoreRecord: 8), new("Defender's Sword", 900, 0, 130, OriginalStoreRecord: 6),
        new("Knight's Sword", 700, 0, 129, OriginalStoreRecord: 9), new("Irish Sword", 500, 0, 131, OriginalStoreRecord: 10),
        new("Battle Sword", 500, 0, 122, OriginalStoreRecord: 2), new("Danish Sword", 450, 0, 113, OriginalStoreRecord: 3),
        new("General Sword", 300, 0, 99, OriginalStoreRecord: 5), new("Heavy Crossbow", 0, 0, 143, false, OriginalItemId: 44),
        new("Light Crossbow", 0, 0, 130, false, OriginalItemId: 43), new("Spiked Mace", 90, 0, 138, OriginalStoreRecord: 21),
        new("Flanged Mace", 100, 0, 103, OriginalStoreRecord: 22), new("Battle Axe", 300, 0, 122, OriginalStoreRecord: 14),
        new("Horseman's Axe", 210, 0, 97, OriginalStoreRecord: 11), new("Saxon Axe", 120, 0, 82, OriginalStoreRecord: 12),
        new("Basic Axe", 122, 0, 81, OriginalStoreRecord: 13), new("War Hammer", 100, 0, 101, OriginalStoreRecord: 20),
        new("Hammer", 130, 0, 85, OriginalStoreRecord: 19), new("Stiletto Dagger", 34, 0, 68, OriginalStoreRecord: 18),
        new("Thruster's Dagger", 44, 0, 61, OriginalStoreRecord: 17), new("Fighter's Dagger", 84, 0, 60, OriginalStoreRecord: 16),
        new("Decorative Dagger", 100, 0, 34, OriginalStoreRecord: 15),
        new("Full Plate", 5000, 35, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 27), new("Half Plate", 3000, 30, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 37),
        new("Quarter Plate", 1600, 25, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 38), new("Chain Hauberk", 1800, 20, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 25),
        new("Chain Tunic", 1200, 15, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 24), new("Leather", 800, 10, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 26), new("Gambeson", 400, 5, 0, Slot: EquipmentSlot.Body, OriginalStoreRecord: 23),
        new("Heraldic Shield", 600, 10, 0, Slot: EquipmentSlot.Shield, OriginalStoreRecord: 36), new("Norman Shield", 200, 10, 0, Slot: EquipmentSlot.Shield, OriginalStoreRecord: 33),
        new("Decorative Shield", 300, 5, 0, Slot: EquipmentSlot.Shield, OriginalStoreRecord: 35), new("Saxon Shield", 120, 5, 0, Slot: EquipmentSlot.Shield, OriginalStoreRecord: 34),
        new("Tilting Shield", 50, 0, 0, Slot: EquipmentSlot.Shield, OriginalStoreRecord: 32), new("Great War Helm", 300, 15, 0, Slot: EquipmentSlot.Helm, OriginalStoreRecord: 30),
        new("War Helm", 180, 10, 0, Slot: EquipmentSlot.Helm, OriginalStoreRecord: 29), new("Norman Helm", 120, 5, 0, Slot: EquipmentSlot.Helm, OriginalStoreRecord: 31), new("Footman's Helm", 60, 0, 0, Slot: EquipmentSlot.Helm, OriginalStoreRecord: 28)
    ];

    public static readonly EquipmentBalance[] StoreEquipment = Equipment
        .Where(item => item.Shop)
        .OrderBy(item => item.OriginalStoreRecord ?? int.MaxValue)
        .ToArray();

    public static readonly CourtshipDefinition[] Courtships =
    [
        new("Adela", false, 0, null, false, 0, []),
        new("Jane", true, 8, null, false, 6,
        [new(1, Item:"Medallion"), new(2, Item:"Decorative Dagger"), new(3, Item:"Defender's Sword"), new(4, Item:"Hammer"), new(5, Item:"Dragon Slaying Lance")]),
        new("Anna Lisa", true, 8, 15, true, 3, [new(1, 5), new(2, 10)]),
        new("Victoria", true, 8, 15, false, 7,
        [new(3, Item:"Thruster's Dagger"), new(5, Item:"Saxon Axe"), new(6, Item:"Dragon Stone")]),
        new("Wendessa", true, 8, null, false, 7,
        [new(3, Item:"Dragon Stone"), new(4, Item:"Knight's Sword"), new(5, Item:"Shield of St. George")]),
        new("Valetta", true, 8, null, false, 5, [new(5, Item:"Dragon Slaying Armor")])
    ];

    public static readonly IReadOnlyDictionary<VictoryKind, VictoryDefinition> Victories = new Dictionary<VictoryKind, VictoryDefinition>
    {
        [VictoryKind.Crown] = new(VictoryKind.Crown, 1, 0, 1, 0, [])
    };

    public static int ScaleFrom50(int at50, int at100, int productivity) =>
        productivity <= 50 ? at50 : (int)Math.Round(at50 + (at100 - at50) * ((productivity - 50) / 50d));

    public static UnitType Counter(UnitType type) => Counters[type];
}
