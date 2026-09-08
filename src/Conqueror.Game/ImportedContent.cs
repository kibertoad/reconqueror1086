using Conqueror.Resources;
using System.Text.Json;

namespace Conqueror.Game;

public sealed record ImportedArtDefinition(string Role, string Kind, string IdSuffix);
public sealed record ImportedLayoutDefinition(string Role, string IdSuffix);

public static class ImportedArt
{
    public static IReadOnlyList<ImportedArtDefinition> Definitions { get; } =
    [
        new("Title.Background", "image", ":fftitle.pcx"),
        new("Character.Options", "image", ":char_ops.pcx"),
        new("Character.Pregenerated", "image", ":pregen.pcx"),
        new("Load.Background", "image", ":loadgame.pcx"),
        new("Map.England", "image", ":engmap1.pcx"),
        new("Tournament.Richard", "image", ":richard.pcc")
    ];
}

public static class ImportedLayouts
{
    public static IReadOnlyList<ImportedLayoutDefinition> Definitions { get; } =
    [
        new("Title", ":title.hat"),
        new("Character.Options", ":cgopts.hat"),
        new("Character.Pregenerated", ":pregen.hat")
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

    private Stream? OpenAsset(ImportedAsset asset)
    {
        var path = Path.GetFullPath(Path.Combine(_root, asset.Path.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return null;
        return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

}
