using System.Buffers.Binary;
using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void ImportedDoorActionImmediatelyUsesItsExplicitSceneStateTarget()
    {
        var source = DoorScene(0x12, target: 0);
        var layout = ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));
        var door = Assert.Single(layout.Objects, item => (item.X, item.Y) == (2, 1));
        Assert.Equal([(1, SiegeTile.Door), (-1, SiegeTile.Floor)],
            door.Stages.Select(stage => (stage.VisualId, stage.Tile)));

        var siege = new SiegeSession(new Player(), new Army(), 0, 1, layout);
        Assert.Empty(SiegeViewProjection.ProjectObjects(siege));
        Assert.Equal(SiegeAction.DoorOpened, siege.Interact());
        Assert.Equal((1, -1, SiegeTile.Floor),
            (siege.ObjectAt(2, 1)!.State, siege.ObjectAt(2, 1)!.VisualId, siege.TileAt(2, 1)));
        Assert.Empty(SiegeViewProjection.ProjectObjects(siege));
        Assert.Equal(SiegeAction.Moved, siege.Move(true));
    }

    [Fact]
    public void ImportedDoorWithoutTheActionBitRemainsClosed()
    {
        var source = DoorScene(3, target: 0);
        var layout = ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));
        var siege = new SiegeSession(new Player(), new Army(), 0, 1, layout);

        Assert.Equal(SiegeAction.None, siege.Interact());
        Assert.Equal(SiegeTile.Door, siege.TileAt(2, 1));
        Assert.Equal(SiegeAction.Blocked, siege.Move(true));
    }

    [Fact]
    public void ImportedDoorCanResolveToItsExplicitBlockingTarget()
    {
        var source = DoorScene(0x12, target: 0, blockingTarget: true);
        var layout = ImportedSiegeLayouts.Convert(DynamixSceneDecoder.Decode(
            source.Viewer, source.Scenario, source.Map, source.Blocks));
        var siege = new SiegeSession(new Player(), new Army(), 0, 1, layout);

        Assert.Equal(SiegeAction.Blocked, siege.Interact());
        Assert.Equal((1, 0, SiegeTile.Wall),
            (siege.ObjectAt(2, 1)!.State, siege.ObjectAt(2, 1)!.VisualId, siege.TileAt(2, 1)));
        Assert.Equal(SiegeAction.Blocked, siege.Move(true));
    }

    private static (byte[] Viewer, byte[] Scenario, byte[] Map, byte[] Blocks) DoorScene(
        int behavior, int target, bool blockingTarget = false)
    {
        var viewer = new byte[DynamixSceneDecoder.ViewerSize];
        WriteDoorInt(viewer, 0, 1 << 8);
        WriteDoorInt(viewer, 4, 1 << 8);
        WriteDoorInt(viewer, 12, 16384);
        var scenario = new byte[DynamixSceneDecoder.ScenarioSize];
        WriteDoorInt(scenario, 20, 2);
        WriteDoorInt(scenario, 24, 2);
        var blocks = new byte[2 * DynamixSceneDecoder.BlockSize];
        WriteDoorBlock(blocks, 0, blockingTarget ? 1 : 0, blockingTarget ? 2 : 1, 0,
            blockingTarget ? "wall" : "ground");
        WriteDoorBlock(blocks, 1, 1, behavior, target, "door");
        var map = new byte[DynamixSceneDecoder.MapSize];
        BinaryPrimitives.WriteUInt16LittleEndian(
            map.AsSpan((2 * DynamixScene.MapHeight + 1) * sizeof(ushort), sizeof(ushort)), 1);
        return (viewer, scenario, map, blocks);
    }

    private static void WriteDoorBlock(byte[] blocks, int index, int kind, int behavior, int target, string name)
    {
        var offset = index * DynamixSceneDecoder.BlockSize;
        WriteDoorInt(blocks, offset, kind);
        WriteDoorInt(blocks, offset + 4, behavior);
        WriteDoorInt(blocks, offset + 64, target);
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(blocks, offset + 78);
        blocks[offset + 94] = 0xcc;
        blocks[offset + 95] = 0xcc;
    }

    private static void WriteDoorInt(byte[] bytes, int offset, int value) =>
        BinaryPrimitives.WriteInt32LittleEndian(bytes.AsSpan(offset, sizeof(int)), value);
}
