namespace Conqueror.Core;

public enum UnitType { Swordsmen, Halberdiers, Knights }
public enum CropType { Grain, Beans, Vegetables, Fruit }
public enum ForestIndustry { Timber, IronMine, CoalMine, GoldMine, SilverMine }
public enum VictoryKind { None, Crown, Dragon, Defeat }
public enum LocationKind { Home, Village, Castle, City, Tournament, London, DragonLair }
public enum EquipmentSlot { Weapon, Body, Shield, Helm, Keepsake }
public enum BuildingKind { House, Church, Monastery, Steward, Beadle, Priest, ServantRoom, Woodward }

public sealed record CharacterStats(int Strength, int Dexterity, int Piety, int Stamina, int Honor, int Intelligence = 8)
{
    public CharacterStats Clamp() => new(
        Math.Clamp(Strength, 0, 20), Math.Clamp(Dexterity, 1, 20),
        Math.Clamp(Piety, 0, 20), Math.Clamp(Stamina, 0, 20), Math.Clamp(Honor, 0, 20),
        Math.Clamp(Intelligence, 0, 20));

    public string DescribeStrength => Describe(Strength, "Feeble", "Runt", "Weak", "Average", "Brawny", "Mighty", "Herculean");
    public string DescribeDexterity => Describe(Dexterity, "Unskilled", "Unskilled", "Clumsy", "Adequate", "Handy", "Skilled", "Expert");
    public string DescribePiety => Describe(Piety, "Evil", "Black-Hearted", "Sordid", "Amoral", "Upright", "Righteous", "Saint");
    public string DescribeStamina => Describe(Stamina, "Exhausted", "Weary", "Dragging", "Average", "Athletic", "Tireless", "Dynamo");
    public string DescribeHonor => Describe(Honor, "Blackguard", "Despicable", "Unprincipled", "Decent", "Gallant", "Valiant", "Chivalrous");
    public string DescribeIntelligence => Describe(Intelligence, "Foolish", "Slow", "Dull", "Average", "Clever", "Brilliant", "Genius");

    private static string Describe(int value, params string[] names) => value switch
    {
        0 => names[0], <= 3 => names[1], <= 7 => names[2], <= 11 => names[3],
        <= 15 => names[4], <= 19 => names[5], _ => names[6]
    };
}

public sealed record CharacterTemplate(string Name, CharacterStats Stats, int Wealth);

public sealed class Inventory
{
    public HashSet<string> Items { get; init; } = new(StringComparer.OrdinalIgnoreCase)
    {
        "Knight's Sword", "Fighter's Dagger", "Gambeson", "Tilting Shield", "Footman's Helm"
    };
    public string Weapon { get; set; } = "Knight's Sword";
    public string Armor { get; set; } = "Gambeson";
    public string Shield { get; set; } = "Tilting Shield";
    public string Helm { get; set; } = "Footman's Helm";
    public int CrossbowBolts { get; set; }

    public void Equip(EquipmentBalance item)
    {
        switch (item.Slot)
        {
            case EquipmentSlot.Weapon: Weapon = item.Name; break;
            case EquipmentSlot.Body: Armor = item.Name; break;
            case EquipmentSlot.Shield: Shield = item.Name; break;
            case EquipmentSlot.Helm: Helm = item.Name; break;
        }
    }

    public void Unequip(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.Weapon: Weapon = Items.Contains("Knight's Sword") ? "Knight's Sword" : "Fists"; break;
            case EquipmentSlot.Body: Armor = Items.Contains("Gambeson") ? "Gambeson" : "None"; break;
            case EquipmentSlot.Shield: Shield = "None"; break;
            case EquipmentSlot.Helm: Helm = "None"; break;
        }
    }
}

public sealed class Army
{
    public Dictionary<UnitType, int> Units { get; init; } = Enum.GetValues<UnitType>().ToDictionary(x => x, _ => 0);
    public int Total => Units.Values.Sum();

    public void RemoveUnits(int count)
    {
        while (count-- > 0 && Total > 0)
        {
            var type = Units.OrderByDescending(x => x.Value).First().Key;
            Units[type]--;
        }
    }
}

