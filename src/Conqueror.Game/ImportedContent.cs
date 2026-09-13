using Conqueror.Resources;
using Conqueror.Core;
using System.Collections.Frozen;
using System.Text.Json;

namespace Conqueror.Game;

public sealed record ImportedArtDefinition(string Role, string Kind, string IdSuffix);
public sealed record ImportedRawArtDefinition(
    string Role, string IdSuffix, string PaletteIdSuffix, int Width, int Height);
public sealed record ImportedLayoutDefinition(string Role, string IdSuffix);
public sealed record ImportedAnimationDefinition(
    string Role, string IdSuffix, string PaletteArtRole, string? PaletteIdSuffix = null);
public sealed record ImportedSoundDefinition(string Role, string IdSuffix, int SampleIndex);
public sealed record ImportedMovieDefinition(string Role, string IdSuffix);
public sealed record PreparedImportedSound(string BankId, int SampleIndex, int SampleRate, byte[] Pcm16LittleEndian);

public static class ImportedArt
{
    public static IReadOnlyList<ImportedArtDefinition> Definitions { get; } =
    [
        new("Title.Background", "image", ":fftitle.pcx"),
        new("Options.Background", "image", ":optfin.pcx"),
        new("Practice.Background", "image", ":practice.pcx"),
        new("Character.Options", "image", ":char_ops.pcx"),
        new("Character.Pregenerated", "image", ":pregen.pcx"),
        new("Campaign.Briefing", "image", ":fluff.pcx"),
        new("Dilemma.Background", "image", ":morality.pcx"),
        new("Load.Background", "image", ":loadgame.pcx"),
        new("Estate.Shell", "image", ":icontemp.pcx"),
        new("Map.England", "image", ":engmap1.pcx"),
        new("Home.Office", "image", ":tactical.pcx"),
        new("Home.Overview", "image", ":f_over.pcx"),
        new("Home.WarPlanning", "image", ":warplan.pcx"),
        new("Farm.Management", "image", ":fiefmgmt.pcx"),
        new("Village.Inn", "image", ":innpeopl.pcx"),
        new("Blacksmith.Workshop", "image", ":forgesmi.pcx"),
        new("Dialogue.Frame", "image", ":comscrn1.pcx"),
        new("Dragon.Background", "image", ":drjstwin.pcx"),
        new("Encounter.Drogo", "image", ":drogo.pcc"),
        new("Blacksmith.Portrait", "image", ":blacksmi.pcc"),
        new("Shop.Inventory", "image", ":swdtemp.pcx"),
        new("Tournament.Richard", "image", ":richard.pcc"),
        .. InnPresentationDefinitions.Fallback.Patrons.Select(patron =>
            new ImportedArtDefinition(patron.PortraitRole, "image", patron.PortraitSuffix))
    ];
}

public static class ImportedLayouts
{
    public static IReadOnlyList<ImportedLayoutDefinition> Definitions { get; } =
    [
        new("Title", ":title.hat"),
        new("Options", ":gameopts.hat"),
        new("Practice", ":practice.hat"),
        new("Character.Options", ":cgopts.hat"),
        new("Character.Pregenerated", ":pregen.hat"),
        new("Dilemma", ":chargen.hat"),
        new("Estate", ":iconmap.hat"),
        new("Home.Office", ":fopts.hat"),
        new("Home.Overview", ":foview.hat"),
        new("Home.WarPlanning", ":fwarplan.hat"),
        new("Village.Inn", ":vinn.hat"),
        .. FarmPresentationDefinitions.Layouts.Select(layout =>
            new ImportedLayoutDefinition(layout.LayoutRole, layout.LayoutSuffix)),
        new("Blacksmith.Workshop", ":vsmith.hat")
    ];
}

public static class ImportedRawArt
{
    public static IReadOnlyList<ImportedRawArtDefinition> Definitions { get; } =
    [
        new("Combat.Shell", ":SKIRMISH.PCX", ":SKIRMISH.PAL", 320, 200)
    ];
}

