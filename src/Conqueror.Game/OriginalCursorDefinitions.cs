namespace Conqueror.Game;

public enum OriginalCursorKind
{
    Sword,
    Wait,
    Travel,
    Talk,
    Target,
    Hand
}

/// <summary>
/// Visual identities of the six ordered <c>FFMOUSE.CSF</c> frames. Startup
/// sequence <c>0x2A4DC</c> passes object-2 resource string <c>+0x3A70</c> to
/// mouse initializer <c>0x72180</c>, which forwards it to resource loader
/// <c>0x18850</c>.
/// </summary>
public static class OriginalCursorDefinitions
{
    public const int FrameCount = 6;

    public static int Frame(OriginalCursorKind kind) => kind switch
    {
        OriginalCursorKind.Sword => 0,
        OriginalCursorKind.Wait => 1,
        OriginalCursorKind.Travel => 2,
        OriginalCursorKind.Talk => 3,
        OriginalCursorKind.Target => 4,
        OriginalCursorKind.Hand => 5,
        _ => throw new ArgumentOutOfRangeException(nameof(kind))
    };
}
