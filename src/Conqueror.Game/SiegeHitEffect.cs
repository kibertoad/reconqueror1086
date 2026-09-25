namespace Conqueror.Game;

// RULE-ASSAULT-028 blood effect. DEV-ASSAULT-003: each of the four frames
// shows for 70 ms of simulation time.
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
