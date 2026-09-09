using System.Text;

namespace Conqueror.Game;

/// <summary>Word-boundary wrapping for the fixed-width fallback renderer.</summary>
public static class PixelTextLayout
{
    public static string Wrap(string text, int maximumColumns)
    {
        ArgumentNullException.ThrowIfNull(text);
        if (maximumColumns <= 0) throw new ArgumentOutOfRangeException(nameof(maximumColumns));

        var result = new StringBuilder(text.Length + text.Length / maximumColumns);
        var paragraphs = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
        for (var paragraphIndex = 0; paragraphIndex < paragraphs.Length; paragraphIndex++)
        {
            if (paragraphIndex > 0) result.Append('\n');
            var words = paragraphs[paragraphIndex].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var lineLength = 0;
            foreach (var word in words)
            {
                if (lineLength == 0)
                {
                    result.Append(word);
                    lineLength = word.Length;
                }
                else if (lineLength + 1 + word.Length <= maximumColumns)
                {
                    result.Append(' ').Append(word);
                    lineLength += 1 + word.Length;
                }
                else
                {
                    result.Append('\n').Append(word);
                    lineLength = word.Length;
                }
            }
        }
        return result.ToString();
    }
}
