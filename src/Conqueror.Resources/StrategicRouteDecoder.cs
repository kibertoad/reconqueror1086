using System.Buffers.Binary;

namespace Conqueror.Resources;

public readonly record struct StrategicRoutePoint(int X, int Y);

public sealed record StrategicRouteResource(IReadOnlyList<StrategicRoutePoint> Points);

public static class StrategicRouteDecoder
{
    public const int PointSize = 8;
    public const int MaximumPointCount = 4096;

    public static StrategicRouteResource Decode(ReadOnlySpan<byte> data)
    {
        if (data.Length < sizeof(int)) throw new InvalidDataException("Strategic route header is truncated.");
        var count = BinaryPrimitives.ReadInt32LittleEndian(data);
        if (count <= 0 || count > MaximumPointCount)
            throw new InvalidDataException("Strategic route point count is invalid.");
        var expectedLength = checked(sizeof(int) + count * PointSize);
        if (data.Length != expectedLength)
            throw new InvalidDataException("Strategic route length does not match its point count.");

        var points = new StrategicRoutePoint[count];
        for (var index = 0; index < count; index++)
        {
            var offset = sizeof(int) + index * PointSize;
            points[index] = new(
                BinaryPrimitives.ReadInt32LittleEndian(data[offset..]),
                BinaryPrimitives.ReadInt32LittleEndian(data[(offset + sizeof(int))..]));
        }
        return new(points);
    }
}
