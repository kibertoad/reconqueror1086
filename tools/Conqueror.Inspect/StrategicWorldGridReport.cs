using Conqueror.Resources;
using System.Text;

internal static class StrategicWorldGridReport
{
    public static string Build(DynamixArchive archive)
    {
        var report = new StringBuilder("# Strategic world grid (derived metadata; original bytes omitted)\n");
        var entry = archive.Entries.FirstOrDefault(candidate =>
            candidate.Name.Equals("icon.jp", StringComparison.OrdinalIgnoreCase));
        if (entry is null || !DynamixArchive.CanDecode(entry))
        {
            report.AppendLine("missing icon.jp");
            return report.ToString();
        }

        try
        {
            var bytes = archive.ReadDecoded(entry);
            var grid = StrategicWorldGridDecoder.Decode(bytes);
            report.AppendLine($"resource-index {entry.Index} name {entry.Name}");
            report.AppendLine($"encoded-length {bytes.Length} xxh3 {ResourceHash.Xxh3(bytes)}");
            report.AppendLine($"cell-size {grid.CellWidth}x{grid.CellHeight} rows {grid.RowCount} columns {grid.ColumnCount} storage column-major-dwords");
            report.AppendLine($"tile-id-range {grid.Cells.Min(cell => cell.TileId)}..{grid.Cells.Max(cell => cell.TileId)} distinct {grid.Cells.Select(cell => cell.TileId).Distinct().Count()}");
            report.AppendLine($"auxiliary-range {grid.Cells.Min(cell => cell.Auxiliary)}..{grid.Cells.Max(cell => cell.Auxiliary)} distinct {grid.Cells.Select(cell => cell.Auxiliary).Distinct().Count()}");
            report.AppendLine($"nonzero-upper-byte {grid.Cells.Count(cell => cell.UpperByte != 0)}");
            AppendRouteProjectionCensus(report, archive);
        }
        catch (InvalidDataException error)
        {
            report.AppendLine($"rejected {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
        return report.ToString();
    }

    private static void AppendRouteProjectionCensus(StringBuilder report, DynamixArchive archive)
    {
        var routes = archive.Entries.Where(entry =>
                (entry.Name.StartsWith("rt_", StringComparison.OrdinalIgnoreCase) ||
                 entry.Name.StartsWith("sc_", StringComparison.OrdinalIgnoreCase)) &&
                entry.Name.EndsWith(".rat", StringComparison.OrdinalIgnoreCase) &&
                DynamixArchive.CanDecode(entry))
            .Select(entry => (entry.Name, Route: StrategicRouteDecoder.Decode(archive.ReadDecoded(entry))))
            .ToArray();
        var points = routes.SelectMany(route => route.Route.Points.Select(point => (route.Name, Point: point))).ToArray();
        if (points.Length == 0)
        {
            report.AppendLine($"route-projection resources {routes.Length} points 0");
            return;
        }
        var misses = 0;
        var edgePoints = 0;
        var cameraVariantPoints = 0;
        var cameraVariantCoordinates = new HashSet<StrategicRoutePoint>();
        var cameraVariantSamples = new List<string>();
        var projected = new List<StrategicWorldCellPosition>(points.Length);
        foreach (var (name, point) in points)
        {
            if (!StrategicWorldProjection.TryWorldToCell(point.X, point.Y, 0, 0, out var originCamera))
            {
                misses++;
                continue;
            }
            projected.Add(originCamera);
            var center = StrategicWorldProjection.CellCenter(originCamera.Row, originCamera.Column);
            if (Math.Abs(point.X - center.X) + 2 * Math.Abs(point.Y - center.Y) ==
                StrategicWorldGridDecoder.CellWidth / 2) edgePoints++;
            if (!StrategicWorldProjection.TryWorldToCell(point.X, point.Y,
                    StrategicWorldGridDecoder.RowCount - 1,
                    StrategicWorldGridDecoder.ColumnCount - 1, out var oppositeCamera) ||
                oppositeCamera != originCamera)
            {
                cameraVariantPoints++;
                cameraVariantCoordinates.Add(point);
                if (cameraVariantSamples.Count < 5)
                    cameraVariantSamples.Add($"{name}:{point.X},{point.Y} " +
                        $"origin={originCamera.Row},{originCamera.Column} " +
                        $"opposite={oppositeCamera.Row},{oppositeCamera.Column}");
            }
        }

        report.AppendLine($"route-projection resources {routes.Length} points {points.Length} " +
            $"world-x {points.Min(item => item.Point.X)}..{points.Max(item => item.Point.X)} " +
            $"world-y {points.Min(item => item.Point.Y)}..{points.Max(item => item.Point.Y)}");
        report.AppendLine($"route-projection misses {misses} inclusive-edge-points {edgePoints} " +
            $"camera-variant-points {cameraVariantPoints} distinct {cameraVariantCoordinates.Count}");
        foreach (var sample in cameraVariantSamples)
            report.AppendLine($"route-projection camera-variant-sample {sample}");
        if (projected.Count > 0)
            report.AppendLine($"route-projection rows {projected.Min(cell => cell.Row)}..{projected.Max(cell => cell.Row)} " +
                $"columns {projected.Min(cell => cell.Column)}..{projected.Max(cell => cell.Column)}");
    }
}
