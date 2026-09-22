namespace Conqueror.Core;

/// <summary>
/// Provides the provisional core-only room and strength for the confirmed
/// Drogo debt encounter. The desktop runtime supplies its decoded MONEY.RES
/// scene through Campaign.CreateDrogoBattle.
/// </summary>
public static class DrogoEncounterDefinitions
{
    public static SiegeSession Create(Player player, int seed)
    {
        ArgumentNullException.ThrowIfNull(player);
        var tiles = new SiegeTile[5, 5];
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 5; y++)
            tiles[x, y] = x is 0 or 4 || y is 0 or 4 ? SiegeTile.Wall : SiegeTile.Floor;
        var layout = new SiegeLayout(tiles, 1, 2, Facing.East,
            [new SiegeSpawn(2, 2, Champion: true)]);
        return new SiegeSession(player, new Army(), garrison: 0, seed, layout, includeRetainers: false);
    }
}
