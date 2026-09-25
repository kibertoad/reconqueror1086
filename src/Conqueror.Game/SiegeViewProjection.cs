using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public enum SiegeWallFace { North, East, South, West }

public readonly record struct SiegeRayHit(
    double Distance, SiegeTile Tile, bool HitVerticalSide, SiegeWallFace Face,
    int MapX, int MapY, double TextureOffset)
{
    public int Distance8 { get; init; }
    public int ContactX8 { get; init; }
    public int ContactY8 { get; init; }
    public bool ContactedBlock { get; init; }
    public int SceneBlockIndex { get; init; } = -1;
}
public readonly record struct SiegeEnemyProjection(double ScreenPosition, double ForwardDistance, SiegeEnemy Enemy);
public readonly record struct SiegeObjectProjection(double ScreenPosition, double ForwardDistance, SiegeObject Object);
public readonly record struct SiegeEnemyFrame(int DirectionOffset, bool FlipHorizontally);
public readonly record struct SiegeBillboardLayout(int Left, int Top, int Width, int Height)
{
    public bool Contains(int x, int y) => x >= Left && x < Left + Width && y >= Top && y < Top + Height;
}

public static class SiegeViewProjection
{
    public const double MaximumDistance = 48.0;
    private const int OriginalCameraHeight8 = 0x80;
    private const int OriginalRayForward14 = 0x4000;
    private const int OriginalRayTraversalSteps = 0x40;

    public static SiegeRayHit Cast(SiegeSession siege, double cameraPosition)
    {
        ArgumentNullException.ThrowIfNull(siege);
        if (!double.IsFinite(cameraPosition) || cameraPosition is < -1 or > 1)
            throw new ArgumentOutOfRangeException(nameof(cameraPosition));
        return CastFixed(siege, (int)Math.Round(cameraPosition * 0x2000));
    }

    public static SiegeRayHit CastColumn(SiegeSession siege, int column, int viewportWidth)
    {
        ArgumentNullException.ThrowIfNull(siege);
        if (viewportWidth <= 1) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (column < 0 || column >= viewportWidth) throw new ArgumentOutOfRangeException(nameof(column));
        var lateral14 = (0x400000 / viewportWidth * (column - viewportWidth / 2)) >> 8;
        return CastFixed(siege, lateral14);
    }

