using Conqueror.Core;
using Conqueror.Resources;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public enum ShopControlAction
{
    Previous,
    Next,
    View,
    Transaction,
    Exit
}

public sealed record ShopControl(ShopControlAction Action, UiBounds Bounds);
public enum ShopOverlayState { ViewUnavailable, ViewAvailable, Sell, Purchase }
public sealed record ShopOverlay(ShopOverlayState State, ShopControlAction Control, int Frame);

public enum SceneNavigationAction
{
    Overview,
    Castle,
    Farm,
    Village,
    Forest,
    WarPlanning,
    Exit,
    Jump,
    Map,
    Orders,
    BlacksmithDialogue,
    Shop
}
public sealed record SceneHotspot(SceneNavigationAction Action, string HoverLabel, int HatRegionId, UiBounds Bounds);

public static class HomePresentationDefinitions
{
    public static UiBounds HoverLabelBounds { get; } = new(1, 443, 638, 35);
    // FOPTS.HAT is the TACTICAL.PCX office descriptor. The executable's adjacent label trio maps
    // the three left-to-right ledger spines to Farm, Village, and Forest. Exits and the small desk
    // object still require dispatch evidence.
    public static IReadOnlyList<SceneHotspot> Hotspots { get; } =
    [
        new(SceneNavigationAction.Overview, "Overview", 0, new UiBounds(248, 184, 64, 31)),
        new(SceneNavigationAction.Castle, "Castle", 1, new UiBounds(172, 153, 72, 44)),
        new(SceneNavigationAction.Farm, "Farm", 2, new UiBounds(277, 146, 17, 38)),
        new(SceneNavigationAction.Village, "Village", 3, new UiBounds(295, 141, 18, 43)),
        new(SceneNavigationAction.Forest, "Forest", 4, new UiBounds(314, 147, 15, 39)),
        new(SceneNavigationAction.Orders, "Orders", 5, new UiBounds(359, 123, 35, 76)),
        new(SceneNavigationAction.Map, "Map", 8, new UiBounds(200, 55, 68, 85))
    ];

    public static IReadOnlyList<SceneHotspot> HotspotsFrom(HatLayout? layout) => Hotspots
        .Select(hotspot => layout?.FindRegion(hotspot.HatRegionId) is { Enabled: not 0 } region
            ? hotspot with { Bounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : hotspot)
        .ToArray();
}

public static class ShopPresentationDefinitions
{
    private const int ItemViewportWidth = 240;

    public static IReadOnlyList<ShopControl> Controls { get; } =
    [
        new(ShopControlAction.Previous, new UiBounds(244, 405, 54, 63)),
        new(ShopControlAction.Next, new UiBounds(298, 405, 52, 63)),
        new(ShopControlAction.View, new UiBounds(350, 400, 77, 80)),
        new(ShopControlAction.Transaction, new UiBounds(427, 417, 105, 51)),
        new(ShopControlAction.Exit, new UiBounds(533, 417, 82, 51))
    ];

    public static IReadOnlyList<ShopOverlay> Overlays { get; } =
    [
        new(ShopOverlayState.ViewUnavailable, ShopControlAction.View, 0),
        new(ShopOverlayState.ViewAvailable, ShopControlAction.View, 1),
        new(ShopOverlayState.Sell, ShopControlAction.Transaction, 2),
        new(ShopOverlayState.Purchase, ShopControlAction.Transaction, 3)
    ];

    public static UiBounds Description { get; } = new(272, 28, 344, 330);
    public static UiBounds Wealth { get; } = new(284, 365, 90, 40);
    public static UiBounds Price { get; } = new(520, 365, 70, 40);

    public static UiBounds ItemBounds(int width, int height) => new((ItemViewportWidth - width) / 2, 20, width, height);

    public static ShopOverlay ViewOverlay(bool available) => Overlay(available
        ? ShopOverlayState.ViewAvailable
        : ShopOverlayState.ViewUnavailable);

    public static ShopOverlay TransactionOverlay(bool owned) => Overlay(owned
        ? ShopOverlayState.Sell
        : ShopOverlayState.Purchase);

