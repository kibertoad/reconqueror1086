using Conqueror.Game;
using Microsoft.Xna.Framework;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
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
