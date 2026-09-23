using Conqueror.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private string? _activeEventMovieRole;
    private OriginalPracticeJoustLance _practiceJoustLance = new();
    private OriginalPracticeJoustTrial _practiceJoustTrial = new();
    private OriginalPracticeJoustResult? _practiceJoustResult;
    private int _practiceJoustLastFrame = -1;
    private IReadOnlyList<Texture2D>? _practiceJoustLances;

    private void LoadPracticeJoustLances(SmackerMoviePlayer movie)
    {
        var id = _importedContent.FindId("indexed-animation", ":lance1.csf")
            ?? throw new InvalidDataException("Practice lance asset is unavailable.");
        var source = _importedContent.DecodeCsf(id)
            ?? throw new InvalidDataException("Practice lance frames are unavailable.");
        if (source.Chunks.Count != 25)
            throw new InvalidDataException("Practice requires the original 25 lance frames.");
        var palette = movie.CurrentPaletteRgb;
        var frames = new List<Texture2D>(25);
        try
        {
            foreach (var chunk in source.Chunks)
            {
                var frame = source.DecodeFrame(chunk);
                var texture = new Texture2D(GraphicsDevice, frame.Width, frame.Height,
                    false, SurfaceFormat.Color);
                frames.Add(texture);
                texture.SetData(frame.ToRgba(palette));
            }
            _practiceJoustLances = frames;
        }
        catch
        {
            foreach (var texture in frames) texture.Dispose();
            throw;
        }
    }

    private void DisposePracticeJoustLances()
    {
        if (_practiceJoustLances is null) return;
        foreach (var texture in _practiceJoustLances) texture.Dispose();
        _practiceJoustLances = null;
    }

    private void UpdateEventMovie(GameTime gameTime, MouseState mouse, bool pressAny, bool click)
    {
        if (_eventMovie is null) { FinishEventMovie(); return; }
        _eventMovie.Update(gameTime.ElapsedGameTime);
        UpdatePracticeJoustPointer(mouse);
        if (!_eventMovie.IsComplete && (_activeEventMovieRole == "Practice.Joust" || !pressAny && !click))
            return;
        if (_activeEventMovieRole == "Practice.Joust" && _eventMovie.IsComplete)
            FinishPracticeJoust();
        else FinishEventMovie();
    }

    private void UpdatePracticeJoustPointer(MouseState mouse)
    {
        if (_activeEventMovieRole != "Practice.Joust") return;
        var (pointerX, pointerY) = OriginalPoint(mouse);
        var frame = _eventMovie?.CurrentFrameIndex ?? -1;
        while (_practiceJoustLastFrame < frame)
        {
            _practiceJoustLance.Advance(++_practiceJoustLastFrame, pointerX, pointerY);
            _practiceJoustTrial.RecordFrame(_practiceJoustLastFrame,
                _practiceJoustLance.X, _practiceJoustLance.Y);
        }
    }

    private void FinishPracticeJoust()
    {
        _practiceJoustResult = _practiceJoustTrial.Resolve(() => Random.Shared.Next(100));
        FinishEventMovie();
        _notice = _practiceJoustResult.Outcome switch
        {
            OriginalPracticeJoustOutcome.PlayerHit => "PRACTICE HIT",
            OriginalPracticeJoustOutcome.OpponentHit => "THE OPPONENT SCORES",
            _ => "BOTH RIDERS MISS"
        };
    }

    private bool UpdatePracticeJoustResult(Func<Keys, bool> press, bool click)
    {
        if (_practiceJoustResult is null) return false;
        if (press(Keys.Enter) || click) _practiceJoustResult = null;
        return true;
    }

    private void DrawPracticeJoustResult()
    {
        if (_practiceJoustResult is not { } result) return;
        Fill(new Rectangle(136, 255, 752, 246), new Color(12, 18, 12));
        DrawText(_notice, 213, 292, Color.Gold, 3);
        DrawText($"LANCE ERROR X {result.HorizontalError}  Y {result.VerticalError}",
            215, 369, Color.White, 2);
        DrawText("ENTER OR CLICK TO CONTINUE", 265, 443, Color.White, 2);
    }
}
