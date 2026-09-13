using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void HitEffectRetainsTheOriginalFourAscendingFrames()
    {
        var effect = new SiegeHitEffect(SiegeCombatPresentation.FatalHitBlood);

        Assert.Equal(43, effect.Frame);
        effect.Advance(SiegeHitEffect.CompatibilityStepSeconds - 0.001);
        Assert.Equal(43, effect.Frame);
        effect.Advance(0.002);
        Assert.Equal(44, effect.Frame);
        effect.Advance(3 * SiegeHitEffect.CompatibilityStepSeconds);
        Assert.True(effect.Complete);
        Assert.Equal(-1, effect.Frame);
    }

    [Fact]
    public void HitEffectProgressDependsOnElapsedTimeNotUpdateFrequency()
    {
        var singleUpdate = new SiegeHitEffect(SiegeCombatPresentation.WoundingHitBlood);
        var sixtyHertz = new SiegeHitEffect(SiegeCombatPresentation.WoundingHitBlood);
        var oneFortyFourHertz = new SiegeHitEffect(SiegeCombatPresentation.WoundingHitBlood);

        singleUpdate.Advance(0.2);
        for (var index = 0; index < 60; index++) sixtyHertz.Advance(0.2 / 60);
        for (var index = 0; index < 144; index++) oneFortyFourHertz.Advance(0.2 / 144);

        Assert.Equal(50, singleUpdate.Frame);
        Assert.Equal(singleUpdate.Frame, sixtyHertz.Frame);
        Assert.Equal(singleUpdate.Frame, oneFortyFourHertz.Frame);
    }

    [Fact]
    public void HitEffectRejectsInvalidRunsAndElapsedTime()
    {
        Assert.Throws<ArgumentException>(() => new SiegeHitEffect(new SiegeFrameRun(43, 5)));
        Assert.Throws<ArgumentException>(() => new SiegeHitEffect(new SiegeFrameRun(43, 4, true)));

        var effect = new SiegeHitEffect(SiegeCombatPresentation.FatalHitBlood);
        Assert.Throws<ArgumentOutOfRangeException>(() => effect.Advance(-0.001));
        Assert.Throws<ArgumentOutOfRangeException>(() => effect.Advance(double.NaN));
    }
}
