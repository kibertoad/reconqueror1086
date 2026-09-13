namespace Conqueror.Core;

public sealed record OriginalCombatantTemplate(int AttackSkill, int Armor, int Health);

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

    public static OriginalCombatantTemplate For(int template)
    {
        if ((uint)template >= (uint)Templates.Length)
            throw new ArgumentOutOfRangeException(nameof(template));
        return Templates[template];
    }
}
