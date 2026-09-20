using Conqueror.Core;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void StrategicInteractiveResolvedDisplayWidthSelectsOnlyTheMappedControlMargins()
    {
        Assert.Equal(0, OriginalStrategicInteractiveEncounterViewport
            .MappedControlStripMarginForResolvedHorizontalSpan(640));
        Assert.Equal(80, OriginalStrategicInteractiveEncounterViewport
            .MappedControlStripMarginForResolvedHorizontalSpan(800));
        Assert.Equal(160, OriginalStrategicInteractiveEncounterViewport
            .MappedControlStripMarginForResolvedHorizontalSpan(1024));
        Assert.Equal(0, OriginalStrategicInteractiveEncounterViewport
            .MappedControlStripMarginForResolvedHorizontalSpan(1280));
        Assert.Throws<ArgumentOutOfRangeException>(() => OriginalStrategicInteractiveEncounterViewport
            .MappedControlStripMarginForResolvedHorizontalSpan(0));

        var viewport = OriginalStrategicInteractiveEncounterViewport.ForResolvedDisplay(
            viewportWidth: 1024, viewportHeight: 768, contentWidth: 1024, contentHeight: 728);
        Assert.Equal(160, viewport.ControlStripMargin);
    }

    [Fact]
    public void StrategicInteractiveHoverStatusPanelUsesItsOwnMarginAndLowerAnchor()
    {
        Assert.Equal(new OriginalStrategicInteractiveEncounterRectangle(170, 455, 83, 20),
            OriginalStrategicInteractiveEncounterPresentation.HoverStatusPanelBoundsFor(
                controlStripMargin: 10, verticalSpan: 480));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicInteractiveEncounterPresentation.HoverStatusPanelBoundsFor(
                controlStripMargin: -1, verticalSpan: 480));
    }

    [Fact]
    public void StrategicInteractiveCategorySelectionAppendsOnlyLivingPlayerUnitsInAuthoredOrder()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(4, 1, 1),
            new OriginalStrategicEncounterForces(2, 1, 1),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        session.Units[3].RemainingStrength = 0;
        Assert.True(session.TryAppendPlayerSelection(1));

        Assert.Equal(2, session.AppendMappedPlayerCategorySelection(
            OriginalStrategicInteractiveEncounterCategory.Swordsmen));
        Assert.Equal([1, 0, 2], session.SelectedUnitIndices);
        Assert.Equal(1, session.AppendMappedPlayerCategorySelection(
            OriginalStrategicInteractiveEncounterCategory.Halberdiers));
        Assert.Equal(1, session.AppendMappedPlayerCategorySelection(
            OriginalStrategicInteractiveEncounterCategory.Knights));
        Assert.Equal([1, 0, 2, 4, 5], session.SelectedUnitIndices);
    }

    [Fact]
    public void StrategicInteractiveRawCategoryInputsRouteToTheirDistinctAppendLoops()
    {
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Swordsmen,
            OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x48));
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Swordsmen,
            OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x68));
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Knights,
            OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x4B));
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Knights,
            OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x6B));
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Halberdiers,
            OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x53));
        Assert.Equal(OriginalStrategicInteractiveEncounterCategory.Halberdiers,
            OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x73));
        Assert.Null(OriginalStrategicInteractiveEncounter.RouteMappedCategorySelectionInputCode(0x4A));

        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 1, 1),
            new OriginalStrategicEncounterForces(1, 1, 1),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);

        Assert.Equal(1, session.AppendMappedPlayerCategorySelectionForInputCode(0x68));
        Assert.Equal(1, session.AppendMappedPlayerCategorySelectionForInputCode(0x4B));
        Assert.Equal(1, session.AppendMappedPlayerCategorySelectionForInputCode(0x73));
        Assert.Equal(0, session.AppendMappedPlayerCategorySelectionForInputCode(0x4A));
        Assert.Equal([0, 2, 1], session.SelectedUnitIndices);
    }

    [Fact]
    public void StrategicInteractiveRawControlCodeInputsPreserveTheirDistinctFilters()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        Assert.True(session.TryAppendPlayerSelection(0));

        Assert.True(session.ApplyMappedRawControlCodeOneInput(0x61));
        Assert.Equal([1, 1], session.Units.Select(unit => unit.ControlCode));

        session.Units[0].ControlCode = 0;
        session.Units[1].ControlCode = 0;
        session.Units[1].RemainingStrength = 0;
        Assert.True(session.ApplyMappedRawControlCodeOneInput(0x41));
        Assert.Equal([1, 0], session.Units.Select(unit => unit.ControlCode));
        Assert.False(session.ApplyMappedRawControlCodeOneInput(0x42));
    }

    [Fact]
    public void StrategicInteractiveRawPauseInputsToggleOnlyAfterFirstControlActivation()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 0, 0), new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        Assert.False(session.IsMappedTacticalAdvancementEnabled);
        Assert.False(session.ToggleMappedTacticalAdvancementForInputCode(0x50));
        session.ActivateMappedFirstControl();
        Assert.True(session.IsMappedTacticalAdvancementEnabled);
        Assert.True(session.ToggleMappedTacticalAdvancementForInputCode(0x70));
        Assert.False(session.IsMappedTacticalAdvancementEnabled);
        Assert.True(session.ToggleMappedTacticalAdvancementForInputCode(0x50));
        Assert.True(session.IsMappedTacticalAdvancementEnabled);
    }

    [Fact]
    public void StrategicInteractiveRawExitInputsLatchTheWithdrawalOutcome()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 0, 0), new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        Assert.False(session.ResolveMappedRawResolverExit(0x12C));
        Assert.True(session.ResolveMappedRawResolverExit(0x12D));
        Assert.Equal(OriginalStrategicInteractiveEncounterOutcome.PlayerWithdrew, session.Outcome);
        Assert.False(session.ResolveMappedRawResolverExit(0x16B));
    }

    [Fact]
    public void StrategicInteractiveRawExitReportsDefeatWhenThePlayerLaneIsEmpty()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 0, 0), new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        session.CompleteDeathAnimation(0);

        Assert.True(session.ResolveMappedRawResolverExit(0x16B));
        Assert.Equal(OriginalStrategicInteractiveEncounterOutcome.PlayerDefeated, session.Outcome);
    }

    [Fact]
    public void StrategicInteractiveGatedRawContactInputsRetainTheirSourceFiltersAndEnemyReset()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(1, 0, 0), new OriginalStrategicEncounterForces(2, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);
        var enabled = new OriginalStrategicInteractiveEncounterRawInputGates(true, true, true);

        session.Units[1].RemainingStrength = 9;
        session.Units[2].RemainingStrength = 3;
        Assert.True(session.ApplyMappedRawContactFilterInput(0x57, enabled));
        Assert.Equal(1, session.MappedContactSideFilter);
        Assert.Equal((20, 20), (session.Units[1].RemainingStrength, session.Units[2].RemainingStrength));

        session.Units[1].RemainingStrength = 7;
        Assert.True(session.ApplyMappedRawContactFilterInput(0x111, enabled));
        Assert.Equal((1, 20), (session.MappedContactSideFilter, session.Units[1].RemainingStrength));

        Assert.True(session.ApplyMappedRawContactFilterInput(0x0C, enabled));
        Assert.Equal(-1, session.MappedContactSideFilter);
        Assert.True(session.ApplyMappedRawContactFilterInput(0x4C, enabled));
        Assert.Equal(-1, session.MappedContactSideFilter);
        Assert.False(session.ApplyMappedRawContactFilterInput(0x111,
            new OriginalStrategicInteractiveEncounterRawInputGates(true, false, true)));
        Assert.Equal(-1, session.MappedContactSideFilter);
    }

    [Fact]
    public void StrategicInteractiveHoverUsesSourceLabelsAndOnePassAggregateStatus()
    {
        var session = OriginalStrategicInteractiveEncounterSession.Create(
            new OriginalStrategicEncounterForces(2, 0, 0),
            new OriginalStrategicEncounterForces(1, 0, 0),
            menuCode: 0, horizontalSpan: 640, verticalSpan: 180, initialTime: TimeSpan.Zero);

        var player = session.CaptureMappedHoverPresentation(
            localX: 50, localY: 20, horizontalOffset: 10, verticalOffset: 10);
        Assert.Equal((OriginalStrategicInteractiveEncounterHoverKind.PlayerStrength, 100, "OUR 100%"),
            (player.Kind, player.Strength, player.Text));

        var winning = session.CaptureMappedHoverPresentation(0, 100, 0, 0);
        Assert.Equal((OriginalStrategicInteractiveEncounterHoverKind.Winning, "WINNING"),
            (winning.Kind, winning.Text));
        Assert.Equal(OriginalStrategicInteractiveEncounterHoverKind.None,
            session.CaptureMappedHoverPresentation(0, 100, 0, 0).Kind);

        var foe = session.CaptureMappedHoverPresentation(0, 0, 0, 0);
        Assert.Equal((OriginalStrategicInteractiveEncounterHoverKind.Foe, "FOE"), (foe.Kind, foe.Text));
    }
}
