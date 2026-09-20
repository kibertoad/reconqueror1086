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

        units[0].PositionX = 300;
        units[0].PositionY = 200;
        Assert.Equal((215, 115), OriginalStrategicInteractiveEncounterPresentation.DrawPositionFor(
            units[0], horizontalScrollOffset: 40, verticalScrollOffset: 40));

        units[1].PositionX = 180;
        units[1].PositionY = 110;
        var overlays = OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayDrawsFor(
            units, [1, 0], horizontalScrollOffset: 40, verticalScrollOffset: 40);
        Assert.Equal([
            new OriginalStrategicInteractiveEncounterOverlayDraw(1, 720, 135, 63),
            new OriginalStrategicInteractiveEncounterOverlayDraw(0, 720, 255, 153),
        ], overlays);

        Assert.Equal((415, 452), OriginalStrategicInteractiveEncounterPresentation
            .PendingFirstControlDrawPositionFor(controlStripMargin: 0, verticalSpan: 480));
        Assert.Equal((420, 452), OriginalStrategicInteractiveEncounterPresentation
            .ControlStripDrawPositionFor(controlStripMargin: 0, verticalSpan: 480));
        Assert.Equal((495, 552), OriginalStrategicInteractiveEncounterPresentation
            .PendingFirstControlDrawPositionFor(controlStripMargin: 80, verticalSpan: 580));

        units[0].RemainingStrength = 0;
        units[0].PositionX = 300;
        units[0].PositionY = 300;
        units[1].PositionX = 300;
        units[1].PositionY = 100;
        var third = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(0, 0, 0))[0];
        third.PositionX = 50;
        third.PositionY = 100;
        var orderedUnits = new[] { units[0], units[1], third };
        Assert.Equal([0, 2, 1], OriginalStrategicInteractiveEncounterPresentation
            .OrderedUnitIndicesFor(orderedUnits));
        Assert.Equal([
            new OriginalStrategicInteractiveEncounterUnitDraw(0, 56, 255, 255),
            new OriginalStrategicInteractiveEncounterUnitDraw(2, 15, 5, 55),
            new OriginalStrategicInteractiveEncounterUnitDraw(1, 635, 255, 55),
        ], OriginalStrategicInteractiveEncounterPresentation.UnitDrawsFor(
            orderedUnits, horizontalScrollOffset: 0, verticalScrollOffset: 0));
    }
}
