using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed class DrogoEncounterTests
{
    [Fact]
    public void HarvestDebtWaitsForThePlayersPayOrFightDecision()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        Assert.True(campaign.Borrow(200));
        campaign.State.Date = new DateTime(1086, 7, 1);
        campaign.SettleMonth();

        Assert.True(campaign.State.PendingDrogoEncounter);
        Assert.Equal(300, campaign.State.Player.Debt);
        var wealth = campaign.State.Player.Wealth;
        campaign.State.Player.Wealth = 299;
        Assert.False(campaign.PayDrogo());
        Assert.True(campaign.State.PendingDrogoEncounter);
        campaign.State.Player.Wealth = Math.Max(wealth, 300);
        Assert.True(campaign.PayDrogo());
        Assert.Equal(0, campaign.State.Player.Debt);
        Assert.False(campaign.State.PendingDrogoEncounter);
    }

    [Fact]
    public void KillingDrogoEndsTheDebtAndClosesTheMoneylender()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2), seed: 7);
        campaign.State.Player.Debt = 300;
        campaign.State.PendingDrogoEncounter = true;
        var battle = campaign.CreateDrogoBattle();

        Assert.Equal(0, battle.AlliesStarted);
        Assert.Single(battle.Enemies);
        for (var attempt = 0; attempt < 100 && !battle.Won && !battle.Defeated; attempt++)
        {
            battle.Attack();
            battle.AdvanceEnemyAnimations(1);
        }

        Assert.True(battle.Won);
        Assert.True(campaign.FinishDrogoBattle(battle));
        Assert.True(campaign.State.DrogoDefeated);
        Assert.Equal(0, campaign.State.Player.Debt);
        Assert.False(campaign.Borrow(1));
    }

    [Fact]
    public void ImportedDrogoBattleUsesTheSuppliedSceneWithoutRetainers()
    {
        Assert.Equal("MONEY.RES", ImportedSiegeLayouts.DrogoSceneName);
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        campaign.State.Player.Debt = 300;
        campaign.State.PendingDrogoEncounter = true;
        var tiles = new SiegeTile[7, 3];
        for (var x = 0; x < 7; x++)
        for (var y = 0; y < 3; y++)
            tiles[x, y] = SiegeTile.Floor;
        var layout = new SiegeLayout(tiles, 1, 1, Facing.North,
            [
                new SiegeSpawn(3, 1, Champion: false, OriginalHealth: 20, OriginalCombatRow: 16, OriginalActorTemplate: 9),
                new SiegeSpawn(4, 1, Champion: false, OriginalHealth: 15, OriginalCombatRow: 17, OriginalActorTemplate: 8),
                new SiegeSpawn(5, 1, Champion: false, OriginalHealth: 10, OriginalCombatRow: 17, OriginalActorTemplate: 3),
                new SiegeSpawn(6, 1, Champion: false, OriginalHealth: 10, OriginalCombatRow: 17, OriginalActorTemplate: 3)
            ], retainers: [new SiegeSpawn(2, 1, Champion: false)],
            playerActor: new SiegeSpawn(1, 1, Champion: false, OriginalActorTemplate: 0));

        var battle = campaign.CreateDrogoBattle(layout);

        Assert.Equal((1, 1, Facing.North), (battle.PlayerX, battle.PlayerY, battle.Facing));
        Assert.Equal(4, battle.Enemies.Count);
        Assert.Equal([9, 8, 3, 3], battle.Enemies.Select(enemy => enemy.OriginalActorTemplate!.Value));
        Assert.Equal(55, battle.Enemies.Sum(enemy => enemy.Health));
        Assert.Equal(0, battle.AlliesStarted);
        Assert.Empty(battle.Retainers);
    }

    [Fact]
    public void LosingToDrogoRecordsADistinctFatalEnding()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        campaign.State.Player.Debt = 300;
        campaign.State.PendingDrogoEncounter = true;
        var battle = campaign.CreateDrogoBattle();

        for (var turn = 0; turn < 100 && !battle.Defeated; turn++)
        {
            battle.Move(true);
            battle.AdvanceEnemyAnimations(1);
        }

        Assert.True(battle.Defeated);
        Assert.False(campaign.FinishDrogoBattle(battle));
        Assert.Equal(VictoryKind.Defeat, campaign.State.Victory);
        Assert.Equal(CampaignEndReason.Drogo, campaign.State.EndReason);
    }
}
