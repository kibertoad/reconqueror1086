using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void PatrolContactScansPhysicalForceThenPlayerOrderAtStrictPreRoutePosition()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 1, 1), 0);
        var first = state.TemporaryForceSlots[0];
        first.Active = true;
        first.CurrentX = 100;
        first.CurrentY = 200;
        var second = state.TemporaryForceSlots[1];
        second.Active = true;
        second.CurrentX = 100;
        second.CurrentY = 200;
        var army1 = state.PlayerMovementSlots[1];
        army1.Active = true;
        army1.PathComplete = true;
        army1.CurrentX = 129.999f;
        army1.CurrentY = 200;
        var army0 = state.PlayerMovementSlots[0];
        army0.Active = true;
        army0.PathComplete = true;
        army0.CurrentX = 130;
        army0.CurrentY = 200;

        var contact = OriginalStrategicMovement.AdvanceTemporaryForceContactPass(
            state, new StubStrategicResources(), new DateTime(1086, 1, 1));

        Assert.Equal(new OriginalStrategicTemporaryForceContact(0, 1), contact.Contact);
        Assert.Empty(contact.Advances);
        Assert.Equal(1, state.SelectedPlayerMovementSlot);
        Assert.Equal(100, first.CurrentX);

        army0.CurrentX = 100;
        var ordered = OriginalStrategicMovement.AdvanceTemporaryForceContactPass(
            state, new StubStrategicResources(), new DateTime(1086, 1, 1));
        Assert.Equal(new OriginalStrategicTemporaryForceContact(0, 0), ordered.Contact);
    }

    [Fact]
    public void PatrolAvatarNoticeUsesSharedCooldownAndContinuesToLaterArmyContact()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 1, 1), 0);
        var patrol = state.TemporaryForceSlots[0];
        patrol.Active = true;
        var avatar = state.PlayerMovementSlots[OriginalStrategicMovement.PlayerAvatarMovementSlot];
        avatar.CurrentX = 0;
        avatar.CurrentY = 0;

        var first = OriginalStrategicMovement.AdvanceTemporaryForceContactPass(
            state, new StubStrategicResources(), new DateTime(1086, 1, 1));
        Assert.Equal(new OriginalStrategicTemporaryForceAvatarNotice(0, 5),
            Assert.Single(first.AvatarNotices));
        Assert.Equal(120, avatar.CollisionCooldown);
        var second = OriginalStrategicMovement.AdvanceTemporaryForceContactPass(
            state, new StubStrategicResources(), new DateTime(1086, 1, 1));
        Assert.Empty(second.AvatarNotices);

        var army = state.PlayerMovementSlots[0];
        army.Active = true;
        army.PathComplete = true;
        var third = OriginalStrategicMovement.AdvanceTemporaryForceContactPass(
            state, new StubStrategicResources(), new DateTime(1086, 1, 1));
        Assert.Equal(new OriginalStrategicTemporaryForceContact(0, 0), third.Contact);
    }

    [Fact]
    public void PatrolAutomaticResolverUsesAllDirectArmyCountersAndAwardsSourceVariable()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(state.Date, 0);
        var strategic = state.OriginalStrategicState;
        var patrol = strategic.TemporaryForceSlots[1];
        patrol.Active = true;
        patrol.CurrentX = 1_000;
        patrol.CurrentY = 2_000;
        patrol.Swordsmen = 1;
        var playerRecord = strategic.PlayerMovementSlots[0];
        playerRecord.Active = true;
        playerRecord.PathComplete = false;
        playerRecord.Waypoints.Add(new OriginalStrategicRoutePoint(1_050, 2_000));
        playerRecord.WaypointCount = 1;
        playerRecord.CurrentX = 1_000.75f;
        playerRecord.CurrentY = 2_000;
        var army = state.Player.ArmyAt(0);
        army.Units[UnitType.Swordsmen] = 100;
        army.Units[UnitType.Halberdiers] = 100;
        army.Units[UnitType.Knights] = 100;
        var campaign = new Campaign(state);
        var resources = new StubStrategicResources();
        resources.Routes["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)];
        campaign.ConfigureOriginalStrategicResources(resources);

        var pass = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput([
                new(false, 0, 0), new(false, 0, 0), new(false, 0, 0)]),
            new QueueStrategicRandom());
        var encounter = Assert.IsType<OriginalStrategicTemporaryForceEncounter>(pass.TemporaryForceEncounter);
        Assert.Equal(new OriginalStrategicEncounterForces(100, 100, 100),
            encounter.Preparation.PlayerResolverForces);
        Assert.Equal(new OriginalStrategicEncounterForces(0, 0, 0),
            encounter.Preparation.PlayerReservedForces);
        Assert.Empty(pass.PlayerPass.Advances);
        Assert.Empty(pass.TemporaryForcePass);

        var settled = campaign.ResolveAutomaticOriginalStrategicPatrolEncounter(
            encounter, new QueueEncounterRandom(0, 0, 0), new QueueStrategicRandom(17));
        Assert.True(settled.PatrolCleared);
        Assert.Equal(40, settled.Reward);
        Assert.False(patrol.Active);
        Assert.True(patrol.EncounterResultMarked);
        Assert.Equal(40, state.ConversationVariables[17]);
        Assert.Equal(1, state.ConversationVariables[24]);
        Assert.Equal(1, state.ConversationVariables[5]);
        Assert.Equal(100, army.Units[UnitType.Swordsmen]);
        Assert.True(playerRecord.PathComplete);
        Assert.Equal((0, 0, 1_000, 2_000),
            (playerRecord.WaypointCount, playerRecord.WaypointIndex,
                playerRecord.DestinationX, playerRecord.DestinationY));
    }

    [Fact]
    public void PatrolLossRemovesAnEmptyOrdinaryFieldRecordAndCountsTheFailure()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(state.Date, 0);
        var strategic = state.OriginalStrategicState;
        var patrol = strategic.TemporaryForceSlots[1];
        patrol.Active = true;
        patrol.Swordsmen = 5;
        var playerRecord = strategic.PlayerMovementSlots[0];
        playerRecord.Active = true;
        playerRecord.PathComplete = true;
        strategic.ActivePlayerRecordCount = 1;
        var army = state.Player.ArmyAt(0);
        army.Units[UnitType.Swordsmen] = 1;
        var campaign = new Campaign(state);
        var encounter = new OriginalStrategicTemporaryForceEncounter(1, 0,
            new(1, 0, 0), new(5, 0, 0));

        var settlement = campaign.ResolveAutomaticOriginalStrategicPatrolEncounter(
            encounter, new QueueEncounterRandom(), new QueueStrategicRandom());

        Assert.False(settlement.PatrolCleared);
        Assert.True(settlement.PlayerFieldRecordRemoved);
        Assert.False(settlement.DistinguishedPlayerLossRequiresModal);
        Assert.False(playerRecord.Active);
        Assert.True(patrol.Active);
        Assert.True(patrol.EncounterResultMarked);
        Assert.Equal(1, state.ConversationVariables[25]);
        Assert.Equal(1, army.Units[UnitType.Swordsmen]);
    }
}
