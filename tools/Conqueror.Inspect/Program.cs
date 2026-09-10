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
var renderSmkName = OptionValue(inspectionOptions, "--render-smk=");
var palettePcxName = OptionValue(inspectionOptions, "--palette-pcx=");
var disassembleAddresses = OptionValue(inspectionOptions, "--disassemble=");
var xrefDataOffsets = OptionValue(inspectionOptions, "--xref-data=");
var xrefCodeAddresses = OptionValue(inspectionOptions, "--xref-code=");
var xrefBlockFlags = OptionValue(inspectionOptions, "--xref-block-flags=");
var reportWeaponCombatTable = inspectionOptions.Contains("--weapon-combat-table", StringComparer.OrdinalIgnoreCase);
var fixupSourceAddresses = OptionValue(inspectionOptions, "--fixup-source=");
var conversationNodeIds = OptionValue(inspectionOptions, "--conversation-nodes=");
var conversationTextIds = OptionValue(inspectionOptions, "--conversation-text=");
var integerResourceName = OptionValue(inspectionOptions, "--resource-integers=");
var actionGroupIds = OptionValue(inspectionOptions, "--action-groups=");
var weaponTextIds = OptionValue(inspectionOptions, "--weapon-text=");
var reportSceneBlocks = inspectionOptions.Contains("--scene-blocks", StringComparer.OrdinalIgnoreCase);
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
if (xrefCodeAddresses is not null)
{
    var executable = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories).Single();
    var addresses = xrefCodeAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(ParseAddress).ToArray();
    File.WriteAllText(Path.Combine(output, "executable-code-xrefs.txt"), LinearExecutableCodeReferences.Find(executable, addresses));
}
if (xrefBlockFlags is not null)
{
    var executable = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories).Single();
    var flags = xrefBlockFlags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(ParseAddress).ToArray();
    File.WriteAllText(Path.Combine(output, "executable-block-flag-xrefs.txt"),
        LinearExecutableCodeReferences.FindBlockFlags(executable, flags));
}
if (reportWeaponCombatTable)
{
    var executable = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories).Single();
    File.WriteAllText(Path.Combine(output, "weapon-combat-table-report.txt"), LinearExecutableCodeReferences.ReadDataTable(executable, 0xCE14, 25, 7));
}
if (fixupSourceAddresses is not null)
{
    var executable = Directory.EnumerateFiles(artifactRoot, "CONQUER.EXE", SearchOption.AllDirectories).Single();
    var addresses = fixupSourceAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(ParseAddress).ToArray();
    File.WriteAllText(Path.Combine(output, "executable-fixup-source-report.txt"), FindLinearExecutableFixupsBySource(executable, addresses));
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
if (inspectionOptions.Contains("--executable-only", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine($"Inspected executable metadata and extracted {extracted} bounded artifacts to {output}.");
    return 0;
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

    if (integerResourceName is not null)
    {
        var integerEntry = gob.Entries.FirstOrDefault(x => x.Name.Equals(integerResourceName, StringComparison.OrdinalIgnoreCase))
            ?? throw new ArgumentException($"GOB resource '{integerResourceName}' was not found.");
        var integerBytes = gob.ReadDecoded(integerEntry);
        if (integerBytes.Length > 64 * 1024 || integerBytes.Length % 4 != 0)
            throw new InvalidDataException("Integer resource must contain at most 64 KiB of complete dwords.");
        var integerReport = new StringBuilder("# Index  Offset  Signed  Hex\n");
        for (var offset = 0; offset < integerBytes.Length; offset += 4)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(integerBytes.AsSpan(offset, 4));
            integerReport.AppendLine($"{offset / 4,7}  0x{offset:X4}  {value,11}  0x{unchecked((uint)value):X8}");
        }
        File.WriteAllText(Path.Combine(output, "resource-integer-report.txt"), integerReport.ToString());
    }

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
            if (weaponTextIds is not null)
            {
                var selected = weaponTextIds.Equals("all", StringComparison.OrdinalIgnoreCase)
                    ? weaponStore.Entries.Select(item => item.RecordIndex).ToArray()
                    : weaponTextIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .Select(ParseWeaponRecordId).Distinct().ToArray();
                var textReport = new StringBuilder("# Selected owner-local weapon-store text (do not redistribute)\n");
                foreach (var index in selected)
                {
                    if ((uint)index >= (uint)weaponStore.Entries.Count)
                        throw new ArgumentOutOfRangeException(nameof(weaponTextIds), $"Weapon-store record {index} does not exist.");
                    var item = weaponStore.Entries[index];
                    textReport.AppendLine($"# Record {item.RecordIndex}; item {item.ItemId}; frame {item.ImageFrame}; price {item.Price}");
                    textReport.AppendLine(item.Description);
                }
                File.WriteAllText(Path.Combine(output, "weapon-store-text.txt"), textReport.ToString());
            }
        }
        catch (InvalidDataException error)
        {
            weaponStoreReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "weapon-store-report.txt"), weaponStoreReport.ToString());

    var conversationReport = new StringBuilder("# Nodes  Empty  PromptVariants  Responses  TerminalResponses  LinkedResponses  Continuations  TerminalContinuations  NodeActions  ResponseActions  Body  Index\n");
    var conversationBody = gob.Entries.FirstOrDefault(x => x.Name.Equals("all.cbf", StringComparison.OrdinalIgnoreCase));
    var conversationIndex = gob.Entries.FirstOrDefault(x => x.Name.Equals("all.cif", StringComparison.OrdinalIgnoreCase));
    if (conversationBody is not null && conversationIndex is not null
        && DynamixArchive.CanDecode(conversationBody) && DynamixArchive.CanDecode(conversationIndex))
    {
        try
        {
            var conversations = DynamixConversationDecoder.Decode(
                gob.ReadDecoded(conversationBody), gob.ReadDecoded(conversationIndex));
            var nodes = conversations.Nodes.Values.ToArray();
            var responses = nodes.SelectMany(node => node.Responses).ToArray();
            conversationReport.AppendLine($"{nodes.Length,7}  {nodes.Count(node => node.PortraitFile is null),5}  {nodes.Sum(node => node.PromptVariants.Count),14}  {responses.Length,9}  {responses.Count(response => response.TargetNodeId == 0),17}  {responses.Count(response => response.TargetNodeId != 0),15}  {nodes.Count(node => node.ContinuationNodeId is not null),13}  {nodes.Count(node => node.ContinuationNodeId == 0),21}  {nodes.Sum(node => node.ActionIds.Count),11}  {responses.Sum(response => response.ActionIds.Count),15}  {conversationBody.Name}  {conversationIndex.Name}");
            if (conversationNodeIds is not null)
            {
                var nodeReport = new StringBuilder("# Id  Offset  Continuation  Prompts  Responses  NodeActionIds  ResponseActionIds  Portrait  Speaker\n");
                var requestedNodeIds = conversationNodeIds.Equals("all", StringComparison.OrdinalIgnoreCase)
                    ? conversations.Nodes.Keys.Order().ToArray()
                    : conversationNodeIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(ParseNodeId).ToArray();
                foreach (var id in requestedNodeIds)
                {
                    var node = conversations.Find(id)
                        ?? throw new ArgumentException($"Conversation node {id} was not found.");
                    var responseActionIds = string.Join('|', node.Responses.Select((response, index) =>
                        $"{index}:{string.Join(',', response.ActionIds)}"));
                    nodeReport.AppendLine($"{node.Id,6}  0x{node.SourceOffset:X8}  {node.ContinuationNodeId?.ToString() ?? "-",12}  {node.PromptVariants.Count,7}  {node.Responses.Count,9}  {string.Join(',', node.ActionIds),13}  {responseActionIds,-24}  {node.PortraitFile ?? "-"}  {node.Speaker ?? "-"}");
                }
                File.WriteAllText(Path.Combine(output, "conversation-node-report.txt"), nodeReport.ToString());
            }
            if (conversationTextIds is not null)
            {
                var textReport = new StringBuilder("# Selected original conversation text; local analysis only; do not redistribute\n");
                var requestedTextIds = conversationTextIds.Equals("all", StringComparison.OrdinalIgnoreCase)
                    ? conversations.Nodes.Keys.Order().ToArray()
                    : conversationTextIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(ParseNodeId).ToArray();
                foreach (var id in requestedTextIds)
                {
                    var node = conversations.Find(id)
                        ?? throw new ArgumentException($"Conversation node {id} was not found.");
                    textReport.AppendLine($"node {node.Id} speaker={ReportText(node.Speaker ?? "-")} portrait={node.PortraitFile ?? "-"} continuation={node.ContinuationNodeId?.ToString() ?? "-"} actions={string.Join(',', node.ActionIds)}");
                    foreach (var prompt in node.PromptVariants.Select((text, index) => (text, index)))
                        textReport.AppendLine($"  prompt {prompt.index}: {ReportText(prompt.text)}");
                    foreach (var response in node.Responses.Select((value, index) => (value, index)))
                        textReport.AppendLine($"  response {response.index}: target={response.value.TargetNodeId} actions={string.Join(',', response.value.ActionIds)} text={ReportText(response.value.Text)}");
                }
                File.WriteAllText(Path.Combine(output, "conversation-text-report.txt"), textReport.ToString());
            }
        }
        catch (InvalidDataException error)
        {
            conversationReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "conversation-report.txt"), conversationReport.ToString());

    var variableTableReport = new StringBuilder("# Variables  ElementSize  ElementKind  NonzeroInitialValues  Name\n");
    var variableTableEntry = gob.Entries.FirstOrDefault(x => x.Name.Equals("all.vtb", StringComparison.OrdinalIgnoreCase));
    if (variableTableEntry is not null && DynamixArchive.CanDecode(variableTableEntry))
    {
        try
        {
            var variables = DynamixVariableTableDecoder.Decode(gob.ReadDecoded(variableTableEntry));
            variableTableReport.AppendLine($"{variables.InitialValues.Count,9}  {variables.ElementSize,11}  {variables.ElementKind,11}  {variables.InitialValues.Count(value => value != 0),20}  {variableTableEntry.Name}");
        }
        catch (InvalidDataException error)
        {
            variableTableReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "variable-table-report.txt"), variableTableReport.ToString());

    var actionTreeReport = new StringBuilder("# IndexHeader  Groups  Actions  Expressions  Values  MissingConversationIds  Body  Index\n");
    var actionBody = gob.Entries.FirstOrDefault(x => x.Name.Equals("all.tmb", StringComparison.OrdinalIgnoreCase));
    var actionIndex = gob.Entries.FirstOrDefault(x => x.Name.Equals("all.tmi", StringComparison.OrdinalIgnoreCase));
    if (actionBody is not null && actionIndex is not null
        && DynamixArchive.CanDecode(actionBody) && DynamixArchive.CanDecode(actionIndex))
    {
        try
        {
            var actionTrees = DynamixActionTreeDecoder.Decode(gob.ReadDecoded(actionBody), gob.ReadDecoded(actionIndex));
            var referencedActionIds = Array.Empty<int>();
            if (conversationBody is not null && conversationIndex is not null
                && DynamixArchive.CanDecode(conversationBody) && DynamixArchive.CanDecode(conversationIndex))
            {
                var conversations = DynamixConversationDecoder.Decode(
                    gob.ReadDecoded(conversationBody), gob.ReadDecoded(conversationIndex));
                referencedActionIds = conversations.Nodes.Values
                    .SelectMany(node => node.ActionIds.Concat(node.Responses.SelectMany(response => response.ActionIds)))
                    .Distinct().Order().ToArray();
            }
            var missing = referencedActionIds.Where(id => actionTrees.Find(id) is null).ToArray();
            actionTreeReport.AppendLine($"{actionTrees.IndexHeaderValue,11}  {actionTrees.Groups.Count,6}  {actionTrees.Actions.Count,7}  {actionTrees.Expressions.Count,11}  {actionTrees.Values.Count,6}  {missing.Length,22}  {actionBody.Name}  {actionIndex.Name}");
            actionTreeReport.AppendLine($"# ActionKinds  {string.Join(' ', actionTrees.Actions.Values.GroupBy(x => x.Kind).OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Count()}"))}");
            actionTreeReport.AppendLine($"# ValueKinds   {string.Join(' ', actionTrees.Values.Values.GroupBy(x => x.Kind).OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Count()}"))}");
            actionTreeReport.AppendLine($"# Operators    {string.Join(' ', actionTrees.Expressions.Values.SelectMany(x => x.Operators).GroupBy(x => x).OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Count()}"))}");
            actionTreeReport.AppendLine($"# FunctionIds  {string.Join(' ', actionTrees.Values.Values.Where(x => x.Kind == DynamixValueKind.Function).GroupBy(x => x.Value).OrderBy(x => x.Key).Select(x => $"{x.Key}:{x.Count()}"))}");
            actionTreeReport.AppendLine($"# FunctionSignatures  {string.Join(' ', actionTrees.Values.Values.Where(x => x.Kind == DynamixValueKind.Function).GroupBy(x => x.Value).OrderBy(x => x.Key).Select(x => $"{x.Key}[{string.Join(',', x.GroupBy(v => v.ArgumentExpressionOffsets.Count).OrderBy(v => v.Key).Select(v => $"{v.Key}:{v.Count()}"))}]"))}");
            if (missing.Length > 0) actionTreeReport.AppendLine($"# MissingIds   {string.Join(',', missing)}");
            if (actionGroupIds is not null)
            {
                var groupReport = new StringBuilder("# Selected action groups; expressions are numeric metadata only\n");
                var requestedGroupIds = actionGroupIds.Equals("all", StringComparison.OrdinalIgnoreCase)
                    ? actionTrees.Groups.Keys.Order().ToArray()
                    : actionGroupIds.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(ParseNodeId).ToArray();
                foreach (var id in requestedGroupIds)
                {
                    var group = actionTrees.Find(id)
                        ?? throw new ArgumentException($"Action group {id} was not found.");
                    groupReport.AppendLine($"group {id} offset=0x{group.SourceOffset:X} actions={group.ActionOffsets.Count}");
                    foreach (var offset in group.ActionOffsets)
                        AppendAction(groupReport, actionTrees, offset, "  ", []);
                }
                File.WriteAllText(Path.Combine(output, "action-group-report.txt"), groupReport.ToString());
            }
        }
        catch (InvalidDataException error)
        {
            actionTreeReport.AppendLine($"rejected  {error.Message.Replace('\r', ' ').Replace('\n', ' ')}");
        }
    }
    File.WriteAllText(Path.Combine(output, "action-tree-report.txt"), actionTreeReport.ToString());

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
        byte[] ReadPreviewResource(string identifier)
        {
            var separator = identifier.IndexOf('/');
            if (separator < 0)
            {
                var entry = gob.Entries.FirstOrDefault(candidate =>
                    candidate.Name.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                    ?? throw new ArgumentException($"GOB resource '{identifier}' was not found.");
                return gob.ReadDecoded(entry);
            }

            var archiveName = identifier[..separator];
            var resourceName = identifier[(separator + 1)..];
            if (archiveName.Length == 0 || resourceName.Length == 0 || identifier[(separator + 1)..].Contains('/'))
                throw new ArgumentException($"Nested resource identifier '{identifier}' is invalid.");
            var archiveFile = files.SingleOrDefault(file =>
                Path.GetFileName(file.Path).Equals(archiveName, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Scene archive '{archiveName}' was not found.");
            var archive = new DynamixArchive(iso.ReadFile(archiveFile), archiveFile.Path);
            var nested = archive.Entries.FirstOrDefault(candidate =>
                candidate.Name.Equals(resourceName, StringComparison.OrdinalIgnoreCase))
                ?? throw new ArgumentException($"Resource '{resourceName}' was not found in '{archiveName}'.");
            return archive.ReadDecoded(nested);
        }

        var previewSequence = new CsfSequence(ReadPreviewResource(renderCsfName));
        var paletteBytes = ReadPreviewResource(palettePcxName);
        var previewPalette = paletteBytes.Length == IndexedPalette.ByteSize
            ? IndexedPaletteDecoder.Decode(paletteBytes).Rgb
            : PcxDecoder.Decode(paletteBytes).PaletteRgb;
        var previewRoot = Path.Combine(artifactRoot, "csf-previews", SafeName(renderCsfName));
        Directory.CreateDirectory(previewRoot);
        foreach (var chunk in previewSequence.Chunks)
            WritePpm(Path.Combine(previewRoot, $"frame-{chunk.Index:D4}.ppm"), previewSequence.DecodeFrame(chunk), previewPalette, 6);
        if (inspectionOptions.Contains("--preview-only", StringComparer.OrdinalIgnoreCase))
        {
            Console.WriteLine($"Rendered {previewSequence.Chunks.Count} bounded CSF previews to {previewRoot}.");
            return 0;
        }
    }
}

var sceneReport = new StringBuilder("# Result  Entries  Stored  Kind1  Kind2  CompressedBlocks  StoredBlocks  ISO path\n");
var sceneTextureReport = new StringBuilder("# Textures  Dimensions  ISO path\n");
var sceneScenarioReport = new StringBuilder("# Enabled  MapCount  DistanceShift  BlendTarget  Generated  BlockOffsets  ISO path\n");
var sceneBlockReport = new StringBuilder("# Archive  Index  Placed  Kind  Behavior  Flags  ColorMap  Field40  Field48  Field4A  Field4C  Size  Surfaces  Name\n");
var paletteReport = new StringBuilder("# Minimum  Maximum  SHA-256  Resource  ISO path\n");
var skirmishFile = files.Single(file =>
    Path.GetFileName(file.Path).Equals("SKIRMISH.RES", StringComparison.OrdinalIgnoreCase));
var skirmishArchive = new DynamixArchive(iso.ReadFile(skirmishFile), skirmishFile.Path);
var skirmishPaletteEntry = skirmishArchive.Entries.Single(entry =>
    entry.Name.Equals("SKIRMISH.PAL", StringComparison.OrdinalIgnoreCase));
var skirmishPalette = IndexedPaletteDecoder.Decode(skirmishArchive.ReadDecoded(skirmishPaletteEntry)).Rgb;
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
        var scenarioEntry = archive.Entries.FirstOrDefault(entry => entry.Name.Equals("Scenario", StringComparison.OrdinalIgnoreCase));
        if (scenarioEntry is not null)
        {
            var scenario = archive.ReadDecoded(scenarioEntry);
            var viewerEntry = archive.Entries.Single(entry => entry.Name.Equals("Viewer", StringComparison.OrdinalIgnoreCase));
            var mapEntry = archive.Entries.Single(entry => entry.Name.Equals("Map", StringComparison.OrdinalIgnoreCase));
            var blocksEntry = archive.Entries.Single(entry => entry.Name.Equals("Blocks", StringComparison.OrdinalIgnoreCase));
            var blockBytes = archive.ReadDecoded(blocksEntry);
            var scene = DynamixSceneDecoder.Decode(archive.ReadDecoded(viewerEntry), scenario,
                archive.ReadDecoded(mapEntry), blockBytes);
            var sceneName = Path.GetFileNameWithoutExtension(file.Path);
            if (reportSceneBlocks && (sceneName.StartsWith("MELEE", StringComparison.OrdinalIgnoreCase)
                || sceneName.StartsWith("DEFEND", StringComparison.OrdinalIgnoreCase)))
            {
                var placements = new int[scene.Blocks.Count];
                for (var x = 0; x < DynamixScene.MapWidth; x++)
                for (var y = 0; y < DynamixScene.MapHeight; y++)
                    placements[scene.BlockIndexAt(x, y)]++;
                foreach (var block in scene.Blocks)
                {
                    var record = blockBytes.AsSpan(block.Index * DynamixSceneDecoder.BlockSize,
                        DynamixSceneDecoder.BlockSize);
                    sceneBlockReport.AppendLine($"{file.Path}  {block.Index,5}  {placements[block.Index],6}  "
                        + $"{block.Kind,4}  {block.Behavior,8}  0x{block.Flags:X8}  {block.ColorMapOffset,8}  "
                        + $"{BinaryPrimitives.ReadInt32LittleEndian(record.Slice(0x40, 4)),7}  "
                        + $"{BinaryPrimitives.ReadInt16LittleEndian(record.Slice(0x48, 2)),7}  {BinaryPrimitives.ReadInt16LittleEndian(record.Slice(0x4A, 2)),7}  {BinaryPrimitives.ReadInt16LittleEndian(record.Slice(0x4C, 2)),7}  "
                        + $"{block.Width}x{block.Height}  {block.Surface0},{block.Surface1},{block.Surface2},{block.Surface3}  "
                        + block.Name.Replace('\r', ' ').Replace('\n', ' '));
                }
            }
            var colorMaps = new DynamixSceneColorMaps(Enumerable.Range(0, DynamixSceneColorMaps.Count).Select(index =>
            {
                var entry = archive.Entries.Single(candidate =>
                    candidate.Name.Equals($"Pal{index}", StringComparison.Ordinal));
                return DynamixSceneColorMapDecoder.Decode(entry.Name, archive.ReadDecoded(entry));
            }));
            var generated = DynamixSceneColorMapGenerator.RegenerateFirstFamily(
                colorMaps, skirmishPalette, scene.ColorMapping);
            var generatedExact = Enumerable.Range(0, scene.ColorMapping.MapCount)
                .All(index => colorMaps[index].Span.SequenceEqual(generated[index].Span));
            if (!generatedExact)
                throw new InvalidDataException("Stored first-family color maps do not match executable generation.");
            var offsets = string.Join(',', scene.Blocks.Select(block => block.ColorMapOffset).Distinct().Order());
            sceneScenarioReport.AppendLine($"{Convert.ToInt32(scene.ColorMapping.Enabled),7}  "
                + $"{scene.ColorMapping.MapCount,8}  {scene.ColorMapping.DistanceShift,13}  "
                + $"{scene.ColorMapping.BlendTarget,11}  {generatedExact,9}  {offsets,-12}  {file.Path}");
        }
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
File.WriteAllText(Path.Combine(output, "scene-scenario-report.txt"), sceneScenarioReport.ToString());
if (reportSceneBlocks)
{
    File.WriteAllText(Path.Combine(output, "scene-block-report.txt"), sceneBlockReport.ToString());
    Console.WriteLine($"Inspected scene block metadata and placements to {output}.");
    return 0;
}
File.WriteAllText(Path.Combine(output, "stored-palette-report.txt"), paletteReport.ToString());
File.WriteAllText(Path.Combine(output, "sound-bank-report.txt"), soundBankReport.ToString());
var smackerReport = new StringBuilder("# Version  Dimensions  Frames  Frame ms  Palette changes  Audio packets  Decoded audio  Final frame SHA-256  Audio tracks  Bytes  ISO path\n");
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
        var videoDecoder = new SmackerVideoDecoder(movie, source);
        var indices = new byte[checked(movie.Width * movie.Height)];
        var palette = new byte[768];
        var paletteChanges = 0;
        var audioPackets = 0;
        long decodedAudioBytes = 0;
        var renderSmk = renderSmkName is not null
            && Path.GetFileName(file.Path).Equals(renderSmkName, StringComparison.OrdinalIgnoreCase);
        var renderFrames = renderSmk
            ? new HashSet<int>([0, movie.Frames.Count / 4, movie.Frames.Count / 2,
                movie.Frames.Count * 3 / 4, movie.Frames.Count - 1])
            : [];
        for (var index = 0; index < movie.Frames.Count; index++)
        {
            var frame = SmackerMovieDecoder.DecodeFrameLayout(movie, index, source, palette);
            palette = frame.Palette;
            videoDecoder.DecodeFrame(source.AsSpan(frame.Video.Offset, frame.Video.Length), indices,
                movie.Frames[index].IsKeyFrame);
            if (renderFrames.Contains(index))
            {
                var renderRoot = Path.Combine(artifactRoot, "smacker");
                Directory.CreateDirectory(renderRoot);
                WriteIndexedPpm(Path.Combine(renderRoot,
                        $"{SafeName(Path.GetFileNameWithoutExtension(file.Path))}-{index:D5}.ppm"),
                    movie.Width, movie.Height, indices, palette);
            }
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
        var frameHash = Convert.ToHexString(SHA256.HashData(indices)).ToLowerInvariant();
        smackerReport.AppendLine(FormattableString.Invariant(
            $"SMK{movie.Version,-4}  {movie.Width}x{movie.Height,-10}  {movie.Frames.Count,6}  {movie.FrameDuration.TotalMilliseconds,8:0.###}  {paletteChanges,15}  {audioPackets,13}  {decodedAudioBytes,13}  {frameHash}  {audio,-28}  {file.Size,9}  {file.Path}"));
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

static int ParseNodeId(string value)
{
    var hexadecimal = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
    var digits = hexadecimal ? value[2..] : value;
    return int.TryParse(digits, hexadecimal
            ? System.Globalization.NumberStyles.HexNumber
            : System.Globalization.NumberStyles.None, null, out var id) && id >= 0
        ? id
        : throw new ArgumentException($"Invalid conversation node identifier '{value}'.");
}

static int ParseWeaponRecordId(string value)
{
    var hexadecimal = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase);
    var digits = hexadecimal ? value[2..] : value;
    return int.TryParse(digits, hexadecimal
            ? System.Globalization.NumberStyles.HexNumber
            : System.Globalization.NumberStyles.None, null, out var id) && id >= 0
        ? id
        : throw new ArgumentException($"Invalid weapon-store record identifier '{value}'.");
}

static string ReportText(string value) => value.Replace('\r', ' ').Replace('\n', ' ');

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
    var module = FindLinearExecutableModuleStart(bytes, le);
    var dataPages = checked((uint)module + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x80, 4)));
    var report = new StringBuilder("# 32-bit LE disassembly (derived metadata; original bytes omitted)\n");
    report.AppendLine($"# bound-module file offset 0x{module:X}; LE file offset 0x{le:X}; objects {objectCount}; page size 0x{pageSize:X}");
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
    var module = FindLinearExecutableModuleStart(bytes, le);
    var dataPages = checked((uint)module + BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(le + 0x80, 4)));
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

