using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed class OriginalPracticeJoustTrialTests
{
    [Fact]
    public void ExactThreeFrameTargetHitUsesStrictIndependentAxisLimits()
    {
        var trial = new OriginalPracticeJoustTrial();
        trial.RecordFrame(79, 0, 0);
        trial.RecordFrame(80, 191, 191);
        trial.RecordFrame(80, 0, 0);
        trial.RecordFrame(81, 161, 203);
        trial.RecordFrame(82, 122, 211);
        var result = trial.Resolve(() => throw new InvalidOperationException("A hit must not draw randomness."));
        Assert.Equal(OriginalPracticeJoustOutcome.PlayerHit, result.Outcome);
        Assert.Equal((0, 0, 22, 50),
            (result.HorizontalError, result.VerticalError, result.PlayerPoints, result.OpponentPoints));
    }

    [Theory]
    [InlineData(79, OriginalPracticeJoustOutcome.OpponentHit, 52)]
    [InlineData(80, OriginalPracticeJoustOutcome.BothMissed, 50)]
    public void EqualThresholdMissUsesStrictOpponentRoll(int roll,
        OriginalPracticeJoustOutcome expected, int opponentPoints)
    {
        var trial = new OriginalPracticeJoustTrial();
        trial.RecordFrame(80, 191 + 70, 191);
        trial.RecordFrame(81, 161, 203);
        trial.RecordFrame(82, 122, 211);
        var result = trial.Resolve(() => roll);
        Assert.Equal(expected, result.Outcome);
        Assert.Equal((70, 0, 20, opponentPoints),
            (result.HorizontalError, result.VerticalError, result.PlayerPoints, result.OpponentPoints));
    }

    [Fact]
    public void IncompleteMovieCannotSettlePractice()
    {
        var trial = new OriginalPracticeJoustTrial();
        trial.RecordFrame(80, 191, 191);
        Assert.Throws<InvalidOperationException>(() => trial.Resolve(() => 0));
    }
}
