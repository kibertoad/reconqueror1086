using Conqueror.Core;

namespace Conqueror.Game;

public enum PracticeCombatKind
{
    War,
    Melee,
    CastleSkirmish
}

/// <summary>
/// Isolated adapters from the original Practice menu into the current tactical
/// and first-person engines. These sessions never receive the active campaign's
/// Player or Army instances, so damage, loot, and survivors cannot leak into it.
/// </summary>
public static class PracticeCombatDefinitions
{
    public static FieldBattleSession CreateWar(int seed = 1086)
    {
        var friendly = Army(12, 12, 8);
        var enemy = Army(10, 14, 8);
        return new FieldBattleSession(friendly, enemy, seed);
    }

    public static SiegeSession CreateMelee(int seed = 1086, SiegeLayout? layout = null) =>
        new(CreateParticipant(), Army(8, 0, 0), garrison: 6, seed: seed, layout: layout);

    public static SiegeSession CreateCastleSkirmish(int seed = 1086, SiegeLayout? layout = null) =>
        new(CreateParticipant(), Army(10, 4, 2), garrison: 18, seed: seed, layout: layout);

    private static Player CreateParticipant() => new()
    {
        Name = "Practice Knight",
        Stats = new CharacterStats(10, 10, 10, 10, 10)
    };

    private static Army Army(int swordsmen, int halberdiers, int knights)
    {
        var army = new Army();
        army.Units[UnitType.Swordsmen] = swordsmen;
        army.Units[UnitType.Halberdiers] = halberdiers;
        army.Units[UnitType.Knights] = knights;
        return army;
    }
}
