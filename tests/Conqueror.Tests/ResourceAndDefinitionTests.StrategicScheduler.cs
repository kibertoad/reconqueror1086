using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-STRATEGY-002, RULE-STRATEGY-003, RULE-STRATEGY-004, RULE-STRATEGY-005, RULE-STRATEGY-008.
public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void SchedulerGeneratesBeforeScanningSoANewRoutedSlotMovesImmediately()
    {
        var state = RuntimeState();
        state.GenerationAccumulator = OriginalStrategicMovement.GenerationIntervalUnits;
        state.StartingRouteSelector = 0;
        state.Properties[0].State13 = 1;
        var resources = new StubStrategicResources
        {
            Routes =
            {
                ["sc_0.rat"] = [
                    new OriginalStrategicRoutePoint(10_800, 1_200),
                    new OriginalStrategicRoutePoint(12_000, 2_000)]
            }
        };

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(), new QueueStrategicRandom(97));

        var construction = Assert.Single(result.Constructions);
        Assert.Equal((0, OriginalStrategicMovement.RoutedMode, "sc_0.rat"),
            (construction.Slot, construction.Mode, construction.RouteResource));
        var advance = Assert.Single(result.Advances);
        Assert.Equal((0, OriginalStrategicMovement.RoutedMode), (advance.Slot, advance.ModeBefore));
        Assert.Equal(1, state.MovementSlots[0].WaypointIndex);
        Assert.True(state.MovementSlots[0].CurrentX > 10_800);
        Assert.Equal(88, state.MovementSlots[0].MarkerFrame);
    }

    [Fact]
    public void ReactivePursuitUsesTargetTroopsHouseholdsAndOriginByteGarrison()
    {
        var state = RuntimeState();
        state.Properties[0].Garrison = 10;
        var targets = SchedulerTargets();
        targets[1] = new OriginalStrategicPursuitTarget(
            true, 20_000, 2_000, 12, 30, Swordsmen: 2, Halberdiers: 3, Knights: 4);
        var input = SchedulerInput(targets) with
        {
            ReactiveDetection = new OriginalStrategicReactiveDetection(0, 1)
        };

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, new StubStrategicResources(), input, new QueueStrategicRandom(2));

        var construction = Assert.Single(result.Constructions);
        Assert.Equal((0, OriginalStrategicMovement.PursuitMode, 1),
            (construction.Slot, construction.Mode, construction.TargetMovementSlot));
        Assert.Equal(0, state.Properties[0].Garrison);
        Assert.Equal((4, 4, 4),
            (state.MovementSlots[0].Swordsmen,
                state.MovementSlots[0].Halberdiers,
                state.MovementSlots[0].Knights));
        Assert.Equal(88, state.MovementSlots[0].MarkerFrame);
        Assert.Equal(0, state.CallsSinceReactiveSuccess);
        Assert.Equal(1, state.GenerationAccumulator);
    }

    [Fact] // Covers RULE-STRATEGY-005, DEV-STRATEGY-004.
    public void SmallPursuitWritesThreeHalberdiersAndClearsTheSlotsEarlierCounts()
    {
        var state = RuntimeState();
        state.Properties[0].Garrison = 1;
        var group = state.Persons[state.Properties[0].Lord].Group;
        foreach (var person in state.Persons.Skip(1).Where(person => person.Group == group).Skip(2))
            person.Assignment = 0;
        state.MovementSlots[0].Swordsmen = 7;
        state.MovementSlots[0].Knights = 5;
        var targets = SchedulerTargets();
        targets[1] = new OriginalStrategicPursuitTarget(
            true, 20_000, 2_000, 12, 30, Swordsmen: 2, Halberdiers: 3, Knights: 4);
        var input = SchedulerInput(targets) with
        {
            ReactiveDetection = new OriginalStrategicReactiveDetection(0, 1)
        };

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, new StubStrategicResources(), input, new QueueStrategicRandom(2));

        var construction = Assert.Single(result.Constructions);
        Assert.Equal((0, OriginalStrategicMovement.PursuitMode), (construction.Slot, construction.Mode));
        Assert.Equal(0, state.Properties[0].Garrison);
        Assert.Equal((0, 3, 0),
            (state.MovementSlots[0].Swordsmen,
                state.MovementSlots[0].Halberdiers,
                state.MovementSlots[0].Knights));
    }

    [Fact]
    public void ReactiveFinderUsesStrictNearAndApproachGatesAndAlertsOnlyOnce()
    {
        var state = RuntimeState();
        for (var index = 1; index < state.Properties.Count; index++)
            state.Properties[index].OwnerOrState = 0;
        var property = state.Properties[0];
        var targets = SchedulerTargets();
        targets[0] = new OriginalStrategicPursuitTarget(
            true, property.MapX8 + OriginalStrategicMovement.ReactiveNearDistance,
            property.MapY8, 10, 20, Swordsmen: 1);
        var resources = new StubStrategicResources();

        var approach = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(targets), new QueueStrategicRandom());

        Assert.Empty(approach.Constructions);
        Assert.Equal(new OriginalStrategicPropertyAlert(0, 0), Assert.Single(approach.PropertyAlerts));
        Assert.Equal((0, 1), (property.State13, property.State14));

        var repeated = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(targets), new QueueStrategicRandom());
        Assert.Empty(repeated.PropertyAlerts);

        var approachEdgeState = RuntimeState();
        for (var index = 1; index < approachEdgeState.Properties.Count; index++)
            approachEdgeState.Properties[index].OwnerOrState = 0;
        var approachEdgeProperty = approachEdgeState.Properties[0];
        var approachEdgeTargets = SchedulerTargets();
        approachEdgeTargets[0] = targets[0] with
        {
            CurrentX = approachEdgeProperty.MapX8
                + OriginalStrategicMovement.ReactiveApproachDistance
        };
        var approachEdge = OriginalStrategicMovement.AdvanceSchedulerPass(
            approachEdgeState, resources, SchedulerInput(approachEdgeTargets),
            new QueueStrategicRandom());
        Assert.Empty(approachEdge.PropertyAlerts);

        targets[0] = targets[0] with { CurrentX = property.MapX8 + 29 };
        var near = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(targets), new QueueStrategicRandom(2));

        Assert.Equal(0, Assert.Single(near.Constructions).OriginProperty);
        Assert.Equal(1, property.State13);
    }

    [Fact]
    public void ReactiveFinderUsesAuthoredPropertyThenPlayerSlotOrder()
    {
        var state = RuntimeState();
        for (var index = 2; index < state.Properties.Count; index++)
            state.Properties[index].OwnerOrState = 0;
        state.Properties[1].MapX8 = state.Properties[0].MapX8;
        state.Properties[1].MapY8 = state.Properties[0].MapY8;
        var targets = SchedulerTargets();
        targets[1] = new OriginalStrategicPursuitTarget(
            true, state.Properties[0].MapX8, state.Properties[0].MapY8, 1, 1, Swordsmen: 1);
        targets[0] = targets[1] with { CurrentX = state.Properties[0].MapX8 + 10 };

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, new StubStrategicResources(), SchedulerInput(targets), new QueueStrategicRandom(2));

        var construction = Assert.Single(result.Constructions);
        Assert.Equal((0, 0), (construction.OriginProperty, construction.TargetMovementSlot));
        Assert.Equal(0, state.Properties[1].State13);
    }

    [Fact]
    public void ReactiveFinderAcceptsLordCellIdentityAndMarksEmptyPropertiesWithoutConstruction()
    {
        var state = RuntimeState();
        for (var index = 1; index < state.Properties.Count; index++)
            state.Properties[index].OwnerOrState = 0;
        var property = state.Properties[0];
        property.Garrison = 0;
        var targets = SchedulerTargets();
        targets[0] = new OriginalStrategicPursuitTarget(
            true, 60_000, 60_000, 10, 20, Swordsmen: 1);
        var resources = new StubStrategicResources();
        resources.GridCells[(10, 20)] = new OriginalStrategicTerrainCell(
            10, 20, ((uint)property.Lord << 16) | 18u);

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(targets), new QueueStrategicRandom());

        Assert.Empty(result.Constructions);
        Assert.Equal(1, property.State13);
    }

    [Fact]
    public void ReactiveFinderUsesTheStrictLondonRectangle()
    {
        var insideState = RuntimeState();
        var insideTargets = SchedulerTargets();
        insideTargets[0] = new OriginalStrategicPursuitTarget(
            true,
            OriginalStrategicMovement.ReactiveSpecialBoundsX,
            OriginalStrategicMovement.ReactiveSpecialBoundsY,
            1, 1, Swordsmen: 1);

        var inside = OriginalStrategicMovement.AdvanceSchedulerPass(
            insideState, new StubStrategicResources(), SchedulerInput(insideTargets),
            new QueueStrategicRandom(2));
        Assert.Equal(OriginalStrategicMovement.ReactiveSpecialProperty,
            Assert.Single(inside.Constructions).OriginProperty);

        var outsideState = RuntimeState();
        var outsideTargets = SchedulerTargets();
        outsideTargets[0] = insideTargets[0] with
        {
            CurrentX = OriginalStrategicMovement.ReactiveSpecialBoundsX
                + OriginalStrategicMovement.ReactiveSpecialBoundsWidth
        };
        var outside = OriginalStrategicMovement.AdvanceSchedulerPass(
            outsideState, new StubStrategicResources(), SchedulerInput(outsideTargets),
            new QueueStrategicRandom());
        Assert.Empty(outside.Constructions);
        Assert.Equal(0, outsideState.Properties[OriginalStrategicMovement.ReactiveSpecialProperty].State13);
    }

    [Fact]
    public void CompletionProbeReinforcesMatchingOriginAndDeactivatesTheSlot()
    {
        var state = RuntimeState();
        var slot = state.MovementSlots[0];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.DirectPropertyMode);
        slot.CurrentX = 10;
        slot.DestinationX = 10;
        slot.DirectionX = 1;
        slot.CurrentY = 10;
        state.Persons[1].Assignment = state.Properties[0].OwnerOrState;
        var resources = new StubStrategicResources();
        resources.GridCells[(7, 11)] = new OriginalStrategicTerrainCell(7, 11, (1u << 16) | 18u);
        var before = state.Properties[0].Garrison;

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(), new QueueStrategicRandom());

        var completion = Assert.Single(result.Completions);
        Assert.Equal(StrategicContactOutcome.ReinforceOriginAndDeactivate, completion.Outcome);
        Assert.False(slot.Active);
        Assert.Equal(unchecked((byte)(before + 3)), state.Properties[0].Garrison);
    }

    [Fact]
    public void EligibleUnassignedContactStopsThePhysicalSlotScanForEncounterHandoff()
    {
        var state = RuntimeState();
        var first = state.MovementSlots[0];
        ActivateRuntimeSlot(first, OriginalStrategicMovement.DirectPropertyMode);
        first.CurrentX = first.DestinationX = 10;
        first.CurrentY = 10;
        first.DirectionX = 1;
        var second = state.MovementSlots[1];
        ActivateRuntimeSlot(second, OriginalStrategicMovement.DirectPropertyMode);
        second.DestinationX = 100;
        second.DirectionX = 1;
        state.Persons[2].Assignment = 0;
        state.Persons[2].Flags |= OriginalStrategicMovement.HouseholdEligibleFlag;
        var resources = new StubStrategicResources();
        resources.GridCells[(7, 11)] = new OriginalStrategicTerrainCell(7, 11, (2u << 16) | 18u);

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, SchedulerInput(), new QueueStrategicRandom());

        Assert.Equal(new OriginalStrategicEncounter(0, 2), result.Encounter);
        Assert.Single(result.Advances);
        Assert.Equal(0f, second.CurrentX);
        Assert.True(first.Active);
    }

    [Fact]
    public void CompletionWithoutAContactRetargetsTheFirstNearbyLiveArmy()
    {
        var state = RuntimeState();
        var slot = state.MovementSlots[0];
        ActivateRuntimeSlot(slot, OriginalStrategicMovement.DirectPropertyMode);
        slot.CurrentX = slot.DestinationX = 100;
        slot.CurrentY = 100;
        slot.DirectionX = 1;
        var targets = SchedulerTargets();
        targets[3] = new OriginalStrategicPursuitTarget(true, 110, 120, 3, 4);

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, new StubStrategicResources(), SchedulerInput(targets), new QueueStrategicRandom());

        var completion = Assert.Single(result.Completions);
        Assert.Equal(StrategicContactOutcome.Retarget, completion.Outcome);
        Assert.Equal((OriginalStrategicMovement.DirectPropertyMode, 110, 120),
            (slot.Mode, slot.DestinationX, slot.DestinationY));
        Assert.True(slot.DirectionX > 0);
        Assert.True(slot.DirectionY > 0);
    }

    [Fact]
    public void TimedPropertyGenerationSelectsCanonicalRouteFromAuthoredCandidates()
    {
        var state = RuntimeState();
        state.GenerationAccumulator = OriginalStrategicMovement.GenerationIntervalUnits;
        state.PropertyListHead = 0;
        state.Properties[0].State13 = 1;
        state.Properties[1].State13 = 0;
        var targetPerson = state.Properties[1].Lord;
        Assert.True(OriginalStrategicMovement.TryGetPropertyRoute(0, 1, out var selected));
        var resources = new StubStrategicResources
        {
            Routes = { [selected.ResourceName] = [new OriginalStrategicRoutePoint(20_000, 2_000)] }
        };
        var input = SchedulerInput() with { GlobalTargetPerson = targetPerson };

        var result = OriginalStrategicMovement.AdvanceSchedulerPass(
            state, resources, input, new QueueStrategicRandom(97, 0));

        var construction = Assert.Single(result.Constructions);
        Assert.Equal((0, selected.ResourceName, selected.Reverse),
            (construction.OriginProperty, construction.RouteResource, construction.RouteReversed));
    }

    private static OriginalStrategicCampaignState RuntimeState() =>
        OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            new DateTime(1086, 6, 1), OriginalStrategicMovement.InitialSpeedMultiplier);

    private static OriginalStrategicSchedulerInput SchedulerInput(
        IReadOnlyList<OriginalStrategicPursuitTarget>? targets = null) =>
        new(
            GlobalTargetPerson: 17,
            GlobalOriginProperty: 0,
            PlayerMovementSlot: 0,
            PlayerPosition: new StrategicPoint(30_000, 3_000),
            PursuitTargets: targets ?? SchedulerTargets());

    private static OriginalStrategicPursuitTarget[] SchedulerTargets() =>
        Enumerable.Repeat(new OriginalStrategicPursuitTarget(false, 0, 0),
            OriginalStrategicMovement.SlotCount).ToArray();

    private sealed class QueueStrategicRandom(params int[] values) : IOriginalStrategicRandom
    {
        private readonly Queue<int> _values = new(values);

        public int Next(int exclusiveMaximum)
        {
            Assert.NotEmpty(_values);
            var value = _values.Dequeue();
            Assert.InRange(value, 0, exclusiveMaximum - 1);
            return value;
        }
    }
}
