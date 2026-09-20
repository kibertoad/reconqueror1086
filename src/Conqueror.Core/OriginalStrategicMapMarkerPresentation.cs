namespace Conqueror.Core;

/// <summary>
/// Selects the strategic-map marker frame from the active player record's
/// opaque source frame base. Asset identity and marker placement remain
/// separate presentation concerns.
/// </summary>
public static class OriginalStrategicMapMarkerPresentation
{
    public const int OrdinaryUnselectedOffset = 1;
    public const int AvatarUnselectedOffset = 2;
    public const int DistinguishedUnselectedOffset = 5;
    public const int OrdinarySelectedOffset = 6;
    public const int AvatarSelectedOffset = 7;

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
}
