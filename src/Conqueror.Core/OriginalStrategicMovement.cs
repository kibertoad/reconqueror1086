namespace Conqueror.Core;

/// <summary>
/// Executable-mapped portions of the original five-slot strategic movement system.
/// Route and lord adapters remain separate because the current campaign model does not
/// yet identify the original property records with its named destinations.
/// </summary>
public static class OriginalStrategicMovement
{
    public const int SlotCount = 5;
    public const int RecordSize = 0x118;
    public const int GenerationIntervalMilliseconds = 0x1388;
    public const int GenerationRollLimit = 0x64;
    public const int GenerationStartThreshold = 0x60;

    public const int ActiveOffset = 0x00;
    public const int PathCompleteOffset = 0x0C;
    public const int TargetLocationOffset = 0x14;
    public const int WaypointCountOffset = 0x18;
    public const int SwordsmenOffset = 0x1C;
    public const int HalberdiersOffset = 0x20;
    public const int KnightsOffset = 0x24;
    public const int OriginLocationOffset = 0x28;
    public const int LordOffset = 0x2C;
    public const int ModeOffset = 0x34;
    public const int DestinationXOffset = 0x3C;
    public const int DestinationYOffset = 0x40;
    public const int GridXOffset = 0x44;
    public const int GridYOffset = 0x48;
    public const int CurrentXOffset = 0x5C;
    public const int CurrentYOffset = 0x60;
    public const int DirectionXOffset = 0x64;
    public const int DirectionYOffset = 0x68;
    public const int RoutePointerOffset = 0x6C;

    public const int DirectPropertyMode = 1;
    public const int RoutedMode = 2;
    public const int PursuitMode = 3;
    public const int CompletedSignal = 0xFFFF;
    public const int WaypointCoordinateTolerance = 6;
    public const int MaximumRoutedStep = 50;
    public const int RetargetFieldArmyRange = 300;

    public const int PropertyTableAddress = 0xB8EC;
    public const int PropertyCount = 14;
    public const int PropertyRecordSize = 0x0F;
    public const int PropertyOwnerOrStateOffset = 0x00;
    public const int PropertyGridXOffset = 0x01;
    public const int PropertyGridYOffset = 0x03;
    public const int PropertyMapX8Offset = 0x05;
    public const int PropertyMapY8Offset = 0x07;
    public const int PropertyLordOffset = 0x09;
    public const int PropertyListNextOffset = 0x0A;
    public const int PropertyGarrisonOffset = 0x0C;
    public const int PropertyState13Offset = 0x0D;
    public const int PropertyState14Offset = 0x0E;

    public const int PersonTableAddress = 0xBA50;
    public const int PersonCount = 176;
    public const int PersonRecordSize = 0x12;
    public const int PersonNameAddressOffset = 0x00;
    public const int PersonGroupOffset = 0x04;
    public const int PersonFlagsOffset = 0x06;
    public const int PersonAssignmentOffset = 0x07;
    public const int PersonXOffset = 0x08;
    public const int PersonYOffset = 0x0A;
    public const int PersonLordRatingOffset = 0x0C;
    public const int PersonListNextOffset = 0x0D;
    public const int HouseholdEligibleFlag = 0x01;

    private static readonly OriginalStrategicPropertyDefinition[] PropertyRows =
    [
        new(1, 134, 59, 0x2A80, 0x049C, 13, 0x00FF, 18, 0, 0),
        new(2, 143, 100, 0x2D00, 0x07E4, 20, 0x00FF, 22, 0, 0),
        new(3, 90, 115, 0x1C70, 0x0910, 35, 0x00FF, 24, 0, 0),
        new(4, 112, 187, 0x2300, 0x0EB0, 59, 0x00FF, 28, 0, 0),
        new(5, 118, 159, 0x2580, 0x0C6C, 73, 0x00FF, 18, 0, 0),
        new(6, 126, 187, 0x27B0, 0x0ED8, 88, 0x00FF, 25, 0, 0),
        new(7, 161, 167, 0x32A0, 0x0D20, 96, 0x00FF, 18, 0, 0),
        new(8, 157, 213, 0x3160, 0x10A4, 100, 0x00FF, 90, 0, 0),
        new(9, 181, 137, 0x38E0, 0x0AF0, 1, 0x00FF, 24, 0, 0),
        new(10, 177, 183, 0x37A0, 0x0E60, 113, 0x00FF, 22, 0, 0),
        new(11, 146, 252, 0x2DA0, 0x13D8, 119, 0x00FF, 18, 0, 0),
        new(12, 146, 216, 0x2DA0, 0x1108, 143, 0x00FF, 24, 0, 0),
        new(13, 75, 239, 0x17C0, 0x12C0, 155, 0x00FF, 15, 0, 0),
        new(14, 68, 266, 0x1590, 0x14DC, 171, 0x00FF, 20, 0, 0)
    ];

    private static readonly int[] InitialHouseholdCounts = [4, 6, 7, 5, 2, 2, 4, 3, 3, 1, 9, 3, 3, 6];

