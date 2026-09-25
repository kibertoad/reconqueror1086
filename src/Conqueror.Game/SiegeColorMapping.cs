using Conqueror.Resources;

namespace Conqueror.Game;

public static class SiegeColorMapping
{
    public static int WallDistanceMap(double distance, DynamixSceneColorMapping parameters, int blockOffset)
    {
        if (!double.IsFinite(distance) || distance < 0)
            throw new ArgumentOutOfRangeException(nameof(distance));
        ArgumentNullException.ThrowIfNull(parameters);
        if (!parameters.Enabled || parameters.MapCount is < 1 or > DynamixSceneColorMaps.Count
            || parameters.DistanceShift is < 2 or > 30)
            throw new ArgumentOutOfRangeException(nameof(parameters));

        // RULE-VIEW-007 selects:
        // clamp((8.8 fixed-point depth >> (Scenario.DistanceShift - 2))
        //       - block offset, 0, Scenario.MapCount - 1).
        var scale = Math.Pow(2, 10 - parameters.DistanceShift);
        var level = Math.Floor(distance * scale) - blockOffset;
        return (int)Math.Clamp(level, 0, parameters.MapCount - 1);
    }
}
