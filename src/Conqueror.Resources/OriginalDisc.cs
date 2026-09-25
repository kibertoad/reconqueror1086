using System.Buffers.Binary;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Conqueror.Resources;

public sealed record CueTrack(int Number, string Mode, int StartSector);
public sealed record IsoFile(string Path, uint Extent, uint Size);
public sealed record ImportedAsset(string Id, string Path, string Kind, long Size, string Xxh3);

public static class SupportedOriginalReleases
{
    public const string GogEnglishSourceImageXxh3 = "b915491c5bdce934ca216d2ceebec0ce";

    public static string? NameForSourceImage(string xxh3) => xxh3.Equals(
        GogEnglishSourceImageXxh3, StringComparison.OrdinalIgnoreCase) ? "GOG English release" : null;
}

public sealed record ImportManifest(int Version, string SourceImageXxh3, ImportedAsset[] Assets)
{
    // Version 2 identifies files by XXH3-128. A manifest of any other version fails verification,
    // so an older local import is re-imported rather than misread.
    public const int CurrentVersion = 2;

    public static ImportManifest Read(string path) => JsonSerializer.Deserialize<ImportManifest>(File.ReadAllText(path))
        ?? throw new InvalidDataException("Invalid imported-content manifest.");

    public void Write(string path) => AtomicFile.WriteAllText(path,
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
}

public sealed record InstalledFile(string Path, long Size, string Xxh3, bool Changed);
public sealed record PlannedImportAsset(string Path, long Size);
public sealed record ImportDiskPlan(long InstalledBytes, long NewBytes, long ReplacementScratchBytes)
{
    public long RequiredAvailableBytes => checked(NewBytes + ReplacementScratchBytes);
}

public static class ImportDiskPlanner
{
    public static ImportDiskPlan Calculate(string root, IEnumerable<PlannedImportAsset> assets)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(assets);
        long installed = 0;
        long newBytes = 0;
        long replacementScratch = 0;
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var asset in assets)
        {
            if (asset.Size < 0) throw new InvalidDataException("Planned asset has a negative size.");
            if (Path.IsPathFullyQualified(asset.Path)) throw new InvalidDataException("Planned asset path must be relative.");
            var target = ResourcePaths.SafeTarget(root, asset.Path);
            if (!paths.Add(target)) throw new InvalidDataException("Planned asset paths are not unique.");
            installed = checked(installed + asset.Size);
            if (File.Exists(target)) replacementScratch = Math.Max(replacementScratch, asset.Size);
            else newBytes = checked(newBytes + asset.Size);
        }
        return new(installed, newBytes, replacementScratch);
    }
}

public static class GeneratedContentInstaller
{
    public static InstalledFile InstallBytes(string root, string relative, ReadOnlySpan<byte> bytes)
    {
        var target = ResourcePaths.SafeTarget(root, relative);
        var hash = ResourceHash.Xxh3(bytes);
        if (Matches(target, bytes.Length, hash)) return new(target, bytes.Length, hash, false);
        AtomicFile.WriteBytes(target, bytes);
        return new(target, bytes.Length, hash, true);
    }

    public static InstalledFile InstallFile(string root, string relative, string source)
    {
        var target = ResourcePaths.SafeTarget(root, relative);
        var info = new FileInfo(source);
        var hash = ResourceHash.Xxh3(source);
        if (Matches(target, info.Length, hash)) return new(target, info.Length, hash, false);
        AtomicFile.Copy(source, target);
        return new(target, info.Length, hash, true);
    }

    public static InstalledFile InstallGenerated(string root, string relative, Action<Stream> write)
    {
        ArgumentNullException.ThrowIfNull(write);
        var target = ResourcePaths.SafeTarget(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var temporary = AtomicFile.TemporaryPath(target);
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       128 * 1024, FileOptions.WriteThrough))
            {
                write(stream);
                stream.Flush(flushToDisk: true);
            }
            var info = new FileInfo(temporary);
            var hash = ResourceHash.Xxh3(temporary);
            if (Matches(target, info.Length, hash)) return new(target, info.Length, hash, false);
            File.Move(temporary, target, overwrite: true);
            return new(target, info.Length, hash, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    private static bool Matches(string path, long size, string hash) => File.Exists(path)
        && new FileInfo(path).Length == size
        && ResourceHash.Xxh3(path).Equals(hash, StringComparison.OrdinalIgnoreCase);
}

public static class ImportedContentUninstaller
{
    public static int Remove(string root, ImportManifest manifest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(manifest);
        var fullRoot = Path.GetFullPath(root);
        var targets = (manifest.Assets ?? []).Select(asset => asset?.Path
                ?? throw new InvalidDataException("Manifest contains a null asset record."))
            .Select(relative => Path.IsPathFullyQualified(relative)
                ? throw new InvalidDataException("Manifest contains an absolute asset path.")
                : ResourcePaths.SafeTarget(fullRoot, relative))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var removed = 0;
        foreach (var target in targets)
            if (File.Exists(target))
            {
                File.Delete(target);
                removed++;
            }
        var manifestPath = ResourcePaths.SafeTarget(fullRoot, "manifest.json");
        if (File.Exists(manifestPath)) File.Delete(manifestPath);
        if (Directory.Exists(fullRoot))
            foreach (var directory in Directory.EnumerateDirectories(fullRoot, "*", SearchOption.AllDirectories)
                         .OrderByDescending(path => path.Length))
                if (!Directory.EnumerateFileSystemEntries(directory).Any()) Directory.Delete(directory);
        return removed;
    }
}

public static class AtomicFile
{
    public static void WriteAllText(string path, string contents) => WriteBytes(path,
        new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(contents));

