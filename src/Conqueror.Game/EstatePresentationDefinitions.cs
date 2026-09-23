using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public enum EstatePanel { Map, Orders, Help }
public enum EstateControlAction { Map, Orders, Help, Home, Village }
public enum EstateSeason { SpringSummer, Autumn, Winter }

public sealed record EstateControl(
    EstateControlAction Action,
    string Label,
    int HatRegionId,
    UiBounds Bounds,
    EstatePanel? Panel = null);
public sealed record EstateTileAtlas(EstateSeason Season, string Role, string IdSuffix);

public sealed record EstateLayout(
    UiBounds MainViewport,
    UiBounds InsetMap,
    UiBounds InformationPanel,
    UiBounds FooterStatus,
    IReadOnlyList<EstateControl> Controls);

public static class EstatePresentationDefinitions
{
    private const int WorldWidth = 760;
    private const int WorldHeight = 768;

    public static EstateLayout Fallback { get; } = new(
        new UiBounds(19, 8, 370, 433),
        new UiBounds(422, 4, 199, 159),
        new UiBounds(422, 178, 199, 264),
        new UiBounds(200, 455, 200, 24),
        [
            new(EstateControlAction.Home, "HOME", 11, new UiBounds(18, 455, 180, 24)),
            new(EstateControlAction.Village, "VILLAGE", 12, new UiBounds(400, 455, 200, 24)),
            new(EstateControlAction.Map, "MAP", 15, new UiBounds(424, 163, 50, 20), EstatePanel.Map),
            new(EstateControlAction.Orders, "ORDERS", 16, new UiBounds(478, 164, 73, 17), EstatePanel.Orders),
            new(EstateControlAction.Help, "HELP", 17, new UiBounds(554, 163, 65, 20), EstatePanel.Help)
        ]);

    public static IReadOnlyList<EstateTileAtlas> TileAtlases { get; } =
    [
        new(EstateSeason.SpringSummer, "Estate.Tiles.SpringSummer", ":ics.csf"),
        new(EstateSeason.Autumn, "Estate.Tiles.Autumn", ":ica.csf"),
        new(EstateSeason.Winter, "Estate.Tiles.Winter", ":icw.csf")
    ];

    public static EstateLayout From(HatLayout? layout)
    {
        UiBounds Region(int id, UiBounds fallback) => layout?.FindRegion(id) is { } region
            ? new UiBounds(region.X, region.Y, region.Width, region.Height)
            : fallback;

        return new EstateLayout(
            Region(10, Fallback.MainViewport),
            Region(0, Fallback.InsetMap),
            Region(9, Fallback.InformationPanel),
            Region(13, Fallback.FooterStatus),
            Fallback.Controls.Select(control => control with { Bounds = Region(control.HatRegionId, control.Bounds) }).ToArray());
    }

    public static EstateSeason SeasonFor(DateTime date) => date.Month switch
    {
        9 or 10 or 11 => EstateSeason.Autumn,
        12 or 1 or 2 => EstateSeason.Winter,
        _ => EstateSeason.SpringSummer
    };

    public static EstateTileAtlas AtlasFor(DateTime date)
    {
        var season = SeasonFor(date);
        return TileAtlases.Single(atlas => atlas.Season == season);
    }

    public static (int X, int Y) InsetPoint(UiBounds inset, WorldLocation location) =>
        InsetPoint(inset, location.X, location.Y);

    public static (int X, int Y) InsetPoint(UiBounds inset, int worldX, int worldY) =>
        (inset.X + worldX * inset.Width / WorldWidth, inset.Y + worldY * inset.Height / WorldHeight);

    public static int LocationAt(UiBounds inset, int x, int y) => World.Locations
        .Select((location, index) => (Index: index, Point: InsetPoint(inset, location)))
        .MinBy(candidate => SquaredDistance(candidate.Point, x, y)).Index;

    private static long SquaredDistance((int X, int Y) point, int x, int y)
    {
        var dx = point.X - x;
        var dy = point.Y - y;
        return (long)dx * dx + (long)dy * dy;
    }
}