static int FindLinearExecutableModuleStart(ReadOnlySpan<byte> source, int linearHeader)
{
    for (var offset = linearHeader; offset >= 0; offset--)
    {
        if (offset > source.Length - 0x40 || source[offset] != (byte)'M' || source[offset + 1] != (byte)'Z')
            continue;
        var relativeHeader = BinaryPrimitives.ReadUInt32LittleEndian(source.Slice(offset + 0x3c, 4));
        if (relativeHeader <= int.MaxValue && offset + (int)relativeHeader == linearHeader)
            return offset;
    }
    throw new InvalidDataException("The Linear Executable header is not owned by a bounded MZ module.");
}

static string FindLinearExecutableFixupsBySource(string path, IReadOnlyList<uint> addresses)
{
    var fixups = LinearExecutableFixupReader.ReadInternalFixups(File.ReadAllBytes(path));
    var report = new StringBuilder("# 32-bit LE fixups near requested source addresses (derived metadata)\n");
    report.AppendLine($"# requested: {string.Join(',', addresses.Select(address => $"0x{address:X}"))}");
    foreach (var fixup in fixups.Where(fixup => addresses.Any(address => Math.Abs((long)fixup.SourceAddress - address) <= 16))
        .OrderBy(fixup => fixup.SourceAddress))
        report.AppendLine($"0x{fixup.SourceAddress:X8}  object{fixup.TargetObject}+0x{fixup.TargetOffset:X}  source-type 0x{fixup.SourceType:X2}{(fixup.Additive ? " additive" : "")}{(fixup.Chained ? " chained" : "")}");
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

static void WriteIndexedPpm(string path, int width, int height, ReadOnlySpan<byte> indices, ReadOnlySpan<byte> palette)
{
    if (width <= 0 || height <= 0 || indices.Length != checked(width * height) || palette.Length != 768)
        throw new InvalidDataException("Indexed image buffers are inconsistent.");
    var header = Encoding.ASCII.GetBytes($"P6\n{width} {height}\n255\n");
    var bytes = new byte[checked(header.Length + indices.Length * 3)];
    header.CopyTo(bytes, 0);
    var target = header.Length;
    foreach (var index in indices)
    {
        var color = index * 3;
        bytes[target++] = palette[color];
        bytes[target++] = palette[color + 1];
        bytes[target++] = palette[color + 2];
    }
    File.WriteAllBytes(path, bytes);
}

static string Hash(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static void AppendAction(StringBuilder report, DynamixActionTreeDatabase database, int offset, string indent, HashSet<int> active)
{
    if (!active.Add(offset))
    {
        report.AppendLine($"{indent}action@0x{offset:X} cycle");
        return;
    }
    var action = database.Actions[offset];
    report.AppendLine($"{indent}{action.Kind}@0x{offset:X}: {FormatExpression(database, action.ExpressionOffset, [])}");
    foreach (var branch in action.BranchActionOffsets)
        AppendAction(report, database, branch, indent + "  ", active);
    active.Remove(offset);
}

static string FormatExpression(DynamixActionTreeDatabase database, int offset, HashSet<int> active)
{
    if (!active.Add(offset)) return $"expr@0x{offset:X}:cycle";
    var expression = database.Expressions[offset];
    var result = FormatValue(database, expression.ValueOffsets[0], active);
    for (var index = 0; index < expression.Operators.Count; index++)
        result = $"({result} {expression.Operators[index]} {FormatValue(database, expression.ValueOffsets[index + 1], active)})";
    active.Remove(offset);
    return result;
}

static string FormatValue(DynamixActionTreeDatabase database, int offset, HashSet<int> active)
{
    var value = database.Values[offset];
    var text = value.Kind switch
    {
        DynamixValueKind.Literal => value.Value.ToString(),
        DynamixValueKind.Expression => FormatExpression(database, value.Value, active),
        DynamixValueKind.Function => $"F{value.Value}({string.Join(',', value.ArgumentExpressionOffsets.Select(argument => FormatExpression(database, argument, active)))})",
        _ => "?"
    };
    return value.Invert ? $"NOT({text})" : text;
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
