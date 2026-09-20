namespace Conqueror.Core;

/// <summary>
/// Executable-mapped portions of the original five-slot strategic movement system.
/// Route and lord adapters remain separate because the current campaign model does not
/// yet identify the original property records with its named destinations.
/// </summary>
public static partial class OriginalStrategicMovement
{
    public const int SlotCount = 5;
    public const int RecordSize = 0x118;
    public const int PlayerMovementTableAddress = 0x1A4B8;
    public const int PlayerMovementRecordCount = 6;
    public const int PlayerArmyMovementCount = 5;
    public const int PlayerAvatarMovementSlot = 5;
    public const int ActivePlayerRecordCountAddress = 0xAE5C;
    public const int SelectedPlayerMovementSlotAddress = 0xAE64;
    public const int EngagedPlayerMovementSlotAddress = 0xAE6C;
    public const int PlayerRouteInputActiveAddress = 0xAE58;
    public const int PlayerHomeGridYAddress = 0x1B0D0;
    public const int PlayerHomeGridXAddress = 0x1B0D4;
    public const int PlayerState8Offset = 0x08;
    public const int PlayerSelectedOffset = 0x04;
    public const int PlayerTargetHandleOffset = 0x14;
    public const int PlayerWaypointCountOffset = 0x18;
    public const int PlayerWaypointIndexOffset = 0x30;
    public const int PlayerCollisionCooldownOffset = 0x54;
    public const int PlayerTerrainKindOffset = 0x58;
    public const int PlayerWaypointArrayOffset = 0x70;
    public const int PlayerWaypointCapacity = 21;
    public const int PlayerRouteInputLimit = 20;
    public const int PlayerEnemyTargetFlag = 0x1000;
    public const int PlayerDivisionTargetFlag = 0x10000;
    public const int PlayerDivisionTargetCount = 3;
    public const int PlayerDirectionLimit = 40;
    public const double PlayerBlockedProbeDistance = 50;
    public const double PlayerEnemyContactDistance = 30;
    public const int PlayerAvatarCollisionCooldown = 120;
    public const int MaximumActivePlayerRecordCount = 6;
    public const int PlayerFormationOffsetTableAddress = 0xA564;
    public const int MovementMarkerFrameTableAddress = 0xAE88;
    public const int GenerationIntervalUnits = 0x1388;
    public const int InitialSpeedMultiplier = 1;
    public const int MinimumSpeedMultiplier = 1;
    public const int MaximumSpeedMultiplier = 15;
    public const int GenerationRollLimit = 0x64;
    public const int GenerationStartThreshold = 0x60;
    public const int GenerationPropertyEligibilityValue = 1;
    public const int ReactiveSpecialProperty = 7;
    public const int ReactiveSpecialPropertyDelay = 0x3E8;
    public const int ReactiveExistingPursuitDelay = 0x32;
    public const int ReactiveSpawnRollLimit = 6;
    public const int ReactiveSpawnRollExclusiveMaximum = 2;
    public const int ReactiveNearDistance = 30;
    public const int ReactiveApproachDistance = 250;
    public const int ReactiveSpecialNearDistance = 40;
    public const int ReactiveSpecialApproachDistance = 200;
    public const int ReactiveSpecialBoundsX = 700;
    public const int ReactiveSpecialBoundsY = 600;
    public const int ReactiveSpecialBoundsWidth = 0x2EA4;
    public const int ReactiveSpecialBoundsHeight = 0x0F78;

    public const int ActiveOffset = 0x00;
    public const int PathCompleteOffset = 0x0C;
    public const int TargetMovementSlotOffset = 0x14;
    public const int WaypointCountOffset = 0x18;
    public const int SwordsmenOffset = 0x1C;
    public const int HalberdiersOffset = 0x20;
    public const int KnightsOffset = 0x24;
    public const int OriginLocationOffset = 0x28;
    public const int LordOffset = 0x2C;
    public const int ModeOffset = 0x34;
    public const int MarkerFrameOffset = 0x38;
    public const int DestinationXOffset = 0x3C;
    public const int DestinationYOffset = 0x40;
    public const int GridXOffset = 0x44;
    public const int GridYOffset = 0x48;
    public const int CurrentXOffset = 0x5C;
    public const int CurrentYOffset = 0x60;
    public const int DirectionXOffset = 0x64;
    public const int DirectionYOffset = 0x68;
    public const int RoutePointerOffset = 0x6C;

