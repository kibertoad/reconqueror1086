using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public sealed record ImportedSiegeScene(
    string ArchiveId, DynamixScene Scene, DynamixSceneBackdrop? Backdrop,
    DynamixSceneColorMaps? ColorMaps, SiegeLayout Layout, int SourceOriginX, int SourceOriginY);

public static class ImportedSiegeLayouts
{
    public static string? SceneNameForCampaignLocation(int location)
    {
        if (location < 0 || location >= World.Locations.Length) return null;
        return World.Locations[location].Kind is LocationKind.Castle or LocationKind.London
            ? "MELEE0.RES"
            : null;
    }

    public static ImportedSiegeScene? ForCampaignLocation(ImportedContentCatalog? catalog, int location) =>
        SceneNameForCampaignLocation(location) is { } name ? Load(catalog, name) : null;

    public static string SceneNameForPracticeMelee(int variant) => variant switch
    {
        0 => "MELEE0.RES",
        1 => "MELEE1.RES",
        2 => "MELEE2.RES",
        _ => throw new ArgumentOutOfRangeException(nameof(variant))
    };

    public static ImportedSiegeScene? ForPracticeMelee(ImportedContentCatalog? catalog, int variant) =>
        Load(catalog, SceneNameForPracticeMelee(variant));
    public static ImportedSiegeScene? ForPracticeCastleSkirmish(ImportedContentCatalog? catalog) => Load(catalog, "DEFEND0.RES");

    public static SiegeLayout Convert(DynamixScene scene) => ConvertWithOrigin(scene).Layout;

    private static (SiegeLayout Layout, int OriginX, int OriginY) ConvertWithOrigin(DynamixScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var sourceTiles = new SiegeTile[DynamixScene.MapWidth, DynamixScene.MapHeight];
        for (var x = 0; x < DynamixScene.MapWidth; x++)
        for (var y = 0; y < DynamixScene.MapHeight; y++)
            sourceTiles[x, y] = TileFor(scene.BlockAt(x, y));
        sourceTiles[scene.Viewer.CellX, scene.Viewer.CellY] = SiegeTile.Floor;

        var reachable = ReachableFromViewer(sourceTiles, scene.Viewer.CellX, scene.Viewer.CellY);
        var points = Enumerable.Range(0, DynamixScene.MapWidth)
            .SelectMany(x => Enumerable.Range(0, DynamixScene.MapHeight).Select(y => (X: x, Y: y)))
            .Where(point => reachable[point.X, point.Y])
            .ToArray();
        var minX = Math.Max(0, points.Min(point => point.X) - 1);
        var maxX = Math.Min(DynamixScene.MapWidth - 1, points.Max(point => point.X) + 1);
        var minY = Math.Max(0, points.Min(point => point.Y) - 1);
        var maxY = Math.Min(DynamixScene.MapHeight - 1, points.Max(point => point.Y) + 1);
        var tiles = new SiegeTile[maxX - minX + 1, maxY - minY + 1];
        for (var x = minX; x <= maxX; x++)
        for (var y = minY; y <= maxY; y++)
            tiles[x - minX, y - minY] = reachable[x, y] ? sourceTiles[x, y] : SiegeTile.Wall;

        var enemies = points
            .Where(point => IsEnemy(scene.BlockAt(point.X, point.Y)))
            .Select(point => EnemyFor(scene.BlockAt(point.X, point.Y), point.X - minX, point.Y - minY))
            .ToArray();
        var objects = points
            .Where(point => IsSceneObject(scene.BlockAt(point.X, point.Y)))
            .Select(point => ObjectFor(scene, point.X, point.Y, minX, minY))
            .ToArray();
        var heading = (scene.Viewer.Heading + 8192) / 16384 & 3;
        var layout = new SiegeLayout(tiles, scene.Viewer.CellX - minX, scene.Viewer.CellY - minY,
            (Facing)heading, enemies, objects);
        return (layout, minX, minY);
    }

    private static bool[,] ReachableFromViewer(SiegeTile[,] tiles, int startX, int startY)
    {
        var reachable = new bool[tiles.GetLength(0), tiles.GetLength(1)];
        var pending = new Queue<(int X, int Y)>();
        reachable[startX, startY] = true;
        pending.Enqueue((startX, startY));
        while (pending.Count > 0)
        {
            var point = pending.Dequeue();
            if (tiles[point.X, point.Y] is SiegeTile.Exit or SiegeTile.Destructible) continue;
            foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
            {
                var x = point.X + dx;
                var y = point.Y + dy;
                if (x < 0 || y < 0 || x >= tiles.GetLength(0) || y >= tiles.GetLength(1) ||
                    reachable[x, y] || tiles[x, y] == SiegeTile.Wall) continue;
                reachable[x, y] = true;
                pending.Enqueue((x, y));
            }
        }
        return reachable;
    }

