namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    private void StartVisual(SiegeEnemy actor, SiegeEnemyVisualState state)
    {
        if (state == SiegeEnemyVisualState.Hit) ApplyActorHitTransition(actor);
        if (actor is SiegeRetainer retainer) retainer.PendingRangedTarget = null;
        else actor.PendingHostileTarget = null;
        actor.VisualState = state;
        actor.VisualElapsed = 0;
    }

    private void ApplyActorHitTransition(SiegeEnemy actor)
    {
        var target = actor is SiegeRetainer retainer ? retainer.OrderedTarget : actor.HostileTarget;
        if (target is not { Health: > 0 }) return;
        // Mode-14 handler 0x503EC compares the newly hit source against its
        // stored target before constructing base-state + 2. Its shared
        // transition 0x4E77E selects attack mode 11 when source >= target and
        // morale mode 13 otherwise. Mode 15 repeats the comparison at 0x504F2.
        var nextMode = target.Health <= actor.Health ? 11 : 13;
        if (actor is SiegeRetainer friendly)
        {
            friendly.Command = SiegeRetainerCommand.Retreat;
            friendly.RetreatMode = nextMode;
            friendly.OrderedTarget = target;
            friendly.RetreatTargetX8 = FixedActorX8(target);
            friendly.RetreatTargetY8 = FixedActorY8(target);
            return;
        }
        if (IsOriginalHostile(actor)) actor.ActorMode = nextMode;
    }

    public void AdvanceHostileMovement(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        foreach (var enemy in _enemies.Where(IsOriginalHostile).ToArray())
        {
            if (enemy.VisualState != SiegeEnemyVisualState.Walk) continue;
            if (!enemy.HostileMovementActive && !TryBeginHostileMovement(enemy))
            {
                enemy.HostileMovementElapsed = 0;
                continue;
            }
            enemy.HostileMovementActive = true;
            var movement = ValidMovement(enemy.OriginalMovement) ?? FallbackActorMovement;
            enemy.HostileMovementElapsed += elapsedSeconds;
            var catchUp = 0;
            while (enemy.HostileMovementElapsed > movement.TickSeconds && catchUp < 24)
            {
                catchUp++;
                enemy.HostileMovementElapsed -= movement.TickSeconds;
                var continues = AdvanceHostileMovementTick(enemy, movement,
                    enemy.HostileMovementWanders
                        ? WanderingMovementFlags(movement.Flags)
                        : DirectMovementFlags(movement.Flags));
                enemy.HostileMovementTick = continues
                    ? (enemy.HostileMovementTick + 1) % movement.TickCount
                    : 0;
                enemy.HostileMovementActive = continues && enemy.HostileMovementTick != 0;
                enemy.WalkFrame = (enemy.WalkFrame + 1) % movement.TickCount;
                if (enemy.HostileMovementActive) continue;
                enemy.HostileMovementScalesEscapeDelta = false;
                if (!TryBeginHostileMovement(enemy))
                {
                    enemy.HostileMovementElapsed = 0;
                    break;
                }
                enemy.HostileMovementActive = true;
            }
            if (catchUp == 24 && enemy.HostileMovementElapsed > movement.TickSeconds)
                enemy.HostileMovementElapsed = movement.TickSeconds;
        }
    }

    private static bool IsOriginalHostile(SiegeEnemy actor) =>
        actor.Health > 0 && actor.OriginalModeProfile is not null && actor.OriginalActorKind is 2 or 4 or 7;

    private void AdvanceHostileOrder(SiegeEnemy enemy)
    {
        if (!IsOriginalHostile(enemy) || enemy.VisualState != SiegeEnemyVisualState.Walk ||
            enemy.HostileMovementActive) return;
        // A thinker may cross several acquisition-only modes before it creates
        // an effect. Bound the loop defensively; the mapped paths settle in at
        // most four transitions.
        for (var transition = 0; transition < 8; transition++)
        {
            switch (enemy.ActorMode)
            {
                case 4:
                {
                    var target = AcquireHostileTarget(enemy);
                    if (target is null)
                    {
                        enemy.ActorMode = enemy.OriginalActorKind == 4 ? 1 : 2;
                        return;
                    }
                    enemy.HostileTarget = target;
                    enemy.ActorMode = 8;
                    continue;
                }
                case 6:
                {
                    var target = AcquireHostileTarget(enemy);
                    if (target is null)
                    {
                        enemy.HostileMovementWanders = true;
                        return;
                    }
                    enemy.HostileTarget = target;
                    enemy.ActorMode = 8;
                    continue;
                }
                case 8:
                    if (enemy.HostileTarget is not { Health: > 0 } pursued ||
                        !IsFriendlyActor(pursued))
                    {
                        enemy.ActorMode = 6;
                        continue;
                    }
                    if (!HostileHasContact(enemy, pursued))
                    {
                        enemy.HostileMovementWanders = false;
                        AimHostileAt(enemy, pursued);
                        return;
                    }
                    enemy.ActorMode = 11;
                    continue;
                case 11:
                    if (TryStartHostileStrike(enemy)) return;
                    enemy.ActorMode = 6;
                    continue;
                case 13:
                    if (enemy.Health >= 6)
                    {
                        enemy.ActorMode = 6;
                        // The mode-13 handler creates no effect. The original
                        // main loop therefore revisits the actor on a later
                        // unrestricted thinker pass rather than recursively
                        // cycling 6 -> 8 -> 11 -> 13 in one call.
                        return;
                    }
                    BeginHostileEscape(enemy);
                    return;
                case 10:
                {
                    var target = AcquireHostileTarget(enemy);
                    enemy.ActorMode = target is null ? 3 : 4;
                    enemy.HostileTarget = target;
                    continue;
                }
                default:
                    return;
            }
        }
    }

    private bool TryBeginHostileMovement(SiegeEnemy enemy)
    {
        AdvanceHostileOrder(enemy);
        if (enemy.VisualState != SiegeEnemyVisualState.Walk) return false;
        if (enemy.ActorMode == 10) return true;
        if (enemy.ActorMode == 6 && enemy.HostileMovementWanders) return true;
        return enemy.ActorMode == 8 && enemy.HostileTarget is { Health: > 0 };
    }

    private SiegeEnemy? AcquireHostileTarget(SiegeEnemy source)
    {
        SiegeEnemy? nearest = null;
        var nearestDistance8 = 0x7fff;
        foreach (var candidate in FriendlyActorsInAuthoredOrder())
        {
            if (ActorRayDistance(source, candidate) is not { } distance8 || distance8 >= nearestDistance8)
                continue;
            nearest = candidate;
            nearestDistance8 = distance8;
            if (distance8 < 0x154) break;
        }
        if (nearest is not null)
        {
            source.HostileTargetX8 = FixedActorX8(nearest);
            source.HostileTargetY8 = FixedActorY8(nearest);
        }
        return nearest;
    }

    private IEnumerable<SiegeEnemy> FriendlyActorsInAuthoredOrder() =>
        _retainers.Cast<SiegeEnemy>().Append(PlayerActor)
            .Where(actor => actor.Health > 0)
            .OrderBy(actor => actor.OriginalActorOrder < 0 ? int.MaxValue : actor.OriginalActorOrder);

    private bool IsFriendlyActor(SiegeEnemy actor) =>
        ReferenceEquals(actor, PlayerActor) || _retainers.Contains(actor);

    private bool HostileHasContact(SiegeEnemy source, SiegeEnemy target)
    {
        if (source.OriginalCombatRow is not { } row || RayToward(source, target) is not { } hit)
            return false;
        return hit.Actor.Health > 0 && IsFriendlyActor(hit.Actor) &&
            hit.Distance8 < OriginalWeaponCombat.ActorContactDistanceForCombatRow(row);
    }

    private bool TryStartHostileStrike(SiegeEnemy source)
    {
        if (source.OriginalCombatRow is not { } row ||
            source.HostileTarget is not { Health: > 0 } intended ||
            RayToward(source, intended) is not { } hit || !IsFriendlyActor(hit.Actor) ||
            hit.Distance8 >= OriginalWeaponCombat.ActorContactDistanceForCombatRow(row))
            return false;
        var target = hit.Actor;
        source.HostileTarget = target;
        source.HostileTargetX8 = FixedActorX8(target);
        source.HostileTargetY8 = FixedActorY8(target);
        source.Facing = DirectionToward(source.X, source.Y, target.X, target.Y, source.Facing);
        source.OriginalHeading8 = OriginalActorMotion.HeadingToward(
            FixedActorX8(target) - FixedActorX8(source), FixedActorY8(target) - FixedActorY8(source));
        StartVisual(source, SiegeEnemyVisualState.Attack);
        source.PendingHostileTarget = target;
        return true;
    }

    private void BeginHostileEscape(SiegeEnemy source)
    {
        var sourceX8 = FixedActorX8(source);
        var sourceY8 = FixedActorY8(source);
        source.ActorMode = 10;
        source.OriginalHeading8 = OriginalActorMotion.HeadingToward(
            sourceX8 - (source.HostileTargetX8 ?? sourceX8),
            sourceY8 - (source.HostileTargetY8 ?? sourceY8));
        source.Facing = FacingForHeading(source.OriginalHeading8.Value);
        source.HostileMovementWanders = false;
        source.HostileMovementScalesEscapeDelta = true;
        source.HostileMovementActive = true;
    }

    private static void AimHostileAt(SiegeEnemy source, SiegeEnemy target)
    {
        source.OriginalHeading8 = OriginalActorMotion.HeadingToward(
            FixedActorX8(target) - FixedActorX8(source), FixedActorY8(target) - FixedActorY8(source));
        source.Facing = FacingForHeading(source.OriginalHeading8.Value);
    }

    private bool AdvanceHostileMovementTick(SiegeEnemy enemy, SiegeActorMovement movement, int flags)
    {
        var localX = enemy.HostileMovementScalesEscapeDelta
            ? OriginalActorMotion.ScaleEscapeDelta(movement.FixedXDeltaPerTick)
            : movement.FixedXDeltaPerTick;
        var localY = enemy.HostileMovementScalesEscapeDelta
            ? OriginalActorMotion.ScaleEscapeDelta(movement.FixedYDeltaPerTick)
            : movement.FixedYDeltaPerTick;
        var heading = enemy.OriginalHeading8 ?? ((int)enemy.Facing << 6);
        var (deltaX, deltaY) = OriginalActorMotion.Rotate(localX, localY, heading);
        return AdvanceHostileMovementAxis(enemy, deltaX, true, flags) &&
               AdvanceHostileMovementAxis(enemy, deltaY, false, flags);
    }

    private bool AdvanceHostileMovementAxis(SiegeEnemy enemy, int delta, bool xAxis, int flags)
    {
        if (delta == 0) return true;
        var offset = (xAxis ? enemy.OffsetX8 : enemy.OffsetY8) + delta;
        var step = Math.Sign(delta);
        if (step < 0 ? offset < -0x59 : offset > 0x59)
        {
            var nextX = enemy.X + (xAxis ? step : 0);
            var nextY = enemy.Y + (xAxis ? 0 : step);
            if (!EnemyCanEnter(enemy, nextX, nextY))
            {
                if ((flags & 0x40) != 0)
                {
                    if (xAxis) enemy.OffsetX8 = 0;
                    else enemy.OffsetY8 = 0;
                    enemy.Facing = (Facing)(((int)enemy.Facing + 3) & 3);
                    enemy.OriginalHeading8 = null;
                }
                return (flags & 0x10) == 0;
            }
        }
        if (step < 0 ? offset < -0x80 : offset > 0x80)
        {
            if (xAxis) enemy.X += step;
            else enemy.Y += step;
            offset -= step * 0x100;
        }
        if (xAxis) enemy.OffsetX8 = offset;
        else enemy.OffsetY8 = offset;
        return true;
    }

    private void ResolveHostileStrike(SiegeEnemy source, SiegeEnemy target)
    {
        if (source.ActorMode != 11 || target.Health <= 0 || !IsFriendlyActor(target)) return;
        var distance = Math.Max(1, (ActorDistanceInFixedPoint(source, target) + 0xff) >> 8);
        if (ReferenceEquals(target, PlayerActor))
        {
            var hit = source.OriginalCombatRow is { } row && source.OriginalAttackSkill is { } skill
                ? OriginalWeaponCombat.Hits(skill, OriginalWeaponCombat.PlayerAttackSkill(_player),
                    row >= 23 && distance <= 1, source.Facing == Facing, _random)
                : _random.Next(100) < Math.Clamp(70 - ArmorRating(), 5, 70);
            if (hit)
                Health -= source.OriginalCombatRow is { } combatRow
                    ? OriginalWeaponCombat.DamageForCombatRow(combatRow, ArmorRating(), _random)
                    : Math.Max(2, 12 - _player.Stats.Stamina / 3);
            Health = Math.Max(0, Health);
            SyncPlayerActor();
            return;
        }
        if (target is SiegeRetainer retainer) EnemyAttackRetainer(source, retainer, distance);
    }
}
