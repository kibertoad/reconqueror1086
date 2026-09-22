using Conqueror.Resources;
using Microsoft.Xna.Framework;

namespace Conqueror.Game;

/// <summary>
/// Original Village screen controls from VOPTS.HAT. The lender remains a
/// keyboard-only compatibility shortcut until its separate original dialogue
/// flow has been recovered; the other four routes have executable-backed
/// runtime destinations.
/// </summary>
public enum VillageHotspotAction { Map, Inn, Blacksmith, Lender, Church }

public sealed record VillageHotspot(VillageHotspotAction Action, string HoverLabel, int HatRegionId, UiBounds Bounds);

public static class VillagePresentationDefinitions
{
    public static UiBounds HoverLabelBounds { get; } = new(4, 319, 137, 161);

    // VOPTS.HAT's background is TOWN_Y.PCX. Regions 1-5 retain the order and
    // semantic labels used by VILLAGE.DAT: map, inn, smith, lender, church.
    public static IReadOnlyList<VillageHotspot> Hotspots { get; } =
    [
        new(VillageHotspotAction.Map, "Map", 1, new UiBounds(541, 347, 100, 54)),
        new(VillageHotspotAction.Inn, "Inn", 2, new UiBounds(157, 166, 73, 39)),
        new(VillageHotspotAction.Blacksmith, "Blacksmith", 3, new UiBounds(388, 229, 43, 30)),
        new(VillageHotspotAction.Lender, "Moneylender", 4, new UiBounds(225, 257, 35, 19)),
        new(VillageHotspotAction.Church, "Church", 5, new UiBounds(489, 192, 52, 253))
    ];

    public static IReadOnlyList<VillageHotspot> HotspotsFrom(HatLayout? layout) => Hotspots
        .Select(hotspot => layout?.FindRegion(hotspot.HatRegionId) is { Enabled: not 0 } region
            ? hotspot with { Bounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : hotspot)
        .ToArray();

    public static VillageHotspotAction? Hit(IReadOnlyList<VillageHotspot> hotspots, Point point) => hotspots
        .FirstOrDefault(hotspot => hotspot.Bounds.Contains(point.X, point.Y))?.Action;
}
