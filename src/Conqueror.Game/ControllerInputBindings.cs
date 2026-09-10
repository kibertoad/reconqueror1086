using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Conqueror.Game;

public enum ControllerInputContext
{
    General,
    Home,
    Village,
    Shop,
    Tournament,
    Map,
    Dialogue,
    FieldBattle,
    Siege,
    DragonBattle
}

public static class ControllerInputBindings
{
    public const float PointerSpeed = 360;
    public const float PointerTriggerThreshold = .5f;
    private static readonly IReadOnlyDictionary<Keys, Buttons[]> Common = new Dictionary<Keys, Buttons[]>
    {
        [Keys.Enter] = [Buttons.A],
        [Keys.Escape] = [Buttons.B],
        [Keys.Up] = [Buttons.DPadUp, Buttons.LeftThumbstickUp],
        [Keys.Down] = [Buttons.DPadDown, Buttons.LeftThumbstickDown],
        [Keys.Left] = [Buttons.DPadLeft, Buttons.LeftThumbstickLeft],
        [Keys.Right] = [Buttons.DPadRight, Buttons.LeftThumbstickRight],
        [Keys.Space] = [Buttons.X],
        [Keys.E] = [Buttons.Y],
        [Keys.F9] = [Buttons.Back],
        [Keys.Pause] = [Buttons.Start]
    };

    private static readonly IReadOnlyDictionary<ControllerInputContext, IReadOnlyDictionary<Keys, Buttons[]>> Contextual =
        new Dictionary<ControllerInputContext, IReadOnlyDictionary<Keys, Buttons[]>>
        {
            [ControllerInputContext.Home] = Bind((Keys.F, Buttons.X), (Keys.V, Buttons.Y)),
            [ControllerInputContext.Village] = Bind((Keys.K, Buttons.X), (Keys.I, Buttons.Y)),
            [ControllerInputContext.Shop] = Bind((Keys.B, Buttons.X), (Keys.S, Buttons.Y)),
            [ControllerInputContext.Tournament] = Bind((Keys.C, Buttons.LeftShoulder), (Keys.K, Buttons.Y)),
            [ControllerInputContext.Map] = Bind((Keys.S, Buttons.X), (Keys.P, Buttons.RightShoulder),
                (Keys.O, Buttons.LeftShoulder), (Keys.C, Buttons.RightStick), (Keys.D, Buttons.LeftStick)),
            [ControllerInputContext.Dialogue] = Bind((Keys.D1, Buttons.A), (Keys.D2, Buttons.X),
                (Keys.D3, Buttons.Y), (Keys.D4, Buttons.LeftShoulder), (Keys.D5, Buttons.RightShoulder)),
            [ControllerInputContext.FieldBattle] = Bind((Keys.A, Buttons.A), (Keys.H, Buttons.X),
                (Keys.Q, Buttons.LeftShoulder), (Keys.E, Buttons.RightShoulder), (Keys.C, Buttons.RightStick)),
            [ControllerInputContext.Siege] = BindMany((Keys.W, [Buttons.DPadUp, Buttons.LeftThumbstickUp]),
                (Keys.S, [Buttons.DPadDown, Buttons.LeftThumbstickDown]),
                (Keys.A, [Buttons.DPadLeft, Buttons.LeftThumbstickLeft]),
                (Keys.D, [Buttons.DPadRight, Buttons.LeftThumbstickRight]),
                (Keys.X, [Buttons.RightShoulder]), (Keys.R, [Buttons.LeftShoulder]), (Keys.M, [Buttons.RightStick])),
            [ControllerInputContext.DragonBattle] = Bind((Keys.Space, Buttons.A))
        };

    public static bool IsPressed(Keys key, ControllerInputContext context, GamePadState current, GamePadState previous) =>
        ButtonsFor(key, context).Any(button => current.IsButtonDown(button) && previous.IsButtonUp(button));

    public static bool AnyPressed(GamePadState current, GamePadState previous) =>
        Common.Values.SelectMany(buttons => buttons).Distinct()
            .Any(button => current.IsButtonDown(button) && previous.IsButtonUp(button));

    public static Vector2 MovePointer(Vector2 current, Vector2 stick, double elapsedSeconds)
    {
        if (elapsedSeconds < 0) throw new ArgumentOutOfRangeException(nameof(elapsedSeconds));
        var seconds = (float)Math.Min(elapsedSeconds, .1);
        return new Vector2(
            Math.Clamp(current.X + stick.X * PointerSpeed * seconds, 0, 639),
            Math.Clamp(current.Y - stick.Y * PointerSpeed * seconds, 0, 479));
    }

    public static bool PrimaryPointerPressed(GamePadState current, GamePadState previous) =>
        current.Triggers.Right > PointerTriggerThreshold && previous.Triggers.Right <= PointerTriggerThreshold;

    public static bool PrimaryPointerReleased(GamePadState current, GamePadState previous) =>
        current.Triggers.Right <= PointerTriggerThreshold && previous.Triggers.Right > PointerTriggerThreshold;

    public static bool SecondaryPointerPressed(GamePadState current, GamePadState previous) =>
        current.Triggers.Left > PointerTriggerThreshold && previous.Triggers.Left <= PointerTriggerThreshold;

    public static IReadOnlyList<Buttons> ButtonsFor(Keys key, ControllerInputContext context)
    {
        var result = new List<Buttons>();
        if (Common.TryGetValue(key, out var common)) result.AddRange(common);
        if (Contextual.TryGetValue(context, out var bindings) && bindings.TryGetValue(key, out var contextual))
            result.AddRange(contextual);
        return result.Distinct().ToArray();
    }

    private static IReadOnlyDictionary<Keys, Buttons[]> Bind(params (Keys Key, Buttons Button)[] bindings) =>
        bindings.ToDictionary(binding => binding.Key, binding => new[] { binding.Button });

    private static IReadOnlyDictionary<Keys, Buttons[]> BindMany(params (Keys Key, Buttons[] Buttons)[] bindings) =>
        bindings.ToDictionary(binding => binding.Key, binding => binding.Buttons);
}
