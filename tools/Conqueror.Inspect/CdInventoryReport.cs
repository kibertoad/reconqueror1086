using Conqueror.Resources;
using System.IO.Hashing;
using System.Text;

// The inventory of the original files: every file on the disc's data track, every CD audio track and
// every file installed next to the disc image, each with its size and XXH3-128. These are the values
// the build manifest in spec/builds/ records.
internal static class CdInventoryReport
{
    public static string Build(string install, string imagePath, string cuePath, Iso9660 iso, IReadOnlyList<IsoFile> files, int sectors)
    {
        var manifest = new StringBuilder();
        manifest.AppendLine("# Original CD inventory (generated; do not redistribute artifacts)");
        manifest.AppendLine("# Hashes are XXH3-128 (xxhsum -H2), the form spec/builds/ manifests use.");
        manifest.AppendLine($"# Source image XXH3-128: {ResourceHash.Xxh3(imagePath)}");
        manifest.AppendLine($"# Data sectors: {sectors}");
        manifest.AppendLine("#        Size  XXH3-128                          Path");
        foreach (var file in files) manifest.AppendLine($"{file.Size,12}  {ResourceHash.Xxh3(iso.ReadFile(file))}  CD:{file.Path}");
        // CD audio tracks, each hashed over its raw 2352-byte sectors from its INDEX 01 to the next track's.
        var tracks = CueSheet.Tracks(File.ReadAllLines(cuePath));
        using (var raw = File.Open(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            for (var index = 0; index < tracks.Length; index++)
            {
                var track = tracks[index];
                if (!track.Mode.Equals("AUDIO", StringComparison.OrdinalIgnoreCase)) continue;
                var endSector = index + 1 < tracks.Length ? tracks[index + 1].StartSector : checked((int)(raw.Length / CddaWave.BytesPerSector));
                var length = (long)(endSector - track.StartSector) * CddaWave.BytesPerSector;
                raw.Position = (long)track.StartSector * CddaWave.BytesPerSector;
                var trackHash = new XxHash128();
                var buffer = new byte[128 * 1024];
                for (var remaining = length; remaining > 0;)
                {
                    var read = raw.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
                    if (read == 0) throw new EndOfStreamException();
                    trackHash.Append(buffer.AsSpan(0, read));
                    remaining -= read;
                }
                manifest.AppendLine($"{length,12}  {Convert.ToHexStringLower(trackHash.GetCurrentHash())}  CD:track{track.Number:00}");
            }
        // Files installed next to the disc image, which the DOSBox configuration mounts as C:.
        foreach (var path in Directory.EnumerateFiles(install).Order(StringComparer.OrdinalIgnoreCase))
            if (!Path.GetFileName(path).Equals("game.gog", StringComparison.OrdinalIgnoreCase))
                manifest.AppendLine($"{new FileInfo(path).Length,12}  {ResourceHash.Xxh3(path)}  {Path.GetFileName(path)}");
        return manifest.ToString();
    }
}
