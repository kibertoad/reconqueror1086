using System.Text;

namespace Conqueror.Resources;

/// <summary>One source-authored exterior scene hit region.</summary>
public sealed record VillageSceneHotspot(bool Enabled, int X, int Y, int Width, int Height, string Label);

/// <summary>
/// A source-authored village or tournament exterior. The original selector that
/// chooses one of these records is deliberately kept outside this structural
/// decoder until its executable call path has been recovered.
/// </summary>
public sealed record VillageSceneDefinition(string BackgroundName, IReadOnlyList<VillageSceneHotspot> Hotspots);

// FMT-UI-003. The original maps hot spots by row position and discards the labels (RULE-UI-001).
/// <summary>Bounded decoder for the line-oriented VILLAGE.DAT/TVILLAGE.DAT scene catalogs.</summary>
public static class VillageSceneCatalogDecoder
{
    public const int HotspotsPerScene = 6;
    public const int SceneWidth = 640;
    public const int SceneHeight = 480;
    private const int MaximumBytes = 128 * 1024;
    private const int MaximumScenes = 256;
    // Six authored rectangles extend one to three pixels beyond the right edge.
    private const int MaximumAuthoredRightEdge = SceneWidth + 3;

    public static IReadOnlyList<VillageSceneDefinition> Decode(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length is 0 or > MaximumBytes)
            throw new InvalidDataException("Village scene catalog has an invalid length.");
        foreach (var value in bytes)
            if ((value < 0x20 && value is not (byte)'\t' and not (byte)'\r' and not (byte)'\n') || value > 0x7E)
                throw new InvalidDataException("Village scene catalog contains non-ASCII data.");

        var text = Encoding.ASCII.GetString(bytes).Replace("\r\n", "\n", StringComparison.Ordinal);
        if (text.Contains('\r')) throw new InvalidDataException("Village scene catalog has an invalid line ending.");
        var lines = text.Split('\n');
        if (lines[^1].Length == 0) lines = lines[..^1];
        var linesPerScene = HotspotsPerScene + 1;
        if (lines.Length == 0 || lines.Length > MaximumScenes * linesPerScene || lines.Length % linesPerScene != 0)
            throw new InvalidDataException("Village scene catalog has incomplete scene records.");

        var scenes = new List<VillageSceneDefinition>(lines.Length / linesPerScene);
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var start = 0; start < lines.Length; start += linesPerScene)
        {
            var background = lines[start].Trim();
            if (!IsBackgroundName(background) || !names.Add(background))
                throw new InvalidDataException("Village scene catalog has an invalid or duplicate background name.");
            var hotspots = new VillageSceneHotspot[HotspotsPerScene];
            for (var index = 0; index < HotspotsPerScene; index++)
                hotspots[index] = ParseHotspot(lines[start + index + 1]);
            scenes.Add(new VillageSceneDefinition(background, hotspots));
        }
        return scenes;
    }

    private static bool IsBackgroundName(string value)
    {
        var underscore = value.IndexOf('_');
        if (underscore is < 2 or > 4 || value.Length != underscore + 9 || !value.EndsWith(".PCX", StringComparison.Ordinal))
            return false;
        if (value[0] is not ('V' or 'T') || !value.AsSpan(1, underscore - 1).ToString().All(char.IsAsciiDigit)) return false;
        return value.AsSpan(underscore + 1, 4).ToString().All(value => value is '0' or '1');
    }

    private static VillageSceneHotspot ParseHotspot(string line)
    {
        // The verified source catalog has three separator typos. Repair only
        // these exact spellings; arbitrary malformed records still fail. The
        // original reads the lender row only up to its first space (DEV-UI-001).
        line = line.TrimStart() switch
        {
            var value when value.StartsWith("1,,539,374,100,59 ; Map", StringComparison.Ordinal)
                => value.Replace("1,,539,374,100,59", "1,539,374,100,59", StringComparison.Ordinal),
            var value when value.StartsWith("0,0,0,0,0, ; Blacksmith", StringComparison.Ordinal)
                => value.Replace("0,0,0,0,0,", "0,0,0,0,0", StringComparison.Ordinal),
            var value when value.StartsWith("1,356 141 43,52 ; Lender", StringComparison.Ordinal)
                => value.Replace("1,356 141 43,52", "1,356,141,43,52", StringComparison.Ordinal),
            _ => line
        };
        var separator = line.IndexOf(';');
        if (separator < 0 || line.IndexOf(';', separator + 1) >= 0)
            throw new InvalidDataException("Village scene hotspot lacks its label separator.");
        var fields = line[..separator].Split(',');
        if (fields.Length != 5 || fields.Any(field => !int.TryParse(field.Trim(), out _)))
            throw new InvalidDataException("Village scene hotspot has invalid numeric fields.");
        var enabled = int.Parse(fields[0].Trim());
        var x = int.Parse(fields[1].Trim());
        var y = int.Parse(fields[2].Trim());
        var width = int.Parse(fields[3].Trim());
        var height = int.Parse(fields[4].Trim());
        var label = line[(separator + 1)..].Trim();
        if (enabled is < 0 or > 1 || label.Length is 0 or > 64 || x < 0 || y < 0 || width < 0 || height < 0
            || x > SceneWidth || y > SceneHeight || width > MaximumAuthoredRightEdge - x || height > SceneHeight - y
            || enabled == 1 && (width == 0 || height == 0))
            throw new InvalidDataException("Village scene hotspot is out of bounds.");
        return new VillageSceneHotspot(enabled == 1, x, y, width, height, label);
    }
}
