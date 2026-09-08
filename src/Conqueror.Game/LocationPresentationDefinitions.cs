using Conqueror.Resources;

namespace Conqueror.Game;

public enum ShopControlAction
{
    Previous,
    Next,
    View,
    Purchase,
    Exit
}

public sealed record ShopControl(ShopControlAction Action, UiBounds Bounds);

public static class ShopPresentationDefinitions
{
    public static IReadOnlyList<ShopControl> Controls { get; } =
    [
        new(ShopControlAction.Previous, new UiBounds(244, 405, 54, 63)),
        new(ShopControlAction.Next, new UiBounds(298, 405, 52, 63)),
        new(ShopControlAction.View, new UiBounds(350, 400, 77, 80)),
        new(ShopControlAction.Purchase, new UiBounds(427, 417, 105, 51)),
        new(ShopControlAction.Exit, new UiBounds(533, 417, 82, 51))
    ];
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
