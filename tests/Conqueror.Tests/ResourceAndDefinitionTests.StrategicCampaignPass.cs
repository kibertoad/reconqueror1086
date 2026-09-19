using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicEncounterMenuPreservesExecutableRegionOrderAndSelectionCodes()
    {
        var entries = OriginalStrategicEncounterMenu.Entries;

        Assert.Equal(5, entries.Count);
        Assert.Equal(new OriginalStrategicEncounterMenuEntry(0, 30, 30, 215, 185, 2), entries[0]);
        Assert.Equal(new OriginalStrategicEncounterMenuEntry(1, 400, 30, 205, 200, 3), entries[1]);
        Assert.Equal(new OriginalStrategicEncounterMenuEntry(2, 25, 250, 185, 185, 1), entries[2]);
        Assert.Equal(new OriginalStrategicEncounterMenuEntry(3, 375, 250, 185, 185, 0), entries[3]);
        Assert.Equal(new OriginalStrategicEncounterMenuEntry(4, 220, 417, 148, 38, null), entries[4]);
        Assert.Equal(OriginalStrategicEncounterMenu.ExitRegionIndex, entries[4].RegionIndex);
        Assert.True(entries[4].ExitsToAutomaticFallback);
        Assert.All(entries.Take(OriginalStrategicEncounterMenu.InteractiveSelectionCount),
            entry => Assert.False(entry.ExitsToAutomaticFallback));
    }

    [Fact]
    public void StrategicInteractiveEncounterPreservesSixCategoryUnitOrderAndPositiveStrengthWriteBack()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(2, 1, 1),
            new OriginalStrategicEncounterForces(1, 1, 2));

        Assert.Equal([
            (OriginalStrategicInteractiveEncounterSide.Player, OriginalStrategicInteractiveEncounterCategory.Swordsmen, 3, 10),
            (OriginalStrategicInteractiveEncounterSide.Player, OriginalStrategicInteractiveEncounterCategory.Swordsmen, 3, 10),
            (OriginalStrategicInteractiveEncounterSide.Player, OriginalStrategicInteractiveEncounterCategory.Halberdiers, 3, 20),
            (OriginalStrategicInteractiveEncounterSide.Player, OriginalStrategicInteractiveEncounterCategory.Knights, 3, 40),
            (OriginalStrategicInteractiveEncounterSide.Enemy, OriginalStrategicInteractiveEncounterCategory.Swordsmen, 7, 10),
            (OriginalStrategicInteractiveEncounterSide.Enemy, OriginalStrategicInteractiveEncounterCategory.Halberdiers, 7, 20),
            (OriginalStrategicInteractiveEncounterSide.Enemy, OriginalStrategicInteractiveEncounterCategory.Knights, 7, 40),
            (OriginalStrategicInteractiveEncounterSide.Enemy, OriginalStrategicInteractiveEncounterCategory.Knights, 7, 40),
        ], units.Select(unit => (unit.Side, unit.Category, unit.CombatTypeCode, unit.CategoryValue)));

        units[0].RemainingStrength = 0;
        units[3].RemainingStrength = -1;
        units[5].RemainingStrength = 0;

        Assert.Equal(new OriginalStrategicEncounterForces(1, 1, 0),
            OriginalStrategicInteractiveEncounter.CountSurvivors(
                units, OriginalStrategicInteractiveEncounterSide.Player));
        Assert.Equal(new OriginalStrategicEncounterForces(1, 0, 2),
            OriginalStrategicInteractiveEncounter.CountSurvivors(
                units, OriginalStrategicInteractiveEncounterSide.Enemy));
    }

    [Fact]
    public void StrategicInteractiveKnightDeathCompletionZeroesStrengthAndDowngradesItsLiveCategory()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(0, 0, 1),
            new OriginalStrategicEncounterForces(0, 1, 0));

        OriginalStrategicInteractiveEncounter.CompleteMappedDeathAnimation(units[0]);

        Assert.Equal(0, units[0].RemainingStrength);
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Halberdiers, units[0].Category);
        Assert.Equal(new OriginalStrategicEncounterForces(0, 0, 0),
            OriginalStrategicInteractiveEncounter.CountSurvivors(
                units, OriginalStrategicInteractiveEncounterSide.Player));
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Halberdiers, units[1].Category);
    }

    [Fact]
    public void StrategicInteractiveEncounterTimingUsesStrictTwoHundredMillisecondSamplesWithoutCatchUp()
    {
        var timing = new OriginalStrategicInteractiveEncounterTiming(TimeSpan.Zero);

        Assert.False(timing.TryBeginPass(TimeSpan.FromMilliseconds(200)));
        Assert.True(timing.TryBeginPass(TimeSpan.FromMilliseconds(201)));
        Assert.Equal(TimeSpan.FromMilliseconds(201), timing.LastSample);
        Assert.False(timing.TryBeginPass(TimeSpan.FromMilliseconds(401)));
        Assert.True(timing.TryBeginPass(TimeSpan.FromMilliseconds(402)));

        Assert.True(timing.TryBeginPass(TimeSpan.FromSeconds(2)));
        Assert.Equal(TimeSpan.FromSeconds(2), timing.LastSample);
        Assert.False(timing.TryBeginPass(TimeSpan.FromMilliseconds(2200)));
    }

    [Fact]
    public void StrategicInteractiveEncounterGeometryUsesPositiveStrengthBoundsAndFirstExclusiveRectangleHit()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0));
        units[0].PositionX = 100;
        units[0].PositionY = 200;
        units[1].PositionX = 100;
        units[1].PositionY = 200;
        units[1].RemainingStrength = 0;

        var playerBounds = OriginalStrategicInteractiveEncounterGeometry.RenderRectangleFor(units[0]);
        var enemyBounds = OriginalStrategicInteractiveEncounterGeometry.RenderRectangleFor(units[1]);

        Assert.Equal(new OriginalStrategicInteractiveEncounterRectangle(85, 180, 25, 30), playerBounds);
        Assert.Equal(default, enemyBounds);
        Assert.True(playerBounds.Contains(85, 180));
        Assert.False(playerBounds.Contains(110, 180));
        Assert.False(playerBounds.Contains(85, 210));
        Assert.Equal(1, OriginalStrategicInteractiveEncounterGeometry.FindFirstContainingOneBased(
            [playerBounds, new OriginalStrategicInteractiveEncounterRectangle(80, 175, 40, 40)], 90, 190));
        Assert.Equal(0, OriginalStrategicInteractiveEncounterGeometry.FindFirstContainingOneBased(
            [playerBounds, enemyBounds], 110, 210));
    }

    [Fact]
    public void StrategicInteractiveMenuCodesZeroAndOneUseTheirExactPlayerPrefixGridLayouts()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(2, 1, 1),
            new OriginalStrategicEncounterForces(1, 0, 0));

        OriginalStrategicInteractiveEncounter.ApplyMappedMenuFormation(units, menuCode: 0,
            verticalSpan: 180);
        Assert.Equal([(60, 30), (60, 90), (60, 150), (120, 30)],
            units.Take(4).Select(unit => (unit.PositionX, unit.PositionY)));
        Assert.All(units.Skip(4), unit => Assert.Equal((0, 0), (unit.PositionX, unit.PositionY)));

        OriginalStrategicInteractiveEncounter.ApplyMappedMenuFormation(units, menuCode: 1,
            verticalSpan: 180);
        Assert.Equal([(120, 30), (120, 90), (120, 150), (60, 30)],
            units.Take(4).Select(unit => (unit.PositionX, unit.PositionY)));

        OriginalStrategicInteractiveEncounter.ApplyMappedMenuFormation(units, menuCode: 2,
            verticalSpan: 180);
        Assert.Equal([(135, 90), (90, 60), (90, 120), (45, 30)],
            units.Take(4).Select(unit => (unit.PositionX, unit.PositionY)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicInteractiveEncounter.ApplyMappedMenuFormation(units, menuCode: 3,
                verticalSpan: 180));
    }

    [Fact]
    public void StrategicInteractiveMenuCodeThreePreservesItsCategorySplitAndRawEnemyLayoutChoice()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(5, 2, 1),
            new OriginalStrategicEncounterForces(2, 1, 1));

        OriginalStrategicInteractiveEncounter.ApplyMappedMenuCodeThreeFormation(
            units, horizontalSpan: 640, verticalSpan: 180, new QueueEncounterRandom(0));

        Assert.Equal([
            (240, 30), (240, 90), (240, 480), (240, 540), (240, 600),
            (120, 30), (120, 90), (120, 150),
        ], units.Take(8).Select(unit => (unit.PositionX, unit.PositionY)));
        Assert.Equal([(580, 30), (580, 90), (580, 150), (520, 30)],
            units.Skip(8).Select(unit => (unit.PositionX, unit.PositionY)));

        OriginalStrategicInteractiveEncounter.ApplyMappedMenuCodeThreeFormation(
            units, horizontalSpan: 640, verticalSpan: 180, new QueueEncounterRandom(1));

        Assert.Equal([(520, 30), (520, 90), (520, 150), (580, 30)],
            units.Skip(8).Select(unit => (unit.PositionX, unit.PositionY)));
    }

    [Fact]
    public void NewCampaignStrategicBootstrapPersistsTheRandomlySelectedStartingRoute()
    {
        const int seed = 37;
        var campaign = new Campaign(Campaign.NewFromTemplate(0), seed);

        var strategic = campaign.InitializeOriginalStrategicNewGame();

        Assert.Same(strategic, campaign.State.OriginalStrategicState);
        Assert.Equal(new Random(seed).Next(OriginalStrategicMovement.StartingRouteCount),
            strategic.StartingRouteSelector);
        Assert.True(strategic.PlayerMovementSlots[OriginalStrategicMovement.PlayerAvatarMovementSlot]
            .Active);
        Assert.Throws<InvalidOperationException>(campaign.InitializeOriginalStrategicNewGame);
    }

    [Fact]
    public void NewCampaignStrategicBootstrapRejectsAnUnsettledDatedMovementRoster()
    {
        var state = Campaign.NewFromTemplate(0);
        state.EnemyMovements.Add(new StrategicEnemyMovement(
            0, 1, 2, state.Date, state.Date.AddDays(1), 1, 1, 1));
        var campaign = new Campaign(state);

        Assert.Throws<InvalidOperationException>(campaign.InitializeOriginalStrategicNewGame);
        Assert.Null(campaign.State.OriginalStrategicState);
    }

    [Fact]
    public void OriginalStrategicPassReportsTheFirstLiveSlotBeforeAdvancingBothRecordFamilies()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        state.Player.ActiveSpies = 1;
        state.Player.ArmyAt(0).Units[UnitType.Swordsmen] = 2;
        state.Player.ArmyAt(0).Units[UnitType.Halberdiers] = 3;
        state.Player.ArmyAt(0).Units[UnitType.Knights] = 4;
        state.Player.SetArmyFieldState(0, fielded: true, location: 0);
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        player.PathComplete = true;
        player.CurrentX = 1_000;
        player.CurrentY = 2_000;
        player.GridX = 7;
        player.GridY = 11;
        var hostile = strategic.MovementSlots[3];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 5;
        hostile.Halberdiers = 6;
        hostile.Knights = 7;
        hostile.CurrentX = 100;
        hostile.CurrentY = 100;
        hostile.DestinationX = 1_000;
        hostile.DestinationY = 100;
        hostile.DirectionX = 1;

        var resources = new StubStrategicResources();
        resources.Routes["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)];
        var campaign = new Campaign(state);
        campaign.ConfigureOriginalStrategicResources(resources);
        var result = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                GlobalTargetPerson: 17,
                GlobalOriginProperty: 0,
                DivisionTargets: [
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                SpecialPropertyHouseholdCount: 76),
            new QueueStrategicRandom());

        Assert.Equal(new StrategicSpyReport(state.Date, 3, -1, 5, 6, 7, "York"), result.SpyReport);
        Assert.Equal(result.SpyReport, campaign.State.LatestSpyReport);
        Assert.Equal(0, campaign.State.Player.ActiveSpies);
        Assert.Equal([0, OriginalStrategicMovement.PlayerAvatarMovementSlot],
            result.PlayerPass.Advances.Select(advance => advance.Slot));
        Assert.Equal([3], result.SchedulerPass!.Advances.Select(advance => advance.Slot));
        Assert.True(hostile.CurrentX > 100);
    }

    [Fact]
    public void ModalStrategicPassReportsAndAdvancesPlayersButDoesNotAdvanceHostiles()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        state.Player.ActiveSpies = 1;
        var hostile = strategic.MovementSlots[0];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 5;
        hostile.CurrentX = 100;
        hostile.CurrentY = 100;
        hostile.DestinationX = 1_000;
        hostile.DestinationY = 100;
        hostile.DirectionX = 1;

        var resources = new StubStrategicResources();
        resources.Routes["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)];
        var campaign = new Campaign(state);
        campaign.ConfigureOriginalStrategicResources(resources);
        var result = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                17,
                0,
                [
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                76,
                SchedulerBlockedByModal: true),
            new QueueStrategicRandom());

        Assert.NotNull(result.SpyReport);
        Assert.NotEmpty(result.PlayerPass.Advances);
        Assert.Null(result.SchedulerPass);
        Assert.Equal(100, hostile.CurrentX);
    }

    [Fact]
    public void ActivePlayerEncounterHandoffSuppressesOnlyTheContactOutput()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        state.Player.ArmyAt(0).Units[UnitType.Swordsmen] = 3;
        state.Player.SetArmyFieldState(0, fielded: true, location: 0);
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        player.PathComplete = true;
        player.CurrentX = 1_000;
        player.CurrentY = 2_000;
        player.GridX = 7;
        player.GridY = 11;
        var hostile = strategic.MovementSlots[0];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 5;
        hostile.CurrentX = 1_000;
        hostile.CurrentY = 2_000;
        hostile.DestinationX = 2_000;
        hostile.DestinationY = 2_000;
        hostile.DirectionX = 1;

        var resources = new StubStrategicResources();
        resources.Routes["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)];
        var campaign = new Campaign(state);
        campaign.ConfigureOriginalStrategicResources(resources);
        var result = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                17,
                0,
                [
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                76,
                PlayerEncounterHandoffActive: true),
            new QueueStrategicRandom());

        Assert.Empty(result.PlayerPass.Contacts);
        Assert.Empty(result.Encounters);
        Assert.Equal(0, strategic.SelectedPlayerMovementSlot);
        Assert.NotNull(result.SchedulerPass);
        Assert.True(hostile.CurrentX > 1_000);
    }

    [Fact]
    public void OriginalStrategicPassCapturesTheContactSixCounterHandoffBeforeTheScheduler()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        var army = state.Player.ArmyAt(0);
        army.Units[UnitType.Swordsmen] = 11;
        army.Units[UnitType.Halberdiers] = 12;
        army.Units[UnitType.Knights] = 13;
        state.Player.SetArmyFieldState(0, fielded: true, location: 0);
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        player.PathComplete = true;
        player.CurrentX = 1_000;
        player.CurrentY = 2_000;
        player.GridX = 7;
        player.GridY = 11;
        var hostile = strategic.MovementSlots[3];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 21;
        hostile.Halberdiers = 22;
        hostile.Knights = 23;
        hostile.CurrentX = 1_000;
        hostile.CurrentY = 2_000;
        hostile.DestinationX = 2_000;
        hostile.DestinationY = 2_000;
        hostile.DirectionX = 1;

        var resources = new StubStrategicResources();
        resources.Routes["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)];
        var campaign = new Campaign(state);
        campaign.ConfigureOriginalStrategicResources(resources);

        var result = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                17,
                0,
                [
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                76),
            new QueueStrategicRandom());

        Assert.Equal(new OriginalStrategicPlayerEnemyEncounter(
                0,
                3,
                new OriginalStrategicEncounterForces(11, 12, 13),
                new OriginalStrategicEncounterForces(21, 22, 23)),
            Assert.Single(result.Encounters));
        Assert.NotNull(result.SchedulerPass);
    }

    [Fact]
    public void StrategicEncounterStagingUsesTheOriginalCategoryDivisorsAndReservedPlayerForces()
    {
        var preparation = OriginalStrategicEncounterStaging.Prepare(
            new OriginalStrategicEncounterForces(10, 30, 62),
            new OriginalStrategicEncounterForces(1, 1, 100));

        Assert.Equal(new OriginalStrategicEncounterForces(10, 9, 41),
            preparation.PlayerResolverForces);
        Assert.Equal(new OriginalStrategicEncounterForces(0, 21, 21),
            preparation.PlayerReservedForces);
        Assert.Equal(new OriginalStrategicEncounterForces(1, 1, 86),
            preparation.EnemyResolverForces);
    }

    [Fact]
    public void StrategicEncounterStagingPreservesTheStrictReductionPlusOneBoundary()
    {
        var preparation = OriginalStrategicEncounterStaging.Prepare(
            new OriginalStrategicEncounterForces(22, 22, 22),
            new OriginalStrategicEncounterForces(3, 3, 60));

        Assert.Equal(new OriginalStrategicEncounterForces(20, 20, 20),
            preparation.PlayerResolverForces);
        Assert.Equal(new OriginalStrategicEncounterForces(2, 2, 2),
            preparation.PlayerReservedForces);
        Assert.Equal(new OriginalStrategicEncounterForces(3, 3, 58),
            preparation.EnemyResolverForces);
    }

    [Fact]
    public void StrategicEncounterAutomaticFallbackEliminatesOnlyTheStagedPlayerOnAScoreLoss()
    {
        var preparation = OriginalStrategicEncounterStaging.Prepare(
            new OriginalStrategicEncounterForces(10, 30, 62),
            new OriginalStrategicEncounterForces(1, 1, 100));

        var result = OriginalStrategicEncounterStaging.ResolveAutomatic(
            preparation, playerScoreModifier: 0, enemyScoreModifier: 0,
            new QueueEncounterRandom());

        Assert.False(result.PlayerWon);
        Assert.Equal(new OriginalStrategicEncounterForces(0, 0, 0), result.PlayerResolverSurvivors);
        Assert.Equal(new OriginalStrategicEncounterForces(0, 21, 21), result.PlayerFinalForces);
        Assert.Equal(preparation.EnemyResolverForces, result.EnemyFinalForces);
    }

    [Fact]
    public void StrategicEncounterAutomaticFallbackUsesOrderedRawRemaindersForPlayerLosses()
    {
        var preparation = OriginalStrategicEncounterStaging.Prepare(
            new OriginalStrategicEncounterForces(100, 100, 100),
            new OriginalStrategicEncounterForces(2, 3, 4));

        var result = OriginalStrategicEncounterStaging.ResolveAutomatic(
            preparation, playerScoreModifier: 0, enemyScoreModifier: 0,
            new QueueEncounterRandom(8, 3, 5));

        Assert.True(result.PlayerWon);
        Assert.Equal(new OriginalStrategicEncounterForces(12, 20, 20), result.PlayerResolverSurvivors);
        Assert.Equal(new OriginalStrategicEncounterForces(92, 100, 100), result.PlayerFinalForces);
        Assert.Equal(preparation.EnemyResolverForces, result.EnemyFinalForces);
    }

    [Fact]
    public void AutomaticStrategicEncounterWritesTheResolvedCountersAndRemovesAnEmptyOrdinaryFieldRecord()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        state.Player.ArmyAt(0).Units[UnitType.Swordsmen] = 1;
        state.Player.ArmyAt(0).Units[UnitType.Halberdiers] = 1;
        state.Player.ArmyAt(0).Units[UnitType.Knights] = 1;
        state.Player.ArmyAt(0).OriginalStrategicEncounterCount = 17;
        state.Player.ArmyAt(0).OriginalStrategicEncounterStates[UnitType.Swordsmen] = 4;
        state.Player.ArmyAt(0).OriginalStrategicEncounterStates[UnitType.Halberdiers] = 5;
        state.Player.ArmyAt(0).OriginalStrategicEncounterStates[UnitType.Knights] = 6;
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        player.PathComplete = true;
        strategic.ActivePlayerRecordCount = 1;
        var hostile = strategic.MovementSlots[3];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 2;
        hostile.Halberdiers = 3;
        hostile.Knights = 4;
        var campaign = new Campaign(state);
        var encounter = new OriginalStrategicPlayerEnemyEncounter(
            0, 3,
            new OriginalStrategicEncounterForces(1, 1, 1),
            new OriginalStrategicEncounterForces(2, 3, 4));

        var applied = campaign.ResolveAutomaticOriginalStrategicEncounter(
            encounter, playerScoreModifier: 0, new QueueEncounterRandom());

        Assert.True(applied.PlayerFieldRecordRemoved);
        Assert.False(applied.DistinguishedPlayerLossRequiresModal);
        Assert.False(player.Active);
        Assert.Equal(0, strategic.ActivePlayerRecordCount);
        Assert.Equal((0, 0, 0), (
            state.Player.ArmyAt(0).Units[UnitType.Swordsmen],
            state.Player.ArmyAt(0).Units[UnitType.Halberdiers],
            state.Player.ArmyAt(0).Units[UnitType.Knights]));
        Assert.Equal((2, 3, 4), (hostile.Swordsmen, hostile.Halberdiers, hostile.Knights));
        Assert.Equal(0, state.Player.ArmyAt(0).OriginalStrategicEncounterCount);
        Assert.All(state.Player.ArmyAt(0).OriginalStrategicEncounterStates.Values,
            stateValue => Assert.Equal(0, stateValue));
    }

    [Fact]
    public void AutomaticStrategicEncounterLeavesTheEmptyDistinguishedRecordForItsLossModal()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        strategic.EngagedPlayerMovementSlot = 0;
        state.Player.ArmyAt(0).Units[UnitType.Swordsmen] = 1;
        state.Player.ArmyAt(0).OriginalStrategicEncounterCount = 7;
        state.Player.ArmyAt(0).OriginalStrategicEncounterStates[UnitType.Swordsmen] = 3;
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        player.PathComplete = true;
        strategic.ActivePlayerRecordCount = 1;
        var hostile = strategic.MovementSlots[0];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 2;
        var campaign = new Campaign(state);
        var encounter = new OriginalStrategicPlayerEnemyEncounter(
            0, 0,
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(2, 0, 0));

        var applied = campaign.ResolveAutomaticOriginalStrategicEncounter(
            encounter, playerScoreModifier: 0, new QueueEncounterRandom());

        Assert.False(applied.PlayerFieldRecordRemoved);
        Assert.True(applied.DistinguishedPlayerLossRequiresModal);
        Assert.True(player.Active);
        Assert.Equal(1, strategic.ActivePlayerRecordCount);
        Assert.Equal(8, state.Player.ArmyAt(0).OriginalStrategicEncounterCount);
        Assert.All(state.Player.ArmyAt(0).OriginalStrategicEncounterStates.Values,
            stateValue => Assert.Equal(0, stateValue));
    }

    private sealed class QueueEncounterRandom(params int[] values) : IOriginalStrategicEncounterRandom
    {
        private readonly Queue<int> _values = new(values);

        public int NextRaw() => _values.Count > 0
            ? _values.Dequeue()
            : throw new InvalidOperationException("No queued encounter random value remains.");
    }

    [Fact]
    public void FieldingAnArmyConstructsItsMatchingOriginalMovementRecord()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 2);
        state.Player.ArmyAt(2).Units[UnitType.Swordsmen] = 100;
        var campaign = new Campaign(state);

        Assert.True(campaign.FieldArmy(2));

        var record = state.OriginalStrategicState.PlayerMovementSlots[2];
        Assert.True(record.Active);
        Assert.True(record.PathComplete);
        Assert.Equal(1, state.OriginalStrategicState.ActivePlayerRecordCount);
        Assert.Equal(2, state.OriginalStrategicState.SelectedPlayerMovementSlot);
    }
}
