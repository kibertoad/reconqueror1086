using Conqueror.Core;
using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private string? _activeEventMovieRole;
    private int _practiceJoustLanceX = 225;
    private int _practiceJoustLanceY = 150;

    private void UpdatePracticeJoustPointer(MouseState mouse)
    {
        if (_activeEventMovieRole != "Practice.Joust") return;
        var (pointerX, pointerY) = OriginalPoint(mouse);
        _practiceJoustLanceX = Math.Clamp(pointerX,
            OriginalDragonLanceSelection.MinimumX, OriginalDragonLanceSelection.MaximumX);
        _practiceJoustLanceY = Math.Clamp(pointerY - 90,
            OriginalDragonLanceSelection.MinimumY, OriginalDragonLanceSelection.MaximumY);
    }
}
