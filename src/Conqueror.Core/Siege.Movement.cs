namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    private static readonly SiegeActorMovement FallbackActorMovement = new(3, 200, 64, 0, 0x142, 5);

    public void AdvanceRetainerMovement(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        foreach (var retainer in _retainers.Where(retainer =>
                     retainer.Health > 0 && retainer.OrderedDestination is not null))
        {
            var destination = retainer.OrderedDestination!.Value;
            if (retainer.MovementTick == 0 &&
                retainer.X == destination.X && retainer.Y == destination.Y)
            {
                CompleteDestination(retainer);
                continue;
            }

            var movement = ValidMovement(retainer.OriginalMovement) ?? FallbackActorMovement;
            retainer.MovementElapsed += elapsedSeconds;
            var catchUp = 0;
            while (retainer.MovementElapsed > movement.TickSeconds && catchUp < 24 &&
                   retainer.OrderedDestination is not null)
            {
                catchUp++;
                retainer.MovementElapsed -= movement.TickSeconds;
                if (retainer.MovementTick == 0)
                    AimRetainerAt(retainer, destination.X, destination.Y);
                AdvanceMovementTick(retainer, movement);
                retainer.MovementTick = (retainer.MovementTick + 1) % movement.TickCount;
                retainer.WalkFrame = (retainer.WalkFrame + 1) % movement.TickCount;
                if (retainer.MovementTick == 0 &&
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

    private void AdvanceMovementTick(SiegeRetainer retainer, SiegeActorMovement movement)
    {
        var (deltaX, deltaY) = RotateMovement(
            movement.FixedXDeltaPerTick, movement.FixedYDeltaPerTick, retainer.Facing);
        AdvanceMovementAxis(retainer, deltaX, true, movement.Flags);
        AdvanceMovementAxis(retainer, deltaY, false, movement.Flags);
    }

    private void AdvanceMovementAxis(SiegeRetainer retainer, int delta, bool xAxis, int flags)
    {
        if (delta == 0) return;
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
                return;
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
    }

    private static (int X, int Y) RotateMovement(int x, int y, Facing facing) => facing switch
    {
        Facing.East => (x, y),
        Facing.South => (-y, x),
        Facing.West => (-x, -y),
        _ => (y, -x)
    };

    private void MoveRetainerToward(SiegeRetainer retainer, int targetX, int targetY)
    {
        var candidates = CardinalSteps(retainer.X, retainer.Y)
            .OrderBy(point => Distance(point.X, point.Y, targetX, targetY));
        MoveRetainer(retainer, candidates.FirstOrDefault(point => RetainerCanEnter(retainer, point.X, point.Y)));
    }

    private void MoveRetainerAway(SiegeRetainer retainer, int targetX, int targetY)
    {
        var candidates = CardinalSteps(retainer.X, retainer.Y)
            .OrderByDescending(point => Distance(point.X, point.Y, targetX, targetY));
        MoveRetainer(retainer, candidates.FirstOrDefault(point => RetainerCanEnter(retainer, point.X, point.Y)));
    }

    private void MoveRetainer(SiegeRetainer retainer, Point destination)
    {
        if (destination == default) return;
        retainer.Facing = DirectionToward(retainer.X, retainer.Y, destination.X, destination.Y, retainer.Facing);
        retainer.X = destination.X;
        retainer.Y = destination.Y;
        retainer.WalkFrame = (retainer.WalkFrame + 1) % 3;
    }

    private bool RetainerCanEnter(SiegeRetainer self, int x, int y) =>
        x >= 0 && y >= 0 && x < Width && y < Height && !_movementBlocks[x, y] &&
        (x != PlayerX || y != PlayerY) &&
        EnemyAt(x, y) is null &&
        _retainers.All(retainer => ReferenceEquals(retainer, self) || retainer.Health <= 0 ||
            retainer.X != x || retainer.Y != y);

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
