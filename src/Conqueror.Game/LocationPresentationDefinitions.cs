using Conqueror.Resources;

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

    public static UiBounds BlacksmithFrom(HatLayout? layout) => layout?.FindRegion(BlacksmithRegionId) is { } region
        ? new UiBounds(region.X, region.Y, region.Width, region.Height)
        : Blacksmith;
}

public static class FarmPresentationDefinitions
{
    public static UiBounds Information { get; } = new(20, 22, 330, 390);
    public static UiBounds Terrain { get; } = new(382, 22, 238, 390);
}
