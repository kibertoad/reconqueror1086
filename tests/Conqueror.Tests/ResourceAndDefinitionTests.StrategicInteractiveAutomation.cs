using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicInteractiveStateZeroAutomationSelectsTheFirstNearestOpposingRecord()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(0, 0, 2));
        var source = units[0];
        source.ControlCode = 1;
        source.PositionX = 0;
        source.PositionY = 0;
        units[1].PositionX = 3;
        units[1].PositionY = 4;
        units[2].PositionX = -3;
        units[2].PositionY = 4;

        Assert.True(OriginalStrategicInteractiveEncounter.TryAssignMappedAutomaticDestination(
            units, 0, playerLaneCount: 1, enemyLaneCount: 2));
        Assert.Equal((3, 4), (source.AuxiliaryX, source.AuxiliaryY));

        source.AuxiliaryX = -1;
        source.AuxiliaryY = -1;
        units[1].AuxiliaryX = 13;
        units[1].AuxiliaryY = 17;
        Assert.True(OriginalStrategicInteractiveEncounter.TryAssignMappedAutomaticDestination(
            units, 0, playerLaneCount: 1, enemyLaneCount: 2));
        Assert.Equal((-1, 17, 0), (source.AuxiliaryX, source.AuxiliaryY, units[1].AuxiliaryX));

        source.AuxiliaryY = -1;
        Assert.False(OriginalStrategicInteractiveEncounter.TryAssignMappedAutomaticDestination(
            units, 0, playerLaneCount: 1, enemyLaneCount: 0));
    }
}
