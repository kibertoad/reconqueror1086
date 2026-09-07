using Conqueror.Resources;
using System.Security.Cryptography;
using System.Text;

try
{
var install = args.Length > 0 ? Path.GetFullPath(args[0]) : @"C:\GOG Games\Conqueror AD1086";
var output = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.GetFullPath(Path.Combine("analysis", "original"));
var imagePath = Path.Combine(install, "game.gog");
var cuePath = Path.Combine(install, "game.ins");
if (!File.Exists(imagePath) || !File.Exists(cuePath))
{
    Console.Error.WriteLine("Expected game.gog and game.ins in the supplied installation directory.");
    return 2;
}

Directory.CreateDirectory(output);
var sectors = CueSheet.DataTrackSectors(File.ReadAllLines(cuePath));
using var image = new RawMode1Image(imagePath, sectors);
var iso = new Iso9660(image);
var files = iso.Files.OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase).ToArray();
var manifest = new StringBuilder();
manifest.AppendLine("# Original CD inventory (generated; do not redistribute artifacts)");
manifest.AppendLine($"# Source image SHA-256: {Hash(imagePath)}");
manifest.AppendLine($"# Data sectors: {sectors}");
foreach (var file in files) manifest.AppendLine($"{file.Size,12}  {file.Path}");
File.WriteAllText(Path.Combine(output, "cd-manifest.txt"), manifest.ToString());

var artifactRoot = Path.Combine(output, "artifacts");
var interesting = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".EXE", ".COM", ".INI", ".CFG", ".DAT", ".TXT" };
var extracted = 0;
foreach (var file in files.Where(x => interesting.Contains(Path.GetExtension(x.Path)) && x.Size <= 16 * 1024 * 1024))
{
    var relative = string.Join(Path.DirectorySeparatorChar, file.Path.Split('/').Select(SafeName));
    var target = Path.GetFullPath(Path.Combine(artifactRoot, relative));
    if (!target.StartsWith(Path.GetFullPath(artifactRoot) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)) continue;
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    File.WriteAllBytes(target, iso.ReadFile(file));
    extracted++;
}

var report = new StringBuilder();
report.AppendLine("# Extracted artifact hashes");
foreach (var path in Directory.EnumerateFiles(artifactRoot, "*", SearchOption.AllDirectories).Order())
    report.AppendLine($"{Hash(path)}  {Path.GetRelativePath(artifactRoot, path)}  {new FileInfo(path).Length}");
File.WriteAllText(Path.Combine(output, "artifact-hashes.txt"), report.ToString());
var terms = args.Skip(2).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
if (terms.Length > 0)
{
    var hits = new StringBuilder("# Printable-string hits (offset, source, text)\n");
    var scanTargets = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories)
        .Concat([Path.Combine(install, "C1086.GOB")]).Where(File.Exists);
    foreach (var target in scanTargets)
        foreach (var (offset, value) in PrintableStrings(target).Where(x => terms.Any(term => x.Value.Contains(term, StringComparison.OrdinalIgnoreCase))))
            hits.AppendLine($"0x{offset:X8}  {Path.GetFileName(target)}  {value}");
    File.WriteAllText(Path.Combine(output, "string-hits.txt"), hits.ToString());
}
var gobEntries = 0;
var gobStoredEntries = 0;
var gobPath = Path.Combine(install, "C1086.GOB");
if (File.Exists(gobPath))
{
    var gob = new DynamixArchive(gobPath);
    gobEntries = gob.Entries.Count;
    gobStoredEntries = gob.Entries.Count(x => x.IsStored);
    var directory = new StringBuilder("# Index  Flags  Stored  Expanded  Offset  Name\n");
    foreach (var entry in gob.Entries)
        directory.AppendLine($"{entry.Index,5}  {entry.Flags,5}  {entry.StoredSize,10}  {entry.ExpandedSize,10}  0x{entry.Offset:X8}  {entry.Name}");
    File.WriteAllText(Path.Combine(output, "gob-directory.txt"), directory.ToString());
}
Console.WriteLine($"Indexed {files.Length} CD files, {gobEntries} GOB entries ({gobStoredEntries} stored), and extracted {extracted} inspectable artifacts to {output}.");
return 0;
}
catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException or OverflowException)
{
    Console.Error.WriteLine($"Original-data inspection failed: {error.Message}");
    return 1;
}

static string SafeName(string name) => string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-'));
static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static IEnumerable<(long Offset, string Value)> PrintableStrings(string path)
{
    using var stream = File.OpenRead(path);
    var bytes = new List<byte>();
    long start = 0;
    for (long offset = 0; offset < stream.Length; offset++)
    {
        var value = stream.ReadByte();
        if (value is >= 32 and <= 126)
        {
            if (bytes.Count == 0) start = offset;
            bytes.Add((byte)value);
        }
        else
        {
            if (bytes.Count >= 4) yield return (start, Encoding.ASCII.GetString(bytes.ToArray()));
            bytes.Clear();
        }
    }
    if (bytes.Count >= 4) yield return (start, Encoding.ASCII.GetString(bytes.ToArray()));
}
