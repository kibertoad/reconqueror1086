using Conqueror.Resources;

try
{
var operation = args.FirstOrDefault()?.ToLowerInvariant();
if (operation == "--verify")
{
    var verifyRoot = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.GetFullPath("UserContent");
    return VerifyInstalledContent(verifyRoot);
}
if (operation == "--uninstall")
{
    var uninstallRoot = args.Length > 1 ? Path.GetFullPath(args[1]) : Path.GetFullPath("UserContent");
    return UninstallContent(uninstallRoot);
}
var positionalOffset = operation == "--repair" ? 1 : 0;
var install = args.Length > positionalOffset ? Path.GetFullPath(args[positionalOffset]) : @"C:\GOG Games\Conqueror AD1086";
var output = args.Length > positionalOffset + 1 ? Path.GetFullPath(args[positionalOffset + 1]) : Path.GetFullPath("UserContent");
var imagePath = Path.Combine(install, "game.gog");
var cuePath = Path.Combine(install, "game.ins");
var gobPath = Path.Combine(install, "C1086.GOB");
if (!File.Exists(imagePath) || !File.Exists(cuePath) || !File.Exists(gobPath))
{
    Console.Error.WriteLine("A complete GOG installation with game.gog, game.ins, and C1086.GOB is required.");
    return 2;
}
var sourceImageHash = ResourceHash.Xxh3(imagePath);
var releaseName = SupportedOriginalReleases.NameForSourceImage(sourceImageHash);
if (releaseName is null)
    Console.Error.WriteLine($"WARNING: unrecognized source image XXH3-128 {sourceImageHash}; bounded validation will continue.");
else
    Console.WriteLine($"Recognized {releaseName} ({sourceImageHash}).");

Directory.CreateDirectory(output);
var cueLines = File.ReadAllLines(cuePath);
var diskPlan = PlanInstallation(imagePath, cueLines, gobPath, output);
var driveRoot = Path.GetPathRoot(output) ?? throw new InvalidDataException("Output path has no filesystem root.");
var availableBytes = new DriveInfo(driveRoot).AvailableFreeSpace;
Console.WriteLine($"Planned {diskPlan.InstalledBytes / 1048576d:F1} MiB installed; {diskPlan.RequiredAvailableBytes / 1048576d:F1} MiB additional/scratch required; {availableBytes / 1048576d:F1} MiB available.");
if (availableBytes < diskPlan.RequiredAvailableBytes)
{
    Console.Error.WriteLine("Insufficient free space for an atomic resource installation.");
    return 4;
}
var entries = new List<ImportedAsset>();
var changedFiles = 0;
using (var image = new RawMode1Image(imagePath, CueSheet.DataTrackSectors(cueLines)))
{
    var iso = new Iso9660(image);
    foreach (var file in iso.Files.Where(file => IsSupportedDiscFile(file.Path)))
    {
        var relative = Path.Combine("Raw", string.Join(Path.DirectorySeparatorChar, file.Path.Split('/').Select(ResourcePaths.SafeName)));
        var target = ResourcePaths.SafeTarget(output, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var installed = GeneratedContentInstaller.InstallBytes(output, relative, iso.ReadFile(file));
        if (installed.Changed) changedFiles++;
        target = installed.Path;
        entries.Add(NewEntry(file.Path, relative, Kind(file.Path), target));
        ReportProgress(entries, changedFiles, file.Path);
        if (DynamixArchive.HasContainerExtension(file.Path))
            changedFiles += InstallDecodedEntries(new DynamixArchive(target), file.Path, output, entries, changedFiles);
    }
}

var archiveRelative = Path.Combine("Archives", "C1086.GOB");
var archiveTarget = ResourcePaths.SafeTarget(output, archiveRelative);
Directory.CreateDirectory(Path.GetDirectoryName(archiveTarget)!);
var installedArchive = GeneratedContentInstaller.InstallFile(output, archiveRelative, gobPath);
if (installedArchive.Changed) changedFiles++;
archiveTarget = installedArchive.Path;
entries.Add(NewEntry("C1086.GOB", archiveRelative, "archive", archiveTarget));
ReportProgress(entries, changedFiles, "C1086.GOB");
changedFiles += InstallDecodedEntries(new DynamixArchive(archiveTarget), "C1086.GOB", output, entries, changedFiles);

var tracks = CueSheet.Tracks(cueLines);
using (var source = File.Open(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read))
for (var index = 0; index < tracks.Length; index++)
{
    var track = tracks[index];
    if (!track.Mode.Equals("AUDIO", StringComparison.OrdinalIgnoreCase)) continue;
    var endSector = index + 1 < tracks.Length ? tracks[index + 1].StartSector : checked((int)(source.Length / 2352));
    var relative = Path.Combine("Audio", $"track{track.Number:00}.wav");
    var target = ResourcePaths.SafeTarget(output, relative);
    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
    var installedTrack = GeneratedContentInstaller.InstallGenerated(output, relative,
        wave => CddaWave.Write(source, wave, track.StartSector, endSector - track.StartSector));
    if (installedTrack.Changed) changedFiles++;
    target = installedTrack.Path;
    entries.Add(NewEntry($"CDDA/TRACK{track.Number:00}", relative, "audio", target));
    ReportProgress(entries, changedFiles, $"CDDA/TRACK{track.Number:00}");
}

var manifest = new ImportManifest(ImportManifest.CurrentVersion, sourceImageHash, entries.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToArray());
manifest.Write(Path.Combine(output, "manifest.json"));
var verification = ImportManifestVerifier.Verify(output, manifest);
if (!verification.IsValid)
{
    foreach (var issue in verification.Issues.Take(20))
        Console.Error.WriteLine($"{issue.AssetId}: {issue.Reason} ({issue.Path})");
    Console.Error.WriteLine($"Installation verification failed with {verification.Issues.Count} issue(s).");
    return 3;
}
Console.WriteLine($"{(operation == "--repair" ? "Repaired" : "Installed")} and verified {entries.Count} owned resources in {output}: {changedFiles} written, {entries.Count - changedFiles} reused.");
return 0;
}

catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException or OverflowException or System.Text.Json.JsonException)
{
    Console.Error.WriteLine($"Resource installation failed: {error.Message}");
    return 1;
}

