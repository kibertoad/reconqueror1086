using Conqueror.Resources;

namespace Conqueror.Game;

public readonly record struct SiegeActorColorSelection(int ColorMapBase, int WalkTextureBase);

public static class SiegeActorColorMapping
{
    private const int ColorMapStride = 32;

    public static SiegeActorColorSelection Normalize(
        string? heraldicColor, bool friendly, int authoredColorMapBase, int authoredWalkTextureBase)
    {
        var playerColor = PlayerColorIndex(heraldicColor);
        if (friendly) return SelectionFor(playerColor);

        var playerMapBase = playerColor * ColorMapStride;
        if (authoredColorMapBase != playerMapBase)
            return new SiegeActorColorSelection(authoredColorMapBase, authoredWalkTextureBase);

        // RULE-ASSAULT-025: a hostile in the player's colour changes to blue,
        // or to green when the player is blue.
        return SelectionFor(playerColor == 2 ? 1 : 2);
    }

    public static int DistanceMapIndex(
        SiegeActorColorSelection selection, double distance,
        DynamixSceneColorMapping parameters, int blockOffset)
    {
        var distanceMap = SiegeColorMapping.WallDistanceMap(distance, parameters, blockOffset);
        var result = checked(selection.ColorMapBase + distanceMap);
        if (result is < 0 or >= DynamixSceneColorMaps.Count)
            throw new ArgumentOutOfRangeException(nameof(selection));
        return result;
    }

    private static int PlayerColorIndex(string? color) => color?.ToUpperInvariant() switch
    {
        "GREEN" => 1,
        "BLUE" => 2,
        _ => 0
    };

    private static SiegeActorColorSelection SelectionFor(int color) => color switch
    {
        1 => new SiegeActorColorSelection(32, 128),
        2 => new SiegeActorColorSelection(64, 96),
        _ => new SiegeActorColorSelection(0, 64)
    };
}
