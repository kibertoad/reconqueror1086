using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-STRATEGY-006, RULE-STRATEGY-007.
public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OriginalStrategicMovementPassUsesSlotOrderAndDirectTerrainScale()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 1);
        ActivateRuntimeSlot(state.MovementSlots[4], OriginalStrategicMovement.DirectPropertyMode);
        ActivateRuntimeSlot(state.MovementSlots[1], OriginalStrategicMovement.DirectPropertyMode);
        foreach (var slot in new[] { state.MovementSlots[1], state.MovementSlots[4] })
        {
            slot.DestinationX = 100;
            slot.DirectionX = 1;
        }
        var resources = new StubStrategicResources();

        var advances = OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets());

        Assert.Equal([1, 4], advances.Select(advance => advance.Slot));
        Assert.All(advances, advance => Assert.False(advance.CompletionSignal));
        Assert.Equal(0.9f, state.MovementSlots[1].CurrentX, precision: 5);
        Assert.Equal(0.9f, state.MovementSlots[4].CurrentX, precision: 5);
        Assert.Equal((7, 11), (state.MovementSlots[1].GridX, state.MovementSlots[1].GridY));
    }

    [Fact]
    public void DirectMovementRequiresCrossingAndWrapsReturnedTroopsIntoOriginByte()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 1);
        var slot = state.MovementSlots[0];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.DirectPropertyMode);
        slot.CurrentX = 9.5f;
        slot.DestinationX = 10;
        slot.DirectionX = 1;
        var resources = new StubStrategicResources();

        var first = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));
        Assert.False(first.CompletionSignal);
        Assert.Equal(10.4f, slot.CurrentX, precision: 4);

        var second = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));
        Assert.True(second.CompletionSignal);
        Assert.True(slot.Active);
        Assert.Equal(10, slot.DestinationX);

        state.Properties[0].Garrison = 250;
        slot.Swordsmen = 3;
        slot.Halberdiers = 3;
        slot.Knights = 3;
        slot.CurrentX = 20;
        slot.DestinationX = 100;
        resources.Terrain = (_, _) => new OriginalStrategicTerrainCell(3, 5, 18);
        state.TerrainMutations.Add(new(3, 5, 0));

        var blocked = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));
        Assert.True(blocked.CompletionSignal);
        Assert.False(blocked.ActiveAfter);
        Assert.Equal(3, state.Properties[0].Garrison);
    }

    [Fact]
    public void RoutedMovementConsumesStrictWaypointToleranceAndSignalsAtTheLastPoint()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 1);
        var slot = state.MovementSlots[2];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.RoutedMode);
        slot.RouteResource = "sc_0.rat";
        slot.WaypointCount = 2;
        var resources = new StubStrategicResources
        {
            Routes =
            {
                ["sc_0.rat"] = [
                    new OriginalStrategicRoutePoint(0, 0),
                    new OriginalStrategicRoutePoint(10, 0)]
            }
        };

        var first = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));
        Assert.False(first.CompletionSignal);
        Assert.Equal(1, slot.WaypointIndex);
        Assert.Equal((10, 0), (slot.DestinationX, slot.DestinationY));
        Assert.Equal(1f, slot.DirectionX);
        Assert.Equal(1f, slot.CurrentX);

        slot.CurrentX = 4;
        var exactBoundary = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));
        Assert.False(exactBoundary.CompletionSignal);
        Assert.Equal(5f, slot.CurrentX);

        var completed = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));
        Assert.True(completed.CompletionSignal);
        Assert.True(completed.ActiveAfter);
        Assert.True(slot.PathComplete);
        Assert.Equal(2, slot.WaypointIndex);
    }

    [Fact]
    public void RoutedMovementRejectsAnOutOfRangePriorDirectionBeforeSamplingTerrain()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 1);
        var slot = state.MovementSlots[0];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.RoutedMode);
        slot.RouteResource = "sc_0.rat";
        slot.WaypointCount = 1;
        slot.DirectionX = 51;
        var resources = new StubStrategicResources
        {
            Routes = { ["sc_0.rat"] = [new OriginalStrategicRoutePoint(100, 0)] }
        };

        var advance = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, InactivePursuitTargets()));

        Assert.True(advance.CompletionSignal);
        Assert.False(advance.ActiveAfter);
        Assert.True(slot.PathComplete);
        Assert.Equal(0, resources.TerrainLookups);
    }

    [Fact]
    public void PursuitTracksTheLiveTargetAndImpassableTerrainSwitchesToDirectOriginReturn()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 2);
        var slot = state.MovementSlots[3];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.PursuitMode);
        slot.TargetMovementSlot = 1;
        var targets = InactivePursuitTargets();
        targets[1] = new OriginalStrategicPursuitTarget(true, 10, 0);
        var resources = new StubStrategicResources();

        var pursuit = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, targets));
        Assert.False(pursuit.CompletionSignal);
        Assert.Equal((10, 0, 1f, 0f, 2f),
            (slot.DestinationX, slot.DestinationY, slot.DirectionX, slot.DirectionY, slot.CurrentX));

        state.Properties[0].MapX8 = 20;
        state.Properties[0].MapY8 = 0;
        slot.CurrentX = 0;
        slot.CurrentY = 0;
        slot.DirectionX = 0;
        slot.DirectionY = 0;
        resources.Terrain = (_, _) => new OriginalStrategicTerrainCell(1, 2, 0);

        var fallback = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, resources, targets));
        Assert.False(fallback.CompletionSignal);
        Assert.Equal((OriginalStrategicMovement.PursuitMode, OriginalStrategicMovement.DirectPropertyMode),
            (fallback.ModeBefore, fallback.ModeAfter));
        Assert.Equal((20, 0, 1f, 0f, 1f),
            (slot.DestinationX, slot.DestinationY, slot.DirectionX, slot.DirectionY, slot.CurrentX));
    }

    [Fact]
    public void PursuitSignalsCompletionWithoutDestroyingTheEnemyWhenItsTargetDisappears()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 1);
        var slot = state.MovementSlots[0];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.PursuitMode);
        slot.TargetMovementSlot = 4;

        var advance = Assert.Single(OriginalStrategicMovement.AdvanceMovementPass(
            state, new StubStrategicResources(), InactivePursuitTargets()));

        Assert.True(advance.CompletionSignal);
        Assert.True(advance.ActiveAfter);
        Assert.Equal(OriginalStrategicMovement.PursuitMode, slot.Mode);
    }

    [Fact]
    public void TemporaryAutomaticPatrolUsesTheSourceCursorLoopAndKeepsLifecycleOwnership()
    {
        var state = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), speedMultiplier: 1);
        var force = state.TemporaryForceSlots[1];
        force.Active = true;
        force.Swordsmen = 1;
        force.WaypointCount = 44;
        force.OriginProperty = 7;
        force.Lord = state.Properties[7].Lord;
        force.Mode = OriginalStrategicMovement.RoutedMode;
        var resources = new StubStrategicResources
        {
            Routes =
            {
                ["scot.rat"] = Enumerable.Range(0, 44)
                    .Select(index => new OriginalStrategicRoutePoint(index * 10, 0))
                    .ToArray()
            }
        };

        var first = Assert.Single(OriginalStrategicMovement.AdvanceTemporaryForcePass(
            state, resources, new DateTime(1086, 6, 1)));

        Assert.Equal((1, false, false, true),
            (first.Slot, first.Looped, first.CompletionSignal, first.ActiveAfter));
        Assert.Equal((1, 10, 0, 1f, 0f, 1f, 0f, 7, 11),
            (force.WaypointIndex, force.DestinationX, force.DestinationY,
             force.DirectionX, force.DirectionY, force.CurrentX, force.CurrentY,
             force.GridX, force.GridY));

        force.WaypointIndex = 43;
        force.CurrentX = 430;
        var loop = Assert.Single(OriginalStrategicMovement.AdvanceTemporaryForcePass(
            state, resources, new DateTime(1086, 6, 1)));

        Assert.True(loop.Looped);
        Assert.False(loop.CompletionSignal);
        Assert.Equal((0, 0, 0, -1f, 429f),
            (force.WaypointIndex, force.DestinationX, force.DestinationY,
             force.DirectionX, force.CurrentX));

        force.PathComplete = false;
        force.WaypointCount = 44;
        force.WaypointIndex = 1;
        force.CurrentX = 0;
        force.DirectionX = 51;
        var completed = Assert.Single(OriginalStrategicMovement.AdvanceTemporaryForcePass(
            state, resources, new DateTime(1086, 6, 1)));

        Assert.True(completed.CompletionSignal);
        Assert.True(force.Active);
        Assert.True(force.PathComplete);
        Assert.Equal((0, 0), (force.WaypointCount, force.WaypointIndex));
    }

    private static void ActivateRuntimeSlot(OriginalStrategicMovementSlot slot, int mode)
    {
        slot.Active = true;
        slot.Mode = mode;
        slot.OriginProperty = 0;
        slot.Lord = 13;
        slot.Swordsmen = 1;
        slot.Halberdiers = 1;
        slot.Knights = 1;
    }

    private static OriginalStrategicPursuitTarget[] InactivePursuitTargets() =>
        Enumerable.Repeat(new OriginalStrategicPursuitTarget(false, 0, 0),
            OriginalStrategicMovement.SlotCount).ToArray();

    private sealed class StubStrategicResources : IOriginalStrategicResources
    {
        public Dictionary<string, IReadOnlyList<OriginalStrategicRoutePoint>> Routes { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public Func<int, int, OriginalStrategicTerrainCell> Terrain { get; set; } =
            (_, _) => new OriginalStrategicTerrainCell(7, 11, 18);

        public int TerrainLookups { get; private set; }

        public Dictionary<(int Row, int Column), OriginalStrategicTerrainCell> GridCells { get; } = [];

        public IReadOnlyList<OriginalStrategicRoutePoint> Route(string resourceName, bool reverse)
        {
            var route = Routes[resourceName];
            return reverse ? route.Reverse().ToArray() : route;
        }

        public bool TryGridCell(int row, int column, out OriginalStrategicTerrainCell cell) =>
            GridCells.TryGetValue((row, column), out cell);

        public bool TryTerrainCell(
            int worldX,
            int worldY,
            int cameraRow,
            int cameraColumn,
            out OriginalStrategicTerrainCell cell)
        {
            TerrainLookups++;
            cell = Terrain(worldX, worldY);
            return true;
        }
    }
}