    public static UiBounds OverlayBounds(ShopOverlay overlay, int width, int height)
    {
        var control = Controls.Single(item => item.Action == overlay.Control).Bounds;
        return new UiBounds(
            control.X + (control.Width - width) / 2,
            control.Y + (control.Height - height) / 2,
            width,
            height);
    }

    private static ShopOverlay Overlay(ShopOverlayState state) => Overlays.Single(overlay => overlay.State == state);
}

public static class BlacksmithPresentationDefinitions
{
    private const int BlacksmithRegionId = 0;
    public static UiBounds Blacksmith { get; } = new(253, 109, 107, 164);
    public static UiBounds HoverLabelBounds { get; } = new(1, 437, 634, 41);

    public static IReadOnlyList<SceneHotspot> Hotspots { get; } =
    [
        new(SceneNavigationAction.BlacksmithDialogue, "Blacksmith", BlacksmithRegionId, Blacksmith),
        new(SceneNavigationAction.Shop, "Buy/Sell", 2, new UiBounds(54, 2, 180, 141))
    ];

    public static IReadOnlyList<SceneHotspot> HotspotsFrom(HatLayout? layout) => Hotspots
        .Select(hotspot => layout?.FindRegion(hotspot.HatRegionId) is { Enabled: not 0 } region
            ? hotspot with { Bounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : hotspot)
        .ToArray();
}

public enum BlacksmithDialogueAction { Shop, Return }
public sealed record BlacksmithDialogueCommand(
    BlacksmithDialogueAction Action,
    string Label,
    IReadOnlyList<Keys> Keys);

public static class BlacksmithDialoguePresentationDefinitions
{
    public const string Speaker = "BLACKSMITH";
    public const string FallbackPrompt = "The smith waits for your question.";
    public static UiBounds Portrait { get; } = new(39, 31, 195, 203);
    public static IReadOnlyList<BlacksmithDialogueCommand> Commands { get; } =
    [
        new(BlacksmithDialogueAction.Shop, "1  BROWSE WEAPONS", [Keys.D1, Keys.B]),
        new(BlacksmithDialogueAction.Return, "2  RETURN TO THE FORGE", [Keys.D2, Keys.Enter])
    ];
}

public static class FarmPresentationDefinitions
{
    public enum Section { Castle, Village, Farm, Forest }
    public enum FooterAction { Okay, Cancel, FullScreen }
    public sealed record Entry(string Label, FarmAction? Action = null);

    public sealed record Layout(
        Section Section,
        string Title,
        string LayoutRole,
        string LayoutSuffix,
        int AccountRowCount,
        int OkayRegionId,
        int CancelRegionId,
        int? ExtraRegionId,
        int TerrainRegionId,
        int FullScreenRegionId,
        UiBounds Terrain,
        UiBounds FullScreen,
        UiBounds Okay,
        UiBounds Cancel,
        UiBounds? Extra)
    {
        public IReadOnlyList<UiBounds> Rows { get; init; } = [];
    }

    public static UiBounds Information { get; } = new(20, 22, 330, 390);
    public static UiBounds Terrain { get; } = new(382, 22, 238, 390);

    private static IReadOnlyList<Layout> FallbackLayouts { get; } =
    [
        new(Section.Castle, "CASTLE MANAGEMENT", "Fief.Castle", ":fcastle.hat", 18, 18, 19, null, 20, 21,
            new UiBounds(383, 20, 232, 380), new UiBounds(532, 0, 86, 16),
            new UiBounds(30, 435, 30, 20), new UiBounds(70, 435, 70, 20), null) { Rows = DefaultRows(18) },
        new(Section.Village, "VILLAGE MANAGEMENT", "Fief.Village", ":fvillage.hat", 14, 14, 15, 16, 17, 18,
            new UiBounds(383, 20, 232, 380), new UiBounds(532, 0, 86, 16),
            new UiBounds(30, 435, 30, 20), new UiBounds(70, 435, 70, 20), new UiBounds(250, 435, 80, 20)) { Rows = DefaultRows(14) },
        new(Section.Farm, "FARM MANAGEMENT", "Fief.Farm", ":ffarm.hat", 9, 9, 10, null, 11, 12,
            new UiBounds(383, 20, 232, 380), new UiBounds(532, 0, 86, 16),
            new UiBounds(30, 435, 30, 20), new UiBounds(70, 435, 70, 20), null) { Rows = DefaultRows(9) },
        new(Section.Forest, "FOREST MANAGEMENT", "Fief.Forest", ":fforest.hat", 8, 8, 9, null, 10, 11,
            new UiBounds(383, 20, 232, 380), new UiBounds(532, 0, 86, 16),
            new UiBounds(30, 435, 30, 20), new UiBounds(70, 435, 70, 20), null) { Rows = DefaultRows(8) }
    ];

