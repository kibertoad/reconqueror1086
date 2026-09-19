namespace Conqueror.Core;

/// <summary>
/// Generator-first, physical-slot-order shell around the original strategic
/// movement handlers. Encounter presentation remains an explicit handoff: the
/// executable leaves the scheduler when that handoff raises its modal state.
/// </summary>
public static partial class OriginalStrategicMovement
{
    public static OriginalStrategicSchedulerResult AdvanceSchedulerPass(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        OriginalStrategicSchedulerInput input,
        IOriginalStrategicRandom random)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(random);
        ValidateSchedulerInput(state, input);

        var constructions = new List<OriginalStrategicConstruction>();
        var propertyAlerts = new List<OriginalStrategicPropertyAlert>();
        AdvanceGeneration(state, resources, input, random, constructions, propertyAlerts);

        var slots = state.MovementSlots.ToDictionary(slot => slot.Slot);
        var advances = new List<OriginalStrategicSlotAdvance>();
        var completions = new List<OriginalStrategicCompletion>();
        OriginalStrategicEncounter? encounter = null;
        for (var index = 0; index < SlotCount; index++)
        {
            var slot = slots[index];
            if (!slot.Active) continue;
            var advance = AdvanceMovementSlot(slot, state, resources, input.PursuitTargets);
            advances.Add(advance);
            if (!advance.CompletionSignal || !slot.Active) continue;

            RefreshGrid(slot, state, resources);
            var person = FindContactPerson(state, resources, slot.GridX, slot.GridY);
            var assignment = person is null ? (int?)null : state.Persons[person.Value].Assignment;
            var eligible = person is not null
                && (state.Persons[person.Value].Flags & HouseholdEligibleFlag) != 0;
            var outcome = ResolveCompletedContact(
                state.Properties[slot.OriginProperty].OwnerOrState, assignment, eligible);
            completions.Add(new OriginalStrategicCompletion(slot.Slot, person, outcome));

            if (outcome == StrategicContactOutcome.Encounter)
            {
                encounter = new OriginalStrategicEncounter(slot.Slot, person!.Value);
                break;
            }
            if (outcome == StrategicContactOutcome.ReinforceOriginAndDeactivate)
            {
                var origin = state.Properties[slot.OriginProperty];
                origin.Garrison = unchecked((byte)(origin.Garrison + slot.Total));
                DeactivateSlot(slot);
                continue;
            }
            Retarget(slot, state, input);
        }

