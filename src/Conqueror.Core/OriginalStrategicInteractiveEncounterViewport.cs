namespace Conqueror.Core;

/// <summary>
/// Scroll offsets used by the interactive encounter input path at
/// <c>0x264F8</c>. The original moves an offset by ten while the pointer is
/// in a five-pixel edge band; the horizontal and vertical content limits are
/// intentionally asymmetric.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterViewport
{
    public const int EdgeBand = 5;
    public const int ScrollStep = 10;
    public const int BottomContentReserve = 50;

    public OriginalStrategicInteractiveEncounterViewport(
        int viewportWidth,
        int viewportHeight,
        int contentWidth,
        int contentHeight,
        int horizontalOffset = 0,
        int verticalOffset = 0)
    {
        if (viewportWidth <= 0)
            throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(viewportHeight));
        if (contentWidth < 0)
            throw new ArgumentOutOfRangeException(nameof(contentWidth));
        if (contentHeight < 0)
            throw new ArgumentOutOfRangeException(nameof(contentHeight));
        if (horizontalOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(horizontalOffset));
        if (verticalOffset < 0)
            throw new ArgumentOutOfRangeException(nameof(verticalOffset));

        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        ContentWidth = contentWidth;
        ContentHeight = contentHeight;
        HorizontalOffset = horizontalOffset;
        VerticalOffset = verticalOffset;
    }

    public int ViewportWidth { get; }
    public int ViewportHeight { get; }
    public int ContentWidth { get; }
    public int ContentHeight { get; }
    public int HorizontalOffset { get; private set; }
    public int VerticalOffset { get; private set; }

    /// <summary>
    /// Applies the four source-ordered edge checks. The right and bottom
    /// checks use strict inequalities, so an offset already at their
    /// calculated limit does not advance further.
    /// </summary>
    public bool ApplyMappedEdgeScroll(int pointerX, int pointerY)
    {
        var changed = false;
        if (pointerX <= EdgeBand && HorizontalOffset > 0)
        {
            HorizontalOffset -= ScrollStep;
            changed = true;
        }

        if (pointerX >= ViewportWidth - EdgeBand
            && ContentWidth - ViewportWidth - ScrollStep > HorizontalOffset)
        {
            HorizontalOffset += ScrollStep;
            changed = true;
        }

        if (pointerY <= EdgeBand && VerticalOffset > 0)
        {
            VerticalOffset -= ScrollStep;
            changed = true;
        }

        if (pointerY >= ViewportHeight - EdgeBand
            && ContentHeight - ViewportHeight - BottomContentReserve > VerticalOffset)
        {
            VerticalOffset += ScrollStep;
            changed = true;
        }

        return changed;
    }
}
