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

    /// <summary>
    /// Mirrors the mutation-bearing input routes from dispatcher
    /// <c>0x267A4-0x26B67</c>. Coordinates remain caller-owned outputs of the
    /// unrecovered event producer: selection offsets its point through the
    /// viewport, the control strip uses its absolute rectangles, and codes
    /// six/seven issue the mapped selected-unit destination order.
    /// </summary>
    public OriginalStrategicInteractiveEncounterInputRoute ApplyMappedInput(
        int inputCode,
        int localX,
        int localY,
        int horizontalOffset,
        int verticalOffset,
        int verticalSpan)
    {
        var route = RouteInputCode(inputCode);
        switch (route)
        {
            case OriginalStrategicInteractiveEncounterInputRoute.PlayerSelection:
                TryAppendMappedPlayerSelectionAt(localX, localY, horizontalOffset, verticalOffset);
                break;

            case OriginalStrategicInteractiveEncounterInputRoute.ControlStrip:
                switch (RouteControlStripHit(localX, localY, horizontalOffset, verticalSpan))
                {
                    case OriginalStrategicInteractiveEncounterControlStripRoute.UnitSelectionFallback:
                        TryAppendMappedPlayerSelectionAt(localX, localY, horizontalOffset, verticalOffset);
                        break;
                    case OriginalStrategicInteractiveEncounterControlStripRoute.UnresolvedFirstControl:
                        if (!_firstControlConfirmationArmed)
                            ArmMappedFirstControlConfirmation();
                        break;
                    case OriginalStrategicInteractiveEncounterControlStripRoute.SetControlCodeOneForSelectedRecords:
                        SetMappedControlCodeOneForSelection();
                        break;
                    case OriginalStrategicInteractiveEncounterControlStripRoute.SetControlCodeOneForLivingRecords:
                        SetMappedControlCodeOneForLivingUnits();
                        break;
                }
                break;

            case OriginalStrategicInteractiveEncounterInputRoute.DestinationOrder:
                ApplyDestinationOrder(localX, localY, horizontalOffset, verticalOffset, verticalSpan);
                break;
        }

        return route;
    }

    /// <summary>
    /// Advances the recovered outer ordering of resolver <c>0x26B88</c>:
    /// edge scrolling, input dispatch, the first-control confirmation branch,
    /// its tactical-suspension flag, then the strict 200-ms tactical gate.
    /// A false response to an already armed first control deliberately falls
    /// through to ordinary selection and leaves the suspension armed.
    /// </summary>
    public OriginalStrategicInteractiveEncounterFrameResult AdvanceMappedFrame(
        TimeSpan currentTime,
        int inputCode,
        int localX,
        int localY,
        OriginalStrategicInteractiveEncounterViewport viewport,
        int playerScoreModifier,
        int contactSideFilter,
        IOriginalStrategicEncounterRandom random,
        bool firstControlConfirmationAccepted = false)
    {
        ArgumentNullException.ThrowIfNull(viewport);
        ArgumentNullException.ThrowIfNull(random);

        var viewportScrolled = viewport.ApplyMappedEdgeScroll(localX, localY);
        var inputRoute = RouteInputCode(inputCode);
        var controlRoute = inputRoute == OriginalStrategicInteractiveEncounterInputRoute.ControlStrip
            ? RouteControlStripHit(localX, localY, viewport.HorizontalOffset, viewport.ViewportHeight)
            : OriginalStrategicInteractiveEncounterControlStripRoute.UnitSelectionFallback;

        if (controlRoute == OriginalStrategicInteractiveEncounterControlStripRoute.UnresolvedFirstControl
            && _firstControlConfirmationArmed)
        {
            if (firstControlConfirmationAccepted)
                return new(inputRoute, viewportScrolled, TacticalPassAdvanced: false, ResolverEnded: true);

            TryAppendMappedPlayerSelectionAt(
                localX, localY, viewport.HorizontalOffset, viewport.VerticalOffset);
        }
        else
        {
            ApplyMappedInput(inputCode, localX, localY, viewport.HorizontalOffset,
                viewport.VerticalOffset, viewport.ViewportHeight);
        }

        if (_firstControlConfirmationArmed || !Timing.TryBeginPass(currentTime))
            return new(inputRoute, viewportScrolled, TacticalPassAdvanced: false, ResolverEnded: false);

        AdvanceMappedTacticalPass(viewport.ContentWidth, viewport.ContentHeight,
            playerScoreModifier, contactSideFilter, random);
        return new(inputRoute, viewportScrolled, TacticalPassAdvanced: true, ResolverEnded: false);
    }

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

    /// <summary>
    /// Mirrors selection's viewport-adjusted selector at
    /// <c>0x26999-0x26A77</c>. A selector miss or an ineligible player record
    /// leaves the append-only selection list unchanged.
    /// </summary>
    public bool TryAppendMappedPlayerSelectionAt(
        int localX,
        int localY,
        int horizontalOffset,
        int verticalOffset)
    {
        var oneBased = OriginalStrategicInteractiveEncounterGeometry.FindFirstContainingOneBased(
            RenderRectangles(),
            checked(localX + horizontalOffset),
            checked(localY + verticalOffset));
        return oneBased != 0 && TryAppendPlayerSelection(checked(oneBased - 1));
    }

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

    /// <summary>
    /// Advances one already-authorized tactical pass from <c>0x26C81-0x27E16</c>.
    /// It builds one rectangle snapshot, visits records in table order, advances
    /// death state, applies the state-<c>0x28</c> zero-offset neighbor probe and
    /// due contact, then enters state zero only when the resulting state is
    /// zero. The caller owns the monotonic timing gate and input dispatch; this
    /// avoids reproducing the original processor-dependent loop frequency.
    /// </summary>
    public void AdvanceMappedTacticalPass(
        int contentWidth,
        int contentHeight,
        int playerScoreModifier,
        int contactSideFilter,
        IOriginalStrategicEncounterRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if (playerScoreModifier < 0)
            throw new ArgumentOutOfRangeException(nameof(playerScoreModifier));

        var rectangles = RenderRectangles();
        for (var index = 0; index < _units.Count; index++)
        {
            var unit = _units[index];
            if (unit.StateCode == OriginalStrategicInteractiveEncounterCombat.DeathAnimationStateCode)
            {
                AdvanceMappedDeathAnimationState(unit);
                continue;
            }

            if (unit.StateCode == OriginalStrategicInteractiveEncounterCombat.ContactStateCode)
            {
                var targetUnitIndex = unit.TargetUnitIndex;
                var probeResult = OriginalStrategicInteractiveEncounterGeometry.ProbeMappedNeighborContact(
                    _units, rectangles, index, 0, 0, contentWidth, contentHeight, ref targetUnitIndex);
                unit.TargetUnitIndex = targetUnitIndex;
                var probe = OriginalStrategicInteractiveEncounterCombat.ApplyMappedContactProbeResult(
                    unit, probeResult);
                if (probe.ContactDue)
                    OriginalStrategicInteractiveEncounterCombat.ApplyMappedResolvedContact(
                        _units, index, playerScoreModifier, contactSideFilter, random);
            }

            if (unit.StateCode == 0)
                OriginalStrategicInteractiveEncounter.AdvanceMappedStateZero(
                    _units, rectangles, index, contentWidth, contentHeight,
                    _playerLaneCount, _enemyLaneCount);
        }
    }

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

    private void AdvanceMappedDeathAnimationState(OriginalStrategicInteractiveEncounterUnit unit)
    {
        if (unit.PhaseCounter == 4)
            return;
        unit.PhaseCounter = (unit.PhaseCounter + 1) % 5;
        if (unit.PhaseCounter != 4)
            return;

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
