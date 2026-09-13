using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void SceneDecoderReadsExecutableMappedSfxDefinitions()
    {
        var source = SyntheticScene();
        WriteInt(source.Scenario, 28, 2);
        WriteInt(source.Blocks, 44, 1 << 16 | 12);
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

        Assert.Equal((12, 1), (scene.Blocks[0].Surface0, scene.Blocks[0].EffectDefinitionIndex));
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
            new ImportManifest(1, new string('a', 64), assets).Write(Path.Combine(root, "manifest.json"));

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
                return new ImportedAsset(id, relative, "resource", bytes.Length, ResourceHash.Sha256(path));
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
