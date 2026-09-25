using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void AcquisitionTextureClosureIncludesStructuralFacesAndEveryBillboardSector()
    {
        var source = SyntheticScene();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        Assert.Equal([12, 13, 14, 15], scene.Blocks[0].RaycastTextureReferences());

        var billboard = scene.Blocks[0] with
        {
            Kind = 4,
            Behavior = 0,
            Surface0 = 20,
            Surface2 = 8
        };
        Assert.Equal(Enumerable.Range(20, 8), billboard.RaycastTextureReferences().Order());

        var mirrored = billboard with { Behavior = 4 };
        Assert.Equal(Enumerable.Range(20, 5), mirrored.RaycastTextureReferences().Order());
    }

    [Fact]
    public void RenderTextureClosureIncludesNormalizedHostileAndFriendlyWalkFrames()
    {
        var source = SyntheticScene();
        var actorOffset = 5 * DynamixSceneDecoder.BlockSize;
        WriteInt(source.Blocks, actorOffset, 4);
        WriteInt(source.Blocks, actorOffset + 8, 32);
        WriteInt(source.Blocks, actorOffset + 44, 20);
        WriteInt(source.Blocks, actorOffset + 52, 8);
        var scene = DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks);
        var tiles = new SiegeTile[3, 1];
        var movement = new SiegeActorMovement(3, 200, 64, 0, 0x142, 5);
        var spawn = new SiegeSpawn(1, 0, false, VisualId: 5, OriginalMovement: movement);
        var layout = new SiegeLayout(tiles, 0, 0, Facing.East,
            [spawn], retainers: [spawn with { X = 2 }]);

        var textures = SiegeTextureDependencies.RenderTextures(
            scene, layout, sourceOriginX: 0, sourceOriginY: 0, heraldicColor: "Green");

        // Green conflicts turn this hostile blue (base 96); friendly actors use
        // the player's green family (base 128). Three five-texture walk frames
        // are reachable for either actor.
        Assert.All(Enumerable.Range(96, 15), texture => Assert.Contains(texture, textures));
        Assert.All(Enumerable.Range(128, 15), texture => Assert.Contains(texture, textures));
    }

    [Theory]
    [InlineData(Facing.North, 0)]
    [InlineData(Facing.East, 192)]
    [InlineData(Facing.South, 384)]
    [InlineData(Facing.West, 576)]
    public void CombatBackdropAlignsItsStoredHorizonAndUsesHeadingScaledPanorama(
        Facing facing, int expectedX)
    {
        Assert.Equal(
            new SiegeBackdropSlice(
                new UiBounds(expectedX, 141, 167, 59),
                new UiBounds(0, 0, 167, 59)),
            SiegeCombatPresentation.BackdropSlice(facing, 1088, 200, 199, 3));
    }

    [Fact]
    public void CombatTextureSourcesUseSceneOverridesBeforeTheCompleteActorAtlas()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-combat-atlas-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var assets = new[]
            {
                Asset("CONQUER/MELEE0.RES#1:TEX096 2 1", "scene-96.bin", [1, 2]),
                Asset("CONQUER/DEFEND2.RES#1:TEX096 2 1", "atlas-96.bin", [3, 4]),
                Asset("CONQUER/DEFEND2.RES#2:TEX128 2 1", "atlas-128.bin", [5, 6])
            };
            new ImportManifest(ImportManifest.CurrentVersion, new string('a', 32), assets).Write(
                Path.Combine(root, "manifest.json"));
            var catalog = Assert.IsType<ImportedContentCatalog>(ImportedContentCatalog.Discover(root));

            var sources = ImportedSiegeLayouts.LoadCombatTextureSources(
                catalog, "CONQUER/MELEE0.RES", [96, 128]);

            Assert.Equal([1, 2], sources[96].Indices);
            Assert.Equal([5, 6], sources[128].Indices);
            var missing = Assert.Throws<InvalidDataException>(() =>
                ImportedSiegeLayouts.LoadCombatTextureSources(
                    catalog, "CONQUER/MELEE0.RES", [96, 129]));
            Assert.Contains("required texture 129", missing.Message, StringComparison.Ordinal);

            ImportedAsset Asset(string id, string name, byte[] bytes)
            {
                var path = Path.Combine(root, name);
                File.WriteAllBytes(path, bytes);
                return new ImportedAsset(id, name, "resource", bytes.Length, ResourceHash.Xxh3(path));
            }
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }
}
