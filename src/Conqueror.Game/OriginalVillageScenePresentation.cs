using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

// PLACEHOLDER: RULE-UI-001. The original picks the catalog and record for every place it visits.
/// <summary>Executable-mapped binding between an original new-game home and its exterior catalog record.</summary>
public static class OriginalVillageScenePresentation
{
    /// <summary>
    /// Resolves the VILLAGE record selected by person record <c>+0x0F</c>.
    /// The generic campaign map has no recovered original location identity
    /// outside its home node, so it intentionally falls back for every other
    /// location and for migrated saves.
    /// </summary>
    public static VillageSceneDefinition? SceneForNewGameHome(
        CampaignState state, IReadOnlyList<VillageSceneDefinition> scenes)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(scenes);
        if (state.CurrentLocation != 0 || state.OriginalStrategicState is not { } strategic)
            return null;
        var person = strategic.SchedulerFallbackTargetPerson;
        if (person < 0 || person >= OriginalStrategicMovement.Persons.Count)
            return null;
        var index = OriginalStrategicMovement.Persons[person].VillageSceneIndex;
        return index < scenes.Count ? scenes[index] : null;
    }
}
