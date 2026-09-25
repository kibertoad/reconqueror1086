namespace Conqueror.Core;

/// <summary>
/// Scroll offsets used by the interactive encounter input path at
/// RULE-BATTLE-008. The original moves an offset by ten while the pointer is
/// in a five-pixel edge band; the horizontal and vertical content limits are
/// intentionally asymmetric. The control-strip margin is separate: setup
/// stores it at <c>battle_control_margin</c>, while scrolling uses <c>battle_scroll_y</c>/battle_scroll_x.
/// </summary>
public sealed class OriginalStrategicInteractiveEncounterViewport
{
    public const int EdgeBand = 5;
    public const int ScrollStep = 10;
    public const int BottomContentReserve = 50;
    public const int MediumResolvedHorizontalSpan = 800;
    public const int WideResolvedHorizontalSpan = 1024;
    public const int MediumControlStripMargin = 80;
    public const int WideControlStripMargin = 160;

    public OriginalStrategicInteractiveEncounterViewport(
        int viewportWidth,
        int viewportHeight,
        int contentWidth,
        int contentHeight,
        int horizontalOffset = 0,
        int verticalOffset = 0,
        int controlStripMargin = 0)
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
        if (controlStripMargin < 0)
            throw new ArgumentOutOfRangeException(nameof(controlStripMargin));

        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
        ContentWidth = contentWidth;
        ContentHeight = contentHeight;
        HorizontalOffset = horizontalOffset;
        VerticalOffset = verticalOffset;
        ControlStripMargin = controlStripMargin;
    }

    public int ViewportWidth { get; }
    public int ViewportHeight { get; }
    public int ContentWidth { get; }
    public int ContentHeight { get; }
    public int HorizontalOffset { get; private set; }
    public int VerticalOffset { get; private set; }
    public int ControlStripMargin { get; }

    /// <summary>
    /// Reproduces resolver setup RULE-BATTLE-001's mapping from its
    /// already-resolved display-width global (<c>battle_mode_width</c>) to the separate
    /// control-strip margin global (<c>battle_control_margin</c>). The preceding
    /// <c>WAR_MODE</c> display-selection and capability probes are deliberately
    /// not modeled here: the source only makes these two nonzero assignments
    /// after a successful display-mode change.
    /// </summary>
    public static int MappedControlStripMarginForResolvedHorizontalSpan(int horizontalSpan)
    {
        if (horizontalSpan <= 0)
            throw new ArgumentOutOfRangeException(nameof(horizontalSpan));

        return horizontalSpan switch
        {
            MediumResolvedHorizontalSpan => MediumControlStripMargin,
            WideResolvedHorizontalSpan => WideControlStripMargin,
            _ => 0,
        };
    }

    /// <summary>
    /// Creates a viewport using the resolver's confirmed control-strip margin
    /// for a display span that the host has already selected. Content size and
    /// viewport height remain explicit because this setup path does not prove
    /// a portable backdrop-scaling policy.
    /// </summary>
    public static OriginalStrategicInteractiveEncounterViewport ForResolvedDisplay(
        int viewportWidth,
        int viewportHeight,
        int contentWidth,
        int contentHeight,
        int horizontalOffset = 0,
        int verticalOffset = 0) =>
        new(viewportWidth, viewportHeight, contentWidth, contentHeight,
            horizontalOffset, verticalOffset,
            MappedControlStripMarginForResolvedHorizontalSpan(viewportWidth));

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