public static class ImportedAnimations
{
    public static IReadOnlyList<ImportedAnimationDefinition> Definitions { get; } =
    [
        new("Options.Widgets", ":option.csf", "Options.Background"),
        new("Home.WarPlanning.Controls", ":warplan.csf", "Home.WarPlanning"),
        new("Shop.Items", ":swords.csf", "Shop.Inventory"),
        new("Shop.Controls", ":buysell.csf", "Shop.Inventory"),
        new("Interface.Cursor", ":ffmouse.csf", "Estate.Shell"),
        new("Combat.FirstPerson", ":skirmish.csf", "", ":SKIRMISH.PAL"),
        new("Dragon.Lance", ":lance1.csf", "Dragon.Background"),
        .. EstatePresentationDefinitions.TileAtlases.Select(atlas =>
            new ImportedAnimationDefinition(atlas.Role, atlas.IdSuffix, "Estate.Shell"))
    ];
}

public static class ImportedSounds
{
    public static IReadOnlyList<ImportedSoundDefinition> Definitions { get; } =
    [
        new("Interface.Activate", ":gameopts.666", 0)
    ];
}

public static class ImportedMovies
{
    public static IReadOnlyList<ImportedMovieDefinition> Definitions { get; } =
    [
        new("Title.Intro", "/title.smk"),
        new("Options.Credits", "/creditzz.smk"),
        new("Practice.Joust", "/jousprac.smk"),
        new("Travel.DragonLair", "/trandrag.smk"),
        new("Ending.DragonVictory", "/drjstwin.smk"),
        new("Ending.DragonInvestiture", "/champl30.smk"),
        new("Ending.DragonDefeat", "/drjstlse.smk"),
        new("Dragon.Retreat", "/drjstrun.smk"),
        new("Ending.CrownVictory", "/crownl30.smk"),
        new("Ending.AgeLimit", "/avg_end.smk")
    ];
}

public sealed class ImportedContentCatalog
{
    private readonly string _root;
    private readonly ImportedAsset[] _assets;
    public int Count => _assets.Length;
    public string SourceImageSha256 { get; }

    private ImportedContentCatalog(string root, ImportManifest manifest)
    {
        _root = Path.GetFullPath(root) + Path.DirectorySeparatorChar;
        _assets = manifest.Assets ?? [];
        SourceImageSha256 = manifest.SourceImageSha256 ?? "unknown";
    }

    public static ImportedContentCatalog? Discover(string? preferredRoot = null)
    {
        var configured = Environment.GetEnvironmentVariable("RECONQUEROR_USER_CONTENT") ??
            Environment.GetEnvironmentVariable("CONQUEROR_USER_CONTENT");
        var candidates = new[]
        {
            preferredRoot,
            configured,
            Path.Combine(Environment.CurrentDirectory, "UserContent"),
            Path.Combine(AppContext.BaseDirectory, "UserContent")
        };
        foreach (var root in candidates.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var manifestPath = Path.Combine(root!, "manifest.json");
            if (!File.Exists(manifestPath)) continue;
            try
            {
                var manifest = ImportManifest.Read(manifestPath);
                if (manifest is { Version: 1 }) return new ImportedContentCatalog(root!, manifest);
            }
            catch (JsonException) { }
        }
        return null;
    }

    public Stream? Open(string id)
    {
        var asset = _assets.FirstOrDefault(x => x.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        return asset is null ? null : OpenAsset(asset);
    }

    public Stream? OpenFirst(string kind)
    {
        var asset = _assets.FirstOrDefault(x => x.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase));
        return asset is null ? null : OpenAsset(asset);
    }

