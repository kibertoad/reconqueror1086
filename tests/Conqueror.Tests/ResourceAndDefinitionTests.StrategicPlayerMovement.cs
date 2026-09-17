using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void PlayerRouteMovementUsesSixRecordOrderAndExactTerrainSpeed()
    {
        var state = RuntimeState();
        var terrainKind = OriginalStrategicMovement.TerrainKindForTile(18);
        ActivatePlayerRoute(state.PlayerMovementSlots[5], terrainKind,
            new OriginalStrategicRoutePoint(100, 0));
        ActivatePlayerRoute(state.PlayerMovementSlots[1], terrainKind,
            new OriginalStrategicRoutePoint(100, 0));
        state.PlayerMovementSlots[1].CollisionCooldown = 2;

        var advances = OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), []);

        Assert.Equal([1, 5], advances.Select(advance => advance.Slot));
        var speed = OriginalStrategicMovement.TerrainSpeed((int)state.TerrainProfile, terrainKind);
        Assert.Equal(speed, state.PlayerMovementSlots[1].CurrentX, precision: 5);
        Assert.Equal(speed, state.PlayerMovementSlots[5].CurrentX, precision: 5);
        Assert.Equal(1, state.PlayerMovementSlots[1].CollisionCooldown);
        Assert.Equal((7, 11),
            (state.PlayerMovementSlots[1].GridX, state.PlayerMovementSlots[1].GridY));
    }

    [Fact]
    public void PlayerRouteUsesStrictSixUnitArrivalAndAdvancesOnlyOneWaypoint()
    {
        var state = RuntimeState();
        var slot = state.PlayerMovementSlots[0];
        var terrainKind = OriginalStrategicMovement.TerrainKindForTile(18);
        ActivatePlayerRoute(slot, terrainKind,
            new OriginalStrategicRoutePoint(10, 0),
            new OriginalStrategicRoutePoint(20, 0));
        slot.CurrentX = 4;

        var boundary = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), []));

        Assert.False(boundary.CompletionSignal);
        Assert.Equal(0, slot.WaypointIndex);
        Assert.True(boundary.Moved);

        slot.CurrentX = 5;
        var next = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), []));
        Assert.False(next.CompletionSignal);
        Assert.Equal(1, slot.WaypointIndex);
        Assert.Equal((20, 0, 1f, 0f),
            (slot.DestinationX, slot.DestinationY, slot.DirectionX, slot.DirectionY));

        slot.CurrentX = 15;
        var complete = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), []));
        Assert.True(complete.CompletionSignal);
        Assert.True(slot.PathComplete);
        Assert.Equal((0, 0), (slot.WaypointCount, slot.WaypointIndex));
        state.Validate();
    }

    [Fact]
    public void PlayerEnemyTargetTracksTheLiveEnemyAndClearsWhenItDisappears()
    {
        var state = RuntimeState();
        var terrainKind = OriginalStrategicMovement.TerrainKindForTile(18);
        var player = state.PlayerMovementSlots[2];
        player.Active = true;
        player.TargetHandle = OriginalStrategicMovement.PlayerEnemyTargetFlag | 3;
        player.TerrainKind = terrainKind;
        var enemy = state.MovementSlots[3];
        ActivateRuntimeSlot(enemy, OriginalStrategicMovement.DirectPropertyMode);
        enemy.CurrentX = 100;

        var tracked = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), []));

        Assert.True(tracked.Moved);
        Assert.Equal((100, 0, 1f, 0f),
            (player.DestinationX, player.DestinationY, player.DirectionX, player.DirectionY));

        enemy.Active = false;
        var vanished = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), []));
        Assert.True(vanished.CompletionSignal);
        Assert.False(player.PathComplete);
        Assert.Equal(0, player.TargetHandle);
        state.Validate();
    }

    [Fact]
    public void PlayerWorldTargetUsesTheRawLowByteIdentityAndCompletesAtExactContact()
    {
        var state = RuntimeState();
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.TargetHandle = 2;
        var targets = new OriginalStrategicPlayerTarget[3];
        targets[2] = new(true, 0, 0);

        var result = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, new StubStrategicResources(), targets));

        Assert.True(result.CompletionSignal);
        Assert.True(player.PathComplete);
        Assert.Equal((0, 0, 0f, 0f),
            (player.TargetHandle, player.WaypointIndex, player.DirectionX, player.DirectionY));
    }

    [Fact]
    public void PlayerMovementStopsAndSelectsTheRecordWhenImpassableProbeRemainsBlocked()
    {
        var state = RuntimeState();
        state.SelectedPlayerMovementSlot = 0;
        var impassableTile = Enumerable.Range(0, OriginalStrategicMovement.TerrainTileKindCount)
            .First(tile => OriginalStrategicMovement.TerrainKindForTile(tile)
                == OriginalStrategicMovement.ImpassableTerrainKind);
        var slot = state.PlayerMovementSlots[4];
        ActivatePlayerRoute(slot, OriginalStrategicMovement.ImpassableTerrainKind,
            new OriginalStrategicRoutePoint(100, 0));
        var resources = new StubStrategicResources
        {
            Terrain = (_, _) => new OriginalStrategicTerrainCell(7, 11, (uint)impassableTile)
        };

        var blocked = Assert.Single(OriginalStrategicMovement.AdvancePlayerMovementPass(
            state, resources, []));

        Assert.True(blocked.Blocked);
        Assert.False(blocked.CompletionSignal);
        Assert.True(slot.PathComplete);
        Assert.Equal(4, state.SelectedPlayerMovementSlot);
        Assert.Equal((0, 0, 0, 0),
            (slot.TargetHandle, slot.WaypointCount, slot.WaypointIndex, slot.DestinationX));
        Assert.Equal(2, resources.TerrainLookups);
    }

    [Fact]
    public void PlayerContactUsesStrictThirtyUnitAxesAndPlayerThenEnemyOrder()
    {
        var state = RuntimeState();
        ActivateStationaryPlayer(state.PlayerMovementSlots[2], 100, 100);
        ActivateStationaryPlayer(state.PlayerMovementSlots[1], 100, 100);
        ActivateRuntimeSlot(state.MovementSlots[3], OriginalStrategicMovement.DirectPropertyMode);
        ActivateRuntimeSlot(state.MovementSlots[0], OriginalStrategicMovement.DirectPropertyMode);
        state.MovementSlots[3].CurrentX = 129.999f;
        state.MovementSlots[3].CurrentY = 70.001f;
        state.MovementSlots[0].CurrentX = 130;
        state.MovementSlots[0].CurrentY = 100;

        var result = OriginalStrategicMovement.AdvancePlayerPass(
            state, new StubStrategicResources(), []);

        Assert.Equal(new OriginalStrategicPlayerEnemyContact(1, 3), Assert.Single(result.Contacts));
        Assert.Equal(2, state.SelectedPlayerMovementSlot);
        Assert.DoesNotContain(result.Contacts, contact => contact.EnemySlot == 0);
    }

    [Fact]
    public void PlayerAvatarCollisionAlertUsesPostDecrementOneHundredTwentyPassCooldown()
    {
        var state = RuntimeState();
        var avatar = state.PlayerMovementSlots[OriginalStrategicMovement.PlayerAvatarMovementSlot];
        ActivateStationaryPlayer(avatar, 100, 100);
        avatar.CollisionCooldown = 1;
        ActivateRuntimeSlot(state.MovementSlots[0], OriginalStrategicMovement.DirectPropertyMode);
        state.MovementSlots[0].CurrentX = 100;
        state.MovementSlots[0].CurrentY = 100;

        var first = OriginalStrategicMovement.AdvancePlayerPass(
            state, new StubStrategicResources(), []);
        Assert.Equal(new OriginalStrategicPlayerAvatarAlert(5, 0),
            Assert.Single(first.AvatarAlerts));
        Assert.Equal(OriginalStrategicMovement.PlayerAvatarCollisionCooldown,
            avatar.CollisionCooldown);

        var second = OriginalStrategicMovement.AdvancePlayerPass(
            state, new StubStrategicResources(), []);
        Assert.Empty(second.AvatarAlerts);
        Assert.Equal(OriginalStrategicMovement.PlayerAvatarCollisionCooldown - 1,
            avatar.CollisionCooldown);
    }

    [Theory]
    [InlineData(63, 116)]
    [InlineData(62, 114)]
    [InlineData(64, 115)]
    [InlineData(63, 114)]
    [InlineData(63, 113)]
    public void EngagedPlayerArmyUsesTheExactFiveCellSpecialTrigger(int gridX, int gridY)
    {
        var state = RuntimeState();
        state.EngagedPlayerMovementSlot = 3;
        var player = state.PlayerMovementSlots[3];
        ActivateStationaryPlayer(player, 100, 100);
        player.GridX = gridX;
        player.GridY = gridY;

        var result = OriginalStrategicMovement.AdvancePlayerPass(
            state, new StubStrategicResources(), []);

        Assert.True(result.SpecialMapTrigger);
    }

    private static void ActivatePlayerRoute(
        OriginalStrategicPlayerMovementSlot slot,
        int terrainKind,
        params OriginalStrategicRoutePoint[] waypoints)
    {
        slot.Active = true;
        slot.TerrainKind = terrainKind;
        slot.WaypointCount = waypoints.Length;
        slot.Waypoints.AddRange(waypoints);
        slot.DirectionX = 1;
    }

    private static void ActivateStationaryPlayer(
        OriginalStrategicPlayerMovementSlot slot,
        float currentX,
        float currentY)
    {
        slot.Active = true;
        slot.PathComplete = true;
        slot.CurrentX = currentX;
        slot.CurrentY = currentY;
    }
}
