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

    private OriginalStrategicInteractiveEncounterSession(
        List<OriginalStrategicInteractiveEncounterUnit> units,
        TimeSpan initialTime)
    {
        _units = units;
        Timing = new OriginalStrategicInteractiveEncounterTiming(initialTime);
    }

    public IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> Units => _units;
    public IReadOnlyList<int> SelectedUnitIndices => _selectedUnitIndices;
    public OriginalStrategicInteractiveEncounterTiming Timing { get; }

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
        OriginalStrategicInteractiveEncounter.CompleteMappedDeathAnimation(_units[unitIndex]);
    }
}
