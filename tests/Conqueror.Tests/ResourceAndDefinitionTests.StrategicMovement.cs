using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OriginalStrategicMovementLayoutMatchesExecutableRecord()
    {
        Assert.Equal(5, OriginalStrategicMovement.SlotCount);
        Assert.Equal(0x118, OriginalStrategicMovement.RecordSize);
        Assert.Equal(0x1A4B8, OriginalStrategicMovement.PlayerMovementTableAddress);
        Assert.Equal(6, OriginalStrategicMovement.PlayerMovementRecordCount);
        Assert.Equal(5, OriginalStrategicMovement.PlayerArmyMovementCount);
        Assert.Equal(5, OriginalStrategicMovement.PlayerAvatarMovementSlot);
        Assert.Equal(0xAE64, OriginalStrategicMovement.SelectedPlayerMovementSlotAddress);
        Assert.Equal(0xAE6C, OriginalStrategicMovement.EngagedPlayerMovementSlotAddress);
        Assert.Equal(0x1388, OriginalStrategicMovement.GenerationIntervalUnits);
        Assert.Equal(1, OriginalStrategicMovement.InitialSpeedMultiplier);
        Assert.Equal(1, OriginalStrategicMovement.MinimumSpeedMultiplier);
        Assert.Equal(15, OriginalStrategicMovement.MaximumSpeedMultiplier);
        Assert.Equal(0x64, OriginalStrategicMovement.GenerationRollLimit);
        Assert.Equal(0x60, OriginalStrategicMovement.GenerationStartThreshold);
        Assert.Equal(1, OriginalStrategicMovement.GenerationPropertyEligibilityValue);
        Assert.Equal(7, OriginalStrategicMovement.ReactiveSpecialProperty);
        Assert.Equal(0x3E8, OriginalStrategicMovement.ReactiveSpecialPropertyDelay);
        Assert.Equal(0x32, OriginalStrategicMovement.ReactiveExistingPursuitDelay);

        Assert.Equal(0x00, OriginalStrategicMovement.ActiveOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PathCompleteOffset);
        Assert.Equal(0x14, OriginalStrategicMovement.TargetMovementSlotOffset);
        Assert.Equal(0x18, OriginalStrategicMovement.WaypointCountOffset);
        Assert.Equal(0x1C, OriginalStrategicMovement.SwordsmenOffset);
        Assert.Equal(0x20, OriginalStrategicMovement.HalberdiersOffset);
        Assert.Equal(0x24, OriginalStrategicMovement.KnightsOffset);
        Assert.Equal(0x28, OriginalStrategicMovement.OriginLocationOffset);
        Assert.Equal(0x2C, OriginalStrategicMovement.LordOffset);
        Assert.Equal(0x34, OriginalStrategicMovement.ModeOffset);
        Assert.Equal(0x38, OriginalStrategicMovement.MarkerFrameOffset);
        Assert.Equal(0x3C, OriginalStrategicMovement.DestinationXOffset);
        Assert.Equal(0x40, OriginalStrategicMovement.DestinationYOffset);
        Assert.Equal(0x44, OriginalStrategicMovement.GridXOffset);
        Assert.Equal(0x48, OriginalStrategicMovement.GridYOffset);
        Assert.Equal(0x5C, OriginalStrategicMovement.CurrentXOffset);
        Assert.Equal(0x60, OriginalStrategicMovement.CurrentYOffset);
        Assert.Equal(0x64, OriginalStrategicMovement.DirectionXOffset);
        Assert.Equal(0x68, OriginalStrategicMovement.DirectionYOffset);
        Assert.Equal(0x6C, OriginalStrategicMovement.RoutePointerOffset);
        Assert.Equal(1, OriginalStrategicMovement.DirectPropertyMode);
        Assert.Equal(2, OriginalStrategicMovement.RoutedMode);
        Assert.Equal(3, OriginalStrategicMovement.PursuitMode);
        Assert.Equal(0xFFFF, OriginalStrategicMovement.CompletedSignal);
        Assert.Equal(6, OriginalStrategicMovement.WaypointCoordinateTolerance);
        Assert.Equal(50, OriginalStrategicMovement.MaximumRoutedStep);
        Assert.Equal(0x4F97, OriginalStrategicMovement.DirectTerrainScaleAddress);
        Assert.Equal(0.9, OriginalStrategicMovement.DirectTerrainScale);
        Assert.Equal(0x7389, OriginalStrategicMovement.RoutedHorizontalLimitAddress);
        Assert.Equal(300, OriginalStrategicMovement.RetargetFieldArmyRange);
        Assert.Equal(30, OriginalStrategicMovement.TerrainKindCount);
        Assert.Equal(0xAF78, OriginalStrategicMovement.TerrainTileKindTableAddress);
        Assert.Equal(331, OriginalStrategicMovement.TerrainTileKindCount);
        Assert.Equal(4, OriginalStrategicMovement.TerrainProfileCount);
        Assert.Equal(2, OriginalStrategicMovement.ReducedTerrainProfile);
        Assert.Equal(9, OriginalStrategicMovement.ImpassableTerrainKind);
        Assert.Equal(0xAE88, OriginalStrategicMovement.MovementMarkerFrameTableAddress);
        Assert.Equal([88, 8, 16, 72, 32, 56, 48, 56, 64, 72, 80, 88, 96, 64],
            Enumerable.Range(0, OriginalStrategicMovement.PropertyCount)
                .Select(OriginalStrategicMovement.MovementMarkerFrameForOriginProperty));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.MovementMarkerFrameForOriginProperty(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.MovementMarkerFrameForOriginProperty(
                OriginalStrategicMovement.PropertyCount));
    }

    [Fact]
    public void OriginalStrategicGenerationChecksThePriorAccumulatorAndConsumesRollOnlyWhenEligible()
    {
        Assert.Equal(new StrategicGenerationClockAdvance(5_001, false, false, false),
            OriginalStrategicMovement.AdvanceGenerationClock(4_999, 2, 0, null));
        Assert.Equal(new StrategicGenerationClockAdvance(0, true, false, false),
            OriginalStrategicMovement.AdvanceGenerationClock(5_001, 10, 5, null));
        Assert.Equal(new StrategicGenerationClockAdvance(0, true, true, false),
            OriginalStrategicMovement.AdvanceGenerationClock(5_001, 10, 4, 96));
        Assert.Equal(new StrategicGenerationClockAdvance(0, true, true, true),
            OriginalStrategicMovement.AdvanceGenerationClock(5_001, 10, 4, 97));
        Assert.Throws<ArgumentNullException>(() =>
            OriginalStrategicMovement.AdvanceGenerationClock(5_000, 1, 4, null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.AdvanceGenerationClock(0, 0, 0, null));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.AdvanceGenerationClock(0, 16, 0, null));
    }

    [Fact]
    public void OriginalStrategicReactivePursuitPreservesSpecialPropertyAndRepeatDelays()
    {
        Assert.False(OriginalStrategicMovement.CanAttemptReactivePursuit(
            property: 7, movementSlot: 2, playerMovementSlot: 1, callsSinceReactiveSuccess: 999,
            hasExistingPursuer: false));
        Assert.True(OriginalStrategicMovement.CanAttemptReactivePursuit(
            property: 7, movementSlot: 2, playerMovementSlot: 1, callsSinceReactiveSuccess: 1_000,
            hasExistingPursuer: false));
        Assert.False(OriginalStrategicMovement.CanAttemptReactivePursuit(
            property: 3, movementSlot: 2, playerMovementSlot: 1, callsSinceReactiveSuccess: 1_000,
            hasExistingPursuer: true));
        Assert.False(OriginalStrategicMovement.CanAttemptReactivePursuit(
            property: 3, movementSlot: 1, playerMovementSlot: 1, callsSinceReactiveSuccess: 50,
            hasExistingPursuer: true));
        Assert.True(OriginalStrategicMovement.CanAttemptReactivePursuit(
            property: 3, movementSlot: 1, playerMovementSlot: 1, callsSinceReactiveSuccess: 51,
            hasExistingPursuer: true));

        StrategicMovementPursuitState[] movements =
        [
            new(true, 2, 3),
            new(true, 3, 2),
            new(false, 3, 1),
            new(true, 1, 2),
            new(false, 0, 0)
        ];
        Assert.True(OriginalStrategicMovement.HasLivePursuer(movements, 2));
        Assert.False(OriginalStrategicMovement.HasLivePursuer(movements, 1));
        Assert.True(OriginalStrategicMovement.ShouldAttemptReactiveSpawn(1, 4));
        Assert.False(OriginalStrategicMovement.ShouldAttemptReactiveSpawn(2, 4));
        Assert.False(OriginalStrategicMovement.ShouldAttemptReactiveSpawn(1, 5));
    }

    [Fact]
    public void OriginalStrategicConstructionUsesFirstFreeSlotAndExactRouteFlagFallbacks()
    {
        Assert.Equal(2, OriginalStrategicMovement.FirstFreeSlot([true, true, false, false, true]));
        Assert.Equal(-1, OriginalStrategicMovement.FirstFreeSlot([true, true, true, true, true]));
        Assert.Equal([0, 2, 4], OriginalStrategicMovement.ActiveMovementSlots([true, false, true, false, true]));

        Assert.Equal(
            [
                new StrategicMovementConstructionAttempt(4, 2, StrategicRouteSelection.AlternateAuthoredRoute),
                new StrategicMovementConstructionAttempt(4, 1, StrategicRouteSelection.AlternateAuthoredRoute)
            ],
            OriginalStrategicMovement.TimedConstructionFallback(
                propertyListPresent: false, globalOriginProperty: 4, selectedProperty: -1));
        Assert.Equal(
            [
                new StrategicMovementConstructionAttempt(8, 2, StrategicRouteSelection.CanonicalPropertyPair),
                new StrategicMovementConstructionAttempt(8, 1, StrategicRouteSelection.CanonicalPropertyPair)
            ],
            OriginalStrategicMovement.TimedConstructionFallback(
                propertyListPresent: true, globalOriginProperty: 4, selectedProperty: 8));
        Assert.Empty(OriginalStrategicMovement.TimedConstructionFallback(
            propertyListPresent: true, globalOriginProperty: 4, selectedProperty: -1));
    }

    [Fact]
    public void OriginalStrategicRoutedStepUsesExecutableTerrainProfilesAndStrictHorizontalLimit()
    {
        float[] primary =
        [
            1f, 0.2f, 1f, 0.5f, 1f, 0.5f, 0.8f, 0.8f, 1f, 5f,
            0.3f, 0.6f, 0.8f, 0.8f, 2f, 2f, 2f, 1f, 0.2f, 0.5f,
            0.5f, 0.5f, 0.5f, 3f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.2f
        ];
        float[] reduced =
        [
            0.8f, 0.2f, 0.8f, 0.5f, 0.8f, 0.5f, 0.6f, 0.6f, 0.8f, 5f,
            0.2f, 0.4f, 0.5f, 0.5f, 1.5f, 1.5f, 1.5f, 0.8f, 0.1f, 0.4f,
            0.4f, 0.4f, 0.4f, 2f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.6f
        ];
        Assert.Equal(primary, Enumerable.Range(0, 30)
            .Select(kind => OriginalStrategicMovement.TerrainSpeed(0, kind)));
        Assert.Equal(primary, Enumerable.Range(0, 30)
            .Select(kind => OriginalStrategicMovement.TerrainSpeed(1, kind)));
        Assert.Equal(reduced, Enumerable.Range(0, 30)
            .Select(kind => OriginalStrategicMovement.TerrainSpeed(2, kind)));
        Assert.Equal(primary, Enumerable.Range(0, 30)
            .Select(kind => OriginalStrategicMovement.TerrainSpeed(3, kind)));

        Assert.Equal(new StrategicRoutedStep(6f, 8f, StrategicRoutedStepOutcome.Apply),
            OriginalStrategicMovement.CalculateRoutedStep(0.6f, 0.8f, 10, 1, 0));
        Assert.Equal(StrategicRoutedStepOutcome.HoldForHorizontalLimit,
            OriginalStrategicMovement.CalculateRoutedStep(2.5f, 0f, 10, 1, 14).Outcome);
        Assert.Equal(new StrategicRoutedStep(0f, 15f, StrategicRoutedStepOutcome.Apply),
            OriginalStrategicMovement.CalculateRoutedStep(0f, 1f, 15, 1, 0));
        Assert.Equal(StrategicRoutedStepOutcome.DeactivateForImpassableTerrain,
            OriginalStrategicMovement.CalculateRoutedStep(1f, 0f, 1, 1, 9).Outcome);
        Assert.Equal(9, OriginalStrategicMovement.TerrainKindForTile(0));
        Assert.Equal(0, OriginalStrategicMovement.TerrainKindForTile(18));
        Assert.Equal(1, OriginalStrategicMovement.TerrainKindForTile(330));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.TerrainKindForTile(331));
    }

    [Fact]
    public void OriginalStrategicTerrainProfilesFollowTheCalendarAndSeasonResources()
    {
        Assert.Equal(0xB720, OriginalStrategicMovement.MonthProfileTableAddress);
        Assert.Equal(0xB750, OriginalStrategicMovement.SeasonMovieTableAddress);
        Assert.Equal(0xB760, OriginalStrategicMovement.SeasonAtlasTableAddress);
        Assert.Equal(
            [
                StrategicTerrainProfile.Winter, StrategicTerrainProfile.Winter,
                StrategicTerrainProfile.Spring, StrategicTerrainProfile.Spring, StrategicTerrainProfile.Spring,
                StrategicTerrainProfile.Summer, StrategicTerrainProfile.Summer, StrategicTerrainProfile.Summer,
                StrategicTerrainProfile.Autumn, StrategicTerrainProfile.Autumn, StrategicTerrainProfile.Autumn,
                StrategicTerrainProfile.Winter
            ],
            OriginalStrategicMovement.MonthProfiles);
        Assert.Equal(
            [
                new OriginalStrategicTerrainProfileDefinition(
                    StrategicTerrainProfile.Summer, "tran4.smk", "ics.csf"),
                new OriginalStrategicTerrainProfileDefinition(
                    StrategicTerrainProfile.Autumn, "tran2.smk", "ica.csf"),
                new OriginalStrategicTerrainProfileDefinition(
                    StrategicTerrainProfile.Winter, "tran3.smk", "icw.csf"),
                new OriginalStrategicTerrainProfileDefinition(
                    StrategicTerrainProfile.Spring, "tran1.smk", "ics.csf")
            ],
            OriginalStrategicMovement.TerrainProfiles);
        Assert.Equal([false, false, true, false],
            OriginalStrategicMovement.TerrainProfiles
                .Select(profile => profile.UsesReducedTerrainSpeeds));
        for (var month = 0; month < 12; month++)
            Assert.Equal(OriginalStrategicMovement.MonthProfiles[month],
                OriginalStrategicMovement.TerrainProfileForMonth(month));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.TerrainProfileForMonth(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.TerrainProfileForMonth(12));
    }

    [Fact]
    public void OriginalStrategicAlternateRoutesBindTheSevenStartingCastles()
    {
        Assert.Equal(0xC9D0, OriginalStrategicMovement.StartingRouteSelectorAddress);
        Assert.Equal(0xB8C8, OriginalStrategicMovement.StartingPersonIndexTableAddress);
        Assert.Equal(0xCA98, OriginalStrategicMovement.StartingRouteTableAddress);
        Assert.Equal(7, OriginalStrategicMovement.StartingRouteCount);

        OriginalStrategicStartingRoute[] expected =
        [
            new(0, 17, "Scott's Keep", 0, 1, 94, 40, "sc_0.rat"),
            new(1, 18, "Anne's Castle", 0, 1, 129, 23, "sc_1.rat"),
            new(2, 24, "Stonetree Castle", 1, 2, 142, 82, "sc_2.rat"),
            new(3, 29, "MacGibbon on the Hill", 1, 2, 120, 100, "sc_3.rat"),
            new(4, 86, "Leecastle", 4, 6, 125, 152, "sc_4.rat"),
            new(5, 144, "Damron Castle", 12, 13, 110, 264, "sc_5.rat"),
            new(6, 166, "Sabine's Keep", 13, 14, 27, 306, "sc_6.rat")
        ];
        Assert.Equal(expected, OriginalStrategicMovement.StartingRoutes);
        Assert.Equal(7, OriginalStrategicMovement.StartingRoutes
            .Select(route => route.ResourceName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        for (var selector = 0; selector < expected.Length; selector++)
        {
            Assert.True(OriginalStrategicMovement.TryGetStartingRoute(selector, out var route));
            Assert.Equal(expected[selector], route);
        }
        Assert.False(OriginalStrategicMovement.TryGetStartingRoute(-1, out _));
        Assert.False(OriginalStrategicMovement.TryGetStartingRoute(7, out _));
    }

    [Fact]
    public void OriginalStrategicTemporaryForceRoutesBindTheTwoAutomaticCreatorSlots()
    {
        Assert.Equal([
            new OriginalStrategicTemporaryForceRoute(1, "scot.rat", 44),
            new OriginalStrategicTemporaryForceRoute(2, "wales.rat", 42)
        ], OriginalStrategicTemporaryForces.Routes);
        Assert.True(OriginalStrategicTemporaryForces.TryGetRoute(1, out var scot));
        Assert.Equal("scot.rat", scot.ResourceName);
        Assert.True(OriginalStrategicTemporaryForces.TryGetRoute(2, out var wales));
        Assert.Equal("wales.rat", wales.ResourceName);
        Assert.False(OriginalStrategicTemporaryForces.TryGetRoute(0, out _));
    }

    [Fact]
    public void OriginalStrategicPropertyLayoutAndRowsMatchExecutableTable()
    {
        Assert.Equal(0xB8EC, OriginalStrategicMovement.PropertyTableAddress);
        Assert.Equal(14, OriginalStrategicMovement.PropertyCount);
        Assert.Equal(0x0F, OriginalStrategicMovement.PropertyRecordSize);
        Assert.Equal(0x00, OriginalStrategicMovement.PropertyOwnerOrStateOffset);
        Assert.Equal(0x01, OriginalStrategicMovement.PropertyGridXOffset);
        Assert.Equal(0x03, OriginalStrategicMovement.PropertyGridYOffset);
        Assert.Equal(0x05, OriginalStrategicMovement.PropertyMapX8Offset);
        Assert.Equal(0x07, OriginalStrategicMovement.PropertyMapY8Offset);
        Assert.Equal(0x09, OriginalStrategicMovement.PropertyLordOffset);
        Assert.Equal(0x0A, OriginalStrategicMovement.PropertyListNextOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PropertyGarrisonOffset);
        Assert.Equal(0x0D, OriginalStrategicMovement.PropertyState13Offset);
        Assert.Equal(0x0E, OriginalStrategicMovement.PropertyState14Offset);

        OriginalStrategicPropertyDefinition[] expected =
        [
            new(1, 134, 59, 0x2A80, 0x049C, 13, 0x00FF, 18, 0, 0),
            new(2, 143, 100, 0x2D00, 0x07E4, 20, 0x00FF, 22, 0, 0),
            new(3, 90, 115, 0x1C70, 0x0910, 35, 0x00FF, 24, 0, 0),
            new(4, 112, 187, 0x2300, 0x0EB0, 59, 0x00FF, 28, 0, 0),
            new(5, 118, 159, 0x2580, 0x0C6C, 73, 0x00FF, 18, 0, 0),
            new(6, 126, 187, 0x27B0, 0x0ED8, 88, 0x00FF, 25, 0, 0),
            new(7, 161, 167, 0x32A0, 0x0D20, 96, 0x00FF, 18, 0, 0),
            new(8, 157, 213, 0x3160, 0x10A4, 100, 0x00FF, 90, 0, 0),
            new(9, 181, 137, 0x38E0, 0x0AF0, 1, 0x00FF, 24, 0, 0),
            new(10, 177, 183, 0x37A0, 0x0E60, 113, 0x00FF, 22, 0, 0),
            new(11, 146, 252, 0x2DA0, 0x13D8, 119, 0x00FF, 18, 0, 0),
            new(12, 146, 216, 0x2DA0, 0x1108, 143, 0x00FF, 24, 0, 0),
            new(13, 75, 239, 0x17C0, 0x12C0, 155, 0x00FF, 15, 0, 0),
            new(14, 68, 266, 0x1590, 0x14DC, 171, 0x00FF, 20, 0, 0)
        ];

        Assert.Equal(expected, OriginalStrategicMovement.Properties);
    }

    [Fact]
    public void OriginalStrategicPersonLayoutAndInitialHouseholdCountsMatchExecutableTable()
    {
        Assert.Equal(0xBA50, OriginalStrategicMovement.PersonTableAddress);
        Assert.Equal(176, OriginalStrategicMovement.PersonCount);
        Assert.Equal(0x12, OriginalStrategicMovement.PersonRecordSize);
        Assert.Equal(0x00, OriginalStrategicMovement.PersonNameAddressOffset);
        Assert.Equal(0x04, OriginalStrategicMovement.PersonGroupOffset);
        Assert.Equal(0x05, OriginalStrategicMovement.PersonState5Offset);
        Assert.Equal(0x06, OriginalStrategicMovement.PersonFlagsOffset);
        Assert.Equal(0x07, OriginalStrategicMovement.PersonAssignmentOffset);
        Assert.Equal(0x08, OriginalStrategicMovement.PersonXOffset);
        Assert.Equal(0x0A, OriginalStrategicMovement.PersonYOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PersonLordRatingOffset);
        Assert.Equal(0x0D, OriginalStrategicMovement.PersonListNextOffset);
        Assert.Equal(0x0E, OriginalStrategicMovement.PersonState14Offset);
        Assert.Equal(0x0F, OriginalStrategicMovement.PersonState15Offset);
        Assert.Equal(0x10, OriginalStrategicMovement.PersonState16Offset);
        Assert.Equal(0x11, OriginalStrategicMovement.PersonState17Offset);
        Assert.Equal(0x01, OriginalStrategicMovement.HouseholdEligibleFlag);
        Assert.Equal([4, 6, 7, 5, 2, 2, 4, 3, 3, 1, 9, 3, 3, 6],
            OriginalStrategicMovement.InitialActiveHouseholdCounts);
    }

    [Fact]
    public void OriginalStrategicPersonPopulationMatchesTheCompleteExecutableCensus()
    {
        Assert.Equal(OriginalStrategicMovement.PersonCount, OriginalStrategicMovement.Persons.Count);

        var census = string.Join("\n", OriginalStrategicMovement.Persons.Select((person, index) =>
            FormattableString.Invariant(
                $"person {index} 0x{person.NameAddress:X} {person.Group} {person.State5} {person.Flags} {person.Assignment} {person.X} {person.Y} {person.LordRating} {person.ListNext} {person.State14} {person.State15} {person.State16} {person.State17}")));
        Assert.Equal(
            "ec3a31ce97eb367cf3a39e033e2d9b6dccdfd72769d590a0ce9a839169754b4f",
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(census))).ToLowerInvariant());

        var eligibleByGroup = OriginalStrategicMovement.Persons
            .Where(person => person.Assignment != 0 &&
                (person.Flags & OriginalStrategicMovement.HouseholdEligibleFlag) != 0)
            .GroupBy(person => person.Group)
            .OrderBy(group => group.Key)
            .Select(group => group.Count());
        Assert.Equal(OriginalStrategicMovement.InitialActiveHouseholdCounts, eligibleByGroup);
    }

    [Fact]
    public void OriginalStrategicLordsAndStartingCastlesResolveToTheirExactPersonRows()
    {
        for (var propertyIndex = 0; propertyIndex < OriginalStrategicMovement.PropertyCount; propertyIndex++)
        {
            var property = OriginalStrategicMovement.Properties[propertyIndex];
            var identity = OriginalStrategicMovement.PropertyIdentities[propertyIndex];
            var lord = OriginalStrategicMovement.Persons[property.Lord];

            Assert.Equal(identity.NameAddress, lord.NameAddress);
            Assert.Equal(identity.PersonGroup, lord.Group);
            Assert.Equal(identity.InitialAssignment, lord.Assignment);
            Assert.Equal(identity.LordRating, lord.LordRating);
            Assert.Equal(property.GridX, lord.X);
            Assert.Equal(property.GridY, lord.Y);
        }

        foreach (var route in OriginalStrategicMovement.StartingRoutes)
        {
            var person = OriginalStrategicMovement.Persons[route.Person];
            Assert.Equal(route.OriginProperty, person.Group);
            Assert.Equal(route.InitialAssignment, person.Assignment);
            Assert.Equal(route.GridX, person.X);
            Assert.Equal(route.GridY, person.Y);
        }
    }

    [Fact]
    public void OriginalStrategicPropertyNamesAndLordInputsFollowTheirPersonRecords()
    {
        OriginalStrategicPropertyIdentity[] expected =
        [
            new("York", 0x66A0, 0, 1, 30),
            new("Lincoln", 0x66F4, 1, 2, 24),
            new("Chester", 0x67B4, 2, 3, 16),
            new("Gloucester", 0x68AC, 3, 4, 20),
            new("Warwick", 0x694C, 4, 5, 18),
            new("Oxford", 0x6A04, 5, 6, 20),
            new("Cambridge", 0x6A58, 6, 7, 16),
            new("London", 0x6A80, 7, 8, 70),
            new("Norwich", 0x6628, 8, 9, 24),
            new("Colchester", 0x6AFC, 9, 10, 28),
            new("Arundel", 0x6B48, 10, 11, 33),
            new("Windsor", 0x6C4C, 11, 12, 32),
            new("Dunster", 0x6CE4, 12, 13, 28),
            new("Okehampton", 0x6D80, 13, 14, 24)
        ];

        Assert.Equal(expected, OriginalStrategicMovement.PropertyIdentities);
        Assert.Equal(
            OriginalStrategicMovement.Properties.Select(property => (int)property.Lord),
            [13, 20, 35, 59, 73, 88, 96, 100, 1, 113, 119, 143, 155, 171]);
    }

    [Fact]
    public void OriginalStrategicGenerationCompactsEligiblePropertiesInAuthoredOrder()
    {
        var properties = Enumerable.Repeat(new OriginalStrategicPropertyGenerationState(1, 0),
            OriginalStrategicMovement.PropertyCount).ToArray();
        properties[1] = new(0, 1);
        properties[3] = new(4, 1);
        properties[8] = new(9, 1);
        properties[12] = new(13, 2);

        Assert.Equal([3, 8], OriginalStrategicMovement.GenerationPropertyCandidates(properties));
        Assert.Equal(3, OriginalStrategicMovement.SelectGenerationProperty(properties, 0));
        Assert.Equal(8, OriginalStrategicMovement.SelectGenerationProperty(properties, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OriginalStrategicMovement.SelectGenerationProperty(properties, 2));

        properties[3] = new(4, 0);
        properties[8] = new(9, 0);
        Assert.Equal(-1, OriginalStrategicMovement.SelectGenerationProperty(properties, 0));
        Assert.Throws<ArgumentException>(() =>
            OriginalStrategicMovement.GenerationPropertyCandidates(properties[..^1]));
    }

    [Fact]
    public void OriginalStrategicCompletionUsesFixedProbeOrderAndContactPrecedence()
    {
        Assert.Equal(
            [new(0, 0), new(0, -2), new(1, 0), new(-1, -1), new(0, 1)],
            OriginalStrategicMovement.ContactProbes);

        Assert.Equal(StrategicContactOutcome.Encounter,
            OriginalStrategicMovement.ResolveCompletedContact(4, 0, contactedPersonEligible: true));
        Assert.Equal(StrategicContactOutcome.Retarget,
            OriginalStrategicMovement.ResolveCompletedContact(4, 0, contactedPersonEligible: false));
        Assert.Equal(StrategicContactOutcome.ReinforceOriginAndDeactivate,
            OriginalStrategicMovement.ResolveCompletedContact(4, 4, contactedPersonEligible: true));
        Assert.Equal(StrategicContactOutcome.Retarget,
            OriginalStrategicMovement.ResolveCompletedContact(4, 3, contactedPersonEligible: true));
        Assert.Equal(StrategicContactOutcome.Retarget,
            OriginalStrategicMovement.ResolveCompletedContact(0, null, contactedPersonEligible: true));
    }

    [Fact]
    public void OriginalStrategicPropertyRoutesUseCanonicalFilesAndReverseOnlyWhenRequired()
    {
        Assert.True(OriginalStrategicMovement.TryGetPropertyRoute(0, 1, out var yorkToLincoln));
        Assert.Equal(new OriginalStrategicRoute("rt_1_2.rat", Reverse: false), yorkToLincoln);

        Assert.True(OriginalStrategicMovement.TryGetPropertyRoute(13, 2, out var okehamptonToChester));
        Assert.Equal(new OriginalStrategicRoute("rt_3_14.rat", Reverse: true), okehamptonToChester);

        Assert.False(OriginalStrategicMovement.TryGetPropertyRoute(4, 4, out _));
        Assert.True(OriginalStrategicMovement.TryGetPropertyRoute(0, 12, out var yorkToDunster));
        Assert.Equal(new OriginalStrategicRoute("rt_1_13.rat", Reverse: false), yorkToDunster);
        Assert.False(OriginalStrategicMovement.TryGetPropertyRoute(6, 12, out _));
        Assert.False(OriginalStrategicMovement.TryGetPropertyRoute(12, 6, out _));

        var supported = 0;
        for (var from = 0; from < OriginalStrategicMovement.PropertyCount; from++)
        for (var to = 0; to < OriginalStrategicMovement.PropertyCount; to++)
            if (OriginalStrategicMovement.TryGetPropertyRoute(from, to, out _)) supported++;

        Assert.Equal(180, supported);
        Assert.Equal(90, OriginalStrategicMovement.PropertyRouteResources.Count);
        Assert.Equal(90, OriginalStrategicMovement.PropertyRouteResources
            .Select(route => route.ResourceName).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void OriginalStrategicRetargetUsesFirstCloseArmyThenNearestPlayerOrOrigin()
    {
        StrategicRetargetCandidate[] armies =
        [
            new(false, new(1, 1)),
            new(true, new(299, 0)),
            new(true, new(2, 0))
        ];

        Assert.Equal(new StrategicRetargetSelection(StrategicRetargetKind.FieldArmy, 1),
            OriginalStrategicMovement.SelectRetarget(new(0, 0), armies, new(500, 0), new(600, 0)));

        armies[1] = new(true, new(300, 0));
        armies[2] = new(false, new(2, 0));
        Assert.Equal(new StrategicRetargetSelection(StrategicRetargetKind.OriginProperty, -1),
            OriginalStrategicMovement.SelectRetarget(new(0, 0), armies, new(500, 0), new(400, 0)));
        Assert.Equal(new StrategicRetargetSelection(StrategicRetargetKind.Player, -1),
            OriginalStrategicMovement.SelectRetarget(new(0, 0), [], new(400, 0), new(400, 0)));
    }

    [Fact]
    public void StrategicRouteDecoderReadsExactSignedCoordinatePairs()
    {
        var data = new byte[4 + 3 * StrategicRouteDecoder.PointSize];
        BinaryPrimitives.WriteInt32LittleEndian(data, 3);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), 0x2A30);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), 0x04B0);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), -7);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16), 9);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(20), int.MinValue);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(24), int.MaxValue);

        var route = StrategicRouteDecoder.Decode(data);

        Assert.Equal(
            [new StrategicRoutePoint(0x2A30, 0x04B0), new(-7, 9), new(int.MinValue, int.MaxValue)],
            route.Points);
    }

    [Fact]
    public void StrategicRouteDecoderRejectsInvalidCountsAndTrailingData()
    {
        Assert.Throws<InvalidDataException>(() => StrategicRouteDecoder.Decode([]));
        Assert.Throws<InvalidDataException>(() => StrategicRouteDecoder.Decode([0, 0, 0, 0]));

        var oversized = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(oversized, StrategicRouteDecoder.MaximumPointCount + 1);
        Assert.Throws<InvalidDataException>(() => StrategicRouteDecoder.Decode(oversized));

        var trailing = new byte[13];
        BinaryPrimitives.WriteInt32LittleEndian(trailing, 1);
        Assert.Throws<InvalidDataException>(() => StrategicRouteDecoder.Decode(trailing));
    }

    [Fact]
    public void StrategicWorldGridDecoderRestoresColumnMajorCellsToRowFirstAddressing()
    {
        var data = new byte[StrategicWorldGridDecoder.EncodedLength];
        BinaryPrimitives.WriteInt32LittleEndian(data, StrategicWorldGridDecoder.CellWidth);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), StrategicWorldGridDecoder.CellHeight);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), StrategicWorldGridDecoder.RowCount);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), StrategicWorldGridDecoder.ColumnCount);
        WriteWorldCell(data, row: 0, column: 0, 0x1234_0012);
        WriteWorldCell(data, row: 1, column: 0, 0xAB56_014A);
        WriteWorldCell(data, row: 0, column: 1, 0xCD78_002A);

        var grid = StrategicWorldGridDecoder.Decode(data);

        Assert.Equal(80, grid.CellWidth);
        Assert.Equal(80, grid.CellHeight);
        Assert.Equal(200, grid.RowCount);
        Assert.Equal(400, grid.ColumnCount);
        Assert.Equal(new StrategicWorldCell(0x1234_0012), grid[0, 0]);
        Assert.Equal((ushort)0x014A, grid[1, 0].TileId);
        Assert.Equal((byte)0x56, grid[1, 0].Auxiliary);
        Assert.Equal((byte)0xAB, grid[1, 0].UpperByte);
        Assert.Equal(new StrategicWorldCell(0xCD78_002A), grid[0, 1]);
    }

    [Fact]
    public void StrategicWorldGridDecoderRejectsWrongDimensionsAndLength()
    {
        Assert.Throws<InvalidDataException>(() => StrategicWorldGridDecoder.Decode([]));
        var data = new byte[StrategicWorldGridDecoder.EncodedLength];
        BinaryPrimitives.WriteInt32LittleEndian(data, StrategicWorldGridDecoder.CellWidth);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), StrategicWorldGridDecoder.CellHeight);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), StrategicWorldGridDecoder.RowCount);
        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), StrategicWorldGridDecoder.ColumnCount - 1);
        Assert.Throws<InvalidDataException>(() => StrategicWorldGridDecoder.Decode(data));

        BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), StrategicWorldGridDecoder.ColumnCount);
        Assert.Throws<InvalidDataException>(() => StrategicWorldGridDecoder.Decode(data.AsSpan(0, data.Length - 1)));
    }

    [Fact]
    public void StrategicWorldProjectionMatchesTheExecutableCellAnchorAndKnownRoutePoint()
    {
        Assert.Equal(new StrategicRoutePoint(0x2A30, 0x04B0),
            StrategicWorldProjection.CellAnchor(134, 59));

        foreach (var camera in new[]
                 {
                     new StrategicWorldCellPosition(0, 0),
                     new StrategicWorldCellPosition(100, 200),
                     new StrategicWorldCellPosition(199, 399)
                 })
        {
            Assert.True(StrategicWorldProjection.TryWorldToCell(
                0x2A30, 0x04B0, camera.Row, camera.Column, out var cell));
            Assert.Equal(new StrategicWorldCellPosition(134, 59), cell);
        }
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(134, 59)]
    [InlineData(199, 399)]
    public void StrategicWorldProjectionRoundTripsDiamondCentersAcrossCameraPositions(int row, int column)
    {
        var center = StrategicWorldProjection.CellCenter(row, column);
        foreach (var camera in new[]
                 {
                     new StrategicWorldCellPosition(0, 0),
                     new StrategicWorldCellPosition(row, column),
                     new StrategicWorldCellPosition(199, 399)
                 })
        {
            Assert.True(StrategicWorldProjection.TryWorldToCell(
                center.X, center.Y, camera.Row, camera.Column, out var actual));
            Assert.Equal(new StrategicWorldCellPosition(row, column), actual);
        }
    }

    [Fact]
    public void StrategicWorldProjectionUsesInclusiveRasterEdgesAndOriginalScanPrecedence()
    {
        var center = StrategicWorldProjection.CellCenter(10, 20);

        Assert.True(StrategicWorldProjection.TryWorldToCell(
            center.X + 40, center.Y, 0, 0, out var sharedEdge));
        Assert.Equal(new StrategicWorldCellPosition(10, 19), sharedEdge);

        Assert.True(StrategicWorldProjection.TryWorldToCell(
            center.X + 41, center.Y, 0, 0, out var neighboringInterior));
        Assert.Equal(new StrategicWorldCellPosition(11, 20), neighboringInterior);
    }

    [Fact]
    public void StrategicWorldProjectionRetainsCameraOrderedOwnershipAtAnOwnedRouteVertex()
    {
        Assert.True(StrategicWorldProjection.TryWorldToCell(
            9040, 3740, 0, 0, out var originCamera));
        Assert.True(StrategicWorldProjection.TryWorldToCell(
            9040, 3740, 199, 399, out var oppositeCamera));

        Assert.Equal(new StrategicWorldCellPosition(112, 185), originCamera);
        Assert.Equal(new StrategicWorldCellPosition(112, 186), oppositeCamera);
    }

    [Fact]
    public void StrategicWorldProjectionRejectsPointsOutsideTheOriginalRoutePlane()
    {
        Assert.False(StrategicWorldProjection.TryWorldToCell(0, 0, 0, 0, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StrategicWorldProjection.TryWorldToCell(80, 20, -1, 0, out _));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StrategicWorldProjection.CellAnchor(200, 0));
    }

    [Fact]
    public void StrategicMapCameraUsesStrictEdgesAndCanPanBothAxesInOnePass()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(state.Date, 0);
        var strategic = state.OriginalStrategicState;
        strategic.CameraRow = 20;
        strategic.CameraColumn = 20;

        Assert.True(OriginalStrategicMapCamera.ApplyMappedEdgeScroll(strategic, 629, 475));
        Assert.Equal((21, 21), (strategic.CameraRow, strategic.CameraColumn));

        Assert.False(OriginalStrategicMapCamera.ApplyMappedEdgeScroll(strategic, 628, 474));
        Assert.Equal((21, 21), (strategic.CameraRow, strategic.CameraColumn));

        Assert.True(OriginalStrategicMapCamera.ApplyMappedEdgeScroll(strategic, 7, 7));
        Assert.Equal((20, 20), (strategic.CameraRow, strategic.CameraColumn));
    }

    [Fact]
    public void StrategicMapCameraPreservesTheOriginalPresentationBounds()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(state.Date, 0);
        var strategic = state.OriginalStrategicState;

        strategic.CameraRow = OriginalStrategicMapCamera.MaximumRow;
        strategic.CameraColumn = OriginalStrategicMapCamera.MaximumColumn;
        Assert.False(OriginalStrategicMapCamera.ApplyMappedEdgeScroll(strategic, 629, 475));

        strategic.CameraRow = OriginalStrategicMapCamera.MinimumRow;
        strategic.CameraColumn = OriginalStrategicMapCamera.MinimumColumn;
        Assert.False(OriginalStrategicMapCamera.ApplyMappedEdgeScroll(strategic, 7, 7));
        Assert.Equal((10, 2), (strategic.CameraRow, strategic.CameraColumn));
    }

    [Fact]
    public void StrategicMapPointerUsesTheOriginalRawPointerToRouteSpaceOffset()
    {
        var state = Campaign.NewFromTemplate(0);
        state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForNewGame(state.Date, 0);
        var strategic = state.OriginalStrategicState;
        strategic.CameraRow = 10;
        strategic.CameraColumn = 2;

        Assert.Equal(new OriginalStrategicRoutePoint(880, 60),
            OriginalStrategicMapPointer.ToMappedRoutePoint(strategic, 40, 0));

        strategic.CameraRow = 192;
        strategic.CameraColumn = 300;
        Assert.Equal(new OriginalStrategicRoutePoint(15_405, 6_139),
            OriginalStrategicMapPointer.ToMappedRoutePoint(strategic, 5, 119));
    }

    [Fact]
    public void ImportedContentCatalogDecodesStrategicRoutesWithoutExposingMalformedData()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-strategic-route-{Guid.NewGuid():N}");
        try
        {
            var relative = Path.Combine("Decoded", "route.rat");
            var path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var data = new byte[4 + StrategicRouteDecoder.PointSize];
            BinaryPrimitives.WriteInt32LittleEndian(data, 1);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), 123);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), -456);
            File.WriteAllBytes(path, data);
            var asset = new ImportedAsset(
                "C1086.GOB#67:rt_1_2.rat", relative, "resource", data.Length, ResourceHash.Sha256(path));
            new ImportManifest(1, new string('a', 64), [asset]).Write(Path.Combine(root, "manifest.json"));

            var catalog = Assert.IsType<ImportedContentCatalog>(ImportedContentCatalog.Discover(root));
            Assert.Equal(
                new StrategicRoutePoint(123, -456),
                Assert.Single(Assert.IsType<StrategicRouteResource>(
                    catalog.DecodeStrategicRoute(asset.Id)).Points));

            File.WriteAllBytes(path, [1, 0, 0, 0]);
            Assert.Null(catalog.DecodeStrategicRoute(asset.Id));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportedContentCatalogDecodesStrategicWorldWithoutExposingMalformedData()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-strategic-world-{Guid.NewGuid():N}");
        try
        {
            var relative = Path.Combine("Decoded", "icon.jp");
            var path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            var data = new byte[StrategicWorldGridDecoder.EncodedLength];
            BinaryPrimitives.WriteInt32LittleEndian(data, StrategicWorldGridDecoder.CellWidth);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), StrategicWorldGridDecoder.CellHeight);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), StrategicWorldGridDecoder.RowCount);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), StrategicWorldGridDecoder.ColumnCount);
            WriteWorldCell(data, row: 7, column: 11, 0x0123_002A);
            File.WriteAllBytes(path, data);
            var asset = new ImportedAsset(
                "C1086.GOB#292:icon.jp", relative, "resource", data.Length, ResourceHash.Sha256(path));
            new ImportManifest(1, new string('a', 64), [asset]).Write(Path.Combine(root, "manifest.json"));

            var catalog = Assert.IsType<ImportedContentCatalog>(ImportedContentCatalog.Discover(root));
            Assert.Equal((ushort)42,
                Assert.IsType<StrategicWorldGrid>(catalog.DecodeStrategicWorldGrid(asset.Id))[7, 11].TileId);

            File.WriteAllBytes(path, [1, 2, 3]);
            Assert.Null(catalog.DecodeStrategicWorldGrid(asset.Id));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportedStrategicResourcesEagerlyBindCanonicalRoutesAndProjectedWorldCells()
    {
        var (root, catalog) = CreateStrategicResourceCatalog();
        try
        {
            var resources = new ImportedOriginalStrategicResources(catalog);
            var resourceName = OriginalStrategicMovement.PropertyRouteResources[0].ResourceName;

            Assert.Equal(
                [new OriginalStrategicRoutePoint(123, -456),
                    new OriginalStrategicRoutePoint(789, 321)],
                resources.Route(resourceName, reverse: false));
            Assert.Equal(
                [new OriginalStrategicRoutePoint(789, 321),
                    new OriginalStrategicRoutePoint(123, -456)],
                resources.Route(resourceName, reverse: true));
            Assert.Equal(
                [new OriginalStrategicRoutePoint(123, -456),
                    new OriginalStrategicRoutePoint(789, 321)],
                resources.Route(OriginalStrategicTemporaryForces.Routes[0].ResourceName,
                    reverse: false));
            Assert.Throws<InvalidDataException>(() => resources.Route("not-a-route.rat", false));

            var center = StrategicWorldProjection.CellCenter(7, 11);
            Assert.True(resources.TryTerrainCell(center.X, center.Y, 0, 0, out var cell));
            Assert.Equal((7, 11, 0xA123_002Au, (ushort)42, (byte)0x23, (byte)0xA1),
                (cell.Row, cell.Column, cell.RawValue, cell.TileId, cell.Auxiliary, cell.UpperByte));
            Assert.False(resources.TryTerrainCell(0, 0, 0, 0, out _));
            Assert.True(resources.TryGridCell(7, 11, out var directCell));
            Assert.Equal(cell, directCell);
            Assert.False(resources.TryGridCell(-1, 11, out _));

            var campaign = new Campaign();
            campaign.ConfigureOriginalStrategicResources(resources);
            Assert.True(campaign.HasOriginalStrategicResources);

            var state = Campaign.NewFromTemplate(0);
            state.OriginalStrategicState = OriginalStrategicCampaignState.CreateForSchemaOneMigration(
                state.Date, OriginalStrategicMovement.InitialSpeedMultiplier);
            var slot = state.OriginalStrategicState.MovementSlots[0];
            slot.Active = true;
            slot.Mode = OriginalStrategicMovement.RoutedMode;
            slot.OriginProperty = 0;
            slot.Lord = 0;
            slot.Knights = 1;
            slot.RouteResource = resourceName;
            slot.WaypointCount = 1;
            var mismatched = new Campaign(state);
            Assert.Throws<InvalidDataException>(() =>
                mismatched.ConfigureOriginalStrategicResources(resources));
            Assert.False(mismatched.HasOriginalStrategicResources);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void ImportedStrategicResourcesRejectOneMalformedRequiredRouteAtStartup()
    {
        var malformed = OriginalStrategicMovement.PropertyRouteResources[0].ResourceName;
        var (root, catalog) = CreateStrategicResourceCatalog(malformed);
        try
        {
            var error = Assert.Throws<InvalidDataException>(() =>
                new ImportedOriginalStrategicResources(catalog));
            Assert.Contains(malformed, error.Message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, recursive: true);
        }
    }

    private static (string Root, ImportedContentCatalog Catalog) CreateStrategicResourceCatalog(
        string? malformedRoute = null)
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-strategic-provider-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        var assets = new List<ImportedAsset>();
        var routeNames = OriginalStrategicMovement.PropertyRouteResources
            .Select(route => route.ResourceName)
            .Concat(OriginalStrategicMovement.StartingRoutes.Select(route => route.ResourceName))
            .Concat(OriginalStrategicTemporaryForces.Routes.Select(route => route.ResourceName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        for (var index = 0; index < routeNames.Length; index++)
        {
            var routeName = routeNames[index];
            var data = new byte[4 + 2 * StrategicRouteDecoder.PointSize];
            BinaryPrimitives.WriteInt32LittleEndian(data, 2);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(4), 123);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(8), -456);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(12), 789);
            BinaryPrimitives.WriteInt32LittleEndian(data.AsSpan(16), 321);
            if (routeName.Equals(malformedRoute, StringComparison.OrdinalIgnoreCase))
                data = [1, 0, 0, 0];
            Add($"C1086.GOB#{index}:{routeName}", $"route-{index}.bin", data);
        }

        var world = new byte[StrategicWorldGridDecoder.EncodedLength];
        BinaryPrimitives.WriteInt32LittleEndian(world, StrategicWorldGridDecoder.CellWidth);
        BinaryPrimitives.WriteInt32LittleEndian(world.AsSpan(4), StrategicWorldGridDecoder.CellHeight);
        BinaryPrimitives.WriteInt32LittleEndian(world.AsSpan(8), StrategicWorldGridDecoder.RowCount);
        BinaryPrimitives.WriteInt32LittleEndian(world.AsSpan(12), StrategicWorldGridDecoder.ColumnCount);
        WriteWorldCell(world, row: 7, column: 11, 0xA123_002A);
        Add("C1086.GOB#292:icon.jp", "icon.jp", world);

        new ImportManifest(1, new string('a', 64), assets.ToArray())
            .Write(Path.Combine(root, "manifest.json"));
        return (root, Assert.IsType<ImportedContentCatalog>(ImportedContentCatalog.Discover(root)));

        void Add(string id, string relativePath, byte[] data)
        {
            var path = Path.Combine(root, relativePath);
            File.WriteAllBytes(path, data);
            assets.Add(new ImportedAsset(
                id, relativePath, "resource", data.Length, ResourceHash.Sha256(path)));
        }
    }

    private static void WriteWorldCell(byte[] data, int row, int column, uint value)
    {
        var offset = StrategicWorldGridDecoder.HeaderSize +
            checked((column * StrategicWorldGridDecoder.RowCount + row) * StrategicWorldGridDecoder.CellSize);
        BinaryPrimitives.WriteUInt32LittleEndian(data.AsSpan(offset), value);
    }

    [Theory]
    [InlineData(0, 0, 0, 0, 1)]
    [InlineData(0, 3, 0, 0, 1)]
    [InlineData(0, 11, 0, 0, 1)]
    [InlineData(0, 12, 1, 1, 1)]
    [InlineData(7, 11, 7, 7, 7)]
    [InlineData(2, 255, 23, 23, 23)]
    public void OriginalStrategicMovementDistributesTheLordQuarterAcrossThreeEqualTypes(
        int household, int lordRating, int swordsmen, int halberdiers, int knights)
    {
        var force = OriginalStrategicMovement.InitialForces(household, lordRating);

        Assert.Equal(new StrategicTroopCounts(swordsmen, halberdiers, knights), force);
        Assert.Equal(swordsmen + halberdiers + knights, force.Total);
    }
}
