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
    private double _elapsed;

    public DragonBattleOutcome Outcome { get; private set; }
    public double AimX { get; private set; } = .5;
    public double AimY { get; private set; } = .72;
    public double EyeX { get; private set; } = 277d / OriginalDragonRunTimeline.ScreenWidth;
    public double EyeY { get; private set; } = 215d / OriginalDragonRunTimeline.ScreenHeight;
    public int SourceFrame { get; private set; }
    public bool EyeVisible => SourceFrame is >= OriginalDragonRunTimeline.FirstTargetFrame
        and < OriginalDragonRunTimeline.EndFrame;
    public double RemainingSeconds => Math.Max(0, Rules.DurationSeconds - _elapsed);
    public int HorizontalError { get; private set; }
    public int VerticalError { get; private set; }
    public int ScoredFrames { get; private set; }
    public int ScoreThreshold => OriginalDragonRunScore.Threshold(_lanceExperience,
        OriginalDragonRunScore.FullEquipmentBonus);
    public string LastMessage { get; private set; } = "Keep the lance aligned with the dragon's eye.";

    public DragonBattleSession(int lanceExperience)
    {
        _lanceExperience = lanceExperience;
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
        var previousFrame = SourceFrame;
        _elapsed = Math.Min(Rules.DurationSeconds, _elapsed + elapsedSeconds);
        SourceFrame = OriginalDragonRunTimeline.FrameAt(TimeSpan.FromSeconds(_elapsed));
        for (var frame = Math.Max(previousFrame + 1, OriginalDragonRunTimeline.FirstTargetFrame);
             frame < Math.Min(SourceFrame + 1, OriginalDragonRunTimeline.EndFrame); frame++)
        {
            var target = OriginalDragonRunTimeline.TargetAt(frame)!.Value;
            EyeX = (double)target.X / OriginalDragonRunTimeline.ScreenWidth;
            EyeY = (double)target.Y / OriginalDragonRunTimeline.ScreenHeight;
            // The host aim-to-lance geometry remains provisional. Score its
            // source-screen position once per distinct movie frame.
            var aimX = (int)Math.Round(AimX * (OriginalDragonRunTimeline.ScreenWidth - 1));
            var aimY = (int)Math.Round(AimY * (OriginalDragonRunTimeline.ScreenHeight - 1));
            HorizontalError += Math.Abs(target.X - aimX);
            VerticalError += Math.Abs(target.Y - aimY);
            ScoredFrames++;
        }
        if (_elapsed >= Rules.DurationSeconds)
        {
            var hit = OriginalDragonRunScore.Succeeds(_lanceExperience,
                OriginalDragonRunScore.FullEquipmentBonus, HorizontalError, VerticalError);
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
