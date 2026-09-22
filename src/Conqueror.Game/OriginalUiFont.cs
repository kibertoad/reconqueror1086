using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Conqueror.Game;

/// <summary>Shape contract for the executable-loaded proportional ASCII font resource.</summary>
public static class OriginalUiFontDefinition
{
    public const string ResourceSuffix = ":CONFONT.CSF";
    public const int GlyphCount = 256;
    public const int GlyphHeight = 13;
    public const int MinimumGlyphWidth = 2;
    public const int MaximumGlyphWidth = 10;

    public static bool IsCompatible(IReadOnlyList<CsfDimensionHeader> glyphs) =>
        glyphs.Count == GlyphCount && glyphs.All(glyph =>
            glyph.Width is >= MinimumGlyphWidth and <= MaximumGlyphWidth
            && glyph.Height == GlyphHeight);

    /// <summary>
    /// Converts a cumulative source-pixel advance into the existing host canvas.
    /// Conversion is deliberately applied after accumulating source advances so
    /// narrow proportional glyphs do not compound rounding error.
    /// </summary>
    public static int CanvasAdvance(int sourcePixels, int scale)
    {
        if (sourcePixels < 0) throw new ArgumentOutOfRangeException(nameof(sourcePixels));
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));
        return (sourcePixels * 2 * scale + 1) / 3;
    }
}

/// <summary>
/// Monochrome textures reconstructed from the source font's alpha masks. Palette indices are
/// deliberately ignored because the runtime text color is supplied by each caller; the original
/// screen-specific palette association has not yet been recovered.
/// </summary>
internal sealed class OriginalUiFont : IDisposable
{
    // The existing canvas has a two-thirds conversion from the source's nominal nine-pixel
    // glyph width to its previous six-pixel text slot. Preserve that host layout while making
    // the source's per-glyph advance exact relative to every other glyph.
    private const int CompatibilityLineHeight = 9;
    private readonly Texture2D[] _glyphs;

    private OriginalUiFont(Texture2D[] glyphs) => _glyphs = glyphs;

    public static OriginalUiFont? Create(GraphicsDevice graphicsDevice, CsfSequence source)
    {
        ArgumentNullException.ThrowIfNull(graphicsDevice);
        ArgumentNullException.ThrowIfNull(source);
        var dimensions = source.Chunks.Select(chunk => source.ReadDimensionHeader(chunk)).ToArray();
        if (!OriginalUiFontDefinition.IsCompatible(dimensions)) return null;

        var glyphs = new Texture2D[OriginalUiFontDefinition.GlyphCount];
        try
        {
            for (var index = 0; index < glyphs.Length; index++)
            {
                var frame = source.DecodeFrame(source.Chunks[index]);
                var pixels = frame.Alpha.Select(alpha => new Color((byte)255, (byte)255, (byte)255, alpha)).ToArray();
                var texture = new Texture2D(graphicsDevice, frame.Width, frame.Height, false, SurfaceFormat.Color);
                texture.SetData(pixels);
                glyphs[index] = texture;
            }
            return new OriginalUiFont(glyphs);
        }
        catch
        {
            foreach (var glyph in glyphs) glyph?.Dispose();
            throw;
        }
    }

    public void Draw(SpriteBatch batch, string text, Vector2 position, Color color, int scale, int wrap)
    {
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(text);
        if (scale <= 0) throw new ArgumentOutOfRangeException(nameof(scale));
        var renderedText = wrap > 0 ? Wrap(text, wrap, scale) : text;
        var x = (int)position.X;
        var y = (int)position.Y;
        var origin = x;
        var sourceAdvance = 0;
        foreach (var character in renderedText)
        {
            if (character == '\r') continue;
            if (character == '\n')
            {
                x = origin;
                y += CompatibilityLineHeight * scale;
                sourceAdvance = 0;
                continue;
            }
            var glyph = _glyphs[character <= byte.MaxValue ? character : '?'];
            x = origin + OriginalUiFontDefinition.CanvasAdvance(sourceAdvance, scale);
            sourceAdvance += glyph.Width;
            var nextX = origin + OriginalUiFontDefinition.CanvasAdvance(sourceAdvance, scale);
            batch.Draw(glyph, new Rectangle(x, y, Math.Max(1, nextX - x),
                OriginalUiFontDefinition.CanvasAdvance(glyph.Height, scale)), color);
        }
    }

    private string Wrap(string text, int maximumWidth, int scale)
    {
        var result = new System.Text.StringBuilder(text.Length + text.Length / 8);
        var paragraphs = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').Split('\n');
        for (var paragraphIndex = 0; paragraphIndex < paragraphs.Length; paragraphIndex++)
        {
            if (paragraphIndex > 0) result.Append('\n');
            var words = paragraphs[paragraphIndex].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            var lineWidth = 0;
            foreach (var word in words)
            {
                var wordWidth = SourceWidth(word);
                var separator = lineWidth == 0 ? 0 : SourceWidth(" ");
                if (lineWidth > 0 && OriginalUiFontDefinition.CanvasAdvance(lineWidth + separator + wordWidth, scale) > maximumWidth)
                {
                    result.Append('\n');
                    lineWidth = 0;
                    separator = 0;
                }
                else if (separator > 0)
                    result.Append(' ');
                result.Append(word);
                lineWidth += separator + wordWidth;
            }
        }
        return result.ToString();
    }

    private int SourceWidth(string text) => text.Sum(character =>
        _glyphs[character <= byte.MaxValue ? character : '?'].Width);

    public void Dispose()
    {
        foreach (var glyph in _glyphs) glyph.Dispose();
    }
}
