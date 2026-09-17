namespace Conqueror.Core;

public sealed partial class Campaign
{
    private const int OriginalEnemyMovementSlotCount = OriginalStrategicMovement.SlotCount;
    private const int EnemyMovementRollLimit = OriginalStrategicMovement.GenerationRollLimit;
    private const int EnemyMovementStartThreshold = OriginalStrategicMovement.GenerationStartThreshold;

    private void ResolveSpyReportFromEnemyMovement()
    {
        if (State.Player.ActiveSpies <= 0) return;
        var movement = State.EnemyMovements.OrderBy(item => item.Slot).FirstOrDefault();
        if (movement is null) return;

        State.Player.ActiveSpies = 0;
        State.LatestSpyReport = new StrategicSpyReport(
            State.Date, movement.Slot, movement.Origin,
            movement.Swordsmen, movement.Halberdiers, movement.Knights);
        Log($"Spy report from {World.Locations[movement.Origin].Name}: " +
            $"{movement.Swordsmen} swordsmen, {movement.Halberdiers} halberdiers, " +
            $"and {movement.Knights} knights are on the march.");
    }

    private void ResolveEnemyMovements()
    {
        foreach (var movement in State.EnemyMovements.OrderBy(item => item.Slot).ToArray())
        {
            if (movement.Arrives > State.Date) continue;
            State.EnemyMovements.Remove(movement);
            if (State.ConqueredLocations.Contains(movement.Destination))
            {
                if (!State.ConqueredLocations.Contains(movement.Origin))
                {
                    State.GarrisonStrength[movement.Origin] = GarrisonAt(movement.Origin) + movement.Total;
                    Log($"The enemy column from {World.Locations[movement.Origin].Name} turns back after losing its destination.");
                }
                else
                {
                    Log("An enemy column disperses after losing both its origin and destination.");
                }
                continue;
            }

            State.GarrisonStrength[movement.Destination] = GarrisonAt(movement.Destination) + movement.Total;
            Log($"An enemy column reaches {World.Locations[movement.Destination].Name}.");
        }
    }

    private void TryStartEnemyMovement()
    {
        if (State.EnemyMovements.Count >= OriginalEnemyMovementSlotCount
            || _random.Next(EnemyMovementRollLimit) <= EnemyMovementStartThreshold) return;

        var busyOrigins = State.EnemyMovements.Select(item => item.Origin).ToHashSet();
        var origins = Enumerable.Range(1, World.Locations.Length - 1)
            .Where(index => IsHostileStronghold(index) && GarrisonAt(index) >= 6 && !busyOrigins.Contains(index))
            .ToArray();
        if (origins.Length == 0) return;
        var origin = origins[_random.Next(origins.Length)];
        var destinations = Enumerable.Range(1, World.Locations.Length - 1)
            .Where(index => index != origin && IsHostileStronghold(index)).ToArray();
        if (destinations.Length == 0) return;
        var destination = destinations[_random.Next(destinations.Length)];

        var slot = Enumerable.Range(0, OriginalEnemyMovementSlotCount)
            .First(index => State.EnemyMovements.All(item => item.Slot != index));
        var share = Math.Max(1, GarrisonAt(origin) / 9);
        var total = share * 3;
        State.GarrisonStrength[origin] = GarrisonAt(origin) - total;
        var arrives = State.Date.AddDays(World.TravelDays(origin, destination));
        State.EnemyMovements.Add(new StrategicEnemyMovement(
            slot, origin, destination, State.Date, arrives, share, share, share));
        Log($"An enemy column leaves {World.Locations[origin].Name} for {World.Locations[destination].Name}.");
    }
}
