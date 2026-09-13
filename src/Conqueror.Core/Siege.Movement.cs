namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    private static readonly SiegeActorMovement FallbackActorMovement = new(3, 200, 64, 0, 0x142, 5);

    public void AdvanceRetainerMovement(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        foreach (var retainer in _retainers.Where(retainer => retainer.Health > 0))
        {
            if (retainer.MovementTick == 0 && !TryBeginRetainerMovement(retainer))
            {
                retainer.MovementElapsed = 0;
                continue;
            }

            var movement = ValidMovement(retainer.OriginalMovement) ?? FallbackActorMovement;
            retainer.MovementElapsed += elapsedSeconds;
            var catchUp = 0;
            while (retainer.MovementElapsed > movement.TickSeconds && catchUp < 24)
            {
                catchUp++;
                retainer.MovementElapsed -= movement.TickSeconds;
                if (retainer.MovementTick == 0 && !TryBeginRetainerMovement(retainer))
                {
                    retainer.MovementElapsed = 0;
                    break;
                }
                var effectContinues = AdvanceMovementTick(retainer, movement,
                    MovementFlagsFor(retainer, movement.Flags));
                retainer.MovementTick = effectContinues
                    ? (retainer.MovementTick + 1) % movement.TickCount
                    : 0;
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
                if (RetreatOrderTarget(retainer) is null)
                {
                    retainer.MovementWanders = false;
                    retainer.Command = SiegeRetainerCommand.Defend;
                    return false;
                }
                retainer.MovementWanders = retainer.OriginalActorKind is null or 0;
                return retainer.MovementWanders;
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
    }

    private static void AimRetainerAt(SiegeRetainer retainer, int targetX, int targetY)
    {
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
        var (deltaX, deltaY) = RotateMovement(
            movement.FixedXDeltaPerTick, movement.FixedYDeltaPerTick, retainer.Facing);
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

    // Acquisition 0x4F98D keeps the nearest opposite-side actor whose exact
    // identity is returned by raycaster 0x470A8. This bounded cell trace is the
    // current compatibility bridge until that fixed-point ray is reproduced.
    private SiegeEnemy? RetreatOrderTarget(SiegeRetainer retainer) =>
        VisibleOrderTarget(retainer);

    private SiegeEnemy? AttackOrderTarget(SiegeRetainer retainer) =>
        VisibleOrderTarget(retainer);

    private SiegeEnemy? VisibleOrderTarget(SiegeRetainer retainer)
    {
        if (retainer.OrderedTarget is { Health: > 0 } ordered && _enemies.Contains(ordered))
            return ActorLineIsClear(retainer, ordered) ? ordered : null;
        return _enemies.Where(enemy => enemy.Health > 0 && ActorLineIsClear(retainer, enemy))
            .OrderBy(enemy => ActorDistanceInFixedPoint(retainer, enemy))
            .FirstOrDefault();
    }

    private bool RetainerRangedAttack(SiegeRetainer retainer, SiegeEnemy target)
    {
        if (retainer.OriginalCombatRow is not { } row ||
            ActorDistanceInFixedPoint(retainer, target) >=
                OriginalWeaponCombat.ActorContactDistanceForCombatRow(row) ||
            !ActorLineIsClear(retainer, target))
            return false;
        retainer.Facing = DirectionToward(retainer.X, retainer.Y, target.X, target.Y, retainer.Facing);
        StartVisual(retainer, SiegeEnemyVisualState.Attack);
        retainer.PendingRangedTarget = target;
        return true;
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

    private static int ActorDistanceInFixedPoint(SiegeEnemy source, SiegeEnemy target)
    {
        var dx = target.X - source.X;
        var dy = target.Y - source.Y;
        return (int)Math.Round(Math.Sqrt((long)dx * dx + (long)dy * dy) * 256.0);
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
