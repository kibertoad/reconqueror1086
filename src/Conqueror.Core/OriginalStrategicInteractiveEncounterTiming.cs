namespace Conqueror.Core;

/// <summary>
/// Stable elapsed-time boundary for interactive resolver loop <c>0x26B88</c>.
/// The executable compares its scaled timer strictly against the prior sample
/// plus <c>0xC8</c>, then records a fresh sample after one accepted pass. Its
/// interrupt frequency is unrecovered, so the host cadence is explicit rather
/// than being presented as an original wall-clock measurement.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterTiming
{
    /// <summary>
    /// Deterministic replacement policy for callers that have no measured
    /// timer calibration. This is not a claim about the original interrupt
    /// frequency.
    /// </summary>
    public static readonly TimeSpan DefaultCompatibilityCadence = TimeSpan.FromMilliseconds(200);

    public OriginalStrategicInteractiveEncounterTiming(
        TimeSpan initialSample,
        TimeSpan? cadence = null)
    {
        if (initialSample < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialSample));
        Cadence = cadence ?? DefaultCompatibilityCadence;
        if (Cadence <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(cadence));
        LastSample = initialSample;
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
    /// Returns true only once the current absolute time is strictly beyond the
    /// prior sample plus the configured stable cadence. A late update becomes
    /// the new baseline rather than producing synthetic catch-up passes.
    /// </summary>
    public bool TryBeginPass(TimeSpan currentTime)
    {
        if (currentTime < LastSample)
            throw new ArgumentOutOfRangeException(nameof(currentTime));
        if (currentTime - LastSample <= Cadence)
            return false;

        LastSample = currentTime;
        return true;
    }
}
