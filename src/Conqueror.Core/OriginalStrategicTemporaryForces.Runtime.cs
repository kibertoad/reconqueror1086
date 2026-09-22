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
    /// Applies source UI action identifiers after the ordinary strategic pass,
    /// at the same post-scheduler boundary as <c>0x3C2D6</c>. This is a raw
    /// action boundary, not a claim about the unrecovered visible controls.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicTemporaryForceCreation> ProcessTemporaryForceActions(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<int>? actionIds,
        IOriginalStrategicRandom random)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(random);
        state.Validate();
        if (actionIds is null || actionIds.Count == 0) return [];

        var creations = new List<OriginalStrategicTemporaryForceCreation>();
        foreach (var creator in OriginalStrategicTemporaryForces.Creators)
        {
            if (!actionIds.Contains(creator.ActionId)) continue;
            var slot = state.TemporaryForceSlots.Single(slot => slot.Slot == creator.DescriptorIndex);
            if (slot.Active) continue;
            if (!OriginalStrategicTemporaryForces.TryGetRoute(creator.DescriptorIndex, out var route))
                throw new InvalidDataException($"Temporary force creator {creator.ActionId:X} has no route.");
            var points = resources.Route(route.ResourceName, reverse: false);
            if (points.Count != route.PointCount)
                throw new InvalidDataException(
                    $"Temporary strategic force route length does not match '{route.ResourceName}'.");

            // Source 0x3B1A2/0x3B1B0 takes random(1), then
            // 0x3B1B8/0x3B1CB takes random(1) + 1, leaving knights zero.
            slot.Active = true;
            slot.PathComplete = false;
            slot.WaypointCount = points.Count;
            slot.WaypointIndex = 0;
            slot.Swordsmen = random.Next(2);
            slot.Halberdiers = checked(random.Next(2) + 1);
            slot.Knights = 0;
            slot.OriginProperty = creator.OriginProperty;
            slot.Lord = state.Properties[creator.OriginProperty].Lord;
            slot.Mode = RoutedMode;
            slot.CurrentX = points[0].X;
            slot.CurrentY = points[0].Y;
            slot.DestinationX = points[0].X;
            slot.DestinationY = points[0].Y;
            slot.DirectionX = 0;
            slot.DirectionY = 0;
            _ = ResolveTemporaryForceTerrain(
                slot, state, resources, points[0].X, points[0].Y);
            creations.Add(new(creator.ActionId, slot.Slot));
        }
        return creations;
    }

    /// <summary>
    /// Advances initialized automatic temporary-force records in physical slot
    /// order. Slot zero and records created by an unrecovered descriptor stay
    /// observable targets/markers but are not assigned a guessed route.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicTemporaryForceAdvance> AdvanceTemporaryForcePass(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        DateTime calendarDate)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        state.Validate();

        var advances = new List<OriginalStrategicTemporaryForceAdvance>();
        foreach (var slot in state.TemporaryForceSlots.OrderBy(slot => slot.Slot))
        {
            if (!slot.Active || !OriginalStrategicTemporaryForces.TryGetRoute(slot.Slot, out var descriptor)
                || !TryGetCreatorForSlot(slot.Slot, out var creator))
                continue;

            OriginalStrategicTemporaryForceAdvance? advance = null;
            if (!slot.PathComplete && slot.WaypointCount != 0)
                advance = AdvanceTemporaryForce(slot, descriptor, state, resources);

            // 0x3B831 invokes 0x4A61C before the two descriptor comparisons.
            // The comparisons are independent, rather than a conventional
            // lexicographic date test: month is zero based and each must be
            // at least its descriptor value to release the route and clear
            // the active record.
            if (calendarDate.Month - 1 >= creator.ExpiryMonth
                && calendarDate.Year >= creator.ExpiryYear)
            {
                // 0x3B86B-0x3B890 clears only the descriptor and force
                // active flags before releasing the heap route. The record's
                // completion, cursor, and direction fields stay intact until
                // a later creator overwrites them.
                slot.Active = false;
                advances.Add(new(slot.Slot, advance?.Looped ?? false,
                    advance?.CompletionSignal ?? false, ActiveAfter: false, ExpirationSignal: true));
            }
            else if (advance is { } result)
                advances.Add(result);
        }
        return advances;
    }

    private static bool TryGetCreatorForSlot(int slot, out OriginalStrategicTemporaryForceCreator creator)
    {
        creator = OriginalStrategicTemporaryForces.Creators.FirstOrDefault(candidate =>
            candidate.DescriptorIndex == slot)!;
        return creator is not null;
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
    bool ActiveAfter,
    bool ExpirationSignal = false);

/// <summary>One accepted source temporary-force creator action.</summary>
public readonly record struct OriginalStrategicTemporaryForceCreation(int ActionId, int Slot);