    public const int DirectPropertyMode = 1;
    public const int RoutedMode = 2;
    public const int PursuitMode = 3;
    public const int CompletedSignal = 0xFFFF;
    public const int WaypointCoordinateTolerance = 6;
    public const int MaximumRoutedStep = 50;
    public const int DirectTerrainScaleAddress = 0x4F97;
    public const double DirectTerrainScale = 0.9;
    public const int RoutedHorizontalLimitAddress = 0x7389;
    public const int RetargetFieldArmyRange = 300;
    public const int TerrainKindCount = 30;
    public const int TerrainTileKindTableAddress = 0xAF78;
    public const int TerrainTileKindCount = 331;
    public const int TerrainProfileCount = 4;
    public const int ReducedTerrainProfile = 2;
    public const int ImpassableTerrainKind = 9;

    // Object-2 +0xAE88 stores the property-keyed selector which 0x3A764 and
    // 0x3AC5C shift left three places into each live movement record +0x38.
    // Only the first fourteen entries are reachable through the 0..13
    // strategic property domain.
    private static readonly int[] MovementMarkerFrameBases =
        [88, 8, 16, 72, 32, 56, 48, 56, 64, 72, 80, 88, 96, 64];

    /// <summary>
    /// Returns the direct <c>icon_men.CSF</c> frame stored at a constructed
    /// movement record's <c>+0x38</c> for its origin property.
    /// </summary>
    public static int MovementMarkerFrameForOriginProperty(int property)
    {
        if (property is < 0 or >= PropertyCount)
            throw new ArgumentOutOfRangeException(nameof(property));
        return MovementMarkerFrameBases[property];
    }

    public const int StartingRouteSelectorAddress = 0xC9D0;
    public const int StartingPersonIndexTableAddress = 0xB8C8;
    public const int StartingRouteTableAddress = 0xCA98;
    public const int StartingRouteCount = 7;
    public const int MonthProfileTableAddress = 0xB720;
    public const int SeasonMovieTableAddress = 0xB750;
    public const int SeasonAtlasTableAddress = 0xB760;

    public const int PropertyTableAddress = 0xB8EC;
    public const int PropertyCount = 14;
    public const int PropertyRecordSize = 0x0F;
    public const int PropertyOwnerOrStateOffset = 0x00;
    public const int PropertyGridXOffset = 0x01;
    public const int PropertyGridYOffset = 0x03;
    public const int PropertyMapX8Offset = 0x05;
    public const int PropertyMapY8Offset = 0x07;
    public const int PropertyLordOffset = 0x09;
    public const int PropertyListNextOffset = 0x0A;
    public const int PropertyGarrisonOffset = 0x0C;
    public const int PropertyState13Offset = 0x0D;
    public const int PropertyState14Offset = 0x0E;

    public const int PersonTableAddress = 0xBA50;
    public const int PersonCount = 176;
    public const int PersonRecordSize = 0x12;
    public const int PersonNameAddressOffset = 0x00;
    public const int PersonGroupOffset = 0x04;
    public const int PersonState5Offset = 0x05;
    public const int PersonFlagsOffset = 0x06;
    public const int PersonAssignmentOffset = 0x07;
    public const int PersonXOffset = 0x08;
    public const int PersonYOffset = 0x0A;
    public const int PersonLordRatingOffset = 0x0C;
    public const int PersonListNextOffset = 0x0D;
    public const int PersonState14Offset = 0x0E;
    public const int PersonState15Offset = 0x0F;
    public const int PersonState16Offset = 0x10;
    public const int PersonState17Offset = 0x11;
    public const int HouseholdEligibleFlag = 0x01;

    private static readonly OriginalStrategicPropertyDefinition[] PropertyRows =
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

    private static readonly int[] InitialHouseholdCounts = [4, 6, 7, 5, 2, 2, 4, 3, 3, 1, 9, 3, 3, 6];

