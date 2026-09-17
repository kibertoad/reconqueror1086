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
