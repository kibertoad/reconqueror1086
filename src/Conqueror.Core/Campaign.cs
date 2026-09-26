using System.Text.Json;
using System.Text;

namespace Conqueror.Core;

public sealed partial class Campaign
{
    public const int CurrentSaveSchemaVersion = 1;

    public CampaignState State { get; private set; }
    private readonly Random _random;
    private IOriginalStrategicResources? _originalStrategicResources;

    public Campaign(CampaignState? state = null, int seed = 1086)
    {
        State = state ?? NewFromTemplate(1);
        _random = new Random(seed);
        EnsureStrategicState();
    }

    public bool HasOriginalStrategicResources => _originalStrategicResources is not null;

    /// <summary>
    /// Binds verified imported route/grid data at the application boundary.
    /// Schema-1 campaigns retain their dated compatibility model, while any
    /// prepared replacement state is checked against the exact route lengths
    /// before the provider becomes observable by the simulation.
    /// </summary>
    public void ConfigureOriginalStrategicResources(IOriginalStrategicResources resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        if (State.OriginalStrategicState is { } strategic)
        {
            strategic.Validate();
            if (strategic.StartingRouteSelector >= 0)
            {
                var starting = OriginalStrategicMovement.StartingRoutes[strategic.StartingRouteSelector];
                _ = resources.Route(starting.ResourceName, reverse: false);
            }
            foreach (var slot in strategic.MovementSlots.Where(slot =>
                         slot.Active && slot.Mode == OriginalStrategicMovement.RoutedMode))
            {
                var points = resources.Route(slot.RouteResource!, slot.RouteReversed);
                if (slot.WaypointCount != points.Count)
                    throw new InvalidDataException(
                        $"Strategic movement slot {slot.Slot} route length does not match '{slot.RouteResource}'.");
            }
        }
        _originalStrategicResources = resources;
    }

    // RULE-PERSON-003: the pre-generated knights.
    public static CampaignState NewFromTemplate(int index, string heraldicColor = "Green",
        int? originalStrategicCharacterColor = null)
    {
        var template = Balance.Templates[Math.Clamp(index, 0, Balance.Templates.Length - 1)];
        return new CampaignState
        {
            Player = new Player
            {
                Name = "Sir " + template.Name,
                HeraldicColor = heraldicColor,
                OriginalStrategicCharacterColor = originalStrategicCharacterColor
                    ?? OriginalStrategicCharacterColor.ForHeraldicColor(heraldicColor),
                Stats = template.Stats,
                Wealth = template.Wealth
            }
        };
    }

    // PLACEHOLDER: RULE-PERSON-002, RULE-PERSON-003. The original starts from row 0 of CHARACTR.DAT
    // shifted by -8 to 8 and ages every character at dubbing; these 2 to 12 rolls are guesses.
    public static CampaignState NewCustom(string name, int seed, string heraldicColor = "Green",
        int? originalStrategicCharacterColor = null)
    {
        var r = new Random(seed);
        int Roll() => r.Next(2, 13);
        return new CampaignState
        {
            Player = new Player
            {
                Name = name,
                HeraldicColor = heraldicColor,
                OriginalStrategicCharacterColor = originalStrategicCharacterColor
                    ?? OriginalStrategicCharacterColor.ForHeraldicColor(heraldicColor),
                Age = Youth.OriginalPool.FirstAge,
                Stats = new(Roll(), Roll(), Roll(), Roll(), Roll(), Roll()),
                Wealth = Balance.StartingCustomWealth
            }
        };
    }

    // RULE-PERSON-004: dilemma (age - 12) * 5 plus a draw of 0 to 4.
    public int CurrentYouthDilemmaNumber
    {
        get
        {
            var pool = Youth.OriginalPool;
            var age = pool.AgeAtStage(State.YouthDilemmasAnswered);
            if (State.ActiveYouthDilemmaNumber is not { } number || !pool.Contains(number, age))
                State.ActiveYouthDilemmaNumber = number = pool.NumberFor(age, _random.Next(pool.VariantsPerAge));
            return number;
        }
    }

    public bool AnswerDilemma(int choice)
    {
        if (State.YouthDilemmasAnswered >= Youth.Dilemmas.Length || choice is < 0 or > 2) return false;
        var answer = Youth.Dilemmas[State.YouthDilemmasAnswered].Choices[choice];
        var s = State.Player.Stats;
        var d = answer.Delta;
        State.Player.Stats = new CharacterStats(s.Strength + d.Strength, s.Dexterity + d.Dexterity, s.Piety + d.Piety, s.Stamina + d.Stamina, s.Honor + d.Honor, s.Intelligence).Clamp();
        State.Player.Wealth += answer.Wealth;
        if (answer.Item is not null) State.Player.Inventory.Items.Add(answer.Item);
        State.Player.Age++;
        CompleteYouthDilemma();
        Log($"Youth: {answer.Text}.");
        return true;
    }

    public YouthDilemmaResult? AnswerDilemma(YouthDilemmaDefinition dilemma, int choice)
    {
        if (State.YouthDilemmasAnswered >= Youth.OriginalPool.StageCount
            || choice < 0 || choice >= dilemma.Choices.Count
            || dilemma.Number != CurrentYouthDilemmaNumber
            || dilemma.Age != Youth.OriginalPool.AgeAtStage(State.YouthDilemmasAnswered)) return null;

        var selected = dilemma.Choices[choice];
        var outcome = YouthDilemmaRules.Resolve(selected, CharacterAttributes.Read(State.Player, selected.ScoringAttribute));
        if (!selected.Outcomes.TryGetValue(outcome, out var resolved)) return null;
        CharacterAttributes.Apply(State.Player, resolved.Changes);
        var result = new YouthDilemmaResult(dilemma.Number, choice + 1, outcome, resolved.Text, resolved.Changes);
        CompleteYouthDilemma();
        Log($"Youth dilemma {dilemma.Number}: choice {choice + 1}, {outcome}.");
        return result;
    }