    private static IReadOnlyDictionary<Section, IReadOnlyList<Entry>> Entries { get; } =
        new Dictionary<Section, IReadOnlyList<Entry>>
        {
            [Section.Castle] =
            [
                new("Wall"), new("Tower"), new("Great Hall"), new("Servant Room", new BuildFarmAction(BuildingKind.ServantRoom)),
                new("Guardhouse"), new("Gate House"), new("Storehouse"), new("Chapel"), new("Well"), new("Stable"),
                new("Steward", new BuildFarmAction(BuildingKind.Steward)), new("Beadle", new BuildFarmAction(BuildingKind.Beadle)),
                new("Guard Captain"), new("Guard"), new("Priest", new BuildFarmAction(BuildingKind.Priest)), new("Mason"), new("Serf")
            ],
            [Section.Village] =
            [
                new("Clear Land"), new("Road"), new("Mill"), new("Tavern"), new("Bakery"), new("Inn"),
                new("Carpenter"), new("Smith"), new("Tanner"), new("Merchant"), new("Church", new BuildFarmAction(BuildingKind.Church)),
                new("Monastery", new BuildFarmAction(BuildingKind.Monastery)), new("Barber"), new("Houses", new BuildFarmAction(BuildingKind.House)),
                new("Livestock"), new("Horses"), new("Granary")
            ],
            [Section.Farm] =
            [
                new("Grain", new PlantFarmAction(CropType.Grain)), new("Beans", new PlantFarmAction(CropType.Beans)),
                new("Vegetables", new PlantFarmAction(CropType.Vegetables)), new("Fruit", new PlantFarmAction(CropType.Fruit))
            ],
            [Section.Forest] =
            [
                new("Cut Timber", new DevelopForestFarmAction(ForestIndustry.Timber)),
                new("Iron Mine", new DevelopForestFarmAction(ForestIndustry.IronMine)),
                new("Woodward", new BuildFarmAction(BuildingKind.Woodward)),
                new("Coal Mine", new DevelopForestFarmAction(ForestIndustry.CoalMine)),
                new("Gold Mine", new DevelopForestFarmAction(ForestIndustry.GoldMine)),
                new("Silver Mine", new DevelopForestFarmAction(ForestIndustry.SilverMine)), new("Prospector")
            ]
        };

    public static IReadOnlyList<Layout> Layouts { get; } = FallbackLayouts;
    public static IReadOnlyList<Entry> EntriesFor(Section section) => Entries[section];

    public static Layout LayoutFrom(Section section, HatLayout? source)
    {
        var fallback = FallbackLayouts.Single(layout => layout.Section == section);
        return fallback with
        {
            Terrain = RegionBounds(source, fallback.TerrainRegionId, fallback.Terrain),
            FullScreen = RegionBounds(source, fallback.FullScreenRegionId, fallback.FullScreen),
            Okay = RegionBounds(source, fallback.OkayRegionId, fallback.Okay),
            Cancel = RegionBounds(source, fallback.CancelRegionId, fallback.Cancel),
            Extra = fallback.ExtraRegionId is { } extraId
                ? RegionBounds(source, extraId, fallback.Extra!)
                : null,
            Rows = Enumerable.Range(0, fallback.AccountRowCount)
                .Select(id => RegionBounds(source, id, fallback.Rows[id]))
                .ToArray()
        };
    }

