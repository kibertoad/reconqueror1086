using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicPointerQueueRetainsEverySameCallbackEdgeInSourceOrder()
    {
        var queue = new OriginalStrategicPointerEventQueue();
        queue.EnqueueTransitions(true, true, true, true, 1, 12, 34);
        queue.EnqueueTransitions(true, false, false, false, 2, 56, 78);

        Assert.Equal(5, queue.Count);
        Assert.True(queue.TryDequeue(out var primaryDown));
        Assert.Equal(new OriginalStrategicPointerInput(1, 12, 34), primaryDown);
        Assert.True(queue.TryDequeue(out var primaryUp));
        Assert.Equal(new OriginalStrategicPointerInput(3, 12, 34), primaryUp);
        Assert.True(queue.TryDequeue(out var secondaryDown));
        Assert.Equal(new OriginalStrategicPointerInput(5, 12, 34), secondaryDown);
        Assert.True(queue.TryDequeue(out var secondaryUp));
        Assert.Equal(new OriginalStrategicPointerInput(7, 12, 34), secondaryUp);
        Assert.True(queue.TryDequeue(out var laterDown));
        Assert.Equal(new OriginalStrategicPointerInput(1, 56, 78), laterDown);
        Assert.False(queue.TryDequeue(out _));
    }

    [Fact]
    public void StrategicPointerQueueStopsAtThirtyEntries()
    {
        var queue = new OriginalStrategicPointerEventQueue();
        for (var timestamp = 0; timestamp < 29; timestamp++)
            queue.EnqueueTransitions(true, false, false, false, timestamp, timestamp, 0);
        queue.EnqueueTransitions(true, true, true, true, 29, 29, 1);
        queue.EnqueueControllerDestination(29, 30, 1);

        Assert.Equal(OriginalStrategicPointerEventQueue.Capacity, queue.Count);
        for (var index = 0; index < 29; index++)
        {
            Assert.True(queue.TryDequeue(out var input));
            Assert.Equal(new OriginalStrategicPointerInput(1, index, 0), input);
        }
        Assert.True(queue.TryDequeue(out var last));
        Assert.Equal(new OriginalStrategicPointerInput(1, 29, 1), last);
        Assert.False(queue.TryDequeue(out _));
    }

    [Fact]
    public void StrategicPointerQueueClassifiesCapturedTimeAfterDelayedConsumption()
    {
        var queue = new OriginalStrategicPointerEventQueue();
        queue.EnqueueTransitions(true, false, false, false, 10, 40, 50);
        queue.EnqueueTransitions(false, true, false, false, 14, 60, 70);

        Assert.True(queue.TryDequeue(out var down));
        Assert.Equal(new OriginalStrategicPointerInput(1, 40, 50), down);
        Assert.True(queue.TryDequeue(out var release));
        Assert.Equal(new OriginalStrategicPointerInput(2, 60, 70), release);
    }

    [Fact]
    public void StrategicPointerClockFollowsTheChainedBiosTickAccumulator()
    {
        Assert.Equal(0, OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(55)));
        Assert.Equal(1, OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(56)));
        Assert.Equal(3, OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(219)));
        Assert.Equal(4, OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(220)));
        Assert.Equal(4, OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(250)));
        Assert.Equal(18, OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromSeconds(1)));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromTicks(-1)));

        var shortPress = new OriginalStrategicPointerEventClassifier();
        shortPress.Classify(OriginalStrategicPointerTransition.PrimaryDown,
            OriginalStrategicPointerClock.UnitsAt(TimeSpan.Zero));
        Assert.Equal(3, shortPress.Classify(OriginalStrategicPointerTransition.PrimaryUp,
            OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(219))));

        var heldPress = new OriginalStrategicPointerEventClassifier();
        heldPress.Classify(OriginalStrategicPointerTransition.PrimaryDown,
            OriginalStrategicPointerClock.UnitsAt(TimeSpan.Zero));
        Assert.Equal(2, heldPress.Classify(OriginalStrategicPointerTransition.PrimaryUp,
            OriginalStrategicPointerClock.UnitsAt(TimeSpan.FromMilliseconds(220))));
    }

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
