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
        Assert.Equal((13, 21, 5, 0, 0),
            (retainer.X, retainer.Y, retainer.VisualId, retainer.OriginalCombatRow,
                retainer.OriginalActorKind));
        Assert.Single(layout.Enemies);
        Assert.NotNull(layout.PlayerActor);
        var authoredActors = layout.Enemies.Concat(layout.Retainers)
            .Append(layout.PlayerActor!).OrderBy(actor => actor.X).ThenBy(actor => actor.Y).ToArray();
        Assert.Equal(authoredActors.Select(actor => actor.OriginalActorOrder).Order().ToArray(),
            authoredActors.Select(actor => actor.OriginalActorOrder).ToArray());
        Assert.DoesNotContain(layout.Objects, item => item.X == 13 && item.Y is 20 or 21);
        Assert.Equal(1, battle.AlliesStarted);
        var activeRetainer = Assert.Single(battle.Retainers);
        Assert.Equal((13, 21, 5, 0, 0),
            (activeRetainer.X, activeRetainer.Y, activeRetainer.VisualId, activeRetainer.OriginalCombatRow,
                activeRetainer.OriginalActorKind));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 0)]
    [InlineData(2, 1)]
    [InlineData(3, 2)]
    [InlineData(4, 3)]
    [InlineData(5, 4)]
    [InlineData(6, 5)]
    [InlineData(7, 6)]
    [InlineData(8, 2)]
    [InlineData(9, 7)]
    public void CombatantTemplatesRetainTheirExecutableActorKind(int template, int expectedKind)
    {
        Assert.Equal(expectedKind, OriginalCombatantTemplates.ActorKindForSceneTemplate(template));
    }

    [Theory]
    [InlineData(0, 4, 4, 4)]
    [InlineData(1, 4, 4, 4)]
    [InlineData(2, 4, 4, 4)]
    [InlineData(3, 6, 8, 6)]
    [InlineData(4, 4, 10, 6)]
    [InlineData(5, 4, 8, 1)]
    [InlineData(6, 1, 4, 2)]
    [InlineData(7, 4, 10, 1)]
    [InlineData(8, 6, 8, 4)]
    [InlineData(9, 4, 8, 4)]
    public void CombatantTemplatesRetainTheirExecutableActorModeProfile(
        int template, int current, int requested, int previous)
    {
        Assert.Equal(new OriginalActorModeProfile(current, requested, previous),
            OriginalCombatantTemplates.ModeProfileForSceneTemplate(template));
    }

    [Fact]
    public void SupportedPlacedCombatantsDoNotInitializeInModeNineOrTen()
    {
        var placedTemplates = Enumerable.Range(0, 10)
            .Where(OriginalCombatantTemplates.IsPlacedSceneTemplate)
            .ToArray();

        Assert.Equal([0, 1, 2, 3, 5, 8, 9], placedTemplates);
        Assert.All(placedTemplates, template =>
        {
            var modes = OriginalCombatantTemplates.ModeProfileForSceneTemplate(template);
            Assert.False(modes.Current is 9 or 10);
            Assert.False(modes.Requested is 9 or 10);
            Assert.False(modes.Previous is 9 or 10);
        });
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OriginalCombatantTemplates.ModeProfileForSceneTemplate(10));
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
        Assert.Equal(2, Assert.Single(attack.Retainers).X);
        attack.AdvanceRetainerMovement(0.6001);
        Assert.Equal((3, 2, -64, 0, Facing.East),
            (Assert.Single(attack.Retainers).X, Assert.Single(attack.Retainers).Y,
                Assert.Single(attack.Retainers).OffsetX8, Assert.Single(attack.Retainers).OffsetY8,
                Assert.Single(attack.Retainers).Facing));

        var defend = RetainerOrderBattle(retainerX: 2, enemyX: 9);
        defend.CommandRetainers(SiegeRetainerCommand.Defend);
        defend.AdvanceRetainerOrders();
        Assert.Equal(2, Assert.Single(defend.Retainers).X);

        var follow = RetainerOrderBattle(retainerX: 5, enemyX: 10);
        follow.CommandRetainers(SiegeRetainerCommand.Follow);
        follow.AdvanceRetainerOrders();
        Assert.Equal(5, Assert.Single(follow.Retainers).X);
        follow.AdvanceRetainerMovement(0.6001);
        Assert.Equal((4, 2, 64, 0, Facing.West),
            (Assert.Single(follow.Retainers).X, Assert.Single(follow.Retainers).Y,
                Assert.Single(follow.Retainers).OffsetX8, Assert.Single(follow.Retainers).OffsetY8,
                Assert.Single(follow.Retainers).Facing));

        var retreat = RetainerOrderBattle(retainerX: 5, enemyX: 9);
        Assert.Single(retreat.Retainers).Facing = Facing.South;
        retreat.CommandRetainers(SiegeRetainerCommand.Retreat);
        retreat.AdvanceRetainerOrders();
        Assert.Equal((5, 2), (Assert.Single(retreat.Retainers).X, Assert.Single(retreat.Retainers).Y));
        retreat.AdvanceRetainerMovement(0.6001);
        Assert.Equal((5, 3, 0, -64, Facing.South),
            (Assert.Single(retreat.Retainers).X, Assert.Single(retreat.Retainers).Y,
                Assert.Single(retreat.Retainers).OffsetX8, Assert.Single(retreat.Retainers).OffsetY8,
                Assert.Single(retreat.Retainers).Facing));
    }

    [Fact]
    public void MeleeRetreatUsesTheModeFiveCollisionTurnFamily()
    {
        var tiles = new SiegeTile[12, 5];
        var movementBlocks = new bool[12, 5];
        movementBlocks[5, 3] = true;
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(5, 2, false, OriginalHealth: 12, OriginalCombatRow: 0,
            OriginalActorKind: 0);
        var enemy = new SiegeSpawn(9, 2, false, OriginalHealth: 10, OriginalCombatRow: 4);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [retainer],
                movementBlocks: movementBlocks));
        var friendly = Assert.Single(battle.Retainers);
        friendly.Facing = Facing.South;
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(0.4001);
        Assert.Equal((5, 2, 0, 0, Facing.East),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal((5, 2, 64, 0, Facing.East),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
    }

    [Fact]
    public void ModeFiveRegroupsThroughModeSevenUsingAuthoredFriendlyOrderAndThePlayerActor()
    {
        var tiles = new SiegeTile[16, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 6;
        var playerActor = new SiegeSpawn(1, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 0);
        var ally = new SiegeSpawn(4, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 1);
        var runner = new SiegeSpawn(8, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 2);
        var enemy = new SiegeSpawn(12, 2, false, OriginalHealth: 10,
            OriginalActorKind: 2, OriginalActorOrder: 3);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [ally, runner],
                playerActor: playerActor));
        var activeRunner = battle.Retainers.Single(item => item.OriginalActorOrder == 2);
        activeRunner.Facing = Facing.West;
        var formationCalls = new List<SiegeEnemy>();
        var reached = false;
        battle.ConfigureActorRaycast((source, target) =>
        {
            if (ReferenceEquals(target, battle.Enemies[0]))
                return new SiegeActorRayHit(target, 0x400);
            formationCalls.Add(target);
            if (reached && ReferenceEquals(target, battle.Retainers[0]))
                return new SiegeActorRayHit(battle.PlayerActor, 0x1ff);
            return new SiegeActorRayHit(target,
                ReferenceEquals(target, battle.PlayerActor) ? 0x500 : 0x300);
        });
        battle.ToggleRetainerSelection(battle.Retainers[0]);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        battle.ToggleRetainerSelection(activeRunner);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0.6001);
        Assert.Equal((7, 2, 64), (activeRunner.X, activeRunner.Y, activeRunner.OffsetX8));

        battle.AdvanceRetainerMovement(0);
        Assert.Equal([battle.PlayerActor, battle.Retainers[0]], formationCalls);
        Assert.Equal(Facing.West, activeRunner.Facing);

        reached = true;
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        var regrouped = (activeRunner.X, activeRunner.Y, activeRunner.OffsetX8, activeRunner.OffsetY8);
        Assert.Equal((7, 2, -128, 0), regrouped);
        battle.AdvanceRetainerMovement(1.0);
        Assert.Equal(regrouped,
            (activeRunner.X, activeRunner.Y, activeRunner.OffsetX8, activeRunner.OffsetY8));

        battle.AdvanceRetainerMovement(0);
        Assert.Equal(Facing.West, activeRunner.Facing);
        battle.AdvanceRetainerMovement(0.2001);
        Assert.Equal(Facing.West, activeRunner.Facing);
        Assert.NotEqual(regrouped,
            (activeRunner.X, activeRunner.Y, activeRunner.OffsetX8, activeRunner.OffsetY8));
    }

    [Fact]
    public void ModeOneWithAnAdjacentFriendlyReentersCombatThroughModeFourAndModeEight()
    {
        var tiles = new SiegeTile[16, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 6;
        var playerActor = new SiegeSpawn(1, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 0);
        var ally = new SiegeSpawn(6, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 1);
        var runner = new SiegeSpawn(8, 2, false, OriginalHealth: 12,
            OriginalCombatRow: 0, OriginalAttackSkill: 1_000,
            OriginalActorKind: 0, OriginalActorOrder: 2,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var enemy = new SiegeSpawn(12, 2, false, OriginalArmor: 0, OriginalHealth: 12,
            OriginalAttackSkill: 1, OriginalActorKind: 2, OriginalActorOrder: 3);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [ally, runner],
                playerActor: playerActor));
        var activeRunner = battle.Retainers.Single(item => item.OriginalActorOrder == 2);
        activeRunner.Facing = Facing.West;
        var reached = false;
        var enemyDistance8 = 0x400;
        battle.ConfigureActorRaycast((_, target) =>
        {
            if (ReferenceEquals(target, battle.Enemies[0]))
                return new SiegeActorRayHit(target, enemyDistance8);
            if (reached && ReferenceEquals(target, battle.Retainers[0]))
                return new SiegeActorRayHit(battle.PlayerActor, 0x1ff);
            return new SiegeActorRayHit(target,
                ReferenceEquals(target, battle.PlayerActor) ? 0x500 : 0x300);
        });
        battle.ToggleRetainerSelection(battle.Retainers[0]);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        battle.ToggleRetainerSelection(activeRunner);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        reached = true;
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(Facing.East, activeRunner.Facing);
        var beforeModeEightTick = activeRunner.OffsetX8;
        battle.AdvanceRetainerMovement(0.2001);
        Assert.True(activeRunner.OffsetX8 > beforeModeEightTick);

        enemyDistance8 = 349;
        for (var tick = 0; tick < 6 && activeRunner.VisualState == SiegeEnemyVisualState.Walk; tick++)
            battle.AdvanceRetainerMovement(0.2001);

        Assert.Equal(SiegeEnemyVisualState.Attack, activeRunner.VisualState);
        Assert.Equal(12, Assert.Single(battle.Enemies).Health);
        battle.AdvanceEnemyAnimations(0.3841);
        Assert.True(Assert.Single(battle.Enemies).Health < 12);
    }

    [Fact]
    public void ModeEightRejectsContactAtTheRawCombatRowBoundaryAndFallsBackToWandering()
    {
        var tiles = new SiegeTile[16, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 6;
        var playerActor = new SiegeSpawn(1, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 0);
        var ally = new SiegeSpawn(6, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 1);
        var runner = new SiegeSpawn(8, 2, false, OriginalHealth: 12,
            OriginalCombatRow: 0, OriginalAttackSkill: 1_000,
            OriginalActorKind: 0, OriginalActorOrder: 2,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var enemy = new SiegeSpawn(12, 2, false, OriginalArmor: 0, OriginalHealth: 10,
            OriginalAttackSkill: 1, OriginalActorKind: 2, OriginalActorOrder: 3);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [ally, runner],
                playerActor: playerActor));
        var activeRunner = battle.Retainers.Single(item => item.OriginalActorOrder == 2);
        activeRunner.Facing = Facing.West;
        var reached = false;
        var enemyDistance8 = 0x400;
        battle.ConfigureActorRaycast((_, target) =>
        {
            if (ReferenceEquals(target, battle.Enemies[0]))
                return new SiegeActorRayHit(target, enemyDistance8);
            if (reached && ReferenceEquals(target, battle.Retainers[0]))
                return new SiegeActorRayHit(battle.PlayerActor, 0x1ff);
            return new SiegeActorRayHit(target,
                ReferenceEquals(target, battle.PlayerActor) ? 0x500 : 0x300);
        });
        battle.ToggleRetainerSelection(battle.Retainers[0]);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        battle.ToggleRetainerSelection(activeRunner);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        reached = true;
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0.6001);
        enemyDistance8 = 350;
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(SiegeEnemyVisualState.Walk, activeRunner.VisualState);
        Assert.Equal(10, Assert.Single(battle.Enemies).Health);
        var beforeWander = (activeRunner.X, activeRunner.Y, activeRunner.OffsetX8, activeRunner.OffsetY8);
        battle.AdvanceRetainerMovement(0.2001);
        Assert.NotEqual(beforeWander,
            (activeRunner.X, activeRunner.Y, activeRunner.OffsetX8, activeRunner.OffsetY8));
    }

    [Theory]
    [InlineData(5, false)]
    [InlineData(6, true)]
    public void StrongModeElevenTargetCancelsTheStrikeThenModeThirteenTestsHealthSix(
        int retainerHealth, bool defends)
    {
        var tiles = new SiegeTile[16, 6];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 6;
        var playerActor = new SiegeSpawn(1, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 0);
        var ally = new SiegeSpawn(6, 2, false, OriginalHealth: 12,
            OriginalActorKind: 0, OriginalActorOrder: 1);
        var runner = new SiegeSpawn(8, 2, false, OriginalHealth: retainerHealth,
            OriginalCombatRow: 0, OriginalAttackSkill: 1_000,
            OriginalActorKind: 0, OriginalActorOrder: 2,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var enemy = new SiegeSpawn(12, 3, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalAttackSkill: 1, OriginalActorKind: 2, OriginalActorOrder: 3);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [ally, runner],
                playerActor: playerActor));
        var activeRunner = battle.Retainers.Single(item => item.OriginalActorOrder == 2);
        activeRunner.Facing = Facing.West;
        var reached = false;
        var enemyDistance8 = 0x400;
        battle.ConfigureActorRaycast((_, target) =>
        {
            if (ReferenceEquals(target, battle.Enemies[0]))
                return new SiegeActorRayHit(target, enemyDistance8);
            if (reached && ReferenceEquals(target, battle.Retainers[0]))
                return new SiegeActorRayHit(battle.PlayerActor, 0x1ff);
            return new SiegeActorRayHit(target,
                ReferenceEquals(target, battle.PlayerActor) ? 0x500 : 0x300);
        });
        battle.ToggleRetainerSelection(battle.Retainers[0]);
        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        battle.ToggleRetainerSelection(activeRunner);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        reached = true;
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0);
        battle.AdvanceRetainerMovement(0);
        enemyDistance8 = 0x153;
        battle.AdvanceRetainerMovement(0.6001);
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(SiegeEnemyVisualState.Walk, activeRunner.VisualState);
        Assert.Equal(20, Assert.Single(battle.Enemies).Health);
        battle.AdvanceRetainerMovement(0);

        Assert.Equal(defends ? SiegeRetainerCommand.Defend : SiegeRetainerCommand.Retreat,
            activeRunner.Command);
        if (defends)
        {
            Assert.Null(activeRunner.OriginalHeading8);
            return;
        }

        var heading = Assert.IsType<int>(activeRunner.OriginalHeading8);
        var beforeX8 = (activeRunner.X << 8) + activeRunner.OffsetX8;
        var beforeY8 = (activeRunner.Y << 8) + activeRunner.OffsetY8;
        battle.AdvanceRetainerMovement(0.2001);
        var actualDelta = (
            (activeRunner.X << 8) + activeRunner.OffsetX8 - beforeX8,
            (activeRunner.Y << 8) + activeRunner.OffsetY8 - beforeY8);
        Assert.Equal(OriginalActorMotion.Rotate(96, 0, heading), actualDelta);
        Assert.NotEqual(0, heading & 0x3f);
    }

    [Fact]
    public void ModeTenEscapeUsesExecutableThreeHalvesAndNonCardinalRounding()
    {
        Assert.Equal(224, OriginalActorMotion.HeadingToward(-256, -256));
        Assert.Equal(96, OriginalActorMotion.ScaleEscapeDelta(64));
        Assert.Equal((-67, -69), OriginalActorMotion.Rotate(96, 0, 224));
    }

    [Fact]
    public void BowmanRetreatDoesNotBorrowTheMeleeModeFiveMovementPath()
    {
        var battle = RetainerOrderBattle(retainerX: 5, enemyX: 9, actorKind: 1, combatRow: 23);
        var friendly = Assert.Single(battle.Retainers);
        friendly.Facing = Facing.South;
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(1.0);

        Assert.Equal((5, 2, 0, 0, Facing.South),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
    }

    [Fact]
    public void BowmanRetreatContinuesThroughModeFourIntoRangedModeEleven()
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(2, 2, false, OriginalArmor: 7, OriginalHealth: 12,
            OriginalCombatRow: 23, OriginalAttackSkill: 1_000, OriginalActorKind: 1,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var enemy = new SiegeSpawn(9, 2, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalCombatRow: 4, OriginalAttackSkill: 1);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [enemy], retainers: [retainer]));
        var friendly = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerOrders();

        Assert.Equal(SiegeEnemyVisualState.Attack, friendly.VisualState);
        Assert.Equal(Facing.East, friendly.Facing);
        Assert.Equal(20, Assert.Single(battle.Enemies).Health);

        battle.AdvanceEnemyAnimations(0.768);
        Assert.Equal(20, Assert.Single(battle.Enemies).Health);
        battle.AdvanceEnemyAnimations(0.0001);

        Assert.True(Assert.Single(battle.Enemies).Health < 20);
        Assert.Equal(SiegeEnemyVisualState.Attack, friendly.VisualState);
    }

    [Fact]
    public void ModeElevenCanStrikeTheInterveningOpponentReturnedByItsSecondRay()
    {
        var tiles = new SiegeTile[14, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(2, 2, false, OriginalHealth: 12,
            OriginalCombatRow: 23, OriginalAttackSkill: 1_000, OriginalActorKind: 1,
            OriginalAnimation: new SiegeActorAnimation(0.384, 0.384, 0.384));
        var aimed = new SiegeSpawn(10, 2, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalCombatRow: 4, OriginalAttackSkill: 1, OriginalActorOrder: 1);
        var intervening = new SiegeSpawn(6, 2, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalCombatRow: 4, OriginalAttackSkill: 1, OriginalActorOrder: 2);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [aimed, intervening], retainers: [retainer]));
        var aimedActor = battle.Enemies.Single(actor => actor.OriginalActorOrder == 1);
        var interveningActor = battle.Enemies.Single(actor => actor.OriginalActorOrder == 2);
        var aimedRays = 0;
        battle.ConfigureActorRaycast((_, target) =>
        {
            if (ReferenceEquals(target, aimedActor) && aimedRays++ == 0)
                return new SiegeActorRayHit(aimedActor, 0x153);
            return new SiegeActorRayHit(interveningActor, 0x180);
        });
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerOrders();
        battle.AdvanceEnemyAnimations(0.7681);

        Assert.Equal(20, aimedActor.Health);
        Assert.True(interveningActor.Health < 20);
    }

    [Fact]
    public void ModeElevenDamageRepeatsOnlyAfterEachStrictDoubledEffectGate()
    {
        var battle = RetainerOrderBattle(retainerX: 2, enemyX: 9,
            actorKind: 1, combatRow: 23, retainerAttackSkill: 1_000, enemyHealth: 200);
        var friendly = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);
        battle.AdvanceRetainerOrders();

        battle.AdvanceEnemyAnimations(0.7681);
        var afterFirst = Assert.Single(battle.Enemies).Health;
        Assert.True(afterFirst < 200);
        Assert.Equal(SiegeEnemyVisualState.Attack, friendly.VisualState);

        battle.AdvanceEnemyAnimations(0.768);
        Assert.Equal(afterFirst, Assert.Single(battle.Enemies).Health);
        battle.AdvanceEnemyAnimations(0.0001);

        Assert.True(Assert.Single(battle.Enemies).Health < afterFirst);
        Assert.Equal(SiegeEnemyVisualState.Attack, friendly.VisualState);
    }

    [Fact]
    public void BowmanAttackAlsoUsesTheModeElevenCompletionGate()
    {
        var battle = RetainerOrderBattle(retainerX: 2, enemyX: 9,
            actorKind: 1, combatRow: 23, retainerAttackSkill: 1_000, enemyHealth: 200);
        battle.CommandRetainers(SiegeRetainerCommand.Attack);

        battle.AdvanceRetainerOrders();
        Assert.Equal(200, Assert.Single(battle.Enemies).Health);
        battle.AdvanceEnemyAnimations(0.7681);

        Assert.True(Assert.Single(battle.Enemies).Health < 200);
    }

    [Fact]
    public void ModeElevenCompletionIsIndependentOfAnimationUpdateSubdivision()
    {
        var whole = RetainerOrderBattle(retainerX: 2, enemyX: 9,
            actorKind: 1, combatRow: 23, retainerAttackSkill: 1_000, enemyHealth: 200);
        var divided = RetainerOrderBattle(retainerX: 2, enemyX: 9,
            actorKind: 1, combatRow: 23, retainerAttackSkill: 1_000, enemyHealth: 200);
        whole.CommandRetainers(SiegeRetainerCommand.Retreat);
        divided.CommandRetainers(SiegeRetainerCommand.Retreat);
        whole.AdvanceRetainerOrders();
        divided.AdvanceRetainerOrders();

        whole.AdvanceEnemyAnimations(0.7681);
        divided.AdvanceEnemyAnimations(0.384);
        divided.AdvanceEnemyAnimations(0.3841);

        Assert.Equal(Assert.Single(whole.Enemies).Health, Assert.Single(divided.Enemies).Health);
        Assert.Equal(Assert.Single(whole.Retainers).VisualState,
            Assert.Single(divided.Retainers).VisualState);
    }

    [Fact]
    public void RecommandingABowmanCancelsThePendingModeElevenDamage()
    {
        var battle = RetainerOrderBattle(retainerX: 2, enemyX: 9,
            actorKind: 1, combatRow: 23, retainerAttackSkill: 1_000, enemyHealth: 200);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);
        battle.AdvanceRetainerOrders();

        battle.CommandRetainers(SiegeRetainerCommand.Defend);
        battle.AdvanceEnemyAnimations(0.7681);

        Assert.Equal(200, Assert.Single(battle.Enemies).Health);
    }

    [Fact]
    public void BowmanRetreatFallsBackToDefendWhenTheAcquisitionRayIsBlocked()
    {
        var tiles = new SiegeTile[12, 5];
        var movementBlocks = new bool[12, 5];
        movementBlocks[5, 2] = true;
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(2, 2, false, OriginalHealth: 12,
            OriginalCombatRow: 23, OriginalAttackSkill: 1_000, OriginalActorKind: 1);
        var enemy = new SiegeSpawn(9, 2, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalCombatRow: 4, OriginalAttackSkill: 1);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [enemy], retainers: [retainer],
                movementBlocks: movementBlocks));
        var friendly = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerOrders();

        Assert.Equal(SiegeRetainerCommand.Defend, friendly.Command);
        Assert.Equal(SiegeEnemyVisualState.Walk, friendly.VisualState);
        Assert.Equal(20, Assert.Single(battle.Enemies).Health);
    }

    [Fact]
    public void BowmanModeElevenUsesEuclideanFixedPointRangeNotManhattanGridRange()
    {
        var tiles = new SiegeTile[32, 32];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(2, 2, false, OriginalHealth: 12,
            OriginalCombatRow: 23, OriginalAttackSkill: 1_000, OriginalActorKind: 1);
        var enemy = new SiegeSpawn(21, 21, false, OriginalArmor: 0, OriginalHealth: 20,
            OriginalCombatRow: 4, OriginalAttackSkill: 1);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [enemy], retainers: [retainer]));
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerOrders();
        battle.AdvanceEnemyAnimations(0.7681);

        Assert.True(Assert.Single(battle.Enemies).Health < 20);
    }

    [Fact]
    public void AttackWithoutAVisibleTargetUsesModeSixWanderingMovement()
    {
        var tiles = new SiegeTile[12, 5];
        var movementBlocks = new bool[12, 5];
        movementBlocks[7, 2] = true;
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(5, 2, false, OriginalHealth: 12,
            OriginalCombatRow: 0, OriginalAttackSkill: 50, OriginalActorKind: 0);
        var enemy = new SiegeSpawn(9, 2, false, OriginalArmor: 6, OriginalHealth: 10,
            OriginalCombatRow: 4, OriginalAttackSkill: 50);
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East, [enemy], retainers: [retainer],
                movementBlocks: movementBlocks));
        var friendly = Assert.Single(battle.Retainers);
        friendly.Facing = Facing.South;
        battle.CommandRetainers(SiegeRetainerCommand.Attack);

        battle.AdvanceRetainerOrders();
        battle.AdvanceRetainerMovement(0.6001);

        Assert.Equal((5, 3, 0, -64, Facing.South),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
        Assert.Equal(SiegeEnemyVisualState.Walk, friendly.VisualState);
        Assert.Equal(10, Assert.Single(battle.Enemies).Health);
    }

    [Fact]
    public void AcquisitionKeepsTheFirstAuthoredActorAtAnEqualRayDepth()
    {
        var battle = AcquisitionOrderBattle();
        var calls = new List<SiegeEnemy>();
        battle.ConfigureActorRaycast((_, target) =>
        {
            calls.Add(target);
            return new SiegeActorRayHit(target, 0x200);
        });
        battle.CommandRetainers(SiegeRetainerCommand.Attack);

        battle.AdvanceRetainerMovement(0);

        Assert.Equal(battle.Enemies, calls);
        Assert.Equal(Facing.East, Assert.Single(battle.Retainers).Facing);
    }

    [Fact]
    public void AcquisitionStopsAtTheFirstAuthoredActorInsideTheCloseThreshold()
    {
        var battle = AcquisitionOrderBattle();
        var calls = new List<SiegeEnemy>();
        battle.ConfigureActorRaycast((_, target) =>
        {
            calls.Add(target);
            return new SiegeActorRayHit(target,
                ReferenceEquals(target, battle.Enemies[0]) ? 0x153 : 0x100);
        });
        battle.CommandRetainers(SiegeRetainerCommand.Attack);

        battle.AdvanceRetainerMovement(0);

        Assert.Equal([battle.Enemies[0]], calls);
        Assert.Equal(Facing.East, Assert.Single(battle.Retainers).Facing);
    }

    [Fact]
    public void ModeSixWanderingKeepsItsFlagFortyCollisionFamilyForTheWholeEffect()
    {
        var tiles = new SiegeTile[12, 5];
        var movementBlocks = new bool[12, 5];
        movementBlocks[7, 2] = true;
        movementBlocks[5, 3] = true;
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 1, Facing.East,
                [new SiegeSpawn(9, 2, false, OriginalHealth: 10)], retainers:
                [new SiegeSpawn(5, 2, false, OriginalHealth: 12, OriginalActorKind: 0)],
                movementBlocks: movementBlocks));
        var friendly = Assert.Single(battle.Retainers);
        friendly.Facing = Facing.South;
        battle.CommandRetainers(SiegeRetainerCommand.Attack);

        battle.AdvanceRetainerMovement(0.4001);

        Assert.Equal((5, 2, 0, 0, Facing.East),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8, friendly.Facing));
    }

    [Fact]
    public void RetreatWithoutAnOpponentFallsBackToDefend()
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var battle = new SiegeSession(new Player(), army, 0, 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [], retainers:
            [new SiegeSpawn(5, 2, false, OriginalHealth: 12, OriginalActorKind: 0)]));
        var friendly = Assert.Single(battle.Retainers);
        battle.CommandRetainers(SiegeRetainerCommand.Retreat);

        battle.AdvanceRetainerMovement(0.2001);

        Assert.Equal(SiegeRetainerCommand.Defend, friendly.Command);
        Assert.Equal((5, 2, 0, 0),
            (friendly.X, friendly.Y, friendly.OffsetX8, friendly.OffsetY8));
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

    private static SiegeSession RetainerOrderBattle(
        int retainerX, int enemyX, int? actorKind = null, int combatRow = 0,
        int retainerAttackSkill = 50, int enemyHealth = 10)
    {
        var tiles = new SiegeTile[12, 5];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        var retainer = new SiegeSpawn(retainerX, 2, false, 1,
            OriginalArmor: 7, OriginalHealth: 12, OriginalCombatRow: combatRow,
            OriginalAttackSkill: retainerAttackSkill, OriginalActorKind: actorKind,
            OriginalAnimation: combatRow >= 23
                ? new SiegeActorAnimation(0.384, 0.384, 0.384)
                : null);
        var enemy = new SiegeSpawn(enemyX, 2, false, 2,
            OriginalArmor: 6, OriginalHealth: enemyHealth, OriginalCombatRow: 4,
            OriginalAttackSkill: 50);
        return new SiegeSession(new Player(), army, 0, seed: 1086,
            new SiegeLayout(tiles, 1, 2, Facing.East, [enemy], retainers: [retainer]));
    }

    private static SiegeSession AcquisitionOrderBattle()
    {
        var tiles = new SiegeTile[8, 8];
        var army = new Army();
        army.Units[SiegeRetainerCombatUnit] = 1;
        return new SiegeSession(new Player(), army, 0, seed: 1086,
            new SiegeLayout(tiles, 0, 0, Facing.East,
                [new SiegeSpawn(5, 2, false, OriginalHealth: 10),
                    new SiegeSpawn(2, 5, false, OriginalHealth: 10)], retainers:
                [new SiegeSpawn(2, 2, false, OriginalHealth: 12, OriginalActorKind: 0)]));
    }

    private const UnitType SiegeRetainerCombatUnit = UnitType.Swordsmen;
}
