using Conqueror.Core;
using Conqueror.Game;
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
        campaign.State.Player.LanceExperience = 20;
        Assert.Equal(442, campaign.BeginDragonBattle()!.ScoreThreshold);
        Assert.Equal(VictoryKind.None, campaign.State.Victory);
    }

    [Fact]
    public void DragonRunUsesTheAuthoredLateTargetTrackAndScoresAtTheEnd()
    {
        var battle = new DragonBattleSession(20, OriginalDragonRunScore.FullEquipmentBonus);
        var firstTargetTime = OriginalDragonRunTimeline.FirstTargetFrame
            * OriginalDragonRunTimeline.FrameMilliseconds / 1000d;
        battle.Tick(firstTargetTime - .001);
        Assert.False(battle.EyeVisible);
        battle.Tick(.001);

        Assert.Equal(108, battle.SourceFrame);
        Assert.True(battle.EyeVisible);
        Assert.Equal(277d / 640, battle.EyeX);
        Assert.Equal(215d / 480, battle.EyeY);
        Assert.Equal(1, battle.ScoredFrames);
        Assert.Equal(DragonBattleOutcome.InProgress, battle.Outcome);
    }

    [Fact]
    public void SustainedLanceAlignmentWinsAtTheMovieBoundary()
    {
        var battle = new DragonBattleSession(20, OriginalDragonRunScore.FullEquipmentBonus);
        battle.SetAim(277d / 639, 145d / 479);
        battle.Tick(133 * .071);
        Assert.Equal(26, battle.ScoredFrames);
        Assert.True(battle.HorizontalError < battle.ScoreThreshold);
        Assert.True(battle.VerticalError < battle.ScoreThreshold);
        Assert.Equal(DragonBattleOutcome.InProgress, battle.Outcome);

        battle.Tick(.071);

        Assert.Equal(DragonBattleOutcome.Victory, battle.Outcome);
    }

    [Fact]
    public void LanceMotionUsesFixedPointRecoilDampingAndPointerPull()
    {
        var motion = new OriginalDragonLanceMotion();
        Assert.Equal((225, 150), (motion.X, motion.Y));

        motion.Advance(0, 225, 150);
        Assert.Equal((225, 150), (motion.X, motion.Y));
        Assert.Equal(-5000, motion.VelocityY8);

        motion.Advance(1, 225, 150);
        Assert.Equal((225, 130), (motion.X, motion.Y));
        Assert.Equal(-3488, motion.VelocityY8);

        var pulled = new OriginalDragonLanceMotion();
        pulled.Advance(0, 400, 150);
        Assert.Equal(4352, pulled.VelocityX8);
        pulled.Advance(1, 400, 150);
        Assert.Equal(242, pulled.X);
    }

    [Fact]
    public void DragonMovieFrameSubdivisionDoesNotChangeSteadyAimOutcome()
    {
        var oneUpdate = new DragonBattleSession(20, 17);
        var manyUpdates = new DragonBattleSession(20, 17);
        oneUpdate.SetAim(277d / 639, 145d / 479);
        manyUpdates.SetAim(277d / 639, 145d / 479);
        oneUpdate.Tick(DragonBattleSession.Rules.DurationSeconds);
        for (var frame = 1; frame <= 134; frame++)
            manyUpdates.Tick(.071);

        Assert.Equal(oneUpdate.ScoredFrames, manyUpdates.ScoredFrames);
        Assert.Equal(oneUpdate.HorizontalError, manyUpdates.HorizontalError);
        Assert.Equal(oneUpdate.VerticalError, manyUpdates.VerticalError);
        Assert.Equal(oneUpdate.Outcome, manyUpdates.Outcome);
    }

    [Fact]
    public void DragonScoreUsesLanceExperienceFullGearBonusAndStrictPerAxisBounds()
    {
        Assert.Equal(17, OriginalDragonRunScore.EquipmentBonus(true, true, true));
        Assert.Equal(8, OriginalDragonRunScore.EquipmentBonus(true, true, false));
        Assert.Equal(17, OriginalDragonRunScore.EquipmentBonus(
            new HashSet<string>([OriginalDragonRunScore.LanceItem,
                OriginalDragonRunScore.ArmorItem, OriginalDragonRunScore.ShieldItem])));
        Assert.Equal("Dragon Slaying Lance", OriginalConversationBindings.Items[10]);
        Assert.Equal("Shield of St. George", OriginalConversationBindings.Items[15]);
        Assert.Equal("Dragon Slaying Armor", OriginalConversationBindings.Items[20]);
        Assert.Equal(0x36, OriginalDragonRunScore.LanceSlot);
        Assert.Equal(0x3B, OriginalDragonRunScore.ShieldSlot);
        Assert.Equal(0x40, OriginalDragonRunScore.ArmorSlot);
        Assert.Equal(442, OriginalDragonRunScore.Threshold(20, 17));
        Assert.Equal(338, OriginalDragonRunScore.Threshold(16, 17));
        Assert.True(OriginalDragonRunScore.Succeeds(20, 17, 441, 441));
        Assert.False(OriginalDragonRunScore.Succeeds(20, 17, 442, 0));
        Assert.False(OriginalDragonRunScore.Succeeds(20, 17, 0, 442));
    }

    [Fact]
    public void LanceFrameUsesSourceBandsAndClampsTheUnboundedHighAimCase()
    {
        Assert.Equal(70, OriginalDragonLanceSelection.HorizontalBandWidth);
        Assert.Equal(4, OriginalDragonLanceSelection.FrameFor(50, 240));
        Assert.Equal(0, OriginalDragonLanceSelection.FrameFor(400, 240));
        Assert.Equal(9, OriginalDragonLanceSelection.FrameFor(50, 188));
        Assert.Equal(14, OriginalDragonLanceSelection.FrameFor(50, 187));
        Assert.Equal(19, OriginalDragonLanceSelection.FrameFor(50, 114));
        Assert.Equal(24, OriginalDragonLanceSelection.FrameFor(50, 92));
        Assert.Equal(24, OriginalDragonLanceSelection.FrameFor(50, 20));
    }

    [Fact]
    public void DragonRunTargetsEndAtTheSourceMovieStopFrame()
    {
        Assert.Contains(new ImportedMovieDefinition("Dragon.Run", "/drjstrun.smk"),
            ImportedMovies.Definitions);
        Assert.Null(OriginalDragonRunTimeline.TargetAt(107));
        Assert.Equal((277, 215), OriginalDragonRunTimeline.TargetAt(108));
        Assert.Equal((254, 158), OriginalDragonRunTimeline.TargetAt(133));
        Assert.Null(OriginalDragonRunTimeline.TargetAt(134));
        Assert.Equal(134, OriginalDragonRunTimeline.FrameAt(TimeSpan.FromMilliseconds(134 * 71)));
        Assert.Equal(134 * 71 / 1000d, DragonBattleSession.Rules.DurationSeconds);
    }

    [Fact]
    public void MissDefeatsThePlayerAndRetreatLeavesTheCampaignOpen()
    {
        var defeatedCampaign = ReadyCampaign();
        var defeatedBattle = defeatedCampaign.BeginDragonBattle()!;
        defeatedBattle.Tick(DragonBattleSession.Rules.DurationSeconds);
        Assert.Equal(DragonBattleOutcome.Defeat, defeatedBattle.Outcome);
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
    public void LanceExperienceRaisesTheScoreGateAndUnalignedAimIsFatal()
    {
        Assert.True(new DragonBattleSession(20, 17).ScoreThreshold
            > new DragonBattleSession(16, 17).ScoreThreshold);
        var battle = new DragonBattleSession(16, 17);

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
