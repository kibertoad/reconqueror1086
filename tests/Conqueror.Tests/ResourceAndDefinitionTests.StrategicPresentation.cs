using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicInteractiveEncounterUsesItsOwnedBackgroundAndUnitSequence()
    {
        Assert.Contains(new ImportedArtDefinition("Encounter.Strategic.Background", "image", ":battle.pcx"),
            ImportedArt.Definitions);
        Assert.Contains(new ImportedAnimationDefinition("Encounter.Strategic.Units", ":men8.csf",
                "Encounter.Strategic.Background"),
            ImportedAnimations.Definitions);
    }
}
