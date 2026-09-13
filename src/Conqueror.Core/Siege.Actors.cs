namespace Conqueror.Core;

public sealed partial class SiegeSession
{
    private SiegeRetainer RetainerFor(SiegeSpawn spawn) => new()
    {
        X = spawn.X,
        Y = spawn.Y,
        Health = spawn.OriginalHealth ?? 1,
        OriginalArmor = spawn.OriginalArmor,
        OriginalAttackSkill = spawn.OriginalAttackSkill,
        OriginalCombatRow = spawn.OriginalCombatRow,
        OriginalActorKind = spawn.OriginalActorKind,
        OriginalActorOrder = spawn.OriginalActorOrder,
        VisualId = spawn.VisualId,
        OriginalAnimation = spawn.OriginalAnimation,
        OriginalMovement = spawn.OriginalMovement,
        OffsetX8 = spawn.InitialOffsetX8,
        OffsetY8 = spawn.InitialOffsetY8,
        Facing = DirectionToward(spawn.X, spawn.Y, PlayerX, PlayerY, Facing.South)
    };

    private static SiegeEnemy ActorFor(SiegeSpawn? spawn, int x, int y, int health, Facing facing) => new()
    {
        X = x,
        Y = y,
        Health = health,
        OriginalArmor = spawn?.OriginalArmor,
        OriginalAttackSkill = spawn?.OriginalAttackSkill,
        OriginalCombatRow = spawn?.OriginalCombatRow,
        OriginalActorKind = spawn?.OriginalActorKind,
        OriginalActorOrder = spawn?.OriginalActorOrder ?? -1,
        VisualId = spawn?.VisualId ?? -1,
        OriginalAnimation = spawn?.OriginalAnimation,
        OriginalMovement = spawn?.OriginalMovement,
        OffsetX8 = spawn?.InitialOffsetX8 ?? 0,
        OffsetY8 = spawn?.InitialOffsetY8 ?? 0,
        Facing = facing
    };

    private void SyncPlayerActor()
    {
        PlayerActor.X = PlayerX;
        PlayerActor.Y = PlayerY;
        PlayerActor.Health = Health;
        PlayerActor.Facing = Facing;
    }
}
