using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

/// <summary>
/// Computes the complete texture population used by the imported first-person
/// renderer. The original loader can replace an actor's authored walk family
/// with the player's heraldic family, so placed-block references alone are not
/// a sufficient render dependency set.
/// </summary>
public static class SiegeTextureDependencies
{
    public static IReadOnlySet<int> AcquisitionTextures(DynamixScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var required = new HashSet<int>();
        var visited = new HashSet<int>();
        var pending = new Stack<DynamixSceneBlock>();
        for (var x = 0; x < DynamixScene.MapWidth; x++)
        for (var y = 0; y < DynamixScene.MapHeight; y++)
            pending.Push(scene.BlockAt(x, y));

        while (pending.TryPop(out var block))
        {
            if (!visited.Add(block.Index)) continue;
            required.UnionWith(block.RaycastTextureReferences());
            if ((block.Behavior & 1) == 0
                || block.Kind != 4 && (block.Behavior & 8) == 0
                || (uint)block.StateTarget >= (uint)scene.Blocks.Count) continue;
            var target = scene.Blocks[block.StateTarget];
            if (target.Kind != 0) pending.Push(target);
        }
        required.RemoveWhere(texture => texture < 0);
        return required;
    }

    public static IReadOnlySet<int> RenderTextures(
        DynamixScene scene,
        SiegeLayout layout,
        int sourceOriginX,
        int sourceOriginY,
        string? heraldicColor)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(layout);
        var required = new HashSet<int>();

        var tiles = layout.CopyTiles();
        for (var x = 0; x < tiles.GetLength(0); x++)
        for (var y = 0; y < tiles.GetLength(1); y++)
            Add(scene.BlockAt(x + sourceOriginX, y + sourceOriginY).TextureReferences());

        foreach (var stage in layout.Objects.SelectMany(item => item.Stages))
        {
            if (stage.VisualId < 0) continue;
            Add(BlockAt(stage.VisualId).TextureReferences());
        }

        foreach (var spawn in layout.Enemies)
            AddActor(spawn, friendly: false);
        foreach (var spawn in layout.Retainers)
            AddActor(spawn, friendly: true);

        return required;

        void AddActor(SiegeSpawn spawn, bool friendly)
        {
            var initial = BlockAt(spawn.VisualId);
            if (initial.Kind != 4 || initial.Surface0 < 0)
                throw new InvalidDataException(
                    $"Scene actor block {spawn.VisualId} has no billboard texture family.");

            var colors = SiegeActorColorMapping.Normalize(
                heraldicColor, friendly, initial.Flags, initial.Surface0);
            var walk = initial with { Surface0 = colors.WalkTextureBase };
            var frameCount = spawn.OriginalMovement?.TickCount ?? 3;
            var stride = spawn.OriginalMovement?.SurfaceIndexDeltaPerTick is > 0 and <= 4096
                ? spawn.OriginalMovement.SurfaceIndexDeltaPerTick
                : initial.Surface2 / 2 + 1;
            for (var frame = 0; frame < frameCount; frame++)
                Add(Enumerable.Range(0, 256)
                    .Select(heading => walk.TextureForBillboardHeading(heading).TextureIndex
                        + frame * stride));

            for (var stateOffset = 1; stateOffset <= 3; stateOffset++)
            {
                var stateIndex = initial.Index + stateOffset;
                if (stateIndex >= scene.Blocks.Count) continue;
                var state = scene.Blocks[stateIndex];
                if (state.Kind == 4
                    && state.Name.Equals(initial.Name, StringComparison.OrdinalIgnoreCase)
                    && state.Surface0 >= 0)
                    Add(state.RaycastTextureReferences());
            }
        }

        DynamixSceneBlock BlockAt(int index) => index >= 0 && index < scene.Blocks.Count
            ? scene.Blocks[index]
            : throw new InvalidDataException($"Scene visual references block {index} outside the block table.");

        void Add(IEnumerable<int> references)
        {
            foreach (var texture in references.Where(texture => texture >= 0))
                required.Add(texture);
        }
    }
}
