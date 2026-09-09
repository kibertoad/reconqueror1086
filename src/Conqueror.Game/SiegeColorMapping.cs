using Conqueror.Resources;

namespace Conqueror.Game;

public static class SiegeColorMapping
{
    // Pal32 starts the confirmed identity-to-black family. The original family
    // selection and distance scale are still provisional and isolated here so
    // executable or controlled-observation evidence can replace this mapping.
    public const int DistanceFamilyStart = 32;
    public const int DistanceFamilyLength = 32;

    public static int WallDistanceMap(double distance)
    {
        if (!double.IsFinite(distance) || distance < 0)
            throw new ArgumentOutOfRangeException(nameof(distance));
        var level = Math.Clamp((int)Math.Floor(distance), 0, DistanceFamilyLength - 1);
        return Math.Min(DynamixSceneColorMaps.Count - 1, DistanceFamilyStart + level);
    }
}
