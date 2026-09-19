namespace Conqueror.Core;

/// <summary>
/// Stable elapsed-time boundary for interactive resolver loop <c>0x26B88</c>.
/// The original compares its scaled timer strictly against the previous sample
/// plus 200 and replaces the previous sample with the current value after one
/// pass; it does not catch up several tactical passes after a long frame.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterTiming
{
    public static readonly TimeSpan Cadence = TimeSpan.FromMilliseconds(200);

    public OriginalStrategicInteractiveEncounterTiming(TimeSpan initialSample)
    {
        if (initialSample < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(initialSample));
        LastSample = initialSample;
    }

    /// <summary>
    /// The absolute time recorded by the last accepted tactical pass.
    /// </summary>
    public TimeSpan LastSample { get; private set; }

    /// <summary>
    /// Returns true only once the current absolute time is strictly beyond the
    /// prior sample plus 200 ms. A late update becomes the new baseline rather
    /// than producing synthetic catch-up passes.
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
