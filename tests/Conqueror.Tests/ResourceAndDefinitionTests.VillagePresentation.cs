using Conqueror.Core;
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

    [Theory]
    [InlineData("Smith", "Lend")]
    [InlineData("Blacksmith", "Lender")]
    public void SelectedVillageSceneUsesItsEnabledCatalogRegions(string smithLabel, string lenderLabel)
    {
        var scene = Assert.Single(VillageSceneCatalogDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(
            $"V31_1111.PCX\n1,540,345,98,55; Map\n0,0,0,0,0; Tournament\n1,418,39,52,38; Inn\n1,476,148,112,80; {smithLabel}\n1,136,158,35,34; {lenderLabel}\n1,342,79,71,216; Church\n")));

        var hotspots = VillagePresentationDefinitions.HotspotsFrom(scene);

        Assert.Equal(
        [
            VillageHotspotAction.Map,
            VillageHotspotAction.Inn,
            VillageHotspotAction.Blacksmith,
            VillageHotspotAction.Lender,
            VillageHotspotAction.Church
        ], hotspots.Select(hotspot => hotspot.Action));
        Assert.Equal(new UiBounds(476, 148, 112, 80), hotspots[2].Bounds);
        Assert.Equal(VillageHotspotAction.Blacksmith,
            VillagePresentationDefinitions.Hit(hotspots, new Point(480, 150)));
    }

    [Theory]
    [InlineData(17, 27)]
    [InlineData(18, 21)]
    [InlineData(24, 33)]
    [InlineData(29, 39)]
    [InlineData(86, 36)]
    [InlineData(144, 35)]
    [InlineData(166, 54)]
    public void OriginalStartingHomesUseTheirPersonVillageSceneIndex(int person, int sceneIndex)
    {
        Assert.Equal(sceneIndex, OriginalStrategicMovement.Persons[person].VillageSceneIndex);
        var scenes = Enumerable.Range(0, 67)
            .Select(index => new VillageSceneDefinition($"V{index}_1111.PCX", []))
            .ToArray();
        var state = new CampaignState
        {
            CurrentLocation = 0,
            OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 1, 1),
                OriginalStrategicMovement.StartingRoutes.Single(route => route.Person == person).Selector)
        };

        Assert.Same(scenes[sceneIndex], OriginalVillageScenePresentation.SceneForNewGameHome(state, scenes));
        state.CurrentLocation = 1;
        Assert.Null(OriginalVillageScenePresentation.SceneForNewGameHome(state, scenes));
    }
}
