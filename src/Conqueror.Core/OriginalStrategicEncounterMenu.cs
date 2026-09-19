namespace Conqueror.Core;

/// <summary>
/// One fixed hit region in resolver <c>0x256FC</c>'s encounter-choice dialog.
/// A null interactive selection code is the dialog's exit path, which makes
/// resolver <c>0x258FC</c> take its automatic fallback.
/// </summary>
public readonly record struct OriginalStrategicEncounterMenuEntry(
    int RegionIndex,
    int X,
    int Y,
    int Width,
    int Height,
    int? InteractiveSelectionCode)
{
    public bool ExitsToAutomaticFallback => InteractiveSelectionCode is null;
}

/// <summary>
/// Verified fixed dialog geometry and dispatch identity for strategic resolver
/// <c>0x256FC</c>. The four interactive values are deliberately numeric: the
/// executable's later battle setup uses them for distinct formations, but the
/// player-facing tactical meanings have not yet been recovered.
/// </summary>
public static class OriginalStrategicEncounterMenu
{
    public const int InteractiveSelectionCount = 4;
    public const int ExitRegionIndex = 4;

    /// <summary>
    /// Regions are in original input-table order. The input helper's exact
    /// edge-inclusion behavior remains an application-input concern, so this
    /// definition intentionally exposes geometry without inventing a click
    /// containment rule.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicEncounterMenuEntry> Entries { get; } =
        Array.AsReadOnly<OriginalStrategicEncounterMenuEntry>([
            new(0, 30, 30, 215, 185, 2),
            new(1, 400, 30, 205, 200, 3),
            new(2, 25, 250, 185, 185, 1),
            new(3, 375, 250, 185, 185, 0),
            new(4, 220, 417, 148, 38, null),
        ]);
}
