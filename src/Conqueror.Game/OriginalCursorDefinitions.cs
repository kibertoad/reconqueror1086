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

/// <summary>Visual identities of the six ordered FFMOUSE.CSF frames.</summary>
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
