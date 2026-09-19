namespace Conqueror.Core;

/// <summary>
/// Stateful clean-room boundary for the recovered interactive encounter setup
/// and player-control records. It deliberately does not invent casualty,
/// target, or movement resolution while those loop states remain unmapped.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterSession
{
    private readonly List<OriginalStrategicInteractiveEncounterUnit> _units;
    private readonly List<int> _selectedUnitIndices = [];
    private int _playerLaneCount;
    private int _enemyLaneCount;
    private bool _firstControlConfirmationArmed;

    private OriginalStrategicInteractiveEncounterSession(
        List<OriginalStrategicInteractiveEncounterUnit> units,
        TimeSpan initialTime)
    {
        _units = units;
        _playerLaneCount = units.Count(unit => unit.Side == OriginalStrategicInteractiveEncounterSide.Player);
        _enemyLaneCount = units.Count(unit => unit.Side == OriginalStrategicInteractiveEncounterSide.Enemy);
        Timing = new OriginalStrategicInteractiveEncounterTiming(initialTime);
    }

    public IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> Units => _units;
    public IReadOnlyList<int> SelectedUnitIndices => _selectedUnitIndices;
    public OriginalStrategicInteractiveEncounterTiming Timing { get; }
    public int PlayerLaneCount => _playerLaneCount;
    public int EnemyLaneCount => _enemyLaneCount;

    /// <summary>
    /// Mirrors the inverted timing gate <c>19C7C</c>: the first control-strip
    /// button sets that word to zero while its confirmation state is armed,
    /// so regular tactical advancement is skipped even if input dispatch
    /// continues to run.
    /// </summary>
    public bool IsTacticalAdvancementSuspendedForFirstControlConfirmation =>
        _firstControlConfirmationArmed;

    public OriginalStrategicInteractiveEncounterInputRoute RouteInputCode(int inputCode) =>
        OriginalStrategicInteractiveEncounter.RouteMappedInputCode(inputCode);

    public OriginalStrategicInteractiveEncounterControlStripRoute RouteControlStripHit(
        int localX,
        int localY,
        int horizontalOffset,
        int verticalSpan) =>
        OriginalStrategicInteractiveEncounter.RouteMappedControlStripHit(
            OriginalStrategicInteractiveEncounterGeometry.FindFirstContainingOneBased(
                OriginalStrategicInteractiveEncounterGeometry.CreateMappedControlStripRectangles(
                    horizontalOffset, verticalSpan),
                localX,
                localY));

    /// <summary>
    /// Mirrors the first control-strip hit when global <c>19C8C</c> is zero:
    /// the source marks its confirmation state and renders the pending path.
    /// The user-facing button and dialog text remain deliberately unnamed.
    /// </summary>
    public void ArmMappedFirstControlConfirmation()
    {
        if (_firstControlConfirmationArmed)
            throw new InvalidOperationException("Mapped first-control confirmation is already armed.");
        _firstControlConfirmationArmed = true;
    }

    /// <summary>
    /// Mirrors the already-armed first-control branch at
    /// <c>0x26832-0x2685B</c>. A true dialog result exits the resolver;
    /// a false result falls through to ordinary unit selection and does not
    /// clear the confirmation state or resume tactical advancement.
    /// </summary>
    public bool ResolveMappedFirstControlConfirmation(bool accepted)
    {
        if (!_firstControlConfirmationArmed)
            throw new InvalidOperationException("Mapped first-control confirmation is not armed.");
        return accepted;
    }

    /// <summary>
    /// Materializes all six counters and applies the original menu layout.
    /// Code 3 alone consumes one raw encounter draw; the remaining choices do
    /// not consume it.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterSession Create(
        OriginalStrategicEncounterForces playerForces,
        OriginalStrategicEncounterForces enemyForces,
        int menuCode,
        int horizontalSpan,
        int verticalSpan,
        TimeSpan initialTime,
        IOriginalStrategicEncounterRandom? random = null)
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(playerForces, enemyForces).ToList();
        if (menuCode is >= 0 and <= 2)
            OriginalStrategicInteractiveEncounter.ApplyMappedMenuFormation(units, menuCode, verticalSpan);
        else if (menuCode == 3)
            OriginalStrategicInteractiveEncounter.ApplyMappedMenuCodeThreeFormation(
                units, horizontalSpan, verticalSpan,
                random ?? throw new ArgumentNullException(nameof(random)));
        else
            throw new ArgumentOutOfRangeException(nameof(menuCode));
        return new(units, initialTime);
    }

    public bool TryAppendPlayerSelection(int unitIndex) =>
        OriginalStrategicInteractiveEncounter.TryAppendMappedPlayerSelection(
            _units, _selectedUnitIndices, unitIndex);

    public void SetMappedControlCodeOneForSelection() =>
        OriginalStrategicInteractiveEncounter.SetMappedControlCodeOneForSelectedRecords(
            _units, _selectedUnitIndices);

    public void SetMappedControlCodeOneForLivingUnits() =>
        OriginalStrategicInteractiveEncounter.SetMappedControlCodeOneForLivingRecords(_units);

    /// <summary>
    /// Applies only a contact whose source target acquisition has already
    /// resolved. It does not synthesize a target or advance tactical phases.
    /// </summary>
    public OriginalStrategicInteractiveEncounterContactResult ApplyResolvedContact(
        int attackerIndex,
        int playerScoreModifier,
        int contactSideFilter,
        IOriginalStrategicEncounterRandom random) =>
        OriginalStrategicInteractiveEncounterCombat.ApplyMappedResolvedContact(
            _units, attackerIndex, playerScoreModifier, contactSideFilter, random);

    public OriginalStrategicInteractiveEncounterContactProbeResult ApplyContactProbeResult(
        int attackerIndex,
        int probeResult)
    {
        if ((uint)attackerIndex >= (uint)_units.Count)
            throw new ArgumentOutOfRangeException(nameof(attackerIndex));
        return OriginalStrategicInteractiveEncounterCombat.ApplyMappedContactProbeResult(
            _units[attackerIndex], probeResult);
    }

    /// <summary>
    /// Runs the exact rectangle-corner contact primitive used by the tactical
    /// loop. This does not choose which offset to request or advance a unit's
    /// state; those remain responsibilities of the still-unmapped loop shell.
    /// </summary>
    public int ProbeMappedNeighborContact(
        int sourceUnitIndex,
        int deltaX,
        int deltaY,
        int contentWidth,
        int contentHeight,
        ref int targetUnitIndex) =>
        OriginalStrategicInteractiveEncounterGeometry.ProbeMappedNeighborContact(
            _units, sourceUnitIndex, deltaX, deltaY, contentWidth, contentHeight,
            ref targetUnitIndex);

    /// <summary>
    /// Advances one source-compatible turn toward this unit's already-stored
    /// target. Target acquisition and movement/collision probes remain caller
    /// responsibilities.
    /// </summary>
    public bool AdvanceTargetHeading(int unitIndex) =>
        OriginalStrategicInteractiveEncounter.AdvanceMappedTargetHeading(_units, unitIndex);

    /// <summary>
    /// Runs the state-zero automatic destination assignment against the live
    /// lane counters, which the session decrements only at the mapped
    /// death-animation completion boundary.
    /// </summary>
    public bool TryAssignAutomaticDestination(int unitIndex) =>
        OriginalStrategicInteractiveEncounter.TryAssignMappedAutomaticDestination(
            _units, unitIndex, _playerLaneCount, _enemyLaneCount);

    /// <summary>
    /// Advances one state-zero record with a fresh caller-visible rectangle
    /// snapshot. A complete tactical pass must instead retain one snapshot
    /// for every unit, matching <c>0x26B88</c>'s pre-pass construction.
    /// </summary>
    public bool AdvanceStateZero(int unitIndex, int contentWidth, int contentHeight) =>
        OriginalStrategicInteractiveEncounter.AdvanceMappedStateZero(
            _units, RenderRectangles(), unitIndex, contentWidth, contentHeight,
            _playerLaneCount, _enemyLaneCount);

    public void ApplyDestinationOrder(
        int localX,
        int localY,
        int horizontalOffset,
        int verticalOffset,
        int verticalSpan) =>
        OriginalStrategicInteractiveEncounter.ApplyMappedDestinationOrder(
            _units, _selectedUnitIndices, localX, localY,
            horizontalOffset, verticalOffset, verticalSpan);

    public IReadOnlyList<OriginalStrategicInteractiveEncounterRectangle> RenderRectangles() =>
        _units.Select(OriginalStrategicInteractiveEncounterGeometry.RenderRectangleFor).ToArray();

    /// <summary>
    /// Applies only the exact post-animation record mutation. Callers must
    /// establish the original death-state and phase transition first.
    /// </summary>
    public void CompleteDeathAnimation(int unitIndex)
    {
        if ((uint)unitIndex >= (uint)_units.Count)
            throw new ArgumentOutOfRangeException(nameof(unitIndex));
        var unit = _units[unitIndex];
        var wasLiving = unit.RemainingStrength > 0;
        OriginalStrategicInteractiveEncounter.CompleteMappedDeathAnimation(unit);
        if (!wasLiving)
            return;
        if (unit.Side == OriginalStrategicInteractiveEncounterSide.Player)
            _playerLaneCount--;
        else
            _enemyLaneCount--;
    }
}
