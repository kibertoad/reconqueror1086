namespace Conqueror.Core;

public enum OriginalPracticeJoustOutcome
{
    PlayerHit,
    OpponentHit,
    BothMissed
}

public sealed record OriginalPracticeJoustResult(
    OriginalPracticeJoustOutcome Outcome, int HorizontalError, int VerticalError,
    int PlayerPoints, int OpponentPoints,
    int HorizontalOffset, int VerticalOffset);

/// <summary>
/// RULE-JOUST-002 contact window on frames 80-82 and result, sampled once
/// per movie frame (DEV-JOUST-001).
/// </summary>
public sealed class OriginalPracticeJoustTrial
{
    private static readonly int[] TargetX = [191, 161, 122];
    private static readonly int[] TargetY = [191, 203, 211];
    private readonly bool[] _sampled = new bool[3];
    private int _horizontalError;
    private int _verticalError;
    private int _horizontalOffset;
    private int _verticalOffset;

    public const int FirstContactFrame = 80;
    public const int LastContactFrame = 82;
    public const int InitialPlayerPoints = 20;
    public const int InitialOpponentPoints = 50;

    public void RecordFrame(int frame, int lanceX, int lanceY)
    {
        var index = frame - FirstContactFrame;
        if ((uint)index >= (uint)TargetX.Length || _sampled[index]) return;
        _sampled[index] = true;
        _horizontalError += Math.Abs(TargetX[index] - lanceX);
        _verticalError += Math.Abs(TargetY[index] - lanceY);
        _horizontalOffset += TargetX[index] - lanceX;
        _verticalOffset += TargetY[index] - lanceY;
    }

    public OriginalPracticeJoustResult Resolve(Func<int> rollBelowOneHundred)
    {
        ArgumentNullException.ThrowIfNull(rollBelowOneHundred);
        if (_sampled.Any(sampled => !sampled))
            throw new InvalidOperationException("The practice contact frames are incomplete.");

        var threshold = 90 - InitialPlayerPoints;
        if (_horizontalError < threshold && _verticalError < threshold)
            return new(OriginalPracticeJoustOutcome.PlayerHit,
                _horizontalError, _verticalError, InitialPlayerPoints + 2, InitialOpponentPoints,
                _horizontalOffset, _verticalOffset);

        var roll = rollBelowOneHundred();
        if ((uint)roll >= 100) throw new ArgumentOutOfRangeException(nameof(rollBelowOneHundred));
        var opponentHit = roll < InitialOpponentPoints - InitialPlayerPoints + 50;
        return new(opponentHit ? OriginalPracticeJoustOutcome.OpponentHit
                : OriginalPracticeJoustOutcome.BothMissed,
            _horizontalError, _verticalError, InitialPlayerPoints,
            InitialOpponentPoints + (opponentHit ? 2 : 0),
            _horizontalOffset, _verticalOffset);
    }
}
