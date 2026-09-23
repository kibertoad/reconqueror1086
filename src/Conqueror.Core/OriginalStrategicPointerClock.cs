namespace Conqueror.Core;

/// <summary>
/// Nominal INT 1Ch count from the original timer service's IRQ0 schedule.
/// The service runs IRQ0 at 250 Hz, then advances the old BIOS IRQ0 vector
/// through a 16.16 accumulator. BIOS IRQ0 invokes the game's INT 1Ch hook.
/// </summary>
public static class OriginalStrategicPointerClock
{
    private const int OscillatorCyclesPerSecond = 0x1234DC;
    private const int HardwareInterruptsPerSecond = 0xFA;
    private const int HardwareDivisor = OscillatorCyclesPerSecond / HardwareInterruptsPerSecond;
    private const int BiosAccumulatorStep = 0x123333 / HardwareInterruptsPerSecond;
    private const int BiosAccumulatorWhole = 0x10000;

    public static long UnitsAt(TimeSpan elapsed)
    {
        if (elapsed < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(elapsed));

        // Decimal avoids overflow when a session runs longer than a few days.
        var hardwareInterrupts = decimal.Truncate(
            (decimal)elapsed.Ticks * OscillatorCyclesPerSecond
            / (HardwareDivisor * (decimal)TimeSpan.TicksPerSecond));
        return (long)decimal.Truncate(
            hardwareInterrupts * BiosAccumulatorStep / BiosAccumulatorWhole);
    }
}
