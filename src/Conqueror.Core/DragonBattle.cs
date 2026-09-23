namespace Conqueror.Core;

public enum DragonBattleOutcome { InProgress, Victory, Defeat, Withdrawn }

public sealed record DragonBattleDefinition(
    double DurationSeconds,
    double BaseEyeRadius,
    double StrengthRadiusBonus,
    double MaximumEyeRadius,
    double AimSpeed);

public sealed class DragonBattleSession
{
    public static readonly DragonBattleDefinition Rules = new(
        OriginalDragonRunTimeline.EndFrame * OriginalDragonRunTimeline.FrameMilliseconds / 1000d,
        .055, .004, .12, .65);

    private readonly int _strength;
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
    public double HitRadius => Math.Min(Rules.MaximumEyeRadius,
        Rules.BaseEyeRadius + Math.Max(0, _strength - 16) * Rules.StrengthRadiusBonus);
    public string LastMessage { get; private set; } = "Steady the lance and aim for the dragon's eye.";

    public DragonBattleSession(int strength)
    {
        _strength = strength;
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
        _elapsed = Math.Min(Rules.DurationSeconds, _elapsed + elapsedSeconds);
        SourceFrame = OriginalDragonRunTimeline.FrameAt(TimeSpan.FromSeconds(_elapsed));
        if (OriginalDragonRunTimeline.TargetAt(SourceFrame) is { } target)
        {
            EyeX = (double)target.X / OriginalDragonRunTimeline.ScreenWidth;
            EyeY = (double)target.Y / OriginalDragonRunTimeline.ScreenHeight;
        }
        if (_elapsed >= Rules.DurationSeconds)
        {
            Outcome = DragonBattleOutcome.Defeat;
            LastMessage = "The dragon strikes before you lower the lance.";
        }
    }

    public bool Strike()
    {
        if (Outcome != DragonBattleOutcome.InProgress) return false;
        var dx = AimX - EyeX;
        var dy = AimY - EyeY;
        // Exact source alignment scoring is still unresolved; this retained
        // one-thrust hit radius is a provisional host interaction rule.
        var hit = EyeVisible && dx * dx + dy * dy <= HitRadius * HitRadius;
        Outcome = hit ? DragonBattleOutcome.Victory : DragonBattleOutcome.Defeat;
        LastMessage = hit
            ? "The lance finds the dragon's eye."
            : "Your only thrust misses the dragon's eye.";
        return hit;
    }

    public void Withdraw()
    {
        if (Outcome != DragonBattleOutcome.InProgress) return;
        Outcome = DragonBattleOutcome.Withdrawn;
        LastMessage = "You turn from the dragon while escape remains possible.";
    }
}
