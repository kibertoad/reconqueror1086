namespace Conqueror.Core;

public enum OriginalStrategicPlayerTargetKind
{
    EnemyMovement,
    DivisionForce
}

public readonly record struct OriginalStrategicPlayerCommandResult(
    bool Applied,
    bool RouteStarted,
    bool RouteLimitReached);

public readonly record struct OriginalStrategicPlayerMapHit(
    int? PlayerSlot,
    int? EnemySlot,
    int? DivisionSlot);

public static partial class OriginalStrategicMovement
{
    private static readonly StrategicPoint[] PlayerFormationOffsetRows =
    [
        new(-20, -30),
        new(20, -30),
        new(-30, 0),
        new(30, 0),
        new(0, 30),
        new(0, 0)
    ];

    public static IReadOnlyList<StrategicPoint> PlayerFormationOffsets =>
        Array.AsReadOnly(PlayerFormationOffsetRows);

    public static bool ConstructPlayerMovementRecord(
        OriginalStrategicCampaignState state,
        int playerSlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (playerSlot is < 0 or >= PlayerMovementRecordCount)
            throw new ArgumentOutOfRangeException(nameof(playerSlot));
        // 0x12CB6 tests > 5, so the sixth successful construction is retained.
        if (state.ActivePlayerRecordCount > PlayerArmyMovementCount) return false;

        var selected = SelectedPlayerSlot(state);
        var record = state.PlayerMovementSlots.Single(slot => slot.Slot == playerSlot);
        var offset = PlayerFormationOffsetRows[playerSlot];
        var anchorX = checked(80 * (state.PlayerHomeGridX + 1));
        var anchorY = checked(20 * (state.PlayerHomeGridY + 1));
        var currentX = checked(anchorX + offset.X);
        var currentY = checked(anchorY + offset.Y);

        state.ActivePlayerRecordCount++;
        record.DestinationX = currentX;
        record.DestinationY = currentY;
        record.State8 = 0;
        record.CollisionCooldown = 0;
        record.GridX = state.PlayerHomeGridX;
        record.GridY = state.PlayerHomeGridY;
        record.Active = true;
        record.PathComplete = true;
        record.CurrentX = currentX;
        record.CurrentY = currentY;

        // The executable clears command fields on the formerly selected record,
        // not on the record being constructed. Stale fields on a reused record survive.
        selected.TargetHandle = 0;
        selected.WaypointCount = 0;
        selected.WaypointIndex = 0;
        state.SelectedPlayerMovementSlot = playerSlot;
        return true;
    }

    public static bool RemovePlayerMovementRecord(
        OriginalStrategicCampaignState state,
        int playerSlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (playerSlot is < 0 or >= PlayerMovementRecordCount)
            throw new ArgumentOutOfRangeException(nameof(playerSlot));
        if (state.ActivePlayerRecordCount <= 0) return false;

        var records = state.PlayerMovementSlots.OrderBy(slot => slot.Slot).ToArray();
        var removed = records[playerSlot];
        var selectedSlot = state.SelectedPlayerMovementSlot;
        if (selectedSlot == playerSlot)
        {
            var replacement = records.FirstOrDefault(record =>
                record.Active && record.Slot != playerSlot);
            if (replacement is not null) selectedSlot = replacement.Slot;
        }

        if (state.EngagedPlayerMovementSlot == playerSlot)
        {
            state.EngagedPlayerMovementSlot = PlayerAvatarMovementSlot;
            var avatar = records[PlayerAvatarMovementSlot];
            avatar.CurrentX = removed.CurrentX;
            avatar.CurrentY = removed.CurrentY;
            avatar.GridX = removed.GridX;
            avatar.GridY = removed.GridY;
            avatar.Active = true;
            avatar.PathComplete = true;
            selectedSlot = PlayerAvatarMovementSlot;
        }

        removed.CollisionCooldown = 0;
        removed.Active = false;
        removed.State8 = 0;
        removed.PathComplete = true;
        state.ActivePlayerRecordCount--;
        state.SelectedPlayerMovementSlot = selectedSlot;
        return true;
    }

    public static void JoinPlayerArmy(
        OriginalStrategicCampaignState state,
        int armySlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (armySlot is < 0 or >= PlayerArmyMovementCount)
            throw new ArgumentOutOfRangeException(nameof(armySlot));

        var army = state.PlayerMovementSlots.Single(slot => slot.Slot == armySlot);
        var avatar = state.PlayerMovementSlots.Single(slot => slot.Slot == PlayerAvatarMovementSlot);
        state.EngagedPlayerMovementSlot = armySlot;
        army.Active = true;
        state.SelectedPlayerMovementSlot = armySlot;
        avatar.Active = false;
    }

    public static bool LeavePlayerArmy(
        OriginalStrategicCampaignState state,
        int armySlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (armySlot is < 0 or >= PlayerArmyMovementCount)
            throw new ArgumentOutOfRangeException(nameof(armySlot));

        var avatar = state.PlayerMovementSlots.Single(slot => slot.Slot == PlayerAvatarMovementSlot);
        if (avatar.Active) return false;
        var army = state.PlayerMovementSlots.Single(slot => slot.Slot == armySlot);

        state.EngagedPlayerMovementSlot = PlayerAvatarMovementSlot;
        avatar.CurrentX = army.CurrentX;
        avatar.CurrentY = army.CurrentY;
        avatar.GridX = army.GridX;
        avatar.GridY = army.GridY;
        avatar.TargetHandle = 0;
        avatar.WaypointCount = 0;
        avatar.WaypointIndex = 0;
        avatar.PathComplete = true;
        avatar.Active = true;
        state.PlayerRouteInputActive = false;
        state.SelectedPlayerMovementSlot = PlayerAvatarMovementSlot;
        return true;
    }

