namespace Conqueror.Core;

public enum OriginalStrategicPointerTransition
{
    PrimaryDown,
    PrimaryUp,
    SecondaryDown,
    SecondaryUp
}

/// <summary>
/// Source 0x63114 classifies queued pointer transitions into codes 1-8.
/// The caller supplies monotonic timer units; the original interrupt rate is
/// not assumed here.
/// </summary>
public sealed class OriginalStrategicPointerEventClassifier
{
    public const long HoldThresholdUnits = 4;
    public const long RepeatThresholdUnits = 4;

    private long? _primaryDown;
    private long? _secondaryDown;
    private long? _lastShortPrimaryUp;
    private long? _lastShortSecondaryUp;
    private long _lastTimestamp;

    public int Classify(OriginalStrategicPointerTransition transition, long timestampUnits)
    {
        if (timestampUnits < _lastTimestamp)
            throw new ArgumentOutOfRangeException(nameof(timestampUnits),
                "Pointer event timestamps must be monotonic.");
        _lastTimestamp = timestampUnits;
        switch (transition)
        {
            case OriginalStrategicPointerTransition.PrimaryDown:
                _primaryDown = timestampUnits;
                return 1;
            case OriginalStrategicPointerTransition.PrimaryUp:
                return ReleasePrimary(timestampUnits);
            case OriginalStrategicPointerTransition.SecondaryDown:
                _secondaryDown = timestampUnits;
                return 5;
            case OriginalStrategicPointerTransition.SecondaryUp:
                return ReleaseSecondary(timestampUnits);
            default:
                throw new ArgumentOutOfRangeException(nameof(transition));
        }
    }

    private int ReleasePrimary(long timestampUnits)
    {
        if (_primaryDown is not { } down) return 0;
        _primaryDown = null;
        if (timestampUnits - down >= HoldThresholdUnits)
        {
            _lastShortPrimaryUp = null;
            return 2;
        }
        if (_lastShortPrimaryUp is { } previous
            && timestampUnits - previous < RepeatThresholdUnits)
        {
            _lastShortPrimaryUp = null;
            return 4;
        }
        _lastShortPrimaryUp = timestampUnits;
        return 3;
    }

    private int ReleaseSecondary(long timestampUnits)
    {
        if (_secondaryDown is not { } down) return 0;
        _secondaryDown = null;
        if (timestampUnits - down >= HoldThresholdUnits)
        {
            _lastShortSecondaryUp = null;
            return 6;
        }
        if (_lastShortSecondaryUp is { } previous
            && timestampUnits - previous < RepeatThresholdUnits)
        {
            _lastShortSecondaryUp = null;
            return 8;
        }
        _lastShortSecondaryUp = timestampUnits;
        return 7;
    }
}
