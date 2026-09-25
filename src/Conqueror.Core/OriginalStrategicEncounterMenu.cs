namespace Conqueror.Core;

/// <summary>
/// One fixed hit region in resolver RULE-BATTLE-001's encounter-choice dialog.
/// A null interactive selection code is the dialog's exit path, which makes
/// resolver RULE-BATTLE-001 take its automatic fallback.
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
/// RULE-BATTLE-001. The four interactive values are deliberately numeric: the
/// executable's later battle setup uses them for distinct formations, but the
/// player-facing tactical meanings have not yet been recovered.
/// </summary>
public static class OriginalStrategicEncounterMenu
{
    public const int InteractiveSelectionCount = 4;
    public const int ExitRegionIndex = 4;

    /// <summary>Regions are in original input-table order.</summary>
    public static IReadOnlyList<OriginalStrategicEncounterMenuEntry> Entries { get; } =
        Array.AsReadOnly<OriginalStrategicEncounterMenuEntry>([
            new(0, 30, 30, 215, 185, 2),
            new(1, 400, 30, 205, 200, 3),
            new(2, 25, 250, 185, 185, 1),
            new(3, 375, 250, 185, 185, 0),
            new(4, 220, 417, 148, 38, null),
        ]);

    /// <summary>
    /// Mirrors the RULE-BATTLE-004 call at RULE-BATTLE-001: scan the
    /// five original regions in table order and accept a left/top edge while
    /// excluding the right/bottom edge. A null result is a missed dialog
    /// click; a returned entry with a null selection code is the fifth
    /// region's distinct automatic-fallback exit.
    /// </summary>
    public static OriginalStrategicEncounterMenuEntry? FindMappedEntryAt(int x, int y)
    {
        foreach (var entry in Entries)
        {
            if (x >= entry.X && x < checked(entry.X + entry.Width)
                && y >= entry.Y && y < checked(entry.Y + entry.Height))
                return entry;
        }

        return null;
    }
}
