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
    private bool _firstControlActivated;
    private bool _tacticalAdvancementPaused = true;
    private bool _hoveredUnitInPreviousFrame;

    private OriginalStrategicInteractiveEncounterSession(
        List<OriginalStrategicInteractiveEncounterUnit> units,
        TimeSpan initialTime,
        TimeSpan? tacticalCadence)
    {
        _units = units;
        _playerLaneCount = units.Count(unit => unit.Side == OriginalStrategicInteractiveEncounterSide.Player);
        _enemyLaneCount = units.Count(unit => unit.Side == OriginalStrategicInteractiveEncounterSide.Enemy);
        Timing = new OriginalStrategicInteractiveEncounterTiming(initialTime, tacticalCadence);
    }

    public IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> Units => _units;
    public IReadOnlyList<int> SelectedUnitIndices => _selectedUnitIndices;
    public OriginalStrategicInteractiveEncounterTiming Timing { get; }
    public int PlayerLaneCount => _playerLaneCount;
    public int EnemyLaneCount => _enemyLaneCount;
    public OriginalStrategicInteractiveEncounterOutcome Outcome { get; private set; }

    /// <summary>
    /// Source <c>19C7C</c> starts nonzero and skips tactical passes. The first
    /// control sets it to zero and enables them; later hits enter retreat confirmation.
    /// </summary>
    public bool IsMappedTacticalAdvancementEnabled => !_tacticalAdvancementPaused;

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
        int verticalSpan,
        int controlStripMargin = 0)
    {
        var route = RouteInputCode(inputCode);
        switch (route)
        {
            case OriginalStrategicInteractiveEncounterInputRoute.PlayerSelection:
                TryAppendMappedPlayerSelectionAt(localX, localY, horizontalOffset, verticalOffset);
                break;

            case OriginalStrategicInteractiveEncounterInputRoute.ControlStrip:
                switch (RouteControlStripHit(localX, localY, controlStripMargin, verticalSpan))
                {
                    case OriginalStrategicInteractiveEncounterControlStripRoute.UnitSelectionFallback:
                        TryAppendMappedPlayerSelectionAt(localX, localY, horizontalOffset, verticalOffset);
                        break;
                    case OriginalStrategicInteractiveEncounterControlStripRoute.RetreatConfirmation:
                        if (!_firstControlActivated)
                            ActivateMappedFirstControl();
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
    /// edge scrolling, input dispatch, the first-control activation or later
    /// retreat-confirmation branch, then the configured stable tactical gate.
    /// A false response to an already activated first control deliberately
    /// falls through to ordinary selection and leaves tactical passes enabled.
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

        if (Outcome != OriginalStrategicInteractiveEncounterOutcome.InProgress)
            return new(OriginalStrategicInteractiveEncounterInputRoute.Ignored,
                ViewportScrolled: false, TacticalPassAdvanced: false, ResolverEnded: true)
            {
                Outcome = Outcome,
            };

        var viewportScrolled = viewport.ApplyMappedEdgeScroll(localX, localY);
        var hover = CaptureMappedHoverPresentation(
            localX, localY, viewport.HorizontalOffset, viewport.VerticalOffset);
        var inputRoute = RouteInputCode(inputCode);
        var controlRoute = inputRoute == OriginalStrategicInteractiveEncounterInputRoute.ControlStrip
            ? RouteControlStripHit(localX, localY, viewport.ControlStripMargin, viewport.ViewportHeight)
            : OriginalStrategicInteractiveEncounterControlStripRoute.UnitSelectionFallback;

        if (controlRoute == OriginalStrategicInteractiveEncounterControlStripRoute.RetreatConfirmation
            && _firstControlActivated)
        {
            if (firstControlConfirmationAccepted)
                return FinishMappedResolver(
                    OriginalStrategicInteractiveEncounterOutcome.PlayerWithdrew,
                    inputRoute, viewportScrolled, hover);

            TryAppendMappedPlayerSelectionAt(
                localX, localY, viewport.HorizontalOffset, viewport.VerticalOffset);
        }
        else
        {
            ApplyMappedInput(inputCode, localX, localY, viewport.HorizontalOffset,
                viewport.VerticalOffset, viewport.ViewportHeight, viewport.ControlStripMargin);
        }

        var terminalOutcome = MappedLaneTerminalOutcome();
        if (terminalOutcome != OriginalStrategicInteractiveEncounterOutcome.InProgress)
            return FinishMappedResolver(terminalOutcome, inputRoute, viewportScrolled, hover);

        if (_tacticalAdvancementPaused || !Timing.TryBeginPass(currentTime))
            return new(inputRoute, viewportScrolled, TacticalPassAdvanced: false, ResolverEnded: false)
            {
                HoverPresentation = hover,
            };

        AdvanceMappedTacticalPass(viewport.ContentWidth, viewport.ContentHeight,
            playerScoreModifier, contactSideFilter, random);
        return new(inputRoute, viewportScrolled, TacticalPassAdvanced: true, ResolverEnded: false)
        {
            HoverPresentation = hover,
        };
    }

    public OriginalStrategicInteractiveEncounterControlStripRoute RouteControlStripHit(
        int localX,
        int localY,
        int controlStripMargin,
        int verticalSpan) =>
        OriginalStrategicInteractiveEncounter.RouteMappedControlStripHit(
            OriginalStrategicInteractiveEncounterGeometry.FindFirstContainingOneBased(
                OriginalStrategicInteractiveEncounterGeometry.CreateMappedControlStripRectangles(
                    controlStripMargin, verticalSpan),
                localX,
                localY));

    /// <summary>
    /// Mirrors the first control hit when <c>19C8C</c> is zero: it records the
    /// activated state and clears <c>19C7C</c>, enabling tactical passes.
    /// </summary>
    public void ActivateMappedFirstControl()
    {
        if (_firstControlActivated)
            throw new InvalidOperationException("Mapped first control is already activated.");
        _firstControlActivated = true;
        _tacticalAdvancementPaused = false;
    }

    /// <summary>
    /// Mirrors the already-activated retreat-control branch at
    /// <c>0x26832-0x2685B</c>. A true dialog result exits the resolver;
    /// a false result falls through to ordinary unit selection and does not
    /// clear the activated state or pause tactical advancement.
    /// </summary>
    public bool ResolveMappedRetreatConfirmation(bool accepted)
    {
        if (!_firstControlActivated)
            throw new InvalidOperationException("Mapped first control is not activated.");
        return accepted;
    }

    /// <summary>
    /// Mirrors the source status branch before input dispatch at
    /// <c>0x265CC-0x26787</c>. The rectangle selector receives the scroll-
    /// adjusted point. A player record formats its current strength; an enemy
    /// record prints the foe label. The first blank pass after either hover
    /// instead compares the live enemy and player lane counts, then clears
    /// the source's one-pass hover flag.
    /// </summary>
    public OriginalStrategicInteractiveEncounterHoverPresentation CaptureMappedHoverPresentation(
        int localX,
        int localY,
        int horizontalOffset,
        int verticalOffset)
    {
        var oneBased = OriginalStrategicInteractiveEncounterGeometry.FindFirstContainingOneBased(
            RenderRectangles(), checked(localX + horizontalOffset), checked(localY + verticalOffset));
        if (oneBased != 0)
        {
            _hoveredUnitInPreviousFrame = true;
            var unit = _units[checked(oneBased - 1)];
            return unit.Side == OriginalStrategicInteractiveEncounterSide.Player
                ? new(OriginalStrategicInteractiveEncounterHoverKind.PlayerStrength, unit.RemainingStrength)
                : new(OriginalStrategicInteractiveEncounterHoverKind.Foe);
        }

        if (!_hoveredUnitInPreviousFrame)
            return default;

        _hoveredUnitInPreviousFrame = false;
        return _enemyLaneCount <= _playerLaneCount
            ? new(OriginalStrategicInteractiveEncounterHoverKind.Winning)
            : new(OriginalStrategicInteractiveEncounterHoverKind.Losing);
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
        IOriginalStrategicEncounterRandom? random = null,
        TimeSpan? tacticalCadence = null)
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
        return new(units, initialTime, tacticalCadence);
    }

    public bool TryAppendPlayerSelection(int unitIndex) =>
        OriginalStrategicInteractiveEncounter.TryAppendMappedPlayerSelection(
            _units, _selectedUnitIndices, unitIndex);

    /// <summary>
    /// Appends the source-selected living player category to the existing
    /// selection list. This preserves the tactical loop's authored table
    /// order rather than sorting by screen position.
    /// </summary>
    public int AppendMappedPlayerCategorySelection(
        OriginalStrategicInteractiveEncounterCategory category) =>
        OriginalStrategicInteractiveEncounter.AppendMappedPlayerCategorySelection(
            _units, _selectedUnitIndices, category);

    /// <summary>
    /// Applies only the raw category-selection key routes recovered from
    /// <c>0x27ED2-0x283CC</c>. Unknown raw codes deliberately do nothing;
    /// the operating-system/input-library event producer remains host-owned.
    /// </summary>
    public int AppendMappedPlayerCategorySelectionForInputCode(int rawInputCode)
    {
        var category = OriginalStrategicInteractiveEncounter
            .RouteMappedCategorySelectionInputCode(rawInputCode);
        return category is { } mappedCategory
            ? AppendMappedPlayerCategorySelection(mappedCategory)
            : 0;
    }

    /// <summary>
    /// Applies the two raw <c>0x27ED2</c> branches that write control code one:
    /// <c>0x61</c> affects the selected list and <c>0x41</c> all living records.
    /// </summary>
    public bool ApplyMappedRawControlCodeOneInput(int rawInputCode)
    {
        switch (rawInputCode)
        {
            case 0x61:
                SetMappedControlCodeOneForSelection();
                return true;
            case 0x41:
                SetMappedControlCodeOneForLivingUnits();
                return true;
            default:
                return false;
        }
    }

    /// <summary>Mirrors raw <c>0x50/0x70</c>: after activation, toggles <c>19C7C</c>.</summary>
    public bool ToggleMappedTacticalAdvancementForInputCode(int rawInputCode)
    {
        if (!_firstControlActivated || (rawInputCode is not 0x50 and not 0x70))
            return false;
        _tacticalAdvancementPaused = !_tacticalAdvancementPaused;
        return true;
    }

    /// <summary>Maps raw <c>0x12D/0x16B</c> resolver exits to the caller's withdrawal result.</summary>
    public bool ResolveMappedRawResolverExit(int rawInputCode)
    {
        if (Outcome != OriginalStrategicInteractiveEncounterOutcome.InProgress
            || (rawInputCode is not 0x12D and not 0x16B))
            return false;
        Outcome = OriginalStrategicInteractiveEncounterOutcome.PlayerWithdrew;
        return true;
    }

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

    private OriginalStrategicInteractiveEncounterOutcome MappedLaneTerminalOutcome()
    {
        // 0x2808E tests 19C94 first, then 0x283E2 tests 19C70. Counts are
        // maintained as non-negative lane totals, so equality is intentional.
        if (_enemyLaneCount == 0)
            return OriginalStrategicInteractiveEncounterOutcome.EnemyDefeated;
        if (_playerLaneCount == 0)
            return OriginalStrategicInteractiveEncounterOutcome.PlayerDefeated;
        return OriginalStrategicInteractiveEncounterOutcome.InProgress;
    }

    private OriginalStrategicInteractiveEncounterFrameResult FinishMappedResolver(
        OriginalStrategicInteractiveEncounterOutcome outcome,
        OriginalStrategicInteractiveEncounterInputRoute inputRoute,
        bool viewportScrolled,
        OriginalStrategicInteractiveEncounterHoverPresentation hover)
    {
        Outcome = outcome;
        return new(inputRoute, viewportScrolled, TacticalPassAdvanced: false, ResolverEnded: true)
        {
            HoverPresentation = hover,
            Outcome = outcome,
        };
    }
}
