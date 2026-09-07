using System.Text.Json;

namespace Conqueror.Game;

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
                var manifest = JsonSerializer.Deserialize<ImportManifest>(File.ReadAllText(manifestPath));
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

    private Stream? OpenAsset(ImportedAsset asset)
    {
        var path = Path.GetFullPath(Path.Combine(_root, asset.Path.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(_root, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return null;
        return File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    private sealed record ImportManifest(int Version, string? SourceImageSha256, ImportedAsset[]? Assets);
    private sealed record ImportedAsset(string Id, string Path, string Kind, long Size, string Sha256);
}