    public static void WriteBytes(string path, ReadOnlySpan<byte> contents)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = TemporaryPath(fullPath);
        try
        {
            using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                       128 * 1024, FileOptions.WriteThrough))
            {
                stream.Write(contents);
                stream.Flush(flushToDisk: true);
            }
            File.Move(temporary, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public static void Copy(string source, string destination)
    {
        var fullPath = Path.GetFullPath(destination);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var temporary = TemporaryPath(fullPath);
        try
        {
            File.Copy(source, temporary, overwrite: false);
            using (var stream = File.Open(temporary, FileMode.Open, FileAccess.Write, FileShare.None))
                stream.Flush(flushToDisk: true);
            File.Move(temporary, fullPath, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    internal static string TemporaryPath(string destination) => Path.Combine(
        Path.GetDirectoryName(Path.GetFullPath(destination))!, $".{Path.GetFileName(destination)}.{Guid.NewGuid():N}.tmp");
}

public sealed record ImportVerificationIssue(string AssetId, string Path, string Reason);

public sealed record ImportVerificationResult(int CheckedAssets, IReadOnlyList<ImportVerificationIssue> Issues)
{
    public bool IsValid => Issues.Count == 0;
}

public static class ImportManifestVerifier
{
    public static ImportVerificationResult Verify(string root, ImportManifest manifest)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        ArgumentNullException.ThrowIfNull(manifest);
        var issues = new List<ImportVerificationIssue>();
        var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (manifest.Version != ImportManifest.CurrentVersion)
            issues.Add(new("manifest", "manifest.json", $"unsupported manifest version {manifest.Version}"));
        if (!ResourceHash.IsXxh3(manifest.SourceImageXxh3))
            issues.Add(new("manifest", "manifest.json", "invalid source-image XXH3"));

        var assets = manifest.Assets ?? [];
        foreach (var asset in assets)
        {
            if (asset is null)
            {
                issues.Add(new("(null)", "", "null asset record"));
                continue;
            }
            var assetId = asset.Id ?? "";
            var assetPath = asset.Path ?? "";
            if (string.IsNullOrWhiteSpace(assetId) || !ids.Add(assetId))
                issues.Add(new(assetId, assetPath, "empty or duplicate asset identifier"));
            if (string.IsNullOrWhiteSpace(assetPath) || Path.IsPathFullyQualified(assetPath) || !paths.Add(assetPath))
            {
                issues.Add(new(assetId, assetPath, "empty, absolute, or duplicate asset path"));
                continue;
            }
            if (asset.Size < 0 || !ResourceHash.IsXxh3(asset.Xxh3))
            {
                issues.Add(new(assetId, assetPath, "invalid declared size or XXH3"));
                continue;
            }

            string target;
            try { target = ResourcePaths.SafeTarget(root, assetPath); }
            catch (InvalidDataException)
            {
                issues.Add(new(assetId, assetPath, "path escapes the imported-content root"));
                continue;
            }
            if (!File.Exists(target))
            {
                issues.Add(new(assetId, assetPath, "file is missing"));
                continue;
            }
            var info = new FileInfo(target);
            if (info.Length != asset.Size)
                issues.Add(new(assetId, assetPath, $"size mismatch: expected {asset.Size}, found {info.Length}"));
            else if (!ResourceHash.Xxh3(target).Equals(asset.Xxh3, StringComparison.OrdinalIgnoreCase))
                issues.Add(new(assetId, assetPath, "XXH3 mismatch"));
        }
        return new(assets.Length, issues);
    }
}

public static class ResourcePaths
{
    public static string SafeName(string name) => string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-'));

    public static string DecodedArchiveFolder(string archiveId)
    {
        ArgumentNullException.ThrowIfNull(archiveId);
        var folder = SafeName(Path.GetFileName(archiveId));
        return string.IsNullOrWhiteSpace(folder) || folder is "." or ".."
            ? throw new InvalidDataException("Archive identifier has no safe filename.")
            : folder;
    }

    public static string SafeTarget(string root, string relative)
    {
        var fullRoot = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        var target = Path.GetFullPath(Path.Combine(root, relative));
        if (!target.StartsWith(fullRoot, StringComparison.OrdinalIgnoreCase)) throw new InvalidDataException("Unsafe resource path.");
        return target;
    }
}

// The 128-bit form of xxHash3 (XXH3_128bits, which `xxhsum -H2` prints), written as 32 lower-case hex
// digits in the byte order of its canonical form. The documentation standard names every original
// file by the same hash, so the build manifest in spec/builds/ and the import manifest agree.
public static class ResourceHash
{
    public const int Xxh3HexLength = 32;

    public static string Xxh3(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            128 * 1024, FileOptions.SequentialScan);
        return Xxh3(stream);
    }

    public static string Xxh3(Stream stream)
    {
        var hash = new XxHash128();
        hash.Append(stream);
        return Convert.ToHexStringLower(hash.GetCurrentHash());
    }

    public static string Xxh3(ReadOnlySpan<byte> bytes) => Convert.ToHexStringLower(XxHash128.Hash(bytes));

    public static bool IsXxh3(string? value) => value is { Length: Xxh3HexLength }
        && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F');
}

public static class CueSheet
{
    public static int DataTrackSectors(IEnumerable<string> lines)
    {
        var tracks = Tracks(lines);
        if (tracks.Length < 2 || !tracks[0].Mode.StartsWith("MODE1", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("Cue sheet does not begin with a MODE1 data track followed by another track.");
        return tracks[1].StartSector;
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
                var sector = checked((int.Parse(index.Groups[1].Value) * 60 + int.Parse(index.Groups[2].Value)) * 75 + int.Parse(index.Groups[3].Value));
                result.Add(new CueTrack(number, mode, sector));
                number = 0;
            }
        }
        if (result.Count == 0) throw new InvalidDataException("Cue sheet contains no indexed tracks.");
        return result.ToArray();
    }
}

public static class CddaWave
{
    public const int BytesPerSector = 2352;

    public static void Write(Stream source, Stream output, int startSector, int sectorCount)
    {
        if (!source.CanSeek || !source.CanRead || !output.CanWrite || startSector < 0 || sectorCount < 0) throw new ArgumentException("Invalid CDDA streams or sector range.");
        var dataLength = checked(sectorCount * BytesPerSector);
        using var writer = new BinaryWriter(output, Encoding.ASCII, true);
        writer.Write("RIFF"u8); writer.Write(dataLength + 36); writer.Write("WAVEfmt "u8); writer.Write(16);
        writer.Write((short)1); writer.Write((short)2); writer.Write(44100); writer.Write(44100 * 4); writer.Write((short)4); writer.Write((short)16);
        writer.Write("data"u8); writer.Write(dataLength);
        source.Position = (long)startSector * BytesPerSector;
        var remaining = dataLength;
        var buffer = new byte[128 * 1024];
        while (remaining > 0)
        {
            var read = source.Read(buffer, 0, Math.Min(buffer.Length, remaining));
            if (read == 0) throw new EndOfStreamException();
            output.Write(buffer, 0, read);
            remaining -= read;
        }
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
            if (length < 34 || offset + length > bytes.Length) throw new InvalidDataException("Invalid ISO directory record.");
            var record = bytes.AsSpan(offset, length);
            var nameLength = record[32];
            if (33 + nameLength > record.Length) throw new InvalidDataException("Invalid ISO filename length.");
            var rawName = Encoding.ASCII.GetString(record.Slice(33, nameLength));
            offset += length;
            if (rawName is "\0" or "\u0001") continue;
            var name = rawName.Split(';')[0].TrimEnd('.');
            var entry = ParseRecord(record, prefix.Length == 0 ? name : $"{prefix}/{name}");
            if ((record[25] & 2) != 0) ReadDirectory(entry, entry.Path, visited); else _files.Add(entry);
        }
    }

    private static IsoFile ParseRecord(ReadOnlySpan<byte> record, string path) => new(path,
        BinaryPrimitives.ReadUInt32LittleEndian(record.Slice(2, 4)),
        BinaryPrimitives.ReadUInt32LittleEndian(record.Slice(10, 4)));
}
