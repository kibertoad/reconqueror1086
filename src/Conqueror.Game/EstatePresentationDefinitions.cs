using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public enum EstatePanel { Map, Orders, Help }
public enum EstateControlAction { Map, Orders, Help, Home, Village }
public enum EstateTerrainKind { Meadow, HedgedField, Forest, Grain, Beans, Vegetables, Fruit, Settlement }
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
    public const int Columns = 5;
    public const int Rows = 14;
    private const int WorldWidth = 760;
    private const int WorldHeight = 768;
    public const int OriginalTileSize = 80;

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

    // The atlases and matching frame order are confirmed. These semantic choices are
    // intentionally provisional until the executable's estate map table is recovered.
    public static IReadOnlyDictionary<EstateTerrainKind, int> TileFrames { get; } =
        new Dictionary<EstateTerrainKind, int>
        {
            [EstateTerrainKind.Meadow] = 293,
            [EstateTerrainKind.HedgedField] = 258,
            [EstateTerrainKind.Forest] = 255,
            [EstateTerrainKind.Grain] = 287,
            [EstateTerrainKind.Beans] = 290,
            [EstateTerrainKind.Vegetables] = 289,
            [EstateTerrainKind.Fruit] = 292,
            [EstateTerrainKind.Settlement] = 24
        };

    private static EstateTerrainKind[] BasePattern { get; } =
    [
        EstateTerrainKind.Meadow, EstateTerrainKind.HedgedField, EstateTerrainKind.Meadow,
        EstateTerrainKind.Forest, EstateTerrainKind.HedgedField, EstateTerrainKind.Meadow,
        EstateTerrainKind.HedgedField, EstateTerrainKind.Forest, EstateTerrainKind.Meadow,
        EstateTerrainKind.HedgedField
    ];

    private static IReadOnlyDictionary<CropType, EstateTerrainKind> CropKinds { get; } =
        new Dictionary<CropType, EstateTerrainKind>
        {
            [CropType.Grain] = EstateTerrainKind.Grain,
            [CropType.Beans] = EstateTerrainKind.Beans,
            [CropType.Vegetables] = EstateTerrainKind.Vegetables,
            [CropType.Fruit] = EstateTerrainKind.Fruit
        };

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

    public static IReadOnlyList<EstateTerrainKind> TerrainFor(Fief fief)
    {
        var count = Columns * Rows;
        var result = Enumerable.Range(0, count).Select(index => BasePattern[index % BasePattern.Length]).ToArray();
        var developed = CropKinds.SelectMany(pair => Enumerable.Repeat(pair.Value, fief.Crops[pair.Key]))
            .Concat(Enumerable.Repeat(EstateTerrainKind.Forest, fief.Forest.Values.Sum()))
            .Concat(Enumerable.Repeat(EstateTerrainKind.Settlement, Math.Min(6, fief.Houses)))
            .Take(count)
            .ToArray();
        for (var index = 0; index < developed.Length; index++) result[(index * 11 + 7) % count] = developed[index];
        return result;
    }

    public static UiBounds TileSpriteBounds(UiBounds viewport, int index)
    {
        if (index < 0 || index >= Columns * Rows) throw new ArgumentOutOfRangeException(nameof(index));
        var column = index % Columns;
        var row = index / Columns;
        var x = viewport.X + column * (viewport.Width - OriginalTileSize) / (Columns - 1);
        var y = viewport.Y + row * (viewport.Height - OriginalTileSize) / (Rows - 1);
        return new UiBounds(x, y, OriginalTileSize, OriginalTileSize);
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
