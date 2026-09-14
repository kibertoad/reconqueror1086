using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void FollowModeSixteenAlwaysTransitionsToModeSeventeenWandering()
    {
        var battle = RetainerOrderBattle(retainerX: 5, enemyX: 10);
        var retainer = Assert.Single(battle.Retainers);
        var predicateCalls = 0;
        retainer.Facing = Facing.South;
        battle.ConfigureActorRaycast((_, target) =>
        {
            predicateCalls++;
            return new SiegeActorRayHit(target, 0x1ff);
        });

        battle.CommandRetainers(SiegeRetainerCommand.Follow);

        battle.AdvanceRetainerOrders();

        Assert.Equal(1, predicateCalls);
        Assert.Equal(17, retainer.ActorMode);
        Assert.Equal(Facing.South, retainer.Facing);
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((0, 64), (retainer.OffsetX8, retainer.OffsetY8));
    }

    [Fact]
    public void FollowModeSeventeenRequiresTheExactPlayerBelowTheStrictDepthLimit()
    {
        var battle = RetainerOrderBattle(retainerX: 5, enemyX: 10);
        var retainer = Assert.Single(battle.Retainers);
        var returnedActor = battle.Enemies[0];
        var returnedDepth = 0x100;
        battle.ConfigureActorRaycast((_, _) => new SiegeActorRayHit(returnedActor, returnedDepth));
        battle.CommandRetainers(SiegeRetainerCommand.Follow);
        battle.AdvanceRetainerOrders();

        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        Assert.Equal(17, retainer.ActorMode);

        battle.AdvanceRetainerMovement(0.6001);
        returnedActor = battle.PlayerActor;
        returnedDepth = 0x7fff;
        battle.AdvanceRetainerMovement(0);
        Assert.Equal(17, retainer.ActorMode);

        battle.AdvanceRetainerMovement(0.6001);
        returnedDepth = 0x7ffe;
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(16, retainer.ActorMode);
        Assert.Equal(Facing.West, retainer.Facing);
    }
}
