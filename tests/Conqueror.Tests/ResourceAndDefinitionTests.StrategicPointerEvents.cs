using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicPointerClassifierPreservesPrimaryPressHoldAndRepeatCodes()
    {
        var events = new OriginalStrategicPointerEventClassifier();
        Assert.Equal(0, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 0));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 1));
        Assert.Equal(3, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 3));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 4));
        Assert.Equal(4, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 5));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 6));
        Assert.Equal(3, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 7));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 11));
        Assert.Equal(2, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 15));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 16));
        Assert.Equal(3, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 17));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 20));
        Assert.Equal(3, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 21));
    }

    [Fact]
    public void StrategicPointerClassifierKeepsSecondaryTimingIndependent()
    {
        var events = new OriginalStrategicPointerEventClassifier();
        Assert.Equal(5, events.Classify(OriginalStrategicPointerTransition.SecondaryDown, 1));
        Assert.Equal(7, events.Classify(OriginalStrategicPointerTransition.SecondaryUp, 2));
        Assert.Equal(1, events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 3));
        Assert.Equal(3, events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 4));
        Assert.Equal(5, events.Classify(OriginalStrategicPointerTransition.SecondaryDown, 5));
        Assert.Equal(8, events.Classify(OriginalStrategicPointerTransition.SecondaryUp, 5));
        Assert.Equal(5, events.Classify(OriginalStrategicPointerTransition.SecondaryDown, 9));
        Assert.Equal(6, events.Classify(OriginalStrategicPointerTransition.SecondaryUp, 13));
        Assert.Throws<ArgumentOutOfRangeException>(() => events.Classify(
            OriginalStrategicPointerTransition.PrimaryDown, 12));
    }

    [Fact]
    public void StrategicPointerReleaseCodesReachTheMappedTacticalRoutes()
    {
        var events = new OriginalStrategicPointerEventClassifier();
        events.Classify(OriginalStrategicPointerTransition.PrimaryDown, 1);
        var primary = events.Classify(OriginalStrategicPointerTransition.PrimaryUp, 2);
        events.Classify(OriginalStrategicPointerTransition.SecondaryDown, 3);
        var secondary = events.Classify(OriginalStrategicPointerTransition.SecondaryUp, 4);

        Assert.Equal(OriginalStrategicInteractiveEncounterInputRoute.ControlStrip,
            OriginalStrategicInteractiveEncounter.RouteMappedInputCode(primary));
        Assert.Equal(OriginalStrategicInteractiveEncounterInputRoute.DestinationOrder,
            OriginalStrategicInteractiveEncounter.RouteMappedInputCode(secondary));
    }
}
