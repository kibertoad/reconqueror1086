using Conqueror.Resources;
using Conqueror.Core;
using System.Collections.Frozen;
using System.Text.Json;

namespace Conqueror.Game;

public sealed record ImportedArtDefinition(string Role, string Kind, string IdSuffix);
public sealed record ImportedLayoutDefinition(string Role, string IdSuffix);
public sealed record ImportedAnimationDefinition(string Role, string IdSuffix, string PaletteArtRole);

public static class ImportedArt
{
    public static IReadOnlyList<ImportedArtDefinition> Definitions { get; } =
    [
        new("Title.Background", "image", ":fftitle.pcx"),
        new("Options.Background", "image", ":optfin.pcx"),
        new("Character.Options", "image", ":char_ops.pcx"),
        new("Character.Pregenerated", "image", ":pregen.pcx"),
        new("Dilemma.Background", "image", ":morality.pcx"),
        new("Load.Background", "image", ":loadgame.pcx"),
        new("Estate.Shell", "image", ":icontemp.pcx"),
        new("Map.England", "image", ":engmap1.pcx"),
        new("Home.Office", "image", ":tactical.pcx"),
        new("Farm.Management", "image", ":fiefmgmt.pcx"),
        new("Blacksmith.Workshop", "image", ":forgesmi.pcx"),
        new("Blacksmith.Dialogue", "image", ":comscrn1.pcx"),
        new("Blacksmith.Portrait", "image", ":blacksmi.pcc"),
        new("Shop.Inventory", "image", ":swdtemp.pcx"),
        new("Tournament.Richard", "image", ":richard.pcc")
    ];
}

public static class ImportedLayouts
{
    public static IReadOnlyList<ImportedLayoutDefinition> Definitions { get; } =
    [
        new("Title", ":title.hat"),
        new("Options", ":gameopts.hat"),
        new("Character.Options", ":cgopts.hat"),
        new("Character.Pregenerated", ":pregen.hat"),
        new("Dilemma", ":chargen.hat"),
        new("Estate", ":iconmap.hat"),
        new("Home.Office", ":fcastle.hat"),
        new("Blacksmith.Workshop", ":vsmith.hat")
    ];
}

public static class ImportedAnimations
{
    public static IReadOnlyList<ImportedAnimationDefinition> Definitions { get; } =
    [
        new("Options.Widgets", ":option.csf", "Options.Background"),
        new("Shop.Items", ":swords.csf", "Shop.Inventory"),
        new("Shop.Controls", ":buysell.csf", "Shop.Inventory"),
        .. EstatePresentationDefinitions.TileAtlases.Select(atlas =>
            new ImportedAnimationDefinition(atlas.Role, atlas.IdSuffix, "Estate.Shell"))
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

    public static ImportedContentCatalog? Discover()
    {
        var configured = Environment.GetEnvironmentVariable("CONQUEROR_USER_CONTENT");
        var candidates = new[] { configured, Path.Combine(Environment.CurrentDirectory, "UserContent"), Path.Combine(AppContext.BaseDirectory, "UserContent") };
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

    private Stream? OpenAsset(ImportedAsset asset)
    {
        var path = Path.GetFullPath(Path.Combine(_root, asset.Path.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return null;
        return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

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
