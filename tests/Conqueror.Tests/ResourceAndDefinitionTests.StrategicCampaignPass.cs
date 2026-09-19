using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
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
