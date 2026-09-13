using Conqueror.Core;

namespace Conqueror.Game;

public enum SiegeForegroundPhase { Approaching, Returning, Holding, Complete }

// CONQUER.EXE 0x54B68-0x55521: row-family target/outer setup, signed 8.8
// velocity, elapsed-millisecond integration, contact reversal, and pose state.
public sealed class SiegeForegroundTrajectory
{
    public const int MaximumUpdateMilliseconds = 166;
    private readonly int _combatRow;
    private readonly int _baseFrame;
    private readonly int _viewportWidth;
    private readonly bool _contacted;
    private readonly int _outerX8;
    private readonly int _outerY8;
    private readonly int _targetX8;
    private readonly int _targetY8;
    private int _x8;
    private int _y8;
    private int _velocityX8;
    private int _velocityY8;
    private double _millisecondRemainder;

    private SiegeForegroundTrajectory(int combatRow, int baseFrame, int viewportWidth, bool contacted,
        int outerX, int outerY, int targetX, int targetY, bool mirror)
    {
        _combatRow = combatRow;
        _baseFrame = baseFrame;
        _viewportWidth = viewportWidth;
        _contacted = contacted;
        _outerX8 = _x8 = checked(outerX << 8);
        _outerY8 = _y8 = checked(outerY << 8);
        _targetX8 = checked(targetX << 8);
        _targetY8 = checked(targetY << 8);
        _velocityX8 = OriginalWeaponCombat.ForegroundVelocityForCombatRow(combatRow, targetX - outerX);
        _velocityY8 = OriginalWeaponCombat.ForegroundVelocityForCombatRow(combatRow, targetY - outerY);
        Mirror = mirror;
        Phase = SiegeForegroundPhase.Approaching;
    }

    public SiegeForegroundPhase Phase { get; private set; }
    public bool Mirror { get; }
    public int AnchorX => _x8 >> 8;
    public int AnchorY => _y8 >> 8;
    public int VelocityX8 => _velocityX8;
    public int VelocityY8 => _velocityY8;
    public int TargetX => _targetX8 >> 8;
    public int TargetY => _targetY8 >> 8;
    public int Frame => Phase switch
    {
        SiegeForegroundPhase.Complete => -1,
        SiegeForegroundPhase.Returning when Math.Abs(_targetX8 - _x8) >= (_viewportWidth << 6) => _baseFrame,
        _ when _combatRow < 23 && Math.Abs(_targetX8 - _x8) < (_viewportWidth << 6) => _baseFrame + 1,
        _ => _baseFrame + 2
    };

    public static SiegeForegroundTrajectory Create(int combatRow, int baseFrame, int spriteWidth, int spriteHeight,
        int targetX, int targetY, int viewportWidth, int viewportHeight, bool contacted, Random random)
    {
        ArgumentNullException.ThrowIfNull(random);
        if ((uint)combatRow >= OriginalWeaponCombat.CombatRowCount) throw new ArgumentOutOfRangeException(nameof(combatRow));
        if (spriteWidth <= 0) throw new ArgumentOutOfRangeException(nameof(spriteWidth));
        if (spriteHeight <= 0) throw new ArgumentOutOfRangeException(nameof(spriteHeight));
        if (viewportWidth <= 0) throw new ArgumentOutOfRangeException(nameof(viewportWidth));
        if (viewportHeight <= 0) throw new ArgumentOutOfRangeException(nameof(viewportHeight));

        var mirror = false;
        int outerX;
        int outerY;
        if (combatRow <= 3)
        {
            targetX -= random.Next(8);
            targetY = Math.Max(targetY - random.Next(16), viewportHeight - spriteHeight);
            outerX = targetX;
            outerY = targetY + viewportHeight / 2;
        }
        else if (combatRow <= 14)
        {
            targetX += 16 - random.Next(40);
            targetY = Math.Max(targetY - random.Next(40), viewportHeight - spriteHeight);
            if (viewportHeight / 3 < targetY)
            {
                outerY = targetY - viewportHeight / 2;
                (outerX, mirror) = HorizontalOuterAnchor(targetX, viewportWidth);
            }
            else
            {
                outerX = targetX;
                outerY = targetY + viewportHeight / 2;
            }
        }
        else if (combatRow <= 22)
        {
            targetX += 16 - random.Next(32);
            targetY = Math.Max(targetY - random.Next(24), viewportHeight - spriteHeight);
            (outerX, mirror) = HorizontalOuterAnchor(targetX, viewportWidth);
            outerY = targetY - 0x38;
        }
        else
        {
            // 0x54E9D-0x54F12: rows 23-24 center base frame 30 on pointer x
            // and rise from the viewport bottom by exactly one sprite height.
            targetX -= spriteWidth / 2;
            targetY = viewportHeight - spriteHeight;
            outerX = targetX;
            outerY = viewportHeight;
        }

        return new SiegeForegroundTrajectory(combatRow, baseFrame, viewportWidth, contacted,
            outerX, outerY, targetX, targetY, mirror);
    }

    public void Advance(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        if (Phase is SiegeForegroundPhase.Holding or SiegeForegroundPhase.Complete) return;
        _millisecondRemainder += Math.Min(elapsedSeconds * 1000, MaximumUpdateMilliseconds);
        var milliseconds = (int)_millisecondRemainder;
        _millisecondRemainder -= milliseconds;
        for (var millisecond = 0; millisecond < milliseconds && Phase != SiegeForegroundPhase.Complete; millisecond++)
            AdvanceMillisecond();
    }

    private void AdvanceMillisecond()
    {
        _x8 += _velocityX8;
        _y8 += _velocityY8;
        if (Phase == SiegeForegroundPhase.Approaching && Reached(_x8, _targetX8, _velocityX8) &&
            Reached(_y8, _targetY8, _velocityY8))
        {
            _x8 = _targetX8;
            _y8 = _targetY8;
            if (_combatRow >= 23)
            {
                // 0x550BC/0x553F9: the ranged pose remains at its target;
                // only later foreground setup or siege teardown replaces it.
                _velocityX8 = 0;
                _velocityY8 = 0;
                Phase = SiegeForegroundPhase.Holding;
                return;
            }
            if (!_contacted)
            {
                Phase = SiegeForegroundPhase.Complete;
                return;
            }
            _velocityX8 = -_velocityX8;
            _velocityY8 = -_velocityY8 / 2;
            Phase = SiegeForegroundPhase.Returning;
        }
        else if (Phase == SiegeForegroundPhase.Returning &&
                 Reached(_x8, _outerX8, _velocityX8) && Reached(_y8, _outerY8, _velocityY8))
        {
            Phase = SiegeForegroundPhase.Complete;
        }
    }

    private static (int X, bool Mirror) HorizontalOuterAnchor(int targetX, int viewportWidth) =>
        viewportWidth / 2 >= targetX
            ? (targetX + viewportWidth / 2, false)
            : (targetX - viewportWidth / 2, true);

    private static bool Reached(int current, int target, int velocity) => velocity switch
    {
        > 0 => current >= target,
        < 0 => current <= target,
        _ => current == target
    };
}
