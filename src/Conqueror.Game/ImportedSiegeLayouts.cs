using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public static class ImportedSiegeLayouts
{
    private static readonly string[] CampaignScenes = ["MELEE0.RES", "MELEE1.RES", "MELEE2.RES"];

    public static SiegeLayout? ForCampaignLocation(ImportedContentCatalog? catalog, int location) =>
        Load(catalog, CampaignScenes[Math.Abs(location % CampaignScenes.Length)]);

    public static SiegeLayout? ForPracticeMelee(ImportedContentCatalog? catalog) => Load(catalog, "MELEE0.RES");
    public static SiegeLayout? ForPracticeCastleSkirmish(ImportedContentCatalog? catalog) => Load(catalog, "DEFEND0.RES");

    public static SiegeLayout Convert(DynamixScene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        var tiles = new SiegeTile[DynamixScene.MapWidth, DynamixScene.MapHeight];
        var enemies = new List<SiegeSpawn>();
        for (var x = 0; x < DynamixScene.MapWidth; x++)
        for (var y = 0; y < DynamixScene.MapHeight; y++)
        {
            var block = scene.BlockAt(x, y);
            tiles[x, y] = TileFor(block.Name);
            if (IsEnemy(block.Name))
                enemies.Add(new SiegeSpawn(x, y, block.Name.Contains("champion", StringComparison.OrdinalIgnoreCase)));
        }

        var heading = (scene.Viewer.Heading + 8192) / 16384 & 3;
        var facing = (Facing)heading;
        tiles[scene.Viewer.CellX, scene.Viewer.CellY] = SiegeTile.Floor;
        return new SiegeLayout(tiles, scene.Viewer.CellX, scene.Viewer.CellY, facing, enemies);
    }

    private static SiegeLayout? Load(ImportedContentCatalog? catalog, string name)
    {
        var scene = catalog?.DecodeScene($"CONQUER/{name}");
        return scene is null ? null : Convert(scene);
    }

    private static SiegeTile TileFor(string name)
    {
        if (name.Contains("secret passage", StringComparison.OrdinalIgnoreCase)) return SiegeTile.SecretDoor;
        if (name.Contains("door", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("portcullis", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Door;
        if (name.Contains("meal", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("barrel", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Barrel;
        if (name.Contains("coin", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("bolt", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("chalis", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("shield", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("ax", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("hauberk", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Treasure;
        if (IsEnemy(name) || name.Equals("ground", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("floor", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("carpet", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("dirt", StringComparison.OrdinalIgnoreCase) ||
            name.Contains("stairs", StringComparison.OrdinalIgnoreCase) ||
            name.Equals("exit", StringComparison.OrdinalIgnoreCase)) return SiegeTile.Floor;
        return SiegeTile.Wall;
    }

    private static bool IsEnemy(string name) =>
        name.Contains("knight", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("footman", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("bowman", StringComparison.OrdinalIgnoreCase) ||
        name.Contains("champion", StringComparison.OrdinalIgnoreCase);
}
