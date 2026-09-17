using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

/// <summary>
/// Eagerly decodes the complete executable-selected strategic route set and
/// immutable world grid. Startup therefore fails at the import boundary rather
/// than allowing malformed resources to surface after a campaign has begun.
/// </summary>
public sealed class ImportedOriginalStrategicResources : IOriginalStrategicResources
{
    private sealed record RoutePair(
        IReadOnlyList<OriginalStrategicRoutePoint> Forward,
        IReadOnlyList<OriginalStrategicRoutePoint> Reverse);

    private readonly Dictionary<string, RoutePair> _routes =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly StrategicWorldGrid _world;

    public ImportedOriginalStrategicResources(ImportedContentCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var routeNames = OriginalStrategicMovement.PropertyRouteResources
            .Select(route => route.ResourceName)
            .Concat(OriginalStrategicMovement.StartingRoutes.Select(route => route.ResourceName))
            .Distinct(StringComparer.OrdinalIgnoreCase);
        foreach (var resourceName in routeNames)
        {
            var id = catalog.FindId("resource", $":{resourceName}")
                ?? throw new InvalidDataException(
                    $"Required original strategic route '{resourceName}' is missing.");
            var decoded = catalog.DecodeStrategicRoute(id)
                ?? throw new InvalidDataException(
                    $"Required original strategic route '{resourceName}' could not be decoded.");
            var forward = decoded.Points
                .Select(point => new OriginalStrategicRoutePoint(point.X, point.Y))
                .ToArray();
            _routes.Add(resourceName, new RoutePair(
                Array.AsReadOnly(forward),
                Array.AsReadOnly(forward.Reverse().ToArray())));
        }

        var worldId = catalog.FindId("resource", ":icon.jp")
            ?? throw new InvalidDataException("Required original strategic world 'icon.jp' is missing.");
        _world = catalog.DecodeStrategicWorldGrid(worldId)
            ?? throw new InvalidDataException("Required original strategic world 'icon.jp' could not be decoded.");
    }

    public IReadOnlyList<OriginalStrategicRoutePoint> Route(string resourceName, bool reverse)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceName);
        if (!_routes.TryGetValue(resourceName, out var route))
            throw new InvalidDataException($"Unknown original strategic route '{resourceName}'.");
        return reverse ? route.Reverse : route.Forward;
    }

    public bool TryTerrainCell(
        int worldX,
        int worldY,
        int cameraRow,
        int cameraColumn,
        out OriginalStrategicTerrainCell cell)
    {
        cell = default;
        if (!StrategicWorldProjection.TryWorldToCell(
                worldX, worldY, cameraRow, cameraColumn, out var position))
            return false;
        cell = new OriginalStrategicTerrainCell(
            position.Row, position.Column, _world[position.Row, position.Column].RawValue);
        return true;
    }
}
