using System.Text.Json;
using Conqueror.Resources;

namespace Conqueror.Game;

// RULE-CONFIG-002. PLACEHOLDER: the original keeps these switches in CONQUER.INI (FMT-CONFIG-001);
// this JSON store, its volumes and its backup file are the rebuild's own.
public sealed record GameSettings
{
    public int Version { get; init; } = GameSettingsStore.CurrentVersion;
    public bool CdMusic { get; init; } = true;
    public bool SoundEffects { get; init; } = true;
    public bool Speech { get; init; } = true;
    public bool Animation { get; init; } = true;
    public bool Fullscreen { get; init; }
    public bool IntegerScaling { get; init; }
    public float MusicVolume { get; init; } = .35f;
    public float EffectsVolume { get; init; } = 1f;
    public float SpeechVolume { get; init; } = 1f;
    public bool ReducedMotion { get; init; }
}

public sealed class GameSettingsStore(string path)
{
    public const int CurrentVersion = 2;

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
            if (settings?.Version == 1) settings = settings with { Version = CurrentVersion };
            if (settings?.Version != CurrentVersion) return false;
            settings = settings with
            {
                MusicVolume = Math.Clamp(settings.MusicVolume, 0, 1),
                EffectsVolume = Math.Clamp(settings.EffectsVolume, 0, 1),
                SpeechVolume = Math.Clamp(settings.SpeechVolume, 0, 1)
            };
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or JsonException
                                      or NotSupportedException or ArgumentException)
        {
            return false;
        }
    }
}
