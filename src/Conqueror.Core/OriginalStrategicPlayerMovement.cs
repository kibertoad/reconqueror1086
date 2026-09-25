namespace Conqueror.Core;

/// <summary>
/// Mutable counterpart of one record of <c>player_forces</c> (FMT-STRATEGY-001,
/// RULE-STRATEGY-010). The first five records are armies; record five
/// is the special player-avatar record.
/// </summary>
public sealed class OriginalStrategicPlayerMovementSlot
{
    public int Slot { get; set; }
    public bool Active { get; set; }
    public int State8 { get; set; }
    public bool PathComplete { get; set; }
    public int TargetHandle { get; set; }
    public int WaypointCount { get; set; }
    public int WaypointIndex { get; set; }
    public int DestinationX { get; set; }
    public int DestinationY { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public int CollisionCooldown { get; set; }
    public int TerrainKind { get; set; }
    public float CurrentX { get; set; }
    public float CurrentY { get; set; }
    public float DirectionX { get; set; }
    public float DirectionY { get; set; }
    public List<OriginalStrategicRoutePoint> Waypoints { get; init; } = [];

    public bool TargetsEnemyMovement => (TargetHandle & OriginalStrategicMovement.PlayerEnemyTargetFlag) != 0;
    public bool TargetsDivisionForce => (TargetHandle & OriginalStrategicMovement.PlayerDivisionTargetFlag) != 0;
    public int TargetIndex => TargetHandle & 0xFF;

    public void Validate()
    {
        var targetKind = TargetHandle
            & (OriginalStrategicMovement.PlayerEnemyTargetFlag
                | OriginalStrategicMovement.PlayerDivisionTargetFlag);
        if (Slot is < 0 or >= OriginalStrategicMovement.PlayerMovementRecordCount
            || State8 < 0
            || TargetHandle < 0
            || (TargetHandle != 0
                && (targetKind is not OriginalStrategicMovement.PlayerEnemyTargetFlag
                    and not OriginalStrategicMovement.PlayerDivisionTargetFlag
                    || (TargetHandle & ~0x110FF) != 0))
            || WaypointCount < 0
            || WaypointCount > OriginalStrategicMovement.PlayerWaypointCapacity
            || WaypointIndex < 0 || WaypointIndex > WaypointCount
            || CollisionCooldown < 0 || TerrainKind is < 0 or >= OriginalStrategicMovement.TerrainKindCount
            || !float.IsFinite(CurrentX) || !float.IsFinite(CurrentY)
            || !float.IsFinite(DirectionX) || !float.IsFinite(DirectionY)
            || Waypoints is null || Waypoints.Count > OriginalStrategicMovement.PlayerWaypointCapacity
            || WaypointCount > Waypoints.Count)
            throw new InvalidDataException("Player strategic movement record contains invalid persisted fields.");
        if (Active && !PathComplete && TargetHandle == 0 && WaypointIndex >= Waypoints.Count)
            throw new InvalidDataException("Active player strategic movement record has no route or target.");
    }

}

public readonly record struct OriginalStrategicPlayerTarget(
    bool Active,
    float CurrentX,
    float CurrentY);

public readonly record struct OriginalStrategicPlayerAdvance(
    int Slot,
    bool CompletionSignal,
    bool PathComplete,
    bool Moved,
    bool Blocked);

public static partial class OriginalStrategicMovement
{
    public static IReadOnlyList<OriginalStrategicPlayerAdvance> AdvancePlayerMovementPass(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPlayerTarget> worldTargets)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(worldTargets);
        state.Validate();

        var slots = state.PlayerMovementSlots.ToDictionary(slot => slot.Slot);
        var advances = new List<OriginalStrategicPlayerAdvance>();
        for (var index = 0; index < PlayerMovementRecordCount; index++)
        {
            var slot = slots[index];
            if (!slot.Active) continue;
            if (slot.CollisionCooldown > 0) slot.CollisionCooldown--;
            advances.Add(AdvancePlayerMovementSlot(slot, state, resources, worldTargets));
        }
        return advances.AsReadOnly();
    }

    public static OriginalStrategicPlayerPassResult AdvancePlayerPass(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPlayerTarget> worldTargets,
        bool modalActive = false,
        bool suppressDragonEntry = false)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(worldTargets);
        state.Validate();

