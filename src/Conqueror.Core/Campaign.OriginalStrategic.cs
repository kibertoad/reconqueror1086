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
    IReadOnlyList<OriginalStrategicPlayerTarget> DivisionTargets,
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

/// <summary>
/// Raw random-source boundary for the original automatic encounter fallback.
/// The executable takes the signed remainder of one raw draw per player
/// category, rather than asking for a directly bounded gameplay roll.
/// </summary>
public interface IOriginalStrategicEncounterRandom
{
    int NextRaw();
}

/// <summary>
/// Result of resolver <c>0x258FC</c>'s non-interactive fallback. The player
/// totals include the staging reserves restored by wrapper <c>0x35924</c>.
/// </summary>
public readonly record struct OriginalStrategicAutomaticEncounterResult(
    bool PlayerWon,
    OriginalStrategicEncounterForces PlayerResolverSurvivors,
    OriginalStrategicEncounterForces PlayerFinalForces,
    OriginalStrategicEncounterForces EnemyFinalForces);

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

    /// <summary>
    /// Reproduces <c>0x258FC:0x259D0-0x25A88</c>, the automatic branch used
    /// when its prior interactive choice dialog returns zero. The caller owns
    /// the two explicit score modifiers because wrapper <c>0x35924</c>
    /// constructs them outside this resolver.
    /// </summary>
    public static OriginalStrategicAutomaticEncounterResult ResolveAutomatic(
        OriginalStrategicEncounterPreparation preparation,
        int playerScoreModifier,
        int enemyScoreModifier,
        IOriginalStrategicEncounterRandom random)
    {
        ArgumentNullException.ThrowIfNull(random);
        preparation.PlayerResolverForces.Validate(nameof(preparation));
        preparation.PlayerReservedForces.Validate(nameof(preparation));
        preparation.EnemyResolverForces.Validate(nameof(preparation));
        if (playerScoreModifier < 0) throw new ArgumentOutOfRangeException(nameof(playerScoreModifier));
        if (enemyScoreModifier < 0) throw new ArgumentOutOfRangeException(nameof(enemyScoreModifier));

        var playerScore = checked(preparation.PlayerResolverForces.Total + playerScoreModifier / 3);
        var enemyScore = checked(preparation.EnemyResolverForces.Total + enemyScoreModifier / 3);
        if (playerScore <= enemyScore)
        {
            var zero = new OriginalStrategicEncounterForces(0, 0, 0);
            return new(false, zero, preparation.PlayerReservedForces, preparation.EnemyResolverForces);
        }

        var remainingEnemyStrength = preparation.EnemyResolverForces.Total;
        if (remainingEnemyStrength <= 0)
            throw new InvalidOperationException("Automatic victory requires a positive hostile force total.");
        var player = preparation.PlayerResolverForces;
        var swordsmenLoss = NextRemainder(random, remainingEnemyStrength);
        player = player with { Swordsmen = Math.Max(0, checked(player.Swordsmen - swordsmenLoss)) };
        remainingEnemyStrength = checked(remainingEnemyStrength - swordsmenLoss);
        var halberdierLoss = NextRemainder(random, remainingEnemyStrength);
        player = player with { Halberdiers = Math.Max(0, checked(player.Halberdiers - halberdierLoss)) };
        remainingEnemyStrength = checked(remainingEnemyStrength - halberdierLoss);
        var knightLoss = NextRemainder(random, remainingEnemyStrength);
        player = player with { Knights = Math.Max(0, checked(player.Knights - knightLoss)) };
        var finalPlayer = new OriginalStrategicEncounterForces(
            checked(player.Swordsmen + preparation.PlayerReservedForces.Swordsmen),
            checked(player.Halberdiers + preparation.PlayerReservedForces.Halberdiers),
            checked(player.Knights + preparation.PlayerReservedForces.Knights));
        return new(true, player, finalPlayer, preparation.EnemyResolverForces);
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

    private static int NextRemainder(IOriginalStrategicEncounterRandom random, int divisor)
    {
        if (divisor <= 0) throw new InvalidOperationException("Encounter loss divisor must remain positive.");
        var raw = random.NextRaw();
        if (raw < 0) throw new InvalidOperationException("Encounter random source returned a negative raw value.");
        return raw % divisor;
    }
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

/// <summary>
/// Applied result of the recovered automatic strategic encounter path. A
/// distinguished-player loss must be presented by the application; an empty
/// ordinary field record is removed through the original record helper.
/// </summary>
public sealed record OriginalStrategicAutomaticEncounterApplication(
    OriginalStrategicAutomaticEncounterResult ResolverResult,
    bool PlayerFieldRecordRemoved,
    bool DistinguishedPlayerLossRequiresModal);

/// <summary>Applied terminal result from the interactive six-counter resolver.</summary>
public sealed record OriginalStrategicInteractiveEncounterApplication(
    OriginalStrategicInteractiveEncounterOutcome Outcome,
    OriginalStrategicEncounterForces PlayerResolverSurvivors,
    OriginalStrategicEncounterForces PlayerFinalForces,
    OriginalStrategicEncounterForces EnemyFinalForces,
    bool PlayerFieldRecordRemoved,
    bool DistinguishedPlayerLossRequiresModal);

public sealed partial class Campaign
{
    /// <summary>
    /// Uses this campaign's seeded random stream for the host-owned fixed
    /// strategic pass. The explicit-random overload remains available to
    /// deterministic tests and non-host integrations.
    /// </summary>
    public OriginalStrategicCampaignPassResult AdvanceOriginalStrategicPass(
        OriginalStrategicCampaignPassInput input) =>
        AdvanceOriginalStrategicPass(input, new CampaignStrategicRandom(_random));

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
        {
            if (strategic.SchedulerFallbackTargetPerson < 0
                || strategic.SchedulerFallbackOriginProperty < 0)
                throw new InvalidOperationException(
                    "Strategic scheduler fallback globals are unavailable for this migrated save.");
            schedulerPass = OriginalStrategicMovement.AdvanceSchedulerPass(
                strategic,
                _originalStrategicResources,
                new OriginalStrategicSchedulerInput(
                    strategic.SchedulerFallbackTargetPerson,
                    strategic.SchedulerFallbackOriginProperty,
                    strategic.EngagedPlayerMovementSlot,
                    new StrategicPoint(Truncate(engaged.CurrentX), Truncate(engaged.CurrentY)),
                    pursuitTargets),
                random);
        }
        return new(report, playerPass, encounters, schedulerPass);
    }

    private sealed class CampaignStrategicRandom(Random random) : IOriginalStrategicRandom
    {
        public int Next(int exclusiveMaximum) => random.Next(exclusiveMaximum);
    }

    /// <summary>
    /// Applies only resolver <c>0x258FC</c>'s automatic-exit result to the
    /// six-counter strategic handoff. The interactive resolver intentionally
    /// remains outside this path.
    /// </summary>
    public OriginalStrategicAutomaticEncounterApplication ResolveAutomaticOriginalStrategicEncounter(
        OriginalStrategicPlayerEnemyEncounter encounter,
        int playerScoreModifier,
        IOriginalStrategicEncounterRandom random)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        ArgumentNullException.ThrowIfNull(random);
        if (State.OriginalStrategicState is not { } strategic)
            throw new InvalidOperationException("Campaign has no original strategic movement state.");

        var current = OriginalStrategicPlayerEnemyEncounter.Capture(
            strategic, State.Player, encounter.PlayerMovementSlot, encounter.EnemyMovementSlot);
        if (current != encounter)
            throw new InvalidOperationException("Strategic encounter counters changed before automatic resolution.");

        var resolverResult = OriginalStrategicEncounterStaging.ResolveAutomatic(
            encounter.Preparation, playerScoreModifier, enemyScoreModifier: 0, random);
        var army = State.Player.ArmyAt(encounter.PlayerMovementSlot);
        army.Units[UnitType.Swordsmen] = resolverResult.PlayerFinalForces.Swordsmen;
        army.Units[UnitType.Halberdiers] = resolverResult.PlayerFinalForces.Halberdiers;
        army.Units[UnitType.Knights] = resolverResult.PlayerFinalForces.Knights;
        army.RecordOriginalStrategicEncounterResolution();
        var enemy = strategic.MovementSlots.Single(slot => slot.Slot == encounter.EnemyMovementSlot);
        enemy.Swordsmen = resolverResult.EnemyFinalForces.Swordsmen;
        enemy.Halberdiers = resolverResult.EnemyFinalForces.Halberdiers;
        enemy.Knights = resolverResult.EnemyFinalForces.Knights;

        var playerFieldRecordRemoved = false;
        var distinguishedPlayerLossRequiresModal = false;
        if (resolverResult.PlayerFinalForces.Total == 0)
        {
            if (encounter.PlayerMovementSlot == strategic.EngagedPlayerMovementSlot)
                distinguishedPlayerLossRequiresModal = true;
            else
            {
                army.ResetOriginalStrategicEncounterState();
                playerFieldRecordRemoved = OriginalStrategicMovement.RemovePlayerMovementRecord(
                    strategic, encounter.PlayerMovementSlot);
            }
        }
        return new(resolverResult, playerFieldRecordRemoved, distinguishedPlayerLossRequiresModal);
    }

    /// <summary>
    /// Opens the interactive resolver through the recovered six-counter
    /// handoff. The encounter snapshot must still name the live player and
    /// hostile records, so a presentation host cannot start a stale resolver
    /// after a later strategic pass has changed either force pool.
    /// </summary>
    public OriginalStrategicInteractiveEncounterSession BeginInteractiveOriginalStrategicEncounter(
        OriginalStrategicPlayerEnemyEncounter encounter,
        int menuCode,
        int horizontalSpan,
        int verticalSpan,
        TimeSpan initialTime,
        IOriginalStrategicEncounterRandom? random = null,
        TimeSpan? tacticalCadence = null)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        if (State.OriginalStrategicState is not { } strategic)
            throw new InvalidOperationException("Campaign has no original strategic movement state.");
        var current = OriginalStrategicPlayerEnemyEncounter.Capture(
            strategic, State.Player, encounter.PlayerMovementSlot, encounter.EnemyMovementSlot);
        if (current != encounter)
            throw new InvalidOperationException("Strategic encounter counters changed before interactive resolution.");
        return OriginalStrategicInteractiveEncounterSession.Create(
            encounter.Preparation, menuCode, horizontalSpan, verticalSpan, initialTime, random, tacticalCadence);
    }

    /// <summary>
    /// Applies the completed interactive resolver's survivor counters through
    /// wrapper <c>0x35924</c>'s same reserve-restoration and field-record path.
    /// </summary>
    public OriginalStrategicInteractiveEncounterApplication ResolveInteractiveOriginalStrategicEncounter(
        OriginalStrategicPlayerEnemyEncounter encounter,
        OriginalStrategicInteractiveEncounterSession session)
    {
        ArgumentNullException.ThrowIfNull(encounter);
        ArgumentNullException.ThrowIfNull(session);
        if (session.Outcome == OriginalStrategicInteractiveEncounterOutcome.InProgress)
            throw new InvalidOperationException("Interactive strategic encounter has not ended.");
        if (State.OriginalStrategicState is not { } strategic)
            throw new InvalidOperationException("Campaign has no original strategic movement state.");
        var current = OriginalStrategicPlayerEnemyEncounter.Capture(
            strategic, State.Player, encounter.PlayerMovementSlot, encounter.EnemyMovementSlot);
        if (current != encounter)
            throw new InvalidOperationException("Strategic encounter counters changed before interactive resolution.");

        var playerResolverSurvivors = OriginalStrategicInteractiveEncounter.CountSurvivors(
            session.Units, OriginalStrategicInteractiveEncounterSide.Player);
        var enemyFinalForces = OriginalStrategicInteractiveEncounter.CountSurvivors(
            session.Units, OriginalStrategicInteractiveEncounterSide.Enemy);
        var reserves = encounter.Preparation.PlayerReservedForces;
        var playerFinalForces = new OriginalStrategicEncounterForces(
            checked(playerResolverSurvivors.Swordsmen + reserves.Swordsmen),
            checked(playerResolverSurvivors.Halberdiers + reserves.Halberdiers),
            checked(playerResolverSurvivors.Knights + reserves.Knights));
        var army = State.Player.ArmyAt(encounter.PlayerMovementSlot);
        army.Units[UnitType.Swordsmen] = playerFinalForces.Swordsmen;
        army.Units[UnitType.Halberdiers] = playerFinalForces.Halberdiers;
        army.Units[UnitType.Knights] = playerFinalForces.Knights;
        army.RecordOriginalStrategicEncounterResolution();
        var enemy = strategic.MovementSlots.Single(slot => slot.Slot == encounter.EnemyMovementSlot);
        enemy.Swordsmen = enemyFinalForces.Swordsmen;
        enemy.Halberdiers = enemyFinalForces.Halberdiers;
        enemy.Knights = enemyFinalForces.Knights;
        var removed = false;
        var requiresModal = false;
        if (playerFinalForces.Total == 0)
        {
            if (encounter.PlayerMovementSlot == strategic.EngagedPlayerMovementSlot)
                requiresModal = true;
            else
            {
                army.ResetOriginalStrategicEncounterState();
                removed = OriginalStrategicMovement.RemovePlayerMovementRecord(
                    strategic, encounter.PlayerMovementSlot);
            }
        }
        return new(session.Outcome, playerResolverSurvivors, playerFinalForces, enemyFinalForces,
            removed, requiresModal);
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
