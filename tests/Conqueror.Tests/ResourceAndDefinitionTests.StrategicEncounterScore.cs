using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicEncounterScoreModifierUsesHonorFameAndOnlyTheDistinguishedRecordBonus()
    {
        var player = new Player
        {
            Stats = new CharacterStats(10, 10, 10, 10, 13),
            Fame = 9,
        };

        Assert.Equal(3, OriginalStrategicEncounterStaging.PlayerScoreModifier(player, false));
        Assert.Equal(7, OriginalStrategicEncounterStaging.PlayerScoreModifier(player, true));
    }
}
