using Conqueror.Core;

namespace Conqueror.Game;

/// <summary>Mouse controls for the replacement overhead field battle.</summary>
public static class FieldBattlePointerControls
{
    public static IReadOnlyList<FieldBattleOrderButton> Buttons { get; } =
    [
        new("HOLD", UnitOrder.Hold, false, new(5, 455, 85, 22)),
        new("ADVANCE", UnitOrder.Advance, false, new(95, 455, 85, 22)),
        new("FLANK L", UnitOrder.FlankLeft, false, new(185, 455, 85, 22)),
        new("FLANK R", UnitOrder.FlankRight, false, new(275, 455, 85, 22)),
        new("RETREAT", UnitOrder.Withdraw, false, new(365, 455, 85, 22)),
        new("CAPTAINS", UnitOrder.Captains, true, new(455, 455, 85, 22)),
        new("ALL BACK", UnitOrder.Withdraw, true, new(545, 455, 85, 22))
    ];

    public static FieldBattleOrderButton? ButtonAt(int x, int y) =>
        Buttons.FirstOrDefault(button => button.Bounds.Contains(x, y));

    public static UnitType? FriendlyUnitAt(FieldBattleSession battle,
        UnitType selectedUnit, int x, int y)
    {
        ArgumentNullException.ThrowIfNull(battle);
        var draws = FieldBattlePresentation.SpriteDrawsFor(
            battle.Squads, selectedUnit, battle.TickNumber);
        for (var index = draws.Count - 1; index >= 0; index--)
        {
            var draw = draws[index];
            var squad = battle.Squads[draw.SquadIndex];
            if (squad.Friendly && x >= draw.X && x < draw.X +
                    OriginalStrategicInteractiveEncounterPresentation.UnitSpriteWidth
                && y >= draw.Y && y < draw.Y +
                    OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHeight)
                return squad.Type;
        }
        return null;
    }
}

public sealed record FieldBattleOrderButton(
    string Label, UnitOrder Order, bool AllUnits, UiBounds Bounds);