    private static readonly OriginalStrategicPropertyIdentity[] PropertyIdentityRows =
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

    private static readonly StrategicContactProbe[] ContactProbeRows =
    [
        new(0, 0),
        new(0, -2),
        new(1, 0),
        new(-1, -1),
        new(0, 1)
    ];

    private static readonly OriginalStrategicRoute[] PropertyRouteResourceRows = BuildPropertyRouteResources();

    private static readonly OriginalStrategicStartingRoute[] StartingRouteRows =
    [
        new(0, 17, "Scott's Keep", 0, 1, 94, 40, "sc_0.rat"),
        new(1, 18, "Anne's Castle", 0, 1, 129, 23, "sc_1.rat"),
        new(2, 24, "Stonetree Castle", 1, 2, 142, 82, "sc_2.rat"),
        new(3, 29, "MacGibbon on the Hill", 1, 2, 120, 100, "sc_3.rat"),
        new(4, 86, "Leecastle", 4, 6, 125, 152, "sc_4.rat"),
        new(5, 144, "Damron Castle", 12, 13, 110, 264, "sc_5.rat"),
        new(6, 166, "Sabine's Keep", 13, 14, 27, 306, "sc_6.rat")
    ];

    private static readonly StrategicTerrainProfile[] MonthProfileRows =
    [
        StrategicTerrainProfile.Winter, StrategicTerrainProfile.Winter,
        StrategicTerrainProfile.Spring, StrategicTerrainProfile.Spring, StrategicTerrainProfile.Spring,
        StrategicTerrainProfile.Summer, StrategicTerrainProfile.Summer, StrategicTerrainProfile.Summer,
        StrategicTerrainProfile.Autumn, StrategicTerrainProfile.Autumn, StrategicTerrainProfile.Autumn,
        StrategicTerrainProfile.Winter
    ];

    private static readonly OriginalStrategicTerrainProfileDefinition[] TerrainProfileRows =
    [
        new(StrategicTerrainProfile.Summer, "tran4.smk", "ics.csf"),
        new(StrategicTerrainProfile.Autumn, "tran2.smk", "ica.csf"),
        new(StrategicTerrainProfile.Winter, "tran3.smk", "icw.csf"),
        new(StrategicTerrainProfile.Spring, "tran1.smk", "ics.csf")
    ];

    private static readonly float[] PrimaryTerrainSpeeds =
    [
        1f, 0.2f, 1f, 0.5f, 1f, 0.5f, 0.8f, 0.8f, 1f, 5f,
        0.3f, 0.6f, 0.8f, 0.8f, 2f, 2f, 2f, 1f, 0.2f, 0.5f,
        0.5f, 0.5f, 0.5f, 3f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.2f
    ];

    private static readonly float[] ReducedTerrainSpeeds =
    [
        0.8f, 0.2f, 0.8f, 0.5f, 0.8f, 0.5f, 0.6f, 0.6f, 0.8f, 5f,
        0.2f, 0.4f, 0.5f, 0.5f, 1.5f, 1.5f, 1.5f, 0.8f, 0.1f, 0.4f,
        0.4f, 0.4f, 0.4f, 2f, 1f, 1f, 0.5f, 0.5f, 0.5f, 0.6f
    ];

    private static readonly byte[] TerrainKindsByTile =
    [
        9,9,23,23,23,23,23,23,23,23,23,23,23,23,23,23,24,25,0,22,22,22,22,22,26,27,28,28,9,23,23,23,
        23,23,23,23,23,23,23,23,23,23,23,24,25,0,22,22,22,22,22,22,22,22,22,22,22,22,22,22,14,14,14,14,
        23,22,22,23,22,22,23,21,21,9,9,9,22,22,22,22,22,22,22,22,22,22,21,20,20,21,21,21,21,21,21,21,
        21,21,21,21,19,19,17,17,17,17,17,17,17,17,17,18,18,18,18,18,18,18,18,18,17,17,17,17,29,29,29,29,
        29,29,19,19,17,17,17,17,17,17,17,17,17,18,18,18,18,18,18,18,18,18,17,17,17,17,29,29,29,29,29,29,
        9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,
        9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,15,15,15,15,15,15,15,14,9,9,9,9,
        9,9,9,9,9,9,9,9,9,9,9,9,15,15,15,15,15,15,15,14,15,15,15,14,15,15,15,29,29,10,11,12,
        13,29,16,16,16,16,16,16,14,14,16,16,16,16,16,16,16,15,15,15,14,15,15,15,29,29,10,11,12,13,29,8,
        7,6,5,4,3,2,2,2,2,2,9,9,9,9,9,9,9,9,9,8,7,6,5,4,3,2,2,2,2,1,1,1,
        1,1,1,1,1,1,1,1,1,1,1
    ];

