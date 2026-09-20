using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicMapHitTestUsesOrderedSourceRectangles()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.CurrentX = 100.9f;
        player.CurrentY = 200.9f;
        var enemy = state.MovementSlots[0];
        enemy.Active = true;
        enemy.PathComplete = true;
        enemy.Mode = OriginalStrategicMovement.DirectPropertyMode;
        enemy.OriginProperty = 0;
        enemy.Lord = 0;
        enemy.Swordsmen = 1;
        enemy.CurrentX = 120.9f;
        enemy.CurrentY = 200.9f;
        var divisions = new[]
        {
            new OriginalStrategicPlayerTarget(true, 120.9f, 200.9f),
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0)
        };

        Assert.Equal(new OriginalStrategicPlayerMapHit(0, null, null),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 120, 180));

        player.Active = false;
        Assert.Equal(new OriginalStrategicPlayerMapHit(null, 0, null),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 120, 180));

        enemy.Active = false;
        Assert.Equal(new OriginalStrategicPlayerMapHit(null, null, 0),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 120, 180));
    }

    [Fact]
    public void StrategicMapHitTestRetainsHalfOpenSourceBounds()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.CurrentX = 100.9f;
        player.CurrentY = 200.9f;
        var divisions = new[]
        {
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0)
        };

        Assert.Equal(new OriginalStrategicPlayerMapHit(0, null, null),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 111, 170));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 110, 170));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 135, 170));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 111, 169));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 111, 201));
        Assert.Throws<ArgumentException>(() => OriginalStrategicMapHitTesting.HitTest(
            state, divisions[..2], 111, 170));
    }
}
