using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public readonly record struct SiegeProjectedBlock(
    DynamixSceneBlock Block, int OffsetX8, int OffsetY8, int MapBlockIndex);

public readonly record struct SiegeSceneRayCandidate(
    SiegeRayHit Hit, DynamixSceneBlock Block, int TextureCoordinate8);

public static class OriginalSiegeProjection
{
    private const int MaximumTraversalSteps = 0x40;
    private const int MaximumCandidates = 0x1f;

    public static IReadOnlyList<SiegeSceneRayCandidate> CastColumn(
        SiegeSession siege,
        DynamixScene scene,
        int sourceOriginX,
        int sourceOriginY,
        int column,
        int viewportWidth,
        Func<int, int, SiegeProjectedBlock?>? activeBlockAt = null)
    {
        ArgumentNullException.ThrowIfNull(siege);
        ArgumentNullException.ThrowIfNull(scene);
        if (viewportWidth <= 1) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (column < 0 || column >= viewportWidth) throw new ArgumentOutOfRangeException(nameof(column));

        var lateral14 = (0x400000 / viewportWidth * (column - viewportWidth / 2)) >> 8;
        var (forwardX, forwardY) = CardinalDirection(siege.Facing);
        var rayX = forwardX * 0x4000 - forwardY * lateral14;
        var rayY = forwardY * 0x4000 + forwardX * lateral14;
        var viewerX8 = ((siege.PlayerX + sourceOriginX) << 8) + 0x80;
        var viewerY8 = ((siege.PlayerY + sourceOriginY) << 8) + 0x80;
        return CastFixed(scene, sourceOriginX, sourceOriginY, viewerX8, viewerY8,
            rayX, rayY, siege, activeBlockAt);
    }

    private static IReadOnlyList<SiegeSceneRayCandidate> CastFixed(
        DynamixScene scene,
        int sourceOriginX,
        int sourceOriginY,
        int viewerX8,
        int viewerY8,
        int rayX,
        int rayY,
        SiegeSession siege,
        Func<int, int, SiegeProjectedBlock?>? activeBlockAt)
    {
        var candidates = new List<SiegeSceneRayCandidate>(MaximumCandidates);
        var signX = Math.Sign(rayX);
        var signY = Math.Sign(rayY);
        var absoluteX = Math.Abs(rayX);
        var absoluteY = Math.Abs(rayY);
        int incrementX;
        int incrementY;
        if (absoluteX > absoluteY)
        {
            incrementX = signX << 8;
            incrementY = (int)((long)incrementX * rayY / rayX);
        }
        else
        {
            incrementY = signY << 8;
            incrementX = (int)((long)incrementY * rayX / rayY);
        }

        var pointX8 = viewerX8;
        var pointY8 = viewerY8;
        var previousX = viewerX8 >> 8 & 0x7f;
        var previousY = viewerY8 >> 8 & 0x7f;
        for (var step = 0; step < MaximumTraversalSteps; step++)
        {
            pointX8 += incrementX;
            pointY8 += incrementY;
            var mapX = pointX8 >> 8 & 0x7f;
            var mapY = pointY8 >> 8 & 0x7f;

            if (mapX != previousX && mapY != previousY)
            {
                if (AddCell(mapX, previousY) || AddCell(previousX, mapY) || AddCell(mapX, mapY)) break;
            }
            else if (mapX == previousX)
            {
                if (AddCell(mapX, mapY) || AddCell((mapX - 1) & 0x7f, mapY) ||
                    AddCell((mapX + 1) & 0x7f, mapY)) break;
            }
            else
            {
                if (AddCell(mapX, mapY) || AddCell(mapX, (mapY - 1) & 0x7f) ||
                    AddCell(mapX, (mapY + 1) & 0x7f)) break;
            }

            previousX = mapX;
            previousY = mapY;
        }
        return candidates;

        bool AddCell(int sourceX, int sourceY)
        {
            var localX = sourceX - sourceOriginX;
            var localY = sourceY - sourceOriginY;
            var projected = activeBlockAt?.Invoke(localX, localY) ?? StaticBlock(sourceX, sourceY);
            var current = projected;
            while (current.Block.Kind != 0)
            {
                if (TryIntersect(current, sourceX, sourceY, viewerX8, viewerY8,
                        rayX, rayY, sourceOriginX, sourceOriginY, siege, out var candidate))
                {
                    candidates.Add(candidate);
                    if ((current.Block.Behavior & 1) == 0 || candidates.Count == MaximumCandidates)
                        return true;
                }

                if (candidates.Count >= MaximumCandidates || (current.Block.Behavior & 1) == 0 ||
                    (current.Block.Kind != 4 && (current.Block.Behavior & 8) == 0) ||
                    (uint)current.Block.StateTarget >= (uint)scene.Blocks.Count)
                    return false;
                var target = scene.Blocks[current.Block.StateTarget];
                if (target.Kind == 0) return false;
                current = new SiegeProjectedBlock(target, target.InitialXOffset8,
                    target.InitialYOffset8, projected.MapBlockIndex);
            }
            return false;
        }

        SiegeProjectedBlock StaticBlock(int sourceX, int sourceY)
        {
            var index = scene.BlockIndexAt(sourceX, sourceY);
            var block = scene.Blocks[index];
            return new SiegeProjectedBlock(block, block.InitialXOffset8, block.InitialYOffset8, index);
        }
    }

