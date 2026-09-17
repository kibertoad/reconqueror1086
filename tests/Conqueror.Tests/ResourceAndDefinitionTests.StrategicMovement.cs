using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OriginalStrategicMovementLayoutMatchesExecutableRecord()
    {
        Assert.Equal(5, OriginalStrategicMovement.SlotCount);
        Assert.Equal(0x118, OriginalStrategicMovement.RecordSize);
        Assert.Equal(0x1388, OriginalStrategicMovement.GenerationIntervalMilliseconds);
        Assert.Equal(0x64, OriginalStrategicMovement.GenerationRollLimit);
        Assert.Equal(0x60, OriginalStrategicMovement.GenerationStartThreshold);

        Assert.Equal(0x00, OriginalStrategicMovement.ActiveOffset);
        Assert.Equal(0x14, OriginalStrategicMovement.TargetLocationOffset);
        Assert.Equal(0x1C, OriginalStrategicMovement.SwordsmenOffset);
        Assert.Equal(0x20, OriginalStrategicMovement.HalberdiersOffset);
        Assert.Equal(0x24, OriginalStrategicMovement.KnightsOffset);
        Assert.Equal(0x28, OriginalStrategicMovement.OriginLocationOffset);
        Assert.Equal(0x2C, OriginalStrategicMovement.LordOffset);
        Assert.Equal(0x34, OriginalStrategicMovement.ModeOffset);
        Assert.Equal(0x5C, OriginalStrategicMovement.CurrentXOffset);
        Assert.Equal(0x60, OriginalStrategicMovement.CurrentYOffset);
        Assert.Equal(0x64, OriginalStrategicMovement.DirectionXOffset);
        Assert.Equal(0x68, OriginalStrategicMovement.DirectionYOffset);
    }

    [Fact]
    public void OriginalStrategicPropertyLayoutAndRowsMatchExecutableTable()
    {
        Assert.Equal(0xB8EC, OriginalStrategicMovement.PropertyTableAddress);
        Assert.Equal(14, OriginalStrategicMovement.PropertyCount);
        Assert.Equal(0x0F, OriginalStrategicMovement.PropertyRecordSize);
        Assert.Equal(0x00, OriginalStrategicMovement.PropertyOwnerOrStateOffset);
        Assert.Equal(0x01, OriginalStrategicMovement.PropertyGridXOffset);
        Assert.Equal(0x03, OriginalStrategicMovement.PropertyGridYOffset);
        Assert.Equal(0x05, OriginalStrategicMovement.PropertyMapX8Offset);
        Assert.Equal(0x07, OriginalStrategicMovement.PropertyMapY8Offset);
        Assert.Equal(0x09, OriginalStrategicMovement.PropertyLordOffset);
        Assert.Equal(0x0A, OriginalStrategicMovement.PropertyListNextOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PropertyGarrisonOffset);
        Assert.Equal(0x0D, OriginalStrategicMovement.PropertyState13Offset);
        Assert.Equal(0x0E, OriginalStrategicMovement.PropertyState14Offset);

        OriginalStrategicPropertyDefinition[] expected =
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

        Assert.Equal(expected, OriginalStrategicMovement.Properties);
    }

    [Fact]
    public void OriginalStrategicPersonLayoutAndInitialHouseholdCountsMatchExecutableTable()
    {
        Assert.Equal(0xBA50, OriginalStrategicMovement.PersonTableAddress);
        Assert.Equal(176, OriginalStrategicMovement.PersonCount);
        Assert.Equal(0x12, OriginalStrategicMovement.PersonRecordSize);
        Assert.Equal(0x04, OriginalStrategicMovement.PersonGroupOffset);
        Assert.Equal(0x06, OriginalStrategicMovement.PersonFlagsOffset);
        Assert.Equal(0x07, OriginalStrategicMovement.PersonAssignmentOffset);
        Assert.Equal(0x08, OriginalStrategicMovement.PersonXOffset);
        Assert.Equal(0x0A, OriginalStrategicMovement.PersonYOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PersonLordRatingOffset);
        Assert.Equal(0x0D, OriginalStrategicMovement.PersonListNextOffset);
        Assert.Equal(0x01, OriginalStrategicMovement.HouseholdEligibleFlag);
        Assert.Equal([4, 6, 7, 5, 2, 2, 4, 3, 3, 1, 9, 3, 3, 6],
            OriginalStrategicMovement.InitialActiveHouseholdCounts);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 1)]
    [InlineData(0, 3, 0, 0, 1)]
    [InlineData(0, 4, 1, 1, 1)]
    [InlineData(7, 11, 9, 9, 9)]
    [InlineData(2, 255, 65, 65, 65)]
    public void OriginalStrategicMovementUsesLordQuarterAndActiveHousehold(
        int household, int lordRating, int swordsmen, int halberdiers, int knights)
    {
        var force = OriginalStrategicMovement.InitialForces(household, lordRating);

        Assert.Equal(new StrategicTroopCounts(swordsmen, halberdiers, knights), force);
        Assert.Equal(swordsmen + halberdiers + knights, force.Total);
    }
}
