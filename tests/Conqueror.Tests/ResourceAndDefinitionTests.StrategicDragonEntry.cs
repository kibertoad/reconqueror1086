using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void DragonMapEntryDefersTheHostileSchedulerUntilTheBattleRouteReturns()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            state.Date, startingRouteSelector: 0);
        var strategic = state.OriginalStrategicState;
        strategic.EngagedPlayerMovementSlot = 3;
        var distinguished = strategic.PlayerMovementSlots[3];
        distinguished.Active = true;
        distinguished.PathComplete = true;
        distinguished.GridX = 63;
        distinguished.GridY = 114;
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
            new OriginalStrategicCampaignPassInput([
                new OriginalStrategicPlayerTarget(false, 0, 0),
                new OriginalStrategicPlayerTarget(false, 0, 0),
                new OriginalStrategicPlayerTarget(false, 0, 0)]),
            new QueueStrategicRandom());

        Assert.True(result.PlayerPass.DragonEntryTriggered);
        Assert.Empty(result.Encounters);
        Assert.Null(result.SchedulerPass);
        Assert.Equal(100, hostile.CurrentX);
    }
}
