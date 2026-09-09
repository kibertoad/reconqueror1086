using Conqueror.Resources;

namespace Conqueror.Game;

public sealed record InnPatronHotspot(
    int HatRegionId,
    string Name,
    string PortraitRole,
    string PortraitSuffix,
    UiBounds Bounds);

public sealed record InnPresentationLayout(
    IReadOnlyList<InnPatronHotspot> Patrons,
    UiBounds ExitBounds,
    UiBounds HoverLabelBounds);

public static class InnPresentationDefinitions
{
    public const int PatronCount = 10;

    public static InnPresentationLayout Fallback { get; } = new(
        [
            new(0, "Frederick", "Inn.Portrait.Frederick", ":frederic.pcc", new UiBounds(3, 233, 38, 70)),
            new(1, "Gerard", "Inn.Portrait.Gerard", ":gerard.pcc", new UiBounds(51, 221, 35, 74)),
            new(2, "Barkeep", "Inn.Portrait.Barkeep", ":barkeep.pcc", new UiBounds(98, 156, 42, 102)),
            new(3, "Otto", "Inn.Portrait.Otto", ":otto.pcc", new UiBounds(226, 193, 47, 60)),
            new(4, "Hugh", "Inn.Portrait.Hugh", ":hugh.pcc", new UiBounds(435, 96, 70, 70)),
            new(5, "Gilbert", "Inn.Portrait.Gilbert", ":gilbert.pcc", new UiBounds(456, 176, 66, 217)),
            new(6, "Nellie", "Inn.Portrait.Nellie", ":nellie.pcc", new UiBounds(185, 157, 28, 101)),
            new(7, "Richard", "Inn.Portrait.Richard", ":richard.pcc", new UiBounds(241, 333, 121, 102)),
            new(8, "Ivo", "Inn.Portrait.Ivo", ":ivo.pcc", new UiBounds(146, 267, 72, 81)),
            new(9, "Albert", "Inn.Portrait.Albert", ":albert.pcc", new UiBounds(22, 307, 78, 93))
        ],
        new UiBounds(1, 437, 634, 41),
        new UiBounds(1, 437, 634, 41));

    public static InnPresentationLayout From(HatLayout? layout) => new(
        Fallback.Patrons.Select(patron => layout?.FindRegion(patron.HatRegionId) is { Enabled: not 0 } region
            ? patron with { Bounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : patron).ToArray(),
        layout?.FindRegion(10) is { Enabled: not 0 } exit
            ? new UiBounds(exit.X, exit.Y, exit.Width, exit.Height)
            : Fallback.ExitBounds,
        layout?.FindRegion(11) is { Enabled: not 0 } footer
            ? new UiBounds(footer.X, footer.Y, footer.Width, footer.Height)
            : Fallback.HoverLabelBounds);
}
