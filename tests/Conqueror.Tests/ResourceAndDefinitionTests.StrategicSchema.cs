using Conqueror.Core;
using System.Text.Json;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void SchemaTwoStrategicStateDeepCopiesEveryOriginalDefinition()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 1, 1), startingRouteSelector: 4, speedMultiplier: 15);

        Assert.Equal((4, 15, StrategicTerrainProfile.Winter, 0, 0, 0xFF, 0xFF, 5, 5),
            (state.StartingRouteSelector, state.SpeedMultiplier, state.TerrainProfile,
             state.CameraRow, state.CameraColumn, state.PropertyListHead, state.PersonListHead,
             state.SelectedPlayerMovementSlot, state.EngagedPlayerMovementSlot));
        Assert.Equal(OriginalStrategicMovement.PropertyCount, state.Properties.Count);
        Assert.Equal(OriginalStrategicMovement.PersonCount, state.Persons.Count);
        Assert.Equal([0, 1, 2, 3, 4], state.MovementSlots.Select(slot => slot.Slot));
        Assert.Equal([0, 1, 2], state.TemporaryForceSlots.Select(slot => slot.Slot));
        Assert.Equal([0, 1, 2, 3, 4, 5], state.PlayerMovementSlots.Select(slot => slot.Slot));
        Assert.All(state.MovementSlots, slot => Assert.False(slot.Active));
        Assert.All(state.TemporaryForceSlots, slot => Assert.False(slot.Active));
        Assert.All(state.PlayerMovementSlots.Take(5), slot => Assert.False(slot.Active));
        var starting = OriginalStrategicMovement.StartingRoutes[4];
        Assert.Equal((starting.GridX, starting.GridY, 0),
            (state.PlayerHomeGridX, state.PlayerHomeGridY, state.ActivePlayerRecordCount));
        var avatar = state.PlayerMovementSlots[5];
        Assert.Equal((true, true, 5, starting.GridX, starting.GridY,
                80 * (starting.GridX + 1), 20 * (starting.GridY + 1)),
            (avatar.Active, avatar.PathComplete, avatar.Slot, avatar.GridX, avatar.GridY,
             (int)avatar.CurrentX, (int)avatar.CurrentY));
        Assert.Empty(state.TerrainMutations);

        for (var index = 0; index < state.Properties.Count; index++)
        {
            var definition = OriginalStrategicMovement.Properties[index];
            var property = state.Properties[index];
            Assert.Equal((definition.OwnerOrState, definition.GridX, definition.GridY,
                definition.MapX8, definition.MapY8, definition.Lord, definition.ListNext,
                definition.Garrison, definition.State13, definition.State14),
                (property.OwnerOrState, property.GridX, property.GridY,
                 property.MapX8, property.MapY8, property.Lord, property.ListNext,
                 property.Garrison, property.State13, property.State14));
        }
        for (var index = 0; index < state.Persons.Count; index++)
        {
            var definition = OriginalStrategicMovement.Persons[index];
            var person = state.Persons[index];
            var expectedAssignment = index == starting.Person ? (byte)0 : definition.Assignment;
            Assert.Equal((definition.NameAddress, definition.Group, definition.State5,
                definition.Flags, expectedAssignment, definition.X, definition.Y,
                definition.LordRating, definition.ListNext, definition.State14,
                definition.State15, definition.State16, definition.State17),
                (person.NameAddress, person.Group, person.State5, person.Flags,
                 person.Assignment, person.X, person.Y, person.LordRating, person.ListNext,
                 person.State14, person.State15, person.State16, person.State17));
        }

        state.Properties[0].Garrison++;
        state.Persons[1].Assignment = 0;
        Assert.NotEqual(state.Properties[0].Garrison, OriginalStrategicMovement.Properties[0].Garrison);
        Assert.NotEqual(state.Persons[1].Assignment, OriginalStrategicMovement.Persons[1].Assignment);
        state.Validate();
    }

    [Fact]
    public void SchemaTwoStrategicStateRoundTripsEveryReplacementStateFamily()
    {
        var campaign = Campaign.NewFromTemplate(0);
        campaign.SchemaVersion = 1;
        campaign.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(
            new DateTime(1086, 9, 1), startingRouteSelector: 6, speedMultiplier: 7);
        var strategic = campaign.OriginalStrategicState;
        strategic.CameraRow = 199;
        strategic.CameraColumn = 399;
        strategic.GenerationAccumulator = 4_999;
        strategic.CallsSinceReactiveSuccess = 51;
        strategic.Properties[3].Garrison = 91;
        strategic.Persons[17].Assignment = 0;
        strategic.TerrainMutations.Add(new(199, 399, 0xA5C3_0123));
        var movement = strategic.MovementSlots[2];
        movement.Active = true;
        movement.PathComplete = false;
        movement.TargetMovementSlot = 3;
        movement.WaypointCount = 23;
        movement.WaypointIndex = 7;
        movement.Swordsmen = 4;
        movement.Halberdiers = 5;
        movement.Knights = 6;
        movement.OriginProperty = 0;
        movement.Lord = 13;
        movement.Mode = OriginalStrategicMovement.RoutedMode;
        movement.DestinationX = 7_520;
        movement.DestinationY = 2_840;
        movement.GridX = 94;
        movement.GridY = 40;
        movement.CurrentX = 7_438.5f;
        movement.CurrentY = 2_801.25f;
        movement.DirectionX = 0.75f;
        movement.DirectionY = -0.25f;
        movement.RouteResource = "sc_0.rat";
        movement.RouteReversed = false;
        var temporary = strategic.TemporaryForceSlots[1];
        temporary.Active = true;
        temporary.PathComplete = false;
        temporary.WaypointCount = 44;
        temporary.WaypointIndex = 7;
        temporary.Swordsmen = 8;
        temporary.Halberdiers = 9;
        temporary.Knights = 10;
        temporary.OriginProperty = 7;
        temporary.Lord = strategic.Properties[7].Lord;
        temporary.Mode = OriginalStrategicMovement.RoutedMode;
        temporary.DestinationX = 4_480;
        temporary.DestinationY = 900;
        temporary.GridX = 55;
        temporary.GridY = 44;
        temporary.CurrentX = 4_444.5f;
        temporary.CurrentY = 888.25f;
        temporary.DirectionX = 0.7f;
        temporary.DirectionY = -0.7f;
        strategic.SelectedPlayerMovementSlot = 4;
        strategic.EngagedPlayerMovementSlot = 3;
        strategic.PlayerRouteInputActive = true;
        strategic.PlayerHomeGridX = 17;
        strategic.PlayerHomeGridY = 23;
        strategic.ActivePlayerRecordCount = 4;
        var playerMovement = strategic.PlayerMovementSlots[4];
        playerMovement.Active = true;
        playerMovement.State8 = 13;
        playerMovement.WaypointCount = 2;
        playerMovement.WaypointIndex = 1;
        playerMovement.DestinationX = 4_000;
        playerMovement.DestinationY = 800;
        playerMovement.GridX = 49;
        playerMovement.GridY = 39;
        playerMovement.CollisionCooldown = 19;
        playerMovement.TerrainKind = 3;
        playerMovement.CurrentX = 3_999.5f;
        playerMovement.CurrentY = 799.25f;
        playerMovement.DirectionX = 0.8f;
        playerMovement.DirectionY = -0.6f;
        playerMovement.Waypoints.AddRange([
            new OriginalStrategicRoutePoint(3_900, 900),
            new OriginalStrategicRoutePoint(4_000, 800)]);
        campaign.Player.ArmyAt(4).OriginalStrategicEncounterCount = 23;
        campaign.Player.ArmyAt(4).OriginalStrategicEncounterStates[UnitType.Swordsmen] = 7;
        campaign.Player.ArmyAt(4).OriginalStrategicEncounterStates[UnitType.Halberdiers] = 8;
        campaign.Player.ArmyAt(4).OriginalStrategicEncounterStates[UnitType.Knights] = 9;
        strategic.Validate();

        var restored = JsonSerializer.Deserialize<CampaignState>(JsonSerializer.Serialize(campaign));
        var roundTripped = Assert.IsType<OriginalStrategicCampaignState>(restored!.OriginalStrategicState);
        roundTripped.Validate();
        var restoredMovement = roundTripped.MovementSlots[2];
        var restoredTemporary = roundTripped.TemporaryForceSlots[1];
        var restoredPlayerMovement = roundTripped.PlayerMovementSlots[4];
        Assert.Equal((6, 7, 199, 399, 4_999, 51, StrategicTerrainProfile.Autumn),
            (roundTripped.StartingRouteSelector, roundTripped.SpeedMultiplier,
             roundTripped.CameraRow, roundTripped.CameraColumn,
             roundTripped.GenerationAccumulator, roundTripped.CallsSinceReactiveSuccess,
             roundTripped.TerrainProfile));
        Assert.Equal(new OriginalStrategicTerrainMutation(199, 399, 0xA5C3_0123),
            Assert.Single(roundTripped.TerrainMutations));
        Assert.Equal((true, 3, 23, 7, 4, 5, 6, 0, 13, 2, "sc_0.rat"),
            (restoredMovement.Active, restoredMovement.TargetMovementSlot,
             restoredMovement.WaypointCount, restoredMovement.WaypointIndex,
             restoredMovement.Swordsmen, restoredMovement.Halberdiers,
             restoredMovement.Knights, restoredMovement.OriginProperty,
             restoredMovement.Lord, restoredMovement.Mode, restoredMovement.RouteResource));
        Assert.Equal((7_438.5f, 2_801.25f, 0.75f, -0.25f),
            (restoredMovement.CurrentX, restoredMovement.CurrentY,
             restoredMovement.DirectionX, restoredMovement.DirectionY));
        Assert.Equal((true, false, 44, 7, 8, 9, 10, 7, 100, 2, 4_480, 900, 55, 44,
                4_444.5f, 888.25f, 0.7f, -0.7f),
            (restoredTemporary.Active, restoredTemporary.PathComplete,
             restoredTemporary.WaypointCount, restoredTemporary.WaypointIndex,
             restoredTemporary.Swordsmen, restoredTemporary.Halberdiers,
             restoredTemporary.Knights, restoredTemporary.OriginProperty,
             restoredTemporary.Lord, restoredTemporary.Mode, restoredTemporary.DestinationX,
             restoredTemporary.DestinationY, restoredTemporary.GridX,
             restoredTemporary.GridY, restoredTemporary.CurrentX,
             restoredTemporary.CurrentY, restoredTemporary.DirectionX,
             restoredTemporary.DirectionY));
        Assert.Equal((17, 23, 4, 4, 3, true, true, 13, 2, 1, 19, 3),
            (roundTripped.PlayerHomeGridX, roundTripped.PlayerHomeGridY,
             roundTripped.ActivePlayerRecordCount,
             roundTripped.SelectedPlayerMovementSlot, roundTripped.EngagedPlayerMovementSlot,
             roundTripped.PlayerRouteInputActive, restoredPlayerMovement.Active,
             restoredPlayerMovement.State8,
             restoredPlayerMovement.WaypointCount,
             restoredPlayerMovement.WaypointIndex, restoredPlayerMovement.CollisionCooldown,
             restoredPlayerMovement.TerrainKind));
        Assert.Equal(playerMovement.Waypoints, restoredPlayerMovement.Waypoints);
        Assert.Equal(23, restored.Player.ArmyAt(4).OriginalStrategicEncounterCount);
        Assert.Equal((7, 8, 9), (
            restored.Player.ArmyAt(4).OriginalStrategicEncounterStates[UnitType.Swordsmen],
            restored.Player.ArmyAt(4).OriginalStrategicEncounterStates[UnitType.Halberdiers],
            restored.Player.ArmyAt(4).OriginalStrategicEncounterStates[UnitType.Knights]));
        Assert.Equal((3_999.5f, 799.25f, 0.8f, -0.6f),
            (restoredPlayerMovement.CurrentX, restoredPlayerMovement.CurrentY,
             restoredPlayerMovement.DirectionX, restoredPlayerMovement.DirectionY));
    }

    [Fact]
    public void SchemaOneSettlementReturnsOnlyColumnsWithAStillHostileOrigin()
    {
        var state = Campaign.NewFromTemplate(0);
        state.SchemaVersion = 1;
        state.Date = new DateTime(1087, 9, 3);
        state.DaySpeed = 99;
        state.GarrisonStrength[2] = 12;
        state.ConqueredLocations.Add(6);
        state.EnemyMovements.Add(new StrategicEnemyMovement(
            3, 2, 4, state.Date, state.Date.AddDays(4), 2, 3, 4));
        state.EnemyMovements.Add(new StrategicEnemyMovement(
            1, 6, 8, state.Date, state.Date.AddDays(4), 1, 2, 3));

        var settlement = StrategicSchemaTwoMigration.Prepare(state);

        Assert.Equal(new StrategicSchemaOneSettlement(1, 1, 9, 6), settlement);
        Assert.Equal(21, state.GarrisonStrength[2]);
        Assert.Empty(state.EnemyMovements);
        Assert.Equal(1, state.SchemaVersion);
        var strategic = Assert.IsType<OriginalStrategicCampaignState>(state.OriginalStrategicState);
        Assert.Equal((-1, 15, StrategicTerrainProfile.Autumn, 5, 5),
            (strategic.StartingRouteSelector, strategic.SpeedMultiplier, strategic.TerrainProfile,
             strategic.SelectedPlayerMovementSlot, strategic.EngagedPlayerMovementSlot));
        Assert.All(strategic.PlayerMovementSlots, slot => Assert.False(slot.Active));
        Assert.Contains("slot 1 (6 troops)", state.Journal[^2], StringComparison.Ordinal);
        Assert.Contains("origin is no longer hostile", state.Journal[^2], StringComparison.Ordinal);
        Assert.Contains("slot 3 (9 troops)", state.Journal[^1], StringComparison.Ordinal);
        Assert.Contains("to Canterbury", state.Journal[^1], StringComparison.Ordinal);

        var restored = JsonSerializer.Deserialize<CampaignState>(JsonSerializer.Serialize(state));
        Assert.NotNull(restored);
        Assert.Empty(restored.EnemyMovements);
        Assert.IsType<OriginalStrategicCampaignState>(restored.OriginalStrategicState).Validate();
        Assert.Throws<InvalidOperationException>(() => StrategicSchemaTwoMigration.Prepare(state));
    }

    [Fact]
    public void SchemaTwoStrategicValidationRejectsLossyOrAmbiguousState()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicCampaignState.CreateForNewGame(DateTime.Today, -1));
        var state = OriginalStrategicCampaignState.CreateForNewGame(DateTime.Today, 0);
        state.TerrainMutations.Add(new(0, 0, 1));
        state.TerrainMutations.Add(new(0, 0, 2));
        Assert.Throws<InvalidDataException>(state.Validate);
        state.TerrainMutations.RemoveAt(1);
        state.MovementSlots[4].Slot = 3;
        Assert.Throws<InvalidDataException>(state.Validate);
        state.MovementSlots[4].Slot = 4;
        state.PlayerMovementSlots[5].Slot = 4;
        Assert.Throws<InvalidDataException>(state.Validate);
        state.PlayerMovementSlots[5].Slot = 5;
        var routed = state.MovementSlots[4];
        routed.Active = true;
        routed.Mode = OriginalStrategicMovement.RoutedMode;
        routed.OriginProperty = 0;
        routed.Lord = 13;
        routed.Knights = 1;
        routed.WaypointCount = 1;
        routed.RouteResource = "not-an-original-route.rat";
        Assert.Throws<InvalidDataException>(state.Validate);

        var legacy = Campaign.NewFromTemplate(0);
        legacy.SchemaVersion = 1;
        legacy.EnemyMovements.Add(new StrategicEnemyMovement(
            0, 1, 2, legacy.Date, legacy.Date.AddDays(1), 1, 1, 1));
        legacy.EnemyMovements.Add(new StrategicEnemyMovement(
            0, 2, 3, legacy.Date, legacy.Date.AddDays(1), 1, 1, 1));
        Assert.Throws<InvalidDataException>(() => StrategicSchemaTwoMigration.Prepare(legacy));
    }
}
