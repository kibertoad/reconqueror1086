using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData("Red", 0, 64)]
    [InlineData("Green", 32, 128)]
    [InlineData("Blue", 64, 96)]
    [InlineData("Unknown", 0, 64)]
    public void FriendlyActorsUseThePlayersExecutableColorTuple(
        string playerColor, int expectedMapBase, int expectedTextureBase)
    {
        var colors = SiegeActorColorMapping.Normalize(playerColor, friendly: true,
            authoredColorMapBase: 96, authoredWalkTextureBase: 211);

        Assert.Equal((expectedMapBase, expectedTextureBase),
            (colors.ColorMapBase, colors.WalkTextureBase));
    }

    [Theory]
    [InlineData("Red", 0, 175, 64, 96)]
    [InlineData("Green", 32, 175, 64, 96)]
    [InlineData("Blue", 64, 175, 32, 128)]
    [InlineData("Red", 32, 175, 32, 175)]
    [InlineData("Green", 96, 211, 96, 211)]
    public void HostilesChangeColorOnlyWhenTheirAuthoredSelectorMatchesThePlayer(
        string playerColor, int authoredMapBase, int authoredTextureBase,
        int expectedMapBase, int expectedTextureBase)
    {
        var colors = SiegeActorColorMapping.Normalize(playerColor, friendly: false,
            authoredMapBase, authoredTextureBase);

        Assert.Equal((expectedMapBase, expectedTextureBase),
            (colors.ColorMapBase, colors.WalkTextureBase));
    }

    [Fact]
    public void ActorColorGroupsRetainTheThirtyTwoDistanceMaps()
    {
        var parameters = new DynamixSceneColorMapping(true, 32, 10, 20);
        var colors = new SiegeActorColorSelection(64, 96);

        Assert.Equal(64, SiegeActorColorMapping.DistanceMapIndex(colors, 0.9, parameters, 0));
        Assert.Equal(79, SiegeActorColorMapping.DistanceMapIndex(colors, 15.9, parameters, 0));
        Assert.Equal(95, SiegeActorColorMapping.DistanceMapIndex(colors, 10_000, parameters, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => SiegeActorColorMapping.DistanceMapIndex(
            new SiegeActorColorSelection(128, 0), 1, parameters, 0));
    }
}
