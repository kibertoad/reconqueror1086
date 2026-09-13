namespace Conqueror.Game;

// CONQUER.EXE 0x552FB-0x55395 advances this four-frame effect once per
// foreground render. The siege loop at 0x5841F has no timing gate, so 70 ms
// is an explicit compatibility policy rather than an original-game formula.
public sealed class SiegeHitEffect
{
    public const double CompatibilityStepSeconds = 0.07;

    private readonly SiegeFrameRun _frames;
    private double _elapsed;
    private int _step;

    public SiegeHitEffect(SiegeFrameRun frames)
    {
        if (frames.Count != 4 || frames.Descending)
            throw new ArgumentException("A hit effect must contain four ascending frames.", nameof(frames));
        _frames = frames;
    }

    public int Step => _step;
    public int Frame => _step < _frames.Count ? _frames.Start + _step : -1;
    public bool Complete => Frame < 0;

    public void Advance(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        if (Complete) return;

        _elapsed += elapsedSeconds;
        while (_elapsed >= CompatibilityStepSeconds && !Complete)
        {
            _elapsed -= CompatibilityStepSeconds;
            _step++;
        }
    }
}
