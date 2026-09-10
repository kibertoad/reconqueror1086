using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData("scene.RES", true)]
    [InlineData("scene.low", true)]
    [InlineData("image.PCX", false)]
    public void DynamixContainerExtensionsIncludeLowSceneArchives(string path, bool expected)
    {
        Assert.Equal(expected, DynamixArchive.HasContainerExtension(path));
    }

    [Fact]
    public void DecodedArchiveFoldersPreserveExtensionsToSeparateSceneTiers()
    {
        Assert.Equal("BAR0.LOW", ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.LOW"));
        Assert.Equal("BAR0.RES", ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.RES"));
        Assert.NotEqual(ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.LOW"),
            ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.RES"));
        Assert.Throws<InvalidDataException>(() => ResourcePaths.DecodedArchiveFolder(".."));
    }
    [Theory]
    [InlineData("TEX000 16 16", 256, 0, 16, 16)]
    [InlineData("TEX081 128 156", 19968, 81, 128, 156)]
    [InlineData("tex241 64 91", 5824, 241, 64, 91)]
    public void SceneTextureNamesBoundRawIndexedPixelDimensions(
        string name, int length, int index, int width, int height)
    {
        var texture = DynamixSceneTextureDecoder.Decode(name, new byte[length]);
        Assert.Equal((index, width, height, length),
            (texture.Index, texture.Width, texture.Height, texture.Indices.Length));
    }

    [Fact]
    public void SceneTextureDecoderRejectsMalformedOrInconsistentResources()
    {
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("PIC000 16 16", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX+01 16 16", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX000 +16 16", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX000 16 16", new byte[255]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX000 11", new byte[11]));
    }

    [Fact]
    public void SceneBackdropDecoderBindsDescriptorGeometryToExactImage()
    {
        var descriptor = new byte[DynamixSceneBackdropDecoder.DescriptorSize];
        WriteInt(descriptor, 0, 1);
        WriteInt(descriptor, 8, 4);
        WriteInt(descriptor, 12, 2);
        WriteInt(descriptor, 16, 1);
        WriteInt(descriptor, 20, 3);

        var backdrop = DynamixSceneBackdropDecoder.Decode(descriptor, new byte[8]);

        Assert.Equal((4, 2, 1, 3, 8),
            (backdrop.Width, backdrop.Height, backdrop.Horizon, backdrop.Mode, backdrop.Indices.Length));
        Assert.Throws<InvalidDataException>(() => DynamixSceneBackdropDecoder.Decode(descriptor, new byte[7]));
        WriteInt(descriptor, 16, 2);
        Assert.Throws<InvalidDataException>(() => DynamixSceneBackdropDecoder.Decode(descriptor, new byte[8]));
    }

    [Fact]
    public void SceneColorMapsRequireTheCompleteCanonicalIndexRamp()
    {
        var decoded = Enumerable.Range(0, DynamixSceneColorMaps.Count)
            .Select(index => DynamixSceneColorMapDecoder.Decode($"Pal{index}",
                Enumerable.Range(0, DynamixSceneColorMaps.EntryCount).Select(value => (byte)value).ToArray()))
            .ToArray();

        var maps = new DynamixSceneColorMaps(decoded.Reverse());

        Assert.Equal(73, maps[9].Span[73]);
        Assert.Throws<InvalidDataException>(() => new DynamixSceneColorMaps(decoded[..^1]));
        Assert.Throws<InvalidDataException>(() => new DynamixSceneColorMaps(decoded.Append(decoded[0])));
        Assert.Throws<InvalidDataException>(() => DynamixSceneColorMapDecoder.Decode("Pal128", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneColorMapDecoder.Decode("pal0", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneColorMapDecoder.Decode("Pal0", new byte[255]));
    }

    [Fact]
    public void SiegeWallDistanceUsesTheScenarioShiftCountAndBlockOffset()
    {
        var melee = new DynamixSceneColorMapping(true, 32, 10, 20);
        Assert.Equal(0, SiegeColorMapping.WallDistanceMap(0, melee, 0));
        Assert.Equal(0, SiegeColorMapping.WallDistanceMap(0.999, melee, 0));
        Assert.Equal(1, SiegeColorMapping.WallDistanceMap(1, melee, 0));
        Assert.Equal(13, SiegeColorMapping.WallDistanceMap(15.9, melee, 2));
        Assert.Equal(31, SiegeColorMapping.WallDistanceMap(10_000, melee, 0));
        var room = melee with { DistanceShift = 8 };
        Assert.Equal(6, SiegeColorMapping.WallDistanceMap(2.1, room, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => SiegeColorMapping.WallDistanceMap(-0.1, melee, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SiegeColorMapping.WallDistanceMap(double.NaN, melee, 0));
    }

    [Theory]
    [InlineData("Basic Axe", 27, 3)]
    [InlineData("Heavy Crossbow", 32, 1)]
    [InlineData("War Hammer", 33, 3)]
    [InlineData("Spiked Mace", 36, 3)]
    [InlineData("Battle Sword", 39, 3)]
    [InlineData("Stiletto Dagger", 42, 1)]
    [InlineData(null, 39, 3)]
    public void EquippedWeaponSelectsItsOriginalFirstPersonFrameRun(string? weapon, int start, int count)
    {
        var run = SiegeCombatPresentation.AttackFramesFor(weapon);
        Assert.Equal((start, count, start + count), (run.Start, run.Count, run.EndExclusive));
        Assert.True(run.EndExclusive <= SiegeCombatPresentation.FatalHitBlood.Start);
        Assert.Equal((320, 200), (SiegeCombatPresentation.OriginalWidth, SiegeCombatPresentation.OriginalHeight));
        Assert.True(SiegeCombatPresentation.Viewport.X + SiegeCombatPresentation.Viewport.Width <=
                    SiegeCombatPresentation.OriginalWidth);
        Assert.True(SiegeCombatPresentation.Viewport.Y + SiegeCombatPresentation.Viewport.Height <=
                    SiegeCombatPresentation.OriginalHeight);
    }

    [Fact]
    public void RawIndexedScreensRequireExactBoundedPlanesAndPalettes()
    {
        var palette = new byte[IndexedPalette.ByteSize];
        palette[3] = 10;
        palette[4] = 20;
        palette[5] = 30;
        var image = RawIndexedImageDecoder.Decode([1, 0], palette, 2, 1);

        Assert.Equal((2, 1), (image.Width, image.Height));
        Assert.Equal(new byte[] { 10, 20, 30, 255, 0, 0, 0, 255 }, image.ToRgba());
        Assert.Throws<InvalidDataException>(() => RawIndexedImageDecoder.Decode([1], palette, 2, 1));
        Assert.Throws<InvalidDataException>(() => RawIndexedImageDecoder.Decode([1, 0], palette, 2, 1, 1));
        Assert.Throws<InvalidDataException>(() => RawIndexedImageDecoder.Decode([1, 0], palette[..^1], 2, 1));
    }

    [Fact]
    public void SceneColorMapsRemapRgbWithoutTurningMappedBlackTransparent()
    {
        var palette = new byte[IndexedPalette.ByteSize];
        palette[3] = 10;
        palette[4] = 11;
        palette[5] = 12;
        palette[6] = 20;
        palette[7] = 21;
        palette[8] = 22;
        var colorMap = Enumerable.Range(0, DynamixSceneColorMaps.EntryCount).Select(value => (byte)value).ToArray();
        colorMap[1] = 2;
        colorMap[2] = 0;

        var rgba = IndexedScenePixels.ToRgba([0, 1, 2], palette, colorMap: colorMap);

        Assert.Equal([0, 0, 0, 0, 20, 21, 22, 255, 0, 0, 0, 255], rgba);
        Assert.Throws<InvalidDataException>(() => IndexedScenePixels.ToRgba([1], palette, colorMap: new byte[255]));
    }

    [Fact]
    public void SceneDecoderReadsColumnMajorMapViewerAndNamedBlocks()
    {
        var source = SyntheticScene();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        Assert.Equal((10, 20, 16384), (scene.Viewer.CellX, scene.Viewer.CellY, scene.Viewer.Heading));
        Assert.Equal((7, 32, 1), (scene.Blocks.Count, scene.TextureCount, scene.SoundEffectCount));
        Assert.Equal((new DynamixSceneColorMapping(true, 32, 10, 20), 2),
            (scene.ColorMapping, scene.Blocks[0].ColorMapOffset));
        Assert.Equal("arched door", scene.BlockAt(11, 20).Name);
        Assert.Equal("Secret Passage", scene.BlockAt(10, 21).Name);
        Assert.Equal("meal", scene.BlockAt(12, 20).Name);
        Assert.Equal("knight", scene.BlockAt(13, 20).Name);
        Assert.Equal("champion", scene.BlockAt(14, 20).Name);
        Assert.Equal((ushort)0, scene.BlockIndexAt(20, 11));
        Assert.Equal((12, 13, 14, 15), (scene.Blocks[0].Surface0, scene.Blocks[0].Surface1,
            scene.Blocks[0].Surface2, scene.Blocks[0].Surface3));
        Assert.Equal([12, 13, 14, 15], Enum.GetValues<DynamixSceneFace>()
            .Select(scene.Blocks[0].TextureForFace));
    }

    [Fact]
    public void SceneDecoderRejectsMalformedRecordsReferencesAndViewer()
    {
        var source = SyntheticScene();
        var missingSentinel = source.Blocks.ToArray();
        missingSentinel[94] = 0;
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, missingSentinel));

        var missingBlock = source.Map.ToArray();
        BinaryPrimitives.WriteUInt16LittleEndian(missingBlock.AsSpan(0, 2), 7);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, missingBlock, source.Blocks));

        var outsideViewer = source.Viewer.ToArray();
        WriteInt(outsideViewer, 0, 128 << 8);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            outsideViewer, source.Scenario, source.Map, source.Blocks));

        var missingTexture = source.Blocks.ToArray();
        WriteInt(missingTexture, 44, 32);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, missingTexture));
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks[..^1]));
    }

    [Fact]
    public void SceneBillboardsExposeOnlyTheirPrimaryTextureField()
    {
        var source = SyntheticScene();
        WriteInt(source.Blocks, 0, 4);
        WriteInt(source.Blocks, 44, 12);
        WriteInt(source.Blocks, 48, 0);
        WriteInt(source.Blocks, 52, 36);
        WriteInt(source.Blocks, 56, 190);

        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        Assert.Equal([12], scene.Blocks[0].TextureReferences());
        WriteInt(source.Blocks, 44, 32);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));
    }

    [Fact]
    public void ImportedSceneLayoutPreservesInteractiveTilesSpawnsAndFacing()
    {
        var source = SyntheticScene();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        var layout = ImportedSiegeLayouts.Convert(scene);
        var siege = new SiegeSession(new Player(), new Army(), 999, 17, layout);

        Assert.Equal((128, 128, 10, 20, Facing.East),
            (siege.Width, siege.Height, siege.PlayerX, siege.PlayerY, siege.Facing));
        Assert.Equal(SiegeTile.Door, siege.TileAt(11, 20));
        Assert.Equal(SiegeTile.SecretDoor, siege.TileAt(10, 21));
        Assert.Equal(SiegeTile.Barrel, siege.TileAt(12, 20));
        Assert.Equal(SiegeTile.Treasure, siege.TileAt(9, 20));
        Assert.Equal(2, siege.Enemies.Count);
        Assert.Contains(siege.Enemies, enemy => (enemy.X, enemy.Y, enemy.Champion) == (13, 20, false));
        Assert.Contains(siege.Enemies, enemy => (enemy.X, enemy.Y, enemy.Champion) == (14, 20, true));
        Assert.Equal(5, Assert.Single(siege.Enemies, enemy => !enemy.Champion).VisualId);
        Assert.Equal(6, Assert.Single(siege.Enemies, enemy => enemy.Champion).VisualId);
        Assert.Equal(SiegeSession.Rules.BaseChampionHealth,
            Assert.Single(siege.Enemies, enemy => enemy.Champion).Health);
    }

    [Fact]
    public void CampaignAndPracticeSelectTheirExecutableMeleeSceneFamilies()
    {
        Assert.Equal("MELEE0.RES", ImportedSiegeLayouts.SceneNameForCampaignLocation(1));
        Assert.All(Enumerable.Range(2, 15), location =>
            Assert.Equal("MELEE0.RES", ImportedSiegeLayouts.SceneNameForCampaignLocation(location)));
        Assert.Null(ImportedSiegeLayouts.SceneNameForCampaignLocation(0));
        Assert.Null(ImportedSiegeLayouts.SceneNameForCampaignLocation(17));
        Assert.Null(ImportedSiegeLayouts.SceneNameForCampaignLocation(-1));
        Assert.Null(ImportedSiegeLayouts.SceneNameForCampaignLocation(World.Locations.Length));
        Assert.Equal(["MELEE0.RES", "MELEE1.RES", "MELEE2.RES"],
            Enumerable.Range(0, 3).Select(ImportedSiegeLayouts.SceneNameForPracticeMelee));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImportedSiegeLayouts.SceneNameForPracticeMelee(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => ImportedSiegeLayouts.SceneNameForPracticeMelee(3));
    }

    [Fact]
    public void ImportedSceneLayoutCropsDisconnectedTemplateGallery()
    {
        var source = SyntheticScene();
        SetSceneBlockName(source.Blocks, 0, "wall");
        source.Map.AsSpan().Clear();
        SetSceneCell(source.Map, 10, 20, 3);
        SetSceneCell(source.Map, 11, 20, 1);
        SetSceneCell(source.Map, 12, 20, 5);
        SetSceneCell(source.Map, 30, 30, 6);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var layout = ImportedSiegeLayouts.Convert(scene);
        var tiles = layout.CopyTiles();

        Assert.Equal((5, 3, 1, 1),
            (tiles.GetLength(0), tiles.GetLength(1), layout.PlayerX, layout.PlayerY));
        Assert.Equal(SiegeTile.Door, tiles[2, 1]);
        var enemy = Assert.Single(layout.Enemies);
        Assert.Equal((3, 1, false), (enemy.X, enemy.Y, enemy.Champion));
    }

    [Fact]
    public void FirstPersonProjectionUsesLayoutWallsFacingAndEnemyPositions()
    {
        var tiles = new SiegeTile[6, 5];
        for (var x = 0; x < 6; x++)
        for (var y = 0; y < 5; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 5 || y == 4 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[4, 2] = SiegeTile.Door;
        var layout = new SiegeLayout(tiles, 1, 2, Facing.East, [new SiegeSpawn(2, 2, false)]);
        var siege = new SiegeSession(new Player(), new Army(), 0, 9, layout);

        var center = SiegeViewProjection.Cast(siege, 0);
        var enemy = Assert.Single(SiegeViewProjection.ProjectEnemies(siege));

        Assert.Equal(SiegeTile.Door, center.Tile);
        Assert.InRange(center.Distance, 2.49, 2.51);
        Assert.True(center.HitVerticalSide);
        Assert.Equal(SiegeWallFace.West, center.Face);
        Assert.Equal((4, 2), (center.MapX, center.MapY));
        Assert.InRange(center.TextureOffset, 0.499, 0.501);
        Assert.InRange(enemy.ScreenPosition, 0.499, 0.501);
        Assert.Equal(1, enemy.ForwardDistance);
    }

    [Fact]
    public void FirstPersonProjectionIdentifiesTheContactedWallFace()
    {
        var tiles = new SiegeTile[5, 5];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 5; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 4 ? SiegeTile.Wall : SiegeTile.Floor;

        foreach (var (facing, expected) in new[]
        {
            (Facing.North, SiegeWallFace.South),
            (Facing.East, SiegeWallFace.West),
            (Facing.South, SiegeWallFace.North),
            (Facing.West, SiegeWallFace.East)
        })
        {
            var layout = new SiegeLayout(tiles, 2, 2, facing, []);
            var siege = new SiegeSession(new Player(), new Army(), 0, 9, layout);
            Assert.Equal(expected, SiegeViewProjection.Cast(siege, 0).Face);
        }
    }

    [Fact]
    public void SiegeDoorsRemainSolidWhileTheirOpeningStateAdvances()
    {
        var tiles = new SiegeTile[4, 3];
        for (var x = 0; x < 4; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 3 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        tiles[2, 1] = SiegeTile.Door;
        var siege = new SiegeSession(new Player(), new Army(), 0, 1,
            new SiegeLayout(tiles, 1, 1, Facing.East, []));

        Assert.Equal(SiegeAction.DoorOpened, siege.Interact());
        Assert.Equal(SiegeTile.OpeningDoor, siege.TileAt(2, 1));
        Assert.Equal(0, siege.DoorOpeningProgress(2, 1));
        Assert.Equal(SiegeAction.Blocked, siege.Move(true));
        siege.AdvanceDoorAnimations(SiegeSession.DoorOpeningSeconds / 2);
        Assert.InRange(siege.DoorOpeningProgress(2, 1)!.Value, 0.49, 0.51);
        Assert.Equal(SiegeTile.OpeningDoor, SiegeViewProjection.Cast(siege, 0).Tile);
        siege.AdvanceDoorAnimations(SiegeSession.DoorOpeningSeconds / 2);
        Assert.Null(siege.DoorOpeningProgress(2, 1));
        Assert.Equal(SiegeTile.Floor, siege.TileAt(2, 1));
        Assert.Equal(SiegeAction.Moved, siege.Move(true));
        Assert.Throws<ArgumentOutOfRangeException>(() => siege.AdvanceDoorAnimations(-0.1));
    }

    [Fact]
    public void EnemyBillboardFramesSelectFiveAnglesAndMirrorTheOtherSide()
    {
        var enemy = new SiegeEnemy { X = 10, Y = 10, Facing = Facing.North };

        Assert.Equal(new SiegeEnemyFrame(4, false), SiegeViewProjection.FrameFor(enemy, 10, 8));
        Assert.Equal(new SiegeEnemyFrame(3, false), SiegeViewProjection.FrameFor(enemy, 9, 9));
        Assert.Equal(new SiegeEnemyFrame(2, false), SiegeViewProjection.FrameFor(enemy, 8, 10));
        Assert.Equal(new SiegeEnemyFrame(1, false), SiegeViewProjection.FrameFor(enemy, 9, 11));
        Assert.Equal(new SiegeEnemyFrame(0, false), SiegeViewProjection.FrameFor(enemy, 10, 12));
        Assert.Equal(new SiegeEnemyFrame(3, true), SiegeViewProjection.FrameFor(enemy, 11, 9));
        Assert.Equal(new SiegeEnemyFrame(2, true), SiegeViewProjection.FrameFor(enemy, 12, 10));
        Assert.Equal(new SiegeEnemyFrame(1, true), SiegeViewProjection.FrameFor(enemy, 11, 11));
    }

    [Fact]
    public void AdjacentEnemiesAdvanceThroughACompleteAttackSequence()
    {
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        var siege = new SiegeSession(new Player(), new Army(), 0, 7,
            new SiegeLayout(tiles, 1, 1, Facing.East, [new SiegeSpawn(2, 1, true, 99)]));
        var enemy = Assert.Single(siege.Enemies);

        siege.Move(false);
        Assert.Equal(SiegeEnemyVisualState.Attack, enemy.VisualState);
        Assert.Equal(Facing.West, enemy.Facing);
        siege.AdvanceEnemyAnimations(SiegeSession.EnemyAttackFrameSeconds * 4.1);
        Assert.Equal(4, enemy.VisualFrame);
        siege.AdvanceEnemyAnimations(SiegeSession.EnemyAttackFrameSeconds * 5);
        Assert.Equal(SiegeEnemyVisualState.Walk, enemy.VisualState);
        Assert.Equal(0, enemy.VisualFrame);
        Assert.Throws<ArgumentOutOfRangeException>(() => siege.AdvanceEnemyAnimations(double.NaN));
    }

    [Fact]
    public void DefeatedEnemiesStopBlockingButFinishTheirCollapseBeforeVictory()
    {
        var player = new Player();
        player.Inventory.Weapon = "Heavy Crossbow";
        player.Inventory.CrossbowBolts = 1;
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        var siege = new SiegeSession(player, new Army(), 0, 3,
            new SiegeLayout(tiles, 1, 1, Facing.East, [new SiegeSpawn(2, 1, false, 99)]));

        Assert.Equal(SiegeAction.Shot, siege.Shoot());
        var dying = Assert.Single(siege.Enemies);
        Assert.Equal(SiegeEnemyVisualState.Dying, dying.VisualState);
        Assert.Null(siege.EnemyAt(2, 1));
        Assert.False(siege.Won);
        siege.AdvanceEnemyAnimations(SiegeSession.EnemyDeathFrameSeconds * 4.1);
        Assert.Equal(4, dying.VisualFrame);
        siege.AdvanceEnemyAnimations(SiegeSession.EnemyDeathFrameSeconds * 4);
        Assert.Empty(siege.Enemies);
        Assert.True(siege.Won);
    }

    [Fact]
    public void ConversationDatabaseDecodesIndexedPromptsResponsesAndLinks()
    {
        var first = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings.", "Ask about the dragon.", "Farewell."],
            [1102, 0], nodeActions: [10, 11], responseActions: [[20], []]);
        var second = ConversationNode(["BARKEEP.PCC", "Bartender", "Good day."], [], 1101);
        var body = first.Concat(second).ToArray();
        var index = new byte[16];
        WriteInt(index, 0, 1102); WriteInt(index, 4, first.Length);
        WriteInt(index, 8, 1101); WriteInt(index, 12, 0);

        var database = DynamixConversationDecoder.Decode(body, index);

        Assert.Equal(2, database.Nodes.Count);
        var gerard = Assert.IsType<DynamixConversationNode>(database.Find(1101));
        Assert.Equal(("GERARD.PCC", "Earl Gerard"), (gerard.PortraitFile, gerard.Speaker));
        Assert.Equal(["Greetings."], gerard.PromptVariants);
        Assert.Equal(["Ask about the dragon.", "Farewell."], gerard.Responses.Select(response => response.Text));
        Assert.Equal([1102, 0], gerard.Responses.Select(response => response.TargetNodeId));
        Assert.Equal([20], gerard.Responses[0].ActionIds);
        Assert.Empty(gerard.Responses[1].ActionIds);
        Assert.Equal([10, 11], gerard.ActionIds);
        var barkeep = Assert.IsType<DynamixConversationNode>(database.Find(1102));
        Assert.Equal(("Bartender", 1101), (barkeep.Speaker, barkeep.ContinuationNodeId));
        Assert.Null(gerard.ContinuationNodeId);
    }

    [Fact]
    public void ConversationDatabaseRejectsMalformedIndicesMarkersTextAndLinks()
    {
        var valid = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings.", "Continue."], [0]);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(valid, new byte[7]));

        var duplicateIndex = new byte[16];
        WriteInt(duplicateIndex, 0, 1); WriteInt(duplicateIndex, 4, 0);
        WriteInt(duplicateIndex, 8, 1); WriteInt(duplicateIndex, 12, 0);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(valid, duplicateIndex));

        var oneIndex = new byte[8];
        WriteInt(oneIndex, 0, 1); WriteInt(oneIndex, 4, 0);
        var badMarker = valid.ToArray(); badMarker[0x49] = 0;
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badMarker, oneIndex));
        var badPromptCount = valid.ToArray(); badPromptCount[0x08]++;
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badPromptCount, oneIndex));
        var badText = valid.ToArray(); badText[^1] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badText, oneIndex));
        var badLink = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings.", "Continue."], [99]);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badLink, oneIndex));
        var badContinuation = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings."], [], 99);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badContinuation, oneIndex));
        var badAction = valid.ToArray(); WriteInt(badAction, 0x2b8, -2);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badAction, oneIndex));
    }

    [Fact]
    public void ImportedConversationSessionTraversesSelectorsResponsesAndTimedContinuations()
    {
        var nodes = new Dictionary<int, DynamixConversationNode>
        {
            [1] = new(1, 0, null, null, [], [], 2, []),
            [2] = new(2, 0, "GERARD.PCC", "Earl Gerard", ["First.", "Second."],
                [new DynamixConversationResponse("Continue.", 3, [])], null, []),
            [3] = new(3, 0, "BARKEEP.PCC", "Bartender", ["Farewell."], [], 0, [])
        };
        var session = new ImportedConversationSession(new DynamixConversationDatabase(nodes));

        Assert.True(session.Start(1, count => count - 1));
        Assert.Equal((2, "Second."), (session.CurrentNode?.Id, session.Prompt));
        Assert.True(session.ChooseResponse(0, _ => 0));
        Assert.Equal((3, "Farewell."), (session.CurrentNode?.Id, session.Prompt));
        Assert.False(session.Continue(_ => 0));
        Assert.True(session.IsComplete);

        var cycle = new ImportedConversationSession(new DynamixConversationDatabase(
            new Dictionary<int, DynamixConversationNode> { [1] = new(1, 0, null, null, [], [], 1, []) }));
        Assert.Throws<InvalidDataException>(() => cycle.Start(1, _ => 0));
    }

    [Fact]
    public void ActionTreeDatabaseDecodesIndexedRecursiveExpressionsWithinBounds()
    {
        var body = new byte[104];
        WriteInt(body, 0, 1); WriteInt(body, 4, 8);
        WriteInt(body, 8, (int)DynamixActionKind.IfElse);
        WriteInt(body, 12, 28); WriteInt(body, 16, 2);
        WriteInt(body, 20, 80); WriteInt(body, 24, 92);
        WriteInt(body, 28, 2); WriteInt(body, 32, 1);
        WriteInt(body, 36, 48); WriteInt(body, 40, 64);
        WriteInt(body, 44, (int)DynamixExpressionOperator.Equal);
        WriteInt(body, 48, (int)DynamixValueKind.Literal); WriteInt(body, 52, 7); WriteInt(body, 56, -1);
        WriteInt(body, 64, (int)DynamixValueKind.Literal); WriteInt(body, 68, 7); WriteInt(body, 72, 1);
        WriteInt(body, 80, (int)DynamixActionKind.Evaluate); WriteInt(body, 84, 28);
        WriteInt(body, 92, (int)DynamixActionKind.Evaluate); WriteInt(body, 96, 28);
        var index = new byte[12];
        WriteInt(index, 0, 1); WriteInt(index, 4, 1101); WriteInt(index, 8, 0);

        var database = DynamixActionTreeDecoder.Decode(body, index);

        var group = Assert.IsType<DynamixActionGroup>(database.Find(1101));
        Assert.Equal([8], group.ActionOffsets);
        Assert.Equal(DynamixActionKind.IfElse, database.Actions[8].Kind);
        Assert.Equal([80, 92], database.Actions[8].BranchActionOffsets);
        Assert.Equal([DynamixExpressionOperator.Equal], database.Expressions[28].Operators);
        Assert.False(database.Values[48].Invert);
        Assert.True(database.Values[64].Invert);
        Assert.Equal(1, database.IndexHeaderValue);
        Assert.Equal((3, 1, 2), (database.Actions.Count, database.Expressions.Count, database.Values.Count));

        var badOperator = body.ToArray(); WriteInt(badOperator, 44, 9);
        Assert.Throws<InvalidDataException>(() => DynamixActionTreeDecoder.Decode(badOperator, index));
        Assert.Throws<InvalidDataException>(() => DynamixActionTreeDecoder.Decode(body, index[..^1]));
    }

    [Fact]
    public void PixelTextWrapsAtWordsAndKeepsOversizedWordsIntact()
    {
        Assert.Equal("ONE TWO\nTHREE", PixelTextLayout.Wrap("ONE TWO THREE", 7));
        Assert.Equal("SUPERCALIFRAGILISTIC", PixelTextLayout.Wrap("SUPERCALIFRAGILISTIC", 5));
        Assert.Equal("ONE\n\nTWO", PixelTextLayout.Wrap(" ONE  \r\n\r\n TWO ", 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => PixelTextLayout.Wrap("ONE", 0));
    }

    [Fact]
    public void VariableTableAndActionInterpreterApplyPersistentMutationAndRedirect()
    {
        var variableBytes = new byte[20];
        WriteInt(variableBytes, 0, 2); WriteInt(variableBytes, 4, 4); WriteInt(variableBytes, 8, 5);
        WriteInt(variableBytes, 12, 3); WriteInt(variableBytes, 16, 0);
        var table = DynamixVariableTableDecoder.Decode(variableBytes);
        Assert.Equal([3, 0], table.InitialValues);
        Assert.Throws<InvalidDataException>(() => DynamixVariableTableDecoder.Decode(variableBytes[..^1]));

        var values = new Dictionary<int, DynamixValueNode>
        {
            [1] = new(1, DynamixValueKind.Literal, 0, false, []),
            [2] = new(2, DynamixValueKind.Literal, 1, false, []),
            [3] = new(3, DynamixValueKind.Function, 5, false, [10, 11, 11]),
            [4] = new(4, DynamixValueKind.Function, 6, false, [10, 11]),
            [5] = new(5, DynamixValueKind.Function, 3, false, [13]),
            [6] = new(6, DynamixValueKind.Function, 3, false, [14])
        };
        var expressions = new Dictionary<int, DynamixExpressionNode>
        {
            [10] = new(10, [1], []), [11] = new(11, [2], []),
            [12] = new(12, [3], []), [13] = new(13, [2], []),
            [14] = new(14, [1], []),
            [15] = new(15, [4, 2], [DynamixExpressionOperator.GreaterThanOrEqual]),
            [16] = new(16, [5], []), [17] = new(17, [6], [])
        };
        var actions = new Dictionary<int, DynamixActionNode>
        {
            [20] = new(20, DynamixActionKind.Evaluate, 12, []),
            [21] = new(21, DynamixActionKind.IfElse, 15, [22, 23]),
            [22] = new(22, DynamixActionKind.Evaluate, 16, []),
            [23] = new(23, DynamixActionKind.Evaluate, 17, [])
        };
        var database = new DynamixActionTreeDatabase(100,
            new Dictionary<int, DynamixActionGroup> { [99] = new(99, 0, [20, 21, 20]) }, actions, expressions, values);
        var state = new TestActionState([3, 0]);

        var result = new DynamixActionInterpreter(database, state).Execute([99]);

        Assert.True(result.Success);
        Assert.Equal(2, state.Variables[1]);
        Assert.Equal(1, result.RedirectNodeId);
        Assert.False(new DynamixActionInterpreter(database, state).Execute([5011]).Success);

        var campaign = new CampaignState { Player = new Player { Wealth = 12 } };
        var campaignState = new ImportedConversationActionState(campaign);
        campaignState.Initialize(table.InitialValues);
        Assert.True(campaignState.TryGetVariable(1, 0, out var wealth));
        Assert.Equal(12, wealth);
        Assert.True(campaignState.TrySetVariable(1, 0, -4));
        Assert.Equal(0, campaign.Player.Wealth);
        Assert.Equal([3, 0], campaign.ConversationVariables);
    }

    [Fact]
    public void OriginalConversationSelectorsAndItemsUseTypedCampaignState()
    {
        var campaign = new CampaignState
        {
            Player = new Player
            {
                Wealth = 120,
                Fame = 4,
                Stats = new CharacterStats(11, 12, 13, 14, 15, 16)
            },
            ConversationItems = { [21] = 1 }
        };
        var state = new ImportedConversationActionState(campaign);

        state.Initialize([]);

        Assert.Equal(7, OriginalConversationBindings.Attributes.Count);
        Assert.Equal(24, OriginalConversationBindings.Items.Count);
        Assert.Contains("Book of Hours", campaign.Player.Inventory.Items);
        Assert.True(state.TryGetVariable(1, 2, out var honor));
        Assert.True(state.TryGetVariable(1, 3, out var fame));
        Assert.True(state.TryGetVariable(1, 5, out var piety));
        Assert.True(state.TryGetVariable(1, 6, out var strength));
        Assert.True(state.TryGetVariable(1, 7, out var stamina));
        Assert.True(state.TryGetVariable(1, 8, out var intelligence));
        Assert.Equal((15, 4, 13, 11, 14, 16), (honor, fame, piety, strength, stamina, intelligence));

        Assert.True(state.TrySetVariable(1, 2, 30));
        Assert.True(state.TrySetVariable(1, 3, -5));
        Assert.Equal((20, 0), (campaign.Player.Stats.Honor, campaign.Player.Fame));
        Assert.True(state.TryAddItem(10));
        Assert.Contains("Dragon Slaying Lance", campaign.Player.Inventory.Items);
        Assert.True(state.HasItem(10));
        Assert.True(state.TryClearItem(10));
        Assert.DoesNotContain("Dragon Slaying Lance", campaign.Player.Inventory.Items);
        campaign.Player.Inventory.Items.Add("Shield of St. George");
        Assert.True(state.HasItem(15));
    }

    [Fact]
    public void SmackerMovieHeaderAndFrameIndexAreBounded()
    {
        var movie = SmackerMovieDecoder.Decode(SyntheticSmacker());

        Assert.Equal((2, 196, 204, 2), (movie.Version, movie.Width, movie.Height, movie.Frames.Count));
        Assert.Equal(TimeSpan.FromMilliseconds(100), movie.FrameDuration);
        Assert.Equal((114, 4), (movie.TreeOffset, movie.TreeLength));
        var track = Assert.Single(movie.AudioTracks);
        Assert.Equal((0, 22050, 4096, true, false, false),
            (track.Index, track.SampleRate, track.MaximumDecodedBytes,
                track.IsCompressed, track.Is16Bit, track.IsStereo));
        Assert.Equal((118, 12, true, (byte)1),
            (movie.Frames[0].Offset, movie.Frames[0].Length, movie.Frames[0].IsKeyFrame, movie.Frames[0].Flags));
        Assert.Equal((130, 12, false, (byte)2),
            (movie.Frames[1].Offset, movie.Frames[1].Length, movie.Frames[1].IsKeyFrame, movie.Frames[1].Flags));
    }

    [Fact]
    public void SmackerFrameLayoutDecodesPaletteAndAudioPacketBoundaries()
    {
        var source = SyntheticSmacker();
        var movie = SmackerMovieDecoder.Decode(source);
        var first = SmackerMovieDecoder.DecodeFrameLayout(movie, 0, source, new byte[768]);

        Assert.True(first.PaletteChanged);
        Assert.Empty(first.AudioPackets);
        Assert.Equal((126, 4), (first.Video.Offset, first.Video.Length));

        var second = SmackerMovieDecoder.DecodeFrameLayout(movie, 1, source, first.Palette);
        Assert.False(second.PaletteChanged);
        var audio = Assert.Single(second.AudioPackets);
        Assert.Equal((0, 3, 134, 4),
            (audio.TrackIndex, audio.DecodedLength, audio.Data.Offset, audio.Data.Length));
        Assert.Equal((138, 4), (second.Video.Offset, second.Video.Length));
    }

    [Fact]
    public void SmackerStreamReaderRetainsOnlyIndexTreesAndOneFrame()
    {
        var source = SyntheticSmacker();
        using var stream = new MemoryStream(source);
        using var reader = new SmackerMovieStream(stream, leaveOpen: true);
        var frameBuffer = new byte[reader.MaximumFrameLength];

        Assert.Equal(source.AsSpan(114, 4).ToArray(), reader.TreeData);
        Assert.Equal(12, reader.ReadFrame(1, frameBuffer));
        var frame = SmackerMovieDecoder.DecodeFramePayload(
            reader.Movie, 1, frameBuffer, new byte[768]);
        var audio = Assert.Single(frame.AudioPackets);
        Assert.Equal((4, 4, 8, 4),
            (audio.Data.Offset, audio.Data.Length, frame.Video.Offset, frame.Video.Length));
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void SmackerPackedMonoAudioDecodesPredictiveHuffmanSamples()
    {
        var packet = new byte[7];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, 3);
        packet[4] = 0x29;
        packet[5] = 0xA0;
        packet[6] = 0x02;
        var track = new SmackerAudioTrack(0, 22050, 4096, true, false, false);

        var decoded = SmackerAudioDecoder.Decode(packet, track);

        Assert.Equal([10, 11, 12], decoded.Samples);
        Assert.Equal([0, 0x8A, 0, 0x8B, 0, 0x8C], decoded.ToPcm16LittleEndian());
    }

    [Fact]
    public void SmackerPackedAudioRejectsTruncationAndProfileMismatch()
    {
        var track = new SmackerAudioTrack(0, 22050, 4096, true, false, false);
        Assert.Throws<InvalidDataException>(() => SmackerAudioDecoder.Decode([3, 0, 0, 0, 1], track));
        Assert.Throws<InvalidDataException>(() => SmackerAudioDecoder.Decode([3, 0, 0, 0, 0x2B, 0xA0, 0x02], track));
        Assert.Throws<NotSupportedException>(() => SmackerAudioDecoder.Decode([3, 0, 0, 0, 1], track with { IsStereo = true }));
    }

    [Fact]
    public void SmackerVideoTreesDecodeFourByFourBlocksIntoPriorFrameBuffer()
    {
        var source = SyntheticSmackerVideo();
        var movie = SmackerMovieDecoder.Decode(source);
        var layout = SmackerMovieDecoder.DecodeFrameLayout(movie, 0, source, new byte[768]);
        var decoder = new SmackerVideoDecoder(movie, source);
        var indices = Enumerable.Repeat((byte)9, 16).ToArray();

        decoder.DecodeFrame(source.AsSpan(layout.Video.Offset, layout.Video.Length), indices, true);

        Assert.All(indices, value => Assert.Equal(0, value));
    }

    [Fact]
    public void SmackerMovieRejectsInvalidHeadersAndFrameExtents()
    {
        var badMagic = SyntheticSmacker();
        badMagic[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => SmackerMovieDecoder.Decode(badMagic));

        var badExtent = SyntheticSmacker();
        BinaryPrimitives.WriteUInt32LittleEndian(badExtent.AsSpan(108, 4), 16);
        Assert.Throws<InvalidDataException>(() => SmackerMovieDecoder.Decode(badExtent));

        var badDimensions = SyntheticSmacker();
        BinaryPrimitives.WriteUInt32LittleEndian(badDimensions.AsSpan(4, 4), 195);
        Assert.Throws<InvalidDataException>(() => SmackerMovieDecoder.Decode(badDimensions));
    }

    [Fact]
    public void Kind1DecodesLiteralCopyAndRunTokens()
    {
        byte[] compressed = [13, 0, 0x40, 0, 0x18, 0, (byte)'A', (byte)'B', (byte)'C', 0, 0x30, 0, 0, 0, (byte)'Z'];

        var decoded = DynamixCompression.DecodeKind1(compressed, 22);

        Assert.Equal("ABCABC" + new string('Z', 16), System.Text.Encoding.ASCII.GetString(decoded));
    }

    [Fact]
    public void Kind1RejectsCopiesBeforeOutputStart()
    {
        byte[] compressed = [7, 0, 0x40, 0, 0x80, 0, 0, 0x10];

        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind1(compressed, 3));
    }

    [Fact]
    public void Kind2DecodesControlCodesAndFourteenBitGrowthSafely()
    {
        var basic = PackMsbCodes([(256, 9), (65, 9), (66, 9), (258, 9), (260, 9), (257, 9)]);
        Assert.Equal("ABABABA", System.Text.Encoding.ASCII.GetString(DynamixCompression.DecodeKind2(basic, 7)));

        (int Count, int Width)[] widthRuns = [(254, 9), (512, 10), (1_024, 11), (2_048, 12), (4_096, 13), (8_193, 14)];
        var growthCodes = new List<(int Code, int Width)> { (256, 9) };
        foreach (var (count, width) in widthRuns) growthCodes.AddRange(Enumerable.Repeat((0, width), count));
        growthCodes.Add((257, 14));
        Assert.Equal(new byte[widthRuns.Sum(run => run.Count)], DynamixCompression.DecodeKind2(PackMsbCodes(growthCodes), widthRuns.Sum(run => run.Count)));

        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(basic[..^1], 7));
        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(PackMsbCodes([(256, 9), (300, 9), (257, 9)]), 1));
        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(basic, 7, 6));
    }

    [Fact]
    public void DynamixSoundBankParsesBoundedRateTaggedSamples()
    {
        var bytes = new byte[4 + 8 + 3 + 8 + 2];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, DynamixSoundBankDecoder.Magic);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), 3);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8), 11025);
        bytes[12] = 0x7f; bytes[13] = 0x80; bytes[14] = 0x81;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(15), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(19), 22050);
        bytes[23] = 0; bytes[24] = 255;

        var bank = DynamixSoundBankDecoder.Decode(bytes);

        Assert.Equal(2, bank.Samples.Count);
        Assert.Equal(11025, bank.Samples[0].SampleRate);
        Assert.Equal(new byte[] { 0x7f, 0x80, 0x81 }, bank.Samples[0].Samples);
        Assert.Equal(22050, bank.Samples[1].SampleRate);
        Assert.Equal(new byte[] { 0x00, 0xFF, 0x00, 0x00, 0x00, 0x01 },
            bank.Samples[0].ToPcm16LittleEndian());
        Assert.Throws<InvalidDataException>(() => DynamixSoundBankDecoder.Decode(bytes[..^1]));
        bytes[0] = 0;
        Assert.Throws<InvalidDataException>(() => DynamixSoundBankDecoder.Decode(bytes));
    }

    [Fact]
    public void OriginalArtRolesAreDataDrivenAndUnique()
    {
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Title.Background", IdSuffix: ":fftitle.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Options", IdSuffix: ":char_ops.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Pregenerated", IdSuffix: ":pregen.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Campaign.Briefing", IdSuffix: ":fluff.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Village.Inn", IdSuffix: ":innpeopl.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Load.Background", IdSuffix: ":loadgame.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Map.England", IdSuffix: ":engmap1.pcx" });
        Assert.Equal(ImportedArt.Definitions.Count, ImportedArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(ImportedAnimations.Definitions.Count,
            ImportedAnimations.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(ImportedMovies.Definitions,
            definition => definition is { Role: "Title.Intro", IdSuffix: "/title.smk" });
        Assert.Contains(ImportedMovies.Definitions,
            definition => definition is { Role: "Options.Credits", IdSuffix: "/creditzz.smk" });
        Assert.Equal(ImportedMovies.Definitions.Count,
            ImportedMovies.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(ImportedSounds.Definitions.Count,
            ImportedSounds.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(ImportedSounds.Definitions,
            sound => sound is { Role: "Interface.Activate", IdSuffix: ":gameopts.666", SampleIndex: 0 });
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition is { Role: "Interface.Cursor", IdSuffix: ":ffmouse.csf", PaletteArtRole: "Estate.Shell" });
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition is
            {
                Role: "Combat.FirstPerson", IdSuffix: ":skirmish.csf", PaletteIdSuffix: ":SKIRMISH.PAL"
            });
        Assert.Contains(ImportedRawArt.Definitions,
            definition => definition is
            {
                Role: "Combat.Shell", IdSuffix: ":SKIRMISH.PCX", PaletteIdSuffix: ":SKIRMISH.PAL",
                Width: 320, Height: 200
            });
        Assert.Equal(ImportedRawArt.Definitions.Count,
            ImportedRawArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void CharacterCreationScreenIsDefinitionDriven()
    {
        Assert.Equal(Enum.GetValues<CharacterCreationAction>().Length, CharacterCreationDefinitions.Options.Count);
        Assert.Equal(CharacterCreationDefinitions.Options.Count, CharacterCreationDefinitions.Options.Select(x => x.Action).Distinct().Count());
        Assert.Equal(Balance.Templates.Length, CharacterCreationDefinitions.PregeneratedCharacters.Count);
        Assert.Equal(["Red", "Green", "Blue"], CharacterCreationDefinitions.HeraldicColors.Select(x => x.Name));
        Assert.All(CharacterCreationDefinitions.HeraldicColors, x => Assert.True(CharacterCreationDefinitions.Options[1].OriginalBounds.Contains(x.OriginalBounds.X, x.OriginalBounds.Y)));
        Assert.Contains(ImportedArt.Definitions, definition => definition.Role == "Dilemma.Background" && definition.IdSuffix == ":morality.pcx");
        Assert.Contains(ImportedLayouts.Definitions, definition => definition.Role == "Dilemma" && definition.IdSuffix == ":chargen.hat");
    }

    [Fact]
    public void OptionsHubActionsAndOriginalRegionsAreDataDriven()
    {
        Assert.Equal(Enum.GetValues<OptionsHubAction>().Length, OptionsHubDefinitions.Options.Count);
        Assert.Equal(OptionsHubDefinitions.Options.Count,
            OptionsHubDefinitions.Options.Select(option => option.Action).Distinct().Count());
        Assert.Equal(new UiBounds(15, 243, 255, 237),
            OptionsHubDefinitions.Options.Single(option => option.Action == OptionsHubAction.NewGame).OriginalBounds);
        Assert.Equal(11, OptionsHubDefinitions.Options.Single(option => option.Action == OptionsHubAction.Resume).HatRegionId);
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Options.Background" && definition.IdSuffix == ":optfin.pcx");
        Assert.Equal(5, OptionsHubDefinitions.Options.Count(option => option.Setting.HasValue));
        Assert.Equal(5, OptionsHubDefinitions.Options.Select(option => option.Setting).OfType<OptionsHubSetting>().Distinct().Count());
        Assert.Equal(OptionsHubDefinitions.EnabledStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: true, pressed: false));
        Assert.Equal(OptionsHubDefinitions.EnabledPressedStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: true, pressed: true));
        Assert.Equal(OptionsHubDefinitions.DisabledStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: false, pressed: false));
        Assert.Equal(OptionsHubDefinitions.DisabledPressedStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: false, pressed: true));
        var optionsAnimation = Assert.Single(ImportedAnimations.Definitions, definition => definition.Role == "Options.Widgets");
        Assert.Equal("Options.Background", optionsAnimation.PaletteArtRole);
    }

    [Fact]
    public void OriginalCursorFramesRetainTheirDecodedOrder()
    {
        Assert.Equal(OriginalCursorDefinitions.FrameCount, Enum.GetValues<OriginalCursorKind>().Length);
        Assert.Equal(Enumerable.Range(0, OriginalCursorDefinitions.FrameCount),
            Enum.GetValues<OriginalCursorKind>().Select(OriginalCursorDefinitions.Frame));
    }

    [Fact]
    public void PracticeMenuUsesOriginalRegionAndExecutableLabelOrder()
    {
        Assert.Equal(Enum.GetValues<PracticeAction>().Length, PracticePresentationDefinitions.Options.Count);
        Assert.Equal(["War", "Joust", "Melee", "Exit", "Castle Skirmish"],
            PracticePresentationDefinitions.Options.Select(option => option.Label));
        Assert.Equal(Enumerable.Range(0, 5),
            PracticePresentationDefinitions.Options.Select(option => option.HatRegionId));
        Assert.Equal(new UiBounds(1, 1, 234, 170),
            PracticePresentationDefinitions.Options.Single(option => option.Action == PracticeAction.CastleSkirmish).OriginalBounds);
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Practice.Background" && definition.IdSuffix == ":practice.pcx");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Practice" && definition.IdSuffix == ":practice.hat");
        Assert.Contains(ImportedMovies.Definitions,
            definition => definition.Role == "Practice.Joust" && definition.IdSuffix == "/jousprac.smk");
    }

    [Fact]
    public void PracticeCombatAdaptersCreateIsolatedRepeatableSessions()
    {
        var war = PracticeCombatDefinitions.CreateWar(seed: 7);
        Assert.Equal(32, war.Friendly.Sum(squad => squad.Count));
        Assert.Equal(32, war.Enemy.Sum(squad => squad.Count));
        Assert.Equal(3, war.Friendly.Count());
        Assert.Equal(3, war.Enemy.Count());

        var melee = PracticeCombatDefinitions.CreateMelee(seed: 7);
        var castle = PracticeCombatDefinitions.CreateCastleSkirmish(seed: 7);
        Assert.Equal(7, melee.Enemies.Count);
        Assert.Equal(11, castle.Enemies.Count);
        Assert.NotSame(melee, PracticeCombatDefinitions.CreateMelee(seed: 7));
        Assert.NotSame(castle, PracticeCombatDefinitions.CreateCastleSkirmish(seed: 7));
    }

    [Fact]
    public void LocationScreenRolesAndShopControlsAreDataDriven()
    {
        var roles = ImportedArt.Definitions.ToDictionary(definition => definition.Role);
        Assert.Equal(":tactical.pcx", roles["Home.Office"].IdSuffix);
        Assert.Equal(":fiefmgmt.pcx", roles["Farm.Management"].IdSuffix);
        Assert.Equal(":forgesmi.pcx", roles["Blacksmith.Workshop"].IdSuffix);
        Assert.Equal(":swdtemp.pcx", roles["Shop.Inventory"].IdSuffix);

        Assert.Equal(Enum.GetValues<ShopControlAction>().Length, ShopPresentationDefinitions.Controls.Count);
        Assert.Equal(ShopPresentationDefinitions.Controls.Count,
            ShopPresentationDefinitions.Controls.Select(control => control.Action).Distinct().Count());
        Assert.Equal(Enum.GetValues<ShopOverlayState>().Length, ShopPresentationDefinitions.Overlays.Count);
        Assert.Equal(ShopPresentationDefinitions.Overlays.Count,
            ShopPresentationDefinitions.Overlays.Select(overlay => overlay.State).Distinct().Count());
        Assert.Equal(1, ShopPresentationDefinitions.ViewOverlay(true).Frame);
        Assert.Equal(0, ShopPresentationDefinitions.ViewOverlay(false).Frame);
        Assert.Equal(2, ShopPresentationDefinitions.TransactionOverlay(true).Frame);
        Assert.Equal(3, ShopPresentationDefinitions.TransactionOverlay(false).Frame);
        Assert.All(ShopPresentationDefinitions.Controls, control =>
        {
            Assert.InRange(control.Bounds.X, 0, 639);
            Assert.InRange(control.Bounds.Y, 0, 479);
            Assert.InRange(control.Bounds.X + control.Bounds.Width, 1, 640);
            Assert.InRange(control.Bounds.Y + control.Bounds.Height, 1, 480);
        });
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Home.Office" && definition.IdSuffix == ":fopts.hat");
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Home.Overview" && definition.IdSuffix == ":f_over.pcx");
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Home.WarPlanning" && definition.IdSuffix == ":warplan.pcx");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Home.Overview" && definition.IdSuffix == ":foview.hat");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Home.WarPlanning" && definition.IdSuffix == ":fwarplan.hat");
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition.Role == "Home.WarPlanning.Controls" && definition.IdSuffix == ":warplan.csf"
                && definition.PaletteArtRole == "Home.WarPlanning");
        Assert.Equal([0, 3, 6, 9, 12], Enumerable.Range(0, 5)
            .Select(index => WarPlanningPresentationDefinitions.ArmyFrame(index, true, true)));
        Assert.Equal([2, 5, 8, 11, 14], Enumerable.Range(0, 5)
            .Select(index => WarPlanningPresentationDefinitions.ArmyFrame(index, false, false)));
        Assert.Equal(5, WarPlanningPresentationDefinitions.Fallback.ArmyButtons.Count);
        Assert.Equal(3, WarPlanningPresentationDefinitions.Fallback.UnitRows.Count);
        Assert.Equal(
            [(SceneNavigationAction.Overview, "Overview", 0, new UiBounds(248, 184, 64, 31)),
             (SceneNavigationAction.Castle, "Castle", 1, new UiBounds(172, 153, 72, 44)),
             (SceneNavigationAction.Farm, "Farm", 2, new UiBounds(277, 146, 17, 38)),
             (SceneNavigationAction.Village, "Village", 3, new UiBounds(295, 141, 18, 43)),
             (SceneNavigationAction.Forest, "Forest", 4, new UiBounds(314, 147, 15, 39)),
             (SceneNavigationAction.WarPlanning, "War Planning", 5, new UiBounds(359, 123, 35, 76)),
             (SceneNavigationAction.Exit, "Exit", 6, new UiBounds(0, 113, 66, 210)),
             (SceneNavigationAction.Jump, "JUMP!!", 7, new UiBounds(568, 84, 72, 200)),
             (SceneNavigationAction.Map, "Map", 8, new UiBounds(200, 55, 68, 85)),
             (SceneNavigationAction.Orders, "Orders", 9, new UiBounds(332, 180, 26, 37))],
            HomePresentationDefinitions.Hotspots.Select(hotspot =>
                (hotspot.Action, hotspot.HoverLabel, hotspot.HatRegionId, hotspot.Bounds)));
        Assert.Equal(
            [(SceneNavigationAction.BlacksmithDialogue, "Blacksmith", new UiBounds(253, 109, 107, 164)),
             (SceneNavigationAction.Shop, "Buy/Sell", new UiBounds(54, 2, 180, 141))],
            BlacksmithPresentationDefinitions.Hotspots.Select(hotspot => (hotspot.Action, hotspot.HoverLabel, hotspot.Bounds)));
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Dialogue.Frame" && definition.IdSuffix == ":comscrn1.pcx");
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Blacksmith.Portrait" && definition.IdSuffix == ":blacksmi.pcc");
        Assert.Equal(Enum.GetValues<BlacksmithDialogueAction>().Length,
            BlacksmithDialoguePresentationDefinitions.Commands.Count);
        Assert.Equal(BlacksmithDialoguePresentationDefinitions.Commands.Count,
            BlacksmithDialoguePresentationDefinitions.Commands.Select(command => command.Action).Distinct().Count());
        Assert.Equal(BlacksmithDialoguePresentationDefinitions.Commands.SelectMany(command => command.Keys).Count(),
            BlacksmithDialoguePresentationDefinitions.Commands.SelectMany(command => command.Keys).Distinct().Count());
    }

}