    private static readonly OriginalStrategicPropertyIdentity[] PropertyIdentityRows =
    [
        new("York", 0x66A0, 0, 1, 30),
        new("Lincoln", 0x66F4, 1, 2, 24),
        new("Chester", 0x67B4, 2, 3, 16),
        new("Gloucester", 0x68AC, 3, 4, 20),
        new("Warwick", 0x694C, 4, 5, 18),
        new("Oxford", 0x6A04, 5, 6, 20),
        new("Cambridge", 0x6A58, 6, 7, 16),
        new("London", 0x6A80, 7, 8, 70),
        new("Norwich", 0x6628, 8, 9, 24),
        new("Colchester", 0x6AFC, 9, 10, 28),
        new("Arundel", 0x6B48, 10, 11, 33),
        new("Windsor", 0x6C4C, 11, 12, 32),
        new("Dunster", 0x6CE4, 12, 13, 28),
        new("Okehampton", 0x6D80, 13, 14, 24)
    ];

    private static readonly StrategicContactProbe[] ContactProbeRows =
    [
        new(0, 0),
        new(0, -2),
        new(1, 0),
        new(-1, -1),
        new(0, 1)
    ];

    public static IReadOnlyList<OriginalStrategicPropertyDefinition> Properties => PropertyRows;

    public static IReadOnlyList<int> InitialActiveHouseholdCounts => InitialHouseholdCounts;

    public static IReadOnlyList<OriginalStrategicPropertyIdentity> PropertyIdentities => PropertyIdentityRows;

    public static IReadOnlyList<StrategicContactProbe> ContactProbes => ContactProbeRows;

    public static StrategicContactOutcome ResolveCompletedContact(
        int originOwnerOrState,
        int? contactedAssignment,
        bool contactedPersonEligible)
    {
        if (contactedAssignment is 0 && contactedPersonEligible)
            return StrategicContactOutcome.Encounter;

        return contactedAssignment == originOwnerOrState
            ? StrategicContactOutcome.ReinforceOriginAndDeactivate
            : StrategicContactOutcome.Retarget;
    }

    public static bool TryGetPropertyRoute(int fromProperty, int toProperty, out OriginalStrategicRoute route)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromProperty);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromProperty, PropertyCount);
        ArgumentOutOfRangeException.ThrowIfNegative(toProperty);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toProperty, PropertyCount);

        if (fromProperty == toProperty ||
            (Math.Min(fromProperty, toProperty) == 0 && Math.Max(fromProperty, toProperty) == 12))
        {
            route = default;
            return false;
        }

        route = new OriginalStrategicRoute(
            $"rt_{Math.Min(fromProperty, toProperty) + 1}_{Math.Max(fromProperty, toProperty) + 1}.rat",
            Reverse: fromProperty > toProperty);
        return true;
    }

    public static StrategicRetargetSelection SelectRetarget(
        StrategicPoint current,
        IReadOnlyList<StrategicRetargetCandidate> fieldArmies,
        StrategicPoint player,
        StrategicPoint originProperty)
    {
        ArgumentNullException.ThrowIfNull(fieldArmies);
        var rangeSquared = RetargetFieldArmyRange * RetargetFieldArmyRange;
        for (var index = 0; index < fieldArmies.Count; index++)
        {
            var candidate = fieldArmies[index];
            if (candidate.Active && DistanceSquared(current, candidate.Position) < rangeSquared)
                return new StrategicRetargetSelection(StrategicRetargetKind.FieldArmy, index);
        }

        return DistanceSquared(current, player) <= DistanceSquared(current, originProperty)
            ? new StrategicRetargetSelection(StrategicRetargetKind.Player, -1)
            : new StrategicRetargetSelection(StrategicRetargetKind.OriginProperty, -1);
    }

    private static long DistanceSquared(StrategicPoint first, StrategicPoint second)
    {
        var dx = (long)first.X - second.X;
        var dy = (long)first.Y - second.Y;
        return checked(dx * dx + dy * dy);
    }

    public static StrategicTroopCounts InitialForces(int activeHouseholdCount, int lordRating)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(activeHouseholdCount);
        ArgumentOutOfRangeException.ThrowIfNegative(lordRating);

        var each = checked(activeHouseholdCount + lordRating / 4);
        return each == 0
            ? new StrategicTroopCounts(0, 0, 1)
            : new StrategicTroopCounts(each, each, each);
    }
}

public readonly record struct OriginalStrategicPropertyDefinition(
    byte OwnerOrState,
    ushort GridX,
    ushort GridY,
    ushort MapX8,
    ushort MapY8,
    byte Lord,
    ushort ListNext,
    byte Garrison,
    byte State13,
    byte State14);

public readonly record struct OriginalStrategicPropertyIdentity(
    string Name,
    int NameAddress,
    byte PersonGroup,
    byte InitialAssignment,
    byte LordRating);

public readonly record struct StrategicContactProbe(int X, int Y);

public enum StrategicContactOutcome
{
    Encounter,
    ReinforceOriginAndDeactivate,
    Retarget
}

public readonly record struct OriginalStrategicRoute(string ResourceName, bool Reverse);

public readonly record struct StrategicPoint(int X, int Y);

public readonly record struct StrategicRetargetCandidate(bool Active, StrategicPoint Position);

public readonly record struct StrategicRetargetSelection(StrategicRetargetKind Kind, int CandidateIndex);

public enum StrategicRetargetKind
{
    FieldArmy,
    Player,
    OriginProperty
}

public readonly record struct StrategicTroopCounts(int Swordsmen, int Halberdiers, int Knights)
{
    public int Total => checked(Swordsmen + Halberdiers + Knights);
}
