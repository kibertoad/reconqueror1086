using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-ASSAULT-005, RULE-ASSAULT-010, RULE-ASSAULT-015, RULE-ASSAULT-017, RULE-ASSAULT-019, RULE-VIEW-003, RULE-VIEW-004, DEV-ASSAULT-002.
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
    public void ImportedActorLayoutUsesExecutableWidthDepthAndElevationProjection()
    {
        var source = SyntheticScene();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        var block = scene.Blocks[0] with { LowerElevation = 0, Height = 256 };
        var actor = new SiegeEnemyProjection(0.75, 2, new SiegeEnemy());

        Assert.Equal(new SiegeBillboardLayout(50, 15, 50, 51),
            SiegeViewProjection.ActorLayout(actor, block, 100, 80));
    }

    [Fact]
    public void PrimaryPointerRequiresAProjectedBlockRatherThanInventingAFloorPlane()
    {
        var tiles = new SiegeTile[12, 12];
        var battle = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 5, 5, Facing.North, []));

        Assert.Null(SiegeViewProjection.GroundCell(battle, 83, 116, 167, 117));

        tiles[5, 3] = SiegeTile.Wall;
        var projected = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 5, 5, Facing.North, []));
        Assert.Equal((5, 3), SiegeViewProjection.GroundCell(projected, 83, 58, 167, 117));
        Assert.Null(SiegeViewProjection.GroundCell(projected, 83, 116, 167, 117));
    }

    [Fact]
    public void PointerContactUsesTheExecutableFixedPointCameraAndTraversalLimit()
    {
        var tiles = new SiegeTile[128, 128];
        tiles[64, 58] = SiegeTile.Wall;
        tiles[43, 58] = SiegeTile.Wall;
        tiles[85, 58] = SiegeTile.Wall;
        var battle = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 64, 100, Facing.North, []));

        Assert.Equal((64, 58), SiegeViewProjection.GroundCell(battle, 83, 60, 167, 117));
        Assert.Equal((43, 58), SiegeViewProjection.GroundCell(battle, 0, 60, 167, 117));
        Assert.Equal((85, 58), SiegeViewProjection.GroundCell(battle, 166, 60, 167, 117));
        Assert.Null(SiegeViewProjection.GroundCell(battle, 83, 61, 167, 117));

        var beyondLimit = new SiegeTile[128, 128];
        beyondLimit[64, 35] = SiegeTile.Wall;
        var limited = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(beyondLimit, 64, 100, Facing.North, []));
        Assert.Null(SiegeViewProjection.GroundCell(limited, 83, 58, 167, 117));
    }

    [Fact]
    public void OriginalCandidateTraversalKeepsPassThroughBlocksBeforeTheStoppingWall()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 4, 3);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 2, 1);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 2 + 4, 2);
        SetSceneCell(source.Map, 11, 20, 1);
        SetSceneCell(source.Map, 12, 20, 2);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        var battle = ProjectionBattle();

        var candidates = OriginalSiegeProjection.CastColumn(
            battle, scene, 10, 20, 83, 167);

        Assert.Equal([1, 2], candidates.Select(candidate => candidate.Block.Index));
        Assert.Equal([256, 384], candidates.Select(candidate => candidate.Hit.Distance8));
    }

    [Theory]
    [InlineData(3, 18, 32, false)]
    [InlineData(7, 14, 31, true)]
    public void OriginalKindFourProjectionUsesCenterDepthHeadingSectorAndMirror(
        int behavior, int textureIndex, int textureX, bool flip)
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        var offset = DynamixSceneDecoder.BlockSize;
        WriteInt(source.Blocks, offset, 4);
        WriteInt(source.Blocks, offset + 4, behavior);
        WriteInt(source.Blocks, offset + 44, 12);
        WriteInt(source.Blocks, offset + 52, 8);
        WriteInt(source.Blocks, offset + 56, 0);
        SetSceneCell(source.Map, 11, 20, 1);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var candidate = Assert.Single(OriginalSiegeProjection.CastColumn(
            ProjectionBattle(), scene, 10, 20, 83, 167));

        Assert.Equal(256, candidate.Hit.Distance8);
        Assert.Equal(128, candidate.TextureCoordinate8);
        Assert.Equal(textureIndex, candidate.TextureIndex);
        Assert.Equal(textureX, candidate.TextureX);
        Assert.Equal(flip, candidate.FlipHorizontally);
        Assert.Equal((1, 0), (candidate.SourceMapX, candidate.SourceMapY));
    }

    [Theory]
    [InlineData(0, -256, 0)]
    [InlineData(256, -256, 32)]
    [InlineData(256, 0, 64)]
    [InlineData(256, 256, 96)]
    [InlineData(0, 256, 128)]
    [InlineData(-256, 256, 160)]
    [InlineData(-256, 0, 192)]
    [InlineData(-256, -256, 224)]
    public void OriginalHeadingHelperMapsLocalVectorsIntoTheExecutableByteTurn(
        int deltaX8, int deltaY8, int expectedHeading)
    {
        Assert.Equal(expectedHeading, OriginalSiegeProjection.HeadingToward(deltaX8, deltaY8));
    }

    [Fact]
    public void NonCardinalHeadingRayUsesTheExecutableIntegerDirectionTable()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize, 1);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 4, 2);
        SetSceneCell(source.Map, 12, 21, 1);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var candidates = OriginalSiegeProjection.CastHeading(
            ProjectionBattle(), scene, 10, 20, 0x80, 0x80,
            OriginalSiegeProjection.HeadingToward(0x200, 0x100));

        var candidate = Assert.Single(candidates);
        Assert.Equal((2, 1), (candidate.SourceMapX, candidate.SourceMapY));
        Assert.Equal(0x1a1, candidate.Hit.Distance8);
    }

    [Fact]
    public void ActorAcquisitionRequiresTheTargetIdentityAtAnOpaqueCenterRayPixel()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        var blockOffset = DynamixSceneDecoder.BlockSize;
        WriteInt(source.Blocks, blockOffset, 4);
        WriteInt(source.Blocks, blockOffset + 4, 0x87);
        WriteInt(source.Blocks, blockOffset + 44, 0);
        WriteInt(source.Blocks, blockOffset + 52, 1);
        WriteInt(source.Blocks, blockOffset + 56, 0);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(new SiegeTile[6, 5], 0, 0, Facing.East,
                [new SiegeSpawn(3, 2, false, 1, OriginalHealth: 10)], retainers:
                [new SiegeSpawn(1, 1, false, 1, OriginalHealth: 10)]));
        var friendly = Assert.Single(battle.Retainers);
        var target = Assert.Single(battle.Enemies);
        var opaque = new DynamixSceneTexture(0, 64, 128,
            Enumerable.Repeat((byte)1, 64 * 128).ToArray());

        SiegeProjectedBlock? ActiveBlockAt(int x, int y)
        {
            var actor = battle.EnemyAt(x, y) ?? (SiegeEnemy?)battle.RetainerAt(x, y);
            return actor is null ? null : new SiegeProjectedBlock(
                scene.Blocks[1], actor.OffsetX8, actor.OffsetY8, 1) { Actor = actor };
        }

        var hit = OriginalSiegeActorAcquisition.CastToward(
            battle, scene, 10, 20, friendly, target, ActiveBlockAt, _ => opaque);
        var transparent = OriginalSiegeActorAcquisition.CastToward(
            battle, scene, 10, 20, friendly, target, ActiveBlockAt,
            _ => opaque with { Indices = new byte[64 * 128] });

        Assert.Same(target, hit?.Actor);
        Assert.InRange(hit!.Distance8, 0x230, 0x250);
        Assert.Null(transparent);
    }

    [Theory]
    [InlineData(3, 256)]
    [InlineData(5, 256)]
    [InlineData(6, 255)]
    public void OriginalCandidateGeometryIntersectsCenterAndDiagonalPlanes(int kind, int distance8)
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize, kind);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 4, 2);
        SetSceneCell(source.Map, 11, 20, 1);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var candidate = Assert.Single(OriginalSiegeProjection.CastColumn(
            ProjectionBattle(), scene, 10, 20, 83, 167));

        Assert.Equal(distance8, candidate.Hit.Distance8);
        Assert.Equal((1, 0), (candidate.Hit.MapX, candidate.Hit.MapY));
    }

    [Fact]
    public void OriginalCandidateArrayStopsAtTheExecutableThirtyOneEntrySentinel()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize, 1);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 4, 3);
        for (var x = 11; x < 60; x++) SetSceneCell(source.Map, x, 20, 1);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var candidates = OriginalSiegeProjection.CastColumn(
            ProjectionBattle(), scene, 10, 20, 83, 167);

        Assert.Equal(31, candidates.Count);
    }

    [Fact]
    public void OriginalCandidateTraversalFollowsEligibleStateTargetsInPlace()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 4, 3);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 0x40, 3);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 3, 3);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 3 + 4, 3);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 2, 1);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 2 + 4, 2);
        SetSceneCell(source.Map, 11, 20, 1);
        SetSceneCell(source.Map, 12, 20, 2);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var candidates = OriginalSiegeProjection.CastColumn(
            ProjectionBattle(), scene, 10, 20, 83, 167);

        Assert.Equal([1, 3, 2], candidates.Select(candidate => candidate.Block.Index));
    }

    [Fact]
    public void OriginalCandidateTraversalPreservesCornerProbeOrder()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        var probes = new List<(int X, int Y)>();

        OriginalSiegeProjection.CastColumn(
            ProjectionBattle(), scene, 10, 20, 2, 3,
            (x, y) =>
            {
                probes.Add((x, y));
                return null;
            });

        Assert.Equal([(1, 0), (1, -1), (1, 1), (2, 0), (1, 1), (2, 1)],
            probes.Take(6));
    }

    [Fact]
    public void OriginalCandidateTraversalWrapsSourceLookupBeforeCropping()
    {
        var source = SyntheticScene();
        source.Map.AsSpan().Clear();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        var probes = new List<(int X, int Y)>();

        OriginalSiegeProjection.CastColumn(
            ProjectionBattle(), scene, 127, 20, 1, 3,
            (x, y) =>
            {
                probes.Add((x, y));
                return null;
            });

        Assert.Equal((-127, 0), probes[0]);
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
    public void PointerActorTargetUsesStrictReturnedRayDepthInsteadOfActorCellCenter()
    {
        var inside = PointerTargetBattle();
        var insideTarget = Assert.Single(inside.Enemies);
        var contactDistance8 = OriginalWeaponCombat.ContactDistanceFor(9);

        Assert.Equal(SiegeAction.Hit, inside.Attack(insideTarget, contactDistance8 - 1));

        var boundary = PointerTargetBattle();
        var boundaryTarget = Assert.Single(boundary.Enemies);
        Assert.Equal(SiegeAction.Missed, boundary.Attack(boundaryTarget, contactDistance8));
        Assert.Equal(20, boundaryTarget.Health);
    }

    [Fact]
    public void SelectedRetainersKeepTheHostileActorChosenByThePointer()
    {
        var tiles = new SiegeTile[10, 10];
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var near = new SiegeSpawn(4, 2, false, OriginalArmor: 0, OriginalHealth: 10,
            OriginalAttackSkill: 1);
        var chosen = new SiegeSpawn(2, 6, false, OriginalArmor: 0, OriginalHealth: 10,
            OriginalAttackSkill: 1);
        var retainer = new SiegeSpawn(2, 2, false, OriginalHealth: 10,
            OriginalCombatRow: 0, OriginalAttackSkill: 1_000,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(tiles, 1, 2, Facing.East, [near, chosen], retainers: [retainer]));
        var friendly = Assert.Single(battle.Retainers);
        var chosenActor = battle.Enemies[1];
        battle.ConfigureActorRaycast((_, target) => ReferenceEquals(target, chosenActor)
            ? new SiegeActorRayHit(chosenActor, 0x100)
            : null);

        battle.ToggleRetainerSelection(friendly);
        Assert.True(battle.CommandSelectedRetainersAt(chosenActor));
        battle.AdvanceRetainerOrders();
        Assert.Equal(11, friendly.ActorMode);
        Assert.Equal(SiegeEnemyVisualState.Attack, friendly.VisualState);
        Assert.Equal(10, battle.Enemies[0].Health);
        Assert.Equal(10, chosenActor.Health);

        battle.AdvanceEnemyAnimations(0.3841);

        Assert.Equal(10, battle.Enemies[0].Health);
        Assert.True(chosenActor.Health < 10);
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
    public void SelectedRetainersAcceptAProjectedBlockedContactAndLetMovementRejectIt()
    {
        var tiles = new SiegeTile[10, 10];
        tiles[2, 3] = SiegeTile.Wall;
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 4,
            new SiegeLayout(tiles, 1, 2, Facing.East, [],
                retainers: [new SiegeSpawn(2, 2, false, OriginalHealth: 10)]));
        var friendly = Assert.Single(battle.Retainers);
        battle.ToggleRetainerSelection(friendly);

        Assert.True(battle.CommandSelectedRetainersTo(2, 3));
        Assert.False(friendly.Selected);
        battle.AdvanceRetainerMovement(0.6001);

        Assert.Equal((2, 2), (friendly.X, friendly.Y));
        Assert.Equal((0, 64), (friendly.OffsetX8, friendly.OffsetY8));
        battle.AdvanceRetainerMovement(0.6001);
        Assert.Equal((2, 2, 0, 64),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8));
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

    private static SiegeSession ProjectionBattle()
    {
        var tiles = new SiegeTile[70, 3];
        return new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 0, 0, Facing.East, []));
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

    [Fact]
    public void PointerObjectTargetUsesTheStrictFixedPointActionLimit()
    {
        var inside = PointerObjectBattle();
        Assert.Equal(SiegeAction.Looted,
            inside.Interact(Assert.Single(inside.Objects), 0x27f));

        var boundary = PointerObjectBattle();
        Assert.Equal(SiegeAction.None,
            boundary.Interact(Assert.Single(boundary.Objects), 0x280));
        Assert.Single(boundary.Objects);
        Assert.Equal(0, boundary.GoldFound);
    }

    private static SiegeSession PointerObjectBattle()
    {
        var tiles = new SiegeTile[6, 6];
        var pickup = new SiegeObjectSpawn(5, 5,
            [new SiegeObjectStage(1, SiegeTile.Treasure,
                new SiegePickupReward(SiegePickupRewardKind.Wealth, 25))]);
        return new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [], [pickup]));
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
