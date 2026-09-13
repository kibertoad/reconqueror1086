using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
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