    private static SiegeRayHit CastFixed(SiegeSession siege, int lateral14)
    {
        var (forwardX, forwardY) = CardinalDirection(siege.Facing);
        var rightX = -forwardY;
        var rightY = forwardX;
        var rayX = forwardX * OriginalRayForward14 + rightX * lateral14;
        var rayY = forwardY * OriginalRayForward14 + rightY * lateral14;
        var originX8 = (siege.PlayerX << 8) + 0x80;
        var originY8 = (siege.PlayerY << 8) + 0x80;
        var mapX = siege.PlayerX;
        var mapY = siege.PlayerY;
        var stepX = Math.Sign(rayX);
        var stepY = Math.Sign(rayY);
        var absoluteX = Math.Abs(rayX);
        var absoluteY = Math.Abs(rayY);
        var numeratorX = stepX < 0 ? originX8 - (mapX << 8) : ((mapX + 1) << 8) - originX8;
        var numeratorY = stepY < 0 ? originY8 - (mapY << 8) : ((mapY + 1) << 8) - originY8;

        for (var step = 0; step < OriginalRayTraversalSteps; step++)
        {
            var crossX = stepX == 0 ? long.MaxValue : (long)numeratorX * absoluteY;
            var crossY = stepY == 0 ? long.MaxValue : (long)numeratorY * absoluteX;
            var vertical = crossX < crossY;
            int contactX8;
            int contactY8;
            SiegeWallFace face;
            if (vertical)
            {
                var boundaryX8 = stepX > 0 ? (mapX + 1) << 8 : mapX << 8;
                contactX8 = stepX > 0 ? boundaryX8 : boundaryX8 - 1;
                contactY8 = originY8 + (int)((long)rayY * (boundaryX8 - originX8) / rayX);
                mapX += stepX;
                numeratorX += 0x100;
                face = stepX > 0 ? SiegeWallFace.West : SiegeWallFace.East;
            }
            else
            {
                var boundaryY8 = stepY > 0 ? (mapY + 1) << 8 : mapY << 8;
                contactY8 = stepY > 0 ? boundaryY8 : boundaryY8 - 1;
                contactX8 = originX8 + (int)((long)rayX * (boundaryY8 - originY8) / rayY);
                mapY += stepY;
                numeratorY += 0x100;
                face = stepY > 0 ? SiegeWallFace.North : SiegeWallFace.South;
            }

            var tile = siege.TileAt(mapX, mapY);
            if (!IsSolid(tile)) continue;
            var distance8 = (contactX8 - originX8) * forwardX + (contactY8 - originY8) * forwardY;
            var coordinate = vertical ? contactY8 & 0xff : contactX8 & 0xff;
            if (face is SiegeWallFace.North or SiegeWallFace.East) coordinate = 0xff - coordinate;
            return new SiegeRayHit(
                Math.Max(0.01, distance8 / 256d), tile, vertical, face, mapX, mapY, coordinate / 256d)
            {
                Distance8 = distance8,
                ContactX8 = contactX8,
                ContactY8 = contactY8,
                ContactedBlock = true
            };
        }

        var fallbackDistance8 = OriginalRayTraversalSteps << 8;
        return new SiegeRayHit(
            OriginalRayTraversalSteps, SiegeTile.Wall, false, SiegeWallFace.North, mapX, mapY, 0)
        {
            Distance8 = fallbackDistance8,
            ContactX8 = originX8 + forwardX * fallbackDistance8,
            ContactY8 = originY8 + forwardY * fallbackDistance8
        };
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
            // Kind-4 blocks keep signed 8.8 offsets at +0x24/+0x28. The
            // scheduler moves them between cells before updating the owning
            // cell index, so drawing and picking must use the same sub-cell point.
            var dx = enemy.X - siege.PlayerX + enemy.OffsetX8 / 256d;
            var dy = enemy.Y - siege.PlayerY + enemy.OffsetY8 / 256d;
            var forward = dx * forwardX + dy * forwardY;
            if (forward <= 0.05 || forward > MaximumDistance) continue;
            var lateral = dx * rightX + dy * rightY;
            // RULE-VIEW-003: the viewer uses forward basis 0x4000 and one
            // viewport width of lateral basis. This is its inverse.
            var screen = 0.5 + lateral / forward;
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
            var screen = 0.5 + lateral / forward;
            if (screen is < -0.25 or > 1.25) continue;
            result.Add(new SiegeObjectProjection(screen, forward, item));
        }
        return result.OrderByDescending(item => item.ForwardDistance).ToArray();
    }

    public static SiegeBillboardLayout ActorLayout(
        SiegeEnemyProjection projection, int viewportWidth, int viewportHeight,
        int? textureWidth = null, int? textureHeight = null)
    {
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        ValidateTextureDimensions(textureWidth, textureHeight);
        var wallHeight = viewportHeight / projection.ForwardDistance;
        var height = textureHeight is null
            ? Math.Clamp((int)(wallHeight * 0.75), 20, viewportHeight)
            : Math.Clamp((int)(wallHeight * textureHeight.Value / 256.0), 20, viewportHeight);
        var width = textureWidth is null
            ? Math.Max(10, height / 2)
            : Math.Max(10, height * textureWidth.Value / textureHeight!.Value);
        return BillboardLayout(projection.ScreenPosition, wallHeight, width, height, viewportWidth, viewportHeight);
    }

    public static SiegeBillboardLayout ActorLayout(
        SiegeEnemyProjection projection, DynamixSceneBlock block,
        int viewportWidth, int viewportHeight)
    {
        ArgumentNullException.ThrowIfNull(block);
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        var depth8 = Math.Max(0x10, (int)Math.Round(projection.ForwardDistance * 256));
        var center = (int)Math.Round(projection.ScreenPosition * viewportWidth);
        var width = Math.Max(1, viewportWidth * 256 / depth8);
        var horizon = viewportHeight / 2;
        var top = horizon - (block.UpperElevation - OriginalCameraHeight8) * viewportWidth / depth8;
        var bottom = horizon + (OriginalCameraHeight8 - block.LowerElevation) * viewportWidth / depth8;
        return new SiegeBillboardLayout(
            center - width / 2, top, width, Math.Max(1, bottom - top + 1));
    }

    public static SiegeBillboardLayout ObjectLayout(
        SiegeObjectProjection projection, int viewportWidth, int viewportHeight,
        int? textureWidth = null, int? textureHeight = null)
    {
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        ValidateTextureDimensions(textureWidth, textureHeight);
        var wallHeight = viewportHeight / projection.ForwardDistance;
        var height = textureHeight is null
            ? Math.Clamp((int)(wallHeight * 0.6), 12, viewportHeight)
            : Math.Clamp((int)(wallHeight * textureHeight.Value / 256.0), 12, viewportHeight);
        var width = textureWidth is null
            ? Math.Max(8, height / 2)
            : Math.Max(8, height * textureWidth.Value / textureHeight!.Value);
        return BillboardLayout(projection.ScreenPosition, wallHeight, width, height, viewportWidth, viewportHeight);
    }

    public static (int X, int Y)? GroundCell(
        SiegeSession siege, int pointerX, int pointerY, int viewportWidth, int viewportHeight)
    {
        ArgumentNullException.ThrowIfNull(siege);
        if (viewportWidth <= 1) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 1) throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        if (pointerX < 0 || pointerY < 0 || pointerX >= viewportWidth || pointerY >= viewportHeight)
            return null;
        var hit = CastColumn(siege, pointerX, viewportWidth);
        if (!hit.ContactedBlock || hit.Distance8 <= 0) return null;
        // The common official wall span is 0..0x100 around viewer elevation
        // 0x80. Full block-specific bounds and alpha are applied by the game.
        var horizon = viewportHeight / 2;
        var halfHeight = OriginalCameraHeight8 * viewportWidth / hit.Distance8;
        if (pointerY < horizon - halfHeight || pointerY > horizon + halfHeight) return null;
        return (hit.ContactX8 >> 8, hit.ContactY8 >> 8);
    }

    private static SiegeBillboardLayout BillboardLayout(
        double screenPosition, double wallHeight, int width, int height, int viewportWidth, int viewportHeight)
    {
        var center = (int)(screenPosition * viewportWidth);
        var floor = viewportHeight / 2 + (int)(wallHeight / 2);
        return new SiegeBillboardLayout(center - width / 2, floor - height, width, height);
    }

    private static void ValidateTextureDimensions(int? width, int? height)
    {
        if (width.HasValue != height.HasValue || width is <= 0 || height is <= 0)
            throw new ArgumentException("Texture dimensions must both be positive or both be omitted.");
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

    private static (int X, int Y) CardinalDirection(Facing facing) => facing switch
    {
        Facing.North => (0, -1),
        Facing.East => (1, 0),
        Facing.South => (0, 1),
        _ => (-1, 0)
    };
}
