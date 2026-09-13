using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OriginalForegroundLowRowsUseTheVerticalRandomEnvelopeAndFixedPointVelocity()
    {
        var motion = SiegeForegroundTrajectory.Create(0, 41, 20, 30, 80, 60, 167, 117,
            contacted: true, new FixedRandom(3));

        Assert.Equal((77, 145, 77, 87, false),
            (motion.AnchorX, motion.AnchorY, motion.TargetX, motion.TargetY, motion.Mirror));
        Assert.Equal((0, -74, 42), (motion.VelocityX8, motion.VelocityY8, motion.Frame));
        motion.Advance(0.166);
        Assert.Equal(SiegeForegroundPhase.Approaching, motion.Phase);
        motion.Advance(0.166);
        Assert.Equal(SiegeForegroundPhase.Returning, motion.Phase);
        Assert.Equal((0, 37), (motion.VelocityX8, motion.VelocityY8));
    }

    [Fact]
    public void OriginalForegroundMiddleRowsChooseAndMirrorTheirHorizontalOuterAnchor()
    {
        var left = SiegeForegroundTrajectory.Create(4, 39, 30, 25, 50, 65, 167, 117,
            contacted: false, new FixedRandom(0));
        Assert.Equal((149, 34, 66, 92, false),
            (left.AnchorX, left.AnchorY, left.TargetX, left.TargetY, left.Mirror));
        Assert.Equal(41, left.Frame);

        var right = SiegeForegroundTrajectory.Create(4, 39, 30, 25, 150, 65, 167, 117,
            contacted: false, new FixedRandom(0));
        Assert.Equal((83, 34, 166, 92, true),
            (right.AnchorX, right.AnchorY, right.TargetX, right.TargetY, right.Mirror));
    }

    [Fact]
    public void OriginalForegroundHighMeleeRowsUseTheFixedUpwardAndSidewaysAnchor()
    {
        var motion = SiegeForegroundTrajectory.Create(15, 27, 32, 24, 80, 70, 167, 117,
            contacted: false, new FixedRandom(4));

        Assert.Equal((9, 37, 92, 93, true),
            (motion.AnchorX, motion.AnchorY, motion.TargetX, motion.TargetY, motion.Mirror));
        Assert.Equal(29, motion.Frame);
        Assert.Equal(SiegeCombatPresentation.SetupFrameForCombatRow(15, 27), motion.Frame);
    }

    [Fact]
    public void ForegroundMotionUsesBoundedStableMillisecondStepsAndCompletesAContactReturn()
    {
        var split = SiegeForegroundTrajectory.Create(0, 41, 20, 30, 80, 60, 167, 117,
            contacted: true, new FixedRandom(0));
        var whole = SiegeForegroundTrajectory.Create(0, 41, 20, 30, 80, 60, 167, 117,
            contacted: true, new FixedRandom(0));

        for (var step = 0; step < 10; step++) split.Advance(0.016);
        whole.Advance(0.160);
        Assert.Equal((whole.AnchorX, whole.AnchorY, whole.Phase),
            (split.AnchorX, split.AnchorY, split.Phase));

        for (var step = 0; step < 20 && split.Phase != SiegeForegroundPhase.Complete; step++)
            split.Advance(0.166);
        Assert.Equal(SiegeForegroundPhase.Complete, split.Phase);
        Assert.Equal(-1, split.Frame);
        Assert.Throws<ArgumentOutOfRangeException>(() => split.Advance(-0.001));
    }
}
