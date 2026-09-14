using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void KindZeroDefendEntersModeElevenAndDefersItsStrike()
    {
        var battle = FriendlyModeBattle(actorKind: 0, retainerX: 4, enemyX: 5,
            retainerHealth: 12, enemyHealth: 20, retainerAttackSkill: 1_000);
        var retainer = Assert.Single(battle.Retainers);
        var enemy = Assert.Single(battle.Enemies);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);

        battle.AdvanceRetainerOrders();

        Assert.Equal(11, retainer.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Attack, retainer.VisualState);
        Assert.Equal(20, enemy.Health);

        battle.AdvanceEnemyAnimations(0.3841);

        Assert.True(enemy.Health < 20);
    }

    [Fact]
    public void KindOneDefendEscapesAnAdjacentOpponentAtThreeHalvesSpeed()
    {
        var battle = FriendlyModeBattle(actorKind: 1, retainerX: 4, enemyX: 5,
            retainerHealth: 12, enemyHealth: 20);
        var retainer = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);

        battle.AdvanceRetainerOrders();

        Assert.Equal(10, retainer.ActorMode);
        Assert.Equal(Facing.West, retainer.Facing);
        battle.AdvanceRetainerOrders();
        Assert.Equal(10, retainer.ActorMode);
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((4, 2, -96, 0),
            (retainer.X, retainer.Y, retainer.OffsetX8, retainer.OffsetY8));
    }

    [Fact]
    public void KindZeroDefendEvaluatesOnlyOneFormationPredicatePerPass()
    {
        var battle = FriendlyModeBattle(actorKind: 0, retainerX: 6, enemyX: 10,
            retainerHealth: 12, enemyHealth: 20);
        var retainer = Assert.Single(battle.Retainers);
        battle.ConfigureActorRaycast((source, target) =>
            ReferenceEquals(source, retainer) && ReferenceEquals(target, battle.PlayerActor)
                ? new SiegeActorRayHit(target, 0x180)
                : null);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);

        battle.AdvanceRetainerOrders();
        Assert.Equal(1, retainer.ActorMode);

        battle.AdvanceRetainerOrders();
        Assert.Equal(3, retainer.ActorMode);

        battle.AdvanceRetainerOrders();
        Assert.Equal(7, retainer.ActorMode);
        Assert.Equal(Facing.West, retainer.Facing);
    }

    [Fact]
    public void KindOneDefendUsesItsDistinctModeOneSupportTransition()
    {
        var battle = FriendlyModeBattle(actorKind: 1, retainerX: 2, enemyX: 10,
            retainerHealth: 12, enemyHealth: 20, playerX: 1);
        var retainer = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);

        battle.AdvanceRetainerOrders();
        Assert.Equal(1, retainer.ActorMode);

        battle.AdvanceRetainerOrders();
        Assert.Equal(4, retainer.ActorMode);
    }

    private static SiegeSession FriendlyModeBattle(int actorKind, int retainerX, int enemyX,
        int retainerHealth, int enemyHealth, int retainerAttackSkill = 50, int playerX = 1)
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[UnitType.Halberdiers] = 1;
        var retainer = new SiegeSpawn(retainerX, 2, false, OriginalArmor: 0,
            OriginalHealth: retainerHealth, OriginalCombatRow: 0,
            OriginalAttackSkill: retainerAttackSkill, OriginalActorKind: actorKind,
            OriginalActorOrder: 1, OriginalActorTemplate: actorKind == 1 ? 2 : 0,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384),
            OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142));
        var enemy = new SiegeSpawn(enemyX, 2, false, OriginalArmor: 0,
            OriginalHealth: enemyHealth, OriginalCombatRow: 0, OriginalAttackSkill: 1,
            OriginalActorKind: 2, OriginalActorOrder: 2, OriginalActorTemplate: 3);
        return new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, playerX, 2, Facing.East, [enemy], retainers: [retainer]));
    }
}
