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
    IReadOnlyList<OriginalStrategicPlayerEnemyEncounter> Encounters,
    OriginalStrategicSchedulerResult? SchedulerPass);

/// <summary>
/// The three typed force counters staged for one side of the original
/// strategic encounter resolver. They deliberately remain distinct from an
/// aggregate strength so the later resolver boundary can return each
/// category independently.
/// </summary>
public readonly record struct OriginalStrategicEncounterForces(
    int Swordsmen,
    int Halberdiers,
    int Knights)
{
    public int Total => checked(Swordsmen + Halberdiers + Knights);

    internal void Validate(string parameterName)
    {
        if (Swordsmen < 0 || Halberdiers < 0 || Knights < 0)
            throw new ArgumentOutOfRangeException(parameterName);
    }
}

/// <summary>
/// The exact force staging performed immediately before resolver <c>0x258FC</c>.
/// Player reserves are deliberately held outside the resolver and added back
/// after it returns; hostile values are resolver-owned write-back counters.
/// </summary>
public readonly record struct OriginalStrategicEncounterPreparation(
    OriginalStrategicEncounterForces PlayerResolverForces,
    OriginalStrategicEncounterForces PlayerReservedForces,
    OriginalStrategicEncounterForces EnemyResolverForces);

public static class OriginalStrategicEncounterStaging
{
    public const int ResolverForceThreshold = 60;
    public const int PlayerLargeCategoryThreshold = 20;

    /// <summary>
    /// Maps wrapper <c>0x35924</c>'s pre-resolver counter reductions. Its
    /// original integer divisions and strict <c>reduction + 1 &lt; count</c>
    /// tests are retained rather than treating either side as an aggregate.
    /// </summary>
    public static OriginalStrategicEncounterPreparation Prepare(
        OriginalStrategicEncounterForces playerForces,
        OriginalStrategicEncounterForces enemyForces)
    {
        playerForces.Validate(nameof(playerForces));
        enemyForces.Validate(nameof(enemyForces));

        var enemyReduction = enemyForces.Total > ResolverForceThreshold
            ? (enemyForces.Total - ResolverForceThreshold) / 3
            : 0;
        var enemyResolver = ReduceEveryCategory(enemyForces, enemyReduction);

        var playerDivisor = CountLargePlayerCategories(playerForces);
        var playerReduction = playerForces.Total > ResolverForceThreshold && playerDivisor > 0
            ? (playerForces.Total - ResolverForceThreshold) / playerDivisor
            : 0;
        var playerResolver = ReduceEveryCategory(playerForces, playerReduction);
        var playerReserved = new OriginalStrategicEncounterForces(
            checked(playerForces.Swordsmen - playerResolver.Swordsmen),
            checked(playerForces.Halberdiers - playerResolver.Halberdiers),
            checked(playerForces.Knights - playerResolver.Knights));
        return new(playerResolver, playerReserved, enemyResolver);
    }

    private static int CountLargePlayerCategories(OriginalStrategicEncounterForces forces) =>
        (forces.Swordsmen >= PlayerLargeCategoryThreshold ? 1 : 0)
        + (forces.Halberdiers >= PlayerLargeCategoryThreshold ? 1 : 0)
        + (forces.Knights >= PlayerLargeCategoryThreshold ? 1 : 0);

    private static OriginalStrategicEncounterForces ReduceEveryCategory(
        OriginalStrategicEncounterForces forces,
        int reduction) => new(
            ReduceCategory(forces.Swordsmen, reduction),
            ReduceCategory(forces.Halberdiers, reduction),
            ReduceCategory(forces.Knights, reduction));

    private static int ReduceCategory(int count, int reduction) => reduction + 1 < count
        ? checked(count - reduction)
        : count;
}

/// <summary>
/// Immutable capture of the original six-counter player/enemy encounter
/// boundary. This is a handoff payload only: it does not select a tactical
/// engine or infer any casualty/result rule.
/// </summary>
public sealed record OriginalStrategicPlayerEnemyEncounter(
    int PlayerMovementSlot,
    int EnemyMovementSlot,
    OriginalStrategicEncounterForces PlayerForces,
    OriginalStrategicEncounterForces EnemyForces)
{
    public OriginalStrategicEncounterPreparation Preparation =>
        OriginalStrategicEncounterStaging.Prepare(PlayerForces, EnemyForces);

    internal static OriginalStrategicPlayerEnemyEncounter Capture(
        OriginalStrategicCampaignState strategic,
        Player player,
        int playerMovementSlot,
        int enemyMovementSlot)
    {
        ArgumentNullException.ThrowIfNull(strategic);
        ArgumentNullException.ThrowIfNull(player);
        strategic.Validate();
        if (playerMovementSlot is < 0 or >= OriginalStrategicMovement.PlayerArmyMovementCount)
            throw new ArgumentOutOfRangeException(nameof(playerMovementSlot));
        if (enemyMovementSlot is < 0 or >= OriginalStrategicMovement.SlotCount)
            throw new ArgumentOutOfRangeException(nameof(enemyMovementSlot));

        var playerRecord = strategic.PlayerMovementSlots.Single(slot => slot.Slot == playerMovementSlot);
        var enemyRecord = strategic.MovementSlots.Single(slot => slot.Slot == enemyMovementSlot);
        if (!playerRecord.Active || !enemyRecord.Active)
            throw new InvalidOperationException("Strategic encounter requires active player and enemy records.");

        player.EnsureArmyRoster();
        var army = player.ArmyAt(playerMovementSlot);
        var playerForces = new OriginalStrategicEncounterForces(
            army.Units[UnitType.Swordsmen],
            army.Units[UnitType.Halberdiers],
            army.Units[UnitType.Knights]);
        var enemyForces = new OriginalStrategicEncounterForces(
            enemyRecord.Swordsmen,
            enemyRecord.Halberdiers,
            enemyRecord.Knights);
        playerForces.Validate(nameof(player));
        enemyForces.Validate(nameof(strategic));
        return new(playerMovementSlot, enemyMovementSlot, playerForces, enemyForces);
    }
}

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
        var encounters = playerPass.Contacts.Select(contact =>
            OriginalStrategicPlayerEnemyEncounter.Capture(
                strategic, State.Player, contact.PlayerSlot, contact.EnemySlot)).ToArray();
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
        return new(report, playerPass, encounters, schedulerPass);
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
