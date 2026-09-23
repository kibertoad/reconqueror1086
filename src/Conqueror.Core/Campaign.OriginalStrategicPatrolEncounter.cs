namespace Conqueror.Core;

/// <summary>Settlement of caller 0x3B365's direct patrol resolver.</summary>
public sealed record OriginalStrategicTemporaryForceSettlement(
    OriginalStrategicEncounterForces PlayerSurvivors,
    OriginalStrategicEncounterForces PatrolSurvivors,
    bool PatrolCleared,
    bool PlayerFieldRecordRemoved,
    bool DistinguishedPlayerLossRequiresModal,
    int Reward);

public sealed partial class Campaign
{
    public OriginalStrategicTemporaryForceSettlement ResolveAutomaticOriginalStrategicPatrolEncounter(
        OriginalStrategicTemporaryForceEncounter encounter,
        IOriginalStrategicEncounterRandom random) =>
        ResolveAutomaticOriginalStrategicPatrolEncounter(encounter, random,
            new CampaignStrategicRandom(_random));

    public OriginalStrategicTemporaryForceSettlement ResolveInteractiveOriginalStrategicPatrolEncounter(
        OriginalStrategicTemporaryForceEncounter encounter,
        OriginalStrategicInteractiveEncounterSession session) =>
        ResolveInteractiveOriginalStrategicPatrolEncounter(encounter, session,
            new CampaignStrategicRandom(_random));

    public OriginalStrategicInteractiveEncounterSession BeginInteractiveOriginalStrategicPatrolEncounter(
        OriginalStrategicTemporaryForceEncounter encounter, int menuCode,
        int horizontalSpan, int verticalSpan, TimeSpan initialTime,
        IOriginalStrategicEncounterRandom? random = null)
    {
        ValidatePatrolEncounter(encounter);
        return OriginalStrategicInteractiveEncounterSession.Create(encounter.Preparation,
            menuCode, horizontalSpan, verticalSpan, initialTime, random);
    }

    public OriginalStrategicTemporaryForceSettlement ResolveAutomaticOriginalStrategicPatrolEncounter(
        OriginalStrategicTemporaryForceEncounter encounter,
        IOriginalStrategicEncounterRandom random,
        IOriginalStrategicRandom rewardRandom)
    {
        ValidatePatrolEncounter(encounter);
        var result = OriginalStrategicEncounterStaging.ResolveAutomatic(
            encounter.Preparation, 0, 0, random);
        return SettlePatrol(encounter, result.PlayerFinalForces,
            result.PlayerWon ? new(0, 0, 0) : result.EnemyFinalForces, rewardRandom);
    }

    public OriginalStrategicTemporaryForceSettlement ResolveInteractiveOriginalStrategicPatrolEncounter(
        OriginalStrategicTemporaryForceEncounter encounter,
        OriginalStrategicInteractiveEncounterSession session,
        IOriginalStrategicRandom rewardRandom)
    {
        ValidatePatrolEncounter(encounter);
        ArgumentNullException.ThrowIfNull(session);
        if (session.Outcome == OriginalStrategicInteractiveEncounterOutcome.InProgress)
            throw new InvalidOperationException("Patrol encounter has not ended.");
        var player = OriginalStrategicInteractiveEncounter.CountSurvivors(
            session.Units, OriginalStrategicInteractiveEncounterSide.Player);
        var patrol = OriginalStrategicInteractiveEncounter.CountSurvivors(
            session.Units, OriginalStrategicInteractiveEncounterSide.Enemy);
        if (session.Outcome == OriginalStrategicInteractiveEncounterOutcome.EnemyDefeated)
            patrol = new(0, 0, 0);
        return SettlePatrol(encounter, player, patrol, rewardRandom);
    }

