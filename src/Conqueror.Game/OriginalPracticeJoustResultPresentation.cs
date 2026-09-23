using Conqueror.Core;

namespace Conqueror.Game;

/// <summary>Authored wording following the source's win and signed-miss branches.</summary>
public static class OriginalPracticeJoustResultPresentation
{
    public static string DescriptionFor(OriginalPracticeJoustResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Outcome == OriginalPracticeJoustOutcome.PlayerHit)
            return "YOU WIN THE PRACTICE PASS";
        var horizontal = result.HorizontalOffset > 0 ? "LEFT"
            : result.HorizontalOffset < 0 ? "RIGHT" : "";
        var vertical = result.VerticalOffset > 0 ? "HIGH"
            : result.VerticalOffset < 0 ? "LOW" : "";
        if (horizontal.Length > 0 && vertical.Length > 0)
            return $"YOUR LANCE MISSES {horizontal} AND {vertical}";
        if (horizontal.Length > 0) return $"YOUR LANCE MISSES {horizontal}";
        if (vertical.Length > 0) return $"YOUR LANCE MISSES {vertical}";
        return "YOUR LANCE MISSES";
    }
}
