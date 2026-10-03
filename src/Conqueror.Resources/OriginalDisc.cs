using RefurbishedDinosaurs.Core.IO;
using System.IO.Hashing;
using System.Text;
using System.Text.Json;

namespace Conqueror.Resources;

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
        var temporary = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(target))!,
            $".{Path.GetFileName(target)}.{Guid.NewGuid():N}.tmp");
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
