namespace Conqueror.Core;

/// <summary>
/// Fixed temporary-force route descriptors recovered from constructor
/// <c>0x3B0E4</c>. Descriptor index is also the physical temporary-record
/// slot selected by the source's 0x20-byte creator-data table.
/// </summary>
public sealed record OriginalStrategicTemporaryForceRoute(
    int DescriptorIndex,
    string ResourceName,
    int PointCount);

public static class OriginalStrategicTemporaryForces
{
    private static readonly OriginalStrategicTemporaryForceRoute[] RouteRows =
    [
        new(1, "scot.rat", 44),
        new(2, "wales.rat", 42)
    ];

    /// <summary>
    /// The automatic source creators leave slot zero to the separate
    /// descriptor-zero UI branch, whose route/payload remains unrecovered.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicTemporaryForceRoute> Routes => RouteRows;

    public static bool TryGetRoute(int descriptorIndex, out OriginalStrategicTemporaryForceRoute route)
    {
        route = RouteRows.FirstOrDefault(route => route.DescriptorIndex == descriptorIndex)!;
        return route is not null;
    }
}