    public IReadOnlyList<string> Ids(string kind) => _assets.Where(x => x.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase)).Select(x => x.Id).ToArray();

    public string? FindId(string kind, string idSuffix) => _assets
        .FirstOrDefault(x => x.Kind.Equals(kind, StringComparison.OrdinalIgnoreCase) && x.Id.EndsWith(idSuffix, StringComparison.OrdinalIgnoreCase))?.Id;

    public PcxImage? DecodePcx(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return PcxDecoder.Decode(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public IndexedImage? DecodeRawIndexedImage(string id, string paletteId, int width, int height)
    {
        try
        {
            var palette = DecodePalette(paletteId);
            return palette is null ? null : RawIndexedImageDecoder.Decode(ReadBytes(id), palette.Rgb, width, height);
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public CsfSequence? DecodeCsf(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return new CsfSequence(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public HatLayout? DecodeHat(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return new HatLayout(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public IndexedPalette? DecodePalette(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return IndexedPaletteDecoder.Decode(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DilemmaTextResource? DecodeDilemma(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return DilemmaTextDecoder.Decode(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public WeaponStoreResource? DecodeWeaponStore(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return WeaponStoreDecoder.Decode(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixScene? DecodeScene(string archiveId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archiveId);
        var prefix = archiveId.Replace('\\', '/');
        var viewerId = _assets.FirstOrDefault(x => x.Id.Equals($"{prefix}#0:Viewer", StringComparison.OrdinalIgnoreCase))?.Id;
        var scenarioId = _assets.FirstOrDefault(x => x.Id.Equals($"{prefix}#1:Scenario", StringComparison.OrdinalIgnoreCase))?.Id;
        var mapId = _assets.FirstOrDefault(x => x.Id.EndsWith(":Map", StringComparison.OrdinalIgnoreCase) &&
            x.Id.StartsWith(prefix + "#", StringComparison.OrdinalIgnoreCase))?.Id;
        var blocksId = _assets.FirstOrDefault(x => x.Id.EndsWith(":Blocks", StringComparison.OrdinalIgnoreCase) &&
            x.Id.StartsWith(prefix + "#", StringComparison.OrdinalIgnoreCase))?.Id;
        var effectsId = _assets.FirstOrDefault(x => x.Id.EndsWith(":SFXDEFS", StringComparison.OrdinalIgnoreCase) &&
            x.Id.StartsWith(prefix + "#", StringComparison.OrdinalIgnoreCase))?.Id;
        if (viewerId is null || scenarioId is null || mapId is null || blocksId is null || effectsId is null) return null;
        try
        {
            return DynamixSceneDecoder.Decode(ReadBytes(viewerId), ReadBytes(scenarioId), ReadBytes(mapId),
                ReadBytes(blocksId), ReadBytes(effectsId));
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixSceneTexture? DecodeSceneTexture(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var separator = id.LastIndexOf(':');
        if (separator < 0 || separator == id.Length - 1) return null;
        try
        {
            return DynamixSceneTextureDecoder.Decode(id[(separator + 1)..], ReadBytes(id));
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixSceneBackdrop? DecodeSceneBackdrop(string archiveId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archiveId);
        var prefix = archiveId.Replace('\\', '/') + "#";
        var descriptorId = _assets.FirstOrDefault(asset => asset.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            asset.Id.EndsWith(":Backdrop", StringComparison.OrdinalIgnoreCase))?.Id;
        var imageId = _assets.FirstOrDefault(asset => asset.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) &&
            asset.Id.EndsWith(":BackImage", StringComparison.OrdinalIgnoreCase))?.Id;
        if (descriptorId is null || imageId is null) return null;
        try
        {
            return DynamixSceneBackdropDecoder.Decode(ReadBytes(descriptorId), ReadBytes(imageId));
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixSceneColorMaps? DecodeSceneColorMaps(string archiveId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(archiveId);
        var prefix = archiveId.Replace('\\', '/') + "#";
        var ids = _assets.Where(asset => asset.Id.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(asset => asset.Id)
            .Where(id => id[(id.LastIndexOf(':') + 1)..].StartsWith("Pal", StringComparison.Ordinal))
            .ToArray();
        if (ids.Length == 0) return null;
        try
        {
            return new DynamixSceneColorMaps(ids.Select(id =>
                DynamixSceneColorMapDecoder.Decode(id[(id.LastIndexOf(':') + 1)..], ReadBytes(id))));
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixConversationDatabase? DecodeConversations(string bodyId, string indexId)
    {
        using var body = Open(bodyId);
        using var index = Open(indexId);
        if (body is null || index is null) return null;
        try
        {
            using var bodyMemory = new MemoryStream();
            using var indexMemory = new MemoryStream();
            body.CopyTo(bodyMemory);
            index.CopyTo(indexMemory);
            return DynamixConversationDecoder.Decode(bodyMemory.ToArray(), indexMemory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private byte[] ReadBytes(string id)
    {
        using var stream = Open(id) ?? throw new InvalidDataException($"Imported asset '{id}' is unavailable.");
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    public DynamixActionTreeDatabase? DecodeActionTrees(string bodyId, string indexId)
    {
        using var body = Open(bodyId);
        using var index = Open(indexId);
        if (body is null || index is null) return null;
        try
        {
            using var bodyMemory = new MemoryStream();
            using var indexMemory = new MemoryStream();
            body.CopyTo(bodyMemory);
            index.CopyTo(indexMemory);
            return DynamixActionTreeDecoder.Decode(bodyMemory.ToArray(), indexMemory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixVariableTable? DecodeVariableTable(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return DynamixVariableTableDecoder.Decode(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    public DynamixSoundBank? DecodeSoundBank(string id)
    {
        using var stream = Open(id);
        if (stream is null) return null;
        try
        {
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            return DynamixSoundBankDecoder.Decode(memory.ToArray());
        }
        catch (InvalidDataException)
        {
            return null;
        }
    }

    private Stream? OpenAsset(ImportedAsset asset)
    {
        var path = Path.GetFullPath(Path.Combine(_root, asset.Path.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return null;
        return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

}

/// <summary>
/// Eager session cache for the small original sound-bank population. Decoding and unsigned
/// 8-to-signed-16-bit conversion happen once during game startup, never in an input handler.
/// </summary>
public sealed class ImportedSoundLibrary
{
    private readonly Dictionary<string, PreparedImportedSound[]> _banks = new(StringComparer.OrdinalIgnoreCase);
    public int BankCount => _banks.Count;
    public int SampleCount => _banks.Values.Sum(samples => samples.Length);

    private ImportedSoundLibrary()
    {
    }

    public static ImportedSoundLibrary Load(ImportedContentCatalog catalog)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        var library = new ImportedSoundLibrary();
        foreach (var id in catalog.Ids("sound-bank").Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var bank = catalog.DecodeSoundBank(id);
            if (bank is null) continue;
            library._banks.Add(id, bank.Samples.Select(sample => new PreparedImportedSound(
                id, sample.Index, sample.SampleRate, sample.ToPcm16LittleEndian())).ToArray());
        }
        return library;
    }

    public PreparedImportedSound? Find(string bankId, int sampleIndex) =>
        sampleIndex >= 0 && _banks.TryGetValue(bankId, out var samples)
            ? samples.ElementAtOrDefault(sampleIndex)
            : null;
}

/// <summary>Runtime access to locally imported original dilemma text.</summary>
public sealed class ImportedDialogueRepository(ImportedContentCatalog catalog)
{
    private readonly Dictionary<int, DilemmaTextResource?> _dilemmas = [];
    private readonly Dictionary<int, YouthDilemmaDefinition?> _playableDilemmas = [];
    private readonly int[] _numbers = catalog.Ids("resource")
        .Select(DilemmaNumberFromId)
        .Where(number => number.HasValue)
        .Select(number => number!.Value)
        .Distinct()
        .Order()
        .ToArray();

    public IReadOnlyList<int> AvailableNumbers => _numbers;

    public DilemmaTextResource? GetDilemma(int number)
    {
        if (number < 0) return null;
        if (_dilemmas.TryGetValue(number, out var cached)) return cached;
        var id = catalog.FindId("resource", $":dilem{number}.dat");
        var resource = id is null ? null : catalog.DecodeDilemma(id);
        _dilemmas.Add(number, resource);
        return resource;
    }

    public IReadOnlyList<DilemmaTextResource> GetDilemmasForAge(int age) => _numbers
        .Select(GetDilemma)
        .OfType<DilemmaTextResource>()
        .Where(dilemma => dilemma.Age == age)
        .OrderBy(dilemma => dilemma.Number)
        .ToArray();

    public YouthDilemmaDefinition? GetPlayableDilemma(int number)
    {
        if (_playableDilemmas.TryGetValue(number, out var cached)) return cached;
        var resource = GetDilemma(number);
        var playable = resource is null ? null : ImportedDilemmaAdapter.Convert(resource);
        _playableDilemmas.Add(number, playable);
        return playable;
    }

    private static int? DilemmaNumberFromId(string id)
    {
        var separator = id.LastIndexOf(':');
        var name = id[(separator + 1)..];
        const string prefix = "dilem";
        const string extension = ".dat";
        if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            || !name.EndsWith(extension, StringComparison.OrdinalIgnoreCase)) return null;
        var digits = name.AsSpan(prefix.Length, name.Length - prefix.Length - extension.Length);
        return int.TryParse(digits, out var number) && number >= 0 ? number : null;
    }
}

public static class ImportedDilemmaAdapter
{
    private static readonly FrozenDictionary<string, CharacterAttribute> Attributes =
        new Dictionary<string, CharacterAttribute>(StringComparer.OrdinalIgnoreCase)
        {
            ["NONE"] = CharacterAttribute.None,
            ["STRENGTH"] = CharacterAttribute.Strength,
            ["DEXTERITY"] = CharacterAttribute.Dexterity,
            ["INTELLIGENCE"] = CharacterAttribute.Intelligence,
            ["PIETY"] = CharacterAttribute.Piety,
            ["STAMINA"] = CharacterAttribute.Stamina,
            ["HONOR"] = CharacterAttribute.Honor,
            ["EXPERIENCE_WITH_SWORD"] = CharacterAttribute.SwordExperience,
            ["FAME"] = CharacterAttribute.Fame,
            ["AGE"] = CharacterAttribute.Age,
            ["WEALTH"] = CharacterAttribute.Wealth
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    public static YouthDilemmaDefinition? Convert(DilemmaTextResource resource)
    {
        var choices = new List<YouthDilemmaChoiceDefinition>(resource.Choices.Count);
        foreach (var choice in resource.Choices)
        {
            if (!Attributes.TryGetValue(choice.ScoringAttribute, out var scoringAttribute)) return null;
            var outcomes = new Dictionary<YouthDilemmaOutcome, YouthDilemmaOutcomeDefinition>();
            foreach (var outcome in choice.Outcomes)
            {
                var changes = new List<CharacterAttributeChange>(outcome.Changes.Count);
                foreach (var change in outcome.Changes)
                {
                    if (!Attributes.TryGetValue(change.Attribute, out var attribute)) return null;
                    changes.Add(new CharacterAttributeChange(attribute, change.Modifier));
                }
                if (!Enum.TryParse<YouthDilemmaOutcome>(outcome.Outcome.ToString(), out var outcomeKind)) return null;
                outcomes.Add(outcomeKind, new YouthDilemmaOutcomeDefinition(outcome.Text, changes));
            }
            choices.Add(new YouthDilemmaChoiceDefinition($"Choice {choice.Number}", scoringAttribute, choice.LowBreakpoint,
                choice.HighBreakpoint, outcomes.ToFrozenDictionary()));
        }
        return new YouthDilemmaDefinition(resource.Number, resource.Age, resource.Title, resource.Prompt, choices, resource.SceneFile);
    }
}
