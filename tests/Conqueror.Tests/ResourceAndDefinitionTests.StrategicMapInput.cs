using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicMapHitTestUsesOrderedSourceRectangles()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.CurrentX = 100.9f;
        player.CurrentY = 200.9f;
        var enemy = state.MovementSlots[0];
        enemy.Active = true;
        enemy.PathComplete = true;
        enemy.Mode = OriginalStrategicMovement.DirectPropertyMode;
        enemy.OriginProperty = 0;
        enemy.Lord = 0;
        enemy.Swordsmen = 1;
        enemy.CurrentX = 120.9f;
        enemy.CurrentY = 200.9f;
        var divisions = new[]
        {
            new OriginalStrategicPlayerTarget(true, 120.9f, 200.9f),
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0)
        };

        Assert.Equal(new OriginalStrategicPlayerMapHit(0, null, null),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 120, 180));

        player.Active = false;
        Assert.Equal(new OriginalStrategicPlayerMapHit(null, 0, null),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 120, 180));

        enemy.Active = false;
        Assert.Equal(new OriginalStrategicPlayerMapHit(null, null, 0),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 120, 180));
    }

    [Fact]
    public void StrategicMapHitTestRetainsHalfOpenSourceBounds()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.CurrentX = 100.9f;
        player.CurrentY = 200.9f;
        var divisions = new[]
        {
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0)
        };

        Assert.Equal(new OriginalStrategicPlayerMapHit(0, null, null),
            OriginalStrategicMapHitTesting.HitTest(state, divisions, 111, 170));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 110, 170));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 135, 170));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 111, 169));
        Assert.Equal(default, OriginalStrategicMapHitTesting.HitTest(state, divisions, 111, 201));
        Assert.Throws<ArgumentException>(() => OriginalStrategicMapHitTesting.HitTest(
            state, divisions[..2], 111, 170));
    }

    [Fact]
    public void StrategicMapCommandComposesRawPointerConversionHitPrecedenceAndRouteInput()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;
        state.SelectedPlayerMovementSlot = 0;
        var player = state.PlayerMovementSlots[0];
        player.Active = true;
        player.CurrentX = 100;
        player.CurrentY = 200;
        var divisions = new[]
        {
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0),
            new OriginalStrategicPlayerTarget(false, 0, 0)
        };
        var resources = new MapInputResources();

        var selection = OriginalStrategicMapCommands.DispatchRawPointer(
            state, resources, divisions, rawPointerX: -729, rawPointerY: 119, targetConfirmed: false);

        Assert.Equal(new OriginalStrategicRoutePoint(111, 179), selection.RoutePoint);
        Assert.Equal(new OriginalStrategicPlayerMapHit(0, null, null), selection.Hit);
        Assert.Equal(new OriginalStrategicPlayerCommandResult(true, false, false), selection.Command);
        Assert.True(state.PlayerRouteInputActive);

        player.CurrentX = 0;
        player.CurrentY = 0;
        var route = OriginalStrategicMapCommands.DispatchRawPointer(
            state, resources, divisions, rawPointerX: 0, rawPointerY: 0, targetConfirmed: false);

        Assert.Equal(new OriginalStrategicRoutePoint(840, 60), route.RoutePoint);
        Assert.Equal(default, route.Hit);
        Assert.Equal(new OriginalStrategicPlayerCommandResult(true, false, false), route.Command);
        Assert.Equal(new OriginalStrategicRoutePoint(840, 60), player.Waypoints[0]);
    }

    [Fact]
    public void StrategicMapTerrainDrawPlanPreservesSourceViewportOrderAndWrapping()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 199;
        state.CameraColumn = 399;

        var draws = OriginalStrategicMapTerrainRendering.BuildDraws(state);

        Assert.Equal(126, draws.Count);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(199, 399, 20, -53), draws[0]);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(0, 399, 100, -53), draws[1]);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(199, 0, -20, -33), draws[5]);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(3, 0, 300, -33), draws[9]);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(3, 21, 340, 387), draws[^1]);
    }

    [Fact]
    public void StrategicMapTerrainDrawPlanAlternatesThePartialLeftTileByCameraParity()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;

        var draws = OriginalStrategicMapTerrainRendering.BuildDraws(state);

        Assert.Equal(127, draws.Count);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(10, 2, -20, -53), draws[0]);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(10, 3, 20, -33), draws[6]);
        Assert.Equal(new OriginalStrategicMapTerrainDraw(15, 24, 380, 387), draws[^1]);
    }

    [Fact]
    public void StrategicMapTerrainTileDrawsUseOnlyTheSourceCellLowWord()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;
        var resources = new MapInputResources();

        var draws = OriginalStrategicMapTerrainRendering.BuildTileDraws(state, resources);

        Assert.Equal(new OriginalStrategicMapTerrainTileDraw(10, 2, -20, -53, 2), draws[0]);
        Assert.Equal(new OriginalStrategicMapTerrainTileDraw(10, 3, 20, -33, 3), draws[6]);
    }

    [Fact]
    public void StrategicMapMarkerFramesRetainSelectedDistinguishedAndAvatarOffsets()
    {
        const int frameBase = 40;

        Assert.Equal(41, OriginalStrategicMapMarkerPresentation.FrameFor(0, false, 2, frameBase));
        Assert.Equal(46, OriginalStrategicMapMarkerPresentation.FrameFor(0, true, 2, frameBase));
        Assert.Equal(45, OriginalStrategicMapMarkerPresentation.FrameFor(2, false, 2, frameBase));
        Assert.Equal(40, OriginalStrategicMapMarkerPresentation.FrameFor(2, true, 2, frameBase));
        Assert.Equal(42, OriginalStrategicMapMarkerPresentation.FrameFor(5, false, 5, frameBase));
        Assert.Equal(47, OriginalStrategicMapMarkerPresentation.FrameFor(5, true, 5, frameBase));
    }

    private sealed class MapInputResources : IOriginalStrategicResources
    {
        public IReadOnlyList<OriginalStrategicRoutePoint> Route(string resourceName, bool reverse) => [];

        public bool TryGridCell(int row, int column, out OriginalStrategicTerrainCell cell)
        {
            cell = new(row, column, ((uint)row << 16) | (uint)column);
            return true;
        }

        public bool TryTerrainCell(
            int worldX,
            int worldY,
            int cameraRow,
            int cameraColumn,
            out OriginalStrategicTerrainCell cell)
        {
            cell = new(0, 0, 0);
            return true;
        }
    }
}
