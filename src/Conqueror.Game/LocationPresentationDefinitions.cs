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
    // FCASTLE.HAT provides rectangles but not their action/label bindings. Keep this empty until
    // executable dispatch or controlled hover observations establish the region-to-target mapping.
    public static IReadOnlyList<SceneHotspot> Hotspots { get; } = [];

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
    public static UiBounds Information { get; } = new(20, 22, 330, 390);
    public static UiBounds Terrain { get; } = new(382, 22, 238, 390);

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

    public static IReadOnlyList<string> HelpRows { get; } = Commands
        .GroupBy(command => command.HelpRow)
        .OrderBy(group => group.Key)
        .Select(group => string.Join("   ", group.Select(command => command.Label)))
        .ToArray();
}

public abstract record FarmAction;
public sealed record BuildFarmAction(BuildingKind Building) : FarmAction;
public sealed record PlantFarmAction(CropType Crop) : FarmAction;
public sealed record DevelopForestFarmAction(ForestIndustry Industry) : FarmAction;
public sealed record RecruitFarmAction(UnitType Unit) : FarmAction;
public sealed record LeaveFarmAction : FarmAction;
public sealed record FarmCommand(Keys Key, string Label, int HelpRow, FarmAction Action);
