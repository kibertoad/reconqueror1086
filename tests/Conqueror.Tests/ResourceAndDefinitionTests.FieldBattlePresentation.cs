using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OverheadBattleMouseSelectsFriendlySpriteAndMapsVisibleOrderButtons()
    {
        static Army ArmyWith(int swords, int halberds, int knights)
        {
            var army = new Army();
            army.Units[UnitType.Swordsmen] = swords;
            army.Units[UnitType.Halberdiers] = halberds;
            army.Units[UnitType.Knights] = knights;
            return army;
        }

        var battle = new FieldBattleSession(ArmyWith(12, 10, 8), ArmyWith(10, 9, 7), 1086);
        var draws = FieldBattlePresentation.SpriteDrawsFor(battle.Squads, UnitType.Swordsmen, 0);
        var knights = Assert.Single(draws, draw =>
            battle.Squads[draw.SquadIndex].Friendly
            && battle.Squads[draw.SquadIndex].Type == UnitType.Knights);
        Assert.Equal(UnitType.Knights, FieldBattlePointerControls.FriendlyUnitAt(
            battle, UnitType.Swordsmen, knights.X + 45, knights.Y + 45));
        var enemy = Assert.Single(draws, draw =>
            !battle.Squads[draw.SquadIndex].Friendly
            && battle.Squads[draw.SquadIndex].Type == UnitType.Knights);
        Assert.Null(FieldBattlePointerControls.FriendlyUnitAt(
            battle, UnitType.Swordsmen, enemy.X + 45, enemy.Y + 45));
        Assert.Equal(UnitOrder.Advance, FieldBattlePointerControls.ButtonAt(100, 460)?.Order);
        Assert.True(FieldBattlePointerControls.ButtonAt(550, 460)?.AllUnits);
        Assert.Null(FieldBattlePointerControls.ButtonAt(100, 440));
    }

    [Fact]
    public void LegacyFieldBattlePresentationUsesTheMappedMen8FamiliesAndStableGridPlacement()
    {
        Assert.Equal("Encounter.Strategic.Background", FieldBattlePresentation.BackgroundArtRole);
        Assert.Equal("Encounter.Strategic.Units", FieldBattlePresentation.UnitAnimationRole);
        Assert.Contains(new ImportedArtDefinition(FieldBattlePresentation.BackgroundArtRole, "image", ":battle.pcx"),
            ImportedArt.Definitions);
        Assert.Contains(new ImportedAnimationDefinition(FieldBattlePresentation.UnitAnimationRole, ":men8.csf",
                FieldBattlePresentation.BackgroundArtRole), ImportedAnimations.Definitions);
        var squads = new[]
        {
            new BattleSquad { Type = UnitType.Swordsmen, Friendly = true, Count = 4, X = 1, Y = 2 },
            new BattleSquad { Type = UnitType.Knights, Friendly = false, Count = 7, X = 12, Y = 6 },
            new BattleSquad { Type = UnitType.Halberdiers, Friendly = true, Count = 0, X = 4, Y = 4 },
        };

        var draws = FieldBattlePresentation.SpriteDrawsFor(squads, UnitType.Swordsmen, animationPhase: 6);

        Assert.Equal([
            new FieldBattleSpriteDraw(0, 16, 42, 91, true),
            new FieldBattleSpriteDraw(1, 636, 507, 273, false),
        ], draws);
        Assert.All(draws, draw => Assert.InRange(draw.Frame, 0,
            OriginalStrategicInteractiveEncounterPresentation.UnitFrameCount - 1));
    }
}
