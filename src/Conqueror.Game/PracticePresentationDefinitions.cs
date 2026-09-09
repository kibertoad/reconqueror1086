using Conqueror.Resources;

namespace Conqueror.Game;

public enum PracticeAction
{
    War,
    Joust,
    Melee,
    Exit,
    CastleSkirmish
}

public sealed record PracticeOption(
    string Label,
    PracticeAction Action,
    UiBounds OriginalBounds,
    int HatRegionId);

/// <summary>
/// PRACTICE.HAT regions in order, paired with the executable's contiguous
/// War/Joust/Melee/Exit/Castle Skirmish label table.
/// </summary>
public static class PracticePresentationDefinitions
{
    public static IReadOnlyList<PracticeOption> Options { get; } =
    [
        new("War", PracticeAction.War, new(58, 250, 250, 138), 0),
        new("Joust", PracticeAction.Joust, new(279, 140, 436, 70), 1),
        new("Melee", PracticeAction.Melee, new(374, 336, 202, 102), 2),
        new("Exit", PracticeAction.Exit, new(567, 239, 48, 18), 3),
        new("Castle Skirmish", PracticeAction.CastleSkirmish, new(1, 1, 234, 170), 4)
    ];

    public static IReadOnlyList<PracticeOption> OptionsFrom(HatLayout? layout) => Options
        .Select(option => layout?.FindRegion(option.HatRegionId) is { } region
            ? option with { OriginalBounds = new UiBounds(region.X, region.Y, region.Width, region.Height) }
            : option)
        .ToArray();
}