static int VerifyInstalledContent(string root)
{
    var manifestPath = Path.Combine(root, "manifest.json");
    if (!File.Exists(manifestPath))
    {
        Console.Error.WriteLine($"No imported-content manifest exists in {root}.");
        return 2;
    }
    var manifest = ImportManifest.Read(manifestPath);
    var result = ImportManifestVerifier.Verify(root, manifest);
    foreach (var issue in result.Issues.Take(20))
        Console.Error.WriteLine($"{issue.AssetId}: {issue.Reason} ({issue.Path})");
    if (!result.IsValid)
    {
        Console.Error.WriteLine($"Verification failed: {result.Issues.Count} issue(s) across {result.CheckedAssets} assets.");
        return 3;
    }
    Console.WriteLine($"Verified {result.CheckedAssets} imported assets in {root}.");
    return 0;
}

static int UninstallContent(string root)
{
    var manifestPath = Path.Combine(root, "manifest.json");
    if (!File.Exists(manifestPath))
    {
        Console.Error.WriteLine($"No imported-content manifest exists in {root}; nothing was removed.");
        return 2;
    }
    var manifest = ImportManifest.Read(manifestPath);
    var removed = ImportedContentUninstaller.Remove(root, manifest);
    Console.WriteLine($"Removed {removed} manifest-owned files from {root}. Unlisted files were preserved.");
    return 0;
}

