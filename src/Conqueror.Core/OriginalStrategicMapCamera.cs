namespace Conqueror.Core;

/// <summary>
/// Maps the strategic screen's edge panning and focusing (RULE-STRATEGY-014).
/// The application owns conversion of a physical
/// pointer to this source coordinate space and requests its own redraw after
/// this state-only operation succeeds.
/// </summary>
public static class OriginalStrategicMapCamera
{
    public const int MinimumRow = 10;
    public const int MaximumRow = 192;
    public const int MinimumColumn = 2;
    public const int MaximumColumn = 300;
    public const int LeftOrTopThreshold = 8;
    public const int RightThreshold = 0x274;
    public const int BottomThreshold = 0x1DA;
    public const int FocusRowLead = 2;
    public const int FocusColumnLead = 11;

    /// <summary>
    /// Maps <c>focus_on</c> (RULE-STRATEGY-014).
    /// </summary>
    public static void FocusOnGridCell(
        OriginalStrategicCampaignState state,
        int gridRow,
        int gridColumn)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (gridRow is < 0 or >= OriginalStrategicCampaignState.WorldRowCount)
            throw new ArgumentOutOfRangeException(nameof(gridRow));
        if (gridColumn is < 0 or >= OriginalStrategicCampaignState.WorldColumnCount)
            throw new ArgumentOutOfRangeException(nameof(gridColumn));

        var wrappedRow = gridRow - FocusRowLead;
        if (wrappedRow < 0) wrappedRow += OriginalStrategicCampaignState.WorldRowCount;
        state.CameraRow = Math.Clamp(wrappedRow, MinimumRow, MaximumRow);
        state.CameraColumn = Math.Clamp(gridColumn - FocusColumnLead, MinimumColumn, MaximumColumn);
    }

    /// <summary>
    /// Moves at most one row and one column.
    /// </summary>
    // PLACEHOLDER: RULE-STRATEGY-014. The rule moves one axis per call, rows before columns, and
    // leaves map_busy set after a move; moving both axes at once is the rebuild's own.
    public static bool ApplyMappedEdgeScroll(
        OriginalStrategicCampaignState state,
        int pointerX,
        int pointerY)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();

        var changed = false;
        if (pointerX > RightThreshold && state.CameraRow < MaximumRow)
        {
            state.CameraRow++;
            changed = true;
        }
        else if (pointerX < LeftOrTopThreshold && state.CameraRow > MinimumRow)
        {
            state.CameraRow--;
            changed = true;
        }

        if (pointerY > BottomThreshold && state.CameraColumn < MaximumColumn)
        {
            state.CameraColumn++;
            changed = true;
        }
        else if (pointerY < LeftOrTopThreshold && state.CameraColumn > MinimumColumn)
        {
            state.CameraColumn--;
            changed = true;
        }
        return changed;
    }
}
