namespace Conqueror.Core;

/// <summary>
/// Converts the original strategic screen's raw map-pointer coordinates to
/// route-space coordinates through helper <c>0x640A0</c>. This intentionally
/// does not convert physical display pixels; the application owns that input
/// boundary before supplying the source coordinate pair.
/// </summary>
public static class OriginalStrategicMapPointer
{
    public const int CellWidth = 80;
    public const int ColumnAdvance = 20;
    public const int RowOriginOffset = 40;

    public static OriginalStrategicRoutePoint ToMappedRoutePoint(
        OriginalStrategicCampaignState state,
        int rawPointerX,
        int rawPointerY)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        return new(
            checked(rawPointerX + state.CameraRow * CellWidth + RowOriginOffset),
            checked(rawPointerY + (state.CameraColumn + 1) * ColumnAdvance));
    }
}
