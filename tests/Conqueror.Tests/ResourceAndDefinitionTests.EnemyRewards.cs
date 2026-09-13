using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void DefeatedEnemiesDoNotCreatePickupRewards()
    {
        var player = new Player { Wealth = 73 };
        player.Inventory.Weapon = "Heavy Crossbow";
        player.Inventory.CrossbowBolts = 1;
        var ownedBefore = player.Inventory.Items.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var tiles = new SiegeTile[5, 3];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = x == 0 || y == 0 || x == 4 || y == 2 ? SiegeTile.Wall : SiegeTile.Floor;
        var siege = new SiegeSession(player, new Army(), 0, 3,
            new SiegeLayout(tiles, 1, 1, Facing.East,
                [new SiegeSpawn(2, 1, false, OriginalAnimation: new SiegeActorAnimation(0.1, 0.1, 0.1))]));

        Assert.Equal(SiegeAction.Shot, siege.Shoot());
        siege.AdvanceEnemyAnimations(0.2);

        Assert.True(siege.Won);
        Assert.Equal(73, player.Wealth);
        Assert.Equal(0, siege.GoldFound);
        Assert.Equal(0, player.Inventory.CrossbowBolts);
        Assert.Empty(siege.ItemsFound);
        Assert.True(ownedBefore.SetEquals(player.Inventory.Items));
    }
}
