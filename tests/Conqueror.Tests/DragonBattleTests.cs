using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed class DragonBattleTests
{
    [Fact]
    public void DragonMoorIsHiddenUntilItsOriginalConversationFlagIsSet()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        var moor = World.Locations.Length - 1;

        Assert.False(campaign.DragonLairDiscovered);
        Assert.False(campaign.CanRevealLocation(moor));
        Assert.False(campaign.CanTravelTo(moor));
        Assert.Equal(0, campaign.TravelTo(moor));
        Assert.Equal(0, campaign.State.CurrentLocation);

        campaign.State.ConversationVariables.AddRange([0, 0, 1]);
        Assert.True(campaign.DragonLairDiscovered);
        Assert.True(campaign.CanRevealLocation(moor));
    }

    [Fact]
    public void AnnaLisaProvidesTheCleanRoomDiscoveryRouteAfterTwoWins()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        for (var win = 0; win < 2; win++)
        {
            campaign.State.Date = new DateTime(1086, 3, 1).AddMonths(win);
            campaign.State.CurrentLocation = World.TournamentIndex(campaign.State.Date);
            Assert.True(campaign.RequestColors("Anna Lisa"));
            Assert.True(campaign.Joust(0, 0));
        }

        Assert.True(campaign.DragonLairDiscovered);
        Assert.Contains("northwestern Wales", campaign.State.Journal[^2], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ImportedTournamentStateDefersCourtshipRewardsToTheDialogueActions()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        campaign.State.ConversationVariables.AddRange(Enumerable.Repeat(0,
            OriginalCampaignVariables.LadyColors + 1));
        campaign.State.CurrentLocation = World.TournamentIndex(campaign.State.Date);

        Assert.True(campaign.RequestColors("Jane"));
        Assert.True(campaign.Joust(0, 0));

        Assert.Empty(campaign.State.Player.CourtshipWins);
        Assert.DoesNotContain("Medallion", campaign.State.Player.Inventory.Items);
        Assert.Contains("return to her conversation", campaign.State.Journal[^2],
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrototypeSaveAlreadyAtTheMoorMigratesToDiscovered()
    {
        var state = Campaign.NewFromTemplate(2);
        state.CurrentLocation = World.Locations.Length - 1;

        var campaign = new Campaign(state);

        Assert.Equal(1, campaign.State.DragonProgress);
        Assert.True(campaign.DragonLairDiscovered);
    }

    [Fact]
    public void ChallengeRequiresTheDragonVictoryEquipmentAndLocation()
    {
        var campaign = ReadyCampaign();
        campaign.State.CurrentLocation = 0;

        Assert.Null(campaign.BeginDragonBattle());

        campaign.State.CurrentLocation = World.Locations.Length - 1;
        campaign.State.DragonProgress = 1;
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
        Assert.Equal(CampaignEndReason.Dragon, defeatedCampaign.State.EndReason);

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

    [Fact]
    public void ThirtiethBirthdayRecordsTheAgeLimitEnding()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        campaign.State.Player.Age = Balance.FinalAge - 1;
        campaign.State.Date = new DateTime(1086, 12, 31);

        campaign.AdvanceDays(1);

        Assert.Equal(VictoryKind.Defeat, campaign.State.Victory);
        Assert.Equal(CampaignEndReason.AgeLimit, campaign.State.EndReason);
    }

    private static Campaign ReadyCampaign()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2));
        campaign.State.Player.Inventory.Items.UnionWith(Balance.Victories[VictoryKind.Dragon].RequiredItems);
        campaign.State.DragonProgress = 1;
        campaign.State.CurrentLocation = World.Locations.Length - 1;
        return campaign;
    }
}
