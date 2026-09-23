using Conqueror.Core;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private string? _activeEventMovieRole;
    private OriginalPracticeJoustLance _practiceJoustLance = new();
    private int _practiceJoustLastFrame = -1;

    private void UpdatePracticeJoustPointer(MouseState mouse)
    {
        if (_activeEventMovieRole != "Practice.Joust") return;
        var (pointerX, pointerY) = OriginalPoint(mouse);
        var frame = _eventMovie?.CurrentFrameIndex ?? -1;
        while (_practiceJoustLastFrame < frame)
            _practiceJoustLance.Advance(++_practiceJoustLastFrame, pointerX, pointerY);
    }
}
