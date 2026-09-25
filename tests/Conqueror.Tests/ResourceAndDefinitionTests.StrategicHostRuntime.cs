using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-STRATEGY-001, RULE-STRATEGY-019.
public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void HostFixedPassCreatesBothPatrolsFromTheConversationActionVariables()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        state.ConversationVariables.AddRange(Enumerable.Repeat(0, 94));
        state.ConversationVariables[0x2B] = 2;
        state.ConversationVariables[0x5D] = -1;
        var campaign = new Campaign(state, seed: 42);
        campaign.ConfigureOriginalStrategicResources(new StubStrategicResources
        {
            Routes =
            {
                ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)],
                ["scot.rat"] = Enumerable.Range(0, 44)
                    .Select(index => new OriginalStrategicRoutePoint(6_656 - index, 405)).ToArray(),
                ["wales.rat"] = Enumerable.Range(0, 42)
                    .Select(index => new OriginalStrategicRoutePoint(5_668 - index, 2_201)).ToArray()
            }
        });

        var created = OriginalStrategicHostRuntime.AdvanceFixedPass(
            campaign, schedulerBlockedByModal: true);

        Assert.NotNull(created);
        Assert.Equal([
            new OriginalStrategicTemporaryForceCreation(0x2B, 1),
            new OriginalStrategicTemporaryForceCreation(0x5D, 2)
        ], created!.TemporaryForceCreations);
        Assert.True(state.OriginalStrategicState.TemporaryForceSlots[1].Active);
        Assert.True(state.OriginalStrategicState.TemporaryForceSlots[2].Active);

        var repeated = OriginalStrategicHostRuntime.AdvanceFixedPass(
            campaign, schedulerBlockedByModal: true);
        Assert.Empty(repeated!.TemporaryForceCreations);
    }

    [Fact]
    public void HostFixedStrategicPassUsesTheCampaignStreamAndDoesNotInventMigratedFallbacks()
    {
        Assert.Equal(TimeSpan.FromSeconds(1d / 60d), OriginalStrategicHostRuntime.FixedCadence);

        var newState = Campaign.NewFromTemplate(0);
        newState.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            newState.Date, startingRouteSelector: 0);
        var newCampaign = new Campaign(newState, seed: 42);
        var resources = new StubStrategicResources
        {
            Routes = { ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)] }
        };
        newCampaign.ConfigureOriginalStrategicResources(resources);

        var first = OriginalStrategicHostRuntime.AdvanceFixedPass(newCampaign);
        var second = OriginalStrategicHostRuntime.AdvanceFixedPass(newCampaign);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotNull(first!.SchedulerPass);
        Assert.NotNull(second!.SchedulerPass);
        Assert.Equal(2, newState.OriginalStrategicState.GenerationAccumulator);

        var distinguished = newState.OriginalStrategicState.PlayerMovementSlots[5];
        distinguished.PathComplete = true;
        distinguished.GridX = 63;
        distinguished.GridY = 114;
        var entry = OriginalStrategicHostRuntime.AdvanceFixedPass(newCampaign);
        var afterWithdrawal = OriginalStrategicHostRuntime.AdvanceFixedPass(
            newCampaign, suppressDragonEntry: true);
        Assert.True(entry!.PlayerPass.DragonEntryTriggered);
        Assert.Null(entry.SchedulerPass);
        Assert.False(afterWithdrawal!.PlayerPass.DragonEntryTriggered);
        Assert.NotNull(afterWithdrawal.SchedulerPass);
        Assert.Equal(3, newState.OriginalStrategicState.GenerationAccumulator);

        var migratedState = Campaign.NewFromTemplate(0);
        migratedState.OriginalStrategicState = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            migratedState.Date, OriginalStrategicMovement.InitialSpeedMultiplier);
        var migratedCampaign = new Campaign(migratedState, seed: 42);
        migratedCampaign.ConfigureOriginalStrategicResources(new StubStrategicResources());

        var migrated = OriginalStrategicHostRuntime.AdvanceFixedPass(migratedCampaign);

        Assert.NotNull(migrated);
        Assert.Null(migrated!.SchedulerPass);
        Assert.Equal(0, migratedState.OriginalStrategicState.GenerationAccumulator);
    }

    [Fact]
    public void HostFixedStrategicPassBlocksFurtherStrategicWorkWhileAContactIsHandedToTheResolver()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var campaign = new Campaign(state, seed: 42);
        campaign.ConfigureOriginalStrategicResources(new StubStrategicResources
        {
            Routes = { ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)] }
        });

        var pass = OriginalStrategicHostRuntime.AdvanceFixedPass(campaign, playerEncounterHandoffActive: true);

        Assert.NotNull(pass);
        Assert.Null(pass!.SchedulerPass);
        Assert.Equal(0, state.OriginalStrategicState.GenerationAccumulator);
    }

    [Fact]
    public void HostFixedStrategicPassKeepsReportingAndPlayerUpdatesRunningDuringAModal()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        state.Player.ActiveSpies = 1;
        var strategic = state.OriginalStrategicState;
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
        var campaign = new Campaign(state, seed: 42);
        campaign.ConfigureOriginalStrategicResources(new StubStrategicResources
        {
            Routes = { ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)] }
        });

        var pass = OriginalStrategicHostRuntime.AdvanceFixedPass(
            campaign, schedulerBlockedByModal: true);

        Assert.NotNull(pass);
        Assert.NotNull(pass!.SpyReport);
        Assert.NotEmpty(pass.PlayerPass.Advances);
        Assert.Null(pass.SchedulerPass);
        Assert.Equal(0, state.OriginalStrategicState.GenerationAccumulator);
    }

    [Fact]
    public void HostFixedStrategicPassAdvancesTemporaryPatrolsBeforeUsingTheirTargets()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        player.PathComplete = false;
        player.TargetHandle = OriginalStrategicMovement.PlayerDivisionTargetFlag | 1;
        player.CurrentX = 100;
        player.CurrentY = 200;
        var force = strategic.TemporaryForceSlots[1];
        force.Active = true;
        force.Swordsmen = 5;
        force.WaypointCount = 44;
        force.OriginProperty = 7;
        force.Lord = strategic.Properties[7].Lord;
        force.Mode = OriginalStrategicMovement.RoutedMode;
        force.CurrentX = 0;
        force.CurrentY = 0;

        var campaign = new Campaign(state, seed: 42);
        campaign.ConfigureOriginalStrategicResources(new StubStrategicResources
        {
            Routes =
            {
                ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)],
                ["scot.rat"] = Enumerable.Range(0, 44)
                    .Select(index => new OriginalStrategicRoutePoint(index * 10, 0))
                    .ToArray()
            }
        });

        var pass = OriginalStrategicHostRuntime.AdvanceFixedPass(
            campaign, schedulerBlockedByModal: true);

        Assert.NotNull(pass);
        Assert.Equal(1, Assert.Single(pass!.TemporaryForcePass).Slot);
        Assert.Equal((1, 0), (player.DestinationX, player.DestinationY));
        Assert.Equal(1f, force.CurrentX);
        Assert.Null(pass.SchedulerPass);
    }
}
