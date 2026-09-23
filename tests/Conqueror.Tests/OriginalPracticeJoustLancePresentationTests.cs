using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed class OriginalPracticeJoustLancePresentationTests
{
    [Fact]
    public void LargeLanceFrameStopsAtSourceMovieCropBeforeScaling()
    {
        var blit = OriginalPracticeJoustLancePresentation.Clip(400, 280, 416, 398);
        Assert.Equal(new UiBounds(0, 0, 239, 19), blit?.Source);
        Assert.Equal(new UiBounds(400, 370, 239, 19), blit?.Destination);
    }

    [Fact]
    public void LeftAndTopClipAdjustTextureSourceAndKeepMovieOffset()
    {
        var blit = OriginalPracticeJoustLancePresentation.Clip(-5, -3, 20, 10);
        Assert.Equal(new UiBounds(5, 3, 15, 7), blit?.Source);
        Assert.Equal(new UiBounds(0, 90, 15, 7), blit?.Destination);
        Assert.Null(OriginalPracticeJoustLancePresentation.Clip(639, 50, 20, 20));
        Assert.Null(OriginalPracticeJoustLancePresentation.Clip(50, 299, 20, 20));
    }
}
