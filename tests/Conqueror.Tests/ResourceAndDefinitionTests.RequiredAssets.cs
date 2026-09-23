using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OriginalUiFontRequiresTheObservedProportionalAsciiSheet()
    {
        Assert.True(OriginalUiFontDefinition.IsCompatible(
            Enumerable.Repeat(new CsfDimensionHeader(9, 13), 256).ToArray()));
        Assert.False(OriginalUiFontDefinition.IsCompatible(
            Enumerable.Repeat(new CsfDimensionHeader(9, 13), 255).ToArray()));
        Assert.False(OriginalUiFontDefinition.IsCompatible(
            Enumerable.Repeat(new CsfDimensionHeader(11, 13), 256).ToArray()));
        Assert.Contains(ImportedFonts.Definitions, font =>
            font.IdSuffix == OriginalUiFontDefinition.ResourceSuffix);
    }

    [Fact]
    public void OriginalUiFontConvertsCumulativeSourceAdvancesWithoutNarrowGlyphDrift()
    {
        Assert.Equal(4, OriginalUiFontDefinition.CanvasAdvance(6, 1));
        Assert.Equal(3, OriginalUiFontDefinition.CanvasAdvance(2, 1)
            + OriginalUiFontDefinition.CanvasAdvance(2, 1)
            + OriginalUiFontDefinition.CanvasAdvance(2, 1));
        Assert.Equal(9, OriginalUiFontDefinition.CanvasAdvance(13, 1));
    }

    [Fact]
    public void OriginalUiFontMasksArePremultipliedForTheDefaultSpriteBatchBlend()
    {
        var transparent = OriginalUiFontDefinition.MaskPixel(0);
        var partial = OriginalUiFontDefinition.MaskPixel(127);
        Assert.Equal((byte)0, transparent.R);
        Assert.Equal((byte)0, transparent.A);
        Assert.Equal((byte)127, partial.R);
        Assert.Equal((byte)127, partial.G);
        Assert.Equal((byte)127, partial.B);
        Assert.Equal((byte)127, partial.A);
    }

    [Fact]
    public void SiegeVisualsResolveTheImportedCombatPaletteByItsDecodedKind()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-combat-palette-{Guid.NewGuid():N}");
        try
        {
            var relative = Path.Combine("Decoded", "SKIRMISH.RES", "0012-SKIRMISH.PAL");
            var path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var bytes = Enumerable.Range(0, IndexedPalette.ByteSize).Select(value => (byte)value).ToArray();
            File.WriteAllBytes(path, bytes);
            var asset = new ImportedAsset(
                "CONQUER/SKIRMISH.RES#12:SKIRMISH.PAL", relative, "palette",
                bytes.Length, ResourceHash.Sha256(path));
            new ImportManifest(1, new string('a', 64), [asset]).Write(Path.Combine(root, "manifest.json"));
            var catalog = Assert.IsType<ImportedContentCatalog>(ImportedContentCatalog.Discover(root));

            Assert.Equal(bytes, ImportedSiegeLayouts.LoadCombatPalette(catalog).Rgb);
            Assert.Null(catalog.FindId("resource", ImportedSiegeLayouts.CombatPaletteSuffix));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void RuntimeRequiresACompleteSupportedOriginalAssetImport()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-required-assets-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var missing = Assert.Throws<InvalidDataException>(() =>
                ImportedContentCatalog.LoadRequired(root));
            Assert.Contains("resources are required", missing.Message);

            new ImportManifest(1, SupportedOriginalReleases.GogEnglishSourceImageSha256, []).Write(
                Path.Combine(root, "manifest.json"));
            var incomplete = Assert.Throws<InvalidDataException>(() =>
                ImportedContentCatalog.LoadRequired(root));
            Assert.Contains("import is incomplete", incomplete.Message);

            new ImportManifest(1, new string('0', 64), []).Write(Path.Combine(root, "manifest.json"));
            var unsupported = Assert.Throws<InvalidDataException>(() =>
                ImportedContentCatalog.LoadRequired(root));
            Assert.Contains("not from a supported", unsupported.Message);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }
}
