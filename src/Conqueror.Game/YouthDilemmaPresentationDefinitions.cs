using Conqueror.Resources;

namespace Conqueror.Game;

public static class YouthDilemmaPresentationDefinitions
{
    public static IReadOnlyList<UiBounds> Choices { get; } =
    [
        new(62, 310, 100, 150),
        new(268, 310, 100, 150),
        new(478, 310, 100, 150)
    ];

    public static IReadOnlyList<UiBounds> ChoicesFrom(HatLayout? layout) => Enumerable.Range(0, Choices.Count)
        .Select(id => layout?.FindRegion(id) is { Enabled: not 0 } region
            ? new UiBounds(region.X, region.Y, region.Width, region.Height)
            : Choices[id])
        .ToArray();
}