    private void CompleteYouthDilemma()
    {
        State.YouthDilemmasAnswered++;
        State.ActiveYouthDilemmaNumber = null;
    }

    public int TravelTo(int location)
    {
        if (!CanTravelTo(location)) return 0;
        var origin = State.CurrentLocation;
        var accompanyingArmyIndex = JoinedArmyIndexAt(origin);
        var days = World.TravelDays(State.CurrentLocation, location);
        AdvanceDays(days);
        State.PreviousLocation = origin;
        State.CurrentLocation = location;
        if (days > 0 && accompanyingArmyIndex is { } armyIndex)
            State.Player.SetArmyFieldState(armyIndex, true, location);
        Log($"Arrived at {World.Locations[location].Name} after {days} days.");
        if (days > 0 && accompanyingArmyIndex is { } friendlyArmyIndex && IsHostileStronghold(location) && GarrisonAt(location) > 0
            && _random.Next(100) < Balance.Strategy.InterceptionPercent)
        {
            State.PendingFieldLocation = location;
            State.PendingFriendlyArmyIndex = friendlyArmyIndex;
            State.PendingEnemyArmy = CreateArmy(GarrisonAt(location));
            Log($"The garrison of {World.Locations[location].Name} intercepts your army.");
        }
        return days;
    }

    public bool DragonLairDiscovered => State.DragonProgress > 0
        || State.ConversationVariables.Count > OriginalCampaignVariables.DragonLairDiscovery
        && State.ConversationVariables[OriginalCampaignVariables.DragonLairDiscovery] != 0;

    public bool CanRevealLocation(int location) => location >= 0 && location < World.Locations.Length
        && (World.Locations[location].Kind != LocationKind.DragonLair || DragonLairDiscovered);

    public bool CanTravelTo(int location) => CanRevealLocation(location) && State.PendingEnemyArmy is null;

    public bool HasPendingFieldBattle => State.PendingEnemyArmy is not null;

    public int JoinedArmyTotalHere => JoinedArmyIndexAt(State.CurrentLocation) is { } index
        ? State.Player.ArmyAt(index).Total
        : 0;

    public bool CanStartFieldBattle => HasPendingFieldBattle
        || (IsHostileStronghold(State.CurrentLocation) && GarrisonAt(State.CurrentLocation) > 0);

    public int GarrisonAt(int location) => State.GarrisonStrength.GetValueOrDefault(location);

    public bool HasGarrisonIntel(int location) => location == 0 || State.ConqueredLocations.Contains(location) || State.SpiedLocations.Contains(location);

    public bool SendSpy(int location)
    {
        if (!IsHostileStronghold(location) || State.SpiedLocations.Contains(location)
            || !Spend(Balance.Strategy.SpyCost, $"spy sent to {World.Locations[location].Name}")) return false;
        State.SpiedLocations.Add(location);
        Log($"The spy reports {GarrisonAt(location)} soldiers guarding {World.Locations[location].Name}.");
        return true;
    }

    // PLACEHOLDER: RULE-ESTATE-001. The original also charges the price and takes 100 from the
    // population, refunds as BUG-ESTATE-001 describes, and has no 60-company limit.
    public bool AdjustArmyCompany(int armyIndex, UnitType type, int companies)
    {
        if (armyIndex is < 0 or >= Player.ArmyDivisionLimit || companies is not (-1 or 1)) return false;
        var player = State.Player;
        player.EnsureArmyRoster();
        if (player.ArmyLocationAt(armyIndex) != 0) return false;
        var army = player.ArmyAt(armyIndex);
        var amount = checked(companies * WarPlanningCheckpoint.CompanySize);
        if (amount > 0 && (player.AvailableSerfs < amount
            || army.Total + amount > WarPlanningCheckpoint.MaximumCompaniesPerArmy * WarPlanningCheckpoint.CompanySize)) return false;
        if (amount < 0 && army.Units[type] < -amount) return false;
        army.Units[type] += amount;
        Log($"Army {armyIndex + 1}: {(amount > 0 ? "raised" : "disbanded")} {Math.Abs(amount)} {type}.");
        return true;
    }

    public bool FieldArmy(int armyIndex)
    {
        if (armyIndex is < 0 or >= Player.ArmyDivisionLimit || State.CurrentLocation != 0) return false;
        var player = State.Player;
        player.EnsureArmyRoster();
        if (player.ArmyAt(armyIndex).Total == 0 || player.ArmyLocationAt(armyIndex) != 0) return false;
        if (State.OriginalStrategicState is { } strategic
            && (strategic.PlayerMovementSlots[armyIndex].Active
                || !OriginalStrategicMovement.ConstructPlayerMovementRecord(strategic, armyIndex))) return false;
        player.SetArmyFieldState(armyIndex, true, 0);
        Log($"{player.ArmyNameAt(armyIndex)} is fielded.");
        return true;
    }

    public bool ToggleArmyMembership(int armyIndex)
    {
        if (armyIndex is < 0 or >= Player.ArmyDivisionLimit) return false;
        var player = State.Player;
        player.EnsureArmyRoster();
        if (player.ArmyAt(armyIndex).Total == 0 || !player.ArmyIsFielded(armyIndex)
            || player.ArmyLocationAt(armyIndex) != State.CurrentLocation
            || State.ArmyOrders.ContainsKey(armyIndex)) return false;
        var leaving = player.JoinedArmyIndex == armyIndex;
        if (State.OriginalStrategicState is { } strategic)
        {
            if (leaving)
            {
                if (!OriginalStrategicMovement.LeavePlayerArmy(strategic, armyIndex)) return false;
            }
            else
                OriginalStrategicMovement.JoinPlayerArmy(strategic, armyIndex);
        }
        player.JoinedArmyIndex = leaving ? null : armyIndex;
        Log(player.JoinedArmyIndex is null ? "You leave the selected army." : $"You join {player.ArmyNameAt(armyIndex)}.");
        return true;
    }

