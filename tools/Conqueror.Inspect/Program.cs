using Conqueror.Resources;
using Iced.Intel;
using System.Buffers.Binary;
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
var inspectionOptions = args.Skip(2).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
var renderCsfName = OptionValue(inspectionOptions, "--render-csf=");
var palettePcxName = OptionValue(inspectionOptions, "--palette-pcx=");
var disassembleAddresses = OptionValue(inspectionOptions, "--disassemble=");
var xrefDataOffsets = OptionValue(inspectionOptions, "--xref-data=");
if (disassembleAddresses is not null)
{
    var executable = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories).Single();
    var addresses = disassembleAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(ParseAddress).ToArray();
    File.WriteAllText(Path.Combine(output, "executable-disassembly-report.txt"), DisassembleLinearExecutable(executable, addresses));
}
if (xrefDataOffsets is not null)
{
    var executable = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories).Single();
    var offsets = xrefDataOffsets.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(ParseAddress).ToArray();
    File.WriteAllText(Path.Combine(output, "executable-data-xrefs.txt"), FindLinearExecutableDataReferences(executable, offsets));
}
var terms = inspectionOptions.Where(x => !x.StartsWith("--", StringComparison.Ordinal)).ToArray();
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
var kind1Blocks = 0;
var resourceInventory = new List<(string Scope, DynamixEntry Entry)>();
var soundBankReport = new StringBuilder("# Scope  Samples  Rates  SampleBytes  Name\n");
var gobPath = Path.Combine(install, "C1086.GOB");
if (File.Exists(gobPath))
{
    var gob = new DynamixArchive(gobPath);
    gobEntries = gob.Entries.Count;
    gobStoredEntries = gob.Entries.Count(x => x.IsStored);
    resourceInventory.AddRange(gob.Entries.Select(x => ("GOB", x)));
    var directory = new StringBuilder("# Index  Flags  Stored  Expanded  Offset  Name\n");
    foreach (var entry in gob.Entries)
        directory.AppendLine($"{entry.Index,5}  {entry.Flags,5}  {entry.StoredSize,10}  {entry.ExpandedSize,10}  0x{entry.Offset:X8}  {entry.Name}");
    File.WriteAllText(Path.Combine(output, "gob-directory.txt"), directory.ToString());

    var compressionReport = new StringBuilder("# Index  Kind  Blocks  Compressed  Stored  Result  Name\n");
    foreach (var entry in gob.Entries.Where(x => !x.IsStored))
    {
        try
        {
            if (entry.Flags == 2)
            {
                _ = gob.ReadDecoded(entry);
                compressionReport.AppendLine($"{entry.Index,5}  {entry.Flags,4}  -  -  -  decoded-size-valid  {entry.Name}");
                continue;
            }
            if (entry.Flags != 1)
            {
                compressionReport.AppendLine($"{entry.Index,5}  {entry.Flags,4}  -  -  -  codec-unidentified  {entry.Name}");
                continue;
            }
            var blocks = DynamixCompression.ReadKind1Blocks(gob.ReadStored(entry));
            if (blocks.Count != DynamixCompression.ExpectedKind1BlockCount(checked((int)entry.ExpandedSize)))
                throw new InvalidDataException("Kind-1 block count does not match the declared expanded size.");
            kind1Blocks += blocks.Count;
            var storedBytes = gob.ReadStored(entry);
            _ = gob.ReadDecoded(entry);
            compressionReport.AppendLine($"{entry.Index,5}  {entry.Flags,4}  {blocks.Count,6}  {blocks.Count(x => x.Storage == DynamixBlockStorage.Compressed),10}  {blocks.Count(x => x.Storage == DynamixBlockStorage.Stored),6}  decoded-size-valid  {entry.Name}");
        }
        catch (InvalidDataException error)
        {
            compressionReport.AppendLine($"{entry.Index,5}  {entry.Flags,4}  -  rejected: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}  {entry.Name}");
        }
    }
    File.WriteAllText(Path.Combine(output, "gob-compression-report.txt"), compressionReport.ToString());

    var imageReport = new StringBuilder("# Width  Height  Pixel-index SHA-256  Name\n");
    foreach (var entry in gob.Entries.Where(DynamixArchive.CanDecode))
    {
        var bytes = gob.ReadDecoded(entry);
        if (bytes.Length < 4 || bytes[0] != 0x0A || bytes[2] != 1 || bytes[3] != 8) continue;
        try
        {
            var pcxImage = PcxDecoder.Decode(bytes);
            imageReport.AppendLine($"{pcxImage.Width,5}  {pcxImage.Height,6}  {Convert.ToHexString(SHA256.HashData(pcxImage.Indices)).ToLowerInvariant()}  {entry.Name}");
        }
        catch (InvalidDataException error)
        {
            imageReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}  {entry.Name}");
        }
    }
    File.WriteAllText(Path.Combine(output, "stored-image-report.txt"), imageReport.ToString());

    var csfReport = new StringBuilder("# Storage  Chunks  Minimum  Maximum  Payload bytes  Segments literal/skip/fill  Frame-sequence SHA-256  Dimension headers  Name\n");
    foreach (var entry in gob.Entries.Where(x => DynamixArchive.CanDecode(x) && Path.GetExtension(x.Name).Equals(".CSF", StringComparison.OrdinalIgnoreCase)))
    {
        try
        {
            var sequence = new CsfSequence(gob.ReadDecoded(entry));
            using var frameHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var dimensionsBytes = new byte[4];
            long literalSegments = 0, transparentSegments = 0, fillSegments = 0;
            foreach (var chunk in sequence.Chunks)
            {
                var frame = sequence.DecodeFrame(chunk);
                literalSegments += frame.LiteralSegments;
                transparentSegments += frame.TransparentSegments;
                fillSegments += frame.FillSegments;
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(dimensionsBytes, checked((ushort)frame.Width));
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(dimensionsBytes.AsSpan(2), checked((ushort)frame.Height));
                frameHash.AppendData(dimensionsBytes);
                frameHash.AppendData(frame.Indices);
                frameHash.AppendData(frame.Alpha);
            }
            var dimensions = sequence.Chunks.Select(x => sequence.ReadDimensionHeader(x)).GroupBy(x => x).OrderByDescending(x => x.Count()).ThenBy(x => x.Key.Width).Select(x => $"{x.Key.Width}x{x.Key.Height}:{x.Count()}");
            csfReport.AppendLine($"{(entry.IsStored ? "stored" : "kind1"),7}  {sequence.Chunks.Count,6}  {sequence.Chunks.Min(x => x.Size),7}  {sequence.Chunks.Max(x => x.Size),7}  {sequence.Chunks.Sum(x => (long)x.Size),13}  {literalSegments}/{transparentSegments}/{fillSegments}  {Convert.ToHexString(frameHash.GetHashAndReset()).ToLowerInvariant()}  {string.Join(',', dimensions)}  {entry.Name}");
        }
        catch (InvalidDataException error)
        {
            csfReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}  {entry.Name}");
        }
    }
    File.WriteAllText(Path.Combine(output, "csf-report.txt"), csfReport.ToString());

    var hatReport = new StringBuilder("# Name  Screen  Origin  Size  Regions  Tag  Background  Region records (id:x,y,width,height,enabled)\n");
    foreach (var entry in gob.Entries.Where(x => (x.IsStored || x.Flags == 1) && Path.GetExtension(x.Name).Equals(".HAT", StringComparison.OrdinalIgnoreCase)))
    {
        try
        {
            var layout = new HatLayout(gob.ReadDecoded(entry));
            var regions = string.Join(' ', layout.Regions.Select(x => $"{x.Id}:{x.X},{x.Y},{x.Width},{x.Height},{x.Enabled}"));
            hatReport.AppendLine($"{entry.Name,-16}  {layout.ScreenId,6}  {layout.OriginX},{layout.OriginY}  {layout.Width}x{layout.Height}  {layout.Regions.Count,7}  0x{layout.UnknownTag:X6}  {layout.BackgroundName}  {regions}");
        }
        catch (InvalidDataException error)
        {
            hatReport.AppendLine($"rejected  {entry.Name}: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "hat-layout-report.txt"), hatReport.ToString());

    foreach (var entry in gob.Entries.Where(x => DynamixArchive.CanDecode(x)
        && Path.GetExtension(x.Name).Equals(".666", StringComparison.OrdinalIgnoreCase)))
        AppendSoundBankReport(soundBankReport, "GOB", entry.Name, gob.ReadDecoded(entry));

    var weaponStoreReport = new StringBuilder("# Record  Movie  Unknown  ImageFrame  ItemId  Price  DescriptionChars\n");
    var weaponStoreEntry = gob.Entries.FirstOrDefault(x => x.Name.Equals("weapons.dat", StringComparison.OrdinalIgnoreCase));
    if (weaponStoreEntry is not null && DynamixArchive.CanDecode(weaponStoreEntry))
    {
        try
        {
            var weaponStore = WeaponStoreDecoder.Decode(gob.ReadDecoded(weaponStoreEntry));
            foreach (var item in weaponStore.Entries)
                weaponStoreReport.AppendLine($"{item.RecordIndex,8}  {(item.MovieFile == "#" ? "none" : "yes"),5}  {item.UnknownValue,7}  {item.ImageFrame,10}  {item.ItemId,6}  {item.Price,5}  {item.Description.Length,16}");
        }
        catch (InvalidDataException error)
        {
            weaponStoreReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "weapon-store-report.txt"), weaponStoreReport.ToString());

    var dilemmaReport = new StringBuilder("# Number  Age  Scene  Choices  Outcomes  Changes  PromptChars  OutcomeChars  Name\n");
    var dilemmaRules = new StringBuilder("# Number  Choice:scoring[low,high] outcome(changes); original prose omitted\n");
    foreach (var entry in gob.Entries.Where(x => (x.IsStored || x.Flags == 1)
        && x.Name.StartsWith("dilem", StringComparison.OrdinalIgnoreCase)
        && Path.GetExtension(x.Name).Equals(".DAT", StringComparison.OrdinalIgnoreCase)))
    {
        try
        {
            var dilemma = DilemmaTextDecoder.Decode(gob.ReadDecoded(entry));
            var outcomes = dilemma.Choices.Sum(choice => choice.Outcomes.Count);
            var changes = dilemma.Choices.Sum(choice => choice.Outcomes.Sum(outcome => outcome.Changes.Count));
            var outcomeChars = dilemma.Choices.Sum(choice => choice.Outcomes.Sum(outcome => outcome.Text.Length));
            dilemmaReport.AppendLine($"{dilemma.Number,6}  {dilemma.Age,3}  {dilemma.SceneFile,-12}  {dilemma.Choices.Count,7}  {outcomes,8}  {changes,7}  {dilemma.Prompt.Length,11}  {outcomeChars,12}  {entry.Name}");
            var rules = dilemma.Choices.Select(choice =>
                $"{choice.Number}:{choice.ScoringAttribute}[{choice.LowBreakpoint},{choice.HighBreakpoint}] "
                + string.Join(' ', choice.Outcomes.OrderBy(outcome => outcome.Outcome).Select(outcome =>
                    $"{outcome.Outcome}({string.Join(',', outcome.Changes.Select(change => $"{change.Attribute}{change.Modifier:+#;-#;0}"))})")));
            dilemmaRules.AppendLine($"{dilemma.Number,6}  {string.Join("; ", rules)}");
        }
        catch (InvalidDataException error)
        {
            dilemmaReport.AppendLine($"rejected  {entry.Name}: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "dilemma-text-report.txt"), dilemmaReport.ToString());
    File.WriteAllText(Path.Combine(output, "dilemma-rules-report.txt"), dilemmaRules.ToString());

    if (renderCsfName is not null || palettePcxName is not null)
    {
        if (renderCsfName is null || palettePcxName is null)
            throw new ArgumentException("CSF previews require both --render-csf=<name> and --palette-pcx=<name>.");
        var csfEntry = gob.Entries.FirstOrDefault(x => x.Name.Equals(renderCsfName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"GOB resource '{renderCsfName}' was not found.");
        var paletteEntry = gob.Entries.FirstOrDefault(x => x.Name.Equals(palettePcxName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"GOB resource '{palettePcxName}' was not found.");
        var previewSequence = new CsfSequence(gob.ReadDecoded(csfEntry));
        var previewPalette = PcxDecoder.Decode(gob.ReadDecoded(paletteEntry)).PaletteRgb;
        var previewRoot = Path.Combine(artifactRoot, "csf-previews", SafeName(csfEntry.Name));
        Directory.CreateDirectory(previewRoot);
        foreach (var chunk in previewSequence.Chunks)
            WritePpm(Path.Combine(previewRoot, $"frame-{chunk.Index:D4}.ppm"), previewSequence.DecodeFrame(chunk), previewPalette, 6);
    }
}

var sceneReport = new StringBuilder("# Result  Entries  Stored  Kind1  Kind2  CompressedBlocks  StoredBlocks  ISO path\n");
var sceneTextureReport = new StringBuilder("# Textures  Dimensions  ISO path\n");
var paletteReport = new StringBuilder("# Minimum  Maximum  SHA-256  Resource  ISO path\n");
var sceneContainers = 0;
var sceneEntries = 0;
var sceneStoredEntries = 0;
var sceneCompressedBlocks = 0;
var sceneVerbatimBlocks = 0;
var sceneTextures = 0;
foreach (var file in files.Where(x => DynamixArchive.HasContainerExtension(x.Path)))
{
    try
    {
        var archive = new DynamixArchive(iso.ReadFile(file), file.Path);
        var stored = archive.Entries.Count(x => x.IsStored);
        var kind1 = archive.Entries.Where(x => !x.IsStored && x.Flags == 1).ToArray();
        var compressedBlocks = 0;
        var verbatimBlocks = 0;
        foreach (var entry in kind1)
        {
            var storedBytes = archive.ReadStored(entry);
            var blocks = DynamixCompression.ReadKind1Blocks(storedBytes);
            if (blocks.Count != DynamixCompression.ExpectedKind1BlockCount(checked((int)entry.ExpandedSize)))
                throw new InvalidDataException("Kind-1 block count does not match the declared expanded size.");
            compressedBlocks += blocks.Count(x => x.Storage == DynamixBlockStorage.Compressed);
            verbatimBlocks += blocks.Count(x => x.Storage == DynamixBlockStorage.Stored);
            _ = archive.ReadDecoded(entry);
        }
        var kind2 = archive.Entries.Count(x => !x.IsStored && x.Flags == 2);
        sceneReport.AppendLine($"valid  {archive.Entries.Count,7}  {stored,6}  {kind1.Length,5}  {kind2,5}  {compressedBlocks,16}  {verbatimBlocks,12}  {file.Path}");
        sceneContainers++;
        sceneEntries += archive.Entries.Count;
        sceneStoredEntries += stored;
        sceneCompressedBlocks += compressedBlocks;
        sceneVerbatimBlocks += verbatimBlocks;
        resourceInventory.AddRange(archive.Entries.Select(x => ("SCENE", x)));
        var textures = archive.Entries.Where(entry => entry.Name.StartsWith("TEX", StringComparison.OrdinalIgnoreCase))
            .Select(entry => DynamixSceneTextureDecoder.Decode(entry.Name, archive.ReadDecoded(entry))).ToArray();
        sceneTextures += textures.Length;
        var dimensions = string.Join(',', textures.Select(texture => $"{texture.Width}x{texture.Height}")
            .Distinct().Order(StringComparer.Ordinal));
        sceneTextureReport.AppendLine($"{textures.Length,8}  {dimensions,-40}  {file.Path}");
        foreach (var entry in archive.Entries.Where(x => DynamixArchive.CanDecode(x)
            && Path.GetExtension(x.Name).Equals(".666", StringComparison.OrdinalIgnoreCase)))
            AppendSoundBankReport(soundBankReport, file.Path, entry.Name, archive.ReadDecoded(entry));
        foreach (var entry in archive.Entries.Where(x => x.IsStored && Path.GetExtension(x.Name).Equals(".PAL", StringComparison.OrdinalIgnoreCase)))
        {
            try
            {
                var palette = IndexedPaletteDecoder.Decode(archive.ReadStored(entry));
                paletteReport.AppendLine($"{palette.Rgb.Min(),7}  {palette.Rgb.Max(),7}  {Convert.ToHexString(SHA256.HashData(palette.Rgb)).ToLowerInvariant()}  {entry.Name}  {file.Path}");
            }
            catch (InvalidDataException error)
            {
                paletteReport.AppendLine($"rejected  {entry.Name}  {file.Path}: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
            }
        }
    }
    catch (InvalidDataException error)
    {
        sceneReport.AppendLine($"rejected: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}  {file.Path}");
    }
}
File.WriteAllText(Path.Combine(output, "scene-res-report.txt"), sceneReport.ToString());
sceneTextureReport.AppendLine($"# total textures: {sceneTextures}");
File.WriteAllText(Path.Combine(output, "scene-texture-report.txt"), sceneTextureReport.ToString());
File.WriteAllText(Path.Combine(output, "stored-palette-report.txt"), paletteReport.ToString());
File.WriteAllText(Path.Combine(output, "sound-bank-report.txt"), soundBankReport.ToString());
var smackerReport = new StringBuilder("# Version  Dimensions  Frames  Frame ms  Palette changes  Audio packets  Decoded audio  Audio tracks  Bytes  ISO path\n");
var smackerMovies = 0;
long smackerBytes = 0;
long smackerFrames = 0;
long smackerPaletteChanges = 0;
long smackerAudioPackets = 0;
long smackerDecodedAudioBytes = 0;
foreach (var file in files.Where(x => Path.GetExtension(x.Path).Equals(".SMK", StringComparison.OrdinalIgnoreCase)))
{
    try
    {
        var source = iso.ReadFile(file);
        var movie = SmackerMovieDecoder.Decode(source);
        var palette = new byte[768];
        var paletteChanges = 0;
        var audioPackets = 0;
        long decodedAudioBytes = 0;
        for (var index = 0; index < movie.Frames.Count; index++)
        {
            var frame = SmackerMovieDecoder.DecodeFrameLayout(movie, index, source, palette);
            palette = frame.Palette;
            if (frame.PaletteChanged) paletteChanges++;
            audioPackets += frame.AudioPackets.Count;
            foreach (var packet in frame.AudioPackets)
            {
                var track = movie.AudioTracks.Single(candidate => candidate.Index == packet.TrackIndex);
                decodedAudioBytes += SmackerAudioDecoder.Decode(
                    source.AsSpan(packet.Data.Offset, packet.Data.Length), track).Samples.Length;
            }
        }
        var audio = string.Join(',', movie.AudioTracks.Select(track =>
            $"{track.Index}:{track.SampleRate}/{(track.IsCompressed ? "packed" : "pcm")}/{(track.Is16Bit ? 16 : 8)}/{(track.IsStereo ? 2 : 1)}"));
        smackerReport.AppendLine(FormattableString.Invariant(
            $"SMK{movie.Version,-4}  {movie.Width}x{movie.Height,-10}  {movie.Frames.Count,6}  {movie.FrameDuration.TotalMilliseconds,8:0.###}  {paletteChanges,15}  {audioPackets,13}  {decodedAudioBytes,13}  {audio,-28}  {file.Size,9}  {file.Path}"));
        smackerMovies++;
        smackerBytes += file.Size;
        smackerFrames += movie.Frames.Count;
        smackerPaletteChanges += paletteChanges;
        smackerAudioPackets += audioPackets;
        smackerDecodedAudioBytes += decodedAudioBytes;
    }
    catch (Exception error) when (error is InvalidDataException or OverflowException or ArgumentException)
    {
        smackerReport.AppendLine($"rejected: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}  {file.Path}");
    }
}
smackerReport.AppendLine($"# totals: {smackerMovies} movies, {smackerFrames} frames, {smackerPaletteChanges} palette changes, {smackerAudioPackets} audio packets, {smackerDecodedAudioBytes} decoded audio bytes, {smackerBytes} source bytes");
File.WriteAllText(Path.Combine(output, "smacker-report.txt"), smackerReport.ToString());
var extensionReport = new StringBuilder("# Extension  Total  Stored  Kind1  Kind2  Other  Scopes\n");
foreach (var group in resourceInventory.GroupBy(x => Path.GetExtension(x.Entry.Name).ToUpperInvariant()).OrderBy(x => x.Key))
{
    var extension = string.IsNullOrEmpty(group.Key) ? "<none>" : group.Key;
    var stored = group.Count(x => x.Entry.IsStored);
    var kind1 = group.Count(x => !x.Entry.IsStored && x.Entry.Flags == 1);
    var kind2 = group.Count(x => !x.Entry.IsStored && x.Entry.Flags == 2);
    var other = group.Count() - stored - kind1 - kind2;
    extensionReport.AppendLine($"{extension,-10}  {group.Count(),5}  {stored,6}  {kind1,5}  {kind2,5}  {other,5}  {string.Join(',', group.Select(x => x.Scope).Distinct().Order())}");
}
File.WriteAllText(Path.Combine(output, "resource-extension-report.txt"), extensionReport.ToString());
Console.WriteLine($"Indexed {files.Length} CD files, {gobEntries} GOB entries ({gobStoredEntries} stored, {kind1Blocks} kind-1 blocks), {sceneContainers} scene containers ({sceneEntries} entries, {sceneStoredEntries} stored, {sceneCompressedBlocks} compressed-marker blocks, {sceneVerbatimBlocks} verbatim blocks), and {smackerMovies} Smacker movies; extracted {extracted} inspectable artifacts to {output}.");
return 0;
}
catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException or OverflowException)
{
    Console.Error.WriteLine($"Original-data inspection failed: {error.Message}");
    return 1;
}

static string SafeName(string name) => string.Concat(name.Where(c => char.IsLetterOrDigit(c) || c is '.' or '_' or '-'));
static string? OptionValue(IEnumerable<string> arguments, string prefix) => arguments
    .FirstOrDefault(x => x.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))?[prefix.Length..];

static uint ParseAddress(string value)
{
    var digits = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? value[2..] : value;
    return uint.TryParse(digits, System.Globalization.NumberStyles.HexNumber, null, out var address)
        ? address
        : throw new ArgumentException($"Invalid hexadecimal address '{value}'.");
}

static void AppendSoundBankReport(StringBuilder report, string scope, string name, byte[] bytes)
{
    try
    {
        var bank = DynamixSoundBankDecoder.Decode(bytes);
        var rates = string.Join(',', bank.Samples.Select(sample => sample.SampleRate).Distinct().Order());
        report.AppendLine($"{scope}  {bank.Samples.Count,7}  {rates,-17}  {bank.Samples.Sum(sample => (long)sample.Samples.Length),11}  {name}");
    }
    catch (InvalidDataException error)
    {
        report.AppendLine($"{scope}  rejected  {name}: {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
    }
}

static string DisassembleLinearExecutable(string path, IReadOnlyList<uint> addresses)
{
    var bytes = File.ReadAllBytes(path);
    var le = Enumerable.Range(0, bytes.Length - 0x84)
        .FirstOrDefault(index => bytes[index] == (byte)'L' && bytes[index + 1] == (byte)'E'
            && bytes[index + 2] == 0 && bytes[index + 3] == 0
            && BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index + 0x44, 4)) is > 0 and < 64
            && BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index + 0x28, 4)) is >= 512 and <= 65536);
    if (le == 0) throw new InvalidDataException("Linear Executable header was not found.");
    var pageSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x28, 4));
    var objectTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x40, 4));
    var objectCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x44, 4));
    var dataPages = checked((uint)le + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x80, 4)));
    var report = new StringBuilder("# 32-bit LE disassembly (derived metadata; original bytes omitted)\n");
    report.AppendLine($"# LE file offset 0x{le:X}; objects {objectCount}; page size 0x{pageSize:X}");
    foreach (var address in addresses)
    {
        var mapped = false;
        for (var objectIndex = 0; objectIndex < objectCount; objectIndex++)
        {
            var descriptor = checked(le + (int)objectTable + objectIndex * 24);
            var virtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor, 4));
            var baseAddress = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 4, 4));
            var pageIndex = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 12, 4));
            if (address < baseAddress || address >= baseAddress + virtualSize) continue;
            var fileOffset = checked(dataPages + (pageIndex - 1) * pageSize + address - baseAddress);
            const int maximumDisassemblyBytes = 4096;
            var available = Math.Min(maximumDisassemblyBytes, bytes.Length - checked((int)fileOffset));
            var reader = new ByteArrayCodeReader(bytes.AsSpan(checked((int)fileOffset), available).ToArray());
            var decoder = Iced.Intel.Decoder.Create(32, reader);
            decoder.IP = address;
            var formatter = new IntelFormatter();
            report.AppendLine($"\n# object {objectIndex + 1}, VA 0x{address:X8}, file offset 0x{fileOffset:X8}");
            for (var instructionIndex = 0; instructionIndex < 1000 && decoder.IP < address + (uint)available; instructionIndex++)
            {
                decoder.Decode(out var instruction);
                if (instruction.IsInvalid) break;
                var formatted = new StringOutput();
                formatter.Format(instruction, formatted);
                report.AppendLine($"0x{instruction.IP:X8}  {formatted}");
            }
            mapped = true;
            break;
        }
        if (!mapped) report.AppendLine($"# VA 0x{address:X8} is outside mapped objects.");
    }
    return report.ToString();
}

