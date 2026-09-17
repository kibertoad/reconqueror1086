using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void AcquisitionTextureClosureIncludesStructuralFacesAndEveryBillboardSector()
    {
        var source = SyntheticScene();
        var scene = DynamixSceneDecoder.Decode(source.Viewer, source.Scenario, source.Map, source.Blocks);
        Assert.Equal([12, 13, 14, 15], scene.Blocks[0].RaycastTextureReferences());

        var billboard = scene.Blocks[0] with
        {
            Kind = 4,
            Behavior = 0,
            Surface0 = 20,
            Surface2 = 8
        };
        Assert.Equal(Enumerable.Range(20, 8), billboard.RaycastTextureReferences().Order());

        var mirrored = billboard with { Behavior = 4 };
        Assert.Equal(Enumerable.Range(20, 5), mirrored.RaycastTextureReferences().Order());
    }
}
