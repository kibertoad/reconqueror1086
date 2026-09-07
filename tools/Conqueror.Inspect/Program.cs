using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
Console.WriteLine($"Indexed {files.Length} CD files and extracted {extracted} inspectable artifacts to {output}.");
return 0;

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

public sealed record CueTrack(int Number, string Mode, int StartSector);

public static class CueSheet
{
    public static int DataTrackSectors(IEnumerable<string> lines)
    {
        var nextTrack = false;
        foreach (var line in lines)
        {
            if (Regex.IsMatch(line, @"^\s*TRACK\s+02\s+", RegexOptions.IgnoreCase)) { nextTrack = true; continue; }
            if (!nextTrack) continue;
            var match = Regex.Match(line, @"INDEX\s+01\s+(\d+):(\d+):(\d+)", RegexOptions.IgnoreCase);
            if (match.Success)
                return (int.Parse(match.Groups[1].Value) * 60 + int.Parse(match.Groups[2].Value)) * 75 + int.Parse(match.Groups[3].Value);
        }
        throw new InvalidDataException("Could not locate track 2 start in cue sheet.");
    }

    public static CueTrack[] Tracks(IEnumerable<string> lines)
    {
        var result = new List<CueTrack>();
        var number = 0;
        var mode = "";
        foreach (var line in lines)
        {
            var track = Regex.Match(line, @"^\s*TRACK\s+(\d+)\s+(\S+)", RegexOptions.IgnoreCase);
            if (track.Success) { number = int.Parse(track.Groups[1].Value); mode = track.Groups[2].Value; continue; }
            var index = Regex.Match(line, @"INDEX\s+01\s+(\d+):(\d+):(\d+)", RegexOptions.IgnoreCase);
            if (number > 0 && index.Success)
            {
                var sector = (int.Parse(index.Groups[1].Value) * 60 + int.Parse(index.Groups[2].Value)) * 75 + int.Parse(index.Groups[3].Value);
                result.Add(new CueTrack(number, mode, sector));
                number = 0;
            }
        }
        return result.ToArray();
    }
}

public sealed class RawMode1Image : IDisposable
{
    private const int RawSector = 2352;
    private const int PayloadOffset = 16;
    private const int PayloadSize = 2048;
    private readonly FileStream _stream;
    public int SectorCount { get; }

    public RawMode1Image(string path, int sectorCount)
    {
        _stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        SectorCount = sectorCount;
    }

    public byte[] Read(long offset, int count)
    {
        if (offset < 0 || count < 0 || offset + count > (long)SectorCount * PayloadSize) throw new EndOfStreamException();
        var result = new byte[count];
        var written = 0;
        while (written < count)
        {
            var logical = offset + written;
            var sector = logical / PayloadSize;
            var within = (int)(logical % PayloadSize);
            var take = Math.Min(count - written, PayloadSize - within);
            _stream.Position = sector * RawSector + PayloadOffset + within;
            _stream.ReadExactly(result.AsSpan(written, take));
            written += take;
        }
        return result;
    }

    public void Dispose() => _stream.Dispose();
}

public sealed record IsoFile(string Path, uint Extent, uint Size);

public sealed class Iso9660
{
    private const int Sector = 2048;
    private readonly RawMode1Image _image;
    private readonly List<IsoFile> _files = [];
    public IReadOnlyList<IsoFile> Files => _files;

    public Iso9660(RawMode1Image image)
    {
        _image = image;
        var descriptor = image.Read(16L * Sector, Sector);
        if (descriptor[0] != 1 || Encoding.ASCII.GetString(descriptor, 1, 5) != "CD001") throw new InvalidDataException("Track 1 is not an ISO-9660 primary volume.");
        var root = ParseRecord(descriptor.AsSpan(156), "");
        ReadDirectory(root, "", new HashSet<uint>());
    }

    public byte[] ReadFile(IsoFile file) => _image.Read((long)file.Extent * Sector, checked((int)file.Size));

    private void ReadDirectory(IsoFile directory, string prefix, HashSet<uint> visited)
    {
        if (!visited.Add(directory.Extent)) return;
        var bytes = ReadFile(directory);
        for (var offset = 0; offset < bytes.Length;)
        {
            var length = bytes[offset];
            if (length == 0) { offset = (offset / Sector + 1) * Sector; continue; }
            if (offset + length > bytes.Length) break;
            var record = bytes.AsSpan(offset, length);
            var nameLength = record[32];
            var rawName = Encoding.ASCII.GetString(record.Slice(33, nameLength));
            offset += length;
            if (rawName is "\0" or "\u0001") continue;
            var name = rawName.Split(';')[0].TrimEnd('.');
            var entry = ParseRecord(record, prefix.Length == 0 ? name : $"{prefix}/{name}");
            if ((record[25] & 2) != 0) ReadDirectory(entry, entry.Path, visited); else _files.Add(entry);
        }
    }

    private static IsoFile ParseRecord(ReadOnlySpan<byte> record, string path) => new(
        path,
        BinaryPrimitives.ReadUInt32LittleEndian(record.Slice(2, 4)),
        BinaryPrimitives.ReadUInt32LittleEndian(record.Slice(10, 4)));
}
