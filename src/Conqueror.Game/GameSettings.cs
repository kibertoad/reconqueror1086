using RefurbishedDinosaurs.Core.Persistence;

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

    private readonly JsonSettingsStore<GameSettings> _store = new(path) { MaximumBytes = 64 * 1024 };
    public string BackupPath => _store.BackupPath;

    public GameSettings Load()
    {
        var settings = _store.Load(() => new GameSettings(), IsSupported, Migrate);
        return settings with
        {
            MusicVolume = Math.Clamp(settings.MusicVolume, 0, 1),
            EffectsVolume = Math.Clamp(settings.EffectsVolume, 0, 1),
            SpeechVolume = Math.Clamp(settings.SpeechVolume, 0, 1)
        };
    }

    public void Save(GameSettings settings) => _store.Save(settings, IsSupported, Migrate);

    private static bool IsSupported(GameSettings settings) => settings.Version == CurrentVersion;

    private static GameSettings? Migrate(GameSettings settings) => settings.Version == 1
        ? settings with { Version = CurrentVersion } : null;
}