    public bool AssignSpy()
    {
        if (State.Player.ActiveSpies != 0 || !Spend(Balance.Strategy.SpyCost, "assign spy")) return false;
        State.Player.ActiveSpies = 1;
        Log("A spy is sent to observe troop movements across England.");
        return true;
    }

    public StrategicArmyOrder? ArmyOrderAt(int armyIndex) =>
        State.ArmyOrders.GetValueOrDefault(armyIndex);

    public bool DispatchArmy(int armyIndex, int destination)
    {
        if (armyIndex is < 0 or >= Player.ArmyDivisionLimit || !CanRevealLocation(destination)) return false;
        var player = State.Player;
        player.EnsureArmyRoster();
        var origin = player.ArmyLocationAt(armyIndex);
        if (player.JoinedArmyIndex == armyIndex || !player.ArmyIsFielded(armyIndex)
            || player.ArmyAt(armyIndex).Total == 0 || origin == destination
            || State.ArmyOrders.ContainsKey(armyIndex)) return false;
        var days = World.TravelDays(origin, destination);
        State.ArmyOrders[armyIndex] = new StrategicArmyOrder(origin, destination, State.Date, State.Date.AddDays(days));
        Log($"{player.ArmyNameAt(armyIndex)} marches for {World.Locations[destination].Name} under its captain; arrival in {days} days.");
        return true;
    }

    public bool StartSiege(int location)
    {
        var friendlyArmyIndex = JoinedArmyIndexAt(location);
        if (location <= 0 || location >= World.Locations.Length || location != State.CurrentLocation || friendlyArmyIndex is null
            || State.ConqueredLocations.Contains(location) || State.PendingEnemyArmy is not null) return false;
        var target = World.Locations[location];
        if (target.Kind is not (LocationKind.Castle or LocationKind.London)) return false;
        State.PendingSiegeLocation = location;
        State.PendingFriendlyArmyIndex = friendlyArmyIndex.Value;
        return true;
    }

    public bool IsTournamentHere => State.CurrentLocation == World.TournamentIndex(State.Date);
    private int CurrentTournamentToken => State.Date.Year * 12 + State.Date.Month;

    private void RefreshTournament()
    {
        if (State.TournamentToken == CurrentTournamentToken) return;
        State.TournamentToken = CurrentTournamentToken;
        State.JoustsThisTournament = 0;
        State.SkirmishedThisTournament = false;
    }

    public bool Spend(int amount, string reason)
    {
        if (amount < 0 || State.Player.Wealth < amount) return false;
        State.Player.Wealth -= amount;
        Log($"Spent {amount}s: {reason}.");
        return true;
    }

    // PLACEHOLDER: RULE-ESTATE-003. The debt of the loan plus half matches; the lower bound of 20 is not applied.
    public bool Borrow(int amount)
    {
        if (amount <= 0 || amount > Balance.MaxLoan || State.Player.Debt != 0
            || State.DrogoDefeated || State.PendingDrogoEncounter) return false;
        State.Player.Wealth += amount;
        State.Player.Debt = amount + (int)(amount * Balance.LoanInterest);
        Log($"Borrowed {amount}s; {State.Player.Debt}s due at harvest.");
        return true;
    }

    public bool Donate(int amount = 15)
    {
        if (!Spend(amount, "church donation")) return false;
        State.Player.Stats = State.Player.Stats with { Piety = Math.Min(20, State.Player.Stats.Piety + 1) };
        return true;
    }

    // RULE-ESTATE-005: crops are planted in March.
    public bool Plant(CropType crop)
    {
        var f = State.Player.Home;
        var rule = Balance.Crops[crop];
        if (State.Date.Month != 3 || f.AvailableSerfs < rule.Serfs || !Spend(rule.Cost, $"plant {crop}")) return false;
        f.Crops[crop]++;
        return true;
    }

    public bool DevelopForest(ForestIndustry industry)
    {
        var f = State.Player.Home;
        var rule = Balance.Forest[industry];
        if (f.AvailableSerfs < rule.Serfs || !Spend(rule.Cost, industry.ToString())) return false;
        f.Forest[industry]++;
        return true;
    }

    public bool Recruit(UnitType type)
    {
        var f = State.Player.Home;
        var price = Balance.ScaleFrom50(Balance.Units[type].PriceAt50, Balance.Units[type].PriceAt100, f.Productivity());
        if (!Spend(price, $"recruit {type}")) return false;
        State.Player.Army.Units[type]++;
        return true;
    }

    public bool BuyEquipment(string name)
    {
        var item = Balance.Equipment.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (item is null || !item.Shop || item.BuyPrice <= 0 || State.Player.Inventory.Items.Contains(item.Name) || !Spend(item.BuyPrice, item.Name)) return false;
        var inventory = State.Player.Inventory;
        inventory.Items.Add(item.Name);
        inventory.Equip(item);
        return true;
    }

    // PLACEHOLDER: RULE-UI-004. The original pays price - price / 4 and clears every copy held.
    public bool SellEquipment(string name)
    {
        var item = Balance.Equipment.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        var inventory = State.Player.Inventory;
        if (item is null || item.BuyPrice <= 0 || !inventory.Items.Remove(item.Name)) return false;
        State.Player.Wealth += item.BuyPrice * 3 / 4;
        if (inventory.Weapon.Equals(item.Name, StringComparison.OrdinalIgnoreCase) || inventory.Armor.Equals(item.Name, StringComparison.OrdinalIgnoreCase)
            || inventory.Shield.Equals(item.Name, StringComparison.OrdinalIgnoreCase) || inventory.Helm.Equals(item.Name, StringComparison.OrdinalIgnoreCase)) inventory.Unequip(item.Slot);
        Log($"Sold {item.Name} for {item.BuyPrice * 3 / 4}s.");
        return true;
    }

    public int Retreat()
    {
        var armyIndex = ActiveArmyIndex();
        var army = State.Player.ArmyAt(armyIndex);
        var losses = army.Total == 0 ? 0 : Math.Clamp((int)Math.Ceiling(army.Total * (.50 + _random.NextDouble() * .25)), 1, army.Total);
        army.RemoveUnits(losses);
        State.Player.SetArmyFieldState(armyIndex, army.Total > 0, State.PreviousLocation);
        Log($"Retreat cost {losses} soldiers.");
        return losses;
    }

