using Conqueror.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private void StartDragonRunMovie()
    {
        _dragonRunMovie?.Dispose();
        _dragonRunMovie = CreateMovie("Dragon.Run")
            ?? throw new InvalidDataException("The required original dragon run movie could not be decoded.");
    }

    private void UpdateDragonBattle(KeyboardState keys, GamePadState gamePad,
        MouseState mouse, GameTime gameTime)
    {
        if (_dragonBattle is null) { _screen = Screen.Map; return; }
        _dragonRunMovie?.Update(gameTime.ElapsedGameTime);
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
        _dragonRunMovie?.Dispose();
        _dragonRunMovie = null;
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
        if (_dragonRunMovie is null)
            throw new InvalidOperationException("Dragon battle requires its original run movie.");
        _batch.Draw(_dragonRunMovie.Texture, new Rectangle(0, 144, 1024, 480), Color.White);

        var eye = new Point((int)(_dragonBattle.EyeX * 1024), (int)(_dragonBattle.EyeY * 768));
        if (_dragonBattle.EyeVisible)
            DrawOutline(new Rectangle(eye.X - 8, eye.Y - 8, 16, 16), Color.Red, 2);
        var aim = new Point((int)(_dragonBattle.AimX * 1024), (int)(_dragonBattle.AimY * 768));
        if (!_originalAnimations.TryGetValue("Dragon.Lance", out var lances) || lances.Frames.Count != 25)
            throw new InvalidOperationException("Dragon battle requires its verified 25-frame lance foreground.");
        // The aim-to-source-lance transform remains host policy. Once mapped
        // into the source's bounded lance coordinates, use its frame selector.
        var sourceX = OriginalDragonLanceSelection.MinimumX + (int)Math.Round(
            _dragonBattle.AimX * (OriginalDragonLanceSelection.MaximumX - OriginalDragonLanceSelection.MinimumX));
        var sourceY = OriginalDragonLanceSelection.MinimumY + (int)Math.Round(
            _dragonBattle.AimY * (OriginalDragonLanceSelection.MaximumY - OriginalDragonLanceSelection.MinimumY));
        var lanceIndex = OriginalDragonLanceSelection.FrameFor(sourceX, sourceY);
        var lance = lances.Frames[lanceIndex];
        var width = lance.Width * 1024 / 640;
        var height = lance.Height * 768 / 480;
        _batch.Draw(lance, new Rectangle(aim.X - width / 2, aim.Y - height / 2,
            width, height), Color.White);
        Fill(new Rectangle(aim.X - 15, aim.Y - 2, 31, 4), Color.Gold);
        Fill(new Rectangle(aim.X - 2, aim.Y - 15, 4, 31), Color.Gold);

        Fill(new Rectangle(70, 690, 884, 18), new Color(25, 20, 15));
        Fill(new Rectangle(70, 690, (int)(884 * _dragonBattle.RemainingSeconds /
            DragonBattleSession.Rules.DurationSeconds), 18), Color.Goldenrod);
        DrawText("KEEP LANCE ON THE EYE   ESC RETREAT", 155, 645, Color.White, 2);
    }
}
