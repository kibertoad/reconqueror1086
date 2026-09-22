namespace Conqueror.Core;

/// <summary>
/// Presentation bridge for the legacy field-battle adapter. It deliberately
/// uses the owned strategic battlefield's unit vocabulary without claiming
/// that this adapter's grid mechanics or positions reproduce the original
/// resolver. The source frame families themselves are documented by
/// <see cref="OriginalStrategicInteractiveEncounterPresentation"/>.
/// </summary>
public static class FieldBattlePresentation
{
    /// <summary>
    /// Both assets are verified runtime requirements. The legacy adapter may
    /// retain provisional formation mechanics, but it must never present a
    /// generated battlefield or substitute unit art.
    /// </summary>
    public const string BackgroundArtRole = "Encounter.Strategic.Background";
    public const string UnitAnimationRole = "Encounter.Strategic.Units";
    public const int LogicalWidth = 640;
    public const int BattlefieldHeight = 455;

    /// <summary>
    /// Places each adapter formation by its stable logical grid coordinate,
    /// then selects a neutral, facing-inward MEN8 sprite. This is host-owned
    /// layout policy for the still-provisional adapter, not a source claim.
    /// </summary>
    public static IReadOnlyList<FieldBattleSpriteDraw> SpriteDrawsFor(
        IReadOnlyList<BattleSquad> squads, UnitType selectedType, int animationPhase)
    {
        ArgumentNullException.ThrowIfNull(squads);
        var phase = Math.Abs(animationPhase % 5);
        var draws = new List<FieldBattleSpriteDraw>(squads.Count);
        for (var index = 0; index < squads.Count; index++)
        {
            var squad = squads[index] ?? throw new ArgumentException("Squad list cannot contain null.", nameof(squads));
            if (squad.Count <= 0) continue;
            var x = Math.Clamp(squad.X, 0, FieldBattleSession.Rules.Width - 1);
            var y = Math.Clamp(squad.Y, 0, FieldBattleSession.Rules.Height - 1);
            var centerX = OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHalfWidth
                + x * (LogicalWidth - OriginalStrategicInteractiveEncounterPresentation.UnitSpriteWidth)
                    / (FieldBattleSession.Rules.Width - 1);
            var centerY = OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHalfHeight
                + y * (BattlefieldHeight - OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHeight)
                    / (FieldBattleSession.Rules.Height - 1);
            draws.Add(new FieldBattleSpriteDraw(index, FrameFor(squad, phase),
                centerX - OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHalfWidth,
                centerY - OriginalStrategicInteractiveEncounterPresentation.UnitSpriteHalfHeight,
                squad.Friendly && squad.Type == selectedType));
        }

        return draws.OrderBy(draw => draw.Y).ThenBy(draw => draw.X).ThenBy(draw => draw.SquadIndex).ToArray();
    }

    private static int FrameFor(BattleSquad squad, int phase)
    {
        var category = squad.Type switch
        {
            UnitType.Swordsmen => 0,
            UnitType.Halberdiers => 120,
            UnitType.Knights => 240,
            _ => throw new ArgumentOutOfRangeException(nameof(squad)),
        };
        var side = squad.Friendly ? 0 : 360;
        var heading = squad.Friendly ? 3 : 7;
        return side + category + heading * 5 + phase;
    }
}

/// <summary>One source-art sprite emitted for a legacy adapter formation.</summary>
public readonly record struct FieldBattleSpriteDraw(int SquadIndex, int Frame, int X, int Y, bool Selected);