    public bool Build(string building)
    {
        var definition = Balance.Buildings.Values.FirstOrDefault(x => x.Name.Equals(building, StringComparison.OrdinalIgnoreCase));
        return definition is not null && Build(definition.Kind);
    }

    // PLACEHOLDER: RULE-ESTATE-004. Construction costs are guesses.
    public bool Build(BuildingKind kind)
    {
        if (!Balance.Buildings.TryGetValue(kind, out var definition)) return false;
        var fief = State.Player.Home;
        if ((!definition.Repeatable && fief.Has(kind)) || !Spend(definition.Cost, definition.Name)) return false;
        fief.Add(kind);
        return true;
    }

    public void AdvanceDays(int days)
    {
        for (var i = 0; i < days && State.Victory == VictoryKind.None; i++)
        {
            var previousMonth = State.Date.Month;
            var previousYear = State.Date.Year;
            State.Date = State.Date.AddDays(1);
            ResolveSpyReportFromEnemyMovement();
            ResolveArmyOrders();
            ResolveEnemyMovements();
            TryStartEnemyMovement();
            if (State.Date.Month != previousMonth)
            {
                if (State.OriginalStrategicState is { } strategic)
                    strategic.TerrainProfile = OriginalStrategicMovement.TerrainProfileForMonth(State.Date.Month - 1);
                SettleMonth();
            }
            if (State.Date.Year != previousYear)
            {
                // PLACEHOLDER: RULE-PERSON-006. What advances AGE after dubbing in the original is not known.
                State.Player.Age++;
                if (State.Player.Age >= Balance.FinalAge)
                {
                    State.Victory = VictoryKind.Defeat;
                    State.EndReason = CampaignEndReason.AgeLimit;
                    Log("Your thirtieth birthday arrives before your destiny is fulfilled.");
                }
            }
        }
    }

    /// <summary>
    /// Changes the live source-shaped movement rate and keeps the dated map
    /// adapter's temporary speed control in step until its replacement.
    /// </summary>
    public int AdjustStrategicSpeed(int delta)
    {
        var strategic = State.OriginalStrategicState;
        var current = strategic?.SpeedMultiplier ?? State.DaySpeed;
        var next = (int)Math.Clamp((long)current + delta,
            OriginalStrategicMovement.MinimumSpeedMultiplier,
            OriginalStrategicMovement.MaximumSpeedMultiplier);
        if (strategic is not null) strategic.SpeedMultiplier = next;
        State.DaySpeed = next;
        return next;
    }

    // PLACEHOLDER: RULE-ESTATE-002. Revenue (RULE-ESTATE-005), growth (RULE-ESTATE-006) and
    // productivity follow SRC-GAMEFAQS-66730; the upkeep of every company in every army matches.
    public void SettleMonth()
    {
        var p = State.Player;
        var f = p.Home;
        var july = State.Date.Month == 7;
        var productivity = f.Productivity(july);
        var revenue = f.Crops.Sum(x => x.Value * (july ? Balance.Crops[x.Key].HarvestRevenueAt50 : Balance.Crops[x.Key].NormalRevenueAt50));
        revenue += f.Forest.Sum(x => x.Value * Balance.Forest[x.Key].RevenueAt50);
        revenue = (int)Math.Round(revenue * productivity / 50d);
        revenue += (int)Math.Round(f.Population * f.TaxRate / 1200d);
        p.EnsureArmyRoster();
        var upkeep = Enumerable.Range(0, Player.ArmyDivisionLimit).Sum(index => p.ArmyAt(index).Units.Sum(x =>
            x.Value * Balance.ScaleFrom50(Balance.Units[x.Key].UpkeepAt50, Balance.Units[x.Key].UpkeepAt100, productivity)));
        p.Wealth += revenue - upkeep;

        var capacity = Math.Min(f.Houses, f.FoodTiles) * 100;
        var ratio = f.Population == 0 ? 0 : Math.Clamp(capacity / (double)Math.Ceiling(f.Population * 1.07 / 100d) / 100d, 0, 1);
        var growthRate = Balance.PopulationGrowth.First(x => ratio >= x.MinimumCapacity).MonthlyRate;
        growthRate *= productivity / 50d;
        if (f.TaxRate > 10) growthRate *= .93;
        f.Population = Math.Max(0, (int)Math.Round(f.Population * (1 + growthRate)));

        // Calendar pass 0x2B060 tests its zero-based month value against 6,
        // then calls debt branch 0x1074C when the outstanding-debt accessor is
        // nonzero. Its refusal paths directly load MONEY.RES at object-2 +0xA4.
        if (july && p.Debt > 0)
        {
            State.PendingDrogoEncounter = true;
            Log($"Drogo, the moneylender's thug, comes to collect the {p.Debt}s debt.");
        }
        Log($"Month settled: +{revenue}s revenue, -{upkeep}s upkeep, {productivity}% productivity.");
    }

    public bool PayDrogo()
    {
        var player = State.Player;
        if (!State.PendingDrogoEncounter || player.Debt <= 0 || player.Wealth < player.Debt) return false;
        var payment = player.Debt;
        player.Wealth -= payment;
        player.Debt = 0;
        State.PendingDrogoEncounter = false;
        Log($"Paid Drogo the {payment}s owed to the moneylender.");
        return true;
    }

    /// <summary>
    /// Constructs the debt encounter in its required imported original scene.
    /// Scene selection belongs at the presentation/import boundary; its
    /// outcome and no-retainer contract remain campaign state.
    /// </summary>
    public SiegeSession CreateDrogoBattle(SiegeLayout layout)
    {
        ArgumentNullException.ThrowIfNull(layout);
        if (!State.PendingDrogoEncounter || State.Player.Debt <= 0)
            throw new InvalidOperationException("Drogo is not waiting to collect a debt.");
        // PLACEHOLDER: RULE-ASSAULT-001. This fight uses a retainer cap of 0, where the rule gives at least 1.
        return new SiegeSession(State.Player, new Army(), garrison: 0,
            State.Date.DayOfYear + State.Date.Year, layout, includeRetainers: false);
    }

