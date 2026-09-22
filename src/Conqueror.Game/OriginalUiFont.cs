using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Conqueror.Game;

/// <summary>Shape contract for the source game's fixed ASCII font resource.</summary>
public static class OriginalUiFontDefinition
{
    public const string ResourceSuffix = ":font.CSF";
    public const int GlyphCount = 256;
    public const int GlyphWidth = 11;
    public const int GlyphHeight = 13;

    public static bool IsCompatible(IReadOnlyList<CsfDimensionHeader> glyphs) =>
        glyphs.Count == GlyphCount && glyphs.All(glyph =>
            glyph.Width == GlyphWidth && glyph.Height == GlyphHeight);
}

/// <summary>
/// Monochrome textures reconstructed from the source font's alpha masks. Palette indices are
/// deliberately ignored because the original face is tinted by each surrounding UI palette.
/// </summary>
internal sealed class OriginalUiFont : IDisposable
{
    // Retain the existing clean-room UI slots while replacing their procedural glyph shapes.
    // Full source-screen typography spacing is not yet mapped.
    private const int CompatibilityAdvance = 6;
    private const int CompatibilityHeight = 7;
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
        var renderedText = wrap > 0
            ? PixelTextLayout.Wrap(text, Math.Max(1, wrap / (CompatibilityAdvance * scale)))
            : text;
        var x = (int)position.X;
        var y = (int)position.Y;
        var origin = x;
        foreach (var character in renderedText)
        {
            if (character == '\r') continue;
            if (character == '\n')
            {
                x = origin;
                y += CompatibilityLineHeight * scale;
                continue;
            }
            var glyph = _glyphs[character <= byte.MaxValue ? character : '?'];
            batch.Draw(glyph, new Rectangle(x, y, CompatibilityAdvance * scale, CompatibilityHeight * scale), color);
            x += CompatibilityAdvance * scale;
        }
    }

    public void Dispose()
    {
        foreach (var glyph in _glyphs) glyph.Dispose();
    }
}
