using System.Security.Cryptography;
using System.Text.Json;

var install = args.Length > 0 ? Path.GetFullPath(args[0]) : @"C:\GOG Games\Conqueror AD1086";
var output = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.GetFullPath("UserContent");
var imagePath = Path.Combine(install, "game.gog");
var cuePath = Path.Combine(install, "game.ins");
var gobPath = Path.Combine(install, "C1086.GOB");
if (!File.Exists(imagePath) || !File.Exists(cuePath) || !File.Exists(gobPath))
{
    Console.Error.WriteLine("A complete GOG installation with game.gog, game.ins, and C1086.GOB is required.");
    return 2;
}

Directory.CreateDirectory(output);
var entries = new List<ImportedAsset>();
var cueLines = File.ReadAllLines(cuePath);
using (var image = new RawMode1Image(imagePath, CueSheet.DataTrackSectors(cueLines)))
{
    var iso = new Iso9660(image);
    var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".SMK", ".RES", ".CSF", ".PCX", ".PCC", ".LOW", ".WAV", ".MID" };
    foreach (var file in iso.Files.Where(x => supported.Contains(Path.GetExtension(x.Path))))
    {
        var relative = Path.Combine("Raw", string.Join(Path.DirectorySeparatorChar, file.Path.Split('/').Select(SafeName)));
        var target = SafeTarget(output, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        File.WriteAllBytes(target, iso.ReadFile(file));
        entries.Add(NewEntry(file.Path, relative, Kind(file.Path), target));
    }
}

var archiveRelative = Path.Combine("Archives", "C1086.GOB");
var archiveTarget = SafeTarget(output, archiveRelative);
Directory.CreateDirectory(Path.GetDirectoryName(archiveTarget)!);
File.Copy(gobPath, archiveTarget, true);
entries.Add(NewEntry("C1086.GOB", archiveRelative, "archive", archiveTarget));

var tracks = CueSheet.Tracks(cueLines);
using (var source = File.Open(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
for (var index = 0; index < tracks.Length; index++)
{
    var track = tracks[index];
    if (!track.Mode.Equals("AUDIO", StringComparison.OrdinalIgnoreCase)) continue;
    var endSector = index + 1 < tracks.Length ? tracks[index + 1].StartSector : checked((int)(source.Length / 2352));
    var relative = Path.Combine("Audio", $"track{track.Number:00}.wav");
    var target = SafeTarget(output, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    WriteWave(source, target, track.StartSector, endSector - track.StartSector);
    entries.Add(NewEntry($"CDDA/TRACK{track.Number:00}", relative, "audio", target));
}

var manifest = new ImportManifest(1, Hash(imagePath), entries.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToArray());
File.WriteAllText(Path.Combine(output, "manifest.json"), JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true }));
Console.WriteLine($"Installed {entries.Count} owned resources into {output}.");
return 0;

static string Kind(string path) => Path.GetExtension(path).ToUpperInvariant() switch
{
    ".SMK" => "movie", ".WAV" or ".MID" => "audio", ".PCX" or ".PCC" => "image", ".CSF" => "dialogue", _ => "resource"
};
static string SafeName(string name) => string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-'));
static string SafeTarget(string root, string relative)
{
    var fullRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
    var target = Path.GetFullPath(Path.Combine(root, relative));
    if (!target.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unsafe archive path.");
    return target;
}
static ImportedAsset NewEntry(string id, string relative, string kind, string path) => new(id.Replace('\\', '/'), relative.Replace('\\', '/'), kind, new FileInfo(path).Length, Hash(path));
static string Hash(string path) { using var stream = File.OpenRead(path); return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant(); }
static void WriteWave(FileStream source, string target, int startSector, int sectorCount)
{
    const int bytesPerSector = 2352;
    var dataLength = checked(sectorCount * bytesPerSector);
    using var output = File.Create(target);
    using var writer = new BinaryWriter(output, System.Text.Encoding.ASCII, true);
    writer.Write("RIFF"u8); writer.Write(dataLength + 36); writer.Write("WAVEfmt "u8); writer.Write(16);
    writer.Write((short)1); writer.Write((short)2); writer.Write(44100); writer.Write(44100 * 4); writer.Write((short)4); writer.Write((short)16);
    writer.Write("data"u8); writer.Write(dataLength);
    source.Position = (long)startSector * bytesPerSector;
    var remaining = dataLength; var buffer = new byte[128 * 1024];
    while (remaining > 0) { var read = source.Read(buffer, 0, Math.Min(buffer.Length, remaining)); if (read == 0) throw new EndOfStreamException(); output.Write(buffer, 0, read); remaining -= read; }
}

sealed record ImportManifest(int Version, string SourceImageSha256, ImportedAsset[] Assets);
sealed record ImportedAsset(string Id, string Path, string Kind, long Size, string Sha256);
