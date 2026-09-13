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
        var activeRetainer = Assert.Single(battle.Retainers);
        Assert.Equal((13, 21, 5, 0),
            (activeRetainer.X, activeRetainer.Y, activeRetainer.VisualId, activeRetainer.OriginalCombatRow));
    }

    [Fact]
    public void OriginalRetainerCommandModesAndShellHitRegionsRemainMapped()
    {
        Assert.Equal(2, (int)SiegeRetainerCommand.Defend);
        Assert.Equal(6, (int)SiegeRetainerCommand.Attack);
        Assert.Equal(10, (int)SiegeRetainerCommand.Retreat);
        Assert.Equal(16, (int)SiegeRetainerCommand.Follow);
        Assert.Equal(
            [
                (SiegeRetainerCommand.Attack, new UiBounds(4, 175, 56, 13)),
                (SiegeRetainerCommand.Defend, new UiBounds(60, 175, 56, 13)),
                (SiegeRetainerCommand.Follow, new UiBounds(116, 175, 56, 13)),
                (SiegeRetainerCommand.Retreat, new UiBounds(172, 175, 38, 13))
            ],
            SiegeCombatPresentation.RetainerCommandButtons.Select(button => (button.Command, button.Bounds)));
        Assert.Equal(SiegeRetainerCommand.Attack, SiegeCombatPresentation.RetainerCommandAt(4, 175));
        Assert.Equal(SiegeRetainerCommand.Defend, SiegeCombatPresentation.RetainerCommandAt(60, 187));
        Assert.Equal(SiegeRetainerCommand.Follow, SiegeCombatPresentation.RetainerCommandAt(171, 180));
        Assert.Equal(SiegeRetainerCommand.Retreat, SiegeCombatPresentation.RetainerCommandAt(209, 187));
        Assert.Null(SiegeCombatPresentation.RetainerCommandAt(210, 187));
        Assert.Null(SiegeCombatPresentation.RetainerCommandAt(4, 188));
    }

    [Fact]
    public void RetainerOrdersTargetSelectionOrFallBackToEveryLivingFriendly()
    {
        var battle = RetainerBattle(3, armyCount: 9, seed: 7);

        battle.ToggleRetainerSelection(1);
        battle.CommandRetainers(SiegeRetainerCommand.Follow);

        Assert.Equal(
            [SiegeRetainerCommand.Attack, SiegeRetainerCommand.Follow, SiegeRetainerCommand.Attack],
            battle.Retainers.Select(retainer => retainer.Command));
        Assert.All(battle.Retainers, retainer => Assert.False(retainer.Selected));

        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        Assert.All(battle.Retainers, retainer => Assert.Equal(SiegeRetainerCommand.Defend, retainer.Command));
    }

    [Fact]
    public void RetainerCapRandomlyPrunesAuthoredActorsWithoutInventingReplacements()
    {
        var battle = RetainerBattle(4, armyCount: 1, seed: 11);

        var retainer = Assert.Single(battle.Retainers);
        Assert.Equal(1, battle.AlliesStarted);
        Assert.Contains(retainer.X, Enumerable.Range(2, 4));
        Assert.Equal(7 + retainer.X, retainer.Health);
        Assert.Equal(20 + retainer.X, retainer.OriginalArmor);
    }

    [Fact]
    public void AuthoredRetainersUseTheSamePerspectiveProjectionAsHostileActors()
    {
        var battle = RetainerBattle(2, armyCount: 9, seed: 3);

        Assert.Equal(2, SiegeViewProjection.ProjectRetainers(battle).Count);
    }

    [Fact]
    public void RetainerCommandIntentsDriveTheirExplicitAuthoredActors()
    {
        var attack = RetainerOrderBattle(retainerX: 2, enemyX: 9);
        attack.CommandRetainers(SiegeRetainerCommand.Attack);
        attack.AdvanceRetainerOrders();
        Assert.Equal(3, Assert.Single(attack.Retainers).X);

        var defend = RetainerOrderBattle(retainerX: 2, enemyX: 9);
        defend.CommandRetainers(SiegeRetainerCommand.Defend);
        defend.AdvanceRetainerOrders();
        Assert.Equal(2, Assert.Single(defend.Retainers).X);

        var follow = RetainerOrderBattle(retainerX: 5, enemyX: 10);
        follow.CommandRetainers(SiegeRetainerCommand.Follow);
        follow.AdvanceRetainerOrders();
        Assert.Equal(4, Assert.Single(follow.Retainers).X);

        var retreat = RetainerOrderBattle(retainerX: 5, enemyX: 9);
        retreat.CommandRetainers(SiegeRetainerCommand.Retreat);
        retreat.AdvanceRetainerOrders();
        Assert.Equal(4, Assert.Single(retreat.Retainers).X);
    }

    [Fact]
    public void DefendingRetainerAttacksOnlyWhenAnOpponentEntersItsNeighborhood()
    {
        var battle = RetainerOrderBattle(retainerX: 2, enemyX: 3);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);

        battle.AdvanceRetainerOrders();

        var retainer = Assert.Single(battle.Retainers);
        Assert.Equal(2, retainer.X);
        Assert.Equal(SiegeEnemyVisualState.Attack, retainer.VisualState);
    }

    [Fact]
    public void DefenderTargetsTheNearestExplicitFriendlyInsteadOfRollingGlobalInterception()
    {
        var tiles = new SiegeTile[10, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(5, 2, false, 1,
            OriginalArmor: 0, OriginalHealth: 1, OriginalCombatRow: 0, OriginalAttackSkill: 0);
        var enemy = new SiegeSpawn(6, 2, false, 2,
            OriginalArmor: 0, OriginalHealth: 10, OriginalCombatRow: 18, OriginalAttackSkill: 1_000);
        var battle = new SiegeSession(new Player(), army, 0, seed: 1086,
            new SiegeLayout(tiles, 1, 2, Facing.West, [enemy], retainers: [retainer]));

        battle.Move(forward: true);

        Assert.Equal(0, Assert.Single(battle.Retainers).Health);
        Assert.Equal(0, battle.AlliesAlive);
        Assert.Equal("A retainer falls in battle.", battle.LastMessage);
    }

    private static SiegeSession RetainerBattle(int authoredCount, int armyCount, int seed)
    {
        var tiles = new SiegeTile[8, 5];
        var retainers = Enumerable.Range(0, authoredCount)
            .Select(index => new SiegeSpawn(2 + index, 2, false, 30 + index,
                OriginalArmor: 22 + index, OriginalHealth: 9 + index,
                OriginalCombatRow: index, OriginalAttackSkill: 40 + index))
            .ToArray();
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = armyCount;
        return new SiegeSession(new Player(), army, 0, seed,
            new SiegeLayout(tiles, 1, 2, Facing.East, [], retainers: retainers));
    }

    private static SiegeSession RetainerOrderBattle(int retainerX, int enemyX)
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(retainerX, 2, false, 1,
            OriginalArmor: 7, OriginalHealth: 12, OriginalCombatRow: 0, OriginalAttackSkill: 50);
        var enemy = new SiegeSpawn(enemyX, 2, false, 2,
            OriginalArmor: 6, OriginalHealth: 10, OriginalCombatRow: 4, OriginalAttackSkill: 50);
        return new SiegeSession(new Player(), army, 0, seed: 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [retainer]));
    }

    private const UnitType SiegeRetainerCombatUnit = UnitType.Swordsmen;
}
