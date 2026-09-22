using Conqueror.Game;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void VillageSceneCatalogDecodesOneCompleteScene()
    {
        var source = "V31_1111.PCX\n1,540,345,98,55; Map\n0,0,0,0,0; Tournament\n1,418,39,52,38; Inn\n1,476,148,112,80; Smith\n1,136,158,35,34; Lend\n1,342,79,71,216; Church\n";
        var scene = Assert.Single(VillageSceneCatalogDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(source)));
        Assert.Equal("V31_1111.PCX", scene.BackgroundName);
        Assert.Equal((true, 540, 345, 98, 55), (scene.Hotspots[0].Enabled, scene.Hotspots[0].X, scene.Hotspots[0].Y, scene.Hotspots[0].Width, scene.Hotspots[0].Height));
        Assert.False(scene.Hotspots[1].Enabled);
    }

    [Theory]
    [InlineData("V31_1111.PCX\n1,1,1,1,1; Map\n")]
    [InlineData("X31_1111.PCX\n1,1,1,1,1; A\n1,1,1,1,1; B\n1,1,1,1,1; C\n1,1,1,1,1; D\n1,1,1,1,1; E\n1,1,1,1,1; F\n")]
    [InlineData("V31_1111.PCX\n2,1,1,1,1; A\n1,1,1,1,1; B\n1,1,1,1,1; C\n1,1,1,1,1; D\n1,1,1,1,1; E\n1,1,1,1,1; F\n")]
    public void VillageSceneCatalogRejectsMalformedRecords(string source)
    {
        Assert.Throws<InvalidDataException>(() => VillageSceneCatalogDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(source)));
    }

    [Fact]
    public void VillagePresentationUsesTheDecodedTownBackgroundAndHotspotOrder()
    {
        Assert.Contains(new ImportedLayoutDefinition("Village", ":vopts.hat"), ImportedLayouts.Definitions);
        Assert.Equal(
        [
            VillageHotspotAction.Map,
            VillageHotspotAction.Inn,
            VillageHotspotAction.Blacksmith,
            VillageHotspotAction.Lender,
            VillageHotspotAction.Church
        ], VillagePresentationDefinitions.Hotspots.Select(hotspot => hotspot.Action));
        Assert.Equal(VillageHotspotAction.Inn,
            VillagePresentationDefinitions.Hit(VillagePresentationDefinitions.Hotspots, new Point(160, 170)));
        Assert.Equal(VillageHotspotAction.Church,
            VillagePresentationDefinitions.Hit(VillagePresentationDefinitions.Hotspots, new Point(500, 300)));
    }
}
