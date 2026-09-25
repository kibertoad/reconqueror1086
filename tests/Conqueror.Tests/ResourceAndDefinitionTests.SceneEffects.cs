using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

// Covers FMT-ASSAULT-003, RULE-ASSAULT-018, RULE-ASSAULT-020, RULE-ASSAULT-029.
public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void BillboardHeadingUsesTheExecutableAngularDivisionAndMirrorFormula()
    {
        var walk = new DynamixSceneBlock(0, 4, 135, 0, 0, 128, 168, 0, 0,
            175, 0, 8, 0, 0, 0, 0, 0, 0, 0, 0, 0, "knight");
        var attack = walk with { Surface0 = 190, Surface2 = 4 };

        Assert.Equal((175, false), walk.TextureForBillboardHeading(0));
        Assert.Equal((176, false), walk.TextureForBillboardHeading(32));
        Assert.Equal((179, false), walk.TextureForBillboardHeading(128));
        Assert.Equal((176, true), walk.TextureForBillboardHeading(224));
        Assert.Equal((190, false), attack.TextureForBillboardHeading(0));
        Assert.Equal((191, false), attack.TextureForBillboardHeading(64));
        Assert.Equal((192, false), attack.TextureForBillboardHeading(128));
        Assert.Equal((191, true), attack.TextureForBillboardHeading(192));
        Assert.Equal(walk.TextureForBillboardHeading(32), walk.TextureForBillboardHeading(288));
        Assert.Equal((175, false), (walk with { Surface2 = 257 }).TextureForBillboardHeading(32));
    }

    [Fact]
    public void ImportedActorStatesUseTheirSelectedEffectCompletionGate()
    {
        var source = SyntheticScene();
        var blocks = new byte[DynamixSceneDecoder.BlockSize * 9];
        source.Blocks.CopyTo(blocks, 0);
        WriteInt(source.Scenario, 24, 9);
        SetSceneCell(source.Map, 14, 20, 0);
        for (var index = 5; index <= 8; index++)
        {
            var offset = index * DynamixSceneDecoder.BlockSize;
            blocks.AsSpan(offset, DynamixSceneDecoder.BlockSize).Clear();
            WriteInt(blocks, offset, 4);
            WriteInt(blocks, offset + 4, 135);
            WriteInt(blocks, offset + 44, 20 + index);
            WriteInt(blocks, offset + 52, index == 5 ? 8 : index == 8 ? 2 : 4);
            System.Text.Encoding.ASCII.GetBytes("knight").CopyTo(blocks, offset + 78);
            blocks[offset + 94] = 0xcc;
            blocks[offset + 95] = 0xcc;
        }
        WriteInteraction(blocks, 5, 1, 3, 0);
        WriteInt(blocks, 5 * DynamixSceneDecoder.BlockSize + 0x44, 1 << 16);
        WriteInt(blocks, 5 * DynamixSceneDecoder.BlockSize + 0x24, 12);
        WriteInt(blocks, 5 * DynamixSceneDecoder.BlockSize + 0x28, -34);
        WriteInt(source.Scenario, 28, 2);
        var effects = new byte[DynamixSceneEffectDecoder.RecordSize * 2];
        WriteEffectField(effects, 0x0c, 2);
        WriteEffectField(effects, 0x14, 192);
        WriteEffectField(effects, 0x28, -1);
        WriteEffectField(effects, 0x40 + 0x0c, 3);
        WriteEffectField(effects, 0x40 + 0x14, 200);
        WriteEffectField(effects, 0x40 + 0x18, 0x142);
        WriteEffectField(effects, 0x40 + 0x1c, 64);
        WriteEffectField(effects, 0x40 + 0x34, 5);
        WriteEffectField(effects, 0x40 + 0x28, -1);

        var scene = DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, blocks, effects);
        var actor = Assert.Single(ImportedSiegeLayouts.Convert(scene).Enemies);

        Assert.Equal(new SiegeActorAnimation(0.384, 0.384, 0.384), actor.OriginalAnimation);
        Assert.Equal(new SiegeActorMovement(3, 200, 64, 0, 0x142, 5), actor.OriginalMovement);
        Assert.Equal((12, -34), (actor.InitialOffsetX8, actor.InitialOffsetY8));
        Assert.Equal(3, actor.OriginalActorTemplate);
    }

    [Fact]
    public void RangedActorAttackGateDoublesTheSelectedInterval()
    {
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        var siege = new SiegeSession(new Player(), new Army(), 0, 7,
            new SiegeLayout(tiles, 1, 1, Facing.East,
                [new SiegeSpawn(2, 1, false, OriginalCombatRow: 23,
                    OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384))]));
        var enemy = Assert.Single(siege.Enemies);

        siege.Move(false);
        siege.AdvanceEnemyAnimations(0.768);
        Assert.Equal(SiegeEnemyVisualState.Attack, enemy.VisualState);
        siege.AdvanceEnemyAnimations(0.002);
        Assert.Equal(SiegeEnemyVisualState.Walk, enemy.VisualState);
    }

    [Fact]
    public void SceneDecoderReadsExecutableMappedSfxDefinitions()
    {
        var source = SyntheticScene();
        WriteInt(source.Scenario, 28, 2);
        WriteInt(source.Blocks, 44, 1 << 16 | 12);
        WriteInt(source.Blocks, 0x44, 1 << 16);
        var effects = new byte[DynamixSceneEffectDecoder.RecordSize * 2];
        WriteEffectField(effects, 0x0c, 3);
        WriteEffectField(effects, 0x14, 192);
        WriteEffectField(effects, 0x18, 5);
        WriteEffectField(effects, 0x1c, -16);
        WriteEffectField(effects, 0x20, 32);
        WriteEffectField(effects, 0x28, -1);
        WriteEffectField(effects, 0x2c, 1);
        WriteEffectField(effects, 0x30, 9);
        WriteEffectField(effects, 0x34, 3);
        WriteEffectField(effects, 0x38, 27);
        WriteEffectField(effects, 0x3c, 64);
        WriteEffectField(effects, 0x40 + 0x0c, 1);
        WriteEffectField(effects, 0x40 + 0x14, 100);
        WriteEffectField(effects, 0x40 + 0x28, -1);

        var scene = DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks, effects);

        Assert.Equal((12, 1, 1), (scene.Blocks[0].Surface0, scene.Blocks[0].EffectDefinitionIndex,
            scene.Blocks[0].MovementEffectDefinitionIndex));
        Assert.Equal((0, 0), (scene.Blocks[0].InitialXOffset8, scene.Blocks[0].InitialYOffset8));
        Assert.Equal(2, scene.EffectDefinitionCount);
        var definition = scene.EffectDefinitions[0];
        Assert.Equal((3, 192, 5, -1),
            (definition.FrameCount, definition.IntervalMilliseconds, definition.Flags, definition.MapBlockIndex));
        Assert.Equal((-16, 32, 1, 9, 3, 27, 64),
            (definition.MapXDeltaPerTick, definition.MapYDeltaPerTick,
                definition.BlockIndexDeltaPerTick, definition.LoopBlockIndex,
                definition.SurfaceIndexDeltaPerTick, definition.TerminalSurfaceIndex,
                definition.HeadingDeltaPerTick));
        Assert.Equal(576, definition.NominalCompletionMilliseconds);
        Assert.Equal(64, definition.FieldAt(0x3c));
        Assert.Throws<ArgumentOutOfRangeException>(() => definition.FieldAt(2));
    }

    [Fact]
    public void SceneEffectDecoderRejectsCountLengthAndReferenceMismatches()
    {
        var source = SyntheticScene();
        var effects = new byte[DynamixSceneEffectDecoder.RecordSize];
        WriteEffectField(effects, 0x0c, 1);
        WriteEffectField(effects, 0x14, 100);
        WriteEffectField(effects, 0x28, -1);

        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks, effects[..^1]));
        WriteInt(source.Blocks, 0x44, 1 << 16);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks, effects));
        WriteInt(source.Blocks, 0x44, 0);
        WriteEffectField(effects, 0x28, 7);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks, effects));
        WriteEffectField(effects, 0x28, -1);
        WriteEffectField(effects, 0x14, -1);
        Assert.Throws<InvalidDataException>(() => DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks, effects));
    }

    [Fact]
    public void ImportedCatalogRequiresAndDecodesSceneSfxDefinitions()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-scene-effects-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var source = SyntheticScene();
            var effects = new byte[DynamixSceneEffectDecoder.RecordSize];
            WriteEffectField(effects, 0x0c, 2);
            WriteEffectField(effects, 0x14, 192);
            WriteEffectField(effects, 0x28, -1);
            var assets = new[]
            {
                Asset("CONQUER/TEST.RES#0:Viewer", "0000-Viewer", source.Viewer),
                Asset("CONQUER/TEST.RES#1:Scenario", "0001-Scenario", source.Scenario),
                Asset("CONQUER/TEST.RES#2:Map", "0002-Map", source.Map),
                Asset("CONQUER/TEST.RES#3:Blocks", "0003-Blocks", source.Blocks),
                Asset("CONQUER/TEST.RES#4:SFXDEFS", "0004-SFXDEFS", effects)
            };
            new ImportManifest(ImportManifest.CurrentVersion, new string('a', 32), assets).Write(Path.Combine(root, "manifest.json"));

            var catalog = Assert.IsType<ImportedContentCatalog>(ImportedContentCatalog.Discover(root));
            var scene = Assert.IsType<DynamixScene>(catalog.DecodeScene("CONQUER/TEST.RES"));
            Assert.Equal(192, Assert.Single(scene.EffectDefinitions).IntervalMilliseconds);

            File.Delete(Path.Combine(root, "Decoded", "0004-SFXDEFS"));
            Assert.Null(catalog.DecodeScene("CONQUER/TEST.RES"));

            ImportedAsset Asset(string id, string name, byte[] bytes)
            {
                var relative = Path.Combine("Decoded", name);
                var path = Path.Combine(root, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllBytes(path, bytes);
                return new ImportedAsset(id, relative, "resource", bytes.Length, ResourceHash.Xxh3(path));
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static void WriteEffectField(byte[] target, int offset, int value) =>
        BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, sizeof(int)), value);
}
