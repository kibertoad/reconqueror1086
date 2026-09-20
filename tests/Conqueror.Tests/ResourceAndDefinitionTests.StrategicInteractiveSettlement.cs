using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void InteractiveStrategicEncounterRestoresPlayerReservesAndWritesTerminalSurvivors()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(state.Date, 0);
        var strategic = state.OriginalStrategicState;
        var army = state.Player.ArmyAt(0);
        army.Units[UnitType.Swordsmen] = 10;
        army.Units[UnitType.Halberdiers] = 30;
        army.Units[UnitType.Knights] = 62;
        var player = strategic.PlayerMovementSlots[0];
        player.Active = true;
        strategic.ActivePlayerRecordCount = 1;
        var hostile = strategic.MovementSlots[0];
        hostile.Active = true;
        hostile.Mode = OriginalStrategicMovement.DirectPropertyMode;
        hostile.OriginProperty = 0;
        hostile.Lord = strategic.Properties[0].Lord;
        hostile.Swordsmen = 1;
        hostile.Halberdiers = 1;
        hostile.Knights = 1;
        var campaign = new Campaign(state);
        var encounter = new OriginalStrategicPlayerEnemyEncounter(0, 0,
            new OriginalStrategicEncounterForces(10, 30, 62),
            new OriginalStrategicEncounterForces(1, 1, 1));
        hostile.Knights++;
        Assert.Throws<InvalidOperationException>(() => campaign.BeginInteractiveOriginalStrategicEncounter(
            encounter, menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero));
        hostile.Knights--;
        var session = campaign.BeginInteractiveOriginalStrategicEncounter(
            encounter,
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        Assert.Equal(60, session.Units.Count(unit =>
            unit.Side == OriginalStrategicInteractiveEncounterSide.Player));
        Assert.Equal(3, session.Units.Count(unit =>
            unit.Side == OriginalStrategicInteractiveEncounterSide.Enemy));
        session.CompleteDeathAnimation(session.Units.Count - 1);
        session.CompleteDeathAnimation(session.Units.Count - 2);
        session.CompleteDeathAnimation(session.Units.Count - 3);
        Assert.True(session.ResolveMappedRawResolverExit(0x12D));

        var applied = campaign.ResolveInteractiveOriginalStrategicEncounter(encounter, session);

        Assert.Equal(OriginalStrategicInteractiveEncounterOutcome.PlayerWithdrew, applied.Outcome);
        Assert.Equal(new OriginalStrategicEncounterForces(10, 30, 62), applied.PlayerFinalForces);
        Assert.Equal(new OriginalStrategicEncounterForces(0, 0, 0), applied.EnemyFinalForces);
        Assert.Equal((10, 30, 62), (army.Units[UnitType.Swordsmen], army.Units[UnitType.Halberdiers], army.Units[UnitType.Knights]));
        Assert.Equal((0, 0, 0), (hostile.Swordsmen, hostile.Halberdiers, hostile.Knights));
    }
}
