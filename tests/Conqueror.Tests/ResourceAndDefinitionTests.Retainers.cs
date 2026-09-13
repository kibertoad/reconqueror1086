using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(99, 2)]
    [InlineData(100, 2)]
    [InlineData(149, 2)]
    [InlineData(150, 3)]
    [InlineData(499, 9)]
    [InlineData(500, 10)]
    [InlineData(6_000, 10)]
    public void CampaignRetainerCapUsesOriginalArmyScaling(int soldiers, int expected)
    {
        Assert.Equal(expected, OriginalRetainerCombat.RetainerCapFor(soldiers));

        var player = Campaign.NewFromTemplate(2).Player;
        player.Army.Units[UnitType.Swordsmen] = soldiers;
        var battle = new SiegeSession(player, player.Army, 0, seed: 1);

        Assert.Equal(expected, battle.AlliesStarted);
    }

    [Fact]
    public void ExplicitDuelStillSuppressesRetainers()
    {
        var player = Campaign.NewFromTemplate(2).Player;
        var battle = new SiegeSession(player, player.Army, 0, seed: 1, layout: null, includeRetainers: false);

        Assert.Equal(0, battle.AlliesStarted);
    }
}
