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

    [Fact]
    public void StrategicInteractiveStateZeroComposesTargetDestinationAndAutomaticBranchesInOrder()
    {
        var contact = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0));
        contact[0].PositionX = 100;
        contact[0].PositionY = 100;
        contact[1].PositionX = 120;
        contact[1].PositionY = 100;
        Assert.True(OriginalStrategicInteractiveEncounter.AdvanceMappedStateZero(
            contact, contact.Select(OriginalStrategicInteractiveEncounterGeometry.RenderRectangleFor).ToArray(),
            0, 640, 480, playerLaneCount: 1, enemyLaneCount: 1));
        Assert.Equal((1, OriginalStrategicInteractiveEncounterCombat.ContactStateCode),
            (contact[0].TargetUnitIndex, contact[0].StateCode));

        var destination = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(0, 0, 0));
        destination[0].PositionX = 100;
        destination[0].PositionY = 100;
        destination[0].AuxiliaryX = 106;
        destination[0].AuxiliaryY = 108;
        Assert.True(OriginalStrategicInteractiveEncounter.AdvanceMappedStateZero(
            destination, destination.Select(OriginalStrategicInteractiveEncounterGeometry.RenderRectangleFor).ToArray(),
            0, 640, 480, playerLaneCount: 1, enemyLaneCount: 0));
        Assert.Equal((2, 100, 100),
            (destination[0].HeadingOctant, destination[0].PositionX, destination[0].PositionY));

        var automatic = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0));
        automatic[0].ControlCode = 1;
        automatic[1].PositionX = 100;
        automatic[1].PositionY = 100;
        Assert.True(OriginalStrategicInteractiveEncounter.AdvanceMappedStateZero(
            automatic, automatic.Select(OriginalStrategicInteractiveEncounterGeometry.RenderRectangleFor).ToArray(),
            0, 640, 480, playerLaneCount: 1, enemyLaneCount: 1));
        Assert.Equal((100, 100, -1),
            (automatic[0].AuxiliaryX, automatic[0].AuxiliaryY, automatic[0].TargetUnitIndex));
    }

    [Fact]
    public void StrategicInteractiveTacticalPassUsesOneSnapshotAndReentersStateZeroOnlyAfterContactClears()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        session.Units[0].PositionX = 100;
        session.Units[0].PositionY = 100;
        session.Units[1].PositionX = 120;
        session.Units[1].PositionY = 100;
        var random = new QueueEncounterRandom(6, 0);

        session.AdvanceMappedTacticalPass(640, 480, playerScoreModifier: 0, contactSideFilter: 0, random);
        Assert.Equal((OriginalStrategicInteractiveEncounterCombat.ContactStateCode, 0, 1),
            (session.Units[0].StateCode, session.Units[0].PhaseCounter, session.Units[0].TargetUnitIndex));

        session.AdvanceMappedTacticalPass(640, 480, playerScoreModifier: 0, contactSideFilter: 0, random);
        Assert.Equal((OriginalStrategicInteractiveEncounterCombat.ContactStateCode, 1, 100),
            (session.Units[0].StateCode, session.Units[0].PhaseCounter, session.Units[1].RemainingStrength));
        session.AdvanceMappedTacticalPass(640, 480, playerScoreModifier: 0, contactSideFilter: 0, random);
        Assert.Equal((2, 94), (session.Units[0].PhaseCounter, session.Units[1].RemainingStrength));

        session.Units[1].StateCode = OriginalStrategicInteractiveEncounterCombat.DeathAnimationStateCode;
        session.Units[1].PhaseCounter = 3;
        session.Units[1].RemainingStrength = 10;
        session.AdvanceMappedTacticalPass(640, 480, playerScoreModifier: 0, contactSideFilter: 0, random);
        Assert.Equal((0, 4, 1, 0),
            (session.Units[1].RemainingStrength, session.Units[1].PhaseCounter,
                session.PlayerLaneCount, session.EnemyLaneCount));
    }
}
