namespace Conqueror.Core;

/// <summary>
/// Explicit fixed-update input for the recovered strategic-map scheduler.
/// The three transient targets mirror the original division-force target
/// handles; they are deliberately supplied at the application boundary rather
/// than guessed from the incompatible dated campaign map. A modal blocks only
/// the hostile scheduler, after the report and player pass, as at `0x3C290`.
/// Player-contact handoff has its own active state, matching `0x39428`'s
/// independent `ADC0` guard.
/// </summary>
public sealed record OriginalStrategicCampaignPassInput(
    int GlobalTargetPerson,
    int GlobalOriginProperty,
    IReadOnlyList<OriginalStrategicPlayerTarget> DivisionTargets,
    int SpecialPropertyHouseholdCount,
    bool PlayerEncounterHandoffActive = false,
    bool SchedulerBlockedByModal = false);

/// <summary>
/// Typed output from one recovered strategic-map pass. A spy report is taken
/// before either player or hostile records advance, matching the executable
/// caller ordering.
/// </summary>
public sealed record OriginalStrategicCampaignPassResult(
    StrategicSpyReport? SpyReport,
    OriginalStrategicPlayerPassResult PlayerPass,
    OriginalStrategicSchedulerResult? SchedulerPass);

public sealed partial class Campaign
{
    /// <summary>
    /// Creates the exact six-record strategic bootstrap for a newly created
    /// campaign. Existing saves deliberately use the schema-one migration
    /// path instead, because they have no recoverable starting-route choice.
    /// </summary>
    public OriginalStrategicCampaignState InitializeOriginalStrategicNewGame()
    {
        if (State.OriginalStrategicState is not null)
            throw new InvalidOperationException("Campaign already has original strategic state.");
        if (State.EnemyMovements.Count != 0)
            throw new InvalidOperationException(
                "A dated enemy movement roster must be settled before strategic initialization.");

        var selector = _random.Next(OriginalStrategicMovement.StartingRouteCount);
        var strategic = OriginalStrategicCampaignState.CreateForNewGame(State.Date, selector);
        State.OriginalStrategicState = strategic;
        return strategic;
    }

    /// <summary>
    /// Advances the imported strategic movement records once. This is an
    /// intentionally explicit fixed-update boundary: callers choose their
    /// stable simulation cadence instead of inheriting the original main
    /// loop's processor-dependent pass rate.
    /// </summary>
    public OriginalStrategicCampaignPassResult AdvanceOriginalStrategicPass(
        OriginalStrategicCampaignPassInput input,
        IOriginalStrategicRandom random)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(random);
        if (_originalStrategicResources is null)
            throw new InvalidOperationException("Original strategic resources are not configured.");
        if (State.OriginalStrategicState is not { } strategic)
            throw new InvalidOperationException("Campaign has no original strategic movement state.");
        if (input.DivisionTargets is null
            || input.DivisionTargets.Count != OriginalStrategicMovement.PlayerDivisionTargetCount)
            throw new ArgumentException(
                $"Strategic pass requires exactly {OriginalStrategicMovement.PlayerDivisionTargetCount} division targets.",
                nameof(input));

        strategic.Validate();
        var report = CaptureOriginalStrategicSpyReport(strategic);
        var playerPass = OriginalStrategicMovement.AdvancePlayerPass(
            strategic, _originalStrategicResources, input.DivisionTargets,
            input.PlayerEncounterHandoffActive);
        var pursuitTargets = PlayerArmyPursuitTargets(strategic);
        var engaged = strategic.PlayerMovementSlots.Single(slot =>
            slot.Slot == strategic.EngagedPlayerMovementSlot);
        OriginalStrategicSchedulerResult? schedulerPass = null;
        if (!input.SchedulerBlockedByModal)
            schedulerPass = OriginalStrategicMovement.AdvanceSchedulerPass(
                strategic,
                _originalStrategicResources,
                new OriginalStrategicSchedulerInput(
                    input.GlobalTargetPerson,
                    input.GlobalOriginProperty,
                    strategic.EngagedPlayerMovementSlot,
                    new StrategicPoint(Truncate(engaged.CurrentX), Truncate(engaged.CurrentY)),
                    pursuitTargets,
                    input.SpecialPropertyHouseholdCount),
                random);
        return new(report, playerPass, schedulerPass);
    }

    private StrategicSpyReport? CaptureOriginalStrategicSpyReport(
        OriginalStrategicCampaignState strategic)
    {
        if (State.Player.ActiveSpies <= 0) return null;
        var movement = strategic.MovementSlots.OrderBy(slot => slot.Slot)
            .FirstOrDefault(slot => slot.Active);
        if (movement is null) return null;

        State.Player.ActiveSpies = 0;
        var origin = OriginalStrategicMovement.PropertyIdentities[movement.OriginProperty];
        var report = new StrategicSpyReport(
            State.Date,
            movement.Slot,
            -1,
            movement.Swordsmen,
            movement.Halberdiers,
            movement.Knights,
            origin.Name);
        State.LatestSpyReport = report;
        Log($"Spy report from {origin.Name}: {movement.Swordsmen} swordsmen, " +
            $"{movement.Halberdiers} halberdiers, and {movement.Knights} knights are on the march.");
        return report;
    }

    private IReadOnlyList<OriginalStrategicPursuitTarget> PlayerArmyPursuitTargets(
        OriginalStrategicCampaignState strategic)
    {
        var player = State.Player;
        player.EnsureArmyRoster();
        return Enumerable.Range(0, OriginalStrategicMovement.PlayerArmyMovementCount)
            .Select(slotIndex =>
            {
                var slot = strategic.PlayerMovementSlots.Single(slot => slot.Slot == slotIndex);
                var army = player.ArmyAt(slotIndex);
                return new OriginalStrategicPursuitTarget(
                    slot.Active && player.ArmyIsFielded(slotIndex),
                    slot.CurrentX,
                    slot.CurrentY,
                    slot.GridX,
                    slot.GridY,
                    army.Units[UnitType.Swordsmen],
                    army.Units[UnitType.Halberdiers],
                    army.Units[UnitType.Knights]);
            })
            .ToArray();
    }

    private static int Truncate(float value) => checked((int)value);
}
