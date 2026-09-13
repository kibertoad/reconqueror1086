using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public static class OriginalSiegeActorAcquisition
{
    public static SiegeActorRayHit? CastToward(
        SiegeSession siege,
        DynamixScene scene,
        int sourceOriginX,
        int sourceOriginY,
        SiegeEnemy source,
        SiegeEnemy target,
        Func<int, int, SiegeProjectedBlock?> activeBlockAt,
        Func<int, DynamixSceneTexture?> textureAt,
        int viewportWidth = 167,
        int viewportHeight = 117)
    {
        ArgumentNullException.ThrowIfNull(siege);
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(activeBlockAt);
        ArgumentNullException.ThrowIfNull(textureAt);
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));

        var sourceX8 = (source.X << 8) + 0x80 + source.OffsetX8;
        var sourceY8 = (source.Y << 8) + 0x80 + source.OffsetY8;
        var targetX8 = (target.X << 8) + 0x80 + target.OffsetX8;
        var targetY8 = (target.Y << 8) + 0x80 + target.OffsetY8;
        var heading = OriginalSiegeProjection.HeadingToward(
            targetX8 - sourceX8, targetY8 - sourceY8);
        var candidates = OriginalSiegeProjection.CastHeading(
            siege, scene, sourceOriginX, sourceOriginY,
            sourceX8, sourceY8, heading, activeBlockAt);
        var horizon = viewportHeight / 2;
        foreach (var candidate in candidates)
        {
            if (candidate.Hit.Distance8 <= 0) continue;
            var textureIndex = candidate.Block.Kind == 4
                ? candidate.TextureIndex
                : candidate.Block.TextureForFace(ToSceneFace(candidate.Hit.Face));
            if (textureIndex < 0) continue;
            var texture = textureAt(textureIndex) ?? throw new InvalidDataException(
                $"Acquisition ray references missing original scene texture {textureIndex}.");
            var textureX = candidate.Block.Kind == 4
                ? candidate.TextureX
                : TextureX(candidate.TextureCoordinate8, candidate.Block, texture.Width);
            if (!OpaqueAtHorizon(candidate, texture, textureX, horizon, viewportWidth)) continue;
            return candidate.Actor is { } actor
                ? new SiegeActorRayHit(actor, candidate.Hit.Distance8)
                : null;
        }
        return null;
    }

    private static bool OpaqueAtHorizon(
        SiegeSceneRayCandidate candidate,
        DynamixSceneTexture texture,
        int textureX,
        int horizon,
        int viewportWidth)
    {
        if (textureX < 0 || textureX >= texture.Width) return false;
        var depth8 = Math.Max(0x10, candidate.Hit.Distance8);
        var top = horizon - (candidate.Block.UpperElevation - 0x80) * viewportWidth / depth8;
        var bottom = horizon + (0x80 - candidate.Block.LowerElevation) * viewportWidth / depth8;
        if (horizon < top || horizon > bottom) return false;
        var height = bottom - top + 1;
        if (height <= 0) return false;
        var textureY = Math.Clamp((horizon - top) * texture.Height / height, 0, texture.Height - 1);
        return texture.Indices[textureY * texture.Width + textureX] != 0;
    }

    private static int TextureX(int coordinate8, DynamixSceneBlock block, int decodedWidth)
    {
        if (coordinate8 is < 0 or > 0xff) return -1;
        if (block.TextureWidthShift is >= 0 and <= 8)
            return coordinate8 >> (8 - block.TextureWidthShift);
        return coordinate8 * decodedWidth >> 8;
    }

    private static DynamixSceneFace ToSceneFace(SiegeWallFace face) => face switch
    {
        SiegeWallFace.North => DynamixSceneFace.North,
        SiegeWallFace.East => DynamixSceneFace.East,
        SiegeWallFace.South => DynamixSceneFace.South,
        _ => DynamixSceneFace.West
    };
}
