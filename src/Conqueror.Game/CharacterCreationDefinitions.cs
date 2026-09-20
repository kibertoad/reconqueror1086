using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public enum CharacterCreationAction
{
    ChooseName,
    ChooseColor,
    GenerateNew,
    ChoosePregenerated
}

public sealed record UiBounds(int X, int Y, int Width, int Height)
{
    public bool Contains(int x, int y) => x >= X && y >= Y && x < X + Width && y < Y + Height;
}

public sealed record CharacterCreationOption(
    string Label,
    CharacterCreationAction Action,
    UiBounds OriginalBounds,
    int? HatRegionId);

public sealed record HeraldicColorOption(
    string Name,
    UiBounds OriginalBounds,
    int HatRegionId,
    int OriginalStrategicCharacterColor);

/// <summary>Original 640x480 character-options screen hotspots, kept as data rather than rendering branches.</summary>
public static class CharacterCreationDefinitions
{
    public static UiBounds NameEntryBounds { get; } = new(72, 164, 340, 35);

    public static IReadOnlyList<CharacterCreationOption> Options { get; } =
    [
        new("Choose Character Name", CharacterCreationAction.ChooseName, new(75, 38, 231, 144), 2),
        new("Choose Color", CharacterCreationAction.ChooseColor, new(470, 22, 165, 185), null),
        new("Generate New Character", CharacterCreationAction.GenerateNew, new(132, 209, 175, 95), 0),
        new("Choose Pre-generated Character", CharacterCreationAction.ChoosePregenerated, new(192, 342, 223, 106), 1)
    ];

    public static IReadOnlyList<UiBounds> PregeneratedCharacters { get; } =
    [
        new(69, 18, 119, 133), new(256, 18, 119, 133), new(439, 18, 119, 133),
        new(69, 250, 119, 133), new(256, 250, 119, 133), new(439, 250, 119, 133)
    ];

    public static IReadOnlyList<HeraldicColorOption> HeraldicColors { get; } =
    [
        new("Red", new(501, 102, 34, 39), 3, OriginalStrategicCharacterColor.Red),
        new("Green", new(552, 102, 34, 39), 4, OriginalStrategicCharacterColor.Green),
        new("Blue", new(525, 150, 34, 39), 5, OriginalStrategicCharacterColor.Blue)
    ];

    public static IReadOnlyList<CharacterCreationOption> OptionsFrom(HatLayout? layout) => Options
        .Select(option => option.HatRegionId is { } id && layout?.FindRegion(id) is { Enabled: not 0 } region
            ? option with { OriginalBounds = Bounds(region) }
            : option).ToArray();

    public static IReadOnlyList<HeraldicColorOption> ColorsFrom(HatLayout? layout) => HeraldicColors
        .Select(option => layout?.FindRegion(option.HatRegionId) is { Enabled: not 0 } region
            ? option with { OriginalBounds = Bounds(region) }
            : option).ToArray();

    public static IReadOnlyList<UiBounds> PregeneratedFrom(HatLayout? layout) => Enumerable.Range(0, PregeneratedCharacters.Count)
        .Select(id => layout?.FindRegion(id) is { Enabled: not 0 } region ? Bounds(region) : PregeneratedCharacters[id]).ToArray();

    private static UiBounds Bounds(HatRegion region) => new(region.X, region.Y, region.Width, region.Height);
}