        var players = state.PlayerMovementSlots.ToDictionary(slot => slot.Slot);
        var enemies = state.MovementSlots.ToDictionary(slot => slot.Slot);
        var advances = new List<OriginalStrategicPlayerAdvance>();
        var contacts = new List<OriginalStrategicPlayerEnemyContact>();
        var avatarAlerts = new List<OriginalStrategicPlayerAvatarAlert>();
        for (var playerIndex = 0; playerIndex < PlayerMovementRecordCount; playerIndex++)
        {
            var player = players[playerIndex];
            if (!player.Active) continue;
            if (player.CollisionCooldown > 0) player.CollisionCooldown--;
            advances.Add(AdvancePlayerMovementSlot(player, state, resources, worldTargets));

            for (var enemyIndex = 0; enemyIndex < SlotCount; enemyIndex++)
            {
                var enemy = enemies[enemyIndex];
                if (!enemy.Active
                    || Math.Abs((double)player.CurrentX - enemy.CurrentX)
                        >= PlayerEnemyContactDistance
                    || Math.Abs((double)player.CurrentY - enemy.CurrentY)
                        >= PlayerEnemyContactDistance)
                    continue;

                state.SelectedPlayerMovementSlot = playerIndex;
                if (playerIndex != PlayerAvatarMovementSlot && !modalActive)
                {
                    contacts.Add(new(playerIndex, enemyIndex));
                    modalActive = true;
                    continue;
                }
                if (playerIndex == PlayerAvatarMovementSlot && player.CollisionCooldown == 0)
                {
                    avatarAlerts.Add(new(playerIndex, enemyIndex));
                    player.CollisionCooldown = PlayerAvatarCollisionCooldown;
                }
            }

            if (!suppressDragonEntry && playerIndex == state.EngagedPlayerMovementSlot
                && IsDragonEntryCell(player.GridX, player.GridY))
                return new(
                    advances.AsReadOnly(), contacts.AsReadOnly(), avatarAlerts.AsReadOnly(), true);
        }

