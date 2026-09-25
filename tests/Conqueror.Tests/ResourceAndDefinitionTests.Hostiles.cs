using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-ASSAULT-007, RULE-ASSAULT-008, RULE-ASSAULT-010, RULE-ASSAULT-011, RULE-ASSAULT-012, RULE-ASSAULT-013, RULE-ASSAULT-015, RULE-ASSAULT-016, RULE-ASSAULT-018, RULE-ASSAULT-020, RULE-ASSAULT-022, DEV-ASSAULT-001.
public sealed partial class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData(3, 2, 6, 8, 6)]
    [InlineData(5, 4, 4, 8, 1)]
    [InlineData(8, 2, 6, 8, 4)]
    [InlineData(9, 7, 4, 8, 4)]
    public void OfficialHostilesPreserveTemplateKindAndInitialModeProfile(
        int template, int kind, int current, int requested, int previous)
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 2, enemyHealth: 10,
            retainerHealth: 5, actorTemplate: template);

        var hostile = Assert.Single(battle.Enemies);
        Assert.Equal(template, hostile.OriginalActorTemplate);
        Assert.Equal(kind, hostile.OriginalActorKind);
        Assert.Equal(new OriginalActorModeProfile(current, requested, previous), hostile.OriginalModeProfile);
        Assert.Equal(current, hostile.ActorMode);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(9)]
    public void SupportedHostileModeNineLeavesForModeSixOnSameSideRay(int template)
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[UnitType.Halberdiers] = 1;
        var kind = OriginalCombatantTemplates.ActorKindForSceneTemplate(template);
        var first = new SiegeSpawn(5, 2, false, OriginalHealth: 10,
            OriginalActorKind: kind, OriginalActorTemplate: template, OriginalActorOrder: 1,
            OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142));
        var second = new SiegeSpawn(8, 2, false, OriginalHealth: 10,
            OriginalActorKind: kind, OriginalActorTemplate: template, OriginalActorOrder: 2);
        var retainer = new SiegeSpawn(2, 2, false, OriginalHealth: 5,
            OriginalActorKind: 0, OriginalActorOrder: 0);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [first, second], retainers: [retainer]));
        var hostile = battle.Enemies[0];
        var ally = battle.Enemies[1];
        typeof(SiegeEnemy).GetProperty(nameof(SiegeEnemy.ActorMode))!.SetValue(hostile, 9);
        battle.ConfigureActorRaycast((source, target) =>
            ReferenceEquals(source, hostile) && ReferenceEquals(target, ally)
                ? new SiegeActorRayHit(ally, 0x180)
                : null);

        battle.AdvanceHostileMovement(0);

        Assert.Equal(6, hostile.ActorMode);
        Assert.Equal((5, 2, 0, 0),
            (hostile.X, hostile.Y, hostile.OffsetX8, hostile.OffsetY8));
        battle.AdvanceHostileMovement(0.2001);
        Assert.Equal(64, Math.Abs(hostile.OffsetX8) + Math.Abs(hostile.OffsetY8));
    }

    [Theory]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(9)]
    public void SupportedHostileModeNineFailureEntersModeOneForOnePass(int template)
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 2, enemyHealth: 10,
            retainerHealth: 5, actorTemplate: template);
        var hostile = Assert.Single(battle.Enemies);
        typeof(SiegeEnemy).GetProperty(nameof(SiegeEnemy.ActorMode))!.SetValue(hostile, 9);
        battle.ConfigureActorRaycast((_, _) => null);

        battle.AdvanceHostileMovement(0);

        Assert.Equal(1, hostile.ActorMode);
        Assert.Equal((5, 2, 0, 0),
            (hostile.X, hostile.Y, hostile.OffsetX8, hostile.OffsetY8));
    }

    [Fact]
    public void HostileModeSixAcquiresAndPursuesWithExactSubCellMovement()
    {
        var battle = HostileBattle(enemyX: 6, retainerX: 2, enemyHealth: 10, retainerHealth: 5);
        var hostile = Assert.Single(battle.Enemies);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(8, hostile.ActorMode);
        Assert.Equal(Facing.West, hostile.Facing);

        battle.AdvanceHostileMovement(0.2001);
        Assert.Equal((6, 2, -64, 0), (hostile.X, hostile.Y, hostile.OffsetX8, hostile.OffsetY8));
    }

    [Fact]
    public void HostileDirectHandlerCardinalizesANonCardinalAcquisitionHeading()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 3, enemyHealth: 10,
            retainerHealth: 5, retainerY: 1);
        var hostile = Assert.Single(battle.Enemies);

        battle.AdvanceHostileMovement(0);

        Assert.Equal(8, hostile.ActorMode);
        Assert.Equal(0xC0, hostile.OriginalHeading8);
        Assert.Equal(Facing.West, hostile.Facing);
    }

    [Fact]
    public void HostileModeElevenDefersDamageUntilStrictEffectCompletion()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 4, enemyHealth: 10,
            retainerHealth: 5, enemyAttackSkill: 1_000);
        var hostile = Assert.Single(battle.Enemies);
        var retainer = Assert.Single(battle.Retainers);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(8, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Walk, hostile.VisualState);
        battle.AdvanceHostileMovement(0.4001);
        Assert.Equal(11, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Attack, hostile.VisualState);
        Assert.Equal(5, retainer.Health);

        battle.AdvanceEnemyAnimations(0.384);
        Assert.Equal(5, retainer.Health);
        battle.AdvanceEnemyAnimations(0.0001);

        Assert.True(retainer.Health < 5);
        Assert.Equal(SiegeEnemyVisualState.Attack, hostile.VisualState);
    }

    [Fact]
    public void HitLowHealthHostileEscapesItsStrongerTargetAtThreeHalvesSpeed()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 4, enemyHealth: 5, retainerHealth: 12,
            enemyArmor: 10, retainerAttackSkill: 1_000);
        var hostile = Assert.Single(battle.Enemies);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(8, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Walk, hostile.VisualState);
        battle.CommandRetainers(SiegeRetainerCommand.Attack);
        battle.AdvanceRetainerOrders();
        battle.AdvanceRetainerMovement(0.4001);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceEnemyAnimations(1.0001);
        Assert.Equal(13, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Hit, hostile.VisualState);
        Assert.Equal(5, hostile.Health);
        battle.AdvanceEnemyAnimations(0.3841);
        Assert.Equal(10, hostile.ActorMode);
        Assert.Equal(Facing.East, hostile.Facing);
        var heading = Assert.IsType<int>(hostile.OriginalHeading8);
        var beforeX8 = (hostile.X << 8) + hostile.OffsetX8;
        var beforeY8 = (hostile.Y << 8) + hostile.OffsetY8;
        battle.AdvanceHostileMovement(0.2001);

        Assert.Equal(OriginalActorMotion.Rotate(96, 0, heading),
            ((hostile.X << 8) + hostile.OffsetX8 - beforeX8,
             (hostile.Y << 8) + hostile.OffsetY8 - beforeY8));
        Assert.Equal(12, Assert.Single(battle.Retainers).Health);
    }

    [Fact]
    public void HitHealthSixHostileTakesModeThirteenSuccessEdgeBackToModeSix()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 4, enemyHealth: 6, retainerHealth: 12,
            enemyArmor: 10, retainerAttackSkill: 1_000);
        var hostile = Assert.Single(battle.Enemies);

        battle.AdvanceHostileMovement(0);
        battle.CommandRetainers(SiegeRetainerCommand.Attack);
        battle.AdvanceRetainerOrders();
        battle.AdvanceRetainerMovement(0.4001);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceEnemyAnimations(1.0001);
        Assert.Equal(13, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Hit, hostile.VisualState);
        Assert.Equal(6, hostile.Health);

        battle.AdvanceEnemyAnimations(0.3841);
        Assert.Equal(6, hostile.ActorMode);
        Assert.Equal((5, 2, 0, 0), (hostile.X, hostile.Y, hostile.OffsetX8, hostile.OffsetY8));
    }

    [Fact]
    public void HostileModeFourUsesOneNoEffectDecisionPerStableUpdate()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 4, enemyHealth: 10,
            retainerHealth: 5, actorTemplate: 5);
        var hostile = Assert.Single(battle.Enemies);
        battle.ConfigureActorRaycast((_, _) => null);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(1, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Walk, hostile.VisualState);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(3, hostile.ActorMode);
        battle.AdvanceHostileMovement(0);
        Assert.Equal(2, hostile.ActorMode);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(11, hostile.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Attack, hostile.VisualState);
    }

    [Fact]
    public void PlayerActionsDoNotInjectExtraImportedHostileThinkerPasses()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 4, enemyHealth: 10,
            retainerHealth: 5, actorTemplate: 5);
        var hostile = Assert.Single(battle.Enemies);
        battle.ConfigureActorRaycast((_, _) => null);

        battle.Attack();
        Assert.Equal(4, hostile.ActorMode);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(1, hostile.ActorMode);
    }

    [Fact]
    public void KindSevenUsesKindTwoModeFourTwoOneSixFallbacks()
    {
        var battle = HostileBattle(enemyX: 9, retainerX: 1, enemyHealth: 10,
            retainerHealth: 5, actorTemplate: 9);
        var hostile = Assert.Single(battle.Enemies);
        battle.ConfigureActorRaycast((_, _) => null);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(2, hostile.ActorMode);
        battle.AdvanceHostileMovement(0);
        Assert.Equal(1, hostile.ActorMode);
        battle.AdvanceHostileMovement(0);
        Assert.Equal(6, hostile.ActorMode);

        battle.AdvanceHostileMovement(0.2001);
        Assert.NotEqual((9, 2, 0, 0),
            (hostile.X, hostile.Y, hostile.OffsetX8, hostile.OffsetY8));
    }

    [Fact]
    public void HostileModeThreeRegroupsThroughModeSevenThenModeOne()
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[UnitType.Halberdiers] = 1;
        var movement = new SiegeActorMovement(3, 200, 64, 0, 0x142);
        var leaderSpawn = new SiegeSpawn(5, 2, false, OriginalArmor: 6,
            OriginalHealth: 10, OriginalCombatRow: 0, OriginalAttackSkill: 50,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384),
            OriginalMovement: movement, OriginalActorKind: 4,
            OriginalActorOrder: 1, OriginalActorTemplate: 5);
        var allySpawn = new SiegeSpawn(7, 2, false, OriginalArmor: 6,
            OriginalHealth: 10, OriginalCombatRow: 0, OriginalAttackSkill: 50,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384),
            OriginalMovement: movement, OriginalActorKind: 2,
            OriginalActorOrder: 2, OriginalActorTemplate: 3);
        var retainerSpawn = new SiegeSpawn(1, 2, false, OriginalArmor: 0,
            OriginalHealth: 5, OriginalCombatRow: 0, OriginalAttackSkill: 50,
            OriginalActorKind: 0, OriginalActorOrder: 0);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [leaderSpawn, allySpawn],
                retainers: [retainerSpawn]));
        var leader = battle.Enemies.Single(actor => actor.OriginalActorKind == 4);
        var ally = battle.Enemies.Single(actor => actor.OriginalActorKind == 2);
        battle.ConfigureActorRaycast((source, target) =>
            ReferenceEquals(source, leader) && ReferenceEquals(target, ally)
                ? new SiegeActorRayHit(ally, 0x180)
                : null);

        battle.AdvanceHostileMovement(0);
        Assert.Equal(1, leader.ActorMode);
        battle.AdvanceHostileMovement(0);
        Assert.Equal(3, leader.ActorMode);
        battle.AdvanceHostileMovement(0);
        Assert.Equal(7, leader.ActorMode);
        Assert.Equal(Facing.East, leader.Facing);

        battle.AdvanceHostileMovement(0.6001);
        Assert.Equal(1, leader.ActorMode);
        Assert.Equal((6, 2, -64, 0),
            (leader.X, leader.Y, leader.OffsetX8, leader.OffsetY8));
    }

    [Fact]
    public void HostileMovementRejectsInvalidElapsedTime()
    {
        var battle = HostileBattle(enemyX: 5, retainerX: 2, enemyHealth: 10, retainerHealth: 5);

        Assert.Throws<ArgumentOutOfRangeException>(() => battle.AdvanceHostileMovement(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => battle.AdvanceHostileMovement(-0.001));
    }

    private static SiegeSession HostileBattle(int enemyX, int retainerX, int enemyHealth,
        int retainerHealth, int actorTemplate = 3, int enemyAttackSkill = 50,
        int enemyArmor = 6, int retainerAttackSkill = 50, int retainerArmor = 0,
        int retainerY = 2, int retainerActorKind = 0)
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[UnitType.Halberdiers] = 1;
        var modes = OriginalCombatantTemplates.ModeProfileForSceneTemplate(actorTemplate);
        var enemy = new SiegeSpawn(enemyX, 2, false, OriginalArmor: enemyArmor,
            OriginalHealth: enemyHealth, OriginalCombatRow: 0, OriginalAttackSkill: enemyAttackSkill,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384),
            OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142),
            OriginalActorKind: OriginalCombatantTemplates.ActorKindForSceneTemplate(actorTemplate),
            OriginalActorOrder: 1, OriginalActorTemplate: actorTemplate);
        var retainer = new SiegeSpawn(retainerX, retainerY, false, OriginalArmor: retainerArmor,
            OriginalHealth: retainerHealth, OriginalCombatRow: 0,
            OriginalAttackSkill: retainerAttackSkill,
            OriginalActorKind: retainerActorKind, OriginalActorOrder: 0);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [enemy], retainers: [retainer]));
        Assert.Equal(modes.Current, Assert.Single(battle.Enemies).ActorMode);
        return battle;
    }
}
