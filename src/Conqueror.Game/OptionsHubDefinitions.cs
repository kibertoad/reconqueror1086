using Conqueror.Resources;

namespace Conqueror.Game;

public enum OptionsHubAction
{
    NewGame,
    Load,
    Save,
    Resume,
    Practice,
    Credits,
    Movie,
    ToggleCdMusic,
    ToggleMidiMusic,
    ToggleSoundEffects,
    ToggleSpeech,
    ToggleAnimation,
    Exit
}

public sealed record OptionsHubOption(
    string Label,
    OptionsHubAction Action,
    UiBounds OriginalBounds,
    int HatRegionId,
    bool RequiresCampaign = false);

public static class OptionsHubDefinitions
{
    public static IReadOnlyList<OptionsHubOption> Options { get; } =
    [
        new("New Game", OptionsHubAction.NewGame, new(15, 243, 255, 237), 3),
        new("Load", OptionsHubAction.Load, new(310, 233, 100, 55), 6),
        new("Save", OptionsHubAction.Save, new(411, 233, 100, 55), 7, true),
        new("Resume", OptionsHubAction.Resume, new(525, 155, 80, 40), 11, true),
        new("Practice", OptionsHubAction.Practice, new(340, 0, 145, 230), 5),
        new("Credits", OptionsHubAction.Credits, new(290, 320, 140, 160), 8),
        new("Movie", OptionsHubAction.Movie, new(430, 320, 150, 160), 9),
        new("CD Music", OptionsHubAction.ToggleCdMusic, new(160, 0, 120, 85), 4),
        new("MIDI Music", OptionsHubAction.ToggleMidiMusic, new(510, 15, 90, 70), 10),
        new("Sound Effects", OptionsHubAction.ToggleSoundEffects, new(145, 90, 100, 65), 0),
        new("Digitized Speech", OptionsHubAction.ToggleSpeech, new(200, 160, 90, 60), 12),
        new("Animation", OptionsHubAction.ToggleAnimation, new(500, 85, 85, 70), 1),
        new("Exit", OptionsHubAction.Exit, new(10, 20, 75, 65), 2)
    ];

    public static IReadOnlyList<OptionsHubOption> OptionsFrom(HatLayout? layout) => Options
        .Select(option => layout?.FindRegion(option.HatRegionId) is { } region
            ? option with { OriginalBounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : option)
        .ToArray();
}