    public bool FinishDrogoBattle(SiegeSession battle)
    {
        ArgumentNullException.ThrowIfNull(battle);
        if (!State.PendingDrogoEncounter) return false;
        if (battle.Won)
        {
            State.DrogoDefeated = true;
            State.PendingDrogoEncounter = false;
            // The original keeps the debt after a refused payment (DEV-ESTATE-001).
            State.Player.Debt = 0;
            Log("You killed Drogo. The moneylender will not bother you again.");
            return true;
        }
        if (!battle.Defeated) return false;
        State.Victory = VictoryKind.Defeat;
        State.EndReason = CampaignEndReason.Drogo;
        State.PendingDrogoEncounter = false;
        Log("Drogo killed you while collecting the moneylender's debt.");
        return false;
    }

    public BattleResult FightFieldBattle(Army enemy)
    {
        var result = Combat.Resolve(State.Player.ArmyAt(ActiveArmyIndex()), enemy, _random);
        State.Player.SwordExperience += result.Won ? 2 : 1;
        if (result.Won) State.Player.Fame++;
        Log(result.Summary);
        return result;
    }

    public FieldBattleSession CreateFieldBattle()
    {
        var location = State.PendingFieldLocation >= 0 ? State.PendingFieldLocation : State.CurrentLocation;
        var strength = GarrisonAt(location);
        if (State.PendingEnemyArmy is null && (!IsHostileStronghold(location) || strength <= 0))
            throw new InvalidOperationException("There is no hostile field army here.");
        var enemy = State.PendingEnemyArmy ?? CreateArmy(Math.Max(Balance.Strategy.MinimumFieldArmy, strength));
        var friendlyArmyIndex = State.PendingFriendlyArmyIndex >= 0
            ? State.PendingFriendlyArmyIndex
            : JoinedArmyIndexAt(location) ?? throw new InvalidOperationException("No joined army is present for battle.");
        State.PendingFieldLocation = location;
        State.PendingFriendlyArmyIndex = friendlyArmyIndex;
        State.PendingEnemyArmy = enemy;
        return new FieldBattleSession(State.Player.ArmyAt(friendlyArmyIndex), enemy, State.Date.DayOfYear + State.CurrentLocation * 37);
    }

    public FieldBattleOutcome FinishFieldBattle(FieldBattleSession battle)
    {
        var friendlyArmyIndex = ActiveArmyIndex();
        var friendlyArmy = State.Player.ArmyAt(friendlyArmyIndex);
        var survivors = battle.FriendlySurvivors();
        foreach (var type in Enum.GetValues<UnitType>()) friendlyArmy.Units[type] = survivors.Units[type];
        var enemySurvivors = battle.EnemySurvivors();
        if (State.PendingFieldLocation >= 0) State.GarrisonStrength[State.PendingFieldLocation] = enemySurvivors.Total;
        State.Player.SwordExperience += battle.Outcome == FieldBattleOutcome.Victory ? 2 : 1;
        if (battle.Outcome == FieldBattleOutcome.Victory) { State.Player.Fame++; Log("Your army holds the field."); }
        else
        {
            if (battle.Outcome == FieldBattleOutcome.Withdrawn) Retreat();
            else Log("Your army is defeated in the field.");
            State.CurrentLocation = State.PreviousLocation;
            State.Player.SetArmyFieldState(friendlyArmyIndex, friendlyArmy.Total > 0, State.CurrentLocation);
            Log($"Your survivors fall back to {World.Locations[State.CurrentLocation].Name}.");
        }
        if (battle.Outcome == FieldBattleOutcome.Victory)
            State.Player.SetArmyFieldState(friendlyArmyIndex, friendlyArmy.Total > 0, State.CurrentLocation);
        State.PendingFieldLocation = -1;
        State.PendingFriendlyArmyIndex = -1;
        State.PendingEnemyArmy = null;
        return battle.Outcome;
    }

    public void WinSiege()
    {
        if (State.PendingSiegeLocation >= 0)
        {
            State.ConqueredLocations.Add(State.PendingSiegeLocation);
            State.GarrisonStrength[State.PendingSiegeLocation] = 0;
        }
        var captured = State.PendingSiegeLocation >= 0 ? World.Locations[State.PendingSiegeLocation] : null;
        State.PendingSiegeLocation = -1;
        State.PendingFriendlyArmyIndex = -1;
        State.CastlesConquered++;
        State.Player.Fiefs++;
        State.Player.Villages += captured?.Villages ?? _random.Next(1, 4);
        State.Player.Fame += 2;
        State.Player.SwordExperience += 5;
        State.Player.Stats = State.Player.Stats with { Strength = Math.Min(20, State.Player.Stats.Strength + 1) };
        var spoils = _random.Next(80, 221);
        State.Player.Wealth += spoils;
        State.Player.ConquestWinnings += spoils;
        Log($"Castle taken. Spoils: {spoils}s.");
    }

    private bool IsHostileStronghold(int location)
    {
        if (location <= 0 || location >= World.Locations.Length || State.ConqueredLocations.Contains(location)) return false;
        return World.Locations[location].Kind is LocationKind.Castle or LocationKind.London;
    }

    private static Army CreateArmy(int strength)
    {
        var enemy = new Army();
        enemy.Units[UnitType.Swordsmen] = strength / 3;
        enemy.Units[UnitType.Halberdiers] = strength / 3;
        enemy.Units[UnitType.Knights] = strength - enemy.Units[UnitType.Swordsmen] - enemy.Units[UnitType.Halberdiers];
        return enemy;
    }

