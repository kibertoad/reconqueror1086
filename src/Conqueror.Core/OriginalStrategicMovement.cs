namespace Conqueror.Core;

/// <summary>
/// Executable-mapped portions of the original five-slot strategic movement system.
/// Route and lord adapters remain separate because the current campaign model does not
/// yet represent the original 15-byte location and 18-byte person records.
/// </summary>
public static class OriginalStrategicMovement
{
    public const int SlotCount = 5;
    public const int RecordSize = 0x118;
    public const int GenerationIntervalMilliseconds = 0x1388;
    public const int GenerationRollLimit = 0x64;
    public const int GenerationStartThreshold = 0x60;

    public const int ActiveOffset = 0x00;
    public const int TargetLocationOffset = 0x14;
    public const int SwordsmenOffset = 0x1C;
    public const int HalberdiersOffset = 0x20;
    public const int KnightsOffset = 0x24;
    public const int OriginLocationOffset = 0x28;
    public const int LordOffset = 0x2C;
    public const int ModeOffset = 0x34;
    public const int CurrentXOffset = 0x5C;
    public const int CurrentYOffset = 0x60;
    public const int DirectionXOffset = 0x64;
    public const int DirectionYOffset = 0x68;

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

public readonly record struct StrategicTroopCounts(int Swordsmen, int Halberdiers, int Knights)
{
    public int Total => checked(Swordsmen + Halberdiers + Knights);
}
