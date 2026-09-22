using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void EstateShellLayoutAndTerrainAreDefinitionDriven()
    {
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Estate.Shell" && definition.IdSuffix == ":icontemp.pcx");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Estate" && definition.IdSuffix == ":iconmap.hat");
        var layout = EstatePresentationDefinitions.From(null);
        Assert.Equal(new UiBounds(19, 8, 370, 433), layout.MainViewport);
        Assert.Equal(new UiBounds(422, 4, 199, 159), layout.InsetMap);
        Assert.Equal(Enum.GetValues<EstateControlAction>().Length, layout.Controls.Count);
        Assert.Equal(layout.Controls.Count, layout.Controls.Select(control => control.Action).Distinct().Count());
        Assert.Equal(Enum.GetValues<EstatePanel>().Length, layout.Controls.Count(control => control.Panel.HasValue));
        Assert.Equal(Enum.GetValues<EstateTerrainKind>().Length, EstatePresentationDefinitions.TileFrames.Count);
        Assert.Equal(Enum.GetValues<EstateSeason>().Length, EstatePresentationDefinitions.TileAtlases.Count);
        Assert.All(EstatePresentationDefinitions.TileFrames.Values, frame => Assert.InRange(frame, 0, 336));
        Assert.Equal(EstateSeason.SpringSummer, EstatePresentationDefinitions.SeasonFor(new DateTime(1086, 3, 1)));
        Assert.Equal(EstateSeason.Autumn, EstatePresentationDefinitions.SeasonFor(new DateTime(1086, 10, 1)));
        Assert.Equal(EstateSeason.Winter, EstatePresentationDefinitions.SeasonFor(new DateTime(1086, 1, 1)));
        Assert.All(EstatePresentationDefinitions.TileAtlases, atlas =>
            Assert.Contains(ImportedAnimations.Definitions,
                definition => definition.Role == atlas.Role && definition.IdSuffix == atlas.IdSuffix
                    && definition.PaletteArtRole == "Estate.Shell"));
        foreach (var index in Enumerable.Range(0, EstatePresentationDefinitions.Columns * EstatePresentationDefinitions.Rows))
        {
            var bounds = EstatePresentationDefinitions.TileSpriteBounds(layout.MainViewport, index);
            Assert.InRange(bounds.X, layout.MainViewport.X, layout.MainViewport.X + layout.MainViewport.Width - bounds.Width);
            Assert.InRange(bounds.Y, layout.MainViewport.Y, layout.MainViewport.Y + layout.MainViewport.Height - bounds.Height);
        }
        var firstTile = EstatePresentationDefinitions.TileSpriteBounds(layout.MainViewport, 0);
        var lastTile = EstatePresentationDefinitions.TileSpriteBounds(layout.MainViewport,
            EstatePresentationDefinitions.Columns * EstatePresentationDefinitions.Rows - 1);
        Assert.Equal(new UiBounds(layout.MainViewport.X, layout.MainViewport.Y, 80, 80), firstTile);
        Assert.Equal(layout.MainViewport.X + layout.MainViewport.Width, lastTile.X + lastTile.Width);
        Assert.Equal(layout.MainViewport.Y + layout.MainViewport.Height, lastTile.Y + lastTile.Height);
        foreach (var (location, index) in World.Locations.Select((location, index) => (location, index)))
        {
            var point = EstatePresentationDefinitions.InsetPoint(layout.InsetMap, location);
            Assert.Equal(index, EstatePresentationDefinitions.LocationAt(layout.InsetMap, point.X, point.Y));
        }

        var fief = new Fief { Houses = 1 };
        fief.Crops[CropType.Grain] = 2;
        var terrain = EstatePresentationDefinitions.TerrainFor(fief);
        Assert.Equal(EstatePresentationDefinitions.Columns * EstatePresentationDefinitions.Rows, terrain.Count);
        Assert.Equal(2, terrain.Count(kind => kind == EstateTerrainKind.Grain));
        Assert.Single(terrain, kind => kind == EstateTerrainKind.Settlement);
    }

    [Fact]
    public void FarmCommandsKeepInputBehaviorAndHelpInOneRegistry()
    {
        var commands = FarmPresentationDefinitions.Commands;
        Assert.Equal(commands.Count, commands.Select(command => command.Key).Distinct().Count());
        Assert.Equal(Enum.GetValues<CropType>(), commands.Select(command => command.Action).OfType<PlantFarmAction>().Select(action => action.Crop).Order());
        Assert.Equal(Enum.GetValues<ForestIndustry>(), commands.Select(command => command.Action).OfType<DevelopForestFarmAction>().Select(action => action.Industry).Order());
        Assert.Equal(Enum.GetValues<UnitType>(), commands.Select(command => command.Action).OfType<RecruitFarmAction>().Select(action => action.Unit).Order());
        Assert.Equal(6, commands.Count(command => command.Action is BuildFarmAction));
        Assert.Equal(2, commands.Count(command => command.Action is LeaveFarmAction));
        Assert.Equal(
            [(FarmPresentationDefinitions.Section.Castle, ":fcastle.hat", 18, 20, 21),
             (FarmPresentationDefinitions.Section.Village, ":fvillage.hat", 14, 17, 18),
             (FarmPresentationDefinitions.Section.Farm, ":ffarm.hat", 9, 11, 12),
             (FarmPresentationDefinitions.Section.Forest, ":fforest.hat", 8, 10, 11)],
            FarmPresentationDefinitions.Layouts.Select(layout =>
                (layout.Section, layout.LayoutSuffix, layout.AccountRowCount, layout.TerrainRegionId, layout.FullScreenRegionId)));
        Assert.All(FarmPresentationDefinitions.Layouts, layout =>
            Assert.Contains(ImportedLayouts.Definitions,
                imported => imported.Role == layout.LayoutRole && imported.IdSuffix == layout.LayoutSuffix));
        Assert.All(Enum.GetValues<FarmPresentationDefinitions.Section>(), section =>
        {
            var sectionCommands = FarmPresentationDefinitions.CommandsFor(section);
            Assert.Equal(2, sectionCommands.Count(command => command.Action is LeaveFarmAction));
            Assert.All(FarmPresentationDefinitions.HelpRowsFor(section), row => Assert.False(string.IsNullOrWhiteSpace(row)));
        });
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Castle),
            command => Assert.True(command.Action is BuildFarmAction or LeaveFarmAction));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Village),
            command => Assert.True(command.Action is BuildFarmAction or LeaveFarmAction));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Farm),
            command => Assert.True(command.Action is PlantFarmAction or LeaveFarmAction));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Forest),
            command => Assert.True(command.Action is DevelopForestFarmAction or LeaveFarmAction));

        var bytes = new byte[40 + 13 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 13);
        for (var id = 0; id < 13; id++)
        {
            var offset = 40 + id * 24;
            WriteInt(bytes, offset, id); WriteInt(bytes, offset + 4, id);
            WriteInt(bytes, offset + 8, id); WriteInt(bytes, offset + 12, 1);
            WriteInt(bytes, offset + 16, 1); WriteInt(bytes, offset + 20, 1);
        }
        var importedFarm = FarmPresentationDefinitions.LayoutFrom(
            FarmPresentationDefinitions.Section.Farm, new HatLayout(bytes));
        Assert.Equal(new UiBounds(11, 11, 1, 1), importedFarm.Terrain);
        Assert.Equal(new UiBounds(12, 12, 1, 1), importedFarm.FullScreen);
        Assert.Equal(9, importedFarm.Rows.Count);
        Assert.Equal(new UiBounds(0, 0, 1, 1), importedFarm.Rows[0]);
        Assert.Equal(new UiBounds(9, 9, 1, 1), importedFarm.Okay);
        Assert.Equal(new UiBounds(10, 10, 1, 1), importedFarm.Cancel);
        Assert.Equal(FarmPresentationDefinitions.FooterAction.Okay,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 9, 9));
        Assert.Equal(FarmPresentationDefinitions.FooterAction.Cancel,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 10, 10));
        Assert.Equal(FarmPresentationDefinitions.FooterAction.FullScreen,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 12, 12));
        Assert.Null(FarmPresentationDefinitions.FooterActionAt(importedFarm, 20, 20));

        Assert.Equal(["Wall", "Tower", "Great Hall", "Servant Room", "Guardhouse", "Gate House", "Storehouse",
            "Chapel", "Well", "Stable", "Steward", "Beadle", "Guard Captain", "Guard", "Priest", "Mason", "Serf"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Castle).Select(entry => entry.Label));
        Assert.Equal(["Clear Land", "Road", "Mill", "Tavern", "Bakery", "Inn", "Carpenter", "Smith", "Tanner",
            "Merchant", "Church", "Monastery", "Barber", "Houses", "Livestock", "Horses", "Granary"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Village).Select(entry => entry.Label));
        Assert.Equal(["Grain", "Beans", "Vegetables", "Fruit"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Farm).Select(entry => entry.Label));
        Assert.Equal(["Cut Timber", "Iron Mine", "Woodward", "Coal Mine", "Gold Mine", "Silver Mine", "Prospector"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Forest).Select(entry => entry.Label));
    }

    [Fact]
    public void FiefManagementCheckpointRestoresPendingEconomyChanges()
    {
        var campaign = new Campaign();
        campaign.State.Player.Wealth = 10_000;
        var originalWealth = campaign.State.Player.Wealth;
        var originalJournal = campaign.State.Journal.Count;
        var checkpoint = FiefManagementCheckpoint.Capture(campaign.State);

        Assert.True(campaign.Build(BuildingKind.House));
        Assert.True(campaign.Plant(CropType.Beans));
        Assert.True(campaign.DevelopForest(ForestIndustry.Timber));
        Assert.True(campaign.Recruit(UnitType.Swordsmen));
        campaign.State.Player.Home.Population = 999;
        campaign.State.Player.Home.TaxRate = 75;
        campaign.State.Player.Home.Prospector = true;
        checkpoint.Restore(campaign.State);

        Assert.Equal(originalWealth, campaign.State.Player.Wealth);
        Assert.Equal((1200, 10, 0, false), (campaign.State.Player.Home.Population,
            campaign.State.Player.Home.TaxRate, campaign.State.Player.Home.Houses,
            campaign.State.Player.Home.Prospector));
        Assert.All(campaign.State.Player.Home.Crops.Values, value => Assert.Equal(0, value));
        Assert.All(campaign.State.Player.Home.Forest.Values, value => Assert.Equal(0, value));
        Assert.All(campaign.State.Player.Army.Units.Values, value => Assert.Equal(0, value));
        Assert.Equal(originalJournal, campaign.State.Journal.Count);
    }

    [Fact]
    public void WarPlanningUsesFiveArmiesAndHundredSerfCompanies()
    {
        var campaign = new Campaign();
        var player = campaign.State.Player;
        Assert.Equal(Player.ArmyDivisionLimit - 1, player.AdditionalArmies.Count);

        Assert.True(campaign.AdjustArmyCompany(4, UnitType.Knights, 1));
        Assert.Equal(100, player.ArmyAt(4).Units[UnitType.Knights]);
        Assert.Equal(1100, player.AvailableSerfs);
        Assert.True(campaign.AdjustArmyCompany(4, UnitType.Knights, -1));
        Assert.Equal(0, player.ArmyAt(4).Total);
        Assert.False(campaign.AdjustArmyCompany(4, UnitType.Knights, -1));

        player.Home.Population = 10_000;
        for (var company = 0; company < WarPlanningCheckpoint.MaximumCompaniesPerArmy; company++)
            Assert.True(campaign.AdjustArmyCompany(0, UnitType.Swordsmen, 1));
        Assert.Equal(6000, player.Army.Total);
        Assert.False(campaign.AdjustArmyCompany(0, UnitType.Swordsmen, 1));

        player.AdditionalArmies[3].Location = 2;
        Assert.False(campaign.AdjustArmyCompany(4, UnitType.Swordsmen, 1));
    }

    [Fact]
    public void CaptainCommandedDivisionTravelsFightsAndSurvivesSaveLoadIndependently()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2), 1086);
        var york = Array.FindIndex(World.Locations, location => location.Name == "York");
        foreach (var type in Enum.GetValues<UnitType>()) campaign.State.Player.ArmyAt(2).Units[type] = 40;
        Assert.True(campaign.FieldArmy(2));
        Assert.False(campaign.DispatchArmy(0, york));
        Assert.True(campaign.DispatchArmy(2, york));

        var order = Assert.IsType<StrategicArmyOrder>(campaign.ArmyOrderAt(2));
        var path = Path.Combine(Path.GetTempPath(), $"conqueror-army-order-{Guid.NewGuid():N}.json");
        try
        {
            campaign.Save(path);
            var loaded = Campaign.Load(path);
            Assert.Equal(order, loaded.ArmyOrderAt(2));
        }
        finally
        {
            File.Delete(path);
        }

        campaign.AdvanceDays((order.Arrives - campaign.State.Date).Days - 1);
        Assert.Equal(0, campaign.State.Player.ArmyLocationAt(2));
        Assert.NotNull(campaign.ArmyOrderAt(2));
        campaign.AdvanceDays(1);

        Assert.Null(campaign.ArmyOrderAt(2));
        Assert.Equal(york, campaign.State.Player.ArmyLocationAt(2));
        Assert.True(campaign.GarrisonAt(york) < World.Locations[york].Garrison);
        Assert.Equal(0, campaign.State.Player.Army.Total);
        Assert.Contains(campaign.State.Journal, entry => entry.Contains("Captain's report", StringComparison.Ordinal));
    }

    [Fact]
    public void ArmyRosterRepairsLegacySlotsAndRejectsOverflow()
    {
        var player = new Player { AdditionalArmies = [] };
        player.EnsureArmyRoster();
        Assert.Equal(["Army 2", "Army 3", "Army 4", "Army 5"],
            player.AdditionalArmies.Select(army => army.Name));

        player.AdditionalArmies.Add(new StrategicArmyDivision { Name = "Army 6" });
        Assert.Throws<InvalidDataException>(player.EnsureArmyRoster);
    }

    [Fact]
    public void WarPlanningCheckpointRollsBackAllDivisionsSpiesAndMembership()
    {
        var campaign = new Campaign();
        campaign.State.Player.Wealth = 500;
        var checkpoint = new WarPlanningCheckpoint(campaign);

        Assert.True(campaign.AdjustArmyCompany(2, UnitType.Halberdiers, 1));
        campaign.State.Player.SetArmyName(2, "Northern Guard");
        Assert.True(campaign.FieldArmy(2));
        Assert.True(campaign.ToggleArmyMembership(2));
        Assert.True(campaign.AssignSpy());
        checkpoint.Restore(campaign);

        var player = campaign.State.Player;
        Assert.Equal(500, player.Wealth);
        Assert.Equal(0, player.ArmyAt(2).Total);
        Assert.Equal("Army 3", player.ArmyNameAt(2));
        Assert.False(player.ArmyIsFielded(2));
        Assert.Equal(0, player.JoinedArmyIndex);
        Assert.Equal(0, player.ActiveSpies);
        Assert.Empty(campaign.State.Journal);
    }

    [Fact]
    public void ActiveSpyReportsTheFirstMovementSlotAndIsConsumedBeforeMovementAdvances()
    {
        var campaign = new Campaign(seed: 17);
        campaign.State.Player.Wealth = 500;
        var departed = campaign.State.Date;
        campaign.State.EnemyMovements.Add(new StrategicEnemyMovement(
            3, 2, 4, departed, departed.AddDays(4), 2, 3, 4));
        campaign.State.EnemyMovements.Add(new StrategicEnemyMovement(
            1, 6, 8, departed, departed.AddDays(1), 5, 6, 7));

        Assert.True(campaign.AssignSpy());
        Assert.Equal((420, 1), (campaign.State.Player.Wealth, campaign.State.Player.ActiveSpies));
        Assert.False(campaign.AssignSpy());
        Assert.Equal(420, campaign.State.Player.Wealth);
        campaign.AdvanceDays(1);

        Assert.Equal(0, campaign.State.Player.ActiveSpies);
        Assert.Equal(new StrategicSpyReport(
            departed.AddDays(1), 1, 6, 5, 6, 7), campaign.State.LatestSpyReport);
        Assert.Contains(campaign.State.Journal,
            entry => entry.Contains("Spy report from Cambridge", StringComparison.Ordinal));
        Assert.Single(campaign.State.EnemyMovements);
        Assert.Equal(3, campaign.State.EnemyMovements[0].Slot);
    }

    [Fact]
    public void ActiveSpyIsNotConsumedByMonthlySettlementWithoutAMovement()
    {
        var campaign = new Campaign();
        campaign.State.Player.Wealth = 500;

        Assert.True(campaign.AssignSpy());
        campaign.SettleMonth();

        Assert.Equal(1, campaign.State.Player.ActiveSpies);
        Assert.Null(campaign.State.LatestSpyReport);
        Assert.DoesNotContain(campaign.State.Journal,
            entry => entry.Contains("Spy report from", StringComparison.Ordinal));
    }

    [Fact]
    public void AutonomousEnemyMovementEventuallySuppliesTheSpyTrigger()
    {
        var campaign = new Campaign(seed: 17);
        campaign.State.Player.Wealth = 500;
        Assert.True(campaign.AssignSpy());

        for (var day = 0; day < 365 && campaign.State.Player.ActiveSpies != 0; day++)
            campaign.AdvanceDays(1);

        Assert.Equal(0, campaign.State.Player.ActiveSpies);
        Assert.NotNull(campaign.State.LatestSpyReport);
        Assert.InRange(campaign.State.LatestSpyReport!.MovementSlot, 0, 4);
        Assert.Equal(campaign.State.LatestSpyReport.Swordsmen,
            campaign.State.LatestSpyReport.Halberdiers);
        Assert.Equal(campaign.State.LatestSpyReport.Swordsmen,
            campaign.State.LatestSpyReport.Knights);
    }

    [Fact]
    public void EnemyMovementReinforcesItsDestinationAndReleasesItsSlotOnArrival()
    {
        var campaign = new Campaign(seed: 17);
        var departed = campaign.State.Date;
        var destinationBefore = campaign.GarrisonAt(4);
        campaign.State.EnemyMovements.Add(new StrategicEnemyMovement(
            2, 2, 4, departed, departed.AddDays(1), 2, 3, 4));

        campaign.AdvanceDays(1);

        Assert.Empty(campaign.State.EnemyMovements);
        Assert.Equal(destinationBefore + 9, campaign.GarrisonAt(4));
        Assert.Contains(campaign.State.Journal,
            entry => entry.Contains("enemy column reaches Bristol", StringComparison.Ordinal));
    }

    [Fact]
    public void WeaponStoreTableParsesBoundedSixLineRecords()
    {
        var text = "first.smk\r\n2\r\n7\r\n9\r\n500\r\nA synthetic sword.\r\n#\r\n3\r\n8\r\n10\r\n120\r\nSynthetic armor.";

        var resource = WeaponStoreDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(text));

        Assert.Equal(2, resource.Entries.Count);
        Assert.Equal(new WeaponStoreEntry(0, "first.smk", 2, 7, 9, 500, "A synthetic sword."), resource.Entries[0]);
        Assert.Equal(8, resource.Entries[1].ImageFrame);
    }

    [Fact]
    public void WeaponStoreTableAndEquipmentMappingsRejectAmbiguity()
    {
        var incomplete = System.Text.Encoding.ASCII.GetBytes("#\r\n1\r\n2");
        Assert.Throws<InvalidDataException>(() => WeaponStoreDecoder.Decode(incomplete));
        Assert.Throws<InvalidDataException>(() => WeaponStoreDecoder.Decode([0xFF]));

        var mapped = Balance.StoreEquipment.Select(item => item.OriginalStoreRecord).ToArray();
        Assert.All(mapped, record => Assert.NotNull(record));
        Assert.Equal(mapped.Length, mapped.Distinct().Count());
        Assert.Equal(Enumerable.Range(0, 39).Select(index => (int?)index), mapped);
        Assert.Equal(2, Balance.Equipment.Single(item => item.Name == "Battle Sword").OriginalStoreRecord);
        Assert.Equal(84, Balance.Equipment.Single(item => item.Name == "Fighter's Dagger").BuyPrice);
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition.Role == "Shop.Items" && definition.IdSuffix == ":swords.csf" && definition.PaletteArtRole == "Shop.Inventory");
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition.Role == "Shop.Controls" && definition.IdSuffix == ":buysell.csf" && definition.PaletteArtRole == "Shop.Inventory");
        Assert.True(new WeaponStoreEntry(0, "item.smk", 0, 0, 0, 1, "Item").HasMovie);
        Assert.False(new WeaponStoreEntry(0, "#", 0, 0, 0, 1, "Item").HasMovie);
    }

    [Fact]
    public void DilemmaChoiceHotspotsComeFromTheOriginalLayout()
    {
        byte[] bytes = new byte[40 + 3 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 3);
        for (var index = 0; index < 3; index++)
        {
            var offset = 40 + index * 24;
            WriteInt(bytes, offset, index); WriteInt(bytes, offset + 4, 10 + index * 100);
            WriteInt(bytes, offset + 8, 300); WriteInt(bytes, offset + 12, 90);
            WriteInt(bytes, offset + 16, 140); WriteInt(bytes, offset + 20, 1);
        }
        var choices = YouthDilemmaPresentationDefinitions.ChoicesFrom(new HatLayout(bytes));

        Assert.Equal([new UiBounds(10, 300, 90, 140), new UiBounds(110, 300, 90, 140), new UiBounds(210, 300, 90, 140)], choices);
        Assert.Equal(YouthDilemmaPresentationDefinitions.Continue,
            YouthDilemmaPresentationDefinitions.ContinueFrom(new HatLayout(bytes)));
    }

    [Fact]
    public void HatLayoutDecodesAndOverridesFallbackRegions()
    {
        var bytes = new byte[64];
        WriteInt(bytes, 0, 7); WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 1);
        System.Text.Encoding.ASCII.GetBytes("SCREEN.PCX").CopyTo(bytes, 24);
        bytes[37] = 0x6d; bytes[38] = 0xc0; bytes[39] = 0x45;
        WriteInt(bytes, 40, 0); WriteInt(bytes, 44, 69); WriteInt(bytes, 48, 18);
        WriteInt(bytes, 52, 119); WriteInt(bytes, 56, 133); WriteInt(bytes, 60, 1);

        var layout = new HatLayout(bytes);

        Assert.Equal((7, "SCREEN.PCX", 0x45c06d), (layout.ScreenId, layout.BackgroundName, layout.UnknownTag));
        Assert.Equal(new HatRegion(0, 69, 18, 119, 133, 1), layout.FindRegion(0));
        Assert.Equal(new UiBounds(69, 18, 119, 133), CharacterCreationDefinitions.PregeneratedFrom(layout)[0]);
        Assert.Equal(new UiBounds(69, 18, 119, 133),
            HomePresentationDefinitions.HotspotsFrom(layout).Single(hotspot => hotspot.HatRegionId == 0).Bounds);
        Assert.Equal(new UiBounds(69, 18, 119, 133),
            WarPlanningPresentationDefinitions.From(layout).ArmyButtons[0]);
    }

    [Fact]
    public void HomeDeskBooksRemainThreeDistinctDescriptorDrivenHotspots()
    {
        var bytes = new byte[40 + 10 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 10);
        for (var id = 0; id < 10; id++)
        {
            var offset = 40 + id * 24;
            WriteInt(bytes, offset, id);
            WriteInt(bytes, offset + 4, 10 + id * 20);
            WriteInt(bytes, offset + 8, 100 + id);
            WriteInt(bytes, offset + 12, 11 + id);
            WriteInt(bytes, offset + 16, 21 + id);
            WriteInt(bytes, offset + 20, 1);
        }

        var books = HomePresentationDefinitions.HotspotsFrom(new HatLayout(bytes))
            .Where(hotspot => hotspot.Action is SceneNavigationAction.Farm
                or SceneNavigationAction.Village or SceneNavigationAction.Forest)
            .ToArray();

        Assert.Equal(
            [(SceneNavigationAction.Farm, 2, new UiBounds(50, 102, 13, 23)),
             (SceneNavigationAction.Village, 3, new UiBounds(70, 103, 14, 24)),
             (SceneNavigationAction.Forest, 4, new UiBounds(90, 104, 15, 25))],
            books.Select(book => (book.Action, book.HatRegionId, book.Bounds)));
    }

    [Fact]
    public void InnPatronHotspotsAndFooterComeFromTheOriginalDescriptor()
    {
        var bytes = new byte[40 + 12 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 12);
        for (var id = 0; id < 12; id++)
        {
            var offset = 40 + id * 24;
            WriteInt(bytes, offset, id);
            WriteInt(bytes, offset + 4, 5 + id * 10);
            WriteInt(bytes, offset + 8, 20 + id);
            WriteInt(bytes, offset + 12, 30 + id);
            WriteInt(bytes, offset + 16, 40 + id);
            WriteInt(bytes, offset + 20, 1);
        }

        var layout = InnPresentationDefinitions.From(new HatLayout(bytes));

        Assert.Equal(InnPresentationDefinitions.PatronCount, layout.Patrons.Count);
        Assert.Equal(Enumerable.Range(0, 10), layout.Patrons.Select(patron => patron.HatRegionId));
        Assert.Equal(["Frederick", "Gerard", "Barkeep", "Otto", "Hugh", "Gilbert", "Nellie", "Richard", "Ivo", "Albert"],
            layout.Patrons.Select(patron => patron.Name));
        Assert.Equal([1900, 1100, 3200, 3500, 1600, 1400, 3000, 1698, 3300, 3600],
            layout.Patrons.Select(patron => patron.ConversationRootNodeId));
        Assert.All(layout.Patrons, patron => Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == patron.PortraitRole && definition.IdSuffix == patron.PortraitSuffix));
        Assert.Equal(new UiBounds(75, 27, 37, 47), layout.Patrons[7].Bounds);
        Assert.Equal(new UiBounds(105, 30, 40, 50), layout.ExitBounds);
        Assert.Equal(new UiBounds(115, 31, 41, 51), layout.HoverLabelBounds);
    }

    [Fact]
    public void HatLayoutRejectsOutOfBoundsRegions()
    {
        var bytes = new byte[64];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 1);
        WriteInt(bytes, 44, 650); WriteInt(bytes, 48, 10); WriteInt(bytes, 52, 20); WriteInt(bytes, 56, 20);
        Assert.Throws<InvalidDataException>(() => new HatLayout(bytes));
    }

    [Fact]
    public void LinearExecutableInternalFixupsResolveObjectRelativeTargets()
    {
        var bytes = new byte[0x180];
        const int header = 0x40;
        bytes[header] = (byte)'L'; bytes[header + 1] = (byte)'E';
        WriteInt(bytes, header + 0x14, 2); WriteInt(bytes, header + 0x28, 0x1000);
        WriteInt(bytes, header + 0x40, 0xb0); WriteInt(bytes, header + 0x44, 2);
        WriteInt(bytes, header + 0x68, 0xe0); WriteInt(bytes, header + 0x6c, 0xec);
        var firstObject = header + 0xb0;
        WriteInt(bytes, firstObject, 0x1000); WriteInt(bytes, firstObject + 4, 0x10000);
        WriteInt(bytes, firstObject + 12, 1); WriteInt(bytes, firstObject + 16, 1);
        var secondObject = firstObject + 24;
        WriteInt(bytes, secondObject, 0x2000); WriteInt(bytes, secondObject + 4, 0x20000);
        WriteInt(bytes, secondObject + 12, 2); WriteInt(bytes, secondObject + 16, 1);
        var pageTable = header + 0xe0;
        WriteInt(bytes, pageTable, 0); WriteInt(bytes, pageTable + 4, 9); WriteInt(bytes, pageTable + 8, 9);
        var record = header + 0xec;
        bytes[record] = 7; bytes[record + 1] = 0x10;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(record + 2), 0x20);
        bytes[record + 4] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(record + 5), 0x17c0);

        Assert.Equal(
            [new LinearExecutableFixup(1, 0x10020, 7, 2, 0x17c0, false, false)],
            LinearExecutableFixupReader.ReadInternalFixups(bytes));
        Assert.Throws<InvalidDataException>(() => LinearExecutableFixupReader.ReadInternalFixups(bytes[..(record + 8)]));
    }

    [Fact]
    public void FiveSaveSlotsRoundTripAndRecognizeLegacySlotOne()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-slots-{Guid.NewGuid():N}");
        try
        {
            var slots = new CampaignSaveSlots(root);
            Assert.Equal(5, slots.Inspect().Count);
            Assert.All(slots.Inspect(), slot => Assert.False(slot.Exists));

            var campaign = new Campaign(Campaign.NewFromTemplate(0));
            campaign.State.Date = new DateTime(1087, 4, 3);
            campaign.State.Player.AdditionalArmies[1].Name = "March Wardens";
            campaign.State.Player.AdditionalArmies[1].Force.Units[UnitType.Knights] = 200;
            campaign.State.Player.AdditionalArmies[1].IsFielded = true;
            campaign.State.Player.JoinedArmyIndex = 2;
            campaign.State.Player.ActiveSpies = 3;
            campaign.State.EnemyMovements.Add(new StrategicEnemyMovement(
                4, 13, 15, campaign.State.Date, campaign.State.Date.AddDays(3), 4, 5, 6));
            campaign.State.LatestSpyReport = new StrategicSpyReport(
                campaign.State.Date, 2, 8, 7, 8, 9);
            slots.Save(campaign, 3);

            var third = slots.Inspect(3);
            Assert.True(third.IsValid);
            Assert.Equal(campaign.State.Player.Name, third.PlayerName);
            Assert.Equal(campaign.State.Date, third.CampaignDate);
            Assert.True(slots.TryLoad(3, out var loaded, out var error));
            Assert.Null(error);
            Assert.Equal(campaign.State.Player.Name, loaded!.State.Player.Name);
            Assert.Equal(("March Wardens", 200, true, 2, 3),
                (loaded.State.Player.AdditionalArmies[1].Name,
                 loaded.State.Player.AdditionalArmies[1].Force.Units[UnitType.Knights],
                 loaded.State.Player.AdditionalArmies[1].IsFielded,
                 loaded.State.Player.JoinedArmyIndex,
                 loaded.State.Player.ActiveSpies));
            Assert.Equal(campaign.State.EnemyMovements, loaded.State.EnemyMovements);
            Assert.Equal(campaign.State.LatestSpyReport, loaded.State.LatestSpyReport);

            campaign.Save(Path.Combine(root, "campaign.json"));
            Assert.True(slots.Inspect(1).IsValid);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SaveSlotsRejectInvalidNumbersAndReportCorruptFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-slots-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "campaign-2.json"), "not json");
            var slots = new CampaignSaveSlots(root);

            Assert.Throws<ArgumentOutOfRangeException>(() => slots.SlotPath(0));
            Assert.True(slots.Inspect(2).Exists);
            Assert.False(slots.Inspect(2).IsValid);
            Assert.False(slots.TryLoad(2, out var campaign, out var error));
            Assert.Null(campaign);
            Assert.Equal("PRIMARY SAVE IS MALFORMED; NO VALID BACKUP IS AVAILABLE", error);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SaveSlotsVersionAtomicWritesAndRecoverThePreviousSave()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-recovery-{Guid.NewGuid():N}");
        try
        {
            var slots = new CampaignSaveSlots(root);
            var campaign = new Campaign(Campaign.NewFromTemplate(0));
            campaign.State.Player.Wealth = 111;
            slots.Save(campaign, 4);
            Assert.Contains($"\"SchemaVersion\": {Campaign.CurrentSaveSchemaVersion}", File.ReadAllText(slots.SlotPath(4)));

            campaign.State.Player.Wealth = 222;
            slots.Save(campaign, 4);
            Assert.True(File.Exists(slots.BackupPath(4)));
            File.WriteAllText(slots.SlotPath(4), "interrupted");

            var info = slots.Inspect(4);
            Assert.True(info.IsValid);
            Assert.True(info.RecoveredFromBackup);
            Assert.Equal("PRIMARY SAVE IS MALFORMED; RECOVERY BACKUP IS AVAILABLE", info.Error);
            Assert.True(slots.TryLoad(4, out var recovered, out var error));
            Assert.Null(error);
            Assert.Equal(111, recovered!.State.Player.Wealth);
            Assert.Empty(Directory.EnumerateFiles(root, "*.tmp"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SaveSlotsExplainMissingAndUnreadableRecoveryGenerations()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-diagnostics-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var slots = new CampaignSaveSlots(root);
            var campaign = new Campaign(Campaign.NewFromTemplate(0));
            campaign.Save(slots.BackupPath(1));

            var backupOnly = slots.Inspect(1);
            Assert.True(backupOnly.IsValid);
            Assert.True(backupOnly.RecoveredFromBackup);
            Assert.Equal("PRIMARY SAVE IS MISSING; RECOVERY BACKUP IS AVAILABLE", backupOnly.Error);
            Assert.True(slots.TryLoad(1, out _, out var recoveryNotice));
            Assert.Null(recoveryNotice);

            File.WriteAllText(slots.SlotPath(2), "broken primary");
            File.WriteAllText(slots.BackupPath(2), "broken backup");
            Assert.False(slots.TryLoad(2, out _, out var bothBroken));
            Assert.Equal("PRIMARY SAVE IS MALFORMED; BACKUP IS MALFORMED", bothBroken);

            var future = Campaign.NewFromTemplate(0);
            future.SchemaVersion = Campaign.CurrentSaveSchemaVersion + 1;
            File.WriteAllText(slots.SlotPath(3), System.Text.Json.JsonSerializer.Serialize(future));
            Assert.Equal("PRIMARY SAVE USES AN UNSUPPORTED FORMAT; NO VALID BACKUP IS AVAILABLE",
                slots.Inspect(3).Error);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void AutosaveUsesItsOwnRecoverableStreamWithoutConsumingManualSlots()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-autosave-{Guid.NewGuid():N}");
        try
        {
            var slots = new CampaignSaveSlots(root);
            var campaign = new Campaign(Campaign.NewFromTemplate(0));
            campaign.State.Player.Wealth = 100;
            slots.SaveAutosave(campaign);
            Assert.True(slots.InspectAutosave().IsValid);
            Assert.All(slots.Inspect(), slot => Assert.False(slot.Exists));

            campaign.State.Player.Wealth = 200;
            slots.SaveAutosave(campaign);
            Assert.True(File.Exists(slots.AutosaveBackupPath));
            File.WriteAllText(slots.AutosavePath, "interrupted");
            var info = slots.InspectAutosave();
            Assert.True(info.IsValid);
            Assert.True(info.RecoveredFromBackup);
            Assert.True(slots.TryLoadAutosave(out var recovered, out var error));
            Assert.Null(error);
            Assert.Equal(100, recovered!.State.Player.Wealth);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void UnversionedSavesMigrateAndFutureSchemasAreRejected()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-schema-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var legacyPath = Path.Combine(root, "legacy.json");
            File.WriteAllText(legacyPath, System.Text.Json.JsonSerializer.Serialize(Campaign.NewFromTemplate(0)));
            Assert.Equal(Campaign.CurrentSaveSchemaVersion, Campaign.Load(legacyPath).State.SchemaVersion);

            var future = Campaign.NewFromTemplate(0);
            future.SchemaVersion = Campaign.CurrentSaveSchemaVersion + 1;
            var futurePath = Path.Combine(root, "future.json");
            File.WriteAllText(futurePath, System.Text.Json.JsonSerializer.Serialize(future));
            Assert.Throws<InvalidDataException>(() => Campaign.Load(futurePath));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

}