    private void EnsureStrategicState()
    {
        State.Player.EnsureArmyRoster();
        if (State.CurrentLocation >= 0 && State.CurrentLocation < World.Locations.Length
            && World.Locations[State.CurrentLocation].Kind == LocationKind.DragonLair)
            State.DragonProgress = Math.Max(1, State.DragonProgress);
        foreach (var (location, index) in World.Locations.Select((location, index) => (location, index)))
            if (location.Kind is LocationKind.Castle or LocationKind.London)
                State.GarrisonStrength.TryAdd(index, State.ConqueredLocations.Contains(index) ? 0 : location.Garrison);
        if (State.PendingFieldLocation < 0) State.PendingEnemyArmy = null;
        if (State.PendingFieldLocation < 0 && State.PendingSiegeLocation < 0) State.PendingFriendlyArmyIndex = -1;
        if (State.PendingFriendlyArmyIndex >= Player.ArmyDivisionLimit) State.PendingFriendlyArmyIndex = -1;
        foreach (var (armyIndex, order) in State.ArmyOrders)
            if (armyIndex is < 0 or >= Player.ArmyDivisionLimit || order.Origin < 0
                || order.Origin >= World.Locations.Length || order.Destination < 0
                || order.Destination >= World.Locations.Length || order.Arrives <= order.Departed
                || State.Player.ArmyLocationAt(armyIndex) != order.Origin)
                throw new InvalidDataException("Campaign contains an invalid army movement order.");
        if (State.EnemyMovements is null || State.EnemyMovements.Count > OriginalEnemyMovementSlotCount
            || State.EnemyMovements.Select(movement => movement.Slot).Distinct().Count() != State.EnemyMovements.Count)
            throw new InvalidDataException("Campaign contains an invalid enemy movement roster.");
        foreach (var movement in State.EnemyMovements)
            if (movement.Slot is < 0 or >= OriginalEnemyMovementSlotCount
                || movement.Origin <= 0 || movement.Origin >= World.Locations.Length
                || movement.Destination <= 0 || movement.Destination >= World.Locations.Length
                || movement.Origin == movement.Destination || movement.Arrives <= movement.Departed
                || movement.Swordsmen < 0 || movement.Halberdiers < 0 || movement.Knights < 0
                || movement.Total <= 0)
                throw new InvalidDataException("Campaign contains an invalid enemy movement record.");
    }

    private void ResolveArmyOrders()
    {
        foreach (var (armyIndex, order) in State.ArmyOrders.OrderBy(pair => pair.Key).ToArray())
        {
            if (order.Arrives > State.Date) continue;
            State.ArmyOrders.Remove(armyIndex);
            var player = State.Player;
            var army = player.ArmyAt(armyIndex);
            player.SetArmyFieldState(armyIndex, army.Total > 0, order.Destination);
            Log($"{player.ArmyNameAt(armyIndex)} arrives at {World.Locations[order.Destination].Name}.");
            if (army.Total == 0 || !IsHostileStronghold(order.Destination) || GarrisonAt(order.Destination) == 0
                || _random.Next(100) >= Balance.Strategy.InterceptionPercent) continue;

            var enemy = CreateArmy(GarrisonAt(order.Destination));
            var result = Combat.Resolve(army, enemy, _random);
            State.GarrisonStrength[order.Destination] = enemy.Total;
            if (!result.Won)
                player.SetArmyFieldState(armyIndex, army.Total > 0, order.Origin);
            else if (army.Total == 0)
                player.SetArmyFieldState(armyIndex, false, order.Destination);
            Log($"Captain's report from {player.ArmyNameAt(armyIndex)}: {result.Summary}" +
                (result.Won ? " The division holds its destination." : $" The survivors return to {World.Locations[order.Origin].Name}."));
        }
    }

    private int? JoinedArmyIndexAt(int location)
    {
        var player = State.Player;
        player.EnsureArmyRoster();
        if (player.JoinedArmyIndex is not { } index || player.ArmyAt(index).Total == 0
            || player.ArmyLocationAt(index) != location) return null;
        return index;
    }

    private int ActiveArmyIndex() => State.PendingFriendlyArmyIndex is >= 0 and < Player.ArmyDivisionLimit
        ? State.PendingFriendlyArmyIndex
        : State.Player.JoinedArmyIndex is int joined and >= 0 and < Player.ArmyDivisionLimit ? joined : 0;

    public SiegeSession CreateSiege(SiegeLayout? layout = null)
    {
        if (State.PendingSiegeLocation < 0) throw new InvalidOperationException("No siege has been started.");
        var target = World.Locations[State.PendingSiegeLocation];
        return new SiegeSession(State.Player, State.Player.ArmyAt(ActiveArmyIndex()), target.Garrison,
            State.Date.DayOfYear + State.PendingSiegeLocation * 1086, layout);
    }

    public bool FinishSiege(SiegeSession siege)
    {
        var location = State.PendingSiegeLocation;
        var armyIndex = ActiveArmyIndex();
        var army = State.Player.ArmyAt(armyIndex);
        OriginalRetainerCombat.ApplyCampaignLosses(army, siege.RetainerLosses);
        if (!siege.Won)
        {
            Log(siege.Defeated ? "You are carried unconscious from the keep." : "The assault is abandoned.");
            State.PendingSiegeLocation = -1;
            State.PendingFriendlyArmyIndex = -1;
            return false;
        }
        WinSiege();
        // PLACEHOLDER: RULE-DRAGON-002. The original crowns the player when the besieged cell holds
        // person 100; this tests the location index of the crown victory.
        if (location == Balance.Victories[VictoryKind.Crown].LocationIndex)
        {
            State.Victory = VictoryKind.Crown;
            Log("London falls. William is overthrown and you take the crown of England.");
        }
        return true;
    }

