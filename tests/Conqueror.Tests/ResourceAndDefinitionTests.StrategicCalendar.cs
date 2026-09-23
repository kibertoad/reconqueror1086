using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData(2, 28, StrategicTerrainProfile.Spring)]
    [InlineData(5, 31, StrategicTerrainProfile.Summer)]
    [InlineData(8, 31, StrategicTerrainProfile.Autumn)]
    [InlineData(11, 30, StrategicTerrainProfile.Winter)]
    [InlineData(12, 31, StrategicTerrainProfile.Winter)]
    public void CampaignCalendarUpdatesTheLiveStrategicTerrainProfileAtMonthBoundaries(
        int month, int day, StrategicTerrainProfile expected)
    {
        var state = Campaign.NewFromTemplate(0);
        state.Date = new DateTime(1086, month, day);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var campaign = new Campaign(state, seed: 42);

        campaign.AdvanceDays(1);

        Assert.Equal(expected, state.OriginalStrategicState.TerrainProfile);
        Assert.Equal(new DateTime(1086, month, day).AddDays(1), state.Date);
    }

    [Fact]
    public void MapSpeedControlChangesTheLiveMovementRateAndCompatibilityClockTogether()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var campaign = new Campaign(state);

        Assert.Equal(2, campaign.AdjustStrategicSpeed(1));
        Assert.Equal(2, state.OriginalStrategicState.SpeedMultiplier);
        Assert.Equal(2, state.DaySpeed);
        Assert.Equal(OriginalStrategicMovement.MaximumSpeedMultiplier,
            campaign.AdjustStrategicSpeed(int.MaxValue));
        Assert.Equal(OriginalStrategicMovement.MinimumSpeedMultiplier,
            campaign.AdjustStrategicSpeed(int.MinValue));
        Assert.Equal(state.OriginalStrategicState.SpeedMultiplier, state.DaySpeed);
    }
}
