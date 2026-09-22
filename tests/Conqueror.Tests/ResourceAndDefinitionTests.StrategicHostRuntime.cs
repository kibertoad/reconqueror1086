using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
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
    public void HostFixedStrategicPassUsesPersistedTemporaryForceTargets()
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
        force.CurrentX = 400.9f;
        force.CurrentY = 600.9f;

        var campaign = new Campaign(state, seed: 42);
        campaign.ConfigureOriginalStrategicResources(new StubStrategicResources
        {
            Routes = { ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)] }
        });

        var pass = OriginalStrategicHostRuntime.AdvanceFixedPass(
            campaign, schedulerBlockedByModal: true);

        Assert.NotNull(pass);
        Assert.Equal((400, 600), (player.DestinationX, player.DestinationY));
        Assert.Null(pass!.SchedulerPass);
    }
}
