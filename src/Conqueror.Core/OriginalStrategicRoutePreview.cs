namespace Conqueror.Core;

/// <summary>
/// Recreates the route preview of RULE-STRATEGY-015. It is presentation-only:
/// route construction and movement remain in <see cref="OriginalStrategicPlayerCommands"/>
/// and <see cref="OriginalStrategicPlayerMovement"/>.
/// </summary>
public static class OriginalStrategicRoutePreview
{
    public const int FrameCount = 4;
    public const int DestinationXOffset = 0x12;
    public const int MarkerSpacing = 0x14;

    /// <summary>
    /// Produces the physical marker.CSF draws for the selected active player
    /// route. The source only draws while route input is live, begins each
    /// segment at the current or prior endpoint, and adjusts an authored
    /// destination x coordinate by <c>0x12</c> before rasterization.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMapMarkerDraw> BuildDraws(
        OriginalStrategicCampaignState state,
        int framePhase)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (framePhase is < 0 or >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(framePhase));
        state.Validate();
        if (!state.PlayerRouteInputActive) return [];

        var player = state.PlayerMovementSlots[state.SelectedPlayerMovementSlot];
        if (!player.Active || player.WaypointCount == 0) return [];

        var draws = new List<OriginalStrategicMapMarkerDraw>();
        var startX = TruncateTowardZero(player.CurrentX);
        var startY = TruncateTowardZero(player.CurrentY);
        var waypointCount = Math.Min(player.WaypointCount, player.Waypoints.Count);
        for (var waypointIndex = 0; waypointIndex < waypointCount; waypointIndex++)
        {
            var waypoint = player.Waypoints[waypointIndex];
            var endX = checked(waypoint.X - DestinationXOffset);
            var endY = waypoint.Y;
            AppendSegment(draws, player.Slot, startX, startY, endX, endY, framePhase);
            startX = endX;
            startY = endY;
        }
        return draws;
    }

    private static void AppendSegment(
        List<OriginalStrategicMapMarkerDraw> draws,
        int slot,
        int startX,
        int startY,
        int endX,
        int endY,
        int framePhase)
    {
        var counter = framePhase;
        var lastX = 0;
        var lastY = 0;
        var deltaX = Math.Abs(checked(endX - startX));
        var deltaY = Math.Abs(checked(endY - startY));

        if (deltaX < deltaY)
        {
            if (startY > endY) Swap(ref startX, ref startY, ref endX, ref endY);
            var stepX = endX > startX ? 1 : -1;
            deltaX = Math.Abs(checked(endX - startX));
            deltaY = endY - startY;
            var error = checked(2 * deltaX - deltaY);
            var x = startX;
            for (var y = checked(startY + 1); y <= endY; y++)
            {
                if (error >= 0)
                {
                    error = checked(error + 2 * (deltaX - deltaY));
                    x = checked(x + stepX);
                }
                else error = checked(error + 2 * deltaX);
                AppendIfSpaced(draws, slot, x, y, ref lastX, ref lastY, ref counter);
            }
            return;
        }

        if (startX > endX) Swap(ref startX, ref startY, ref endX, ref endY);
        var stepY = endY > startY ? 1 : -1;
        deltaX = endX - startX;
        deltaY = Math.Abs(checked(endY - startY));
        var shallowError = checked(2 * deltaY - deltaX);
        var yPosition = startY;
        for (var x = checked(startX + 1); x <= endX; x++)
        {
            if (shallowError >= 0)
            {
                shallowError = checked(shallowError + 2 * (deltaY - deltaX));
                yPosition = checked(yPosition + stepY);
            }
            else shallowError = checked(shallowError + 2 * deltaY);
            AppendIfSpaced(draws, slot, x, yPosition, ref lastX, ref lastY, ref counter);
        }
    }

    private static void AppendIfSpaced(
        List<OriginalStrategicMapMarkerDraw> draws,
        int slot,
        int x,
        int y,
        ref int lastX,
        ref int lastY,
        ref int counter)
    {
        if (Math.Abs(checked(lastX - x)) <= MarkerSpacing
            && Math.Abs(checked(lastY - y)) <= MarkerSpacing)
            return;

        draws.Add(new OriginalStrategicMapMarkerDraw(slot, x, y, counter % FrameCount));
        counter++;
        lastX = x;
        lastY = y;
    }

    private static int TruncateTowardZero(float value) => checked((int)Math.Truncate(value));

    private static void Swap(ref int leftX, ref int leftY, ref int rightX, ref int rightY)
    {
        (leftX, rightX) = (rightX, leftX);
        (leftY, rightY) = (rightY, leftY);
    }
}