    public static OriginalStrategicPlayerCommandResult DispatchPlayerMapCommand(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        OriginalStrategicPlayerMapHit hit,
        int worldX,
        int worldY,
        bool targetConfirmed)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        if (hit.PlayerSlot is int player)
        {
            SelectPlayerMovementSlot(state, player);
            return new(true, false, false);
        }
        if (hit.EnemySlot is int enemy)
        {
            if (targetConfirmed)
                TargetPlayerMovementSlot(
                    state, OriginalStrategicPlayerTargetKind.EnemyMovement, enemy);
            return new(targetConfirmed, false, false);
        }
        if (hit.DivisionSlot is int division)
        {
            if (targetConfirmed)
                TargetPlayerMovementSlot(
                    state, OriginalStrategicPlayerTargetKind.DivisionForce, division);
            return new(targetConfirmed, false, false);
        }
        return AppendPlayerRoutePoint(state, resources, worldX, worldY);
    }

    public static void SelectPlayerMovementSlot(
        OriginalStrategicCampaignState state,
        int playerSlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (playerSlot is < 0 or >= PlayerMovementRecordCount)
            throw new ArgumentOutOfRangeException(nameof(playerSlot));
        if (!state.PlayerMovementSlots.Single(slot => slot.Slot == playerSlot).Active)
            throw new InvalidOperationException("Only an active player movement record can be selected.");

        state.SelectedPlayerMovementSlot = playerSlot;
        state.PlayerRouteInputActive = true;
    }

    public static void TargetPlayerMovementSlot(
        OriginalStrategicCampaignState state,
        OriginalStrategicPlayerTargetKind kind,
        int targetSlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        var maximum = kind switch
        {
            OriginalStrategicPlayerTargetKind.EnemyMovement => SlotCount,
            OriginalStrategicPlayerTargetKind.DivisionForce => PlayerDivisionTargetCount,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        if (targetSlot is < 0 || targetSlot >= maximum)
            throw new ArgumentOutOfRangeException(nameof(targetSlot));

        var selected = SelectedPlayerSlot(state);
        selected.TargetHandle = targetSlot | (kind switch
        {
            OriginalStrategicPlayerTargetKind.EnemyMovement => PlayerEnemyTargetFlag,
            OriginalStrategicPlayerTargetKind.DivisionForce => PlayerDivisionTargetFlag,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        });
        selected.WaypointCount = 0;
        selected.WaypointIndex = 0;
        selected.PathComplete = false;
        state.PlayerRouteInputActive = false;
    }

    public static OriginalStrategicPlayerCommandResult AppendPlayerRoutePoint(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int worldX,
        int worldY)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        state.Validate();
        var selected = SelectedPlayerSlot(state);
        if (!selected.Active)
            throw new InvalidOperationException("The selected player movement record is inactive.");

        var routeStarted = !state.PlayerRouteInputActive;
        if (routeStarted)
        {
            selected.TargetHandle = 0;
            selected.WaypointCount = 0;
            selected.WaypointIndex = 0;
            selected.PathComplete = false;
            state.PlayerRouteInputActive = true;
        }

        if (selected.WaypointCount >= PlayerRouteInputLimit)
        {
            PrimePlayerRoute(selected, state, resources);
            return new(false, routeStarted, true);
        }

        var point = new OriginalStrategicRoutePoint(worldX, worldY);
        if (selected.WaypointCount < selected.Waypoints.Count)
            selected.Waypoints[selected.WaypointCount] = point;
        else
            selected.Waypoints.Add(point);

        if (selected.WaypointCount == 0)
            PrimePlayerRoute(selected, state, resources);
        selected.PathComplete = false;
        selected.WaypointCount++;
        state.PlayerRouteInputActive = true;
        return new(true, routeStarted, false);
    }

    public static bool RemoveLastPlayerRoutePoint(OriginalStrategicCampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        if (!state.PlayerRouteInputActive) return false;

        var selected = SelectedPlayerSlot(state);
        if (selected.WaypointCount > 0) selected.WaypointCount--;
        if (selected.WaypointCount != 0) return true;

        selected.PathComplete = true;
        selected.TargetHandle = 0;
        state.PlayerRouteInputActive = false;
        return true;
    }

    public static void EndPlayerRouteInput(OriginalStrategicCampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();
        state.PlayerRouteInputActive = false;
    }

    private static OriginalStrategicPlayerMovementSlot SelectedPlayerSlot(
        OriginalStrategicCampaignState state) =>
        state.PlayerMovementSlots.Single(slot => slot.Slot == state.SelectedPlayerMovementSlot);

    private static void PrimePlayerRoute(
        OriginalStrategicPlayerMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources)
    {
        if (slot.Waypoints.Count == 0) return;
        var first = slot.Waypoints[0];
        slot.DestinationX = first.X;
        slot.DestinationY = first.Y;
        SetPlayerDirection(
            slot,
            checked(first.X - Truncate(slot.CurrentX)),
            checked(first.Y - Truncate(slot.CurrentY)));
        var probeX = checked(Truncate(slot.CurrentX) + Truncate(slot.DirectionX));
        var probeY = checked(Truncate(slot.CurrentY) + Truncate(slot.DirectionY));
        var terrain = ResolvePlayerTerrain(slot, state, resources, probeX, probeY);
        slot.TerrainKind = TerrainKindForTile(terrain.TileId);
    }
}