    private static bool TryIntersect(
        SiegeProjectedBlock projected,
        int sourceCellX,
        int sourceCellY,
        int viewerX8,
        int viewerY8,
        int rayX,
        int rayY,
        int sourceOriginX,
        int sourceOriginY,
        SiegeSession siege,
        out SiegeSceneRayCandidate candidate)
    {
        var block = projected.Block;
        var minX = (sourceCellX << 8) - viewerX8 + projected.OffsetX8;
        var minY = (sourceCellY << 8) - viewerY8 + projected.OffsetY8;
        var maxX = minX + 0xff;
        var maxY = minY + 0xff;
        int contactX;
        int contactY;
        SiegeWallFace face;
        var hit = block.Kind switch
        {
            1 or 4 => IntersectBox(minX, minY, maxX, maxY, rayX, rayY,
                out contactX, out contactY, out face),
            2 => IntersectHorizontal(minX, maxX, minY + 0x80, rayX, rayY,
                out contactX, out contactY, out face),
            3 => IntersectVertical(minY, maxY, minX + 0x80, rayX, rayY,
                out contactX, out contactY, out face),
            5 => IntersectDiagonal(minX - 1, minY - 1, maxX + 1, maxY + 1,
                rayX, rayY, out contactX, out contactY, out face),
            6 => IntersectDiagonal(minX - 1, maxY + 1, maxX + 1, minY - 1,
                rayX, rayY, out contactX, out contactY, out face),
            _ => IntersectBox(minX, minY, maxX, maxY, rayX, rayY,
                out contactX, out contactY, out face)
        };
        if (!hit)
        {
            candidate = default;
            return false;
        }

        var worldX8 = contactX + viewerX8 - projected.OffsetX8;
        var worldY8 = contactY + viewerY8 - projected.OffsetY8;
        if (block.Kind == 4)
        {
            worldX8 += projected.OffsetX8;
            worldY8 += projected.OffsetY8;
        }
        var localContactX8 = worldX8 - (sourceOriginX << 8);
        var localContactY8 = worldY8 - (sourceOriginY << 8);
        var localMapX = localContactX8 >> 8;
        var localMapY = localContactY8 >> 8;
        var distance8 = ForwardDistance8(siege.Facing,
            localContactX8 - ((siege.PlayerX << 8) + 0x80),
            localContactY8 - ((siege.PlayerY << 8) + 0x80));
        if (distance8 <= 0)
        {
            candidate = default;
            return false;
        }

        var coordinate8 = face is SiegeWallFace.East or SiegeWallFace.West
            ? worldY8 & 0xff
            : worldX8 & 0xff;
        if (face is SiegeWallFace.North or SiegeWallFace.East) coordinate8 = 0xff - coordinate8;
        var tile = siege.TileAt(localMapX, localMapY);
        candidate = new SiegeSceneRayCandidate(
            new SiegeRayHit(distance8 / 256d, tile,
                face is SiegeWallFace.East or SiegeWallFace.West,
                face, localMapX, localMapY, coordinate8 / 256d)
            {
                Distance8 = distance8,
                ContactX8 = localContactX8,
                ContactY8 = localContactY8,
                ContactedBlock = true,
                SceneBlockIndex = block.Index
            }, block, coordinate8);
        return true;
    }

