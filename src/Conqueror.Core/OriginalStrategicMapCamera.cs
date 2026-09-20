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