    public static IReadOnlyList<FarmCommand> Commands { get; } =
    [
        new(Keys.D1, "1 STEWARD", 0, new BuildFarmAction(BuildingKind.Steward)),
        new(Keys.D2, "2 BEADLE", 0, new BuildFarmAction(BuildingKind.Beadle)),
        new(Keys.D3, "3 PRIEST", 0, new BuildFarmAction(BuildingKind.Priest)),
        new(Keys.D4, "4 SERVANT ROOM", 0, new BuildFarmAction(BuildingKind.ServantRoom)),
        new(Keys.D5, "5 HOUSE", 0, new BuildFarmAction(BuildingKind.House)),
        new(Keys.D6, "6 MONASTERY", 0, new BuildFarmAction(BuildingKind.Monastery)),
        new(Keys.D7, "7 BEANS", 1, new PlantFarmAction(CropType.Beans)),
        new(Keys.D8, "8 VEGETABLES", 1, new PlantFarmAction(CropType.Vegetables)),
        new(Keys.Z, "Z GRAIN", 1, new PlantFarmAction(CropType.Grain)),
        new(Keys.X, "X FRUIT", 1, new PlantFarmAction(CropType.Fruit)),
        new(Keys.D9, "9 TIMBER", 2, new DevelopForestFarmAction(ForestIndustry.Timber)),
        new(Keys.G, "G GOLD", 2, new DevelopForestFarmAction(ForestIndustry.GoldMine)),
        new(Keys.I, "I IRON", 2, new DevelopForestFarmAction(ForestIndustry.IronMine)),
        new(Keys.C, "C COAL", 2, new DevelopForestFarmAction(ForestIndustry.CoalMine)),
        new(Keys.S, "S SILVER", 2, new DevelopForestFarmAction(ForestIndustry.SilverMine)),
        new(Keys.Q, "Q SWORDSMEN", 3, new RecruitFarmAction(UnitType.Swordsmen)),
        new(Keys.W, "W HALBERDIERS", 3, new RecruitFarmAction(UnitType.Halberdiers)),
        new(Keys.R, "R KNIGHTS", 3, new RecruitFarmAction(UnitType.Knights)),
        new(Keys.Enter, "ENTER OFFICE", 4, new LeaveFarmAction()),
        new(Keys.H, "H OFFICE", 4, new LeaveFarmAction())
    ];

    public static IReadOnlyList<FarmCommand> CommandsFor(Section section) => Commands
        .Where(command => command.Action switch
        {
            BuildFarmAction build => section switch
            {
                Section.Castle => build.Building is BuildingKind.Steward or BuildingKind.Beadle
                    or BuildingKind.Priest or BuildingKind.ServantRoom,
                Section.Village => build.Building is BuildingKind.House or BuildingKind.Church or BuildingKind.Monastery,
                Section.Forest => build.Building == BuildingKind.Woodward,
                _ => false
            },
            RecruitFarmAction => false,
            PlantFarmAction => section == Section.Farm,
            DevelopForestFarmAction => section == Section.Forest,
            LeaveFarmAction => true,
            _ => false
        })
        .ToArray();

    public static IReadOnlyList<string> HelpRowsFor(Section section) => CommandsFor(section)
        .GroupBy(command => command.HelpRow)
        .OrderBy(group => group.Key)
        .Select(group => string.Join("   ", group.Select(command => command.Label)))
        .ToArray();

    public static FooterAction? FooterActionAt(Layout layout, int x, int y)
    {
        if (layout.Okay.Contains(x, y)) return FooterAction.Okay;
        if (layout.Cancel.Contains(x, y)) return FooterAction.Cancel;
        if (layout.FullScreen.Contains(x, y)) return FooterAction.FullScreen;
        return null;
    }

    private static UiBounds RegionBounds(HatLayout? layout, int id, UiBounds fallback) =>
        layout?.FindRegion(id) is { Enabled: not 0 } region
            ? new UiBounds(region.X, region.Y, region.Width, region.Height)
            : fallback;

    private static IReadOnlyList<UiBounds> DefaultRows(int count) => Enumerable.Range(0, count)
        .Select(row => new UiBounds(37, 68 + row * 14, 300, 14))
        .ToArray();
}

public abstract record FarmAction;
public sealed record BuildFarmAction(BuildingKind Building) : FarmAction;
public sealed record PlantFarmAction(CropType Crop) : FarmAction;
public sealed record DevelopForestFarmAction(ForestIndustry Industry) : FarmAction;
public sealed record RecruitFarmAction(UnitType Unit) : FarmAction;
public sealed record LeaveFarmAction : FarmAction;
public sealed record FarmCommand(Keys Key, string Label, int HelpRow, FarmAction Action);