    // PLACEHOLDER: RULE-TOURNEY-003. The limit of three matches; the wager, win test and rewards are guesses.
    public bool Joust(int accuracy, int opponentIndex = 2)
    {
        RefreshTournament();
        if (!IsTournamentHere) { Log("There is no tournament here this month."); return false; }
        if (State.JoustsThisTournament >= 3) { Log("You have already ridden three jousts this tournament."); return false; }
        var opponent = Balance.TournamentOpponents[Math.Clamp(opponentIndex, 0, Balance.TournamentOpponents.Length - 1)];
        if (!Spend(opponent.Wager, $"joust wager against {opponent.Name}")) return false;
        State.JoustsThisTournament++;
        var won = accuracy <= opponent.JoustTolerance;
        if (won)
        {
            State.Player.LanceExperience = Math.Min(20, State.Player.LanceExperience + 1);
            State.Player.Stats = State.Player.Stats with
            {
                Dexterity = Math.Min(20, State.Player.Stats.Dexterity + (State.Player.LanceExperience % 4 == 0 ? 1 : 0)),
                Honor = Math.Min(20, State.Player.Stats.Honor + 1)
            };
            State.Player.Wealth += opponent.Wager * 2;
            State.Player.TournamentWinnings += opponent.Wager;
            if (State.Player.LadyColors is { } lady)
            {
                // With ALL.VTB installed, the selected lady and the joust
                // result are carried by original global slots 42 and 3.
                // Her conversation action tree then decides whether a reward
                // is earned and applies its exact item/attribute mutations.
                // Do not duplicate that work through the prototype ladder.
                if (State.ConversationVariables.Count > OriginalCampaignVariables.LadyColors)
                    Log($"Joust won wearing {lady}'s colors; return to her conversation for the result.");
                else
                    RewardCourtship(lady);
            }
            Log($"Joust won against {opponent.Name}: {opponent.Wager}s profit and honor gained.");
        }
        else Log($"Unhorsed by {opponent.Name}; the {opponent.Wager}s wager is lost.");
        return won;
    }

    // PLACEHOLDER: RULE-TOURNEY-004. The limit of one matches; the wager, battle and settlement are guesses.
    public BattleResult? TournamentSkirmish(int opponentIndex = 2)
    {
        RefreshTournament();
        if (!IsTournamentHere || State.SkirmishedThisTournament) { Log("No further skirmish is available at this tournament."); return null; }
        var opponent = Balance.TournamentOpponents[Math.Clamp(opponentIndex, 0, Balance.TournamentOpponents.Length - 1)];
        if (!Spend(opponent.Wager, $"skirmish wager against {opponent.Name}")) return null;
        State.SkirmishedThisTournament = true;
        var friendly = new Army(); var enemy = new Army();
        friendly.Units[UnitType.Swordsmen] = 3; friendly.Units[UnitType.Halberdiers] = 3; friendly.Units[UnitType.Knights] = 2;
        enemy.Units[UnitType.Swordsmen] = opponent.Swordsmen;
        enemy.Units[UnitType.Halberdiers] = opponent.Halberdiers;
        enemy.Units[UnitType.Knights] = opponent.Knights;
        var result = Combat.Resolve(friendly, enemy, _random);
        State.Player.SwordExperience++;
        if (result.Won)
        {
            State.Player.Wealth += opponent.Wager * 2;
            State.Player.TournamentWinnings += opponent.Wager;
        }
        Log($"Tournament skirmish against {opponent.Name} {(result.Won ? $"won for {opponent.Wager}s profit" : $"lost with a {opponent.Wager}s wager")}.");
        return result;
    }

    public bool AttemptCrown()
    {
        return AttemptVictory(VictoryKind.Crown);
    }

    public DragonBattleSession? BeginDragonBattle()
    {
        return DragonLairDiscovered && State.CurrentLocation == World.Locations.Length - 1
            ? CreateDragonBattle()
            : null;
    }

    public DragonBattleSession? BeginDragonBattleFromStrategicMap()
    {
        if (State.OriginalStrategicState is not { } strategic) return null;
        var distinguished = strategic.PlayerMovementSlots.Single(slot =>
            slot.Slot == strategic.EngagedPlayerMovementSlot);
        return distinguished.Active
            && OriginalStrategicMovement.IsDragonEntryCell(distinguished.GridX, distinguished.GridY)
            ? CreateDragonBattle()
            : null;
    }

    private DragonBattleSession CreateDragonBattle() =>
        new(State.Player.LanceExperience,
            OriginalDragonRunScore.EquipmentBonus(State.Player.Inventory.Items));

    // PLACEHOLDER: RULE-DRAGON-001. A win does not add 2 to the lance experience, and the
    // withdrawn outcome has no counterpart in the original.
    public bool FinishDragonBattle(DragonBattleSession battle)
    {
        ArgumentNullException.ThrowIfNull(battle);
        switch (battle.Outcome)
        {
            case DragonBattleOutcome.Victory:
                State.Victory = VictoryKind.Dragon;
                Log("The dragon falls. England hails its champion.");
                return true;
            case DragonBattleOutcome.Defeat:
                State.Victory = VictoryKind.Defeat;
                State.EndReason = CampaignEndReason.Dragon;
                Log("The dragon ends your quest for England's championship.");
                return false;
            case DragonBattleOutcome.Withdrawn:
                Log("You escape the dragon and may prepare for another challenge.");
                return false;
            default:
                throw new InvalidOperationException("The dragon battle is still in progress.");
        }
    }

    // PLACEHOLDER: RULE-DRAGON-002. The original has no requirement check; its crown comes only from a won siege.
    public bool AttemptVictory(VictoryKind kind)
    {
        if (kind != VictoryKind.Crown) return false;
        if (!MeetsVictoryRequirements(kind, logFailure: true)) return false;
        State.Victory = kind;
        Log("William is overthrown. You take the crown of England.");
        return true;
    }

