using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicInteractiveEncounterUsesItsOwnedBackgroundAndUnitSequence()
    {
        Assert.Contains(new ImportedArtDefinition("Encounter.Strategic.Background", "image", ":battle.pcx"),
            ImportedArt.Definitions);
        Assert.Contains(new ImportedAnimationDefinition("Encounter.Strategic.Units", ":men8.csf",
                "Encounter.Strategic.Background"),
            ImportedAnimations.Definitions);
    }

    [Fact]
    public void StrategicInteractivePresentationUsesTheResolverFrameIndexFormula()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(0, 0, 1));

        Assert.Equal(15, OriginalStrategicInteractiveEncounterPresentation.FrameFor(units[0]));
        Assert.Equal(635, OriginalStrategicInteractiveEncounterPresentation.FrameFor(units[1]));

        units[0].PhaseCounter = 6;
        units[0].StateCode = 0x28;
        Assert.Equal(56, OriginalStrategicInteractiveEncounterPresentation.FrameFor(units[0]));
        Assert.Equal(720, OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayFrame);
        Assert.Equal(721, OriginalStrategicInteractiveEncounterPresentation.PendingFirstControlFrame);
        Assert.Equal(722, OriginalStrategicInteractiveEncounterPresentation.ControlStripFrame);
    }
}