    private static bool IntersectBox(
        int minX, int minY, int maxX, int maxY, int rayX, int rayY,
        out int x, out int y, out SiegeWallFace face)
    {
        var hasX = rayX != 0;
        var xFace = rayX > 0 ? minX : maxX;
        var yAtX = hasX ? RoundedDivide((long)xFace * rayY, rayX) : 0;
        hasX &= xFace >= Math.Min(0, rayX) && xFace <= Math.Max(0, rayX) && yAtX >= minY && yAtX <= maxY;

        var hasY = rayY != 0;
        var yFace = rayY > 0 ? minY : maxY;
        var xAtY = hasY ? RoundedDivide((long)yFace * rayX, rayY) : 0;
        hasY &= yFace >= Math.Min(0, rayY) && yFace <= Math.Max(0, rayY) && xAtY >= minX && xAtY <= maxX;
        if (!hasX && !hasY) return NoHit(out x, out y, out face);

        if (hasX && (!hasY || FractionBefore(xFace, rayX, yFace, rayY)))
        {
            x = xFace;
            y = yAtX;
            face = rayX > 0 ? SiegeWallFace.West : SiegeWallFace.East;
            return true;
        }
        x = xAtY;
        y = yFace;
        face = rayY > 0 ? SiegeWallFace.North : SiegeWallFace.South;
        return true;
    }

    private static bool IntersectHorizontal(
        int minX, int maxX, int planeY, int rayX, int rayY,
        out int x, out int y, out SiegeWallFace face)
    {
        if (rayY == 0 || planeY < Math.Min(0, rayY) || planeY > Math.Max(0, rayY))
            return NoHit(out x, out y, out face);
        x = RoundedDivide((long)planeY * rayX, rayY);
        y = planeY;
        if (x < minX || x > maxX) return NoHit(out x, out y, out face);
        face = rayY > 0 ? SiegeWallFace.North : SiegeWallFace.South;
        return true;
    }

    private static bool IntersectVertical(
        int minY, int maxY, int planeX, int rayX, int rayY,
        out int x, out int y, out SiegeWallFace face)
    {
        if (rayX == 0 || planeX < Math.Min(0, rayX) || planeX > Math.Max(0, rayX))
            return NoHit(out x, out y, out face);
        x = planeX;
        y = RoundedDivide((long)planeX * rayY, rayX);
        if (y < minY || y > maxY) return NoHit(out x, out y, out face);
        face = rayX > 0 ? SiegeWallFace.West : SiegeWallFace.East;
        return true;
    }

    private static bool IntersectDiagonal(
        int ax, int ay, int bx, int by, int rayX, int rayY,
        out int x, out int y, out SiegeWallFace face)
    {
        var sx = bx - ax;
        var sy = by - ay;
        var denominator = (long)rayX * sy - (long)rayY * sx;
        if (denominator == 0) return NoHit(out x, out y, out face);
        var rayNumerator = (long)ax * sy - (long)ay * sx;
        var segmentNumerator = (long)ax * rayY - (long)ay * rayX;
        if (!WithinFraction(rayNumerator, denominator) || !WithinFraction(segmentNumerator, denominator))
            return NoHit(out x, out y, out face);
        x = RoundedDivide((long)rayX * rayNumerator, denominator);
        y = RoundedDivide((long)rayY * rayNumerator, denominator);
        face = (long)rayX * sy - (long)rayY * sx >= 0
            ? SiegeWallFace.South
            : SiegeWallFace.North;
        return true;
    }

    private static bool WithinFraction(long numerator, long denominator) => denominator > 0
        ? numerator >= 0 && numerator <= denominator
        : numerator <= 0 && numerator >= denominator;

    private static bool FractionBefore(int leftNumerator, int leftDenominator,
        int rightNumerator, int rightDenominator) =>
        Math.Abs((long)leftNumerator * rightDenominator) <
        Math.Abs((long)rightNumerator * leftDenominator);

    private static int RoundedDivide(long numerator, long denominator)
    {
        if (denominator == 0) throw new DivideByZeroException();
        var adjustment = Math.Abs(denominator) / 2;
        if (numerator < 0) adjustment = -adjustment;
        return checked((int)((numerator + adjustment) / denominator));
    }

    private static bool NoHit(out int x, out int y, out SiegeWallFace face)
    {
        x = y = 0;
        face = default;
        return false;
    }

    private static int ForwardDistance8(Facing facing, int dx8, int dy8) => facing switch
    {
        Facing.North => -dy8,
        Facing.East => dx8,
        Facing.South => dy8,
        _ => -dx8
    };

    private static (int X, int Y) CardinalDirection(Facing facing) => facing switch
    {
        Facing.North => (0, -1),
        Facing.East => (1, 0),
        Facing.South => (0, 1),
        _ => (-1, 0)
    };
}
