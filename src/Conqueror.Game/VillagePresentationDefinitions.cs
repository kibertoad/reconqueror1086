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

    /// <summary>
    /// Converts a selected <c>VILLAGE.DAT</c> record into its enabled source
    /// hit regions. Tournament remains absent because its callback path has
    /// not yet been recovered; the mapped exterior labels retain their
    /// catalog rectangles rather than borrowing <c>VOPTS.HAT</c> geometry.
    /// </summary>
    public static IReadOnlyList<VillageHotspot> HotspotsFrom(VillageSceneDefinition scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        return scene.Hotspots
            .Where(hotspot => hotspot.Enabled && TryActionForLabel(hotspot.Label, out _))
            .Select(hotspot =>
            {
                _ = TryActionForLabel(hotspot.Label, out var action);
                return new VillageHotspot(action, hotspot.Label, -1,
                    new UiBounds(hotspot.X, hotspot.Y, hotspot.Width, hotspot.Height));
            })
            .ToArray();
    }

    public static VillageHotspotAction? Hit(IReadOnlyList<VillageHotspot> hotspots, Point point) => hotspots
        .FirstOrDefault(hotspot => hotspot.Bounds.Contains(point.X, point.Y))?.Action;

    private static bool TryActionForLabel(string label, out VillageHotspotAction action)
    {
        switch (label)
        {
            case "Map": action = VillageHotspotAction.Map; return true;
            case "Inn": action = VillageHotspotAction.Inn; return true;
            case "Smith": action = VillageHotspotAction.Blacksmith; return true;
            case "Lend": action = VillageHotspotAction.Lender; return true;
            case "Church": action = VillageHotspotAction.Church; return true;
            default: action = default; return false;
        }
    }
}