        return new OriginalStrategicSchedulerResult(
            constructions.AsReadOnly(), advances.AsReadOnly(), completions.AsReadOnly(),
            propertyAlerts.AsReadOnly(), encounter);
    }

    private static void AdvanceGeneration(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        OriginalStrategicSchedulerInput input,
        IOriginalStrategicRandom random,
        List<OriginalStrategicConstruction> constructions,
        List<OriginalStrategicPropertyAlert> propertyAlerts)
    {
        state.CallsSinceReactiveSuccess = checked(state.CallsSinceReactiveSuccess + 1);
        var reactiveDetection = input.ReactiveDetection
            ?? FindReactiveDetection(state, resources, input.PursuitTargets, propertyAlerts);
        if (reactiveDetection is { } detected
            && CanAttemptReactivePursuit(
                detected.Property,
                detected.TargetMovementSlot,
                input.PlayerMovementSlot,
                state.CallsSinceReactiveSuccess,
                HasLivePursuer(state.MovementSlots.Select(slot =>
                    new StrategicMovementPursuitState(slot.Active, slot.Mode, slot.TargetMovementSlot)).ToArray(),
                    detected.TargetMovementSlot))
            && TryConstructPursuit(state, input, detected.TargetMovementSlot, detected.Property,
                out var pursuit))
        {
            constructions.Add(pursuit);
            state.CallsSinceReactiveSuccess = 0;
            state.GenerationAccumulator = checked(state.GenerationAccumulator + state.SpeedMultiplier);
            var fallbackRoll = Next(random, ReactiveSpawnRollLimit);
            if (ShouldAttemptReactiveSpawn(fallbackRoll, ActiveCount(state)))
            {
                TryConstructionFallback(
                    state, resources, input, detected.Property,
                    StrategicRouteSelection.AlternateAuthoredRoute, constructions);
                state.GenerationAccumulator = 0;
            }
            return;
        }

        if (state.GenerationAccumulator < GenerationIntervalUnits)
        {
            state.GenerationAccumulator = checked(state.GenerationAccumulator + state.SpeedMultiplier);
            return;
        }

        state.GenerationAccumulator = 0;
        if (ActiveCount(state) >= SlotCount) return;
        if (Next(random, GenerationRollLimit) <= GenerationStartThreshold) return;

        var propertyListPresent = state.PropertyListHead != 0xFF;
        // 0x3BE58 reads the property local after 0x3BB4C has returned false even
        // though 0x3BB4C does not initialize either out pointer on failure. The
        // original result is stale stack data. The clean-room scheduler admits
        // this branch only when this pass supplied a real detection.
        if (propertyListPresent && reactiveDetection is { } timedDetected
            && state.Properties[timedDetected.Property].State13 != 0)
        {
            for (var targetSlot = 0; targetSlot < SlotCount; targetSlot++)
            {
                if (!input.PursuitTargets[targetSlot].Active) continue;
                var property = SelectRandomGenerationProperty(state, random);
                if (property >= 0
                    && TryConstructPursuit(state, input, targetSlot, property, out var timedPursuit))
                {
                    constructions.Add(timedPursuit);
                    return;
                }
            }
        }

        if (!propertyListPresent)
        {
            TryConstructionFallback(state, resources, input, input.GlobalOriginProperty,
                StrategicRouteSelection.AlternateAuthoredRoute, constructions);
            return;
        }

        var selectedProperty = SelectRandomGenerationProperty(state, random);
        if (selectedProperty >= 0)
            TryConstructionFallback(state, resources, input, selectedProperty,
                StrategicRouteSelection.CanonicalPropertyPair, constructions);
    }

    private static void TryConstructionFallback(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        OriginalStrategicSchedulerInput input,
        int originProperty,
        StrategicRouteSelection routeSelection,
        List<OriginalStrategicConstruction> constructions)
    {
        if (TryConstructOrdinary(state, resources, input, input.GlobalTargetPerson,
                originProperty, RoutedMode, routeSelection, out var construction)
            || TryConstructOrdinary(state, resources, input, input.GlobalTargetPerson,
                originProperty, DirectPropertyMode, routeSelection, out construction))
            constructions.Add(construction);
    }

    private static bool TryConstructOrdinary(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        OriginalStrategicSchedulerInput input,
        int targetPerson,
        int originProperty,
        int mode,
        StrategicRouteSelection routeSelection,
        out OriginalStrategicConstruction construction)
    {
        construction = default;
        if (state.Properties[originProperty].State13 == 0) return false;
        var slotIndex = FirstFreeSlot(state.MovementSlots.OrderBy(slot => slot.Slot)
            .Select(slot => slot.Active).ToArray());
        if (slotIndex < 0 || mode is not (DirectPropertyMode or RoutedMode)) return false;

        string? routeName = null;
        var reverse = false;
        IReadOnlyList<OriginalStrategicRoutePoint>? route = null;
        if (mode == RoutedMode)
        {
            OriginalStrategicRoute selectedRoute;
            if (routeSelection == StrategicRouteSelection.CanonicalPropertyPair)
            {
                var targetProperty = state.Persons[targetPerson].Group;
                if (!TryGetPropertyRoute(originProperty, targetProperty, out selectedRoute)) return false;
            }
            else
            {
                if (!TryGetStartingRoute(state.StartingRouteSelector, out var starting)) return false;
                selectedRoute = new OriginalStrategicRoute(starting.ResourceName, false);
            }
            routeName = selectedRoute.ResourceName;
            reverse = selectedRoute.Reverse;
            route = resources.Route(routeName, reverse);
            if (route.Count == 0) return false;
        }

        var slot = state.MovementSlots.Single(candidate => candidate.Slot == slotIndex);
        ResetSlot(slot);
        var origin = state.Properties[originProperty];
        slot.Active = true;
        slot.OriginProperty = originProperty;
        slot.Lord = origin.Lord;
        slot.Mode = mode;
        slot.GridX = origin.GridX;
        slot.GridY = origin.GridY;
        (slot.CurrentX, slot.CurrentY) = RouteAnchor(origin.GridX, origin.GridY);
        if (mode == DirectPropertyMode)
        {
            var target = state.Persons[targetPerson];
            (slot.DestinationX, slot.DestinationY) = RouteAnchor(target.X, target.Y);
            SetNormalizedDirection(
                slot, slot.DestinationX - Truncate(slot.CurrentX),
                slot.DestinationY - Truncate(slot.CurrentY));
        }
        else
        {
            slot.RouteResource = routeName;
            slot.RouteReversed = reverse;
            slot.WaypointCount = route!.Count;
        }
        SetOrdinaryForces(slot, state);
        construction = new OriginalStrategicConstruction(
            slotIndex, mode, originProperty, targetPerson, -1, routeName, reverse);
        return true;
    }

    private static bool TryConstructPursuit(
        OriginalStrategicCampaignState state,
        OriginalStrategicSchedulerInput input,
        int targetSlot,
        int originProperty,
        out OriginalStrategicConstruction construction)
    {
        construction = default;
        var slotIndex = FirstFreeSlot(state.MovementSlots.OrderBy(slot => slot.Slot)
            .Select(slot => slot.Active).ToArray());
        var origin = state.Properties[originProperty];
        if (slotIndex < 0 || origin.Garrison == 0) return false;

        var target = input.PursuitTargets[targetSlot];
        var slot = state.MovementSlots.Single(candidate => candidate.Slot == slotIndex);
        ResetSlot(slot);
        slot.Active = true;
        slot.TargetMovementSlot = targetSlot;
        slot.OriginProperty = originProperty;
        slot.Lord = origin.Lord;
        slot.Mode = PursuitMode;
        slot.DestinationX = Truncate(target.CurrentX);
        slot.DestinationY = Truncate(target.CurrentY);
        slot.GridX = target.GridX;
        slot.GridY = target.GridY;
        (slot.CurrentX, slot.CurrentY) = RouteAnchor(origin.GridX, origin.GridY);
        SetNormalizedDirection(
            slot, slot.DestinationX - Truncate(slot.CurrentX),
            slot.DestinationY - Truncate(slot.CurrentY));
        SetPursuitForces(slot, state, target.Total);
        construction = new OriginalStrategicConstruction(
            slotIndex, PursuitMode, originProperty, -1, targetSlot, null, false);
        return true;
    }

    private static void SetOrdinaryForces(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state)
    {
        var household = HouseholdCount(state, slot.OriginProperty);
        var forces = InitialForces(household, state.Persons[slot.Lord].LordRating);
        slot.Swordsmen = forces.Swordsmen;
        slot.Halberdiers = forces.Halberdiers;
        slot.Knights = forces.Knights;
    }

    private static void SetPursuitForces(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        int targetTotal)
    {
        var origin = state.Properties[slot.OriginProperty];
        var detached = Math.Min(origin.Garrison, checked(targetTotal + 3));
        var household = detached == 0
            ? 1
            : Math.Min(HouseholdCount(state, slot.OriginProperty), 30);
        var total = checked(detached + household);
        if (total <= 3)
            slot.Swordsmen = 3;
        else
            slot.Swordsmen = slot.Halberdiers = slot.Knights = total / 3;
        origin.Garrison = checked((byte)(origin.Garrison - detached));
    }

    private static int HouseholdCount(
        OriginalStrategicCampaignState state,
        int property)
    {
        if (property == ReactiveSpecialProperty)
            return PersonCount - CountPersonListEntries(state);
        var group = state.Persons[state.Properties[property].Lord].Group;
        return state.Persons.Skip(1).Count(person =>
            person.Group == group && person.Assignment != 0
            && (person.Flags & HouseholdEligibleFlag) != 0);
    }

    // 0x38A8C/0x38B40 special-case property 7: 0xB0 minus 0x43558's
    // linked PersonListHead count, not a screen-provided population value.
    private static int CountPersonListEntries(OriginalStrategicCampaignState state)
    {
        var count = 0;
        var index = state.PersonListHead;
        while (index != byte.MaxValue)
        {
            if (index >= state.Persons.Count || count >= state.Persons.Count)
                throw new InvalidDataException("Strategic person list is malformed.");
            count++;
            index = state.Persons[index].ListNext;
        }
        return count;
    }

    private static int SelectRandomGenerationProperty(
        OriginalStrategicCampaignState state,
        IOriginalStrategicRandom random)
    {
        var candidates = GenerationPropertyCandidates(state.Properties.Select(property =>
            new OriginalStrategicPropertyGenerationState(property.OwnerOrState, property.State13)).ToArray());
        return candidates.Count == 0 ? -1 : candidates[Next(random, candidates.Count)];
    }

    private static OriginalStrategicReactiveDetection? FindReactiveDetection(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPursuitTarget> targets,
        List<OriginalStrategicPropertyAlert> propertyAlerts)
    {
        for (var propertyIndex = 0; propertyIndex < PropertyCount; propertyIndex++)
        {
            var property = state.Properties[propertyIndex];
            if (property.OwnerOrState == 0) continue;

            for (var targetSlot = 0; targetSlot < SlotCount; targetSlot++)
            {
                var target = targets[targetSlot];
                if (!target.Active) continue;

                var currentX = Truncate(target.CurrentX);
                var currentY = Truncate(target.CurrentY);
                var deltaX = Math.Abs((long)currentX - property.MapX8);
                var deltaY = Math.Abs((long)currentY - property.MapY8);
                var special = propertyIndex == ReactiveSpecialProperty;
                var inSpecialBounds = special && IsInsideReactiveSpecialBounds(currentX, currentY);
                var contactPerson = PersonAtGrid(state, resources, target.GridX, target.GridY);
                var lordContact = contactPerson == property.Lord;
                var nearDistance = special ? ReactiveSpecialNearDistance : ReactiveNearDistance;

                if ((deltaX < nearDistance && deltaY < nearDistance)
                    || inSpecialBounds || lordContact)
                {
                    property.State13 = 1;
                    if (property.Garrison > 0)
                        return new OriginalStrategicReactiveDetection(propertyIndex, targetSlot);
                    break;
                }

                var approachDistance = special
                    ? ReactiveSpecialApproachDistance
                    : ReactiveApproachDistance;
                if (deltaX >= approachDistance || deltaY >= approachDistance
                    || property.State14 != 0)
                    continue;

                property.State14 = 1;
                propertyAlerts.Add(new OriginalStrategicPropertyAlert(propertyIndex, targetSlot));
            }
        }
        return null;
    }

    private static bool IsInsideReactiveSpecialBounds(int x, int y) =>
        x >= ReactiveSpecialBoundsX
        && x < ReactiveSpecialBoundsX + ReactiveSpecialBoundsWidth
        && y >= ReactiveSpecialBoundsY
        && y < ReactiveSpecialBoundsY + ReactiveSpecialBoundsHeight;

    private static int? FindContactPerson(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int row,
        int column)
    {
        foreach (var probe in ContactProbeRows)
        {
            var person = PersonAtGrid(
                state, resources, checked(row + probe.X), checked(column + probe.Y));
            if (person > 0 && person < state.Persons.Count) return person;
        }
        return null;
    }

    private static int PersonAtGrid(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        int row,
        int column)
    {
        if (!resources.TryGridCell(row, column, out var cell)) return 0;
        var mutation = state.TerrainMutations.FirstOrDefault(candidate =>
            candidate.Row == cell.Row && candidate.Column == cell.Column);
        return (mutation is null ? cell : cell with { RawValue = mutation.CellValue }).Auxiliary;
    }

    private static void Retarget(
        OriginalStrategicMovementSlot slot,
        OriginalStrategicCampaignState state,
        OriginalStrategicSchedulerInput input)
    {
        slot.Mode = DirectPropertyMode;
        var selection = SelectRetarget(
            new StrategicPoint(Truncate(slot.CurrentX), Truncate(slot.CurrentY)),
            input.PursuitTargets.Select(target => new StrategicRetargetCandidate(
                target.Active, new StrategicPoint(Truncate(target.CurrentX), Truncate(target.CurrentY)))).ToArray(),
            input.PlayerPosition,
            new StrategicPoint(
                state.Properties[slot.OriginProperty].MapX8,
                state.Properties[slot.OriginProperty].MapY8));
        var destination = selection.Kind switch
        {
            StrategicRetargetKind.FieldArmy => new StrategicPoint(
                Truncate(input.PursuitTargets[selection.CandidateIndex].CurrentX),
                Truncate(input.PursuitTargets[selection.CandidateIndex].CurrentY)),
            StrategicRetargetKind.Player => input.PlayerPosition,
            _ => new StrategicPoint(
                state.Properties[slot.OriginProperty].MapX8,
                state.Properties[slot.OriginProperty].MapY8)
        };
        slot.DestinationX = destination.X;
        slot.DestinationY = destination.Y;
        SetNormalizedDirection(
            slot, destination.X - Truncate(slot.CurrentX), destination.Y - Truncate(slot.CurrentY));
    }

    private static void DeactivateSlot(OriginalStrategicMovementSlot slot)
    {
        if (slot.Mode == RoutedMode && !slot.PathComplete) slot.PathComplete = true;
        slot.Active = false;
    }

    private static void ResetSlot(OriginalStrategicMovementSlot slot)
    {
        var index = slot.Slot;
        slot.Active = false;
        slot.PathComplete = false;
        slot.TargetMovementSlot = -1;
        slot.WaypointCount = 0;
        slot.WaypointIndex = 0;
        slot.Swordsmen = 0;
        slot.Halberdiers = 0;
        slot.Knights = 0;
        slot.OriginProperty = -1;
        slot.Lord = -1;
        slot.Mode = 0;
        slot.DestinationX = 0;
        slot.DestinationY = 0;
        slot.GridX = 0;
        slot.GridY = 0;
        slot.CurrentX = 0;
        slot.CurrentY = 0;
        slot.DirectionX = 0;
        slot.DirectionY = 0;
        slot.RouteResource = null;
        slot.RouteReversed = false;
        slot.Slot = index;
    }

    private static (int X, int Y) RouteAnchor(int row, int column) =>
        (checked(80 * (row + 1)), checked(20 * (column + 1)));

    private static int ActiveCount(OriginalStrategicCampaignState state) =>
        state.MovementSlots.Count(slot => slot.Active);

    private static int Next(IOriginalStrategicRandom random, int exclusiveMaximum)
    {
        var value = random.Next(exclusiveMaximum);
        if (value < 0 || value >= exclusiveMaximum)
            throw new InvalidDataException(
                $"Strategic random source returned {value} outside [0,{exclusiveMaximum}).");
        return value;
    }

    private static void ValidateSchedulerInput(
        OriginalStrategicCampaignState state,
        OriginalStrategicSchedulerInput input)
    {
        state.Validate();
        if (input.GlobalTargetPerson is < 0 or >= PersonCount)
            throw new ArgumentOutOfRangeException(nameof(input.GlobalTargetPerson));
        ValidateProperty(input.GlobalOriginProperty);
        ValidatePlayerMovementSlot(input.PlayerMovementSlot, nameof(input.PlayerMovementSlot));
        if (input.PursuitTargets is null || input.PursuitTargets.Count != SlotCount)
            throw new ArgumentException($"Scheduler requires exactly {SlotCount} player movement targets.",
                nameof(input.PursuitTargets));
        foreach (var target in input.PursuitTargets)
        {
            if (!float.IsFinite(target.CurrentX) || !float.IsFinite(target.CurrentY)
                || target.GridX is < 0 or >= OriginalStrategicCampaignState.WorldRowCount
                || target.GridY is < 0 or >= OriginalStrategicCampaignState.WorldColumnCount
                || target.Swordsmen < 0 || target.Halberdiers < 0 || target.Knights < 0)
                throw new InvalidDataException("Scheduler target contains invalid movement state.");
        }
        if (input.ReactiveDetection is { } detection)
        {
            ValidateProperty(detection.Property);
            ValidateSlot(detection.TargetMovementSlot, nameof(detection.TargetMovementSlot));
        }
    }
}

