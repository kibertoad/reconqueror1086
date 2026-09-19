namespace Conqueror.Core;

/// <summary>
/// Result of one already-acquired state-<c>0x28</c> contact at
/// <c>0x26DCB-0x270B4</c>. Target acquisition and phase scheduling are
/// deliberately outside this narrow arithmetic boundary.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterContactResult(
    bool Applied,
    int Damage,
    bool DeathAnimationStarted,
    bool AttackerTargetCleared);

/// <summary>
/// Outcome of the state-<c>0x28</c> neighbor-probe branch before its possible
/// contact arithmetic. Probe values retain their source representation.
/// </summary>
public readonly record struct OriginalStrategicInteractiveEncounterContactProbeResult(
    bool RetainedContactState,
    bool ContactDue);

/// <summary>
/// Exact contact arithmetic for the interactive strategic encounter. This
/// preserves the original category triangle and record mutations but does not
/// name the surrounding tactical action while its controls remain unmapped.
/// </summary>
public static class OriginalStrategicInteractiveEncounterCombat
{
    public const int ContactStateCode = 0x28;
    public const int DeathAnimationStateCode = 0x50;
    public const int LowHealthDeathThreshold = 20;

    /// <summary>
    /// Applies <c>0x26D4B-0x26DC5</c> after the caller has supplied the
    /// original neighbor helper's return value. Values zero and one clear the
    /// current contact; every other value advances the phase modulo five and
    /// makes a resolved contact due only at phase two.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterContactProbeResult ApplyMappedContactProbeResult(
        OriginalStrategicInteractiveEncounterUnit attacker,
        int probeResult)
    {
        ArgumentNullException.ThrowIfNull(attacker);
        if (attacker.StateCode != ContactStateCode)
            throw new InvalidOperationException("Mapped contact probe requires state 0x28.");

        if (probeResult is 0 or 1)
        {
            attacker.StateCode = 0;
            attacker.TargetUnitIndex = -1;
            if (attacker.ControlCode == 1)
            {
                attacker.AuxiliaryX = -1;
                attacker.AuxiliaryY = -1;
            }
            return default;
        }

        attacker.PhaseCounter = (attacker.PhaseCounter + 1) % 5;
        return new(true, attacker.PhaseCounter == 2);
    }

    /// <summary>
    /// Applies a contact after the source's neighbor query has selected the
    /// target. <paramref name="contactSideFilter"/> preserves global
    /// <c>19C98</c>: one suppresses contacts against the player lane and
    /// minus one suppresses contacts against the enemy lane; zero permits
    /// both. Other values take the source's ordinary contact path.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterContactResult ApplyMappedResolvedContact(
        IReadOnlyList<OriginalStrategicInteractiveEncounterUnit> units,
        int attackerIndex,
        int playerScoreModifier,
        int contactSideFilter,
        IOriginalStrategicEncounterRandom random)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(random);
        if ((uint)attackerIndex >= (uint)units.Count)
            throw new ArgumentOutOfRangeException(nameof(attackerIndex));
        if (playerScoreModifier < 0)
            throw new ArgumentOutOfRangeException(nameof(playerScoreModifier));

        var attacker = units[attackerIndex];
        ArgumentNullException.ThrowIfNull(attacker);
        var targetIndex = attacker.TargetUnitIndex;
        if ((uint)targetIndex >= (uint)units.Count)
            throw new InvalidOperationException("Mapped contact requires a live target record index.");
        var target = units[targetIndex];
        ArgumentNullException.ThrowIfNull(target);

        if (contactSideFilter == 1 && target.Side == OriginalStrategicInteractiveEncounterSide.Player
            || contactSideFilter == -1 && target.Side == OriginalStrategicInteractiveEncounterSide.Enemy)
            return default;

        var raw = random.NextRaw();
        if (raw < 0)
            throw new InvalidOperationException("Encounter random source returned a negative raw value.");

        var counterHit = IsCounterHit(attacker.Category, target.Category);
        var damage = raw % (counterHit ? 10 : 7);
        if (target.Side == OriginalStrategicInteractiveEncounterSide.Enemy)
            damage = checked(damage + playerScoreModifier / 3);
        target.RemainingStrength = checked(target.RemainingStrength - damage);

        var deathAnimationStarted = false;
        if (target.RemainingStrength is > 0 and < LowHealthDeathThreshold
            && target.StateCode != DeathAnimationStateCode)
        {
            target.StateCode = DeathAnimationStateCode;
            target.PhaseCounter = 0;
            deathAnimationStarted = true;
        }

        var attackerTargetCleared = target.RemainingStrength <= 0;
        if (attackerTargetCleared)
        {
            attacker.TargetUnitIndex = -1;
            attacker.StateCode = 0;
            if (attacker.ControlCode == 1)
            {
                attacker.AuxiliaryX = -1;
                attacker.AuxiliaryY = -1;
            }
        }

        return new(true, damage, deathAnimationStarted, attackerTargetCleared);
    }

    private static bool IsCounterHit(
        OriginalStrategicInteractiveEncounterCategory attacker,
        OriginalStrategicInteractiveEncounterCategory target) =>
        attacker == OriginalStrategicInteractiveEncounterCategory.Knights
        && target == OriginalStrategicInteractiveEncounterCategory.Halberdiers
        || attacker == OriginalStrategicInteractiveEncounterCategory.Swordsmen
        && target == OriginalStrategicInteractiveEncounterCategory.Knights
        || attacker == OriginalStrategicInteractiveEncounterCategory.Halberdiers
        && target == OriginalStrategicInteractiveEncounterCategory.Swordsmen;
}
