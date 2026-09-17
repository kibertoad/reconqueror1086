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
        state.SelectedPlayerMovementSlot = 0;
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
    public void PlayerDivisionTargetUsesTheTaggedLowByteIdentityAndCompletesAtExactContact()
    {
        var state = RuntimeState();
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.TargetHandle = OriginalStrategicMovement.PlayerDivisionTargetFlag | 2;
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
    public void PlayerMapCommandUsesPlayerEnemyDivisionThenRoutePrecedence()
    {
        var state = RuntimeState();
        state.PlayerMovementSlots[0].Active = true;
        state.PlayerMovementSlots[0].PathComplete = true;
        state.PlayerMovementSlots[3].Active = true;
        state.PlayerMovementSlots[3].PathComplete = true;
        var resources = new StubStrategicResources();

        var selected = OriginalStrategicMovement.DispatchPlayerMapCommand(
            state, resources, new(3, 2, 1), 80, 90, targetConfirmed: true);
        Assert.True(selected.Applied);
        Assert.Equal(3, state.SelectedPlayerMovementSlot);
        Assert.Equal(0, state.PlayerMovementSlots[3].TargetHandle);
        Assert.True(state.PlayerRouteInputActive);

        var enemyDeclined = OriginalStrategicMovement.DispatchPlayerMapCommand(
            state, resources, new(null, 2, 1), 80, 90, targetConfirmed: false);
        Assert.False(enemyDeclined.Applied);
        Assert.Equal(0, state.PlayerMovementSlots[3].TargetHandle);

        var enemy = OriginalStrategicMovement.DispatchPlayerMapCommand(
            state, resources, new(null, 2, 1), 80, 90, targetConfirmed: true);
        Assert.True(enemy.Applied);
        Assert.Equal(OriginalStrategicMovement.PlayerEnemyTargetFlag | 2,
            state.PlayerMovementSlots[3].TargetHandle);
        Assert.False(state.PlayerRouteInputActive);

        var division = OriginalStrategicMovement.DispatchPlayerMapCommand(
            state, resources, new(null, null, 1), 80, 90, targetConfirmed: true);
        Assert.True(division.Applied);
        Assert.Equal(OriginalStrategicMovement.PlayerDivisionTargetFlag | 1,
            state.PlayerMovementSlots[3].TargetHandle);
    }

    [Fact]
    public void PlayerJoinAndLeaveExchangeDistinguishedArmyAndAvatarState()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 3, 1), startingRouteSelector: 2);
        var army = state.PlayerMovementSlots[3];
        army.CurrentX = 4_321.5f;
        army.CurrentY = 987.25f;
        army.GridX = 53;
        army.GridY = 48;

        OriginalStrategicMovement.JoinPlayerArmy(state, 3);

        Assert.Equal((3, 3, true, false),
            (state.EngagedPlayerMovementSlot, state.SelectedPlayerMovementSlot,
             army.Active, state.PlayerMovementSlots[5].Active));

        Assert.True(OriginalStrategicMovement.LeavePlayerArmy(state, 3));
        var avatar = state.PlayerMovementSlots[5];
        Assert.Equal((5, 5, true, true, 0, 0, 0),
            (state.EngagedPlayerMovementSlot, state.SelectedPlayerMovementSlot,
             avatar.Active, avatar.PathComplete, avatar.TargetHandle,
             avatar.WaypointCount, avatar.WaypointIndex));
        Assert.Equal((4_321.5f, 987.25f, 53, 48),
            (avatar.CurrentX, avatar.CurrentY, avatar.GridX, avatar.GridY));
        Assert.True(army.Active);
        Assert.False(state.PlayerRouteInputActive);
        Assert.False(OriginalStrategicMovement.LeavePlayerArmy(state, 3));
    }

    [Fact]
    public void PlayerRecordConstructorUsesHomeFormationAndFormerSelectionClearing()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 3, 1), startingRouteSelector: 2);
        var formerlySelected = state.PlayerMovementSlots[5];
        formerlySelected.TargetHandle = OriginalStrategicMovement.PlayerEnemyTargetFlag | 3;
        formerlySelected.WaypointCount = 2;
        formerlySelected.WaypointIndex = 1;
        formerlySelected.Waypoints.AddRange([new(1, 2), new(3, 4)]);
        var constructed = state.PlayerMovementSlots[2];
        constructed.State8 = 7;
        constructed.CollisionCooldown = 9;
        constructed.TargetHandle = OriginalStrategicMovement.PlayerDivisionTargetFlag | 1;
        constructed.WaypointCount = 1;
        constructed.WaypointIndex = 1;
        constructed.Waypoints.Add(new(99, 101));

        Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 2));

        var offset = OriginalStrategicMovement.PlayerFormationOffsets[2];
        var expectedX = 80 * (state.PlayerHomeGridX + 1) + offset.X;
        var expectedY = 20 * (state.PlayerHomeGridY + 1) + offset.Y;
        Assert.Equal((1, 2),
            (state.ActivePlayerRecordCount, state.SelectedPlayerMovementSlot));
        Assert.Equal((true, true, 0, 0, state.PlayerHomeGridX, state.PlayerHomeGridY,
                expectedX, expectedY, (float)expectedX, (float)expectedY),
            (constructed.Active, constructed.PathComplete, constructed.State8,
                constructed.CollisionCooldown, constructed.GridX, constructed.GridY,
                constructed.DestinationX, constructed.DestinationY,
                constructed.CurrentX, constructed.CurrentY));
        Assert.Equal((OriginalStrategicMovement.PlayerDivisionTargetFlag | 1, 1, 1),
            (constructed.TargetHandle, constructed.WaypointCount, constructed.WaypointIndex));
        Assert.Equal((0, 0, 0),
            (formerlySelected.TargetHandle, formerlySelected.WaypointCount,
                formerlySelected.WaypointIndex));
    }

    [Fact]
    public void PlayerRecordConstructorAcceptsSixRecordsThenRejectsTheSeventh()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 3, 1), startingRouteSelector: 0);

        for (var slot = 0; slot < OriginalStrategicMovement.PlayerMovementRecordCount; slot++)
            Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, slot));

        Assert.Equal(OriginalStrategicMovement.MaximumActivePlayerRecordCount,
            state.ActivePlayerRecordCount);
        Assert.False(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 0));
    }

    [Fact]
    public void PlayerRecordRemovalSelectsFirstActiveRecordAndRestoresEngagedAvatar()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 3, 1), startingRouteSelector: 1);
        Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 0));
        Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 2));
        var removed = state.PlayerMovementSlots[2];
        removed.CurrentX = 4_321.5f;
        removed.CurrentY = 987.25f;
        removed.GridX = 53;
        removed.GridY = 48;
        removed.State8 = 12;
        removed.CollisionCooldown = 19;
        OriginalStrategicMovement.JoinPlayerArmy(state, 2);

        Assert.True(OriginalStrategicMovement.RemovePlayerMovementRecord(state, 2));

        var avatar = state.PlayerMovementSlots[5];
        Assert.Equal((1, 5, 5),
            (state.ActivePlayerRecordCount, state.SelectedPlayerMovementSlot,
                state.EngagedPlayerMovementSlot));
        Assert.Equal((false, true, 0, 0),
            (removed.Active, removed.PathComplete, removed.State8, removed.CollisionCooldown));
        Assert.Equal((true, true, 4_321.5f, 987.25f, 53, 48),
            (avatar.Active, avatar.PathComplete, avatar.CurrentX, avatar.CurrentY,
                avatar.GridX, avatar.GridY));
    }

    [Fact]
    public void PlayerRecordRemovalUsesFirstActivePhysicalReplacement()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 3, 1), startingRouteSelector: 1);
        Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 3));
        Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 1));
        Assert.True(OriginalStrategicMovement.ConstructPlayerMovementRecord(state, 3));

        Assert.True(OriginalStrategicMovement.RemovePlayerMovementRecord(state, 3));

        Assert.Equal(1, state.SelectedPlayerMovementSlot);
        Assert.Equal(2, state.ActivePlayerRecordCount);
    }

    [Fact]
    public void CampaignMembershipKeepsOriginalDistinguishedRecordInSync()
    {
        var campaignState = Campaign.NewFromTemplate(0);
        campaignState.SchemaVersion = 1;
        campaignState.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            campaignState.Date, startingRouteSelector: 0);
        campaignState.Player.JoinedArmyIndex = null;
        campaignState.Player.ArmyAt(2).Units[UnitType.Swordsmen] = 100;
        campaignState.Player.SetArmyFieldState(2, fielded: true, location: 0);
        var campaign = new Campaign(campaignState);

        Assert.True(campaign.ToggleArmyMembership(2));
        Assert.Equal(2, campaign.State.Player.JoinedArmyIndex);
        Assert.Equal(2, campaignState.OriginalStrategicState.EngagedPlayerMovementSlot);
        Assert.False(campaignState.OriginalStrategicState.PlayerMovementSlots[5].Active);

        Assert.True(campaign.ToggleArmyMembership(2));
        Assert.Null(campaign.State.Player.JoinedArmyIndex);
        Assert.Equal(5, campaignState.OriginalStrategicState.EngagedPlayerMovementSlot);
        Assert.True(campaignState.OriginalStrategicState.PlayerMovementSlots[5].Active);
    }

    [Fact]
    public void PlayerRouteCommandClearsTargetPrimesFirstPointAndRetainsStalePairs()
    {
        var state = RuntimeState();
        state.SelectedPlayerMovementSlot = 0;
        var slot = state.PlayerMovementSlots[0];
        slot.Active = true;
        slot.TargetHandle = OriginalStrategicMovement.PlayerEnemyTargetFlag | 2;
        slot.PathComplete = true;
        slot.WaypointCount = 1;
        slot.Waypoints.Add(new(999, 999));
        var resources = new StubStrategicResources();

        var first = OriginalStrategicMovement.AppendPlayerRoutePoint(
            state, resources, 100, 0);

        Assert.True(first.Applied);
        Assert.True(first.RouteStarted);
        Assert.Equal((0, 1, 0, false),
            (slot.TargetHandle, slot.WaypointCount, slot.WaypointIndex, slot.PathComplete));
        Assert.Equal(new OriginalStrategicRoutePoint(100, 0), slot.Waypoints[0]);
        Assert.Equal((100, 0, 1f, 0f),
            (slot.DestinationX, slot.DestinationY, slot.DirectionX, slot.DirectionY));

        Assert.True(OriginalStrategicMovement.RemoveLastPlayerRoutePoint(state));
        Assert.True(slot.PathComplete);
        Assert.False(state.PlayerRouteInputActive);
        Assert.Equal(0, slot.WaypointCount);
        Assert.Single(slot.Waypoints);
        Assert.Equal(new OriginalStrategicRoutePoint(100, 0), slot.Waypoints[0]);
        state.Validate();
    }

    [Fact]
    public void PlayerRouteInputStopsAtTwentyDespiteTwentyOnePairRecordCapacity()
    {
        var state = RuntimeState();
        state.SelectedPlayerMovementSlot = 0;
        var slot = state.PlayerMovementSlots[0];
        slot.Active = true;
        slot.PathComplete = true;
        var resources = new StubStrategicResources();

        for (var index = 0; index < OriginalStrategicMovement.PlayerRouteInputLimit; index++)
            Assert.True(OriginalStrategicMovement.AppendPlayerRoutePoint(
                state, resources, 100 + index, index).Applied);

        var capped = OriginalStrategicMovement.AppendPlayerRoutePoint(
            state, resources, 999, 999);

        Assert.False(capped.Applied);
        Assert.True(capped.RouteLimitReached);
        Assert.Equal(20, slot.WaypointCount);
        Assert.Equal(20, slot.Waypoints.Count);
        Assert.Equal(21, OriginalStrategicMovement.PlayerWaypointCapacity);
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
