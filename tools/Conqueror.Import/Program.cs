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

Directory.CreateDirectory(output);
var entries = new List<ImportedAsset>();
var cueLines = File.ReadAllLines(cuePath);
using (var image = new RawMode1Image(imagePath, CueSheet.DataTrackSectors(cueLines)))
{
    var iso = new Iso9660(image);
    var supported = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".SMK", ".RES", ".CSF", ".PCX", ".PCC", ".LOW", ".WAV", ".MID" };
    foreach (var file in iso.Files.Where(x => supported.Contains(Path.GetExtension(x.Path))))
    {
        var relative = Path.Combine("Raw", string.Join(Path.DirectorySeparatorChar, file.Path.Split('/').Select(ResourcePaths.SafeName)));
        var target = ResourcePaths.SafeTarget(output, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var installed = GeneratedContentInstaller.InstallBytes(output, relative, iso.ReadFile(file));
        target = installed.Path;
        entries.Add(NewEntry(file.Path, relative, Kind(file.Path), target));
        if (DynamixArchive.HasContainerExtension(file.Path))
            InstallDecodedEntries(new DynamixArchive(target), file.Path, output, entries);
    }
}

var archiveRelative = Path.Combine("Archives", "C1086.GOB");
var archiveTarget = ResourcePaths.SafeTarget(output, archiveRelative);
Directory.CreateDirectory(Path.GetDirectoryName(archiveTarget)!);
archiveTarget = GeneratedContentInstaller.InstallFile(output, archiveRelative, gobPath).Path;
entries.Add(NewEntry("C1086.GOB", archiveRelative, "archive", archiveTarget));
InstallDecodedEntries(new DynamixArchive(archiveTarget), "C1086.GOB", output, entries);

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
    target = GeneratedContentInstaller.InstallGenerated(output, relative,
        wave => CddaWave.Write(source, wave, track.StartSector, endSector - track.StartSector)).Path;
    entries.Add(NewEntry($"CDDA/TRACK{track.Number:00}", relative, "audio", target));
}

var manifest = new ImportManifest(1, ResourceHash.Sha256(imagePath), entries.OrderBy(x => x.Id, StringComparer.OrdinalIgnoreCase).ToArray());
manifest.Write(Path.Combine(output, "manifest.json"));
var verification = ImportManifestVerifier.Verify(output, manifest);
if (!verification.IsValid)
{
    foreach (var issue in verification.Issues.Take(20))
        Console.Error.WriteLine($"{issue.AssetId}: {issue.Reason} ({issue.Path})");
    Console.Error.WriteLine($"Installation verification failed with {verification.Issues.Count} issue(s).");
    return 3;
}
Console.WriteLine($"{(operation == "--repair" ? "Repaired" : "Installed")} and verified {entries.Count} owned resources in {output}.");
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

static string Kind(string path) => Path.GetExtension(path).ToUpperInvariant() switch
{
    ".SMK" => "movie", ".WAV" or ".MID" => "audio", ".PCX" or ".PCC" => "image", _ => "resource"
};
static ImportedAsset NewEntry(string id, string relative, string kind, string path) => new(id.Replace('\\', '/'), relative.Replace('\\', '/'), kind, new FileInfo(path).Length, ResourceHash.Sha256(path));

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

static void InstallDecodedEntries(DynamixArchive archive, string archiveId, string output, List<ImportedAsset> entries)
{
    foreach (var entry in archive.Entries.Where(DynamixArchive.CanDecode))
    {
        var folder = ResourcePaths.DecodedArchiveFolder(archiveId);
        var relative = Path.Combine("Decoded", folder, $"{entry.Index:0000}-{ResourcePaths.SafeName(entry.Name)}");
        var target = ResourcePaths.SafeTarget(output, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        target = GeneratedContentInstaller.InstallBytes(output, relative, archive.ReadDecoded(entry)).Path;
        entries.Add(NewEntry($"{archiveId}#{entry.Index}:{entry.Name}", relative, DecodedKind(entry.Name, target), target));
    }
}