    public static IReadOnlyList<OriginalStrategicPropertyDefinition> Properties => PropertyRows;

    public static IReadOnlyList<int> InitialActiveHouseholdCounts => InitialHouseholdCounts;

    public static IReadOnlyList<OriginalStrategicPropertyIdentity> PropertyIdentities => PropertyIdentityRows;

    public static IReadOnlyList<StrategicContactProbe> ContactProbes => ContactProbeRows;

    public static IReadOnlyList<OriginalStrategicRoute> PropertyRouteResources => PropertyRouteResourceRows;

    public static IReadOnlyList<OriginalStrategicStartingRoute> StartingRoutes => StartingRouteRows;

    public static IReadOnlyList<StrategicTerrainProfile> MonthProfiles => MonthProfileRows;

    public static IReadOnlyList<OriginalStrategicTerrainProfileDefinition> TerrainProfiles => TerrainProfileRows;

    public static StrategicTerrainProfile TerrainProfileForMonth(int zeroBasedMonth)
    {
        if (zeroBasedMonth is < 0 or >= 12) throw new ArgumentOutOfRangeException(nameof(zeroBasedMonth));
        return MonthProfileRows[zeroBasedMonth];
    }

    public static bool TryGetStartingRoute(int selector, out OriginalStrategicStartingRoute route)
    {
        if (selector is < 0 or >= StartingRouteCount)
        {
            route = default;
            return false;
        }
        route = StartingRouteRows[selector];
        return true;
    }

    public static StrategicGenerationClockAdvance AdvanceGenerationClock(
        int accumulatorUnits,
        int speedMultiplier,
        int activeMovementCount,
        int? roll)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(accumulatorUnits);
        ValidateSpeedMultiplier(speedMultiplier);
        ArgumentOutOfRangeException.ThrowIfNegative(activeMovementCount);

        if (accumulatorUnits < GenerationIntervalUnits)
            return new(checked(accumulatorUnits + speedMultiplier), BoundaryReached: false,
                RollConsumed: false, StartGeneration: false);

        if (activeMovementCount >= SlotCount)
            return new(0, BoundaryReached: true, RollConsumed: false, StartGeneration: false);

        if (roll is null)
            throw new ArgumentNullException(nameof(roll), "A generation roll is required at an eligible boundary.");
        if (roll < 0 || roll >= GenerationRollLimit)
            throw new ArgumentOutOfRangeException(nameof(roll));