    private static ImportedSiegeScene? Load(ImportedContentCatalog? catalog, string name)
    {
        var archiveId = $"CONQUER/{name}";
        var scene = catalog?.DecodeScene(archiveId);
        if (scene is null) return null;
        var converted = ConvertWithOrigin(scene);
        return new ImportedSiegeScene(archiveId, scene, catalog!.DecodeSceneBackdrop(archiveId),
            catalog.DecodeSceneColorMaps(archiveId),
            converted.Layout, converted.OriginX, converted.OriginY);
    }

    private static SiegeTile TileFor(DynamixSceneBlock block)
    {
        var name = block.Name;
        // Across the owned MELEE*/DEFEND* population, behavior 83 is used only
        // by placed exit/gate markers. They are scene boundaries, not
        // members of the behavior-19 locked-door family.
        if (block.Behavior == 83) return SiegeTile.Exit;
        if (IsEnemy(block)) return SiegeTile.Floor;
        if (IsDestructible(block)) return SiegeTile.Destructible;
        // Placed kind-4 behavior-19 records are the scene pickups. Their names
        // distinguish food from equipment/currency while the metadata keeps an
        // unfamiliar pickup name from becoming a wall.
        if (block.Kind == 4 && block.Behavior == 19)
            return name.Contains("meal", StringComparison.OrdinalIgnoreCase)
                ? SiegeTile.Barrel
                : SiegeTile.Treasure;
        if (name.Contains("secret passage", StringComparison.OrdinalIgnoreCase)) return SiegeTile.SecretDoor;
        if (name.Contains("door", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("portcullis", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("gate", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Door;
        if (name.Contains("meal", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("barrel", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Barrel;
        if (name.Contains("coin", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("bolt", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("chalis", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("shield", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("ax", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("hauberk", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Treasure;
        if (block.Kind == 4) return SiegeTile.Floor;
        if (name.Equals("ground", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("floor", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("carpet", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("dirt", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("grass", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("flagstone", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("stairs", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("exit", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Floor;
        return SiegeTile.Wall;
    }

    private static bool IsEnemy(DynamixSceneBlock block) =>
        block.Behavior == 135 || IsEnemyName(block.Name);

    private static SiegeSpawn EnemyFor(DynamixSceneBlock block, int x, int y)
    {
        if (block.ActorCombatRow is < 0 or >= OriginalWeaponCombat.CombatRowCount)
            throw new InvalidDataException($"Scene actor {block.Index} references an unknown combat row.");
        OriginalCombatantTemplate template;
        try { template = OriginalCombatantTemplates.For(block.ActorTemplate); }
        catch (ArgumentOutOfRangeException exception)
        {
            throw new InvalidDataException($"Scene actor {block.Index} references an unknown combatant template.", exception);
        }
        var champion = block.Name.Contains("champion", StringComparison.OrdinalIgnoreCase) ||
            block.Name.Contains("lord", StringComparison.OrdinalIgnoreCase);
        return new SiegeSpawn(x, y, champion, block.Index, template.Armor, template.Health,
            block.ActorCombatRow, template.AttackSkill);
    }

    private static bool IsDestructible(DynamixSceneBlock block) =>
        block.Kind == 4 && (block.Behavior & 0x20) != 0;

    private static bool IsSceneObject(DynamixSceneBlock block) =>
        block.Kind == 4 && !IsEnemy(block) && block.Behavior != 83;

    private static SiegeObjectSpawn ObjectFor(DynamixScene scene, int x, int y, int minX, int minY)
    {
        var initial = scene.BlockAt(x, y);
        var initialTile = TileFor(initial);
        var stages = new List<SiegeObjectStage> { new(initial.Index, initialTile) };
        if (initialTile is SiegeTile.Destructible or SiegeTile.Barrel or SiegeTile.Treasure)
        {
            if ((uint)initial.StateTarget >= (uint)scene.Blocks.Count)
                throw new InvalidDataException($"Scene object {initial.Index} references a state outside the block table.");
            var target = scene.Blocks[initial.StateTarget];
            stages.Add(new(target.Kind == 4 ? target.Index : -1, TileFor(target)));
        }
        return new SiegeObjectSpawn(x - minX, y - minY, stages);
    }

    private static bool IsEnemyName(string name) =>
        name.Contains("knight", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("footman", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("bowman", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("champion", StringComparison.OrdinalIgnoreCase);
}
