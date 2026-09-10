using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed class DragonBattleTests
{
    [Fact]
    public void ChallengeRequiresTheDragonVictoryEquipmentAndLocation()
    {
        var campaign = ReadyCampaign();
        campaign.State.CurrentLocation = 0;

        Assert.Null(campaign.BeginDragonBattle());

        campaign.State.CurrentLocation = World.Locations.Length - 1;
        Assert.NotNull(campaign.BeginDragonBattle());
        Assert.Equal(VictoryKind.None, campaign.State.Victory);
    }

    [Fact]
    public void EyeMovesAndOneAccurateThrustWins()
    {
        var battle = new DragonBattleSession(16);
        battle.Tick(1);

        Assert.NotEqual(.5, battle.EyeX);
        battle.MoveAim((battle.EyeX - battle.AimX) / DragonBattleSession.Rules.AimSpeed,
            (battle.EyeY - battle.AimY) / DragonBattleSession.Rules.AimSpeed, 1);

        Assert.True(battle.Strike());
        Assert.Equal(DragonBattleOutcome.Victory, battle.Outcome);
        Assert.False(battle.Strike());
    }

    [Fact]
    public void MissDefeatsThePlayerAndRetreatLeavesTheCampaignOpen()
    {
        var defeatedCampaign = ReadyCampaign();
        var defeatedBattle = defeatedCampaign.BeginDragonBattle()!;
        Assert.False(defeatedBattle.Strike());
        Assert.False(defeatedCampaign.FinishDragonBattle(defeatedBattle));
        Assert.Equal(VictoryKind.Defeat, defeatedCampaign.State.Victory);

        var withdrawnCampaign = ReadyCampaign();
        var withdrawnBattle = withdrawnCampaign.BeginDragonBattle()!;
        withdrawnBattle.Withdraw();
        Assert.False(withdrawnCampaign.FinishDragonBattle(withdrawnBattle));
        Assert.Equal(VictoryKind.None, withdrawnCampaign.State.Victory);
    }

    [Fact]
    public void StrengthWidensTheEyeHitWindowAndDelayIsFatal()
    {
        Assert.True(new DragonBattleSession(30).HitRadius > new DragonBattleSession(16).HitRadius);
        var battle = new DragonBattleSession(16);

        battle.Tick(DragonBattleSession.Rules.DurationSeconds);

        Assert.Equal(DragonBattleOutcome.Defeat, battle.Outcome);
    }

    private static Campaign ReadyCampaign()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        campaign.State.Player.Inventory.Items.UnionWith(Balance.Victories[VictoryKind.Dragon].RequiredItems);
        campaign.State.CurrentLocation = World.Locations.Length - 1;
        return campaign;
    }
}
