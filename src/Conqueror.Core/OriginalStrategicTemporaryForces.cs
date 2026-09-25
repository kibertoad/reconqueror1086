namespace Conqueror.Core;

/// <summary>
/// Fixed brigand order descriptors (FMT-STRATEGY-006, RULE-STRATEGY-016).
/// The descriptor index is also the brigand record's slot.
/// </summary>
public sealed record OriginalStrategicTemporaryForceRoute(
    int DescriptorIndex,
    string ResourceName,
    int PointCount);

/// <summary>
/// One executable-fixed creator descriptor. The source stores its expiry
/// month and year in the descriptor table, then removes the record only when
/// both independent comparisons have reached their thresholds.
/// </summary>
public sealed record OriginalStrategicTemporaryForceCreator(
    int ActionId,
    int DescriptorIndex,
    int OriginProperty,
    int ExpiryMonth,
    int ExpiryYear);

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