public sealed class StrategicArmyDivision
{
    public string Name { get; set; } = "Army";
    public Army Force { get; init; } = new();
    public bool IsFielded { get; set; }
    public int Location { get; set; }
}

public sealed record StrategicArmyOrder(int Origin, int Destination, DateTime Departed, DateTime Arrives);

public sealed class Fief
{
    public string Name { get; init; } = "Home Fief";
    public int Population { get; set; } = 1200;
    public int TaxRate { get; set; } = 10;
    public int Houses { get; set; }
    public bool Church { get; set; }
    public bool Monastery { get; set; }
    public bool Steward { get; set; }
    public bool Beadle { get; set; }
    public bool Priest { get; set; }
    public bool ServantRoom { get; set; }
    public bool Woodward { get; set; }
    public bool Prospector { get; set; }
    public Dictionary<CropType, int> Crops { get; init; } = Enum.GetValues<CropType>().ToDictionary(x => x, _ => 0);
    public Dictionary<ForestIndustry, int> Forest { get; init; } = Enum.GetValues<ForestIndustry>().ToDictionary(x => x, _ => 0);
    public int AvailableSerfs => Math.Max(0, Population - UsedSerfs);
    public int UsedSerfs => Crops.Values.Sum() * 5 + Forest.Sum(x => x.Value * Balance.Forest[x.Key].Serfs);
    public int FoodTiles => Crops.Values.Sum();

    public bool Has(BuildingKind kind) => kind switch
    {
        BuildingKind.House => Houses > 0, BuildingKind.Church => Church, BuildingKind.Monastery => Monastery,
        BuildingKind.Steward => Steward, BuildingKind.Beadle => Beadle, BuildingKind.Priest => Priest,
        BuildingKind.ServantRoom => ServantRoom, BuildingKind.Woodward => Woodward, _ => false
    };

    public void Add(BuildingKind kind)
    {
        switch (kind)
        {
            case BuildingKind.House: Houses++; break; case BuildingKind.Church: Church = true; break;
            case BuildingKind.Monastery: Monastery = true; break; case BuildingKind.Steward: Steward = true; break;
            case BuildingKind.Beadle: Beadle = true; break; case BuildingKind.Priest: Priest = true; break;
            case BuildingKind.ServantRoom: ServantRoom = true; break; case BuildingKind.Woodward: Woodward = true; break;
        }
    }

    public int Productivity(bool julyCheck = false)
    {
        var value = 50 + Balance.Buildings.Values.Where(x => Has(x.Kind)).Sum(x => x.ProductivityBonus);
        if (julyCheck)
        {
            if (!ServantRoom && (Steward || Beadle || Priest)) value -= 10;
            if (FoodTiles > 0 && Crops[CropType.Beans] * 4 < FoodTiles) value -= 15;
            if (Houses * 100 < Population) value -= 15;
        }
        return Math.Clamp(value, 0, 100);
    }
}

