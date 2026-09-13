using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void BillboardHitGeometryMatchesTheRuntimeDrawGeometry()
    {
        var enemy = new SiegeEnemy();
        var actor = new SiegeEnemyProjection(0.5, 2, enemy);
        var tiles = new SiegeTile[2, 2];
        var session = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 0, 0, Facing.East, [],
                [new SiegeObjectSpawn(1, 1, [new SiegeObjectStage(0, SiegeTile.Barrel)])]));
        var item = new SiegeObjectProjection(0.5, 2,
            Assert.Single(session.Objects));

        Assert.Equal(new SiegeBillboardLayout(45, 40, 10, 20),
            SiegeViewProjection.ActorLayout(actor, 100, 80, 64, 128));
        Assert.Equal(new SiegeBillboardLayout(45, 40, 10, 20),
            SiegeViewProjection.ObjectLayout(item, 100, 80, 64, 128));
        Assert.True(SiegeViewProjection.ActorLayout(actor, 100, 80, 64, 128).Contains(45, 40));
        Assert.False(SiegeViewProjection.ActorLayout(actor, 100, 80, 64, 128).Contains(55, 60));
    }

    [Fact]
    public void LowerViewportPointerMapsToVisibleGroundInFrontOfThePlayer()
    {
        var tiles = new SiegeTile[12, 12];
        var battle = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 5, 5, Facing.North, []));

        Assert.Equal((5, 4), SiegeViewProjection.GroundCell(battle, 83, 116, 167, 117));
        Assert.Null(SiegeViewProjection.GroundCell(battle, 83, 58, 167, 117));

        tiles[5, 4] = SiegeTile.Wall;
        var blocked = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 5, 5, Facing.North, []));
        Assert.Null(SiegeViewProjection.GroundCell(blocked, 83, 116, 167, 117));
    }

    [Fact]
    public void ExplicitActorTargetCanBeStruckOutsideTheKeyboardCenterRay()
    {
        var battle = PointerTargetBattle();
        var target = Assert.Single(battle.Enemies);

        Assert.Equal(SiegeAction.Hit, battle.Attack(target));
        Assert.Equal(SiegeAction.Missed, PointerTargetBattle().Attack());
    }

    [Fact]
    public void SelectedRetainersKeepTheHostileActorChosenByThePointer()
    {
        var tiles = new SiegeTile[10, 10];
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var near = new SiegeSpawn(4, 2, false, OriginalHealth: 10);
        var chosen = new SiegeSpawn(2, 6, false, OriginalHealth: 10);
        var retainer = new SiegeSpawn(2, 2, false, OriginalHealth: 10);
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(tiles, 1, 2, Facing.East, [near, chosen], retainers: [retainer]));
        var friendly = Assert.Single(battle.Retainers);

        battle.ToggleRetainerSelection(friendly);
        Assert.True(battle.CommandSelectedRetainersAt(battle.Enemies[1]));
        battle.AdvanceRetainerOrders();

        Assert.Equal((2, 3), (friendly.X, friendly.Y));
        Assert.False(friendly.Selected);
    }

    [Fact]
    public void SelectedRetainersMoveToTheGroundCellThenResumeTheirPriorCommand()
    {
        var tiles = new SiegeTile[10, 10];
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(tiles, 1, 2, Facing.East, [],
                retainers: [new SiegeSpawn(2, 2, false, OriginalHealth: 10)]));
        var friendly = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        Assert.False(battle.CommandSelectedRetainersTo(2, 5));
        battle.ToggleRetainerSelection(friendly);

        Assert.True(battle.CommandSelectedRetainersTo(2, 5));
        battle.AdvanceRetainerMovement(0.6);
        Assert.Equal((2, 2), (friendly.X, friendly.Y));
        battle.AdvanceRetainerMovement(0.0001);
        Assert.Equal((2, 3), (friendly.X, friendly.Y));
        battle.AdvanceRetainerMovement(1.2);

        Assert.Equal((2, 5), (friendly.X, friendly.Y));
        Assert.Equal(SiegeRetainerCommand.Defend, friendly.Command);
        Assert.False(friendly.Selected);
    }

    [Fact]
    public void GroundOrderMovementIsIndependentOfUpdateFrequencyAtStrictEffectDeadlines()
    {
        var oneUpdate = GroundOrderBattle();
        var splitUpdates = GroundOrderBattle();

        oneUpdate.AdvanceRetainerMovement(0.6001);
        splitUpdates.AdvanceRetainerMovement(0.2);
        splitUpdates.AdvanceRetainerMovement(0.2);
        splitUpdates.AdvanceRetainerMovement(0.2001);

        Assert.Equal((Assert.Single(oneUpdate.Retainers).X, Assert.Single(oneUpdate.Retainers).Y),
            (Assert.Single(splitUpdates.Retainers).X, Assert.Single(splitUpdates.Retainers).Y));
        Assert.Equal((2, 3), (Assert.Single(oneUpdate.Retainers).X, Assert.Single(oneUpdate.Retainers).Y));
    }

    private static SiegeSession GroundOrderBattle()
    {
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(new SiegeTile[10, 10], 1, 2, Facing.East, [], retainers:
            [new SiegeSpawn(2, 2, false, OriginalHealth: 10,
                OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142))]));
        battle.ToggleRetainerSelection(Assert.Single(battle.Retainers));
        Assert.True(battle.CommandSelectedRetainersTo(2, 5));
        return battle;
    }

    [Fact]
    public void ExplicitObjectTargetUsesTheOriginalTwoAndAHalfCellActionLimit()
    {
        var tiles = new SiegeTile[5, 5];
        var pickup = new SiegeObjectSpawn(2, 2,
            [new SiegeObjectStage(1, SiegeTile.Treasure,
                new SiegePickupReward(SiegePickupRewardKind.Wealth, 25))]);
        var player = new Player();
        var wealthBefore = player.Wealth;
        var battle = new SiegeSession(player, new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [], [pickup]));

        Assert.Equal(SiegeAction.Looted, battle.Interact(Assert.Single(battle.Objects)));
        Assert.Equal(wealthBefore + 25, player.Wealth);
    }

    private static SiegeSession PointerTargetBattle()
    {
        var tiles = new SiegeTile[6, 6];
        var enemy = new SiegeSpawn(2, 2, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalCombatRow: 0, OriginalAttackSkill: 0);
        return new SiegeSession(new Player(), new Army(), 0, 2,
            new SiegeLayout(tiles, 1, 1, Facing.East, [enemy]));
    }
}
