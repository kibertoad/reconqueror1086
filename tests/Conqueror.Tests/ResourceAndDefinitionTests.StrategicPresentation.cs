using Conqueror.Core;
using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

// Covers RULE-STRATEGY-015.
public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicInteractiveEncounterUsesItsOwnedBackgroundAndUnitSequence()
    {
        Assert.Equal("Encounter.Strategic.Background", OriginalStrategicInteractiveEncounterPresentation.BackgroundArtRole);
        Assert.Equal("Encounter.Strategic.Units", OriginalStrategicInteractiveEncounterPresentation.UnitAnimationRole);
        Assert.Contains(new ImportedArtDefinition(OriginalStrategicInteractiveEncounterPresentation.BackgroundArtRole, "image", ":battle.pcx"),
            ImportedArt.Definitions);
        Assert.Contains(new ImportedAnimationDefinition(OriginalStrategicInteractiveEncounterPresentation.UnitAnimationRole, ":men8.csf",
                OriginalStrategicInteractiveEncounterPresentation.BackgroundArtRole),
            ImportedAnimations.Definitions);
        Assert.Contains(new ImportedAnimationDefinition("Strategic.Map.Markers", ":icon_men.csf",
                "Estate.Shell"),
            ImportedAnimations.Definitions);
        Assert.Contains(new ImportedAnimationDefinition("Strategic.Map.MarkerOverlay", ":marker.csf",
                "Estate.Shell"),
            ImportedAnimations.Definitions);
    }

    [Fact]
    public void StrategicMarkerPresentationRetainsSourceFrameOrderAndBlitterClipping()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;
        state.SelectedPlayerMovementSlot = 0;
        state.EngagedPlayerMovementSlot = 2;
        var ordinary = state.PlayerMovementSlots[0];
        ordinary.Active = true;
        ordinary.CurrentX = 1107;
        ordinary.CurrentY = 200;
        var avatar = state.PlayerMovementSlots[5];
        avatar.CurrentX = 1067;
        avatar.CurrentY = 100;

        var blits = OriginalStrategicMarkerPresentation.BuildBlits(
            state, OriginalStrategicCharacterColor.Green, frameCount: 128,
            frameWidth: 27, frameHeight: 35);

        Assert.Equal([
            new OriginalStrategicMarkerBlit(0, 30, new UiBounds(40, 112, 27, 35),
                new UiBounds(0, 0, 27, 35)),
            new OriginalStrategicMarkerBlit(5, 26, new UiBounds(20, 12, 7, 35),
                new UiBounds(20, 0, 7, 35))
        ], blits);
        Assert.Throws<InvalidDataException>(() => OriginalStrategicMarkerPresentation.BuildBlits(
            state, OriginalStrategicCharacterColor.Green, frameCount: 30,
            frameWidth: 27, frameHeight: 35));
    }

    [Fact]
    public void StrategicMovementMarkersUseTheirStoredFramesInPhysicalPassOrder()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;
        ActivateMovementMarker(state, slotIndex: 0, originProperty: 0, sourceX: 1107, sourceY: 200);
        ActivateMovementMarker(state, slotIndex: 2, originProperty: 2, sourceX: 1067, sourceY: 100);

        var blits = OriginalStrategicMarkerPresentation.BuildMovementBlits(
            state, frameCount: 128, frameWidth: 27, frameHeight: 35);

        Assert.Equal([
            new OriginalStrategicMarkerBlit(0, 88, new UiBounds(40, 112, 27, 35),
                new UiBounds(0, 0, 27, 35)),
            new OriginalStrategicMarkerBlit(2, 16, new UiBounds(20, 12, 7, 35),
                new UiBounds(20, 0, 7, 35))
        ], blits);
    }

    [Fact]
    public void StrategicTemporaryForceMarkersUseTheSharedIconFrameAndClipping()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;
        state.TemporaryForceSlots[1].Active = true;
        state.TemporaryForceSlots[1].CurrentX = 1107;
        state.TemporaryForceSlots[1].CurrentY = 200;
        state.TemporaryForceSlots[2].Active = true;
        state.TemporaryForceSlots[2].CurrentX = 1067;
        state.TemporaryForceSlots[2].CurrentY = 100;

        var blits = OriginalStrategicMarkerPresentation.BuildTemporaryForceBlits(
            state, frameCount: 128, frameWidth: 27, frameHeight: 35);

        Assert.Equal([
            new OriginalStrategicMarkerBlit(1, 3, new UiBounds(40, 112, 27, 35),
                new UiBounds(0, 0, 27, 35)),
            new OriginalStrategicMarkerBlit(2, 3, new UiBounds(20, 12, 7, 35),
                new UiBounds(20, 0, 7, 35))
        ], blits);
        Assert.Throws<InvalidDataException>(() => OriginalStrategicMarkerPresentation
            .BuildTemporaryForceBlits(state, frameCount: 3, frameWidth: 27, frameHeight: 35));
    }

    [Fact]
    public void StrategicRoutePreviewUsesTheFourFrameMarkerSequenceAndSharedProjection()
    {
        var state = OriginalStrategicCampaignState.CreateForNewGame(new DateTime(1086, 3, 1), 0);
        state.CameraRow = 10;
        state.CameraColumn = 2;
        state.SelectedPlayerMovementSlot = OriginalStrategicMovement.PlayerAvatarMovementSlot;
        state.PlayerRouteInputActive = true;
        var player = state.PlayerMovementSlots[OriginalStrategicMovement.PlayerAvatarMovementSlot];
        player.CurrentX = 1106;
        player.CurrentY = 200;
        player.Waypoints.Add(new OriginalStrategicRoutePoint(1200, 200));
        player.WaypointCount = 1;

        Assert.Equal([
            new OriginalStrategicMarkerBlit(5, 1, new UiBounds(40, 139, 9, 8),
                new UiBounds(0, 0, 9, 8)),
            new OriginalStrategicMarkerBlit(5, 2, new UiBounds(61, 139, 9, 8),
                new UiBounds(0, 0, 9, 8)),
            new OriginalStrategicMarkerBlit(5, 3, new UiBounds(82, 139, 9, 8),
                new UiBounds(0, 0, 9, 8)),
            new OriginalStrategicMarkerBlit(5, 0, new UiBounds(103, 139, 9, 8),
                new UiBounds(0, 0, 9, 8))
        ], OriginalStrategicMarkerPresentation.BuildRoutePreviewBlits(
            state, framePhase: 1, frameCount: 4, frameWidth: 9, frameHeight: 8));
        Assert.Throws<InvalidDataException>(() => OriginalStrategicMarkerPresentation
            .BuildRoutePreviewBlits(state, framePhase: 1, frameCount: 3, frameWidth: 9, frameHeight: 8));
    }

    private static void ActivateMovementMarker(
        OriginalStrategicCampaignState state,
        int slotIndex,
        int originProperty,
        float sourceX,
        float sourceY)
    {
        var slot = state.MovementSlots[slotIndex];
        slot.Active = true;
        slot.OriginProperty = originProperty;
        slot.Lord = state.Properties[originProperty].Lord;
        slot.Mode = OriginalStrategicMovement.DirectPropertyMode;
        slot.Swordsmen = 1;
        slot.CurrentX = sourceX;
        slot.CurrentY = sourceY;
        slot.MarkerFrame = OriginalStrategicMovement.MovementMarkerFrameForOriginProperty(originProperty);
    }

    [Fact]
    public void StrategicInteractivePresentationUsesTheResolverFrameIndexFormula()
    {
        var units = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(0, 0, 1));

        Assert.Equal(15, OriginalStrategicInteractiveEncounterPresentation.FrameFor(units[0]));
        Assert.Equal(635, OriginalStrategicInteractiveEncounterPresentation.FrameFor(units[1]));

        units[0].PhaseCounter = 6;
        units[0].StateCode = 0x28;
        Assert.Equal(56, OriginalStrategicInteractiveEncounterPresentation.FrameFor(units[0]));
        Assert.Equal(720, OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayFrame);
        Assert.Equal(721, OriginalStrategicInteractiveEncounterPresentation.PendingFirstControlFrame);
        Assert.Equal(722, OriginalStrategicInteractiveEncounterPresentation.ControlStripFrame);

        units[0].PositionX = 300;
        units[0].PositionY = 200;
        Assert.Equal((215, 115), OriginalStrategicInteractiveEncounterPresentation.DrawPositionFor(
            units[0], horizontalScrollOffset: 40, verticalScrollOffset: 40));

        units[1].PositionX = 180;
        units[1].PositionY = 110;
        var overlays = OriginalStrategicInteractiveEncounterPresentation.SelectionOverlayDrawsFor(
            units, [1, 0], horizontalScrollOffset: 40, verticalScrollOffset: 40);
        Assert.Equal([
            new OriginalStrategicInteractiveEncounterOverlayDraw(1, 720, 135, 63),
            new OriginalStrategicInteractiveEncounterOverlayDraw(0, 720, 255, 153),
        ], overlays);

        Assert.Equal((415, 452), OriginalStrategicInteractiveEncounterPresentation
            .PendingFirstControlDrawPositionFor(controlStripMargin: 0, verticalSpan: 480));
        Assert.Equal((420, 452), OriginalStrategicInteractiveEncounterPresentation
            .ControlStripDrawPositionFor(controlStripMargin: 0, verticalSpan: 480));
        Assert.Equal((495, 552), OriginalStrategicInteractiveEncounterPresentation
            .PendingFirstControlDrawPositionFor(controlStripMargin: 80, verticalSpan: 580));

        units[0].RemainingStrength = 0;
        units[0].PositionX = 300;
        units[0].PositionY = 300;
        units[1].PositionX = 300;
        units[1].PositionY = 100;
        var third = OriginalStrategicInteractiveEncounter.Materialize(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(0, 0, 0))[0];
        third.PositionX = 50;
        third.PositionY = 100;
        var orderedUnits = new[] { units[0], units[1], third };
        Assert.Equal([0, 2, 1], OriginalStrategicInteractiveEncounterPresentation
            .OrderedUnitIndicesFor(orderedUnits));
        Assert.Equal([
            new OriginalStrategicInteractiveEncounterUnitDraw(0, 56, 255, 255),
            new OriginalStrategicInteractiveEncounterUnitDraw(2, 15, 5, 55),
            new OriginalStrategicInteractiveEncounterUnitDraw(1, 635, 255, 55),
        ], OriginalStrategicInteractiveEncounterPresentation.UnitDrawsFor(
            orderedUnits, horizontalScrollOffset: 0, verticalScrollOffset: 0));
    }
}
