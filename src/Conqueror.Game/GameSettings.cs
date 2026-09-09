using System.Text.Json;
using Conqueror.Resources;

namespace Conqueror.Game;

public sealed record GameSettings(
    int Version = GameSettingsStore.CurrentVersion,
    bool CdMusic = true,
    bool SoundEffects = true,
    bool Speech = true,
    bool Animation = true,
    bool Fullscreen = false,
    bool IntegerScaling = false);

public sealed class GameSettingsStore(string path)
{
    public const int CurrentVersion = 1;

    private readonly string _path = Path.GetFullPath(path);
    public string BackupPath => _path + ".bak";

    public GameSettings Load()
    {
        if (TryLoad(_path, out var settings) || TryLoad(BackupPath, out settings)) return settings!;
        return new GameSettings();
    }

    public void Save(GameSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        if (settings.Version != CurrentVersion) throw new InvalidDataException("Cannot save an unsupported settings version.");
        if (TryLoad(_path, out _)) AtomicFile.Copy(_path, BackupPath);
        AtomicFile.WriteAllText(_path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static bool TryLoad(string path, out GameSettings? settings)
    {
        settings = null;
        if (!File.Exists(path)) return false;
        try
        {
            settings = JsonSerializer.Deserialize<GameSettings>(File.ReadAllText(path));
            return settings?.Version == CurrentVersion;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException
                                      or NotSupportedException or ArgumentException)
        {
            return false;
        }
    }
}