static ImportDiskPlan PlanInstallation(string imagePath, IReadOnlyList<string> cueLines, string gobPath, string output)
{
    var assets = new List<PlannedImportAsset>();
    using (var image = new RawMode1Image(imagePath, CueSheet.DataTrackSectors(cueLines)))
    {
        var iso = new Iso9660(image);
        foreach (var file in iso.Files.Where(file => IsSupportedDiscFile(file.Path)))
        {
            var relative = Path.Combine("Raw", string.Join(Path.DirectorySeparatorChar,
                file.Path.Split('/').Select(ResourcePaths.SafeName)));
            assets.Add(new(relative, file.Size));
            if (!DynamixArchive.HasContainerExtension(file.Path)) continue;
            var archive = new DynamixArchive(iso.ReadFile(file), file.Path);
            foreach (var entry in archive.Entries.Where(DynamixArchive.CanDecode))
                assets.Add(new(Path.Combine("Decoded", ResourcePaths.DecodedArchiveFolder(file.Path),
                    $"{entry.Index:0000}-{ResourcePaths.SafeName(entry.Name)}"), entry.ExpandedSize));
        }
    }

    assets.Add(new(Path.Combine("Archives", "C1086.GOB"), new FileInfo(gobPath).Length));
    var gob = new DynamixArchive(gobPath);
    foreach (var entry in gob.Entries.Where(DynamixArchive.CanDecode))
        assets.Add(new(Path.Combine("Decoded", ResourcePaths.DecodedArchiveFolder("C1086.GOB"),
            $"{entry.Index:0000}-{ResourcePaths.SafeName(entry.Name)}"), entry.ExpandedSize));

    var tracks = CueSheet.Tracks(cueLines);
    var imageSectors = new FileInfo(imagePath).Length / CddaWave.BytesPerSector;
    for (var index = 0; index < tracks.Length; index++)
    {
        var track = tracks[index];
        if (!track.Mode.Equals("AUDIO", StringComparison.OrdinalIgnoreCase)) continue;
        var endSector = index + 1 < tracks.Length ? tracks[index + 1].StartSector : imageSectors;
        assets.Add(new(Path.Combine("Audio", $"track{track.Number:00}.wav"),
            checked(44L + (endSector - track.StartSector) * CddaWave.BytesPerSector)));
    }
    return ImportDiskPlanner.Calculate(output, assets);
}

static bool IsSupportedDiscFile(string path) => Path.GetExtension(path).ToUpperInvariant() is
    ".SMK" or ".RES" or ".CSF" or ".PCX" or ".PCC" or ".LOW" or ".WAV" or ".MID";

static string Kind(string path) => Path.GetExtension(path).ToUpperInvariant() switch
{
    ".SMK" => "movie", ".WAV" or ".MID" => "audio", ".PCX" or ".PCC" => "image", _ => "resource"
};
static ImportedAsset NewEntry(string id, string relative, string kind, string path) => new(id.Replace('\\', '/'), relative.Replace('\\', '/'), kind, new FileInfo(path).Length, ResourceHash.Xxh3(path));

static string DecodedKind(string name, string path)
{
    if (Path.GetExtension(name).Equals(".CSF", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            _ = new CsfSequence(File.ReadAllBytes(path));
            return "indexed-animation";
        }
        catch (InvalidDataException) { }
    }
    if (Path.GetExtension(name).Equals(".PAL", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            _ = IndexedPaletteDecoder.Decode(File.ReadAllBytes(path));
            return "palette";
        }
        catch (InvalidDataException) { }
    }
    if (Path.GetExtension(name).Equals(".666", StringComparison.OrdinalIgnoreCase))
    {
        try
        {
            _ = DynamixSoundBankDecoder.Decode(File.ReadAllBytes(path));
            return "sound-bank";
        }
        catch (InvalidDataException) { }
    }
    return Kind(name);
}

static int InstallDecodedEntries(DynamixArchive archive, string archiveId, string output, List<ImportedAsset> entries, int changedBefore)
{
    var changed = 0;
    foreach (var entry in archive.Entries.Where(DynamixArchive.CanDecode))
    {
        var folder = ResourcePaths.DecodedArchiveFolder(archiveId);
        var relative = Path.Combine("Decoded", folder, $"{entry.Index:0000}-{ResourcePaths.SafeName(entry.Name)}");
        var target = ResourcePaths.SafeTarget(output, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var installed = GeneratedContentInstaller.InstallBytes(output, relative, archive.ReadDecoded(entry));
        if (installed.Changed) changed++;
        target = installed.Path;
        entries.Add(NewEntry($"{archiveId}#{entry.Index}:{entry.Name}", relative, DecodedKind(entry.Name, target), target));
        ReportProgress(entries, changedBefore + changed, $"{archiveId}#{entry.Index}:{entry.Name}");
    }
    return changed;
}

static void ReportProgress(List<ImportedAsset> entries, int changed, string current)
{
    if (entries.Count != 1 && entries.Count % 500 != 0) return;
    Console.WriteLine($"Processed {entries.Count}: {changed} written, {entries.Count - changed} reused; current {current}");
}