        return new(0, BoundaryReached: true, RollConsumed: true,
            StartGeneration: roll > GenerationStartThreshold);
    }

    public static bool CanAttemptReactivePursuit(
        int property,
        int movementSlot,
        int playerMovementSlot,
        int callsSinceReactiveSuccess,
        bool hasExistingPursuer)
    {
        ValidateProperty(property);
        ValidateSlot(movementSlot, nameof(movementSlot));
        ValidatePlayerMovementSlot(playerMovementSlot, nameof(playerMovementSlot));
        ArgumentOutOfRangeException.ThrowIfNegative(callsSinceReactiveSuccess);

        if (property == ReactiveSpecialProperty &&
            callsSinceReactiveSuccess < ReactiveSpecialPropertyDelay &&
            movementSlot != playerMovementSlot)
            return false;

        return !hasExistingPursuer ||
            movementSlot == playerMovementSlot && callsSinceReactiveSuccess > ReactiveExistingPursuitDelay;
    }

    public static bool HasLivePursuer(
        IReadOnlyList<StrategicMovementPursuitState> movements,
        int targetSlot)
    {
        ArgumentNullException.ThrowIfNull(movements);
        ValidateSlot(targetSlot, nameof(targetSlot));
        if (movements.Count != SlotCount)
            throw new ArgumentException($"Pursuit inspection requires exactly {SlotCount} movement states.", nameof(movements));

        return movements.Any(movement =>
            movement.Active && movement.Mode == PursuitMode && movement.TargetSlot == targetSlot);
    }

    public static bool ShouldAttemptReactiveSpawn(int roll, int activeMovementCount)
    {
        if (roll < 0 || roll >= ReactiveSpawnRollLimit)
            throw new ArgumentOutOfRangeException(nameof(roll));
        ArgumentOutOfRangeException.ThrowIfNegative(activeMovementCount);
        return roll < ReactiveSpawnRollExclusiveMaximum && activeMovementCount < SlotCount;
    }

    public static int FirstFreeSlot(IReadOnlyList<bool> activeSlots)
    {
        ArgumentNullException.ThrowIfNull(activeSlots);
        if (activeSlots.Count != SlotCount)
            throw new ArgumentException($"Slot selection requires exactly {SlotCount} states.", nameof(activeSlots));
        for (var index = 0; index < activeSlots.Count; index++)
            if (!activeSlots[index]) return index;
        return -1;
    }

    public static IReadOnlyList<int> ActiveMovementSlots(IReadOnlyList<bool> activeSlots)
    {
        ArgumentNullException.ThrowIfNull(activeSlots);
        if (activeSlots.Count != SlotCount)
            throw new ArgumentException($"Slot selection requires exactly {SlotCount} states.", nameof(activeSlots));
        return activeSlots.Select((active, index) => (active, index))
            .Where(entry => entry.active).Select(entry => entry.index).ToArray();
    }

    public static IReadOnlyList<StrategicMovementConstructionAttempt> ConstructionFallback(
        int originProperty,
        StrategicRouteSelection routeSelection)
    {
        ValidateProperty(originProperty);
        return
        [
            new(originProperty, RoutedMode, routeSelection),
            new(originProperty, DirectPropertyMode, routeSelection)
        ];
    }

    public static IReadOnlyList<StrategicMovementConstructionAttempt> TimedConstructionFallback(
        bool propertyListPresent,
        int globalOriginProperty,
        int selectedProperty)
    {
        if (!propertyListPresent)
            return ConstructionFallback(globalOriginProperty, StrategicRouteSelection.AlternateAuthoredRoute);
        if (selectedProperty < 0) return [];
        return ConstructionFallback(selectedProperty, StrategicRouteSelection.CanonicalPropertyPair);
    }

    public static float TerrainSpeed(int profile, int terrainKind)
    {
        if (profile < 0 || profile >= TerrainProfileCount)
            throw new ArgumentOutOfRangeException(nameof(profile));
        if (terrainKind < 0 || terrainKind >= TerrainKindCount)
            throw new ArgumentOutOfRangeException(nameof(terrainKind));
        return (profile == ReducedTerrainProfile ? ReducedTerrainSpeeds : PrimaryTerrainSpeeds)[terrainKind];
    }

    public static int TerrainKindForTile(int tileId)
    {
        if (tileId < 0 || tileId >= TerrainKindsByTile.Length)
            throw new ArgumentOutOfRangeException(nameof(tileId));
        return TerrainKindsByTile[tileId];
    }

    public static StrategicRoutedStep CalculateRoutedStep(
        float normalizedDirectionX,
        float normalizedDirectionY,
        int speedMultiplier,
        int profile,
        int terrainKind)
    {
        ValidateSpeedMultiplier(speedMultiplier);
        if (terrainKind == ImpassableTerrainKind)
            return new(0, 0, StrategicRoutedStepOutcome.DeactivateForImpassableTerrain);

        var speed = TerrainSpeed(profile, terrainKind);
        var deltaX = (double)normalizedDirectionX * speed * speedMultiplier;
        var deltaY = (double)normalizedDirectionY * speed * speedMultiplier;
        return new(
            (float)deltaX,
            (float)deltaY,
            Math.Abs(deltaX) < MaximumRoutedStep
                ? StrategicRoutedStepOutcome.Apply
                : StrategicRoutedStepOutcome.HoldForHorizontalLimit);
    }

    public static IReadOnlyList<int> GenerationPropertyCandidates(
        IReadOnlyList<OriginalStrategicPropertyGenerationState> properties)
    {
        ArgumentNullException.ThrowIfNull(properties);
        if (properties.Count != PropertyCount)
            throw new ArgumentException($"Generation requires exactly {PropertyCount} property states.", nameof(properties));

        var candidates = new List<int>();
        for (var index = 0; index < properties.Count; index++)
        {
            var property = properties[index];
            if (property.OwnerOrState != 0 && property.State13 == GenerationPropertyEligibilityValue)
                candidates.Add(index);
        }
        return candidates;
    }

    public static int SelectGenerationProperty(
        IReadOnlyList<OriginalStrategicPropertyGenerationState> properties,
        int candidateOrdinal)
    {
        var candidates = GenerationPropertyCandidates(properties);
        if (candidates.Count == 0) return -1;
        ArgumentOutOfRangeException.ThrowIfNegative(candidateOrdinal);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(candidateOrdinal, candidates.Count);
        return candidates[candidateOrdinal];
    }

    public static StrategicContactOutcome ResolveCompletedContact(
        int originOwnerOrState,
        int? contactedAssignment,
        bool contactedPersonEligible)
    {
        if (contactedAssignment is 0 && contactedPersonEligible)
            return StrategicContactOutcome.Encounter;

        return contactedAssignment == originOwnerOrState
            ? StrategicContactOutcome.ReinforceOriginAndDeactivate
            : StrategicContactOutcome.Retarget;
    }

    public static bool TryGetPropertyRoute(int fromProperty, int toProperty, out OriginalStrategicRoute route)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(fromProperty);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(fromProperty, PropertyCount);
        ArgumentOutOfRangeException.ThrowIfNegative(toProperty);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(toProperty, PropertyCount);

        if (fromProperty == toProperty ||
            (Math.Min(fromProperty, toProperty) == 6 && Math.Max(fromProperty, toProperty) == 12))
        {
            route = default;
            return false;
        }

        route = new OriginalStrategicRoute(
            $"rt_{Math.Min(fromProperty, toProperty) + 1}_{Math.Max(fromProperty, toProperty) + 1}.rat",
            Reverse: fromProperty > toProperty);
        return true;
    }

    public static StrategicRetargetSelection SelectRetarget(
        StrategicPoint current,
        IReadOnlyList<StrategicRetargetCandidate> fieldArmies,
        StrategicPoint player,
        StrategicPoint originProperty)
    {
        ArgumentNullException.ThrowIfNull(fieldArmies);
        var rangeSquared = RetargetFieldArmyRange * RetargetFieldArmyRange;
        for (var index = 0; index < fieldArmies.Count; index++)
        {
            var candidate = fieldArmies[index];
            if (candidate.Active && DistanceSquared(current, candidate.Position) < rangeSquared)
                return new StrategicRetargetSelection(StrategicRetargetKind.FieldArmy, index);
        }

        return DistanceSquared(current, player) <= DistanceSquared(current, originProperty)
            ? new StrategicRetargetSelection(StrategicRetargetKind.Player, -1)
            : new StrategicRetargetSelection(StrategicRetargetKind.OriginProperty, -1);
    }

    private static long DistanceSquared(StrategicPoint first, StrategicPoint second)
    {
        var dx = (long)first.X - second.X;
        var dy = (long)first.Y - second.Y;
        return checked(dx * dx + dy * dy);
    }

    private static void ValidateProperty(int property)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(property);
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(property, PropertyCount);
    }

    private static void ValidateSlot(int slot, string parameterName)
    {
        if (slot < 0 || slot >= SlotCount) throw new ArgumentOutOfRangeException(parameterName);
    }

    private static void ValidatePlayerMovementSlot(int slot, string parameterName)
    {
        if (slot < 0 || slot >= PlayerMovementRecordCount)
            throw new ArgumentOutOfRangeException(parameterName);
    }

    private static void ValidateSpeedMultiplier(int speedMultiplier)
    {
        if (speedMultiplier is < MinimumSpeedMultiplier or > MaximumSpeedMultiplier)
            throw new ArgumentOutOfRangeException(nameof(speedMultiplier));
    }

    private static OriginalStrategicRoute[] BuildPropertyRouteResources()
    {
        var routes = new List<OriginalStrategicRoute>();
        for (var from = 0; from < PropertyCount; from++)
        for (var to = from + 1; to < PropertyCount; to++)
            if (TryGetPropertyRoute(from, to, out var route)) routes.Add(route);
        return [.. routes];
    }

    public static StrategicTroopCounts InitialForces(int activeHouseholdCount, int lordRating)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(activeHouseholdCount);
        ArgumentOutOfRangeException.ThrowIfNegative(lordRating);

        var each = checked(activeHouseholdCount + (lordRating / 4) / 3);
        return each == 0
            ? new StrategicTroopCounts(0, 0, 1)
            : new StrategicTroopCounts(each, each, each);
    }
}

