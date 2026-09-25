namespace Conqueror.Core;

/// <summary>
/// Source <c>CHARACTR.DAT</c> <c>COLOR</c> values written by the three
/// character-options shield controls. The values select strategic marker
/// frame groups and must not be substituted with combat palette indices.
/// </summary>
// RULE-PERSON-003: the shields write COLOR 0, 3 and 5. PLACEHOLDER: which one is red, green or blue is a guess.
public static class OriginalStrategicCharacterColor
{
    public const int Red = 0;
    public const int Green = 3;
    public const int Blue = 5;

    public static int ForHeraldicColor(string heraldicColor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(heraldicColor);
        return heraldicColor.ToUpperInvariant() switch
        {
            "RED" => Red,
            "GREEN" => Green,
            "BLUE" => Blue,
            _ => throw new ArgumentOutOfRangeException(nameof(heraldicColor), heraldicColor,
                "No source character-options color is mapped for this heraldic color.")
        };
    }
}
