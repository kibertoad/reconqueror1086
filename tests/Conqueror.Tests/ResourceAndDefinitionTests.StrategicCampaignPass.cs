using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
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
