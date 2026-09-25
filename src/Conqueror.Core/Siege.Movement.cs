namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    public void ConfigureActorRaycast(SiegeActorRaycast raycast)
    {
        ArgumentNullException.ThrowIfNull(raycast);
        _actorRaycast = raycast;
    }
    // PLACEHOLDER: RULE-ASSAULT-018. Actors without an imported movement descriptor use invented values.
    private static readonly SiegeActorMovement FallbackActorMovement = new(3, 200, 64, 0, 0x142, 5);

    // RULE-ASSAULT-006 thinker pass and RULE-ASSAULT-018 effect ticks for retainers.
    public void AdvanceRetainerMovement(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        // DEV-ASSAULT-001 and DEV-ASSAULT-002: the thinker runs on the fixed
        // update and settles ties in authored retainer order.
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
            // PLACEHOLDER: RULE-ASSAULT-018. The rule keeps every overrun; the cap of 24 catch-up ticks per update is the rebuild's own.
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
                // PLACEHOLDER: RULE-ASSAULT-013. at_destination compares the occupied cell; the rule uses the cell under the live position.
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

    // RULE-ASSAULT-015: direct chase, follow and mode 12 rewrite only the
    // descriptor's low flag byte: and 0xA7, then or 0x10 (0x142 becomes 0x112).
    private static int DirectMovementFlags(int descriptorFlags) =>
        (descriptorFlags & ~0xFF) | ((descriptorFlags & 0xFF & 0xA7) | 0x10);

    // RULE-ASSAULT-014: melee Retreat reaches kind-0 mode 5, whose wandering
    // handler uses and 0xA7, then or 0x40 (0x142 stays 0x142).
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
            // PLACEHOLDER: RULE-ASSAULT-013. at_destination compares the occupied cell; the rule uses the cell under the live position.
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
            case SiegeRetainerCommand.Defend:
                if (retainer.MovementActive) return true;
                AdvanceDefendOrder(retainer);
                return retainer.MovementActive;
            case SiegeRetainerCommand.Attack:
                AdvanceAttackOrder(retainer);
                return retainer.MovementActive;
            case SiegeRetainerCommand.Follow:
                AdvanceFollowOrder(retainer);
                return retainer.MovementActive;
            case SiegeRetainerCommand.Retreat:
                AdvanceRetreatOrder(retainer);
                return retainer.MovementActive;
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
        // PLACEHOLDER: RULE-ASSAULT-015. The heading starts from the occupied cell; the rule's go_to uses the cell under the live position.
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
        // RULE-VIEW-001 gives exact diagonal headings 0x20/0x60/0xA0/0xE0.
        // RULE-ASSAULT-015 adds 0x20 and masks with 0xC0, choosing clockwise on ties.
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
        // PLACEHOLDER: RULE-ASSAULT-017. Testing the x axis first, and stopping the whole tick on a refusal, follow the rule's unconfirmed choice.
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
                // RULE-ASSAULT-017: flag 0x10 stops the live effect, so the
                // actor decides again right after this tick.
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

    // RULE-ASSAULT-019: all supported actor states use behavior 0x87, so the
    // actor's map block sets bit 0x02 (FMT-VIEW-001) like a static blocker.
    // Keep actor occupancy exclusive while direct movers keep and retry offsets.
    // PLACEHOLDER: RULE-ASSAULT-017. Cells outside the map block movement; the rule does not record what the lookup reads there.
    private bool RetainerCanEnter(SiegeRetainer self, int x, int y) =>
        x >= 0 && y >= 0 && x < Width && y < Height && !_movementBlocks[x, y] &&
        (x != PlayerX || y != PlayerY) &&
        EnemyAt(x, y) is null &&
        _retainers.All(retainer => ReferenceEquals(retainer, self) || retainer.Health <= 0 ||
            retainer.X != x || retainer.Y != y);

    // RULE-ASSAULT-010: walk authored actor order and keep only a strictly
    // nearer opposite-side actor that the ray (RULE-VIEW-003) returns. Stop
    // early once the 8.8 depth is below 0x154.
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

    // RULE-ASSAULT-012: the mode-8 test casts toward the stored target, but
    // accepts any living opposite-side actor that ray returns. Contact is
    // strict against the source combat row's raw reach.
    private SiegeEnemy? ModeEightContactTarget(SiegeRetainer retainer, SiegeEnemy target)
    {
        if (retainer.OriginalCombatRow is not { } row ||
            RayToward(retainer, target) is not { } hit)
            return null;
        return hit.Actor.Health > 0 && _enemies.Contains(hit.Actor) &&
               hit.Distance8 < OriginalWeaponCombat.ActorContactDistanceForCombatRow(row)
            ? hit.Actor
            : null;
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

    private SiegeEnemy? AdjacentOpponentActor(SiegeRetainer source)
    {
        var centerX = FixedActorX8(source) >> 8;
        var centerY = FixedActorY8(source) >> 8;
        for (var dy = -1; dy <= 1; dy++)
        for (var dx = -1; dx <= 1; dx++)
        {
            var opponent = EnemyAt(centerX + dx, centerY + dy);
            if (opponent is not null) return opponent;
        }
        return null;
    }

    private void BeginFriendlyEscape(SiegeRetainer source)
    {
        var sourceX8 = FixedActorX8(source);
        var sourceY8 = FixedActorY8(source);
        source.OriginalHeading8 = OriginalActorMotion.HeadingToward(
            sourceX8 - (source.RetreatTargetX8 ?? sourceX8),
            sourceY8 - (source.RetreatTargetY8 ?? sourceY8));
        source.Facing = FacingForHeading(source.OriginalHeading8.Value);
        source.MovementWanders = false;
        source.MovementScalesEscapeDelta = true;
        source.MovementActive = true;
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
