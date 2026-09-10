namespace Conqueror.Core;

public sealed record OriginalCombatantTemplate(int Armor, int Health);

public static class OriginalCombatantTemplates
{
    // CONQUER.EXE object 2 offset 0xCE10 is initialized as ten 0x44-byte
    // combatant templates. Armor and health occupy offsets 0x3c and 0x40.
    private static readonly OriginalCombatantTemplate[] Templates =
    [
        new(7, 12), new(8, 15), new(5, 10), new(6, 10), new(6, 10),
        new(6, 10), new(6, 10), new(5, 10), new(8, 15), new(10, 20)
    ];

    public static OriginalCombatantTemplate For(int template)
    {
        if ((uint)template >= (uint)Templates.Length)
            throw new ArgumentOutOfRangeException(nameof(template));
        return Templates[template];
    }
}
