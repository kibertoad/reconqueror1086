namespace Conqueror.Core;

/// <summary>
/// Application-facing result of one original strategic-map pointer dispatch.
/// The caller owns only the target-confirmation response; conversion, hit
/// precedence, and command construction remain executable-mapped.
/// </summary>
public readonly record struct OriginalStrategicMapDispatchResult(
    OriginalStrategicRoutePoint RoutePoint,
    OriginalStrategicPlayerMapHit Hit,
    OriginalStrategicPlayerCommandResult Command)
{
    /// <summary>
    /// The source opens its shared target-confirmation modal only for the
    /// second and third hit-table families; a player selection or empty route
    /// click has already completed its own command path.
    /// </summary>
    public bool RequiresTargetConfirmation => Hit.EnemySlot is not null || Hit.DivisionSlot is not null;
}

public static class OriginalStrategicMapCommands
{
    /// <summary>
    /// Performs the map click of RULE-STRATEGY-013 after the caller
    /// has supplied the logical source pointer and any confirmation response
    /// for an enemy or temporary-division target.
    /// </summary>
    public static OriginalStrategicMapDispatchResult DispatchRawPointer(
        OriginalStrategicCampaignState state,
        IOriginalStrategicResources resources,
        IReadOnlyList<OriginalStrategicPlayerTarget> divisionTargets,
        int rawPointerX,
        int rawPointerY,
        bool targetConfirmed)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(divisionTargets);

        var routePoint = OriginalStrategicMapPointer.ToMappedRoutePoint(
            state, rawPointerX, rawPointerY);
        var hit = OriginalStrategicMapHitTesting.HitTest(
            state, divisionTargets, routePoint.X, routePoint.Y);
        var command = OriginalStrategicMovement.DispatchPlayerMapCommand(
            state, resources, hit, routePoint.X, routePoint.Y, targetConfirmed);
        return new(routePoint, hit, command);
    }
}
