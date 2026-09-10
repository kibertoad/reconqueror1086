using Conqueror.Core;
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
