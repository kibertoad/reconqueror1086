namespace Conqueror.Core;

/// <summary>
/// Selects the strategic-map marker frame from the active player record's
/// source frame base. Asset identity and marker placement remain separate
/// presentation concerns.
/// </summary>
public static class OriginalStrategicMapMarkerPresentation
{
    public const int TemporaryForceMarkerFrame = 3;
    public const int OrdinaryUnselectedOffset = 1;
    public const int AvatarUnselectedOffset = 2;
    public const int DistinguishedUnselectedOffset = 5;
    public const int OrdinarySelectedOffset = 6;
    public const int AvatarSelectedOffset = 7;
    public const int CharacterColorFrameStride = 8;

    /// <summary>
    /// The frame base of RULE-STRATEGY-015: eight times the COLOR attribute.
    /// </summary>
    public static int FrameBaseForCharacterColor(int characterColor)
    {
        if (characterColor < 0) throw new ArgumentOutOfRangeException(nameof(characterColor));
        return checked(characterColor * CharacterColorFrameStride);
    }

    /// <summary>Creates the common source <c>+0x38</c> base for all six player records.</summary>
    public static IReadOnlyList<int> FrameBasesForCharacterColor(int characterColor)
    {
        var frameBase = FrameBaseForCharacterColor(characterColor);
        return Enumerable.Repeat(frameBase, OriginalStrategicMovement.PlayerMovementRecordCount).ToArray();
    }

    /// <summary>
    /// Enumerates the active source marker inputs in the physical record order
    /// of <c>draw_player_markers</c> (RULE-STRATEGY-015). Coordinates are left in route
    /// space: the clipping of <c>draw_marker</c> still belongs
    /// to the presentation boundary.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMapMarkerDraw> BuildDraws(
        OriginalStrategicCampaignState state,
        IReadOnlyList<int> frameBasesBySlot)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(frameBasesBySlot);
        if (frameBasesBySlot.Count != OriginalStrategicMovement.PlayerMovementRecordCount)
            throw new ArgumentException(
                $"Strategic marker drawing requires exactly {OriginalStrategicMovement.PlayerMovementRecordCount} frame bases.",
                nameof(frameBasesBySlot));
        state.Validate();

        var draws = new List<OriginalStrategicMapMarkerDraw>();
        foreach (var record in state.PlayerMovementSlots.OrderBy(record => record.Slot))
        {
            if (!record.Active) continue;
            var selected = record.Slot == state.SelectedPlayerMovementSlot;
            draws.Add(new OriginalStrategicMapMarkerDraw(
                record.Slot,
                TruncateTowardZero(record.CurrentX),
                TruncateTowardZero(record.CurrentY),
                FrameFor(record.Slot, selected, state.EngagedPlayerMovementSlot,
                    frameBasesBySlot[record.Slot])));
        }
        return draws;
    }

    /// <summary>
    /// Enumerates the five movement records in the physical order used
    /// of <c>draw_hostile_markers</c> (RULE-STRATEGY-015). Unlike player markers, their
    /// stored frame is used as it is: there is no selection
    /// offset. A zero value on an old replacement save is recovered from the
    /// confirmed origin-property table, because no supported source property
    /// uses frame zero.
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMapMarkerDraw> BuildMovementDraws(
        OriginalStrategicCampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();

        var draws = new List<OriginalStrategicMapMarkerDraw>();
        foreach (var record in state.MovementSlots.OrderBy(record => record.Slot))
        {
            if (!record.Active) continue;
            var frame = record.MarkerFrame != 0
                ? record.MarkerFrame
                : OriginalStrategicMovement.MovementMarkerFrameForOriginProperty(
                    record.OriginProperty);
            draws.Add(new OriginalStrategicMapMarkerDraw(
                record.Slot,
                TruncateTowardZero(record.CurrentX),
                TruncateTowardZero(record.CurrentY),
                frame));
        }
        return draws;
    }

    /// <summary>
    /// Enumerates active temporary-force records in their physical source
    /// order, drawn with frame 3 at the truncated position (RULE-STRATEGY-017).
    /// </summary>
    public static IReadOnlyList<OriginalStrategicMapMarkerDraw> BuildTemporaryForceDraws(
        OriginalStrategicCampaignState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        state.Validate();

        var draws = new List<OriginalStrategicMapMarkerDraw>();
        foreach (var force in state.TemporaryForceSlots.OrderBy(force => force.Slot))
        {
            if (!force.Active) continue;
            draws.Add(new OriginalStrategicMapMarkerDraw(
                force.Slot,
                TruncateTowardZero(force.CurrentX),
                TruncateTowardZero(force.CurrentY),
                TemporaryForceMarkerFrame));
        }
        return draws;
    }

    /// <summary>
    /// Mirrors <c>draw_player_markers</c> (RULE-STRATEGY-015) for an already-active player record.
    /// The original stores <paramref name="frameBase"/> at record <c>+0x38</c>
    /// and selects an offset from record <c>+0x04</c>, global <c>AE6C</c>, and
    /// the special avatar slot 5.
    /// </summary>
    public static int FrameFor(
        int slot,
        bool selected,
        int distinguishedSlot,
        int frameBase)
    {
        if (slot is < 0 or >= OriginalStrategicMovement.PlayerMovementRecordCount)
            throw new ArgumentOutOfRangeException(nameof(slot));
        if (distinguishedSlot is < 0 or >= OriginalStrategicMovement.PlayerMovementRecordCount)
            throw new ArgumentOutOfRangeException(nameof(distinguishedSlot));
        if (frameBase < 0)
            throw new ArgumentOutOfRangeException(nameof(frameBase));

        var offset = slot == OriginalStrategicMovement.PlayerAvatarMovementSlot
            ? selected ? AvatarSelectedOffset : AvatarUnselectedOffset
            : slot == distinguishedSlot
                ? selected ? 0 : DistinguishedUnselectedOffset
                : selected ? OrdinarySelectedOffset : OrdinaryUnselectedOffset;
        return checked(frameBase + offset);
    }

    private static int TruncateTowardZero(float value) => checked((int)Math.Truncate(value));
}

/// <summary>
/// One active player marker immediately before the original map blitter's
/// viewport projection. <see cref="SourceX"/> and <see cref="SourceY"/> are
/// the toward-zero integer conversions of RULE-STRATEGY-015.
/// </summary>
public readonly record struct OriginalStrategicMapMarkerDraw(
    int Slot,
    int SourceX,
    int SourceY,
    int Frame);
