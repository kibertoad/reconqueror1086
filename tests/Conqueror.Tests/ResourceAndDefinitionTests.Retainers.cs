using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(6, 2)]
    [InlineData(9, 3)]
    [InlineData(99, 3)]
    public void CampaignRetainerCapUsesOriginalPerUnitTypeScaling(int soldiers, int expected)
    {
        var player = Campaign.NewFromTemplate(2).Player;
        foreach (var type in Enum.GetValues<UnitType>()) player.Army.Units[type] = 0;
        player.Army.Units[UnitType.Halberdiers] = soldiers;
        var battle = new SiegeSession(player, player.Army, 0, seed: 1);

        Assert.Equal(expected, OriginalRetainerCombat.CampaignRetainerCapFor(player.Army));
        Assert.Equal(expected, battle.AlliesStarted);
    }

    [Fact]
    public void CampaignRetainerCapAddsTheThreeIndependentlyCappedUnitPools()
    {
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 9;
        army.Units[UnitType.Halberdiers] = 30;
        army.Units[UnitType.Knights] = 300;

        Assert.Equal(9, OriginalRetainerCombat.CampaignRetainerCapFor(army));
    }

    [Fact]
    public void CampaignRetainerDeathsRemoveRepresentedUnitsInOriginalOrder()
    {
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 9;
        army.Units[UnitType.Halberdiers] = 6;
        army.Units[UnitType.Knights] = 3;

        OriginalRetainerCombat.ApplyCampaignLosses(army, 4);

        Assert.Equal(6, army.Units[UnitType.Swordsmen]);
        Assert.Equal(5, army.Units[UnitType.Halberdiers]);
        Assert.Equal(3, army.Units[UnitType.Knights]);
    }

    [Theory]
    [InlineData(UnitType.Swordsmen)]
    [InlineData(UnitType.Halberdiers)]
    [InlineData(UnitType.Knights)]
    public void DeathOfTheFallbackRetainerRemovesTheOnlySoldier(UnitType type)
    {
        var army = new Army();
        army.Units[type] = 1;

        OriginalRetainerCombat.ApplyCampaignLosses(army, 1);

        Assert.Equal(0, army.Total);
    }

    [Fact]
    public void ExplicitDuelStillSuppressesRetainers()
    {
        var player = Campaign.NewFromTemplate(2).Player;
        var battle = new SiegeSession(player, player.Army, 0, seed: 1, layout: null, includeRetainers: false);

        Assert.Equal(0, battle.AlliesStarted);
    }

    [Fact]
    public void ImportedSceneUsesFirstFriendlyAsPlayerAndCapsRetainersToAuthoredPlacements()
    {
        var source = SyntheticScene();
        WriteInteraction(source.Blocks, 5, 1, 0, 0);
        SetSceneCell(source.Map, 13, 21, 5);
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);

        var layout = ImportedSiegeLayouts.Convert(scene);
        var army = new Army();
        army.Units[UnitType.Swordsmen] = 99;
        army.Units[UnitType.Halberdiers] = 99;
        army.Units[UnitType.Knights] = 99;
        var battle = new SiegeSession(new Player(), army, 0, 1, layout);

        var retainer = Assert.Single(layout.Retainers);
        Assert.Equal((13, 21, 5, 0),
            (retainer.X, retainer.Y, retainer.VisualId, retainer.OriginalCombatRow));
        Assert.Single(layout.Enemies);
        Assert.DoesNotContain(layout.Objects, item => item.X == 13 && item.Y is 20 or 21);
        Assert.Equal(1, battle.AlliesStarted);
    }
}