static string FindLinearExecutableDataReferences(string path, IReadOnlyList<uint> offsets)
{
    var bytes = File.ReadAllBytes(path);
    var fixups = LinearExecutableFixupReader.ReadInternalFixups(bytes);
    var le = Enumerable.Range(0, bytes.Length - 0x84)
        .FirstOrDefault(index => bytes[index] == (byte)'L' && bytes[index + 1] == (byte)'E'
            && bytes[index + 2] == 0 && bytes[index + 3] == 0
            && BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index + 0x44, 4)) is > 0 and < 64
            && BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(index + 0x28, 4)) is >= 512 and <= 65536);
    if (le == 0) throw new InvalidDataException("Linear Executable header was not found.");
    var pageSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x28, 4));
    var objectTable = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x40, 4));
    var objectCount = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x44, 4));
    var dataPages = checked((uint)le + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x80, 4)));
    var report = new StringBuilder("# 32-bit LE references to object-relative data offsets (derived metadata; original bytes omitted)\n");
    report.AppendLine($"# requested: {string.Join(',', offsets.Select(offset => $"0x{offset:X}"))}");
    report.AppendLine($"# decoded internal fixups: {fixups.Count}");
    var matchedFixups = fixups.Where(fixup => offsets.Contains(fixup.TargetOffset)).OrderBy(fixup => fixup.SourceAddress).ToArray();
    foreach (var fixup in matchedFixups)
        report.AppendLine($"0x{fixup.SourceAddress:X8}  relocation object{fixup.TargetObject}+0x{fixup.TargetOffset:X}  source-type 0x{fixup.SourceType:X2}{(fixup.Additive ? " additive" : "")}{(fixup.Chained ? " chained" : "")}");
    var nearbyFixups = fixups.Where(candidate => !matchedFixups.Contains(candidate)
        && matchedFixups.Any(match => Math.Abs((long)candidate.SourceAddress - match.SourceAddress) <= 0x100))
        .OrderBy(fixup => fixup.SourceAddress).ToArray();
    if (nearbyFixups.Length > 0)
    {
        report.AppendLine("# nearby relocation context (within 0x100 source bytes of a requested match)");
        foreach (var fixup in nearbyFixups)
                report.AppendLine($"0x{fixup.SourceAddress:X8}  nearby object{fixup.TargetObject}+0x{fixup.TargetOffset:X}  source-type 0x{fixup.SourceType:X2}{(fixup.Additive ? " additive" : "")}{(fixup.Chained ? " chained" : "")}");
    }

    for (var objectIndex = 0; objectIndex < objectCount; objectIndex++)
    {
        var descriptor = checked(le + (int)objectTable + objectIndex * 24);
        var virtualSize = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor, 4));
        var baseAddress = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 4, 4));
        var pageIndex = BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(descriptor + 12, 4));
        var fileOffset = checked(dataPages + (pageIndex - 1) * pageSize);
        var available = Math.Min(checked((int)virtualSize), bytes.Length - checked((int)fileOffset));
        var reader = new ByteArrayCodeReader(bytes.AsSpan(checked((int)fileOffset), available).ToArray());
        var decoder = Iced.Intel.Decoder.Create(32, reader);
        decoder.IP = baseAddress;
        var formatter = new IntelFormatter();
        while (decoder.IP < baseAddress + (uint)available)
        {
            decoder.Decode(out var instruction);
            if (instruction.IsInvalid) continue;
            var formatted = new StringOutput();
            formatter.Format(instruction, formatted);
            var text = formatted.ToString();
            foreach (var offset in offsets)
            {
                var pattern = $@"(?<![0-9A-F])0*{offset:X}h(?![0-9A-F])";
                if (System.Text.RegularExpressions.Regex.IsMatch(text, pattern,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    report.AppendLine($"0x{instruction.IP:X8}  data+0x{offset:X}  {text}");
            }
        }
    }
    return report.ToString();
}

static void WritePpm(string path, CsfFrame frame, byte[] palette, int scale)
{
    if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));
    var width = checked(frame.Width * scale);
    var height = checked(frame.Height * scale);
    var header = Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
    var bytes = new byte[checked(header.Length + width * height * 3)];
    header.CopyTo(bytes, 0);
    var target = header.Length;
    for (var y = 0; y < height; y++)
    for (var x = 0; x < width; x++)
    {
        var sourceX = x / scale;
        var sourceY = y / scale;
        var source = sourceY * frame.Width + sourceX;
        if (frame.Alpha[source] == 0)
        {
            var checker = ((sourceX / 4) + (sourceY / 4)) % 2 == 0 ? (byte)48 : (byte)80;
            bytes[target++] = checker;
            bytes[target++] = checker;
            bytes[target++] = checker;
        }
        else
        {
            var color = frame.Indices[source] * 3;
            bytes[target++] = palette[color];
            bytes[target++] = palette[color + 1];
            bytes[target++] = palette[color + 2];
        }
    }
    File.WriteAllBytes(path, bytes);
}

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
