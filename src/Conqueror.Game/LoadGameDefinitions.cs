namespace Conqueror.Game;

public static class LoadGameDefinitions
{
    // Confirmed from the decoded 640x480 LOADGAME.PCX artwork. No matching HAT
    // descriptor occurs in the inspected GOB, so these remain presentation bounds.
    public static IReadOnlyList<UiBounds> Slots { get; } =
    [
        new(32, 125, 584, 37),
        new(32, 170, 584, 37),
        new(32, 215, 584, 37),
        new(32, 262, 584, 37),
        new(32, 307, 584, 37)
    ];

    public static UiBounds Resume { get; } = new(13, 382, 64, 55);
}
