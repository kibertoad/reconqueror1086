using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void ImportedSceneLayoutUsesOriginalBehaviorClassesBeforeNames()
    {
        var source = SyntheticScene();
        WriteInt(source.Blocks, 4, 2);
        SetSceneBlockName(source.Blocks, 1, "gate");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize + 4, 83);
        SetSceneBlockName(source.Blocks, 4, "mystery pickup");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 4, 19);
        SetSceneBlockName(source.Blocks, 5, "mystery actor");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 5 + 4, 135);

        var layout = ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));

        var tiles = layout.CopyTiles();
        Assert.Equal(SiegeTile.Floor, tiles[8, 20]);
        Assert.True(layout.BlocksMovementAt(8, 20));
        Assert.False(layout.BlocksMovementAt(10, 20));
        Assert.Equal(SiegeTile.Exit, tiles[11, 20]);
        Assert.Equal(SiegeTile.Treasure, tiles[9, 20]);
        Assert.Contains(layout.Enemies, enemy => (enemy.X, enemy.Y, enemy.VisualId) == (13, 20, 5));
    }

    [Fact]
    public void SiegeExitIsVisibleAndLeavesWithoutCrossingItsBoundary()
    {
        var tiles = new SiegeTile[4, 3];
        for (var x = 0; x < 4; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 3 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[2, 1] = SiegeTile.Exit;
        var siege = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, []));

        Assert.Equal(SiegeTile.Exit, SiegeViewProjection.Cast(siege, 0).Tile);
        Assert.Equal(SiegeAction.Exited, siege.Move(true));
        Assert.Equal((1, 1), (siege.PlayerX, siege.PlayerY));
        Assert.Equal("You leave the battle.", siege.LastMessage);
    }

    [Fact]
    public void DestructibleSceneObjectsAdvanceThroughTheirResourceStates()
    {
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[2, 1] = SiegeTile.Destructible;
        var stages = new[]
        {
            new SiegeObjectStage(10, SiegeTile.Destructible),
            new SiegeObjectStage(11, SiegeTile.Destructible),
            new SiegeObjectStage(12, SiegeTile.Floor)
        };
        var siege = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [], [new SiegeObjectSpawn(2, 1, stages)]));

        Assert.Equal(SiegeAction.Blocked, siege.Move(true));
        Assert.Equal(10, Assert.Single(SiegeViewProjection.ProjectObjects(siege)).Object.VisualId);
        Assert.Equal(SiegeAction.Hit, siege.Attack());
        Assert.Equal((1, 11, SiegeTile.Destructible),
            (Assert.Single(siege.Objects).State, Assert.Single(siege.Objects).VisualId, siege.TileAt(2, 1)));
        Assert.Equal(SiegeAction.Hit, siege.Attack());
        Assert.Equal((2, 12, SiegeTile.Floor),
            (Assert.Single(siege.Objects).State, Assert.Single(siege.Objects).VisualId, siege.TileAt(2, 1)));
        Assert.Equal(SiegeAction.Moved, siege.Move(true));
    }

    [Fact]
    public void ImportedDestructibleUsesItsExplicitSceneStateTarget()
    {
        var source = SyntheticScene();
        SetSceneBlockName(source.Blocks, 4, "tree");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 4, 35);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 64, 6);
        SetSceneBlockName(source.Blocks, 5, "tree chopped");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 5, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 5 + 4, 35);
        SetSceneBlockName(source.Blocks, 6, "ground");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 6, 0);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 6 + 4, 1);
        SetSceneCell(source.Map, 13, 20, 0);
        SetSceneCell(source.Map, 14, 20, 0);

        var layout = ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));

        var item = Assert.Single(layout.Objects, item => (item.X, item.Y) == (9, 20));
        Assert.Equal((9, 20), (item.X, item.Y));
        Assert.Equal([(4, SiegeTile.Destructible), (-1, SiegeTile.Floor)],
            item.Stages.Select(stage => (stage.VisualId, stage.Tile)));
        var siege = new SiegeSession(new Player(), new Army(), 0, 1, layout);
        siege.TurnLeft();
        siege.TurnLeft();
        Assert.Equal(SiegeAction.Hit, siege.Attack());
        Assert.Equal((1, -1, SiegeTile.Floor),
            (siege.ObjectAt(9, 20)!.State, siege.ObjectAt(9, 20)!.VisualId, siege.TileAt(9, 20)));
    }

    [Fact]
    public void ImportedSceneRejectsAnUnboundedActionableStateTarget()
    {
        var source = SyntheticScene();
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 4, 35);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 64, 7);

        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        Assert.Throws<InvalidDataException>(() => ImportedSiegeLayouts.Convert(scene));
    }

    [Fact]
    public void ImportedSceneRejectsAnUnknownActorTemplate()
    {
        var source = SyntheticScene();
        BinaryPrimitives.WriteInt16LittleEndian(
            source.Blocks.AsSpan(5 * DynamixSceneDecoder.BlockSize + 0x4a, sizeof(short)), 10);

        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        Assert.Throws<InvalidDataException>(() => ImportedSiegeLayouts.Convert(scene));
    }

    [Fact]
    public void ImportedSceneRejectsAnUnknownActorCombatRow()
    {
        var source = SyntheticScene();
        BinaryPrimitives.WriteInt16LittleEndian(
            source.Blocks.AsSpan(5 * DynamixSceneDecoder.BlockSize + 0x4c, sizeof(short)), 25);

        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        Assert.Throws<InvalidDataException>(() => ImportedSiegeLayouts.Convert(scene));
    }

    [Fact]
    public void ImportedPickupRetainsItsExplicitDebrisBillboard()
    {
        var source = SyntheticScene();
        SetSceneBlockName(source.Blocks, 4, "meal");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 4, 19);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 64, 6);
        WriteInteraction(source.Blocks, 4, 7, 2, 6);
        SetSceneBlockName(source.Blocks, 6, "broken barrel");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 6, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 6 + 4, 1);

        var layout = ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));

        var item = Assert.Single(layout.Objects, item => (item.X, item.Y) == (9, 20));
        Assert.Equal([(4, SiegeTile.Barrel), (6, SiegeTile.Floor)],
            item.Stages.Select(stage => (stage.VisualId, stage.Tile)));
        var siege = new SiegeSession(new Player(), new Army(), 0, 1, layout);
        siege.TurnLeft();
        siege.TurnLeft();
        Assert.Equal(SiegeAction.Healed, siege.Interact());
        Assert.Equal((1, 6, SiegeTile.Floor),
            (siege.ObjectAt(9, 20)!.State, siege.ObjectAt(9, 20)!.VisualId, siege.TileAt(9, 20)));
        Assert.Contains(SiegeViewProjection.ProjectObjects(siege), projection =>
            projection.Object.X == 9 && projection.Object.VisualId == 6);
    }

    [Fact]
    public void SceneObjectCanExposePickupBeforeRetainingDebris()
    {
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[2, 1] = SiegeTile.Destructible;
        var stages = new[]
        {
            new SiegeObjectStage(20, SiegeTile.Destructible),
            new SiegeObjectStage(21, SiegeTile.Barrel,
                new(SiegePickupRewardKind.Healing, 2, 6)),
            new SiegeObjectStage(22, SiegeTile.Floor)
        };
        var siege = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [], [new SiegeObjectSpawn(2, 1, stages)]));

        Assert.Equal(SiegeAction.Hit, siege.Attack());
        Assert.Equal((1, 21, SiegeTile.Barrel),
            (Assert.Single(siege.Objects).State, Assert.Single(siege.Objects).VisualId, siege.TileAt(2, 1)));
        Assert.Equal(21, Assert.Single(SiegeViewProjection.ProjectObjects(siege)).Object.VisualId);
        Assert.Equal(SiegeAction.Healed, siege.Interact());
        Assert.Equal((2, 22, SiegeTile.Floor),
            (Assert.Single(siege.Objects).State, Assert.Single(siege.Objects).VisualId, siege.TileAt(2, 1)));
    }

    [Fact]
    public void PickupRequiresExplicitInteractionAndUsesOriginalTwoCellRange()
    {
        var tiles = new SiegeTile[6, 3];
        for (var x = 0; x < 6; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 5 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[3, 1] = SiegeTile.Barrel;
        var siege = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [],
                [new SiegeObjectSpawn(3, 1, [
                    new(30, SiegeTile.Barrel, new(SiegePickupRewardKind.Healing, 2, 6)),
                    new(31, SiegeTile.Floor)])]));

        Assert.Equal(SiegeAction.Healed, siege.Interact());
        Assert.Equal(SiegeTile.Floor, siege.TileAt(3, 1));
    }

    [Fact]
    public void WalkingOntoPickupDoesNotCollectIt()
    {
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[2, 1] = SiegeTile.Barrel;
        var siege = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [],
                [new SiegeObjectSpawn(2, 1, [
                    new(30, SiegeTile.Barrel, new(SiegePickupRewardKind.Healing, 2, 6)),
                    new(31, SiegeTile.Floor)])]));

        Assert.Equal(SiegeAction.Moved, siege.Move(true));
        Assert.Equal(SiegeTile.Barrel, siege.TileAt(2, 1));
        Assert.Equal(0, Assert.Single(siege.Objects).State);
    }

    [Fact]
    public void SiegeContactUsesTheOriginalWeaponRowDistance()
    {
        var tiles = new SiegeTile[6, 3];
        for (var x = 0; x < 6; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 5 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[3, 1] = SiegeTile.Destructible;
        var player = new Player();
        var siege = new SiegeSession(player, new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, [],
                [new SiegeObjectSpawn(3, 1, [new(30, SiegeTile.Destructible), new(31, SiegeTile.Floor)])]));

        Assert.Equal(SiegeAction.Missed, siege.Attack());
        Assert.Equal(0, Assert.Single(siege.Objects).State);
        player.Inventory.Weapon = "Kingslayer Sword";
        Assert.Equal(SiegeAction.Hit, siege.Attack());
        Assert.Equal(1, Assert.Single(siege.Objects).State);
    }

    [Fact]
    public void FatalAndWoundingHitsUseTheirExecutableBloodRuns()
    {
        Assert.Equal(new SiegeFrameRun(43, 4), SiegeCombatPresentation.BloodFramesFor(true));
        Assert.Equal(new SiegeFrameRun(48, 4), SiegeCombatPresentation.BloodFramesFor(false));
    }

    [Fact]
    public void FirstPersonWeaponPosesFollowTheExecutableApproachAndReturnOrder()
    {
        var sword = SiegeCombatPresentation.SwordAttack;
        Assert.Equal((41, 39), (sword.First, sword.Last));
        Assert.Equal(40, sword.Next(41));
        Assert.Equal(39, sword.Next(40));
        Assert.Equal(-1, sword.Next(39));

        var crossbow = SiegeCombatPresentation.CrossbowAttack;
        Assert.Equal((30, 32, 31), (crossbow.First, crossbow.Last, crossbow.Next(30)));
        Assert.Equal((43, 46), (SiegeCombatPresentation.FatalHitBlood.First,
            SiegeCombatPresentation.FatalHitBlood.Last));
    }

    [Fact]
    public void OriginalWeaponIdentifiersUseTheExecutableCombatRowPermutation()
    {
        int[] expectedRows =
        [
            4, 5, 12, 14, 6, 13, 11, 7, 9, 8, 10, 16,
            17, 18, 15, 3, 0, 1, 2, 19, 20, 21, 22
        ];
        int[] expectedBases =
        [
            39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 39, 27,
            27, 27, 27, 41, 41, 41, 41, 33, 33, 36, 36
        ];

        Assert.Equal(expectedRows, Enumerable.Range(0, 23).Select(OriginalWeaponCombat.CombatRowFor));
        Assert.Equal(expectedBases, Enumerable.Range(0, 23).Select(SiegeCombatPresentation.OriginalForegroundBaseFor));
        Assert.All(Balance.Equipment.Where(item => item.OriginalWeaponItemId is not null),
            item =>
            {
                var foregroundBase = SiegeCombatPresentation.OriginalForegroundBaseFor(item.OriginalWeaponItemId!.Value);
                var expectedAttackStart = foregroundBase == 41 ? 42 : foregroundBase;
                Assert.Equal(expectedAttackStart, SiegeCombatPresentation.AttackFramesFor(item.Name).Start);
            });
        Assert.Equal(
            [500, 450, 250, 250, 350, 200, 300, 400, 500, 300, 200, 300, 250, 250, 350, 350, null, 450, 500, 350, 250, 350, 350],
            Enumerable.Range(0, 23).Select(OriginalWeaponCombat.BreakRollRangeFor));
        Assert.Equal((23, 30, 500), (OriginalWeaponCombat.CombatRowFor(43),
            SiegeCombatPresentation.OriginalForegroundBaseFor(43), OriginalWeaponCombat.BreakRollRangeFor(43)));
        Assert.Equal((24, 30, 500), (OriginalWeaponCombat.CombatRowFor(44),
            SiegeCombatPresentation.OriginalForegroundBaseFor(44), OriginalWeaponCombat.BreakRollRangeFor(44)));
        Assert.Equal((514, 2), (OriginalWeaponCombat.ContactDistanceFor(0), OriginalWeaponCombat.GridReachFor(0)));
        Assert.Equal((484, 1), (OriginalWeaponCombat.ContactDistanceFor(2), OriginalWeaponCombat.GridReachFor(2)));
        Assert.Equal((7064, 27), (OriginalWeaponCombat.ContactDistanceFor(43), OriginalWeaponCombat.GridReachFor(43)));
        Assert.Equal((8256, 32), (OriginalWeaponCombat.ContactDistanceFor(44), OriginalWeaponCombat.GridReachFor(44)));
        Assert.Equal((7000, 8192),
            (OriginalWeaponCombat.ActorContactDistanceForCombatRow(23),
                OriginalWeaponCombat.ActorContactDistanceForCombatRow(24)));
        Assert.Equal(
            [400, 400, 380, 380, 400, 420, 460, 450, 470, 480, 500, 440, 480,
                480, 480, 520, 500, 500, 500, 520, 530, 520, 530, 900, 1000],
            Enumerable.Range(0, OriginalWeaponCombat.CombatRowCount)
                .Select(OriginalWeaponCombat.ForegroundMotionDivisorForCombatRow));
        Assert.Equal(128, OriginalWeaponCombat.ForegroundVelocityForCombatRow(0, 100));
        Assert.Equal(-128, OriginalWeaponCombat.ForegroundVelocityForCombatRow(0, -100));
        Assert.Equal(56, OriginalWeaponCombat.ForegroundVelocityForCombatRow(23, 100));
        Assert.Equal(43, Balance.Equipment.Single(item => item.Name == "Light Crossbow").OriginalWeaponItemId);
        Assert.Equal(44, Balance.Equipment.Single(item => item.Name == "Heavy Crossbow").OriginalWeaponItemId);
        Assert.Throws<ArgumentOutOfRangeException>(() => OriginalWeaponCombat.CombatRowFor(23));
        Assert.Throws<ArgumentOutOfRangeException>(() => OriginalWeaponCombat.ForegroundMotionDivisorForCombatRow(25));
    }

    [Fact]
    public void OriginalWeaponDamageRollsDiceThenAppliesArmorPenetration()
    {
        Assert.Equal(0, OriginalWeaponCombat.DamageFor(9, 20, new MaximumRandom()));
        Assert.Equal(17, OriginalWeaponCombat.DamageFor(0, 7, new MaximumRandom()));
        Assert.Equal(10, OriginalWeaponCombat.DamageFor(44, 10, new MaximumRandom()));
        Assert.Equal(11, OriginalWeaponCombat.DamageForCombatRow(5, 10, new MaximumRandom()));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalWeaponCombat.DamageFor(0, -1, new Random(1)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalWeaponCombat.DamageForCombatRow(25, 0, new Random(1)));
    }

    [Fact]
    public void ImportedRangedActorsUseTheirCombatRowContactDistance()
    {
        var clear = Corridor(6);
        var ranged = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(clear, 1, 1, Facing.East,
                [new SiegeSpawn(4, 1, false, OriginalCombatRow: 23)]), includeRetainers: false);

        Assert.Equal(SiegeAction.Missed, ranged.Attack());
        Assert.Equal(SiegeEnemyVisualState.Attack, Assert.Single(ranged.Enemies).VisualState);

        clear[2, 1] = SiegeTile.Wall;
        var occluded = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(clear, 1, 1, Facing.East,
                [new SiegeSpawn(4, 1, false, OriginalCombatRow: 23)]), includeRetainers: false);
        Assert.Equal(SiegeAction.Missed, occluded.Attack());
        Assert.Equal(SiegeEnemyVisualState.Walk, Assert.Single(occluded.Enemies).VisualState);

        static SiegeTile[,] Corridor(int width)
        {
            var tiles = new SiegeTile[width, 3];
            for (var x = 0; x < width; x++)
            for (var y = 0; y < 3; y++)
                tiles[x, y] = x == 0 || x == width - 1 || y is 0 or 2 ? SiegeTile.Wall : SiegeTile.Floor;
            return tiles;
        }
    }

    [Fact]
    public void OriginalCombatantTemplatesRetainExecutableSkillArmorAndHealth()
    {
        Assert.Equal(new OriginalCombatantTemplate(50, 7, 12), OriginalCombatantTemplates.For(0));
        Assert.Equal(new OriginalCombatantTemplate(70, 8, 15), OriginalCombatantTemplates.For(1));
        Assert.Equal(new OriginalCombatantTemplate(85, 10, 20), OriginalCombatantTemplates.For(9));
        Assert.Throws<ArgumentOutOfRangeException>(() => OriginalCombatantTemplates.For(10));
    }

    [Fact]
    public void OriginalHitEligibilityUsesCombatSkillsAndPositionalBonuses()
    {
        var player = new Player
        {
            Stats = new CharacterStats(12, 17, 8, 10, 10),
            SwordExperience = 9
        };

        Assert.Equal(47, OriginalWeaponCombat.PlayerAttackSkill(player));
        Assert.Equal(32, OriginalWeaponCombat.PlayerHealth(player));
        Assert.Equal(122, OriginalWeaponCombat.HitThreshold(47, 50, false, false));
        Assert.Equal(152, OriginalWeaponCombat.HitThreshold(47, 50, true, false));
        Assert.Equal(182, OriginalWeaponCombat.HitThreshold(47, 50, true, true));
        Assert.True(OriginalWeaponCombat.Hits(47, 50, false, false, new FixedRandom(121)));
        Assert.False(OriginalWeaponCombat.Hits(47, 50, false, false, new FixedRandom(122)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalWeaponCombat.HitThreshold(-1, 50, false, false));
    }

    [Fact]
    public void SceneCardinalFacesRetainTheirExecutableHitMasks()
    {
        Assert.Equal([0x100, 0x200, 0x400, 0x800],
            Enum.GetValues<DynamixSceneFace>().Select(face => (int)face));
    }

    [Fact]
    public void SceneColorMapGenerationMatchesTheExecutableBlendAndNearestPaletteRules()
    {
        var palette = new byte[IndexedPalette.ByteSize];
        for (var index = 0; index < IndexedPalette.ColorCount; index++)
            palette.AsSpan(index * 3, 3).Fill((byte)index);
        var stored = new DynamixSceneColorMaps(Enumerable.Range(0, DynamixSceneColorMaps.Count)
            .Select(index => new DynamixSceneColorMap(index,
                Enumerable.Range(0, DynamixSceneColorMaps.EntryCount).Select(value => (byte)value).ToArray())));

        var generated = DynamixSceneColorMapGenerator.RegenerateFirstFamily(
            stored, palette, new DynamixSceneColorMapping(true, 4, 10, 0));

        Assert.Equal(100, generated[0].Span[100]);
        Assert.Equal(2, generated[1].Span[2]);
        Assert.Equal(75, generated[1].Span[100]);
        Assert.Equal(50, generated[2].Span[100]);
        Assert.Equal(25, generated[3].Span[100]);
        Assert.Equal(100, generated[4].Span[100]);
    }

    [Fact]
    public void ControllerBindingsProvideEdgeTriggeredNavigationAndContextActions()
    {
        var released = default(GamePadState);
        var accept = new GamePadState(Vector2.Zero, Vector2.Zero, 0, 0, Buttons.A);
        Assert.True(ControllerInputBindings.IsPressed(Keys.Enter, ControllerInputContext.General, accept, released));
        Assert.False(ControllerInputBindings.IsPressed(Keys.Enter, ControllerInputContext.General, accept, accept));

        Assert.Equal([Buttons.DPadUp, Buttons.LeftThumbstickUp],
            ControllerInputBindings.ButtonsFor(Keys.Up, ControllerInputContext.General));
        Assert.Contains(Buttons.X, ControllerInputBindings.ButtonsFor(Keys.F, ControllerInputContext.Home));
        Assert.Empty(ControllerInputBindings.ButtonsFor(Keys.F, ControllerInputContext.General));
        Assert.Contains(Buttons.RightShoulder,
            ControllerInputBindings.ButtonsFor(Keys.P, ControllerInputContext.Map));
        Assert.Contains(Buttons.RightShoulder,
            ControllerInputBindings.ButtonsFor(Keys.D5, ControllerInputContext.Dialogue));
        Assert.Contains(Buttons.LeftThumbstickUp,
            ControllerInputBindings.ButtonsFor(Keys.W, ControllerInputContext.Siege));
        Assert.Contains(Buttons.A,
            ControllerInputBindings.ButtonsFor(Keys.A, ControllerInputContext.FieldBattle));
        Assert.Contains(Buttons.A,
            ControllerInputBindings.ButtonsFor(Keys.Space, ControllerInputContext.DragonBattle));
        Assert.Contains(Buttons.RightShoulder,
            ControllerInputBindings.ButtonsFor(Keys.I, ControllerInputContext.Tournament));
        Assert.Contains(Buttons.RightShoulder,
            ControllerInputBindings.ButtonsFor(Keys.P, ControllerInputContext.Village));

        var moved = ControllerInputBindings.MovePointer(new Vector2(638, 2), new Vector2(1, 1), 1);
        Assert.Equal(new Vector2(639, 0), moved);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerInputBindings.MovePointer(Vector2.Zero, Vector2.Zero, -.01));
        var trigger = new GamePadState(Vector2.Zero, Vector2.Zero, 0, 1, Buttons.None);
        Assert.True(ControllerInputBindings.PrimaryPointerPressed(trigger, released));
        Assert.True(ControllerInputBindings.PrimaryPointerReleased(released, trigger));
        var secondaryTrigger = new GamePadState(Vector2.Zero, Vector2.Zero, 1, 0, Buttons.None);
        Assert.True(ControllerInputBindings.SecondaryPointerPressed(secondaryTrigger, released));
    }

    [Fact]
    public void DragonEncounterUsesItsOwnedArtAnimationAndOutcomeMovies()
    {
        Assert.Contains(new ImportedArtDefinition("Dragon.Background", "image", ":drjstwin.pcx"),
            ImportedArt.Definitions);
        Assert.Contains(new ImportedAnimationDefinition("Dragon.Lance", ":lance1.csf", "Dragon.Background"),
            ImportedAnimations.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Travel.DragonLair", "/trandrag.smk"),
            ImportedMovies.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Ending.DragonVictory", "/drjstwin.smk"),
            ImportedMovies.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Ending.DragonInvestiture", "/champl30.smk"),
            ImportedMovies.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Ending.DragonDefeat", "/drjstlse.smk"),
            ImportedMovies.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Dragon.Retreat", "/drjstrun.smk"),
            ImportedMovies.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Ending.CrownVictory", "/crownl30.smk"),
            ImportedMovies.Definitions);
        Assert.Contains(new ImportedMovieDefinition("Ending.AgeLimit", "/avg_end.smk"),
            ImportedMovies.Definitions);
        Assert.Equal(ImportedMovies.Definitions.Count,
            ImportedMovies.Definitions.Select(movie => movie.Role).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void TournamentConversationRootsAndOriginalStateBridgesAreDefinitionDriven()
    {
        Assert.Equal(Balance.Courtships.Select(lady => lady.Name),
            TournamentConversationDefinitions.Ladies.Select(lady => lady.Lady));
        Assert.Equal([2200, 2900, 2400, 2100, 2000, 2500],
            TournamentConversationDefinitions.Ladies.Select(lady => lady.RootNodeId));
        Assert.Equal("Anna Lisa", OriginalConversationBindings.LadyColors[3]);
        Assert.Equal(3201, BlacksmithDialoguePresentationDefinitions.OriginalConversationRootNodeId);
        Assert.Equal(3100, ChurchConversationPresentationDefinitions.GenericRootNodeId);
        var cambridge = Array.FindIndex(World.Locations,
            location => location.Name == ChurchConversationPresentationDefinitions.ArmorQuestLocation);
        Assert.True(cambridge >= 0);
        Assert.Equal(3149, ChurchConversationPresentationDefinitions.RootNodeIdFor(cambridge));
        Assert.Equal(3100, ChurchConversationPresentationDefinitions.RootNodeIdFor(0));

        var campaign = Campaign.NewFromTemplate(2);
        campaign.ConversationVariables.AddRange(Enumerable.Repeat(0, 43));
        OriginalConversationBindings.RecordJoustResult(campaign, "Anna Lisa", won: true);
        OriginalConversationBindings.SynchronizeTournamentState(campaign);

        Assert.Equal(3, campaign.ConversationVariables[OriginalConversationBindings.LadyColorsVariable]);
        Assert.Equal(2, campaign.ConversationVariables[OriginalConversationBindings.JoustOutcomeVariable]);
        Assert.Equal("Anna Lisa", campaign.Player.LadyColors);
    }

    [Fact]
    public void ImportedContentVerificationDetectsDamageAndUnsafeManifestRecords()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-verify-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var relative = Path.Combine("Decoded", "fixture.bin");
            var path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, [1, 2, 3, 4]);
            var sourceHash = new string('a', 64);
            var valid = new ImportedAsset("fixture", relative, "resource", 4, ResourceHash.Sha256(path));
            var manifest = new ImportManifest(1, sourceHash, [valid]);
            Assert.True(ImportManifestVerifier.Verify(root, manifest).IsValid);

            File.WriteAllBytes(path, [4, 3, 2, 1]);
            var damaged = ImportManifestVerifier.Verify(root, manifest);
            Assert.False(damaged.IsValid);
            Assert.Contains(damaged.Issues, issue => issue.Reason == "SHA-256 mismatch");

            var unsafeManifest = new ImportManifest(1, sourceHash,
            [
                valid,
                valid with { Path = "../escape.bin" },
                valid with { Id = "bad-hash", Path = Path.Combine("Decoded", "other.bin"), Sha256 = "bad" }
            ]);
            var unsafeResult = ImportManifestVerifier.Verify(root, unsafeManifest);
            Assert.False(unsafeResult.IsValid);
            Assert.Contains(unsafeResult.Issues, issue => issue.Reason.Contains("escapes", StringComparison.Ordinal));
            Assert.Contains(unsafeResult.Issues, issue => issue.Reason.Contains("duplicate", StringComparison.Ordinal));
            Assert.Contains(unsafeResult.Issues, issue => issue.Reason.Contains("SHA-256", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GeneratedContentInstallationIsAtomicIncrementalAndManifestScoped()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-install-{Guid.NewGuid():N}");
        try
        {
            var relative = Path.Combine("Decoded", "fixture.bin");
            var first = GeneratedContentInstaller.InstallBytes(root, relative, [1, 2, 3]);
            Assert.True(first.Changed);
            var unchanged = GeneratedContentInstaller.InstallBytes(root, relative, [1, 2, 3]);
            Assert.False(unchanged.Changed);
            var replaced = GeneratedContentInstaller.InstallGenerated(root, relative, stream => stream.Write([4, 5, 6, 7]));
            Assert.True(replaced.Changed);
            Assert.Equal([4, 5, 6, 7], File.ReadAllBytes(replaced.Path));
            Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(replaced.Path)!, "*.tmp"));

            var unlisted = Path.Combine(root, "keep.txt");
            File.WriteAllText(unlisted, "mine");
            var asset = new ImportedAsset("fixture", relative, "resource", replaced.Size, replaced.Sha256);
            var manifest = new ImportManifest(1, new string('a', 64), [asset]);
            manifest.Write(Path.Combine(root, "manifest.json"));

            var unsafeManifest = manifest with { Assets = [asset, asset with { Id = "unsafe", Path = "../outside.bin" }] };
            Assert.Throws<InvalidDataException>(() => ImportedContentUninstaller.Remove(root, unsafeManifest));
            Assert.True(File.Exists(replaced.Path));

            Assert.Equal(1, ImportedContentUninstaller.Remove(root, manifest));
            Assert.False(File.Exists(replaced.Path));
            Assert.False(File.Exists(Path.Combine(root, "manifest.json")));
            Assert.True(File.Exists(unlisted));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SupportedOriginalReleaseIsIdentifiedByExactSourceImageHash()
    {
        Assert.Equal("GOG English release", SupportedOriginalReleases.NameForSourceImage(
            SupportedOriginalReleases.GogEnglishSourceImageSha256.ToUpperInvariant()));
        Assert.Null(SupportedOriginalReleases.NameForSourceImage(new string('0', 64)));
    }

    [Fact]
    public void ImportDiskPlanningAccountsForNewFilesAndAtomicReplacementScratch()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-space-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(Path.Combine(root, "existing.bin"), [1]);
            var plan = ImportDiskPlanner.Calculate(root,
            [
                new("existing.bin", 400),
                new(Path.Combine("new", "one.bin"), 100),
                new(Path.Combine("new", "two.bin"), 200)
            ]);
            Assert.Equal((700, 300, 400, 700),
                (plan.InstalledBytes, plan.NewBytes, plan.ReplacementScratchBytes, plan.RequiredAvailableBytes));
            Assert.Throws<InvalidDataException>(() => ImportDiskPlanner.Calculate(root,
                [new("same.bin", 1), new("SAME.BIN", 1)]));
            Assert.Throws<InvalidDataException>(() => ImportDiskPlanner.Calculate(root,
                [new("../escape.bin", 1)]));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GameSettingsPersistAndRecoverThePreviousValidGeneration()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-settings-{Guid.NewGuid():N}");
        try
        {
            var path = Path.Combine(root, "settings.json");
            var store = new GameSettingsStore(path);
            Assert.Equal(new GameSettings(), store.Load());

            var first = new GameSettings
            {
                CdMusic = false,
                SoundEffects = true,
                Speech = false,
                Animation = true,
                Fullscreen = true,
                MusicVolume = .7f,
                EffectsVolume = .4f,
                SpeechVolume = .8f,
                ReducedMotion = true
            };
            store.Save(first);
            Assert.Equal(first, store.Load());
            var second = first with { SoundEffects = false, Fullscreen = false };
            store.Save(second);
            Assert.True(File.Exists(store.BackupPath));
            Assert.Equal(second, store.Load());

            File.WriteAllText(path, "corrupt");
            Assert.Equal(first, store.Load());
            File.WriteAllText(store.BackupPath, "corrupt too");
            Assert.Equal(new GameSettings(), store.Load());
            Assert.Throws<InvalidDataException>(() => store.Save(first with
                { Version = GameSettingsStore.CurrentVersion + 1 }));

            var legacyJson = "{\"Version\":1,\"CdMusic\":false,\"Fullscreen\":true}";
            File.WriteAllText(path, legacyJson);
            File.Delete(store.BackupPath);
            var migrated = store.Load();
            Assert.Equal(GameSettingsStore.CurrentVersion, migrated.Version);
            Assert.False(migrated.CdMusic);
            Assert.True(migrated.Fullscreen);
            Assert.Equal(.35f, migrated.MusicVolume);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void PresentationScalingPreservesAspectRatioAndReversesPointerCoordinates()
    {
        Assert.Equal(new UiBounds(240, 0, 1440, 1080), PresentationScaling.Destination(1920, 1080, false));
        Assert.Equal(new UiBounds(448, 156, 1024, 768), PresentationScaling.Destination(1920, 1080, true));
        Assert.Equal(new UiBounds(0, 0, 800, 600), PresentationScaling.Destination(800, 600, true));

        var destination = PresentationScaling.Destination(1920, 1080, false);
        Assert.Equal((0, 0), PresentationScaling.ToVirtual(destination.X, destination.Y, destination));
        Assert.Equal((512, 384), PresentationScaling.ToVirtual(960, 540, destination));
        Assert.Equal((1023, 767), PresentationScaling.ToVirtual(destination.X + destination.Width - 1,
            destination.Y + destination.Height - 1, destination));
        Assert.True(PresentationScaling.ToVirtual(0, 0, destination).X < 0);
        Assert.True(PresentationScaling.ToLogical(destination.X - 1, destination.Y, destination, 640, 480).X < 0);
    }

    [Fact]
    public void DilemmaTextIsParsedIntoDataDrivenChoicesAndOutcomes()
    {
        var dilemma = DilemmaTextDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(SyntheticDilemma()));

        Assert.Equal((7, 12, "SYNTHETIC", "D777.CSF", "A synthetic prompt continues here."), (dilemma.Number, dilemma.Age, dilemma.Title, dilemma.SceneFile, dilemma.Prompt));
        Assert.Equal(3, dilemma.Choices.Count);
        Assert.All(dilemma.Choices, choice => Assert.Equal(3, choice.Outcomes.Count));
        Assert.Equal(("STRENGTH", 17, 6), (dilemma.Choices[0].ScoringAttribute, dilemma.Choices[0].HighBreakpoint, dilemma.Choices[0].LowBreakpoint));
        Assert.Equal(new DilemmaAttributeChange("HONOR", 2), dilemma.Choices[0].Outcomes.Single(x => x.Outcome == DilemmaOutcome.Win).Changes.Single());
    }

    [Fact]
    public void ImportedDilemmaDefinitionsMapThroughTypedAttributes()
    {
        var resource = DilemmaTextDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(SyntheticDilemma()));

        var definition = Assert.IsType<YouthDilemmaDefinition>(ImportedDilemmaAdapter.Convert(resource));

        Assert.Equal(CharacterAttribute.Strength, definition.Choices[0].ScoringAttribute);
        Assert.Equal("D777.CSF", definition.SceneFile);
        Assert.Equal(CharacterAttribute.Honor,
            definition.Choices[0].Outcomes[YouthDilemmaOutcome.Win].Changes.Single().Attribute);
    }

    [Theory]
    [InlineData(17, YouthDilemmaOutcome.Win)]
    [InlineData(6, YouthDilemmaOutcome.Draw)]
    [InlineData(5, YouthDilemmaOutcome.Lose)]
    public void DilemmaBreakpointsUseInclusiveOrderedBands(int score, YouthDilemmaOutcome expected)
    {
        var choice = new YouthDilemmaChoiceDefinition("Choice", CharacterAttribute.Strength, 6, 17,
            new Dictionary<YouthDilemmaOutcome, YouthDilemmaOutcomeDefinition>());

        Assert.Equal(expected, YouthDilemmaRules.Resolve(choice, score));
    }

    [Fact]
    public void CampaignPersistsSelectionAndAppliesImportedOutcomeChanges()
    {
        var state = Campaign.NewCustom("Test", 42);
        state.Player.Stats = state.Player.Stats with { Strength = 17, Intelligence = 8 };
        var campaign = new Campaign(state, 42);
        var number = campaign.CurrentYouthDilemmaNumber;
        var changes = new CharacterAttributeChange[]
        {
            new(CharacterAttribute.Strength, 1),
            new(CharacterAttribute.Intelligence, 2),
            new(CharacterAttribute.SwordExperience, 1),
            new(CharacterAttribute.Age, 1)
        };
        var choice = new YouthDilemmaChoiceDefinition("Choice", CharacterAttribute.Strength, 6, 17,
            new Dictionary<YouthDilemmaOutcome, YouthDilemmaOutcomeDefinition>
            {
                [YouthDilemmaOutcome.Win] = new("Won", changes),
                [YouthDilemmaOutcome.Draw] = new("Drew", []),
                [YouthDilemmaOutcome.Lose] = new("Lost", [])
            });
        var definition = new YouthDilemmaDefinition(number, 12, "Title", "Prompt", [choice]);

        Assert.Equal(number, campaign.CurrentYouthDilemmaNumber);
        var restoredState = System.Text.Json.JsonSerializer.Deserialize<CampaignState>(
            System.Text.Json.JsonSerializer.Serialize(state));
        Assert.Equal(number, Assert.IsType<CampaignState>(restoredState).ActiveYouthDilemmaNumber);
        var result = Assert.IsType<YouthDilemmaResult>(campaign.AnswerDilemma(definition, 0));

        Assert.InRange(number, 0, 4);
        Assert.Equal(YouthDilemmaOutcome.Win, result.Outcome);
        Assert.Equal((18, 10, 1, 13), (state.Player.Stats.Strength, state.Player.Stats.Intelligence,
            state.Player.SwordExperience, state.Player.Age));
        Assert.Equal(1, state.YouthDilemmasAnswered);
        Assert.Null(state.ActiveYouthDilemmaNumber);
        Assert.InRange(campaign.CurrentYouthDilemmaNumber, 5, 9);
    }

    [Fact]
    public void DilemmaTextRejectsUnboundedOrIncompleteData()
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(SyntheticDilemma());
        Assert.Throws<InvalidDataException>(() => DilemmaTextDecoder.Decode(bytes, bytes.Length - 1));
        Assert.Throws<InvalidDataException>(() => DilemmaTextDecoder.Decode("!HEADER\n7 D777.CSF\n"u8));
        Assert.Throws<InvalidDataException>(() => DilemmaTextDecoder.Decode([0xff]));
    }

    private static string SyntheticDilemma()
    {
        var text = new System.Text.StringBuilder("# AGE: 12\r\n# TITLE: SYNTHETIC\r\n!DILEMMA_NUMBER DILEMMA_SFG_FILE\r\n7 D777.CSF\r\n&DILEMMA TEXT\r\n^A synthetic prompt\r\n^continues here.\r\n");
        var attributes = new[] { "STRENGTH", "DEXTERITY", "NONE" };
        foreach (var choice in Enumerable.Range(1, 3))
        {
            text.Append("@RELEVANT SCORING ATTRIBUTE\r\n~").Append(attributes[choice - 1]).Append("\r\n");
            text.Append("%HIGH SCORING BREAKPOINT LOW SCORING BREAKPOINT\r\n17 6\r\n");
            foreach (var outcome in Enum.GetNames<DilemmaOutcome>())
            {
                text.Append("?DILEMMA CHOICE ").Append(choice).Append(' ').Append(outcome.ToUpperInvariant()).Append(" TEXT\r\n");
                text.Append("^Synthetic outcome text.\r\n*NUMBER OF ATTRIBUTES MODIFIED\r\n1\r\n$ATTRIBUTE MODIFIER\r\nHONOR 2\r\n");
            }
        }
        return text.Append('\u001a').ToString();
    }

    private static byte[] ConversationNode(
        IReadOnlyList<string> strings,
        IReadOnlyList<int> targets,
        int continuationNodeId = 0,
        IReadOnlyList<int>? nodeActions = null,
        IReadOnlyList<IReadOnlyList<int>>? responseActions = null)
    {
        var encoded = strings.Select(System.Text.Encoding.ASCII.GetBytes).ToArray();
        var result = new byte[0x348 + encoded.Sum(bytes => bytes.Length + 1)];
        result[0x48] = checked((byte)targets.Count);
        result[0x08] = checked((byte)(strings.Count == 0 ? 0 : strings.Count - 2 - targets.Count));
        result[0x49] = 0x65; result[0x4a] = 0x3a; result[0x4b] = 0x5c;
        for (var response = 0; response < 5; response++)
        for (var action = 0; action < 30; action++)
            WriteInt(result, 0x60 + response * 0x78 + action * 4, -1);
        for (var action = 0; action < 30; action++) WriteInt(result, 0x2b8 + action * 4, -1);
        if (targets.Count == 0) WriteInt(result, 0x4c, continuationNodeId);
        for (var index = 0; index < targets.Count; index++) WriteInt(result, 0x4c + index * 4, targets[index]);
        for (var index = 0; index < (nodeActions?.Count ?? 0); index++) WriteInt(result, 0x2b8 + index * 4, nodeActions![index]);
        for (var response = 0; response < (responseActions?.Count ?? 0); response++)
        for (var action = 0; action < responseActions![response].Count; action++)
            WriteInt(result, 0x60 + response * 0x78 + action * 4, responseActions[response][action]);
        var position = 0x348;
        foreach (var bytes in encoded)
        {
            bytes.CopyTo(result, position);
            position += bytes.Length + 1;
        }
        return result;
    }

    private static void WriteInt(byte[] target, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, 4), value);

    private static void WriteInteraction(byte[] blocks, int index, short selector, short argument, short argument2)
    {
        var offset = index * DynamixSceneDecoder.BlockSize;
        BinaryPrimitives.WriteInt16LittleEndian(blocks.AsSpan(offset + 0x48, 2), selector);
        BinaryPrimitives.WriteInt16LittleEndian(blocks.AsSpan(offset + 0x4a, 2), argument);
        BinaryPrimitives.WriteInt16LittleEndian(blocks.AsSpan(offset + 0x4c, 2), argument2);
    }

    private static (byte[] Viewer, byte[] Scenario, byte[] Map, byte[] Blocks) SyntheticScene()
    {
        var names = new[] { "ground", "arched door", "Secret Passage", "meal", "bag of coins", "knight", "champion" };
        var viewer = new byte[DynamixSceneDecoder.ViewerSize];
        WriteInt(viewer, 0, 10 << 8);
        WriteInt(viewer, 4, 20 << 8);
        WriteInt(viewer, 8, 48);
        WriteInt(viewer, 12, 16384);

        var scenario = new byte[DynamixSceneDecoder.ScenarioSize];
        WriteInt(scenario, 20, 32);
        WriteInt(scenario, 24, names.Length);
        WriteInt(scenario, 28, 1);
        WriteInt(scenario, 40, 1);
        WriteInt(scenario, 44, 32);
        WriteInt(scenario, 48, 10);
        WriteInt(scenario, 52, 20);

        var blocks = new byte[names.Length * DynamixSceneDecoder.BlockSize];
        for (var index = 0; index < names.Length; index++)
        {
            var offset = index * DynamixSceneDecoder.BlockSize;
            System.Text.Encoding.ASCII.GetBytes(names[index]).CopyTo(blocks, offset + 78);
            WriteInt(blocks, offset + 44, 12 + index);
            WriteInt(blocks, offset + 12, 2);
            WriteInt(blocks, offset + 48, 13 + index);
            WriteInt(blocks, offset + 52, 14 + index);
            WriteInt(blocks, offset + 56, 15 + index);
            blocks[offset + 94] = 0xcc;
            blocks[offset + 95] = 0xcc;
        }
        WriteInt(blocks, 5 * DynamixSceneDecoder.BlockSize + 4, 135);
        WriteInteraction(blocks, 5, 1, 3, 0);
        WriteInt(blocks, 6 * DynamixSceneDecoder.BlockSize + 4, 135);
        WriteInteraction(blocks, 6, 1, 9, 0);

        var map = new byte[DynamixSceneDecoder.MapSize];
        SetSceneCell(map, 11, 20, 1);
        SetSceneCell(map, 10, 21, 2);
        SetSceneCell(map, 12, 20, 3);
        SetSceneCell(map, 9, 20, 4);
        SetSceneCell(map, 13, 20, 5);
        SetSceneCell(map, 14, 20, 6);
        return (viewer, scenario, map, blocks);
    }

    private static void SetSceneCell(byte[] map, int x, int y, ushort block) =>
        BinaryPrimitives.WriteUInt16LittleEndian(map.AsSpan((x * DynamixScene.MapHeight + y) * 2, 2), block);

    private static void SetSceneBlockName(byte[] blocks, int index, string name)
    {
        var field = blocks.AsSpan(index * DynamixSceneDecoder.BlockSize + 78, 16);
        field.Clear();
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(field);
    }

    private sealed class TestActionState(IReadOnlyList<int> initial) : IDynamixActionState
    {
        public List<int> Variables { get; } = [.. initial];
        private readonly Dictionary<int, int> _items = [];
        public bool TryGetVariable(int scope, int index, out int value)
        {
            value = 0;
            if (scope != 0 || (uint)index >= (uint)Variables.Count) return false;
            value = Variables[index];
            return true;
        }
        public bool TrySetVariable(int scope, int index, int value)
        {
            if (scope != 0 || (uint)index >= (uint)Variables.Count) return false;
            Variables[index] = value;
            return true;
        }
        public bool TryAddItem(int index) { _items[index] = _items.GetValueOrDefault(index) + 1; return true; }
        public bool TryClearItem(int index) => _items.Remove(index);
        public bool HasItem(int index) => _items.GetValueOrDefault(index) != 0;
    }

    private sealed class MaximumRandom : Random
    {
        public override int Next(int maxValue) => maxValue - 1;
    }

    private sealed class FixedRandom(int value) : Random
    {
        public override int Next(int maxValue) => Math.Clamp(value, 0, maxValue - 1);
    }

    private static byte[] SyntheticSmacker()
    {
        var source = new byte[142];
        "SMK2"u8.CopyTo(source);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(4, 4), 196);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(8, 4), 204);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(12, 4), 2);
        BinaryPrimitives.WriteInt32LittleEndian(source.AsSpan(16, 4), 100);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(24, 4), 4096);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(52, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(56, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(60, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(64, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(68, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(72, 4), 0xC000_0000u | 22050);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(104, 4), 13);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(108, 4), 12);
        source[112] = 1;
        source[113] = 2;
        source[118] = 2;
        source[122] = 0xFE;
        source[123] = 0xFF;
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(130, 4), 8);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(134, 4), 3);
        return source;
    }

    private static byte[] SyntheticSmackerVideo()
    {
        var source = new byte[120];
        "SMK2"u8.CopyTo(source);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(4, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(8, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(12, 4), 1);
        BinaryPrimitives.WriteInt32LittleEndian(source.AsSpan(16, 4), 100);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(52, 4), 7);
        for (var offset = 56; offset <= 68; offset += 4)
            BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(offset, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(104, 4), 5);
        source[109] = 0x08;
        source[115] = 0x80;
        return source;
    }

    private static byte[] PackMsbCodes(IEnumerable<(int Code, int Width)> codes)
    {
        var values = codes.ToArray();
        var result = new byte[(values.Sum(value => value.Width) + 7) / 8];
        var bitPosition = 0;
        foreach (var (code, width) in values)
            for (var bit = width - 1; bit >= 0; bit--, bitPosition++)
                if ((code & (1 << bit)) != 0) result[bitPosition >> 3] |= (byte)(1 << (7 - (bitPosition & 7)));
        return result;
    }

    [Fact]
    public void BalanceDefinitionsRemainInternallyConsistent()
    {
        Assert.All(Balance.Buildings, x => Assert.Equal(x.Key, x.Value.Kind));
        Assert.Equal(UnitType.Swordsmen, Balance.Counter(UnitType.Knights));
        Assert.Equal(UnitType.Halberdiers, Balance.Counter(UnitType.Swordsmen));
        Assert.Equal(UnitType.Knights, Balance.Counter(UnitType.Halberdiers));
    }
}
