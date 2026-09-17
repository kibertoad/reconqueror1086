namespace Conqueror.Core;

/// <summary>
/// Fixed-update motion kernel for the original five enemy movement records.
/// Generator construction and completion/contact resolution remain separate,
/// matching the call boundaries around dispatcher <c>0x3C088</c>.
/// </summary>
public static partial class OriginalStrategicMovement
{
    public static IReadOnlyList<OriginalStrategicSlotAdvance> AdvanceMovementPass(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPursuitTarget> pursuitTargets)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(pursuitTargets);
        state.Validate();
        if (pursuitTargets.Count != SlotCount)
            throw new ArgumentException($"Pursuit requires exactly {SlotCount} player movement targets.",
                nameof(pursuitTargets));

        var slots = state.MovementSlots.ToDictionary(slot => slot.Slot);
        var advances = new List<OriginalStrategicSlotAdvance>();
        for (var index = 0; index < SlotCount; index++)
        {
            var slot = slots[index];
            if (!slot.Active) continue;
            var modeBefore = slot.Mode;
            var completionSignal = slot.Mode switch
            {
                DirectPropertyMode => AdvanceDirect(slot, state, resources),
                RoutedMode => AdvanceRouted(slot, state, resources),
                PursuitMode => AdvancePursuit(slot, state, resources, pursuitTargets),
                _ => throw new InvalidDataException(
                    $"Strategic movement slot {slot.Slot} has unsupported mode {slot.Mode}.")
            };
            advances.Add(new OriginalStrategicSlotAdvance(
                slot.Slot, modeBefore, slot.Mode, completionSignal, slot.Active));
        }
        return advances;
    }

    private static bool AdvanceDirect(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources)
    {
        var prospectiveX = Truncate(slot.CurrentX + slot.DirectionX);
        var prospectiveY = Truncate(slot.CurrentY + slot.DirectionY);
        var remainingX = slot.DestinationX - prospectiveX;
        var remainingY = slot.DestinationY - prospectiveY;
        if (PassedDirectDestination(remainingX, slot.DirectionX)
            || PassedDirectDestination(remainingY, slot.DirectionY))
        {
            slot.DestinationX = Truncate(slot.CurrentX);
            slot.DestinationY = Truncate(slot.CurrentY);
            return true;
        }

        var terrain = TerrainAtProspectivePosition(slot, state, resources);
        if (TerrainKindForTile(terrain.TileId) == ImpassableTerrainKind)
        {
            slot.DestinationX = Truncate(slot.CurrentX);
            slot.DestinationY = Truncate(slot.CurrentY);
            slot.Active = false;
            var origin = state.Properties[slot.OriginProperty];
            origin.Garrison = unchecked((byte)(origin.Garrison + slot.Total));
            return true;
        }

        var speed = TerrainSpeed((int)state.TerrainProfile, TerrainKindForTile(terrain.TileId));
        slot.CurrentX = (float)(slot.CurrentX
            + (double)slot.DirectionX * DirectTerrainScale * speed * state.SpeedMultiplier);
        slot.CurrentY = (float)(slot.CurrentY
            + (double)slot.DirectionY * DirectTerrainScale * speed * state.SpeedMultiplier);
        RefreshGrid(slot, state, resources);
        return false;
    }

    private static bool AdvanceRouted(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources)
    {
        if (slot.PathComplete) return false;
        var route = resources.Route(slot.RouteResource!, slot.RouteReversed);
        if (route.Count != slot.WaypointCount)
            throw new InvalidDataException(
                $"Strategic movement slot {slot.Slot} route length does not match '{slot.RouteResource}'.");
        if (slot.WaypointIndex >= route.Count)
            return CompleteRoute(slot, deactivate: false);

        var waypoint = route[slot.WaypointIndex];
        slot.DestinationX = waypoint.X;
        slot.DestinationY = waypoint.Y;
        var remainingX = waypoint.X - Truncate(slot.CurrentX);
        var remainingY = waypoint.Y - Truncate(slot.CurrentY);
        if (ReachedOrPassedWaypoint(remainingX, remainingY, slot.DirectionX, slot.DirectionY))
        {
            slot.WaypointIndex++;
            if (slot.WaypointIndex >= route.Count)
                return CompleteRoute(slot, deactivate: false);
            waypoint = route[slot.WaypointIndex];
            slot.DestinationX = waypoint.X;
            slot.DestinationY = waypoint.Y;
            remainingX = waypoint.X - Truncate(slot.CurrentX);
            remainingY = waypoint.Y - Truncate(slot.CurrentY);
        }

        var length = IntegerLength(remainingX, remainingY);
        if (length <= 0) return CompleteRoute(slot, deactivate: true);
        var normalizedX = (float)remainingX / length;
        var normalizedY = (float)remainingY / length;

        if (Truncate(slot.DirectionX) is < -MaximumRoutedStep or > MaximumRoutedStep
            || Truncate(slot.DirectionY) is < -MaximumRoutedStep or > MaximumRoutedStep)
            return CompleteRoute(slot, deactivate: true);

        var terrain = TerrainAtProspectivePosition(slot, state, resources);
        var terrainKind = TerrainKindForTile(terrain.TileId);
        if (terrainKind == ImpassableTerrainKind)
            return CompleteRoute(slot, deactivate: true);

        var step = CalculateRoutedStep(
            normalizedX, normalizedY, state.SpeedMultiplier, (int)state.TerrainProfile, terrainKind);
        slot.DirectionX = step.DeltaX;
        slot.DirectionY = step.DeltaY;
        if (step.Outcome == StrategicRoutedStepOutcome.Apply)
        {
            slot.CurrentX += step.DeltaX;
            slot.CurrentY += step.DeltaY;
        }
        return false;
    }

    private static bool AdvancePursuit(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPursuitTarget> pursuitTargets)
    {
        var target = pursuitTargets[slot.TargetMovementSlot];
        if (!target.Active) return true;

        slot.DestinationX = Truncate(target.CurrentX);
        slot.DestinationY = Truncate(target.CurrentY);
        var remainingX = slot.DestinationX - Truncate(slot.CurrentX + slot.DirectionX);
        var remainingY = slot.DestinationY - Truncate(slot.CurrentY + slot.DirectionY);
        SetNormalizedDirection(slot, remainingX, remainingY);

        var terrain = TerrainAtProspectivePosition(slot, state, resources);
        var terrainKind = TerrainKindForTile(terrain.TileId);
        if (terrainKind == ImpassableTerrainKind)
        {
            var origin = state.Properties[slot.OriginProperty];
            slot.DestinationX = origin.MapX8;
            slot.DestinationY = origin.MapY8;
            SetNormalizedDirection(
                slot,
                slot.DestinationX - Truncate(slot.CurrentX),
                slot.DestinationY - Truncate(slot.CurrentY));
            slot.Mode = DirectPropertyMode;
            slot.CurrentX += slot.DirectionX;
            slot.CurrentY += slot.DirectionY;
            return false;
        }

        var speed = TerrainSpeed((int)state.TerrainProfile, terrainKind);
        slot.CurrentX = (float)(slot.CurrentX
            + (double)slot.DirectionX * speed * state.SpeedMultiplier);
        slot.CurrentY = (float)(slot.CurrentY
            + (double)slot.DirectionY * speed * state.SpeedMultiplier);
        RefreshGrid(slot, state, resources);
        return false;
    }

    private static bool CompleteRoute(OriginalStrategicMovementSlot slot, bool deactivate)
    {
        slot.PathComplete = true;
        if (deactivate) slot.Active = false;
        return true;
    }

    private static void SetNormalizedDirection(
        OriginalStrategicMovementSlot slot,
        int remainingX,
        int remainingY)
    {
        var length = IntegerLength(remainingX, remainingY);
        if (length <= 0)
        {
            slot.DirectionX = 1;
            slot.DirectionY = 1;
            return;
        }
        slot.DirectionX = (float)remainingX / length;
        slot.DirectionY = (float)remainingY / length;
    }

    private static int IntegerLength(int x, int y)
    {
        var squared = checked((long)x * x + (long)y * y);
        if (squared > int.MaxValue)
            throw new InvalidDataException("Strategic movement vector exceeds the original integer range.");
        return (int)Math.Sqrt(squared);
    }

    private static bool PassedDirectDestination(int remaining, float direction) =>
        remaining > 0 && direction < 0 || remaining < 0 && direction > 0;

    private static bool ReachedOrPassedWaypoint(
        int remainingX,
        int remainingY,
        float directionX,
        float directionY) =>
        Math.Abs(remainingX) < WaypointCoordinateTolerance
        && Math.Abs(remainingY) < WaypointCoordinateTolerance
        || remainingX >= 0 && directionX < 0
        || remainingX <= 0 && directionX > 0
        || remainingY >= 0 && directionY < 0
        || remainingY <= 0 && directionY > 0;

    private static OriginalStrategicTerrainCell TerrainAtProspectivePosition(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources) =>
        ResolveTerrain(
            slot,
            state,
            resources,
            checked(Truncate(slot.CurrentX) + Truncate(slot.DirectionX)),
            checked(Truncate(slot.CurrentY) + Truncate(slot.DirectionY)));

    private static void RefreshGrid(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources) =>
        _ = ResolveTerrain(
            slot,
            state,
            resources,
            Truncate(slot.CurrentX),
            Truncate(slot.CurrentY));

    private static OriginalStrategicTerrainCell ResolveTerrain(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int worldX,
        int worldY)
    {
        if (!resources.TryTerrainCell(
                worldX, worldY, state.CameraRow, state.CameraColumn, out var cell))
            throw new InvalidDataException(
                $"Strategic movement slot {slot.Slot} left the original route plane.");
        slot.GridX = cell.Row;
        slot.GridY = cell.Column;
        var mutation = state.TerrainMutations.FirstOrDefault(candidate =>
            candidate.Row == cell.Row && candidate.Column == cell.Column);
        return mutation is null ? cell : cell with { RawValue = mutation.CellValue };
    }

    private static int Truncate(float value) => checked((int)value);
}

public readonly record struct OriginalStrategicPursuitTarget(
    bool Active,
    float CurrentX,
    float CurrentY);

public readonly record struct OriginalStrategicSlotAdvance(
    int Slot,
    int ModeBefore,
    int ModeAfter,
    bool CompletionSignal,
    bool ActiveAfter);
