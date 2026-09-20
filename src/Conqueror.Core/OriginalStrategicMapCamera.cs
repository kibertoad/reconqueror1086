namespace Conqueror.Core;

/// <summary>
/// Maps the strategic screen's raw-pointer edge panning at
/// <c>0x3D83C-0x3D8B5</c>. The application owns conversion of a physical
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
    /// Maps focus adapter <c>0x130E4</c> and helper <c>0x3C6E0</c>. The
    /// helper first wraps the requested grid row minus two in the 200-row
    /// world and subtracts eleven from the grid column. The caller then
    /// clamps the resulting render camera rather than trying to center an
    /// unavailable edge location.
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
    /// Moves at most one row and one column. Right/bottom edges are strict
    /// greater-than tests, while left/top edges are strict less-than tests,
    /// matching the original branch ordering and allowing diagonal panning.
    /// </summary>
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
