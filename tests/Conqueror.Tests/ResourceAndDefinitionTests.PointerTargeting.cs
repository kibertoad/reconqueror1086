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
    public void GroundPointerUsesTheExecutableFixedPointCameraAndTraversalLimit()
    {
        var tiles = new SiegeTile[128, 128];
        var battle = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 64, 100, Facing.North, []));

        Assert.Equal((64, 58), SiegeViewProjection.GroundCell(battle, 83, 60, 167, 117));
        Assert.Equal((43, 58), SiegeViewProjection.GroundCell(battle, 0, 60, 167, 117));
        Assert.Equal((85, 58), SiegeViewProjection.GroundCell(battle, 166, 60, 167, 117));
        Assert.Null(SiegeViewProjection.GroundCell(battle, 83, 59, 167, 117));

        tiles[64, 80] = SiegeTile.Wall;
        var blocked = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 64, 100, Facing.North, []));
        Assert.Null(SiegeViewProjection.GroundCell(blocked, 83, 60, 167, 117));
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
        Assert.Equal((2, 2), (friendly.X, friendly.Y));
        battle.AdvanceRetainerMovement(0.6001);

        Assert.Equal((2, 3, 0, -64, Facing.South),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
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
        Assert.Equal((0, 128), (friendly.OffsetX8, friendly.OffsetY8));
        battle.AdvanceRetainerMovement(0.0001);
        Assert.Equal((2, 3), (friendly.X, friendly.Y));
        Assert.Equal((0, -64), (friendly.OffsetX8, friendly.OffsetY8));
        battle.AdvanceRetainerMovement(1.6001);

        Assert.Equal((2, 5), (friendly.X, friendly.Y));
        Assert.Equal((0, -64), (friendly.OffsetX8, friendly.OffsetY8));
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((2, 5), (friendly.X, friendly.Y));
        Assert.Equal((0, 0), (friendly.OffsetX8, friendly.OffsetY8));
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
        Assert.Equal((Assert.Single(oneUpdate.Retainers).OffsetX8, Assert.Single(oneUpdate.Retainers).OffsetY8),
            (Assert.Single(splitUpdates.Retainers).OffsetX8, Assert.Single(splitUpdates.Retainers).OffsetY8));
        Assert.Equal((0, -64),
            (Assert.Single(oneUpdate.Retainers).OffsetX8, Assert.Single(oneUpdate.Retainers).OffsetY8));
    }

    [Theory]
    [InlineData(3, 2, Facing.East, 64, 0)]
    [InlineData(1, 2, Facing.West, -64, 0)]
    [InlineData(2, 3, Facing.South, 0, 64)]
    [InlineData(2, 1, Facing.North, 0, -64)]
    public void MovementDescriptorDeltaRotatesIntoEachCardinalSubcellDirection(
        int targetX, int targetY, Facing expectedFacing, int expectedOffsetX8, int expectedOffsetY8)
    {
        var battle = GroundOrderBattle(targetX, targetY);
        var friendly = Assert.Single(battle.Retainers);

        battle.AdvanceRetainerMovement(0.2001);

        Assert.Equal((2, 2), (friendly.X, friendly.Y));
        Assert.Equal(expectedFacing, friendly.Facing);
        Assert.Equal((expectedOffsetX8, expectedOffsetY8), (friendly.OffsetX8, friendly.OffsetY8));
    }

    [Theory]
    [InlineData(3, 3, Facing.South)]
    [InlineData(1, 3, Facing.West)]
    [InlineData(1, 1, Facing.North)]
    [InlineData(3, 1, Facing.East)]
    public void ModeTwelveCardinalQuantizationChoosesClockwiseAtDiagonalTies(
        int targetX, int targetY, Facing expectedFacing)
    {
        var battle = GroundOrderBattle(targetX, targetY);
        var friendly = Assert.Single(battle.Retainers);

        battle.AdvanceRetainerMovement(0.2001);

        Assert.Equal(expectedFacing, friendly.Facing);
    }

    [Fact]
    public void ModeTwelveCollisionUsesBehaviorBitTwoAndStopsItsRewrittenEffectAtTheStrictBand()
    {
        var tiles = new SiegeTile[10, 10];
        var movementBlocks = new bool[10, 10];
        movementBlocks[3, 2] = true;
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(tiles, 0, 0, Facing.East, [], retainers:
            [new SiegeSpawn(2, 2, false, OriginalHealth: 10,
                OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142, 5))],
                movementBlocks: movementBlocks));
        var friendly = Assert.Single(battle.Retainers);
        battle.ToggleRetainerSelection(friendly);
        Assert.True(battle.CommandSelectedRetainersTo(4, 2));

        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((64, 0, Facing.East),
            (friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((2, 2, 64, 0, Facing.East),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((64, 0, Facing.East),
            (friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((64, 0, Facing.East),
            (friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
    }

    [Fact]
    public void SharedGroundDestinationAllowsOneOccupantAndBlocksFollowers()
    {
        var battle = SharedDestinationBattle();
        var first = battle.Retainers[0];

        Assert.Equal(0x87, OriginalCombatantTemplates.PlacedActorBehavior);
        Assert.NotEqual(0, OriginalCombatantTemplates.PlacedActorBehavior & 0x02);
        battle.AdvanceRetainerMovement(0.6001);

        var occupant = Assert.Single(battle.Retainers, retainer => retainer.X == 3);
        var follower = Assert.Single(battle.Retainers,
            retainer => !ReferenceEquals(retainer, occupant));
        Assert.Equal((3, 2, -64), (occupant.X, occupant.Y, occupant.OffsetX8));
        Assert.Equal(64, Math.Abs(follower.OffsetX8));
        Assert.Same(first, occupant);

        battle.AdvanceRetainerMovement(1.6001);

        Assert.Equal((3, 2), (occupant.X, occupant.Y));
        Assert.NotEqual((3, 2), (follower.X, follower.Y));
        Assert.Equal(64, Math.Abs(follower.OffsetX8));
    }

    [Fact]
    public void SharedDestinationWinnerIsIndependentOfUpdateSubdivision()
    {
        var whole = SharedDestinationBattle();
        var divided = SharedDestinationBattle();

        whole.AdvanceRetainerMovement(0.6001);
        divided.AdvanceRetainerMovement(0.2);
        divided.AdvanceRetainerMovement(0.2);
        divided.AdvanceRetainerMovement(0.2001);

        Assert.Equal(
            whole.Retainers.Select(retainer => (retainer.X, retainer.Y,
                retainer.OffsetX8, retainer.OffsetY8)),
            divided.Retainers.Select(retainer => (retainer.X, retainer.Y,
                retainer.OffsetX8, retainer.OffsetY8)));
        Assert.Equal(3, whole.Retainers[0].X);
        Assert.Equal(4, whole.Retainers[1].X);
    }

    [Fact]
    public void AuthoredObjectStateReplacementUpdatesTheMovementBlockerPlane()
    {
        var tiles = new SiegeTile[10, 10];
        tiles[3, 2] = SiegeTile.Destructible;
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var obstacle = new SiegeObjectSpawn(3, 2,
        [
            new SiegeObjectStage(1, SiegeTile.Destructible, BlocksMovement: true),
            new SiegeObjectStage(2, SiegeTile.Floor, BlocksMovement: false)
        ]);
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(tiles, 3, 1, Facing.South, [], [obstacle],
                [new SiegeSpawn(2, 2, false, OriginalHealth: 10,
                    OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142, 5))]));
        var friendly = Assert.Single(battle.Retainers);
        battle.ToggleRetainerSelection(friendly);
        Assert.True(battle.CommandSelectedRetainersTo(5, 2));
        battle.AdvanceRetainerMovement(0.4001);
        Assert.Equal((2, 2, 64, Facing.East),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.Facing));

        Assert.Equal(SiegeAction.DoorOpened, battle.Interact(Assert.Single(battle.Objects)));
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((2, 2, 128), (friendly.X, friendly.Y, friendly.OffsetX8));
        battle.AdvanceRetainerMovement(0.4001);

        Assert.Equal((3, 2, 0), (friendly.X, friendly.Y, friendly.OffsetX8));
    }

    [Fact]
    public void ActorProjectionUsesTheExecutableMappedSubcellOffsets()
    {
        var battle = GroundOrderBattle(3, 2, 0, 2);
        battle.AdvanceRetainerMovement(0.2001);

        var projection = Assert.Single(SiegeViewProjection.ProjectRetainers(battle));

        Assert.Equal(2.25, projection.ForwardDistance);
        Assert.Equal(0.5, projection.ScreenPosition);
    }

    private static SiegeSession GroundOrderBattle(
        int targetX = 2, int targetY = 5, int playerX = 0, int playerY = 0)
    {
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(new SiegeTile[10, 10], playerX, playerY, Facing.East, [], retainers:
            [new SiegeSpawn(2, 2, false, OriginalHealth: 10,
                OriginalMovement: new SiegeActorMovement(3, 200, 64, 0, 0x142, 5))]));
        battle.ToggleRetainerSelection(Assert.Single(battle.Retainers));
        Assert.True(battle.CommandSelectedRetainersTo(targetX, targetY));
        return battle;
    }

    private static SiegeSession SharedDestinationBattle()
    {
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 6;
        var movement = new SiegeActorMovement(3, 200, 64, 0, 0x142, 5);
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(new SiegeTile[8, 5], 0, 0, Facing.East, [], retainers:
            [
                new SiegeSpawn(2, 2, false, OriginalHealth: 10, OriginalMovement: movement),
                new SiegeSpawn(4, 2, false, OriginalHealth: 10, OriginalMovement: movement)
            ]));
        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        battle.ToggleRetainerSelection(battle.Retainers[0]);
        battle.ToggleRetainerSelection(battle.Retainers[1]);
        Assert.True(battle.CommandSelectedRetainersTo(3, 2));
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
