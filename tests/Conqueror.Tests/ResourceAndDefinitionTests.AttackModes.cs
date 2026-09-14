using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void KindZeroAttackReachesModeElevenBeforeApplyingDamage()
    {
        var battle = FriendlyModeBattle(actorKind: 0, retainerX: 4, enemyX: 5,
            retainerHealth: 12, enemyHealth: 20, retainerAttackSkill: 1_000);
        var retainer = Assert.Single(battle.Retainers);
        var enemy = Assert.Single(battle.Enemies);
        battle.CommandRetainers(SiegeRetainerCommand.Attack);

        battle.AdvanceRetainerOrders();

        Assert.Equal(8, retainer.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Walk, retainer.VisualState);
        Assert.Equal(20, enemy.Health);

        battle.AdvanceRetainerMovement(0.4001);
        Assert.Equal(20, enemy.Health);
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(11, retainer.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Attack, retainer.VisualState);
        Assert.Equal(20, enemy.Health);

        battle.AdvanceEnemyAnimations(0.3841);

        Assert.True(enemy.Health < 20);
    }

    [Fact]
    public void AttackModeEightRejectsTheExactRawContactBoundary()
    {
        var battle = FriendlyModeBattle(actorKind: 0, retainerX: 2, enemyX: 9,
            retainerHealth: 12, enemyHealth: 20, retainerAttackSkill: 1_000);
        var retainer = Assert.Single(battle.Retainers);
        var enemy = Assert.Single(battle.Enemies);
        var distance8 = 0x400;
        battle.ConfigureActorRaycast((_, target) => new SiegeActorRayHit(target, distance8));
        battle.CommandRetainers(SiegeRetainerCommand.Attack);
        battle.AdvanceRetainerOrders();
        battle.AdvanceRetainerMovement(0.6001);

        distance8 = 350;
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(6, retainer.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Walk, retainer.VisualState);
        Assert.Equal(20, enemy.Health);
    }

    [Fact]
    public void AttackModeEightUsesTheInterveningOpponentReturnedByItsRay()
    {
        var tiles = new SiegeTile[14, 5];
        var army = new Army();
        army.Units[UnitType.Halberdiers] = 1;
        var retainerSpawn = new SiegeSpawn(2, 2, false, OriginalArmor: 0,
            OriginalHealth: 12, OriginalCombatRow: 0, OriginalAttackSkill: 1_000,
            OriginalActorKind: 0, OriginalActorOrder: 0,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var aimedSpawn = new SiegeSpawn(10, 2, false, OriginalArmor: 0,
            OriginalHealth: 20, OriginalAttackSkill: 1, OriginalActorOrder: 1);
        var interveningSpawn = new SiegeSpawn(6, 2, false, OriginalArmor: 0,
            OriginalHealth: 20, OriginalAttackSkill: 1, OriginalActorOrder: 2);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [aimedSpawn, interveningSpawn],
                retainers: [retainerSpawn]));
        var retainer = Assert.Single(battle.Retainers);
        var aimed = battle.Enemies.Single(actor => actor.OriginalActorOrder == 1);
        var intervening = battle.Enemies.Single(actor => actor.OriginalActorOrder == 2);
        battle.ConfigureActorRaycast((_, target) => ReferenceEquals(target, aimed)
            ? new SiegeActorRayHit(intervening, 0x100)
            : new SiegeActorRayHit(target, 0x100));
        battle.ToggleRetainerSelection(retainer);
        Assert.True(battle.CommandSelectedRetainersAt(aimed));

        battle.AdvanceRetainerMovement(0);

        Assert.Equal(11, retainer.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Attack, retainer.VisualState);
        Assert.Equal(20, aimed.Health);
        Assert.Equal(20, intervening.Health);

        battle.AdvanceEnemyAnimations(0.3841);

        Assert.Equal(20, aimed.Health);
        Assert.True(intervening.Health < 20);
    }
}
