namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    private void StartVisual(SiegeEnemy actor, SiegeEnemyVisualState state)
    {
        if (state == SiegeEnemyVisualState.Hit) ApplyActorHitTransition(actor);
        if (state is SiegeEnemyVisualState.Hit or SiegeEnemyVisualState.Dying)
        {
            actor.HostileMovementActive = false;
            actor.HostileMovementTick = 0;
            actor.HostileMovementElapsed = 0;
            actor.HostileMovementScalesEscapeDelta = false;
            if (actor is SiegeRetainer interrupted)
            {
                interrupted.MovementActive = false;
                interrupted.MovementTick = 0;
                interrupted.MovementElapsed = 0;
                interrupted.MovementScalesEscapeDelta = false;
            }
        }
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
            friendly.ActorMode = nextMode;
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
        // State routine 0x4F49C evaluates exactly one current-mode predicate,
        // applies its kind-table edge through 0x4E5F0, and then invokes the
        // new mode's handler. Modes 1-4/13 construct no live effect and are
        // reconsidered only on a later thinker pass.
        var nextMode = enemy.ActorMode;
        switch (enemy.ActorMode)
        {
            case 1:
            {
                var ally = AdjacentHostileActor(enemy);
                if (ally is not null) enemy.HostileTarget = ally;
                nextMode = enemy.OriginalActorKind == 4
                    ? ally is null ? 3 : 4
                    : ally is null ? 6 : 4;
                break;
            }
            case 2:
            {
                var opponent = AdjacentFriendlyActor(enemy);
                if (opponent is not null) enemy.HostileTarget = opponent;
                nextMode = enemy.OriginalActorKind == 4
                    ? opponent is null ? 4 : 11
                    : opponent is null ? 1 : 11;
                break;
            }
            case 3:
            {
                var ally = AcquireHostileFormationTarget(enemy);
                if (ally is not null) enemy.HostileTarget = ally;
                nextMode = enemy.OriginalActorKind == 4
                    ? ally is null ? 2 : 7
                    : ally is null ? 1 : 7;
                break;
            }
            case 4:
            {
                var opponent = AcquireHostileTarget(enemy);
                if (opponent is not null) enemy.HostileTarget = opponent;
                nextMode = opponent is not null ? 8 : enemy.OriginalActorKind == 4 ? 1 : 2;
                break;
            }
            case 5:
            {
                var ally = AcquireHostileFormationTarget(enemy);
                if (ally is not null) enemy.HostileTarget = ally;
                nextMode = ally is null ? 5 : 7;
                break;
            }
            case 6:
            {
                var opponent = AcquireHostileTarget(enemy);
                if (opponent is not null) enemy.HostileTarget = opponent;
                nextMode = opponent is null ? 6 : 8;
                break;
            }
            case 7:
                nextMode = HostileFormationReached(enemy)
                    ? 1
                    : enemy.OriginalActorKind == 4 ? 5 : 6;
                break;
            case 8:
                nextMode = enemy.HostileTarget is { Health: > 0 } pursued && IsFriendlyActor(pursued) &&
                    HostileHasContact(enemy, pursued) ? 11 : 6;
                break;
            case 9:
            {
                // Kinds 2/4/7 use the same 0x4FC34 acquisition but 0x4E8D8
                // leaves mode 9 for mode 6 on success or mode 1 on failure.
                var ally = AcquireHostileFormationTarget(enemy);
                if (ally is not null) enemy.HostileTarget = ally;
                nextMode = ally is null ? 1 : 6;
                break;
            }
            case 10:
            {
                var opponent = AcquireHostileTarget(enemy);
                if (opponent is not null) enemy.HostileTarget = opponent;
                nextMode = opponent is null ? 3 : 4;
                break;
            }
            case 13:
                nextMode = enemy.Health >= 6 ? 6 : 10;
                break;
        }
        enemy.ActorMode = nextMode;
        BeginHostileMode(enemy);
    }

    private void BeginHostileMode(SiegeEnemy enemy)
    {
        enemy.HostileMovementActive = false;
        enemy.HostileMovementWanders = false;
        switch (enemy.ActorMode)
        {
            case 5:
            case 6:
                // Shared handler 0x4FDCD clears actor/coordinate targets and
                // constructs the descriptor's 0x40 collision family.
                enemy.HostileTarget = null;
                enemy.HostileTargetX8 = null;
                enemy.HostileTargetY8 = null;
                enemy.HostileMovementWanders = true;
                enemy.HostileMovementActive = true;
                break;
            case 7:
            case 8:
                if (enemy.HostileTarget is { Health: > 0 } target)
                {
                    AimHostileAt(enemy, target);
                    enemy.HostileMovementActive = true;
                }
                break;
            case 10:
                BeginHostileEscape(enemy);
                break;
            case 11:
                TryStartHostileStrike(enemy);
                break;
        }
    }

    private bool TryBeginHostileMovement(SiegeEnemy enemy)
    {
        AdvanceHostileOrder(enemy);
        if (enemy.VisualState != SiegeEnemyVisualState.Walk) return false;
        return enemy.HostileMovementActive;
    }

    private SiegeEnemy? AdjacentHostileActor(SiegeEnemy source)
    {
        var centerX = FixedActorX8(source) >> 8;
        var centerY = FixedActorY8(source) >> 8;
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
        {
            var actor = EnemyAt(centerX + dx, centerY + dy);
            if (actor is not null && !ReferenceEquals(actor, source)) return actor;
        }
        return null;
    }

    private SiegeEnemy? AdjacentFriendlyActor(SiegeEnemy source)
    {
        var centerX = FixedActorX8(source) >> 8;
        var centerY = FixedActorY8(source) >> 8;
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
        {
            var x = centerX + dx;
            var y = centerY + dy;
            var retainer = RetainerAt(x, y);
            if (retainer is not null) return retainer;
            if (PlayerActor.Health > 0 && PlayerActor.X == x && PlayerActor.Y == y)
                return PlayerActor;
        }
        return null;
    }

    private SiegeEnemy? AcquireHostileFormationTarget(SiegeEnemy source)
    {
        SiegeEnemy? nearest = null;
        var nearestDistance8 = 0x7fff;
        foreach (var candidate in _enemies
                     .Where(actor => !ReferenceEquals(actor, source) && actor.Health > 0)
                     .OrderBy(actor => actor.OriginalActorOrder < 0 ? int.MaxValue : actor.OriginalActorOrder))
        {
            if (ActorRayDistance(source, candidate) is not { } distance8 || distance8 >= nearestDistance8)
                continue;
            nearest = candidate;
            nearestDistance8 = distance8;
            if (distance8 < 0x200) break;
        }
        return nearest;
    }

    private bool HostileFormationReached(SiegeEnemy source)
    {
        if (source.HostileTarget is not { Health: > 0 } aimed ||
            RayToward(source, aimed) is not { Distance8: < 0x200 } hit)
            return false;
        return !ReferenceEquals(hit.Actor, source) && hit.Actor.Health > 0 && _enemies.Contains(hit.Actor);
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
            source.HostileTarget is not { Health: > 0 } intended)
            return false;
        var hit = ActorManhattanDistanceInFixedPoint(source, intended) <= 0x154
            ? new SiegeActorRayHit(intended, 0x154)
            : RayToward(source, intended);
        if (hit is null || !IsFriendlyActor(hit.Actor) ||
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
        var heading = OriginalActorMotion.HeadingToward(
            FixedActorX8(target) - FixedActorX8(source), FixedActorY8(target) - FixedActorY8(source));
        source.OriginalHeading8 = (heading + 0x20) & 0xC0;
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
