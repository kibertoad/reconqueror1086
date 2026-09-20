using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicInteractiveHoverStatusPanelUsesItsOwnMarginAndLowerAnchor()
    {
        Assert.Equal(new OriginalStrategicInteractiveEncounterRectangle(170, 455, 83, 20),
            OriginalStrategicInteractiveEncounterPresentation.HoverStatusPanelBoundsFor(
                controlStripMargin: 10, verticalSpan: 480));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicInteractiveEncounterPresentation.HoverStatusPanelBoundsFor(
                controlStripMargin: -1, verticalSpan: 480));
    }

    [Fact]
    public void StrategicInteractiveHoverUsesSourceLabelsAndOnePassAggregateStatus()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(2, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);

        var player = session.CaptureMappedHoverPresentation(
            localX: 50, localY: 20, horizontalOffset: 10, verticalOffset: 10);
        Assert.Equal((OriginalStrategicInteractiveEncounterHoverKind.PlayerStrength, 100, "OUR 100%"),
            (player.Kind, player.Strength, player.Text));

        var winning = session.CaptureMappedHoverPresentation(0, 100, 0, 0);
        Assert.Equal((OriginalStrategicInteractiveEncounterHoverKind.Winning, "WINNING"),
            (winning.Kind, winning.Text));
        Assert.Equal(OriginalStrategicInteractiveEncounterHoverKind.None,
            session.CaptureMappedHoverPresentation(0, 100, 0, 0).Kind);

        var foe = session.CaptureMappedHoverPresentation(0, 0, 0, 0);
        Assert.Equal((OriginalStrategicInteractiveEncounterHoverKind.Foe, "FOE"), (foe.Kind, foe.Text));
    }
}
