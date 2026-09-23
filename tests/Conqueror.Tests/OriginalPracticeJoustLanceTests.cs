using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed class OriginalPracticeJoustLanceTests
{
    [Fact]
    public void PracticeBandsUseTheirOwnSourceTableAndMovieLocalCoordinates()
    {
        Assert.Equal(70, OriginalPracticeJoustLance.HorizontalBandWidth);
        Assert.Equal(4, OriginalPracticeJoustLance.FrameFor(50, 150));
        Assert.Equal(0, OriginalPracticeJoustLance.FrameFor(400, 150));
        Assert.Equal(9, OriginalPracticeJoustLance.FrameFor(50, 98));
        Assert.Equal(14, OriginalPracticeJoustLance.FrameFor(50, 97));
        Assert.Equal(19, OriginalPracticeJoustLance.FrameFor(50, 24));
        Assert.Equal(24, OriginalPracticeJoustLance.FrameFor(50, 23));
        Assert.Equal(24, OriginalPracticeJoustLance.FrameFor(-1, -1));
        Assert.Equal(90, OriginalPracticeJoustLance.MovieTop);
    }

    [Fact]
    public void RawPointerDrivesDampedEightPointEightMotionAndSourceKick()
    {
        var lance = new OriginalPracticeJoustLance();
        lance.Advance(0, 400, 280);
        Assert.Equal((225, 150, 1813, -114),
            (lance.X, lance.Y, lance.VelocityX8, lance.VelocityY8));
        lance.Advance(1, 400, 280);
        Assert.Equal((232, 149, 3156, 1295),
            (lance.X, lance.Y, lance.VelocityX8, lance.VelocityY8));
    }
}
