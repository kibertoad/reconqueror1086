using Conqueror.Core;

namespace Conqueror.Game;

/// <summary>
/// Application boundary for the recovered strategic main-loop pass. The
/// original ran this work in an unrestricted processor-rate loop; the host
/// invokes this once from each explicit 60 Hz map update instead.
/// </summary>
public static class OriginalStrategicHostRuntime
{
    public static readonly TimeSpan FixedCadence = TimeSpan.FromSeconds(1d / 60d);

    /// <summary>
    /// Advances player and hostile strategic records once when the campaign
    /// owns a source-shaped strategic state. Schema-one migrated saves retain
    /// player-record updates but cannot safely start the hostile scheduler:
    /// their original fallback globals were unavailable and are never
    /// invented by the replacement.
    /// </summary>
    public static OriginalStrategicCampaignPassResult? AdvanceFixedPass(
        Campaign campaign,
        bool playerEncounterHandoffActive = false,
        bool schedulerBlockedByModal = false,
        bool suppressDragonEntry = false)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        if (campaign.State.OriginalStrategicState is not { } strategic) return null;

        var schedulerFallbackUnavailable = strategic.SchedulerFallbackTargetPerson < 0
            || strategic.SchedulerFallbackOriginProperty < 0;
        return campaign.AdvanceOriginalStrategicPass(
            new OriginalStrategicCampaignPassInput(
                strategic.TemporaryForceSlots.OrderBy(slot => slot.Slot)
                    .Select(slot => slot.AsPlayerTarget()).ToArray(),
                PlayerEncounterHandoffActive: playerEncounterHandoffActive,
                SchedulerBlockedByModal: schedulerFallbackUnavailable
                    || playerEncounterHandoffActive
                    || schedulerBlockedByModal,
                SuppressDragonEntry: suppressDragonEntry));
    }
}
