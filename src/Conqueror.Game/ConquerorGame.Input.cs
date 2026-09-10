using Microsoft.Xna.Framework.Input;

namespace Conqueror.Game;

public sealed partial class ConquerorGame
{
    private (bool Click, bool Release, bool RightClick) UpdateControllerPointer(
        GamePadState gamePad, MouseState mouse, double elapsedSeconds)
    {
        if (mouse.X != _lastMouse.X || mouse.Y != _lastMouse.Y) _controllerPointerActive = false;
        var stick = gamePad.ThumbSticks.Right;
        if (Math.Abs(stick.X) >= .15f || Math.Abs(stick.Y) >= .15f)
        {
            _controllerPointer = ControllerInputBindings.MovePointer(_controllerPointer, stick, elapsedSeconds);
            _controllerPointerActive = true;
        }

        var click = ControllerInputBindings.PrimaryPointerPressed(gamePad, _lastGamePad);
        var release = ControllerInputBindings.PrimaryPointerReleased(gamePad, _lastGamePad);
        var rightClick = ControllerInputBindings.SecondaryPointerPressed(gamePad, _lastGamePad);
        if (click || rightClick) _controllerPointerActive = true;
        _controllerPointerPressed = gamePad.Triggers.Right > ControllerInputBindings.PointerTriggerThreshold;
        IsMouseVisible = !_originalAnimations.ContainsKey(OriginalCursorAnimationRole) && !_controllerPointerActive;
        return (click, release, rightClick);
    }

    private static ControllerInputContext ControllerContextFor(Screen screen) => screen switch
    {
        Screen.Home => ControllerInputContext.Home,
        Screen.Village => ControllerInputContext.Village,
        Screen.Shop => ControllerInputContext.Shop,
        Screen.Tournament => ControllerInputContext.Tournament,
        Screen.Map => ControllerInputContext.Map,
        Screen.InnDialogue or Screen.BlacksmithDialogue => ControllerInputContext.Dialogue,
        Screen.FieldBattle => ControllerInputContext.FieldBattle,
        Screen.Siege => ControllerInputContext.Siege,
        Screen.DragonBattle => ControllerInputContext.DragonBattle,
        _ => ControllerInputContext.General
    };
}