public interface IOriginalStrategicRandom
{
    int Next(int exclusiveMaximum);
}

public sealed record OriginalStrategicSchedulerInput(
    int GlobalTargetPerson,
    int GlobalOriginProperty,
    int PlayerMovementSlot,
    StrategicPoint PlayerPosition,
    IReadOnlyList<OriginalStrategicPursuitTarget> PursuitTargets,
    OriginalStrategicReactiveDetection? ReactiveDetection = null);

public readonly record struct OriginalStrategicReactiveDetection(
    int Property,
    int TargetMovementSlot);

public readonly record struct OriginalStrategicConstruction(
    int Slot,
    int Mode,
    int OriginProperty,
    int TargetPerson,
    int TargetMovementSlot,
    string? RouteResource,
    bool RouteReversed);

public readonly record struct OriginalStrategicCompletion(
    int Slot,
    int? ContactPerson,
    StrategicContactOutcome Outcome);

public readonly record struct OriginalStrategicEncounter(int Slot, int Person);

public readonly record struct OriginalStrategicPropertyAlert(int Property, int TargetMovementSlot);

public sealed record OriginalStrategicSchedulerResult(
    IReadOnlyList<OriginalStrategicConstruction> Constructions,
    IReadOnlyList<OriginalStrategicSlotAdvance> Advances,
    IReadOnlyList<OriginalStrategicCompletion> Completions,
    IReadOnlyList<OriginalStrategicPropertyAlert> PropertyAlerts,
    OriginalStrategicEncounter? Encounter);
