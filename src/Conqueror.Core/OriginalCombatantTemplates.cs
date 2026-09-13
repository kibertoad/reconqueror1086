namespace Conqueror.Core;

public sealed record OriginalCombatantTemplate(int AttackSkill, int Armor, int Health);
public sealed record OriginalActorModeProfile(int Current, int Requested, int Previous);

public static class OriginalCombatantTemplates
{
    // CONQUER.EXE object 2 offset 0xCE10 is initialized as ten 0x44-byte
    // combatant templates. Attack skill, armor, and health occupy offsets
    // 0x34, 0x3c, and 0x40.
    private static readonly OriginalCombatantTemplate[] Templates =
    [
        new(50, 7, 12), new(70, 8, 15), new(60, 5, 10), new(50, 6, 10), new(50, 6, 10),
        new(50, 6, 10), new(50, 6, 10), new(50, 5, 10), new(70, 8, 15), new(85, 10, 20)
    ];

    // The same initializer writes current/requested/previous actor modes at
    // offsets +0x18/+0x1c/+0x20 in each 0x44-byte template.
    private static readonly OriginalActorModeProfile[] ModeProfiles =
    [
        new(4, 4, 4), new(4, 4, 4), new(4, 4, 4), new(6, 8, 6), new(4, 10, 6),
        new(4, 8, 1), new(1, 4, 2), new(4, 10, 1), new(6, 8, 4), new(4, 8, 4)
    ];

    public static OriginalCombatantTemplate For(int template)
    {
        if ((uint)template >= (uint)Templates.Length)
            throw new ArgumentOutOfRangeException(nameof(template));
        return Templates[template];
    }

    public static OriginalActorModeProfile ModeProfileForSceneTemplate(int template)
    {
        if ((uint)template >= (uint)ModeProfiles.Length)
            throw new ArgumentOutOfRangeException(nameof(template));
        return ModeProfiles[template];
    }

    // The complete owned MELEE*/DEFEND* actor-base population uses templates
    // 0-2 for the player side and 3, 5, 8, and 9 for the opposing side.
    public static bool IsFriendlySceneTemplate(int template) => template is 0 or 1 or 2;

    public static bool IsPlacedSceneTemplate(int template) => template is 0 or 1 or 2 or 3 or 5 or 8 or 9;

    // CONQUER.EXE 0x542F8 initializes the actor-kind selector for templates
    // 0-9 in this order. The selector chooses the transition table at 0x4E5F0.
    public static int ActorKindForSceneTemplate(int template) => template switch
    {
        0 or 1 => 0,
        2 => 1,
        3 => 2,
        4 => 3,
        5 => 4,
        6 => 5,
        7 => 6,
        8 => 2,
        9 => 7,
        _ => throw new ArgumentOutOfRangeException(nameof(template))
    };
}
