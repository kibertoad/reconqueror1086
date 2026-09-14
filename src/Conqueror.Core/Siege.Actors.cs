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
        OriginalActorTemplate = spawn.OriginalActorTemplate,
        OriginalModeProfile = spawn.OriginalActorTemplate is { } actorTemplate
            ? OriginalCombatantTemplates.ModeProfileForSceneTemplate(actorTemplate)
            : null,
        ActorMode = spawn.OriginalActorTemplate is { } initialTemplate
            ? OriginalCombatantTemplates.ModeProfileForSceneTemplate(initialTemplate).Current
            : (int)SiegeRetainerCommand.Attack,
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
        OriginalActorTemplate = spawn?.OriginalActorTemplate,
        OriginalModeProfile = spawn?.OriginalActorTemplate is { } actorTemplate
            ? OriginalCombatantTemplates.ModeProfileForSceneTemplate(actorTemplate)
            : null,
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
        if (retainer.VisualState != SiegeEnemyVisualState.Walk || retainer.MovementActive) return;
        if (retainer.OrderedDestination is { } destination)
        {
            if (retainer.X == destination.X && retainer.Y == destination.Y)
                retainer.OrderedDestination = null;
            return;
        }
        switch (retainer.Command)
        {
            case SiegeRetainerCommand.Defend:
                AdvanceDefendOrder(retainer);
                break;
            case SiegeRetainerCommand.Attack:
                AdvanceAttackOrder(retainer);
                break;
            case SiegeRetainerCommand.Follow:
                AdvanceFollowOrder(retainer);
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
                    else if (retainer.RetreatMode == 11 &&
                        retainer.OrderedTarget is { Health: > 0 } meleeTarget)
                        RetainerRangedAttack(retainer, meleeTarget);
                    break;
                }
                // Kind 1 does not jump directly from requested mode 10 to a
                // shot. It uses its ordinary table one predicate at a time:
                // 10 -> 4/2, then 4 -> 11/1. The same route owns mode 13
                // recovery after a hit.
                AdvanceKindOneRetreatOrder(retainer);
                break;
        }
    }

    private void AdvanceKindOneRetreatOrder(SiegeRetainer retainer)
    {
        var previousMode = retainer.ActorMode;
        AdvanceDefendOrder(retainer);
        if (previousMode == 10 && retainer.ActorMode == 2)
        {
            retainer.Command = SiegeRetainerCommand.Defend;
            retainer.OrderedTarget = null;
        }
    }

    private void AdvanceDefendOrder(SiegeRetainer retainer)
    {
        // Friendly mode tables begin at object-1 offsets 0x3E43C (kind 0)
        // and 0x3E480 (kind 1). Like 0x4F49C, evaluate one predicate and
        // invoke only the selected mode handler during this thinker pass.
        switch (retainer.ActorMode)
        {
            case 1:
            {
                var ally = AdjacentFriendlyActor(retainer);
                if (ally is not null) retainer.RetreatRegroupTarget = ally;
                retainer.ActorMode = ally is null
                    ? retainer.OriginalActorKind == 1 ? 2 : 3
                    : 4;
                break;
            }
            case 2:
            {
                var opponent = AdjacentOpponentActor(retainer);
                if (opponent is not null) StoreFriendlyTarget(retainer, opponent);
                retainer.ActorMode = opponent is null ? 1 : retainer.OriginalActorKind == 1 ? 10 : 11;
                break;
            }
            case 3:
            {
                var ally = RetreatFormationTarget(retainer);
                if (ally is not null) retainer.RetreatRegroupTarget = ally;
                retainer.ActorMode = ally is null ? 2 : 7;
                break;
            }
            case 4:
            {
                var opponent = AcquireRetreatOrderTarget(retainer);
                if (opponent is not null) StoreFriendlyTarget(retainer, opponent);
                retainer.ActorMode = opponent is null ? 1 : retainer.OriginalActorKind == 1 ? 11 : 8;
                break;
            }
            case 5:
            {
                var ally = RetreatFormationTarget(retainer);
                if (ally is not null) retainer.RetreatRegroupTarget = ally;
                retainer.ActorMode = ally is null ? 5 : 7;
                break;
            }
            case 6:
            {
                var opponent = AcquireRetreatOrderTarget(retainer);
                if (opponent is not null) StoreFriendlyTarget(retainer, opponent);
                retainer.ActorMode = opponent is null
                    ? 6
                    : retainer.OriginalActorKind == 1 ? 11 : 8;
                break;
            }
            case 7:
                retainer.ActorMode = retainer.RetreatRegroupTarget is { Health: > 0 } regroup &&
                    RetreatFormationReached(retainer, regroup)
                        ? retainer.OriginalActorKind == 1 ? 3 : 1
                        : 5;
                break;
            case 8:
            {
                var opponent = retainer.OrderedTarget is { Health: > 0 } pursued
                    ? ModeEightContactTarget(retainer, pursued)
                    : null;
                if (opponent is not null) StoreFriendlyTarget(retainer, opponent);
                retainer.ActorMode = opponent is null ? 6 : 11;
                break;
            }
            case 10:
            {
                var opponent = AcquireRetreatOrderTarget(retainer);
                if (opponent is not null) StoreFriendlyTarget(retainer, opponent);
                retainer.ActorMode = opponent is null ? 2 : retainer.OriginalActorKind == 1 ? 4 : 5;
                break;
            }
            case 13:
                retainer.ActorMode = retainer.Health >= 6
                    ? retainer.OriginalActorKind == 1 ? 6 : 2
                    : 10;
                break;
        }
        BeginFriendlyMode(retainer);
    }

    private void AdvanceFollowOrder(SiegeRetainer retainer)
    {
        switch (retainer.ActorMode)
        {
            case 16:
                // Predicate 0x4F8B0 is evaluated, but transition 0x4E7CA
                // records mode 17 for both success and failure.
                _ = FollowModeSixteenHasContact(retainer);
                retainer.ActorMode = 17;
                break;
            case 17:
                retainer.ActorMode = FollowPlayerIsVisible(retainer) ? 16 : 17;
                break;
            default:
                retainer.ActorMode = 16;
                retainer.OrderedTarget = PlayerActor;
                break;
        }
        BeginFriendlyMode(retainer);
    }

    private void AdvanceAttackOrder(SiegeRetainer retainer)
    {
        switch (retainer.ActorMode)
        {
            case 6:
            {
                var target = AttackOrderTarget(retainer);
                if (target is not null) StoreFriendlyTarget(retainer, target);
                retainer.ActorMode = target is null ? 6 : retainer.OriginalActorKind == 1 ? 11 : 8;
                break;
            }
            case 8:
            {
                var target = retainer.OrderedTarget is { Health: > 0 } aimed
                    ? ModeEightContactTarget(retainer, aimed)
                    : null;
                if (target is not null) StoreFriendlyTarget(retainer, target);
                retainer.ActorMode = target is null ? 6 : 11;
                break;
            }
            case 11:
                break;
            default:
                retainer.ActorMode = 6;
                break;
        }
        BeginFriendlyMode(retainer);
    }

    private void BeginFriendlyMode(SiegeRetainer retainer)
    {
        retainer.MovementWanders = false;
        switch (retainer.ActorMode)
        {
            case 5:
            case 6:
                retainer.OrderedTarget = null;
                retainer.MovementWanders = true;
                retainer.MovementActive = true;
                break;
            case 7:
                if (retainer.RetreatRegroupTarget is { Health: > 0 } ally)
                {
                    AimRetainerAt(retainer, ally.X, ally.Y);
                    retainer.MovementActive = true;
                }
                break;
            case 8:
                if (retainer.OrderedTarget is { Health: > 0 } opponent)
                {
                    AimRetainerAt(retainer, opponent.X, opponent.Y);
                    retainer.MovementActive = true;
                }
                break;
            case 10:
                BeginFriendlyEscape(retainer);
                break;
            case 11:
                if (retainer.OrderedTarget is { Health: > 0 } target)
                    RetainerRangedAttack(retainer, target);
                break;
            case 16:
                if (retainer.OrderedTarget is { Health: > 0 } followed)
                {
                    AimRetainerAt(retainer, followed.X, followed.Y);
                    retainer.MovementActive = true;
                }
                break;
            case 17:
                retainer.OrderedTarget = null;
                retainer.MovementWanders = true;
                retainer.MovementActive = true;
                break;
        }
    }

    private bool FollowModeSixteenHasContact(SiegeRetainer retainer)
    {
        if (retainer.OrderedTarget is not { Health: > 0 } target ||
            RayToward(retainer, target) is not { Distance8: < 0x200 } hit)
            return false;
        return hit.Actor.Health > 0 &&
            (ReferenceEquals(hit.Actor, PlayerActor) || _retainers.Contains(hit.Actor));
    }

    private bool FollowPlayerIsVisible(SiegeRetainer retainer)
    {
        if (PlayerActor.Health <= 0 || RayToward(retainer, PlayerActor) is not { } hit ||
            !ReferenceEquals(hit.Actor, PlayerActor) || hit.Distance8 >= 0x7fff)
            return false;
        StoreFriendlyTarget(retainer, PlayerActor);
        return true;
    }

    private static void StoreFriendlyTarget(SiegeRetainer retainer, SiegeEnemy target)
    {
        retainer.OrderedTarget = target;
        retainer.RetreatTargetX8 = FixedActorX8(target);
        retainer.RetreatTargetY8 = FixedActorY8(target);
    }
}
