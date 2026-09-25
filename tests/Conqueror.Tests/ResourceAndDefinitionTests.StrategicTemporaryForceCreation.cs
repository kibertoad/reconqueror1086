using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-STRATEGY-016, FMT-STRATEGY-006.
public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicPassConsumesTheTwoMappedTemporaryCreatorActionsAfterSchedulerWork()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        var resources = new StubStrategicResources
        {
            Routes =
            {
                ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)],
                ["scot.rat"] = Enumerable.Range(0, 44)
                    .Select(index => new OriginalStrategicRoutePoint(6_656 + index, 405))
                    .ToArray(),
                ["wales.rat"] = Enumerable.Range(0, 42)
                    .Select(index => new OriginalStrategicRoutePoint(5_668 + index, 2_201))
                    .ToArray()
            }
        };
        var campaign = new Campaign(state);
        campaign.ConfigureOriginalStrategicResources(resources);

        var created = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                DivisionTargets: [
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                SchedulerBlockedByModal: true,
                TemporaryForceActionIds: [0x5D, 0x2B]),
            new QueueStrategicRandom(1, 0, 0, 1));

        Assert.Empty(created.TemporaryForcePass);
        Assert.Equal([
            new OriginalStrategicTemporaryForceCreation(0x2B, 1),
            new OriginalStrategicTemporaryForceCreation(0x5D, 2)
        ], created.TemporaryForceCreations);
        Assert.Equal((true, false, 44, 0, 1, 1, 0, 7, strategic.Properties[7].Lord, 2, 6_656, 405),
            (strategic.TemporaryForceSlots[1].Active, strategic.TemporaryForceSlots[1].PathComplete,
             strategic.TemporaryForceSlots[1].WaypointCount, strategic.TemporaryForceSlots[1].WaypointIndex,
             strategic.TemporaryForceSlots[1].Swordsmen, strategic.TemporaryForceSlots[1].Halberdiers,
             strategic.TemporaryForceSlots[1].Knights, strategic.TemporaryForceSlots[1].OriginProperty,
             strategic.TemporaryForceSlots[1].Lord, strategic.TemporaryForceSlots[1].Mode,
             (int)strategic.TemporaryForceSlots[1].CurrentX, (int)strategic.TemporaryForceSlots[1].CurrentY));
        Assert.Equal((0, 2, 0), (strategic.TemporaryForceSlots[2].Swordsmen,
            strategic.TemporaryForceSlots[2].Halberdiers, strategic.TemporaryForceSlots[2].Knights));

        var repeated = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                [new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                SchedulerBlockedByModal: true,
                TemporaryForceActionIds: [0x2B, 0x5D]),
            new QueueStrategicRandom());
        Assert.Empty(repeated.TemporaryForceCreations);
        Assert.Equal((1, 1, 0), (strategic.TemporaryForceSlots[1].Swordsmen,
            strategic.TemporaryForceSlots[1].Halberdiers, strategic.TemporaryForceSlots[1].Knights));
        Assert.Equal((0, 2, 0), (strategic.TemporaryForceSlots[2].Swordsmen,
            strategic.TemporaryForceSlots[2].Halberdiers, strategic.TemporaryForceSlots[2].Knights));

        var advanced = campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                [new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0),
                    new OriginalStrategicPlayerTarget(false, 0, 0)],
                SchedulerBlockedByModal: true),
            new QueueStrategicRandom());

        Assert.Equal([1, 2], advanced.TemporaryForcePass.Select(advance => advance.Slot));
        Assert.Empty(advanced.TemporaryForceCreations);
    }

    [Fact]
    public void TemporaryForceExpiryRunsAfterItsMovementAndUsesTheIndependentMonthAndYearGates()
    {
        var state = Campaign.NewFromTemplate(0);
        state.Date = new DateTime(2000, 2, 1);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        var force = strategic.TemporaryForceSlots[1];
        force.Active = true;
        force.WaypointCount = 44;
        force.Swordsmen = 1;
        force.OriginProperty = 7;
        force.Lord = strategic.Properties[7].Lord;
        force.Mode = OriginalStrategicMovement.RoutedMode;
        var resources = new StubStrategicResources
        {
            Routes =
            {
                ["sc_0.rat"] = [new OriginalStrategicRoutePoint(0, 0)],
                ["scot.rat"] = Enumerable.Range(0, 44)
                    .Select(index => new OriginalStrategicRoutePoint(index + 1, 0)).ToArray()
            }
        };
        var campaign = new Campaign(state);
        campaign.ConfigureOriginalStrategicResources(resources);

        var expired = Assert.Single(campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput([
                new OriginalStrategicPlayerTarget(false, 0, 0),
                new OriginalStrategicPlayerTarget(false, 0, 0),
                new OriginalStrategicPlayerTarget(false, 0, 0)], SchedulerBlockedByModal: true),
            new QueueStrategicRandom()).TemporaryForcePass);

        Assert.Equal((1, false, false, true),
            (expired.Slot, expired.Looped, expired.CompletionSignal, expired.ExpirationSignal));
        Assert.Equal((false, 44, 1, 0.8f, 0f),
            (force.Active, force.WaypointCount, force.WaypointIndex, force.DirectionX, force.DirectionY));

        state.Date = new DateTime(2001, 1, 1);
        force.Active = true;
        force.WaypointCount = 44;
        var retained = Assert.Single(campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput([
                new OriginalStrategicPlayerTarget(false, 0, 0),
                new OriginalStrategicPlayerTarget(false, 0, 0),
                new OriginalStrategicPlayerTarget(false, 0, 0)], SchedulerBlockedByModal: true),
            new QueueStrategicRandom()).TemporaryForcePass);
        Assert.False(retained.ExpirationSignal);
        Assert.True(force.Active);
    }
}