public readonly record struct OriginalStrategicPropertyDefinition(
    byte OwnerOrState,
    ushort GridX,
    ushort GridY,
    ushort MapX8,
    ushort MapY8,
    byte Lord,
    ushort ListNext,
    byte Garrison,
    byte State13,
    byte State14);

public readonly record struct OriginalStrategicPropertyIdentity(
    string Name,
    int NameAddress,
    byte PersonGroup,
    byte InitialAssignment,
    byte LordRating);

public readonly record struct OriginalStrategicPersonDefinition(
    int NameAddress,
    byte Group,
    byte State5,
    byte Flags,
    byte Assignment,
    ushort X,
    ushort Y,
    byte LordRating,
    byte ListNext,
    byte State14,
    byte State15,
    byte State16,
    byte State17);

public readonly record struct OriginalStrategicPropertyGenerationState(byte OwnerOrState, byte State13);

public readonly record struct OriginalStrategicStartingRoute(
    int Selector,
    int Person,
    string Name,
    byte OriginProperty,
    byte InitialAssignment,
    ushort GridX,
    ushort GridY,
    string ResourceName);

public enum StrategicTerrainProfile
{
    Summer = 0,
    Autumn = 1,
    Winter = 2,
    Spring = 3
}

public readonly record struct OriginalStrategicTerrainProfileDefinition(
    StrategicTerrainProfile Profile,
    string TransitionMovieResource,
    string AtlasResource)
{
    public bool UsesReducedTerrainSpeeds => Profile == StrategicTerrainProfile.Winter;
}

