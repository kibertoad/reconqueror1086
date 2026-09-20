namespace Conqueror.Core;

/// <summary>
/// Reproduces the original strategic-map dispatcher's ordered record hit
/// testing after <c>0x640A0</c> has converted the raw pointer to route space.
/// The temporary division targets remain caller-owned because their table is
/// not persisted by the strategic campaign state.
/// </summary>
public static class OriginalStrategicMapHitTesting
{
    public const int PlayerLeftOffset = 11;
    public const int PlayerTopOffset = -30;
    public const int PlayerWidth = 24;
    public const int PlayerHeight = 31;
    public const int TargetTopOffset = -20;
    public const int TargetWidth = 40;
    public const int TargetHeight = 40;

    public static OriginalStrategicPlayerMapHit HitTest(
        OriginalStrategicCampaignState state,
        IReadOnlyList<OriginalStrategicPlayerTarget> divisionTargets,
        int routeX,
        int routeY)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(divisionTargets);
        if (divisionTargets.Count != OriginalStrategicMovement.PlayerDivisionTargetCount)
            throw new ArgumentException(
                $"Strategic map hit testing requires exactly {OriginalStrategicMovement.PlayerDivisionTargetCount} division targets.",
                nameof(divisionTargets));
        state.Validate();

        foreach (var player in state.PlayerMovementSlots.OrderBy(slot => slot.Slot))
            if (player.Active && Contains(
                    Truncate(player.CurrentX) + PlayerLeftOffset,
                    Truncate(player.CurrentY) + PlayerTopOffset,
                    PlayerWidth,
                    PlayerHeight,
                    routeX,
                    routeY))
                return new(player.Slot, null, null);

        foreach (var enemy in state.MovementSlots.OrderBy(slot => slot.Slot))
            if (enemy.Active && Contains(
                    Truncate(enemy.CurrentX),
                    Truncate(enemy.CurrentY) + TargetTopOffset,
                    TargetWidth,
                    TargetHeight,
                    routeX,
                    routeY))
                return new(null, enemy.Slot, null);

        for (var division = 0; division < divisionTargets.Count; division++)
        {
            var target = divisionTargets[division];
            if (target.Active && Contains(
                    Truncate(target.CurrentX),
                    Truncate(target.CurrentY) + TargetTopOffset,
                    TargetWidth,
                    TargetHeight,
                    routeX,
                    routeY))
                return new(null, null, division);
        }

        return default;
    }

    private static bool Contains(int left, int top, int width, int height, int x, int y) =>
        x >= left && x < checked(left + width)
        && y >= top && y < checked(top + height);

    private static int Truncate(float value) => checked((int)value);
}