public sealed class Player
{
    public const int ArmyDivisionLimit = 5;
    public string Name { get; set; } = "Sir Ronald DeMille";
    public string HeraldicColor { get; set; } = "Green";
    public CharacterStats Stats { get; set; } = new(9, 8, 8, 10, 10);
    public int Wealth { get; set; } = 490;
    public int Age { get; set; } = 18;
    public int Fame { get; set; }
    public int TournamentWinnings { get; set; }
    public int ConquestWinnings { get; set; }
    public int SwordExperience { get; set; }
    public int LanceExperience { get; set; }
    public string? Wife { get; set; }
    public string? LadyColors { get; set; }
    public Dictionary<string, int> CourtshipWins { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public int Debt { get; set; }
    public int Fiefs { get; set; } = 1;
    public int Villages { get; set; } = 1;
    public Inventory Inventory { get; init; } = new();
    public Army Army { get; init; } = new();
    public string PrimaryArmyName { get; set; } = "Army 1";
    public bool PrimaryArmyIsFielded { get; set; }
    public int PrimaryArmyLocation { get; set; }
    public List<StrategicArmyDivision> AdditionalArmies { get; set; } =
        Enumerable.Range(2, ArmyDivisionLimit - 1)
            .Select(number => new StrategicArmyDivision { Name = $"Army {number}" }).ToList();
    public int? JoinedArmyIndex { get; set; } = 0;
    public int ActiveSpies { get; set; }
    public Fief Home { get; init; } = new();

    public void EnsureArmyRoster()
    {
        AdditionalArmies ??= [];
        if (AdditionalArmies.Count > ArmyDivisionLimit - 1)
            throw new InvalidDataException("Campaign contains more than five army divisions.");
        while (AdditionalArmies.Count < ArmyDivisionLimit - 1)
            AdditionalArmies.Add(new StrategicArmyDivision { Name = $"Army {AdditionalArmies.Count + 2}" });
        if (string.IsNullOrWhiteSpace(PrimaryArmyName)) PrimaryArmyName = "Army 1";
        for (var index = 0; index < AdditionalArmies.Count; index++)
            if (string.IsNullOrWhiteSpace(AdditionalArmies[index].Name)) AdditionalArmies[index].Name = $"Army {index + 2}";
        JoinedArmyIndex = JoinedArmyIndex is >= 0 and < ArmyDivisionLimit ? JoinedArmyIndex : null;
    }

    public Army ArmyAt(int index) => index switch
    {
        0 => Army,
        >= 1 and < ArmyDivisionLimit => AdditionalArmies[index - 1].Force,
        _ => throw new ArgumentOutOfRangeException(nameof(index))
    };

    public string ArmyNameAt(int index) => index == 0 ? PrimaryArmyName : AdditionalArmies[index - 1].Name;
    public void SetArmyName(int index, string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        if (name.Length > 16) throw new ArgumentOutOfRangeException(nameof(name));
        if (index == 0) PrimaryArmyName = name;
        else AdditionalArmies[index - 1].Name = name;
    }
    public bool ArmyIsFielded(int index) => index == 0 ? PrimaryArmyIsFielded : AdditionalArmies[index - 1].IsFielded;
    public int ArmyLocationAt(int index) => index == 0 ? PrimaryArmyLocation : AdditionalArmies[index - 1].Location;

    public void SetArmyFieldState(int index, bool fielded, int location)
    {
        if (index == 0) { PrimaryArmyIsFielded = fielded; PrimaryArmyLocation = location; return; }
        var division = AdditionalArmies[index - 1];
        division.IsFielded = fielded;
        division.Location = location;
    }

    public int TotalArmyPopulation => Enumerable.Range(0, ArmyDivisionLimit).Sum(index => ArmyAt(index).Total);
    public int AvailableSerfs => Math.Max(0, Home.AvailableSerfs - TotalArmyPopulation);
}

public sealed class CampaignState
{
    public Player Player { get; init; } = new();
    public DateTime Date { get; set; } = new(1086, 3, 1);
    public VictoryKind Victory { get; set; }
    public int DaySpeed { get; set; } = 1;
    public int CastlesConquered { get; set; }
    public int DragonProgress { get; set; }
    public int CurrentLocation { get; set; }
    public int PreviousLocation { get; set; }
    public int PendingSiegeLocation { get; set; } = -1;
    public int PendingFieldLocation { get; set; } = -1;
    public int PendingFriendlyArmyIndex { get; set; } = -1;
    public Army? PendingEnemyArmy { get; set; }
    public Dictionary<int, StrategicArmyOrder> ArmyOrders { get; init; } = [];
    public int TournamentToken { get; set; } = -1;
    public int JoustsThisTournament { get; set; }
    public bool SkirmishedThisTournament { get; set; }
    public HashSet<int> ConqueredLocations { get; init; } = [];
    public Dictionary<int, int> GarrisonStrength { get; init; } = [];
    public HashSet<int> SpiedLocations { get; init; } = [];
    public int YouthDilemmasAnswered { get; set; }
    public int? ActiveYouthDilemmaNumber { get; set; }
    public List<string> Journal { get; init; } = [];
}
