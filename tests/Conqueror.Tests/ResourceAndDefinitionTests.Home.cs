using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void JumpRegionKeepsItsOriginalLabelButHasNoRegisteredAction()
    {
        var jump = HomePresentationDefinitions.Hotspots.Single(hotspot =>
            hotspot.Action == SceneNavigationAction.Jump);

        Assert.Equal("JUMP!!", jump.HoverLabel);
        Assert.Equal(7, jump.HatRegionId);
        Assert.False(jump.Interactive);
        Assert.All(HomePresentationDefinitions.Hotspots.Where(hotspot =>
            hotspot.Action != SceneNavigationAction.Jump), hotspot => Assert.True(hotspot.Interactive));
    }
}