    private void ValidatePatrolEncounter(OriginalStrategicTemporaryForceEncounter encounter)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        if (State.OriginalStrategicState is not { } strategic)
            throw new InvalidOperationException("Campaign has no original strategic movement state.");
        if (encounter.PlayerMovementSlot is < 0 or >= OriginalStrategicMovement.PlayerArmyMovementCount
            || encounter.Slot is < 0 or >= OriginalStrategicMovement.PlayerDivisionTargetCount)
            throw new ArgumentOutOfRangeException(nameof(encounter));
        var patrol = strategic.TemporaryForceSlots.Single(slot => slot.Slot == encounter.Slot);
        var playerRecord = strategic.PlayerMovementSlots.Single(slot => slot.Slot == encounter.PlayerMovementSlot);
        if (!patrol.Active || !playerRecord.Active
            || OriginalStrategicTemporaryForceEncounter.Capture(strategic, State.Player,
                new(encounter.Slot, encounter.PlayerMovementSlot)) != encounter)
            throw new InvalidOperationException("Patrol encounter changed before resolution.");
    }

    private OriginalStrategicTemporaryForceSettlement SettlePatrol(
        OriginalStrategicTemporaryForceEncounter encounter,
        OriginalStrategicEncounterForces player,
        OriginalStrategicEncounterForces patrol,
        IOriginalStrategicRandom rewardRandom)
    {
        ArgumentNullException.ThrowIfNull(rewardRandom);
        player = new(Math.Max(0, player.Swordsmen), Math.Max(0, player.Halberdiers), Math.Max(0, player.Knights));
        patrol = new(Math.Max(0, patrol.Swordsmen), Math.Max(0, patrol.Halberdiers), Math.Max(0, patrol.Knights));
        var strategic = State.OriginalStrategicState!;
        var army = State.Player.ArmyAt(encounter.PlayerMovementSlot);
        var force = strategic.TemporaryForceSlots.Single(slot => slot.Slot == encounter.Slot);
        force.Swordsmen = patrol.Swordsmen;
        force.Halberdiers = patrol.Halberdiers;
        force.Knights = patrol.Knights;
        // 0x3B5D5-0x3B645 ends the contacted player's current route even
        // when neither side has been eliminated. 0x63EC0 truncates the live
        // floats toward zero before storing the destination integers.
        var playerRecord = strategic.PlayerMovementSlots.Single(slot =>
            slot.Slot == encounter.PlayerMovementSlot);
        playerRecord.WaypointCount = 0;
        playerRecord.WaypointIndex = 0;
        playerRecord.DestinationX = checked((int)playerRecord.CurrentX);
        playerRecord.DestinationY = checked((int)playerRecord.CurrentY);
        playerRecord.PathComplete = true;
        strategic.PlayerRouteInputActive = false;

        var variables = State.ConversationVariables;
        while (variables.Count <= 25) variables.Add(0);
        variables[5] = unchecked(variables[5] + 1);
        var cleared = patrol.Total == 0;
        var removed = false;
        var modal = false;
        var reward = 0;
        if (cleared)
        {
            army.Units[UnitType.Swordsmen] = player.Swordsmen;
            army.Units[UnitType.Halberdiers] = player.Halberdiers;
            army.Units[UnitType.Knights] = player.Knights;
            army.RecordOriginalStrategicEncounterResolution();
            // 0x3B690 draws before 0x3B6A5 overrides slot one's award.
            var drawnReward = checked(rewardRandom.Next(100) + 50);
            reward = encounter.Slot == 1 ? 40 : drawnReward;
            variables[17] = unchecked(variables[17] + reward);
            variables[24] = unchecked(variables[24] + 1);
            force.EncounterResultMarked = true;
            force.Active = false;
        }
        else if (player.Total == 0)
        {
            variables[25] = unchecked(variables[25] + 1);
            force.EncounterResultMarked = true;
            if (encounter.PlayerMovementSlot == strategic.EngagedPlayerMovementSlot)
                modal = true;
            else
            {
                army.ResetOriginalStrategicEncounterState();
                removed = OriginalStrategicMovement.RemovePlayerMovementRecord(
                    strategic, encounter.PlayerMovementSlot);
            }
        }
        return new(player, patrol, cleared, removed, modal, reward);
    }
}
