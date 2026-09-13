namespace Conqueror.Core;

/// <summary>Integer actor-heading and movement arithmetic recovered from the original executable.</summary>
public static class OriginalActorMotion
{
    // Object-2 +0xC904. The executable uses this signed 1.15 quarter-wave
    // through 0x446BC/0x4472C/0x44740 rather than host trigonometry.
    private static readonly int[] QuarterSine15 =
    [
        0, 817, 1633, 2449, 3263, 4074, 4884, 5690,
        6493, 7291, 8085, 8875, 9658, 10436, 11207, 11971,
        12728, 13477, 14217, 14949, 15671, 16384, 17086, 17778,
        18458, 19128, 19785, 20430, 21062, 21681, 22287, 22879,
        23457, 24020, 24568, 25101, 25618, 26120, 26605, 27073,
        27525, 27960, 28377, 28777, 29158, 29522, 29867, 30194,
        30502, 30791, 31061, 31311, 31542, 31754, 31945, 32117,
        32269, 32401, 32513, 32604, 32675, 32726, 32757, 32767
    ];

    // 0x445C4 receives the original local (-worldY, worldX) vector.
    public static int HeadingToward(int deltaX8, int deltaY8) =>
        Heading(-deltaY8, deltaX8);

    // The movement scheduler rotates descriptor-local coordinates by the
    // actor's byte-turn heading, rounding every 1.15 product independently.
    public static (int X, int Y) Rotate(int x, int y, int heading)
    {
        var sine = Sin15(heading);
        var cosine = Cos15(heading);
        return (
            FixedProduct15(sine, x) + FixedProduct15(cosine, y),
            -FixedProduct15(cosine, x) + FixedProduct15(sine, y));
    }

    // Handler 0x50020 multiplies each descriptor coordinate by object-2
    // double +0x7CEA (= 1.5) and converts it with x87 FISTP (nearest-even).
    public static int ScaleEscapeDelta(int value) => checked((int)Math.Round(
        value * 1.5d, MidpointRounding.ToEven));

    public static int FixedProduct15(int left, int right) =>
        checked((int)(((long)left * right + 0x3fff) >> 15));

    public static int Cos15(int heading) => Sin15(heading + 0x40);

    public static int Sin15(int heading)
    {
        var angle = heading & 0xff;
        return (angle >> 6) switch
        {
            0 => QuarterSine15[angle],
            1 => QuarterSine15[0x7f - angle],
            2 => -QuarterSine15[angle - 0x80],
            _ => -QuarterSine15[0xff - angle]
        };
    }

    private static int Heading(int deltaX, int deltaY)
    {
        if (deltaX == 0 && deltaY == 0) return 0;
        var x = Math.Abs((long)deltaX);
        var y = Math.Abs((long)deltaY);
        if (deltaY >= 0)
        {
            if (deltaX >= 0)
                return x > y ? (int)(y * 0x20 / x) : 0x40 - (int)(x * 0x20 / y);
            return x >= y ? 0x80 - (int)(y * 0x20 / x) : 0x40 + (int)(x * 0x20 / y);
        }
        if (deltaX < 0)
            return x > y ? 0x80 + (int)(y * 0x20 / x) : 0xc0 - (int)(x * 0x20 / y);
        var result = x < y ? 0xc0 + (int)(x * 0x20 / y) : 0x100 - (int)(y * 0x20 / x);
        return result & 0xff;
    }
}
