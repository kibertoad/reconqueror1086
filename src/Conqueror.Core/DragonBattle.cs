namespace Conqueror.Core;

public enum DragonBattleOutcome { InProgress, Victory, Defeat, Withdrawn }

public sealed record DragonBattleDefinition(
    double DurationSeconds,
    double AimSpeed);

public sealed class DragonBattleSession
{
    public static readonly DragonBattleDefinition Rules = new(
        OriginalDragonRunTimeline.EndFrame * OriginalDragonRunTimeline.FrameMilliseconds / 1000d,
        .65);

    private readonly int _lanceExperience;
    private readonly int _equipmentBonus;
    private readonly OriginalDragonLanceMotion _lance = new();
    private long _elapsedTicks;
    private int _lastProcessedFrame = -1;

    public DragonBattleOutcome Outcome { get; private set; }
    public double AimX { get; private set; } = .5;
    public double AimY { get; private set; } = .72;
    public double EyeX { get; private set; } = 277d / OriginalDragonRunTimeline.ScreenWidth;
    public double EyeY { get; private set; } = 215d / OriginalDragonRunTimeline.ScreenHeight;
    public int LanceX => _lance.X;
    public int LanceY => _lance.Y;
    public int LanceFrame => OriginalDragonLanceSelection.FrameFor(LanceX, LanceY);
    public int SourceFrame { get; private set; }
    public bool EyeVisible => SourceFrame is >= OriginalDragonRunTimeline.FirstTargetFrame
        and < OriginalDragonRunTimeline.EndFrame;
    public double RemainingSeconds => (OriginalDragonRunTimeline.EndFrame *
        OriginalDragonRunTimeline.FrameMilliseconds * TimeSpan.TicksPerMillisecond - _elapsedTicks)
        / (double)TimeSpan.TicksPerSecond;
    public int HorizontalError { get; private set; }
    public int VerticalError { get; private set; }
    public int ScoredFrames { get; private set; }
    public int ScoreThreshold => OriginalDragonRunScore.Threshold(_lanceExperience,
        _equipmentBonus);
    public string LastMessage { get; private set; } = "Keep the lance aligned with the dragon's eye.";

    public DragonBattleSession(int lanceExperience, int equipmentBonus)
    {
        _lanceExperience = lanceExperience;
        _equipmentBonus = equipmentBonus;
    }

    public void MoveAim(double horizontal, double vertical, double elapsedSeconds)
    {
        if (Outcome != DragonBattleOutcome.InProgress || elapsedSeconds <= 0) return;
        AimX = Math.Clamp(AimX + horizontal * Rules.AimSpeed * elapsedSeconds, 0, 1);
        AimY = Math.Clamp(AimY + vertical * Rules.AimSpeed * elapsedSeconds, 0, 1);
    }

    public void SetAim(double x, double y)
    {
        if (Outcome != DragonBattleOutcome.InProgress) return;
        AimX = Math.Clamp(x, 0, 1);
        AimY = Math.Clamp(y, 0, 1);
    }

    public void Tick(double elapsedSeconds)
    {
        if (Outcome != DragonBattleOutcome.InProgress || elapsedSeconds <= 0) return;
        var durationTicks = OriginalDragonRunTimeline.EndFrame *
            OriginalDragonRunTimeline.FrameMilliseconds * TimeSpan.TicksPerMillisecond;
        var incrementTicks = (long)Math.Round(Math.Min(elapsedSeconds, Rules.DurationSeconds)
            * TimeSpan.TicksPerSecond, MidpointRounding.AwayFromZero);
        _elapsedTicks = Math.Min(durationTicks, _elapsedTicks + incrementTicks);
        SourceFrame = OriginalDragonRunTimeline.FrameAt(TimeSpan.FromTicks(_elapsedTicks));
        var pointerX = (int)Math.Round(AimX * (OriginalDragonRunTimeline.ScreenWidth - 1));
        var pointerY = (int)Math.Round(AimY * (OriginalDragonRunTimeline.ScreenHeight - 1));
        for (var frame = _lastProcessedFrame + 1;
             frame <= SourceFrame && frame < OriginalDragonRunTimeline.EndFrame; frame++)
        {
            _lance.Advance(frame, pointerX, pointerY);
            _lastProcessedFrame = frame;
            if (OriginalDragonRunTimeline.TargetAt(frame) is not { } target) continue;
            EyeX = (double)target.X / OriginalDragonRunTimeline.ScreenWidth;
            EyeY = (double)target.Y / OriginalDragonRunTimeline.ScreenHeight;
            HorizontalError += Math.Abs(target.X - LanceX);
            VerticalError += Math.Abs(target.Y - OriginalDragonRunTimeline.MovieTop - LanceY);
            ScoredFrames++;
        }
        if (_elapsedTicks >= durationTicks)
        {
            var hit = OriginalDragonRunScore.Succeeds(_lanceExperience,
                _equipmentBonus, HorizontalError, VerticalError);
            Outcome = hit ? DragonBattleOutcome.Victory : DragonBattleOutcome.Defeat;
            LastMessage = hit ? "The lance finds the dragon's eye."
                : "The lance misses the dragon's eye.";
        }
    }

    public void Withdraw()
    {
        if (Outcome != DragonBattleOutcome.InProgress) return;
        Outcome = DragonBattleOutcome.Withdrawn;
        LastMessage = "You turn from the dragon while escape remains possible.";
    }
}
