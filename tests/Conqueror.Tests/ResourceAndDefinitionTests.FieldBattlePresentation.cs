using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void LegacyFieldBattlePresentationUsesTheMappedMen8FamiliesAndStableGridPlacement()
    {
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
