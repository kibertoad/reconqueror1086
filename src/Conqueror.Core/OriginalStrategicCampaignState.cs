namespace Conqueror.Core;

/// <summary>
/// Mutable save state for the executable-mapped strategic system. Definitions in
/// <see cref="OriginalStrategicMovement"/> remain immutable initialization sources.
/// </summary>
public sealed class OriginalStrategicCampaignState
{
    public const int WorldRowCount = 200;
    public const int WorldColumnCount = 400;

    public int StartingRouteSelector { get; set; } = -1;
    public int SpeedMultiplier { get; set; } = OriginalStrategicMovement.InitialSpeedMultiplier;
    public int GenerationAccumulator { get; set; }
    public int CallsSinceReactiveSuccess { get; set; }
    public int CameraRow { get; set; }
    public int CameraColumn { get; set; }
    public StrategicTerrainProfile TerrainProfile { get; set; }
    public byte PropertyListHead { get; set; } = 0xFF;
    public byte PersonListHead { get; set; } = 0xFF;
    public List<OriginalStrategicPropertyState> Properties { get; init; } = [];
    public List<OriginalStrategicPersonState> Persons { get; init; } = [];
    public List<OriginalStrategicMovementSlot> MovementSlots { get; init; } = [];
    public List<OriginalStrategicTerrainMutation> TerrainMutations { get; init; } = [];

    public static OriginalStrategicCampaignState CreateForNewGame(
        DateTime date,
        int startingRouteSelector,
        int speedMultiplier = OriginalStrategicMovement.InitialSpeedMultiplier)
    {
        if (startingRouteSelector is < 0 or >= OriginalStrategicMovement.StartingRouteCount)
            throw new ArgumentOutOfRangeException(nameof(startingRouteSelector));
        return Create(date, startingRouteSelector, speedMultiplier);
    }

    public static OriginalStrategicCampaignState CreateForSchemaOneMigration(
        DateTime date,
        int speedMultiplier) => Create(date, startingRouteSelector: -1, speedMultiplier);

    public void Validate()
    {
        if (StartingRouteSelector is < -1 or >= OriginalStrategicMovement.StartingRouteCount)
            throw new InvalidDataException("Strategic state contains an invalid starting-route selector.");
        if (SpeedMultiplier is < OriginalStrategicMovement.MinimumSpeedMultiplier
            or > OriginalStrategicMovement.MaximumSpeedMultiplier)
            throw new InvalidDataException("Strategic state contains an invalid speed multiplier.");
        if (GenerationAccumulator < 0 || CallsSinceReactiveSuccess < 0)
            throw new InvalidDataException("Strategic state contains an invalid generator counter.");
        if (CameraRow is < 0 or >= WorldRowCount || CameraColumn is < 0 or >= WorldColumnCount)
            throw new InvalidDataException("Strategic state contains an invalid camera position.");
        if (!Enum.IsDefined(TerrainProfile))
            throw new InvalidDataException("Strategic state contains an invalid terrain profile.");
        if (Properties is null || Properties.Count != OriginalStrategicMovement.PropertyCount)
            throw new InvalidDataException("Strategic state must contain all original properties.");
        if (Persons is null || Persons.Count != OriginalStrategicMovement.PersonCount)
            throw new InvalidDataException("Strategic state must contain all original people.");
        if (MovementSlots is null || MovementSlots.Count != OriginalStrategicMovement.SlotCount
            || MovementSlots.Select(slot => slot.Slot).Distinct().Count() != OriginalStrategicMovement.SlotCount
            || MovementSlots.Any(slot => slot.Slot is < 0 or >= OriginalStrategicMovement.SlotCount))
            throw new InvalidDataException("Strategic state must contain the five original movement slots.");
        if (TerrainMutations is null || TerrainMutations.Any(mutation =>
                mutation.Row is < 0 or >= WorldRowCount
                || mutation.Column is < 0 or >= WorldColumnCount)
            || TerrainMutations.Select(mutation => (mutation.Row, mutation.Column)).Distinct().Count()
                != TerrainMutations.Count)
            throw new InvalidDataException("Strategic state contains invalid terrain mutations.");

        foreach (var property in Properties)
            if (property.Lord >= Persons.Count)
                throw new InvalidDataException("Strategic property references an invalid lord.");
        foreach (var slot in MovementSlots) slot.Validate();
    }

