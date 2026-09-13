using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void ImportedPickupSelectorsApplyTheirExactResourceRewards()
    {
        var player = new Player { Wealth = 100 };
        var coins = PickupLayout("bag of coins", 5, 25, 0);
        var coinSession = new SiegeSession(player, new Army(), 0, 1, coins);
        coinSession.TurnLeft(); coinSession.TurnLeft();
        Assert.Equal(SiegeAction.Looted, coinSession.Interact());
        Assert.Equal((125, 25), (player.Wealth, coinSession.GoldFound));

        var bolts = PickupLayout("pile of bolts", 10, 12, 0);
        var boltSession = new SiegeSession(player, new Army(), 0, 1, bolts);
        boltSession.TurnLeft(); boltSession.TurnLeft();
        Assert.Equal(SiegeAction.Looted, boltSession.Interact());
        Assert.Equal(12, player.Inventory.CrossbowBolts);

        var armor = PickupLayout("chain hauberk", 9, 6, 0);
        var armorSession = new SiegeSession(player, new Army(), 0, 1, armor);
        armorSession.TurnLeft(); armorSession.TurnLeft();
        Assert.Equal(SiegeAction.Looted, armorSession.Interact());
        Assert.Contains("Chain Hauberk", player.Inventory.Items);
        Assert.Contains("Chain Hauberk", armorSession.ItemsFound);

        var meal = PickupLayout("meal", 7, 2, 6);
        var mealReward = Assert.Single(meal.Objects, item => (item.X, item.Y) == (9, 20)).Stages[0].Pickup;
        Assert.Equal((SiegePickupRewardKind.Healing, 2, 6),
            (mealReward!.Kind, mealReward.Amount, mealReward.DieSides));
    }

    private static SiegeLayout PickupLayout(string name, short selector, short argument, short argument2)
    {
        var source = SyntheticScene();
        SetSceneBlockName(source.Blocks, 4, name);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4, 4);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 4, 19);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 4 + 64, 6);
        WriteInteraction(source.Blocks, 4, selector, argument, argument2);
        SetSceneBlockName(source.Blocks, 6, "ground");
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 6, 0);
        WriteInt(source.Blocks, DynamixSceneDecoder.BlockSize * 6 + 4, 1);
        return ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));
    }
}