public readonly record struct StrategicGenerationClockAdvance(
    int AccumulatorUnits,
    bool BoundaryReached,
    bool RollConsumed,
    bool StartGeneration);

public readonly record struct StrategicMovementPursuitState(bool Active, int Mode, int TargetSlot);

public readonly record struct StrategicMovementConstructionAttempt(
    int OriginProperty,
    int Mode,
    StrategicRouteSelection RouteSelection);

public enum StrategicRouteSelection
{
    AlternateAuthoredRoute = 0,
    CanonicalPropertyPair = 1
}

public readonly record struct StrategicRoutedStep(float DeltaX, float DeltaY, StrategicRoutedStepOutcome Outcome);

public enum StrategicRoutedStepOutcome
{
    Apply,
    HoldForHorizontalLimit,
    DeactivateForImpassableTerrain
}

public readonly record struct StrategicContactProbe(int X, int Y);

public enum StrategicContactOutcome
{
    Encounter,
    ReinforceOriginAndDeactivate,
    Retarget
}

public readonly record struct OriginalStrategicRoute(string ResourceName, bool Reverse);

public readonly record struct StrategicPoint(int X, int Y);

public readonly record struct StrategicRetargetCandidate(bool Active, StrategicPoint Position);

public readonly record struct StrategicRetargetSelection(StrategicRetargetKind Kind, int CandidateIndex);

public enum StrategicRetargetKind
{
    FieldArmy,
    Player,
    OriginProperty
}

public readonly record struct StrategicTroopCounts(int Swordsmen, int Halberdiers, int Knights)
{
    public int Total => checked(Swordsmen + Halberdiers + Knights);
}