    private bool MeetsVictoryRequirements(VictoryKind kind, bool logFailure)
    {
        if (!Balance.Victories.TryGetValue(kind, out var definition)) return false;
        var p = State.Player;
        var missingItems = definition.RequiredItems.Where(x => !p.Inventory.Items.Contains(x)).ToArray();
        if (State.CurrentLocation != definition.LocationIndex || p.Fiefs < definition.RequiredFiefs || p.Army.Total < definition.RequiredArmy
            || p.Stats.Strength < definition.RequiredStrength || missingItems.Length > 0)
        {
            if (logFailure)
                Log($"Requirements not met for {kind}: travel to {World.Locations[definition.LocationIndex].Name}, fiefs {definition.RequiredFiefs}, army {definition.RequiredArmy}, strength {definition.RequiredStrength}, items {string.Join(", ", definition.RequiredItems)}.");
            return false;
        }
        return true;
    }

    // PLACEHOLDER: RULE-PERSON-005. The original runs courtship through the lady conversations.
    public bool RequestColors(string lady)
    {
        if (!IsTournamentHere) { Log("Courtship takes place at the tournament stands."); return false; }
        var definition = Balance.Courtships.FirstOrDefault(x => x.Name.Equals(lady, StringComparison.OrdinalIgnoreCase));
        if (definition is null || !definition.CourtAble) { Log($"{lady} cannot be courted."); return false; }
        var p = State.Player;
        var pietyBlocked = definition.MaxPiety is { } maximum && p.Stats.Piety > maximum && (!definition.FameWaivesPiety || p.Fame == 0);
        if (p.Stats.Honor < definition.MinHonor || pietyBlocked) { Log($"{lady} declines your request for her colors."); return false; }
        p.LadyColors = definition.Name;
        Log($"{definition.Name} grants you her colors for the next joust.");
        return true;
    }

    // PLACEHOLDER: RULE-PERSON-005.
    private void RewardCourtship(string lady)
    {
        var p = State.Player;
        var wins = p.CourtshipWins.GetValueOrDefault(lady) + 1;
        p.CourtshipWins[lady] = wins;
        p.LadyColors = null;
        var definition = Balance.Courtships.Single(x => x.Name.Equals(lady, StringComparison.OrdinalIgnoreCase));
        var reward = definition.Rewards.FirstOrDefault(x => x.Win == wins);
        if (reward is not null && reward.Wealth > 0) { p.Wealth += reward.Wealth; Log($"{lady} rewards you with {reward.Wealth}s."); }
        if (reward?.Item is not null) { p.Inventory.Items.Add(reward.Item); Log($"{lady} rewards you with {reward.Item}."); }
        if (definition.Name == "Anna Lisa" && wins >= 2 && !DragonLairDiscovered)
        {
            State.DragonProgress = 1;
            Log("Anna Lisa confides that the dragon's lair lies in the mountains of northwestern Wales.");
        }
        if (definition.MarriageWins > 0 && wins >= definition.MarriageWins) p.Wife = definition.Name;
    }

    public void Save(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var fullPath = Path.GetFullPath(path);
        var directory = Path.GetDirectoryName(fullPath)!;
        Directory.CreateDirectory(directory);
        State.SchemaVersion = CurrentSaveSchemaVersion;
        var json = JsonSerializer.Serialize(State, new JsonSerializerOptions { WriteIndented = true });
        var temporaryPath = Path.Combine(directory, $".{Path.GetFileName(fullPath)}.{Guid.NewGuid():N}.tmp");
        try
        {
            using (var stream = new FileStream(temporaryPath, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       bufferSize: 4096, FileOptions.WriteThrough))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
            {
                writer.Write(json);
                writer.Flush();
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporaryPath, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    public static Campaign Load(string path, int seed = 1086)
    {
        var state = JsonSerializer.Deserialize<CampaignState>(File.ReadAllText(path))
            ?? throw new InvalidDataException("Invalid campaign save.");
        if (state.SchemaVersion < 0 || state.SchemaVersion > CurrentSaveSchemaVersion)
            throw new InvalidDataException($"Unsupported campaign save schema {state.SchemaVersion}.");
        MigrateSave(state);
        return new Campaign(state, seed);
    }

    private static void MigrateSave(CampaignState state)
    {
        // Schema 0 is every save written before explicit versioning. Existing
        // constructors and EnsureStrategicState supply its missing fields.
        if (state.SchemaVersion == 0) state.SchemaVersion = 1;
    }
    public void Log(string text) { State.Journal.Add($"{State.Date:dd MMM yyyy}: {text}"); if (State.Journal.Count > 60) State.Journal.RemoveAt(0); }
}

public sealed record BattleResult(bool Won, int FriendlyLosses, int EnemyLosses, string Summary);

public static class Combat
{
    public static BattleResult Resolve(Army friendly, Army enemy, Random random)
    {
        var friendlyStart = friendly.Total;
        var enemyStart = enemy.Total;
        if (friendlyStart == 0) return new(false, 0, 0, "No army stands with you.");
        var friendlyPower = Power(friendly, enemy) * (.9 + random.NextDouble() * .2);
        var enemyPower = Power(enemy, friendly) * (.9 + random.NextDouble() * .2);
        var won = friendlyPower >= enemyPower;
        var friendlyLosses = Math.Min(friendlyStart, (int)Math.Round(enemyPower / Math.Max(1, friendlyPower) * friendlyStart * (won ? .32 : .72)));
        var enemyLosses = Math.Min(enemyStart, (int)Math.Round(friendlyPower / Math.Max(1, enemyPower) * enemyStart * (won ? .72 : .32)));
        ApplyLosses(friendly, friendlyLosses);
        ApplyLosses(enemy, enemyLosses);
        return new(won, friendlyLosses, enemyLosses, $"Field battle {(won ? "won" : "lost")}: {friendlyLosses} of yours and {enemyLosses} enemies fell.");
    }

    private static double Power(Army army, Army enemy) => army.Units.Sum(pair =>
        pair.Value * (1 + enemy.Units[Balance.Counter(pair.Key)] / (double)Math.Max(1, enemy.Total) * .75));

    private static void ApplyLosses(Army army, int losses)
    {
        while (losses-- > 0 && army.Total > 0)
        {
            var type = army.Units.OrderByDescending(x => x.Value).First().Key;
            army.Units[type]--;
        }
    }
}