    private static OriginalStrategicCampaignState Create(
        DateTime date,
        int startingRouteSelector,
        int speedMultiplier)
    {
        if (startingRouteSelector is < -1 or >= OriginalStrategicMovement.StartingRouteCount)
            throw new ArgumentOutOfRangeException(nameof(startingRouteSelector));
        if (speedMultiplier is < OriginalStrategicMovement.MinimumSpeedMultiplier
            or > OriginalStrategicMovement.MaximumSpeedMultiplier)
            throw new ArgumentOutOfRangeException(nameof(speedMultiplier));

        var state = new OriginalStrategicCampaignState
        {
            StartingRouteSelector = startingRouteSelector,
            SpeedMultiplier = speedMultiplier,
            TerrainProfile = OriginalStrategicMovement.TerrainProfileForMonth(date.Month - 1)
        };
        state.Properties.AddRange(OriginalStrategicMovement.Properties.Select(
            OriginalStrategicPropertyState.FromDefinition));
        state.Persons.AddRange(OriginalStrategicMovement.Persons.Select(
            OriginalStrategicPersonState.FromDefinition));
        state.MovementSlots.AddRange(Enumerable.Range(0, OriginalStrategicMovement.SlotCount)
            .Select(slot => new OriginalStrategicMovementSlot { Slot = slot }));
        state.Validate();
        return state;
    }
}

public sealed class OriginalStrategicPropertyState
{
    public byte OwnerOrState { get; set; }
    public ushort GridX { get; set; }
    public ushort GridY { get; set; }
    public ushort MapX8 { get; set; }
    public ushort MapY8 { get; set; }
    public byte Lord { get; set; }
    public ushort ListNext { get; set; }
    public byte Garrison { get; set; }
    public byte State13 { get; set; }
    public byte State14 { get; set; }

    public static OriginalStrategicPropertyState FromDefinition(OriginalStrategicPropertyDefinition definition) =>
        new()
        {
            OwnerOrState = definition.OwnerOrState,
            GridX = definition.GridX,
            GridY = definition.GridY,
            MapX8 = definition.MapX8,
            MapY8 = definition.MapY8,
            Lord = definition.Lord,
            ListNext = definition.ListNext,
            Garrison = definition.Garrison,
            State13 = definition.State13,
            State14 = definition.State14
        };
}

public sealed class OriginalStrategicPersonState
{
    public int NameAddress { get; set; }
    public byte Group { get; set; }
    public byte State5 { get; set; }
    public byte Flags { get; set; }
    public byte Assignment { get; set; }
    public ushort X { get; set; }
    public ushort Y { get; set; }
    public byte LordRating { get; set; }
    public byte ListNext { get; set; }
    public byte State14 { get; set; }
    public byte State15 { get; set; }
    public byte State16 { get; set; }
    public byte State17 { get; set; }

    public static OriginalStrategicPersonState FromDefinition(OriginalStrategicPersonDefinition definition) =>
        new()
        {
            NameAddress = definition.NameAddress,
            Group = definition.Group,
            State5 = definition.State5,
            Flags = definition.Flags,
            Assignment = definition.Assignment,
            X = definition.X,
            Y = definition.Y,
            LordRating = definition.LordRating,
            ListNext = definition.ListNext,
            State14 = definition.State14,
            State15 = definition.State15,
            State16 = definition.State16,
            State17 = definition.State17
        };
}

public sealed class OriginalStrategicMovementSlot
{
    public int Slot { get; set; }
    public bool Active { get; set; }
    public bool PathComplete { get; set; }
    public int TargetLocation { get; set; } = -1;
    public int TargetMovementSlot { get; set; } = -1;
    public int WaypointCount { get; set; }
    public int WaypointIndex { get; set; }
    public int Swordsmen { get; set; }
    public int Halberdiers { get; set; }
    public int Knights { get; set; }
    public int OriginProperty { get; set; } = -1;
    public int Lord { get; set; } = -1;
    public int Mode { get; set; }
    public int DestinationX { get; set; }
    public int DestinationY { get; set; }
    public int GridX { get; set; }
    public int GridY { get; set; }
    public float CurrentX { get; set; }
    public float CurrentY { get; set; }
    public float DirectionX { get; set; }
    public float DirectionY { get; set; }
    public string? RouteResource { get; set; }
    public bool RouteReversed { get; set; }

    public int Total => checked(Swordsmen + Halberdiers + Knights);

    public void Validate()
    {
        if (Slot is < 0 or >= OriginalStrategicMovement.SlotCount
            || Swordsmen < 0 || Halberdiers < 0 || Knights < 0
            || WaypointCount < 0 || WaypointIndex < 0 || WaypointIndex > WaypointCount
            || !float.IsFinite(CurrentX) || !float.IsFinite(CurrentY)
            || !float.IsFinite(DirectionX) || !float.IsFinite(DirectionY))
            throw new InvalidDataException("Strategic movement slot contains invalid persisted fields.");
        if (!Active) return;
        if (Mode is < OriginalStrategicMovement.DirectPropertyMode
            or > OriginalStrategicMovement.PursuitMode
            || OriginProperty is < 0 or >= OriginalStrategicMovement.PropertyCount
            || Lord is < 0 or >= OriginalStrategicMovement.PersonCount
            || Total <= 0)
            throw new InvalidDataException("Active strategic movement slot is incomplete.");
        if (Mode == OriginalStrategicMovement.RoutedMode
            && (string.IsNullOrWhiteSpace(RouteResource) || WaypointCount == 0
                || !IsKnownRouteResource(RouteResource)))
            throw new InvalidDataException("Routed strategic movement slot has an invalid route cursor.");
        if (Mode == OriginalStrategicMovement.PursuitMode
            && TargetMovementSlot is < 0 or >= OriginalStrategicMovement.SlotCount)
            throw new InvalidDataException("Pursuit movement slot has an invalid target slot.");
    }

