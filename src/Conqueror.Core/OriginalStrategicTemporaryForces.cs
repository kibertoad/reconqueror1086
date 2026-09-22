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

/// <summary>
/// One executable-fixed creator descriptor. The two calendar comparison
/// fields are retained as raw descriptor facts; their user-facing lifetime
/// semantics have not been recovered.
/// </summary>
public sealed record OriginalStrategicTemporaryForceCreator(
    int ActionId,
    int DescriptorIndex,
    int OriginProperty,
    int FirstCalendarThreshold,
    int SecondCalendarThreshold);

public static class OriginalStrategicTemporaryForces
{
    private static readonly OriginalStrategicTemporaryForceRoute[] RouteRows =
    [
        new(1, "scot.rat", 44),
        new(2, "wales.rat", 42)
    ];

    private static readonly OriginalStrategicTemporaryForceCreator[] CreatorRows =
    [
        new(0x2B, 1, 7, 1, 0x7D0),
        new(0x5D, 2, 7, 1, 0x7D0)
    ];

    /// <summary>
    /// The automatic source creators leave slot zero to the separate
    /// descriptor-zero UI branch, whose route/payload remains unrecovered.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicTemporaryForceRoute> Routes => RouteRows;

    public static IReadOnlyList<OriginalStrategicTemporaryForceCreator> Creators => CreatorRows;

    public static bool TryGetRoute(int descriptorIndex, out OriginalStrategicTemporaryForceRoute route)
    {
        route = RouteRows.FirstOrDefault(route => route.DescriptorIndex == descriptorIndex)!;
        return route is not null;
    }

    public static bool TryGetCreatorForAction(int actionId, out OriginalStrategicTemporaryForceCreator creator)
    {
        creator = CreatorRows.FirstOrDefault(creator => creator.ActionId == actionId)!;
        return creator is not null;
    }
}
