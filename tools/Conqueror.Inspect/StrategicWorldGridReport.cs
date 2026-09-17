using Conqueror.Resources;
using System.Security.Cryptography;
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
            report.AppendLine($"encoded-length {bytes.Length} sha256 {Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()}");
            report.AppendLine($"cell-size {grid.CellWidth}x{grid.CellHeight} rows {grid.RowCount} columns {grid.ColumnCount} storage column-major-dwords");
            report.AppendLine($"tile-id-range {grid.Cells.Min(cell => cell.TileId)}..{grid.Cells.Max(cell => cell.TileId)} distinct {grid.Cells.Select(cell => cell.TileId).Distinct().Count()}");
            report.AppendLine($"auxiliary-range {grid.Cells.Min(cell => cell.Auxiliary)}..{grid.Cells.Max(cell => cell.Auxiliary)} distinct {grid.Cells.Select(cell => cell.Auxiliary).Distinct().Count()}");
            report.AppendLine($"nonzero-upper-byte {grid.Cells.Count(cell => cell.UpperByte != 0)}");
        }
        catch (InvalidDataException error)
        {
            report.AppendLine($"rejected {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
        return report.ToString();
    }
}
