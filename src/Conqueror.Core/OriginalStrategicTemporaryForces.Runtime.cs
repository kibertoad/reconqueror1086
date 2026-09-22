namespace Conqueror.Core;

/// <summary>
/// Fixed-update execution for the two recovered temporary-force patrols.
/// It mirrors updater <c>0x4A61C</c>: the route cursor loops instead of
/// becoming an arrival state, while terminal route failures retain the live
/// record for the surrounding lifecycle routine to resolve.
/// </summary>
public static partial class OriginalStrategicMovement
{
    /// <summary>
    /// Advances initialized automatic temporary-force records in physical slot
    /// order. Slot zero and records created by an unrecovered descriptor stay
    /// observable targets/markers but are not assigned a guessed route.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicTemporaryForceAdvance> AdvanceTemporaryForcePass(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        state.Validate();

        var advances = new List<OriginalStrategicTemporaryForceAdvance>();
        foreach (var slot in state.TemporaryForceSlots.OrderBy(slot => slot.Slot))
        {
            if (!slot.Active
                || slot.PathComplete
                || slot.WaypointCount == 0
                || !OriginalStrategicTemporaryForces.TryGetRoute(slot.Slot, out var descriptor))
                continue;
            advances.Add(AdvanceTemporaryForce(slot, descriptor, state, resources));
        }
        return advances;
    }

    private static OriginalStrategicTemporaryForceAdvance AdvanceTemporaryForce(
        OriginalStrategicTemporaryForceSlot slot,
        OriginalStrategicTemporaryForceRoute descriptor,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources)
    {
        var route = resources.Route(descriptor.ResourceName, reverse: false);
        if (route.Count != slot.WaypointCount)
            throw new InvalidDataException(
                $"Temporary strategic force slot {slot.Slot} route length does not match '{descriptor.ResourceName}'.");

        var looped = false;
        var waypoint = route[slot.WaypointIndex];
        slot.DestinationX = waypoint.X;
        slot.DestinationY = waypoint.Y;
        var remainingX = waypoint.X - Truncate(slot.CurrentX);
        var remainingY = waypoint.Y - Truncate(slot.CurrentY);
        if (ReachedOrPassedWaypoint(remainingX, remainingY, slot.DirectionX, slot.DirectionY))
        {
            slot.WaypointIndex++;
            if (slot.WaypointIndex >= slot.WaypointCount)
            {
                slot.WaypointIndex = 0;
                looped = true;
            }
            waypoint = route[slot.WaypointIndex];
            slot.DestinationX = waypoint.X;
            slot.DestinationY = waypoint.Y;
            remainingX = waypoint.X - Truncate(slot.CurrentX);
            remainingY = waypoint.Y - Truncate(slot.CurrentY);
        }

        var length = IntegerLength(remainingX, remainingY);
        if (length <= 0
            || Truncate(slot.DirectionX) is < -MaximumRoutedStep or > MaximumRoutedStep
            || Truncate(slot.DirectionY) is < -MaximumRoutedStep or > MaximumRoutedStep)
            return CompleteTemporaryForce(slot, looped);

        var terrain = ResolveTemporaryForceTerrain(
            slot,
            state,
            resources,
            checked(Truncate(slot.CurrentX) + Truncate(slot.DirectionX)),
            checked(Truncate(slot.CurrentY) + Truncate(slot.DirectionY)));
        var step = CalculateRoutedStep(
            (float)remainingX / length,
            (float)remainingY / length,
            state.SpeedMultiplier,
            (int)state.TerrainProfile,
            TerrainKindForTile(terrain.TileId));
        slot.DirectionX = step.DeltaX;
        slot.DirectionY = step.DeltaY;
        if (step.Outcome == StrategicRoutedStepOutcome.DeactivateForImpassableTerrain)
            return CompleteTemporaryForce(slot, looped);
        if (step.Outcome == StrategicRoutedStepOutcome.Apply)
        {
            slot.CurrentX += step.DeltaX;
            slot.CurrentY += step.DeltaY;
        }
        return new(
            Slot: slot.Slot,
            Looped: looped,
            CompletionSignal: false,
            ActiveAfter: slot.Active);
    }

    private static OriginalStrategicTemporaryForceAdvance CompleteTemporaryForce(
        OriginalStrategicTemporaryForceSlot slot,
        bool looped)
    {
        // 0x4A61C marks completion and clears its cursor/count, but leaves
        // lifecycle ownership (including active/expiry) to 0x3B0E4.
        slot.PathComplete = true;
        slot.WaypointIndex = 0;
        slot.WaypointCount = 0;
        return new(
            Slot: slot.Slot,
            Looped: looped,
            CompletionSignal: true,
            ActiveAfter: slot.Active);
    }

    private static OriginalStrategicTerrainCell ResolveTemporaryForceTerrain(
        OriginalStrategicTemporaryForceSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int worldX,
        int worldY)
    {
        if (!resources.TryTerrainCell(
                worldX, worldY, state.CameraRow, state.CameraColumn, out var cell))
            throw new InvalidDataException(
                $"Temporary strategic force slot {slot.Slot} left the original route plane.");
        slot.GridX = cell.Row;
        slot.GridY = cell.Column;
        var mutation = state.TerrainMutations.FirstOrDefault(candidate =>
            candidate.Row == cell.Row && candidate.Column == cell.Column);
        return mutation is null ? cell : cell with { RawValue = mutation.CellValue };
    }
}

/// <summary>One automatic temporary-force update result.</summary>
public readonly record struct OriginalStrategicTemporaryForceAdvance(
    int Slot,
    bool Looped,
    bool CompletionSignal,
    bool ActiveAfter);
