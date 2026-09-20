namespace Conqueror.Core;

/// <summary>
/// Selects the strategic-map marker frame from the active player record's
/// source frame base. Asset identity and marker placement remain separate
/// presentation concerns.
/// </summary>
public static class OriginalStrategicMapMarkerPresentation
{
    public const int OrdinaryUnselectedOffset = 1;
    public const int AvatarUnselectedOffset = 2;
    public const int DistinguishedUnselectedOffset = 5;
    public const int OrdinarySelectedOffset = 6;
    public const int AvatarSelectedOffset = 7;
    public const int CharacterColorFrameStride = 8;

    /// <summary>
    /// Recreates initializer <c>0x12A77-0x12AA5</c>'s frame-base assignment:
    /// the character configuration's zero-based <c>COLOR</c> field is scaled
    /// by eight before being stored at each player record's <c>+0x38</c>.
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
    /// used by <c>0x12F28</c>. Coordinates are deliberately left in route
    /// space: the subsequent <c>0x3F0A0</c> clipping projection still belongs
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
    /// Mirrors <c>0x12F32-0x130C3</c> for an already-active player record.
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
/// the x87 toward-zero integer conversions made by <c>0x12F28</c>.
/// </summary>
public readonly record struct OriginalStrategicMapMarkerDraw(
    int Slot,
    int SourceX,
    int SourceY,
    int Frame);
