namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    public void ConfigureActorRaycast(SiegeActorRaycast raycast)
    {
        ArgumentNullException.ThrowIfNull(raycast);
        _actorRaycast = raycast;
    }
    private static readonly SiegeActorMovement FallbackActorMovement = new(3, 200, 64, 0, 0x142, 5);

    public void AdvanceRetainerMovement(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        // The original D5C4 cursor advances per unrestricted main-loop pass,
        // so simultaneous actor precedence is processor/input-timing dependent.
        // Authored list order is the stable compatibility tie-breaker.
        foreach (var retainer in _retainers.Where(retainer => retainer.Health > 0))
        {
            if (!retainer.MovementActive && !TryBeginRetainerMovement(retainer))
            {
                retainer.MovementElapsed = 0;
                continue;
            }
            retainer.MovementActive = true;

            var movement = ValidMovement(retainer.OriginalMovement) ?? FallbackActorMovement;
            retainer.MovementElapsed += elapsedSeconds;
            var catchUp = 0;
            while (retainer.MovementElapsed > movement.TickSeconds && catchUp < 24)
            {
                catchUp++;
                retainer.MovementElapsed -= movement.TickSeconds;
                if (!retainer.MovementActive && !TryBeginRetainerMovement(retainer))
                {
                    retainer.MovementElapsed = 0;
                    break;
                }
                retainer.MovementActive = true;
                var effectContinues = AdvanceMovementTick(retainer, movement,
                    MovementFlagsFor(retainer, movement.Flags));
                retainer.MovementTick = effectContinues
                    ? (retainer.MovementTick + 1) % movement.TickCount
                    : 0;
                retainer.MovementActive = effectContinues && retainer.MovementTick != 0;
                retainer.WalkFrame = (retainer.WalkFrame + 1) % movement.TickCount;
                if (retainer.MovementTick == 0 && retainer.OrderedDestination is { } destination &&
                    retainer.X == destination.X && retainer.Y == destination.Y)
                    CompleteDestination(retainer);
            }
            if (catchUp == 24 && retainer.MovementElapsed > movement.TickSeconds)
                retainer.MovementElapsed = movement.TickSeconds;
        }
    }

    private static SiegeActorMovement? ValidMovement(SiegeActorMovement? movement) =>
        movement is { TickCount: > 0 and <= 4096, IntervalMilliseconds: > 0 and <= 60_000 }
            ? movement
            : null;

    // Direct chase/follow and mode 12 rewrite only the descriptor's low flag
    // byte: and 0xA7, then or 0x10 (0x142 therefore becomes 0x112).
    private static int DirectMovementFlags(int descriptorFlags) =>
        (descriptorFlags & ~0xFF) | ((descriptorFlags & 0xFF & 0xA7) | 0x10);

    // Melee Retreat reaches kind-0 mode 5. Its handler keeps the descriptor's
    // 0x40 collision family: and 0xA7, then or 0x40 (0x142 stays 0x142).
    private static int WanderingMovementFlags(int descriptorFlags) =>
        (descriptorFlags & ~0xFF) | ((descriptorFlags & 0xFF & 0xA7) | 0x40);

    private static int MovementFlagsFor(SiegeRetainer retainer, int descriptorFlags) =>
        retainer.MovementWanders
            ? WanderingMovementFlags(descriptorFlags)
            : DirectMovementFlags(descriptorFlags);

    private bool TryBeginRetainerMovement(SiegeRetainer retainer)
    {
        if (retainer.VisualState != SiegeEnemyVisualState.Walk) return false;
        // Scaling belongs to one constructed mode-10 effect. The exact
        // heading itself persists into mode 5, whose handler preserves it.
        retainer.MovementScalesEscapeDelta = false;
        if (retainer.OrderedDestination is { } destination)
        {
            retainer.MovementWanders = false;
            if (retainer.X == destination.X && retainer.Y == destination.Y)
            {
                CompleteDestination(retainer);
                return false;
            }
            AimRetainerAt(retainer, destination.X, destination.Y);
            return true;
        }

        switch (retainer.Command)
        {
            case SiegeRetainerCommand.Attack:
                var target = AttackOrderTarget(retainer);
                if (target is null)
                {
                    // Failed mode-6 acquisition leaves both friendly kinds in
                    // mode 6, whose handler preserves heading and installs 0x40.
                    retainer.MovementWanders = true;
                    return true;
                }
                retainer.MovementWanders = false;
                if (Distance(retainer.X, retainer.Y, target.X, target.Y) <=
                    RetainerAttackRange(retainer) || retainer.OriginalCombatRow is >= 23) return false;
                AimRetainerAt(retainer, target.X, target.Y);
                return true;
            case SiegeRetainerCommand.Follow:
                retainer.MovementWanders = false;
                if (Distance(retainer.X, retainer.Y, PlayerX, PlayerY) <= 1) return false;
                AimRetainerAt(retainer, PlayerX, PlayerY);
                return true;
            case SiegeRetainerCommand.Retreat:
                // Requested mode 10 transitions to mode 5 for friendly kind 0
                // and mode 4 for kind 1 after acquiring a visible opponent.
                // Mode 5 preserves heading; kind 1 continues through ranged mode 11.
                if (retainer.OriginalActorKind == 1)
                    return false;
                if (retainer.RetreatMode == 0)
                {
                    if (AcquireRetreatOrderTarget(retainer) is not null)
                    {
                        retainer.RetreatMode = 5;
                        retainer.MovementWanders = true;
                        return true;
                    }
                    retainer.MovementWanders = false;
                    retainer.Command = SiegeRetainerCommand.Defend;
                    return false;
                }
                if (retainer.RetreatMode == 5)
                {
                    if (_actorRaycast is not null && RetreatFormationTarget(retainer) is { } formation)
                    {
                        retainer.RetreatMode = 7;
                        retainer.RetreatRegroupTarget = formation;
                        retainer.MovementWanders = false;
                        AimRetainerAt(retainer, formation.X, formation.Y);
                        return true;
                    }
                    retainer.MovementWanders = true;
                    return true;
                }
                if (retainer.RetreatMode == 7)
                {
                    if (retainer.RetreatRegroupTarget is { Health: > 0 } regroup &&
                        RetreatFormationReached(retainer, regroup))
                    {
                        retainer.RetreatMode = 1;
                        retainer.RetreatRegroupTarget = null;
                        retainer.MovementWanders = false;
                        return false;
                    }
                    retainer.RetreatMode = 5;
                    retainer.RetreatRegroupTarget = null;
                    retainer.MovementWanders = true;
                    return true;
                }
                if (retainer.RetreatMode == 1)
                {
                    // Acquisition 0x4F648 scans the surrounding 3x3 map
                    // cells, y then x from -1 through +1, and accepts the
                    // first living same-side actor other than self.
                    retainer.RetreatRegroupTarget = AdjacentFriendlyActor(retainer);
                    retainer.RetreatMode = retainer.RetreatRegroupTarget is null ? 3 : 4;
                    retainer.MovementWanders = false;
                    return false;
                }
                if (retainer.RetreatMode == 3)
                {
                    if (_actorRaycast is not null && RetreatFormationTarget(retainer) is { } formation)
                    {
                        retainer.RetreatMode = 7;
                        retainer.RetreatRegroupTarget = formation;
                        retainer.MovementWanders = false;
                        AimRetainerAt(retainer, formation.X, formation.Y);
                        return true;
                    }
                    retainer.RetreatMode = 2;
                    retainer.RetreatRegroupTarget = null;
                    retainer.MovementWanders = false;
                    retainer.Command = SiegeRetainerCommand.Defend;
                    return false;
                }
                if (retainer.RetreatMode == 4)
                {
                    if (AcquireRetreatOrderTarget(retainer) is { } opponent)
                    {
                        retainer.RetreatMode = 8;
                        retainer.RetreatRegroupTarget = null;
                        retainer.OrderedTarget = opponent;
                        retainer.MovementWanders = false;
                        AimRetainerAt(retainer, opponent.X, opponent.Y);
                        return true;
                    }
                    retainer.RetreatMode = 1;
                    retainer.MovementWanders = false;
                    return false;
                }
                if (retainer.RetreatMode == 8)
                {
                    if (retainer.OrderedTarget is { Health: > 0 } pursued &&
                        _enemies.Contains(pursued) && ModeEightHasContact(retainer, pursued))
                    {
                        retainer.RetreatMode = 11;
                        retainer.MovementWanders = false;
                        RetainerRangedAttack(retainer, pursued);
                        return false;
                    }
                    // Kind-0 transition 0x4E745 selects mode 6 after failed
                    // contact. Its handler clears the target and resumes the
                    // same 0x40 wandering family used by public Attack.
                    retainer.RetreatMode = 6;
                    retainer.OrderedTarget = null;
                    retainer.MovementWanders = true;
                    return true;
                }
                if (retainer.RetreatMode == 6)
                {
                    if (AcquireRetreatOrderTarget(retainer) is { } reacquired)
                    {
                        retainer.RetreatMode = 8;
                        retainer.OrderedTarget = reacquired;
                        retainer.MovementWanders = false;
                        AimRetainerAt(retainer, reacquired.X, reacquired.Y);
                        return true;
                    }
                    retainer.MovementWanders = true;
                    return true;
                }
                if (retainer.RetreatMode == 13)
                {
                    // Predicate 0x4FD9A succeeds at health >= 6. Kind-0
                    // transition 0x4E7A4 therefore chooses mode 2 when
                    // healthy and mode 10 when morale has broken.
                    if (retainer.Health >= 6)
                    {
                        retainer.RetreatMode = 2;
                        retainer.MovementWanders = false;
                        retainer.Command = SiegeRetainerCommand.Defend;
                        return false;
                    }
                    var sourceX8 = FixedActorX8(retainer);
                    var sourceY8 = FixedActorY8(retainer);
                    var targetX8 = retainer.RetreatTargetX8 ?? sourceX8;
                    var targetY8 = retainer.RetreatTargetY8 ?? sourceY8;
                    retainer.RetreatMode = 10;
                    retainer.OriginalHeading8 = OriginalActorMotion.HeadingToward(
                        sourceX8 - targetX8, sourceY8 - targetY8);
                    retainer.Facing = FacingForHeading(retainer.OriginalHeading8.Value);
                    retainer.MovementWanders = false;
                    retainer.MovementScalesEscapeDelta = true;
                    return true;
                }
                if (retainer.RetreatMode == 10)
                {
                    // After the escape effect, the ordinary kind-0 mode-10
                    // acquisition chooses mode 5 on success or mode 2 on
                    // failure (transition 0x4E76B).
                    if (AcquireRetreatOrderTarget(retainer) is not null)
                    {
                        retainer.RetreatMode = 5;
                        retainer.MovementWanders = true;
                        return true;
                    }
                    retainer.RetreatMode = 2;
                    retainer.MovementWanders = false;
                    retainer.Command = SiegeRetainerCommand.Defend;
                    return false;
                }
                retainer.MovementWanders = false;
                return false;
            default:
                retainer.MovementWanders = false;
                return false;
        }
    }

    private static void CompleteDestination(SiegeRetainer retainer)
    {
        retainer.OrderedDestination = null;
        retainer.MovementElapsed = 0;
        retainer.MovementTick = 0;
        retainer.MovementActive = false;
    }

    private static void AimRetainerAt(SiegeRetainer retainer, int targetX, int targetY)
    {
        retainer.OriginalHeading8 = null;
        var dx = targetX - retainer.X;
        var dy = targetY - retainer.Y;
        var absX = Math.Abs(dx);
        var absY = Math.Abs(dy);
        if (absX > absY)
        {
            retainer.Facing = dx > 0 ? Facing.East : Facing.West;
            return;
        }
        if (absY > absX)
        {
            retainer.Facing = dy > 0 ? Facing.South : Facing.North;
            return;
        }
        // 0x445C4 yields exact diagonal headings 0x20/0x60/0xA0/0xE0;
        // mode 12 adds 0x20 and masks with 0xC0, choosing clockwise on ties.
        if (dy > 0) retainer.Facing = dx > 0 ? Facing.South : Facing.West;
        else if (dy < 0) retainer.Facing = dx > 0 ? Facing.East : Facing.North;
    }

    private bool AdvanceMovementTick(SiegeRetainer retainer, SiegeActorMovement movement, int effectFlags)
    {
        var localX = retainer.MovementScalesEscapeDelta
            ? OriginalActorMotion.ScaleEscapeDelta(movement.FixedXDeltaPerTick)
            : movement.FixedXDeltaPerTick;
        var localY = retainer.MovementScalesEscapeDelta
            ? OriginalActorMotion.ScaleEscapeDelta(movement.FixedYDeltaPerTick)
            : movement.FixedYDeltaPerTick;
        var (deltaX, deltaY) = retainer.OriginalHeading8 is { } heading
            ? OriginalActorMotion.Rotate(localX, localY, heading)
            : RotateMovement(localX, localY, retainer.Facing);
        return AdvanceMovementAxis(retainer, deltaX, true, effectFlags) &&
               AdvanceMovementAxis(retainer, deltaY, false, effectFlags);
    }

    private bool AdvanceMovementAxis(SiegeRetainer retainer, int delta, bool xAxis, int flags)
    {
        if (delta == 0) return true;
        var offset = (xAxis ? retainer.OffsetX8 : retainer.OffsetY8) + delta;
        var step = Math.Sign(delta);
        var crossesCollisionBand = step < 0 ? offset < -0x59 : offset > 0x59;
        if (crossesCollisionBand)
        {
            var nextX = retainer.X + (xAxis ? step : 0);
            var nextY = retainer.Y + (xAxis ? 0 : step);
            if (!RetainerCanEnter(retainer, nextX, nextY))
            {
                if ((flags & 0x40) != 0)
                {
                    if (xAxis) retainer.OffsetX8 = 0;
                    else retainer.OffsetY8 = 0;
                    retainer.Facing = (Facing)(((int)retainer.Facing + 3) & 3);
                    retainer.OriginalHeading8 = null;
                }
                // Flag 0x10 zeros the live effect's coordinate deltas and tick
                // count, so actor thinking resumes immediately after this tick.
                return (flags & 0x10) == 0;
            }
        }
        var crossesCell = step < 0 ? offset < -0x80 : offset > 0x80;
        if (crossesCell)
        {
            if (xAxis) retainer.X += step;
            else retainer.Y += step;
            offset -= step * 0x100;
        }
        if (xAxis) retainer.OffsetX8 = offset;
        else retainer.OffsetY8 = offset;
        return true;
    }

    private static (int X, int Y) RotateMovement(int x, int y, Facing facing) => facing switch
    {
        Facing.East => (x, y),
        Facing.South => (-y, x),
        Facing.West => (-x, -y),
        _ => (y, -x)
    };

    // All supported actor states use behavior 0x87, so their installed map
    // block contributes bit 0x02 exactly like an authored static blocker.
    // Keep actor occupancy exclusive while direct movers retain/retry offsets.
    private bool RetainerCanEnter(SiegeRetainer self, int x, int y) =>
        x >= 0 && y >= 0 && x < Width && y < Height && !_movementBlocks[x, y] &&
        (x != PlayerX || y != PlayerY) &&
        EnemyAt(x, y) is null &&
        _retainers.All(retainer => ReferenceEquals(retainer, self) || retainer.Health <= 0 ||
            retainer.X != x || retainer.Y != y);

    // Acquisition 0x4F98D walks authored actor order and keeps only a strictly
    // nearer opposite-side actor whose identity is returned by 0x470A8. It
    // exits early once the returned 8.8 depth is below 0x154.
    private SiegeEnemy? RetreatOrderTarget(SiegeRetainer retainer) =>
        VisibleOrderTarget(retainer, preserveExplicitTarget: false);

    private SiegeEnemy? AcquireRetreatOrderTarget(SiegeRetainer retainer)
    {
        var target = RetreatOrderTarget(retainer);
        if (target is null) return null;
        retainer.RetreatTargetX8 = FixedActorX8(target);
        retainer.RetreatTargetY8 = FixedActorY8(target);
        return target;
    }

    private SiegeEnemy? AttackOrderTarget(SiegeRetainer retainer) =>
        VisibleOrderTarget(retainer, preserveExplicitTarget: true);

    private SiegeEnemy? VisibleOrderTarget(SiegeRetainer retainer, bool preserveExplicitTarget)
    {
        if (preserveExplicitTarget &&
            retainer.OrderedTarget is { Health: > 0 } ordered && _enemies.Contains(ordered))
            return ActorRayDistance(retainer, ordered) is not null ? ordered : null;

        SiegeEnemy? nearest = null;
        var nearestDistance8 = 0x7fff;
        foreach (var enemy in _enemies.Where(enemy => enemy.Health > 0))
        {
            if (ActorRayDistance(retainer, enemy) is not { } distance8 || distance8 >= nearestDistance8)
                continue;
            nearest = enemy;
            nearestDistance8 = distance8;
            if (distance8 < 0x154) break;
        }
        return nearest;
    }

    // Mode-8 predicate 0x4F7A2 always casts toward the stored target, but it
    // accepts any living opposite-side actor returned by that ray. Contact is
    // strict against the source combat row's raw column-4 distance.
    private bool ModeEightHasContact(SiegeRetainer retainer, SiegeEnemy target)
    {
        if (retainer.OriginalCombatRow is not { } row ||
            RayToward(retainer, target) is not { } hit)
            return false;
        return hit.Actor.Health > 0 && _enemies.Contains(hit.Actor) &&
               hit.Distance8 < OriginalWeaponCombat.ActorContactDistanceForCombatRow(row);
    }

    private bool RetainerRangedAttack(SiegeRetainer retainer, SiegeEnemy target)
    {
        if (retainer.OriginalCombatRow is not { } row) return false;
        SiegeActorRayHit? hit;
        if (ActorManhattanDistanceInFixedPoint(retainer, target) <= 0x154)
            hit = new SiegeActorRayHit(target, 0x154);
        else
            hit = RayToward(retainer, target);
        if (hit is null || hit.Actor.Health <= 0 || !_enemies.Contains(hit.Actor) ||
            hit.Distance8 >= OriginalWeaponCombat.ActorContactDistanceForCombatRow(row))
            return false;
        target = hit.Actor;
        retainer.OrderedTarget = target;
        retainer.Facing = DirectionToward(retainer.X, retainer.Y, target.X, target.Y, retainer.Facing);
        StartVisual(retainer, SiegeEnemyVisualState.Attack);
        retainer.PendingRangedTarget = target;
        return true;
    }

    private static int FixedActorX8(SiegeEnemy actor) =>
        checked((actor.X << 8) + 0x80 + actor.OffsetX8);

    private static int FixedActorY8(SiegeEnemy actor) =>
        checked((actor.Y << 8) + 0x80 + actor.OffsetY8);

    private static Facing FacingForHeading(int heading) =>
        (Facing)(((heading + 0x20) & 0xff) >> 6);

    private SiegeActorRayHit? RayToward(SiegeEnemy source, SiegeEnemy target)
    {
        if (_actorRaycast is not null) return _actorRaycast(source, target);
        return ActorLineIsClear(source, target)
            ? new SiegeActorRayHit(target, ActorDistanceInFixedPoint(source, target))
            : null;
    }

    private bool ActorLineIsClear(SiegeEnemy source, SiegeEnemy target)
    {
        var dx = target.X - source.X;
        var dy = target.Y - source.Y;
        var steps = Math.Max(Math.Abs(dx), Math.Abs(dy));
        for (var step = 1; step < steps; step++)
        {
            var x = source.X + (int)Math.Round(dx * step / (double)steps);
            var y = source.Y + (int)Math.Round(dy * step / (double)steps);
            if (_movementBlocks[x, y] || x == PlayerX && y == PlayerY ||
                EnemyAt(x, y) is not null || RetainerAt(x, y) is not null)
                return false;
        }
        return true;
    }

    private SiegeEnemy? RetreatFormationTarget(SiegeRetainer source)
    {
        SiegeEnemy? nearest = null;
        var nearestDistance8 = 0x7fff;
        foreach (var candidate in FriendlyActorsInAuthoredOrder(source))
        {
            if (ActorRayDistance(source, candidate) is not { } distance8 || distance8 >= nearestDistance8)
                continue;
            nearest = candidate;
            nearestDistance8 = distance8;
            if (distance8 < 0x200) break;
        }
        return nearest;
    }

    private bool RetreatFormationReached(SiegeRetainer source, SiegeEnemy target)
    {
        if (_actorRaycast?.Invoke(source, target) is not { Distance8: < 0x200 } hit) return false;
        return !ReferenceEquals(hit.Actor, source) && hit.Actor.Health > 0 &&
            (ReferenceEquals(hit.Actor, PlayerActor) || _retainers.Contains(hit.Actor));
    }

    private SiegeEnemy? AdjacentFriendlyActor(SiegeRetainer source)
    {
        var centerX = ((source.X << 8) + 0x80 + source.OffsetX8) >> 8;
        var centerY = ((source.Y << 8) + 0x80 + source.OffsetY8) >> 8;
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
        {
            var x = centerX + dx;
            var y = centerY + dy;
            if (PlayerActor.Health > 0 && PlayerActor.X == x && PlayerActor.Y == y)
                return PlayerActor;
            var retainer = _retainers.FirstOrDefault(actor =>
                !ReferenceEquals(actor, source) && actor.Health > 0 && actor.X == x && actor.Y == y);
            if (retainer is not null) return retainer;
        }
        return null;
    }

    private IEnumerable<SiegeEnemy> FriendlyActorsInAuthoredOrder(SiegeRetainer source) =>
        _retainers.Cast<SiegeEnemy>().Append(PlayerActor)
            .Where(actor => !ReferenceEquals(actor, source) && actor.Health > 0)
            .OrderBy(actor => actor.OriginalActorOrder < 0 ? int.MaxValue : actor.OriginalActorOrder);

    private int? ActorRayDistance(SiegeEnemy source, SiegeEnemy target) => _actorRaycast is not null
        ? _actorRaycast(source, target) is { } hit && ReferenceEquals(hit.Actor, target) ? hit.Distance8 : null
        : ActorLineIsClear(source, target) ? ActorDistanceInFixedPoint(source, target) : null;

    private static int ActorDistanceInFixedPoint(SiegeEnemy source, SiegeEnemy target)
    {
        var dx8 = ((target.X - source.X) << 8) + target.OffsetX8 - source.OffsetX8;
        var dy8 = ((target.Y - source.Y) << 8) + target.OffsetY8 - source.OffsetY8;
        return (int)Math.Round(Math.Sqrt((long)dx8 * dx8 + (long)dy8 * dy8));
    }

    private static int ActorManhattanDistanceInFixedPoint(SiegeEnemy source, SiegeEnemy target)
    {
        var dx8 = ((target.X - source.X) << 8) + target.OffsetX8 - source.OffsetX8;
        var dy8 = ((target.Y - source.Y) << 8) + target.OffsetY8 - source.OffsetY8;
        return checked(Math.Abs(dx8) + Math.Abs(dy8));
    }

    private static IReadOnlyList<Point> CardinalSteps(int x, int y) =>
        [new(x + 1, y), new(x - 1, y), new(x, y + 1), new(x, y - 1)];

    private static int Distance(int x1, int y1, int x2, int y2) =>
        Math.Abs(x1 - x2) + Math.Abs(y1 - y2);

    private static bool IsNeighbor(int x1, int y1, int x2, int y2) =>
        Math.Abs(x1 - x2) <= 1 && Math.Abs(y1 - y2) <= 1;

    private static bool[,] MovementBlocksFor(SiegeTile[,] tiles)
    {
        var result = new bool[tiles.GetLength(0), tiles.GetLength(1)];
        for (var x = 0; x < tiles.GetLength(0); x++)
        for (var y = 0; y < tiles.GetLength(1); y++)
            result[x, y] = tiles[x, y] != SiegeTile.Floor;
        return result;
    }
}