    private static bool IsKnownRouteResource(string resource) =>
        OriginalStrategicMovement.PropertyRouteResources.Any(route =>
            route.ResourceName.Equals(resource, StringComparison.OrdinalIgnoreCase))
        || OriginalStrategicMovement.StartingRoutes.Any(route =>
            route.ResourceName.Equals(resource, StringComparison.OrdinalIgnoreCase));
}

public sealed record OriginalStrategicTerrainMutation(int Row, int Column, uint CellValue);

public readonly record struct StrategicSchemaOneSettlement(
    int ReturnedColumns,
    int DispersedColumns,
    int ReturnedTroops,
    int DispersedTroops);

public static class StrategicSchemaTwoMigration
{
    public static StrategicSchemaOneSettlement Prepare(CampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.SchemaVersion != 1)
            throw new InvalidOperationException("Only schema-1 campaign state can be prepared for schema 2.");
        if (state.OriginalStrategicState is not null)
            throw new InvalidOperationException("Campaign already contains schema-2 strategic state.");
        ValidateSchemaOneRoster(state);

        var returnedColumns = 0;
        var dispersedColumns = 0;
        var returnedTroops = 0;
        var dispersedTroops = 0;
        foreach (var movement in state.EnemyMovements.OrderBy(movement => movement.Slot))
        {
            var total = checked(movement.Swordsmen + movement.Halberdiers + movement.Knights);
            var validOrigin = movement.Origin > 0 && movement.Origin < World.Locations.Length
                && World.Locations[movement.Origin].Kind is LocationKind.Castle or LocationKind.London
                && !state.ConqueredLocations.Contains(movement.Origin);
            if (validOrigin)
            {
                var current = state.GarrisonStrength.TryGetValue(movement.Origin, out var persisted)
                    ? persisted
                    : World.Locations[movement.Origin].Garrison;
                state.GarrisonStrength[movement.Origin] = checked(current + total);
                returnedColumns++;
                returnedTroops = checked(returnedTroops + total);
                AppendJournal(state, $"Schema 2 migration returns dated movement slot {movement.Slot} " +
                    $"({total} troops) to {World.Locations[movement.Origin].Name}.");
            }
            else
            {
                dispersedColumns++;
                dispersedTroops = checked(dispersedTroops + total);
                AppendJournal(state, $"Schema 2 migration disperses dated movement slot {movement.Slot} " +
                    $"({total} troops) because its origin is no longer hostile.");
            }
        }

        state.EnemyMovements.Clear();
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
            state.Date,
            Math.Clamp(state.DaySpeed, OriginalStrategicMovement.MinimumSpeedMultiplier,
                OriginalStrategicMovement.MaximumSpeedMultiplier));
        return new(returnedColumns, dispersedColumns, returnedTroops, dispersedTroops);
    }

    private static void ValidateSchemaOneRoster(CampaignState state)
    {
        if (state.EnemyMovements is null
            || state.EnemyMovements.Count > OriginalStrategicMovement.SlotCount
            || state.EnemyMovements.Select(movement => movement.Slot).Distinct().Count()
                != state.EnemyMovements.Count)
            throw new InvalidDataException("Campaign contains an invalid schema-1 movement roster.");
        foreach (var movement in state.EnemyMovements)
        {
            var total = (long)movement.Swordsmen + movement.Halberdiers + movement.Knights;
            if (movement.Slot is < 0 or >= OriginalStrategicMovement.SlotCount
                || movement.Origin <= 0 || movement.Origin >= World.Locations.Length
                || movement.Destination <= 0 || movement.Destination >= World.Locations.Length
                || movement.Origin == movement.Destination || movement.Arrives <= movement.Departed
                || movement.Swordsmen < 0 || movement.Halberdiers < 0 || movement.Knights < 0
                || total is <= 0 or > int.MaxValue)
                throw new InvalidDataException("Campaign contains an invalid schema-1 movement record.");
        }
    }

    private static void AppendJournal(CampaignState state, string message)
    {
        state.Journal.Add($"{state.Date:dd MMM yyyy}: {message}");
        while (state.Journal.Count > 60) state.Journal.RemoveAt(0);
    }
}
