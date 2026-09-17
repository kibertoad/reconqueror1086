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

        Assert.Equal((4, 15, StrategicTerrainProfile.Winter, 0, 0, 0xFF, 0xFF),
            (state.StartingRouteSelector, state.SpeedMultiplier, state.TerrainProfile,
             state.CameraRow, state.CameraColumn, state.PropertyListHead, state.PersonListHead));
        Assert.Equal(OriginalStrategicMovement.PropertyCount, state.Properties.Count);
        Assert.Equal(OriginalStrategicMovement.PersonCount, state.Persons.Count);
        Assert.Equal([0, 1, 2, 3, 4], state.MovementSlots.Select(slot => slot.Slot));
        Assert.All(state.MovementSlots, slot => Assert.False(slot.Active));
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
            Assert.Equal((definition.NameAddress, definition.Group, definition.State5,
                definition.Flags, definition.Assignment, definition.X, definition.Y,
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
        movement.TargetLocation = 17;
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
        strategic.Validate();

        var restored = JsonSerializer.Deserialize<CampaignState>(JsonSerializer.Serialize(campaign));
        var roundTripped = Assert.IsType<OriginalStrategicCampaignState>(restored!.OriginalStrategicState);
        roundTripped.Validate();
        var restoredMovement = roundTripped.MovementSlots[2];
        Assert.Equal((6, 7, 199, 399, 4_999, 51, StrategicTerrainProfile.Autumn),
            (roundTripped.StartingRouteSelector, roundTripped.SpeedMultiplier,
             roundTripped.CameraRow, roundTripped.CameraColumn,
             roundTripped.GenerationAccumulator, roundTripped.CallsSinceReactiveSuccess,
             roundTripped.TerrainProfile));
        Assert.Equal(new OriginalStrategicTerrainMutation(199, 399, 0xA5C3_0123),
            Assert.Single(roundTripped.TerrainMutations));
        Assert.Equal((true, 17, 23, 7, 4, 5, 6, 0, 13, 2, "sc_0.rat"),
            (restoredMovement.Active, restoredMovement.TargetLocation,
             restoredMovement.WaypointCount, restoredMovement.WaypointIndex,
             restoredMovement.Swordsmen, restoredMovement.Halberdiers,
             restoredMovement.Knights, restoredMovement.OriginProperty,
             restoredMovement.Lord, restoredMovement.Mode, restoredMovement.RouteResource));
        Assert.Equal((7_438.5f, 2_801.25f, 0.75f, -0.25f),
            (restoredMovement.CurrentX, restoredMovement.CurrentY,
             restoredMovement.DirectionX, restoredMovement.DirectionY));
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
        Assert.Equal((-1, 15, StrategicTerrainProfile.Autumn),
            (strategic.StartingRouteSelector, strategic.SpeedMultiplier, strategic.TerrainProfile));
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
