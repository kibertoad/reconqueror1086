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

    public void AdvanceRetainerOrders()
    {
        foreach (var retainer in _retainers.Where(retainer => retainer.Health > 0).ToArray())
            AdvanceRetainerOrder(retainer);
    }

    private void AdvanceRetainerOrder(SiegeRetainer retainer)
    {
        if (retainer.VisualState != SiegeEnemyVisualState.Walk) return;
        if (retainer.OrderedDestination is { } destination)
        {
            if (retainer.X == destination.X && retainer.Y == destination.Y)
                retainer.OrderedDestination = null;
            return;
        }
        var target = RetainerOrderTarget(retainer);
        if (target is null) retainer.OrderedTarget = null;
        switch (retainer.Command)
        {
            case SiegeRetainerCommand.Defend:
                if (target is not null && IsNeighbor(retainer.X, retainer.Y, target.X, target.Y))
                    RetainerAttack(retainer, target);
                break;
            case SiegeRetainerCommand.Attack:
                var attackTarget = AttackOrderTarget(retainer);
                if (attackTarget is not null)
                {
                    if (retainer.OriginalActorKind == 1)
                        RetainerRangedAttack(retainer, attackTarget);
                    else
                        RetainerAttack(retainer, attackTarget);
                }
                break;
            case SiegeRetainerCommand.Follow:
                break;
            case SiegeRetainerCommand.Retreat:
                if (retainer.OriginalActorKind != 1)
                {
                    // After the public mode-10 decision, kind 0 owns a
                    // multi-state regroup/re-entry route. Do not collapse a
                    // later mode-1/3/4 decision to Defend merely because its
                    // hostile acquisition is temporarily empty.
                    if (retainer.RetreatMode == 0 && RetreatOrderTarget(retainer) is null)
                    {
                        retainer.Command = SiegeRetainerCommand.Defend;
                        retainer.OrderedTarget = null;
                    }
                    break;
                }
                var retreatTarget = RetreatOrderTarget(retainer);
                if (retreatTarget is null)
                {
                    retainer.Command = SiegeRetainerCommand.Defend;
                    retainer.OrderedTarget = null;
                }
                else
                    RetainerRangedAttack(retainer, retreatTarget);
                break;
        }
    }
}
