using RefurbishedDinosaurs.Core.Presentation;

namespace Conqueror.Game;

/// <summary>Word-boundary wrapping for the fixed-width fallback renderer.</summary>
public static class PixelTextLayout
{
    public static string Wrap(string text, int maximumColumns) => FixedWidthText.Wrap(text, maximumColumns);
}
