using Conqueror.Core;

namespace Conqueror.Game;

public enum SiegeWallFace { North, East, South, West }

public readonly record struct SiegeRayHit(
    double Distance, SiegeTile Tile, bool HitVerticalSide, SiegeWallFace Face,
    int MapX, int MapY, double TextureOffset);
public readonly record struct SiegeEnemyProjection(double ScreenPosition, double ForwardDistance, SiegeEnemy Enemy);
public readonly record struct SiegeObjectProjection(double ScreenPosition, double ForwardDistance, SiegeObject Object);
public readonly record struct SiegeEnemyFrame(int DirectionOffset, bool FlipHorizontally);

public static class SiegeViewProjection
{
    public const double FieldOfView = Math.PI / 3.0;
    public const double MaximumDistance = 48.0;

    public static SiegeRayHit Cast(SiegeSession siege, double cameraPosition)
    {
        ArgumentNullException.ThrowIfNull(siege);
        var (forwardX, forwardY) = Direction(siege.Facing);
        var angle = Math.Atan2(forwardY, forwardX) + cameraPosition * FieldOfView / 2.0;
        var rayX = Math.Cos(angle);
        var rayY = Math.Sin(angle);
        var mapX = siege.PlayerX;
        var mapY = siege.PlayerY;
        var originX = siege.PlayerX + 0.5;
        var originY = siege.PlayerY + 0.5;
        var deltaX = rayX == 0 ? double.PositiveInfinity : Math.Abs(1.0 / rayX);
        var deltaY = rayY == 0 ? double.PositiveInfinity : Math.Abs(1.0 / rayY);
        var stepX = rayX < 0 ? -1 : 1;
        var stepY = rayY < 0 ? -1 : 1;
        var sideX = rayX < 0 ? (originX - mapX) * deltaX : (mapX + 1.0 - originX) * deltaX;
        var sideY = rayY < 0 ? (originY - mapY) * deltaY : (mapY + 1.0 - originY) * deltaY;

        for (var step = 0; step < 256; step++)
        {
            bool vertical;
            double distance;
            if (sideX < sideY)
            {
                distance = sideX;
                sideX += deltaX;
                mapX += stepX;
                vertical = true;
            }
            else
            {
                distance = sideY;
                sideY += deltaY;
                mapY += stepY;
                vertical = false;
            }

            var tile = siege.TileAt(mapX, mapY);
            if (!IsSolid(tile) && distance < MaximumDistance) continue;
            var corrected = Math.Max(0.01, Math.Min(distance, MaximumDistance) * Math.Cos(cameraPosition * FieldOfView / 2.0));
            var textureOffset = vertical ? originY + rayY * distance : originX + rayX * distance;
            textureOffset -= Math.Floor(textureOffset);
            if ((vertical && rayX > 0) || (!vertical && rayY < 0)) textureOffset = 1.0 - textureOffset;
            var face = vertical
                ? stepX > 0 ? SiegeWallFace.West : SiegeWallFace.East
                : stepY > 0 ? SiegeWallFace.North : SiegeWallFace.South;
            return new SiegeRayHit(corrected, tile, vertical, face, mapX, mapY, textureOffset);
        }

        return new SiegeRayHit(MaximumDistance, SiegeTile.Wall, false, SiegeWallFace.North, mapX, mapY, 0);
    }

    public static IReadOnlyList<SiegeEnemyProjection> ProjectEnemies(SiegeSession siege)
    {
        ArgumentNullException.ThrowIfNull(siege);
        return ProjectActors(siege, siege.Enemies);
    }

    public static IReadOnlyList<SiegeEnemyProjection> ProjectRetainers(SiegeSession siege)
    {
        ArgumentNullException.ThrowIfNull(siege);
        return ProjectActors(siege, siege.Retainers);
    }

    private static IReadOnlyList<SiegeEnemyProjection> ProjectActors(
        SiegeSession siege, IEnumerable<SiegeEnemy> actors)
    {
        var (forwardX, forwardY) = Direction(siege.Facing);
        var rightX = -forwardY;
        var rightY = forwardX;
        var result = new List<SiegeEnemyProjection>();
        foreach (var enemy in actors.Where(actor => actor.Health > 0))
        {
            var dx = enemy.X - siege.PlayerX;
            var dy = enemy.Y - siege.PlayerY;
            var forward = dx * forwardX + dy * forwardY;
            if (forward <= 0.05 || forward > MaximumDistance) continue;
            var lateral = dx * rightX + dy * rightY;
            var screen = 0.5 + lateral / (forward * 2.0 * Math.Tan(FieldOfView / 2.0));
            if (screen is < -0.25 or > 1.25) continue;
            result.Add(new SiegeEnemyProjection(screen, forward, enemy));
        }
        return result.OrderByDescending(item => item.ForwardDistance).ToArray();
    }

    public static IReadOnlyList<SiegeObjectProjection> ProjectObjects(SiegeSession siege)
    {
        ArgumentNullException.ThrowIfNull(siege);
        var (forwardX, forwardY) = Direction(siege.Facing);
        var rightX = -forwardY;
        var rightY = forwardX;
        var result = new List<SiegeObjectProjection>();
        foreach (var item in siege.Objects)
        {
            if (item.VisualId < 0 || item.Tile is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor) continue;
            var dx = item.X - siege.PlayerX;
            var dy = item.Y - siege.PlayerY;
            var forward = dx * forwardX + dy * forwardY;
            if (forward <= 0.05 || forward > MaximumDistance) continue;
            var lateral = dx * rightX + dy * rightY;
            var screen = 0.5 + lateral / (forward * 2.0 * Math.Tan(FieldOfView / 2.0));
            if (screen is < -0.25 or > 1.25) continue;
            result.Add(new SiegeObjectProjection(screen, forward, item));
        }
        return result.OrderByDescending(item => item.ForwardDistance).ToArray();
    }

    public static SiegeEnemyFrame FrameFor(SiegeEnemy enemy, int viewerX, int viewerY)
    {
        ArgumentNullException.ThrowIfNull(enemy);
        var (forwardX, forwardY) = Direction(enemy.Facing);
        var viewX = viewerX - enemy.X;
        var viewY = viewerY - enemy.Y;
        if (viewX == 0 && viewY == 0) return new SiegeEnemyFrame(4, false);
        var length = Math.Sqrt(viewX * viewX + viewY * viewY);
        var dot = Math.Clamp((forwardX * viewX + forwardY * viewY) / length, -1, 1);
        var angle = Math.Acos(dot);
        var directionOffset = Math.Clamp(4 - (int)Math.Round(angle / (Math.PI / 4.0)), 0, 4);
        var cross = forwardX * viewY - forwardY * viewX;
        return new SiegeEnemyFrame(directionOffset, cross > 0);
    }

    private static bool IsSolid(SiegeTile tile) =>
        tile is SiegeTile.Wall or SiegeTile.Door or SiegeTile.SecretDoor or SiegeTile.Exit;

    private static (double X, double Y) Direction(Facing facing) => facing switch
    {
        Facing.North => (0, -1),
        Facing.East => (1, 0),
        Facing.South => (0, 1),
        _ => (-1, 0)
    };
}
