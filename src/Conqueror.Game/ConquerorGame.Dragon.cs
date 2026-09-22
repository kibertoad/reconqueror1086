using Conqueror.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private void UpdateDragonBattle(Func<Keys, bool> press, KeyboardState keys, GamePadState gamePad,
        MouseState mouse, bool click, GameTime gameTime)
    {
        if (_dragonBattle is null) { _screen = Screen.Map; return; }
        var seconds = gameTime.ElapsedGameTime.TotalSeconds;
        var horizontal = (keys.IsKeyDown(Keys.Right) || keys.IsKeyDown(Keys.D) ? 1d : 0)
            - (keys.IsKeyDown(Keys.Left) || keys.IsKeyDown(Keys.A) ? 1d : 0);
        var vertical = (keys.IsKeyDown(Keys.Down) || keys.IsKeyDown(Keys.S) ? 1d : 0)
            - (keys.IsKeyDown(Keys.Up) || keys.IsKeyDown(Keys.W) ? 1d : 0);
        horizontal += gamePad.ThumbSticks.Left.X;
        vertical -= gamePad.ThumbSticks.Left.Y;
        _dragonBattle.MoveAim(horizontal, vertical, seconds);

        if (_controllerPointerActive)
            _dragonBattle.SetAim(_controllerPointer.X / 639d, _controllerPointer.Y / 479d);
        else if (mouse.X != _lastMouse.X || mouse.Y != _lastMouse.Y)
        {
            var point = OriginalPoint(mouse);
            _dragonBattle.SetAim(point.X / 639d, point.Y / 479d);
        }

        _dragonBattle.Tick(seconds);
        if (press(Keys.Space) || click) _dragonBattle.Strike();
        _notice = _dragonBattle.LastMessage.ToUpperInvariant();
        if (_dragonBattle.Outcome != DragonBattleOutcome.InProgress) ResolveDragonBattle();
    }

    private void ResolveDragonBattle()
    {
        if (_dragonBattle is null) return;
        var battle = _dragonBattle;
        var outcome = battle.Outcome;
        _campaign.FinishDragonBattle(battle);
        _dragonBattle = null;
        _notice = battle.LastMessage.ToUpperInvariant();
        _screen = outcome == DragonBattleOutcome.Withdrawn ? Screen.Map : Screen.Ending;
        Autosave();
        if (outcome == DragonBattleOutcome.Victory)
            PlayEventMovieSequence(["Ending.DragonVictory", "Ending.DragonInvestiture"], Screen.Ending);
        else if (outcome == DragonBattleOutcome.Defeat) PlayEventMovie("Ending.DragonDefeat", Screen.Ending);
        else PlayEventMovie("Dragon.Retreat", Screen.Map);
    }

    private void DrawDragonBattle()
    {
        if (_dragonBattle is null) return;
        if (!DrawOriginal("Dragon.Background", new Rectangle(0, 0, 1024, 768)))
            throw new InvalidOperationException("Dragon battle requires its verified original battlefield art.");

        var eye = new Point((int)(_dragonBattle.EyeX * 1024), (int)(_dragonBattle.EyeY * 768));
        var radius = Math.Max(8, (int)(_dragonBattle.HitRadius * 768));
        DrawOutline(new Rectangle(eye.X - radius, eye.Y - radius, radius * 2, radius * 2), Color.Red, 3);
        var aim = new Point((int)(_dragonBattle.AimX * 1024), (int)(_dragonBattle.AimY * 768));
        Fill(new Rectangle(aim.X - 15, aim.Y - 2, 31, 4), Color.Gold);
        Fill(new Rectangle(aim.X - 2, aim.Y - 15, 4, 31), Color.Gold);

        Fill(new Rectangle(70, 690, 884, 18), new Color(25, 20, 15));
        Fill(new Rectangle(70, 690, (int)(884 * _dragonBattle.RemainingSeconds /
            DragonBattleSession.Rules.DurationSeconds), 18), Color.Goldenrod);
        DrawText("MOVE TO AIM   SPACE/CLICK THRUST   ESC RETREAT", 155, 645, Color.White, 2);
    }
}
