namespace Conqueror.Core;

public readonly record struct OriginalStrategicPointerInput(int Code, int X, int Y);

/// <summary>
/// Bounded source-order mouse event queue from callback 0x8497C and pop 0x63098.
/// Each callback can append several transitions with one timestamp and position.
/// </summary>
public sealed class OriginalStrategicPointerEventQueue
{
    public const int Capacity = 30;

    private readonly Queue<QueuedEvent> _events = new();
    private readonly OriginalStrategicPointerEventClassifier _classifier = new();

    public int Count => _events.Count;

    public void EnqueueTransitions(bool primaryDown, bool primaryUp,
        bool secondaryDown, bool secondaryUp, long timestampUnits, int x, int y)
    {
        if (timestampUnits < 0) throw new ArgumentOutOfRangeException(nameof(timestampUnits));
        if (primaryDown) Enqueue(new(OriginalStrategicPointerTransition.PrimaryDown, null, timestampUnits, x, y));
        if (primaryUp) Enqueue(new(OriginalStrategicPointerTransition.PrimaryUp, null, timestampUnits, x, y));
        if (secondaryDown) Enqueue(new(OriginalStrategicPointerTransition.SecondaryDown, null, timestampUnits, x, y));
        if (secondaryUp) Enqueue(new(OriginalStrategicPointerTransition.SecondaryUp, null, timestampUnits, x, y));
    }

    /// <summary>Host controller shortcut with the source's short secondary-release code.</summary>
    public void EnqueueControllerDestination(long timestampUnits, int x, int y)
    {
        if (timestampUnits < 0) throw new ArgumentOutOfRangeException(nameof(timestampUnits));
        Enqueue(new(null, 7, timestampUnits, x, y));
    }

    public bool TryDequeue(out OriginalStrategicPointerInput input)
    {
        if (_events.Count == 0)
        {
            input = default;
            return false;
        }

        var next = _events.Dequeue();
        var code = next.DirectCode ?? _classifier.Classify(next.Transition!.Value, next.TimestampUnits);
        input = new(code, next.X, next.Y);
        return true;
    }

    private void Enqueue(QueuedEvent item)
    {
        if (_events.Count < Capacity) _events.Enqueue(item);
    }

    private readonly record struct QueuedEvent(
        OriginalStrategicPointerTransition? Transition, int? DirectCode,
        long TimestampUnits, int X, int Y);
}
