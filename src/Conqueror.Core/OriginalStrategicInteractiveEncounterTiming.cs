namespace Conqueror.Core;

/// <summary>
/// Stable elapsed-time boundary for interactive resolver loop RULE-BATTLE-003.
/// The executable compares its scaled timer strictly against the prior sample
/// plus <c>0xC8</c>, then records a fresh sample after one accepted pass. Its
/// callback is registered for 250 Hz, so the source clock advances in four-unit
/// steps. The default host clock preserves that quantization.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterTiming
{
    /// <summary>
    /// Recovered nominal duration of the source's 200-unit threshold.
    /// </summary>
    public static readonly TimeSpan DefaultCompatibilityCadence = TimeSpan.FromMilliseconds(200);
    private static readonly TimeSpan SourceTimerTick = TimeSpan.FromMilliseconds(4);
    private readonly bool _useSourceTimerTicks;

    public OriginalStrategicInteractiveEncounterTiming(
        TimeSpan initialSample,
        TimeSpan? cadence = null)
    {
        if (initialSample < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialSample));
        Cadence = cadence ?? DefaultCompatibilityCadence;
        if (Cadence <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(cadence));
        _useSourceTimerTicks = cadence is null;
        LastSample = _useSourceTimerTicks ? QuantizeToSourceTick(initialSample) : initialSample;
    }

    /// <summary>
    /// The absolute time recorded by the last accepted tactical pass.
    /// </summary>
    public TimeSpan LastSample { get; private set; }

    /// <summary>
    /// Stable host duration corresponding to the source's strict <c>0xC8</c>
    /// timer-unit threshold.
    /// </summary>
    public TimeSpan Cadence { get; }

    /// <summary>
    /// Returns true only once the current clock is strictly beyond the prior
    /// sample plus the threshold. The recovered default samples four-ms ticks;
    /// explicit host calibrations use their supplied continuous clock. A late
    /// update becomes the new baseline without synthetic catch-up passes.
    /// </summary>
    public bool TryBeginPass(TimeSpan currentTime)
    {
        if (currentTime < LastSample)
            throw new ArgumentOutOfRangeException(nameof(currentTime));
        var sampledTime = _useSourceTimerTicks ? QuantizeToSourceTick(currentTime) : currentTime;
        if (sampledTime - LastSample <= Cadence)
            return false;

        LastSample = sampledTime;
        return true;
    }

    private static TimeSpan QuantizeToSourceTick(TimeSpan time) =>
        TimeSpan.FromTicks(time.Ticks / SourceTimerTick.Ticks * SourceTimerTick.Ticks);
}
