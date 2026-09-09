using Conqueror.Resources;

namespace Conqueror.Game;

public sealed record InnPatronHotspot(int HatRegionId, UiBounds Bounds);

public sealed record InnPresentationLayout(
    IReadOnlyList<InnPatronHotspot> Patrons,
    UiBounds HoverLabelBounds);

public static class InnPresentationDefinitions
{
    public const int PatronCount = 10;

    public static InnPresentationLayout Fallback { get; } = new(
        [
            new(0, new UiBounds(3, 233, 38, 70)),
            new(1, new UiBounds(51, 221, 35, 74)),
            new(2, new UiBounds(98, 156, 42, 102)),
            new(3, new UiBounds(226, 193, 47, 60)),
            new(4, new UiBounds(435, 96, 70, 70)),
            new(5, new UiBounds(456, 176, 66, 217)),
            new(6, new UiBounds(185, 157, 28, 101)),
            new(7, new UiBounds(241, 333, 121, 102)),
            new(8, new UiBounds(146, 267, 72, 81)),
            new(9, new UiBounds(22, 307, 78, 93))
        ],
        new UiBounds(1, 437, 634, 41));

    public static InnPresentationLayout From(HatLayout? layout) => new(
        Fallback.Patrons.Select(patron => layout?.FindRegion(patron.HatRegionId) is { Enabled: not 0 } region
            ? patron with { Bounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : patron).ToArray(),
        layout?.FindRegion(10) is { Enabled: not 0 } footer
            ? new UiBounds(footer.X, footer.Y, footer.Width, footer.Height)
            : Fallback.HoverLabelBounds);
}