        return new(advances.AsReadOnly(), contacts.AsReadOnly(), avatarAlerts.AsReadOnly(), false);
    }

    private static OriginalStrategicPlayerAdvance AdvancePlayerMovementSlot(
        OriginalStrategicPlayerMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPlayerTarget> worldTargets)
    {
        if (slot.PathComplete)
            return new(slot.Slot, false, true, false, false);

        int remainingX;
        int remainingY;
        if (slot.TargetHandle != 0)
        {
            if (!TryResolvePlayerTarget(slot, state, worldTargets, out var target))
            {
                slot.WaypointCount = 0;
                slot.WaypointIndex = 0;
                slot.PathComplete = false;
                slot.TargetHandle = 0;
                if (slot.Waypoints.Count == 0) slot.Waypoints.Add(default);
                return new(slot.Slot, true, false, false, false);
            }

            slot.DestinationX = Truncate(target.CurrentX);
            slot.DestinationY = Truncate(target.CurrentY);
            remainingX = checked(slot.DestinationX - Truncate(slot.CurrentX));
            remainingY = checked(slot.DestinationY - Truncate(slot.CurrentY));
            SetPlayerDirection(slot, remainingX, remainingY);
        }
        else
        {
            if (slot.WaypointIndex >= slot.Waypoints.Count)
                throw new InvalidDataException(
                    $"Player movement record {slot.Slot} has no waypoint at {slot.WaypointIndex}.");
            var waypoint = slot.Waypoints[slot.WaypointIndex];
            slot.DestinationX = waypoint.X;
            slot.DestinationY = waypoint.Y;
            remainingX = checked(waypoint.X - Truncate(slot.CurrentX));
            remainingY = checked(waypoint.Y - Truncate(slot.CurrentY));
        }

        if (ReachedOrPassedWaypoint(
                remainingX, remainingY, slot.DirectionX, slot.DirectionY))
        {
            slot.WaypointIndex++;
            if (slot.WaypointIndex >= slot.WaypointCount || slot.TargetHandle != 0)
            {
                slot.PathComplete = true;
                slot.WaypointCount = 0;
                slot.WaypointIndex = 0;
                slot.TargetHandle = 0;
                return new(slot.Slot, true, true, false, false);
            }

            var waypoint = slot.Waypoints[slot.WaypointIndex];
            slot.DestinationX = waypoint.X;
            slot.DestinationY = waypoint.Y;
            remainingX = checked(waypoint.X - Truncate(slot.CurrentX));
            remainingY = checked(waypoint.Y - Truncate(slot.CurrentY));
            SetPlayerDirection(slot, remainingX, remainingY);
        }

        var speed = TerrainSpeed((int)state.TerrainProfile, slot.TerrainKind);
        if (slot.DirectionX is > PlayerDirectionLimit or < -PlayerDirectionLimit
            || slot.DirectionY is > PlayerDirectionLimit or < -PlayerDirectionLimit)
            return StopBlockedPlayerMovement(slot, state);

        var probeX = checked(Truncate(slot.CurrentX)
            + Truncate(slot.DirectionX) * Truncate(speed) * state.SpeedMultiplier);
        var probeY = checked(Truncate(slot.CurrentY)
            + Truncate(slot.DirectionY) * Truncate(speed) * state.SpeedMultiplier);
        var prospective = ResolvePlayerTerrain(slot, state, resources, probeX, probeY);
        var prospectiveKind = TerrainKindForTile(prospective.TileId);

        if (slot.TerrainKind == ImpassableTerrainKind)
        {
            var blockedProbeX = checked(Truncate(slot.CurrentX)
                + Truncate((float)(slot.DirectionX * PlayerBlockedProbeDistance)));
            var blockedProbeY = checked(Truncate(slot.CurrentY)
                + Truncate((float)(slot.DirectionY * PlayerBlockedProbeDistance)));
            var blockedProbe = ResolvePlayerTerrain(
                slot, state, resources, blockedProbeX, blockedProbeY);
            if (TerrainKindForTile(blockedProbe.TileId) == ImpassableTerrainKind)
                return StopBlockedPlayerMovement(slot, state);
        }

        slot.TerrainKind = prospectiveKind;
        slot.CurrentX = (float)(slot.CurrentX
            + (double)slot.DirectionX * speed * state.SpeedMultiplier);
        slot.CurrentY = (float)(slot.CurrentY
            + (double)slot.DirectionY * speed * state.SpeedMultiplier);
        RefreshPlayerGrid(slot, state, resources);
        return new(slot.Slot, false, slot.PathComplete, true, false);
    }

    private static bool TryResolvePlayerTarget(
        OriginalStrategicPlayerMovementSlot slot,
        OriginalStrategicCampaignState state,
        IReadOnlyList<OriginalStrategicPlayerTarget> worldTargets,
        out OriginalStrategicPlayerTarget target)
    {
        if (slot.TargetsEnemyMovement)
        {
            if (slot.TargetIndex >= SlotCount)
                throw new InvalidDataException(
                    $"Player movement record {slot.Slot} targets invalid enemy slot {slot.TargetIndex}.");
            var enemy = state.MovementSlots.Single(candidate => candidate.Slot == slot.TargetIndex);
            target = new(enemy.Active, enemy.CurrentX, enemy.CurrentY);
        }
        else if (slot.TargetsDivisionForce)
        {
            if (slot.TargetIndex >= worldTargets.Count)
                throw new InvalidDataException(
                    $"Player movement record {slot.Slot} targets missing world record {slot.TargetIndex}.");
            target = worldTargets[slot.TargetIndex];
        }
        else
            throw new InvalidDataException(
                $"Player movement record {slot.Slot} has untagged target handle {slot.TargetHandle}.");
        return target.Active;
    }

    private static void SetPlayerDirection(
        OriginalStrategicPlayerMovementSlot slot,
        int remainingX,
        int remainingY)
    {
        var length = IntegerLength(remainingX, remainingY);
        if (length <= 0)
        {
            slot.DirectionX = 0;
            slot.DirectionY = 0;
            return;
        }
        slot.DirectionX = (float)remainingX / length;
        slot.DirectionY = (float)remainingY / length;
    }

    private static OriginalStrategicPlayerAdvance StopBlockedPlayerMovement(
        OriginalStrategicPlayerMovementSlot slot,
        OriginalStrategicCampaignState state)
    {
        state.SelectedPlayerMovementSlot = slot.Slot;
        slot.TargetHandle = 0;
        slot.WaypointCount = 0;
        slot.WaypointIndex = 0;
        slot.PathComplete = true;
        slot.DestinationX = Truncate(slot.CurrentX);
        slot.DestinationY = Truncate(slot.CurrentY);
        return new(slot.Slot, false, true, false, true);
    }

    private static OriginalStrategicTerrainCell ResolvePlayerTerrain(
        OriginalStrategicPlayerMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int worldX,
        int worldY)
    {
        if (!resources.TryTerrainCell(
                worldX, worldY, state.CameraRow, state.CameraColumn, out var cell))
            throw new InvalidDataException(
                $"Player movement record {slot.Slot} left the original route plane.");
        slot.GridX = cell.Row;
        slot.GridY = cell.Column;
        var mutation = state.TerrainMutations.FirstOrDefault(candidate =>
            candidate.Row == cell.Row && candidate.Column == cell.Column);
        return mutation is null ? cell : cell with { RawValue = mutation.CellValue };
    }

    private static void RefreshPlayerGrid(
        OriginalStrategicPlayerMovementSlot slot,
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources) =>
        _ = ResolvePlayerTerrain(
            slot, state, resources, Truncate(slot.CurrentX), Truncate(slot.CurrentY));

    public static bool IsDragonEntryCell(int x, int y) =>
        x == 63 && y - 2 == 114
        || x + 1 == 63 && y == 114
        || x - 1 == 63 && y - 1 == 114
        || x == 63 && y == 114
        || x == 63 && y + 1 == 114;
}

public readonly record struct OriginalStrategicPlayerEnemyContact(int PlayerSlot, int EnemySlot);

public readonly record struct OriginalStrategicPlayerAvatarAlert(int PlayerSlot, int EnemySlot);

public sealed record OriginalStrategicPlayerPassResult(
    IReadOnlyList<OriginalStrategicPlayerAdvance> Advances,
    IReadOnlyList<OriginalStrategicPlayerEnemyContact> Contacts,
    IReadOnlyList<OriginalStrategicPlayerAvatarAlert> AvatarAlerts,
    bool DragonEntryTriggered);
