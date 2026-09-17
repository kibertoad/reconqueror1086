using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void OriginalStrategicMovementLayoutMatchesExecutableRecord()
    {
        Assert.Equal(5, OriginalStrategicMovement.SlotCount);
        Assert.Equal(0x118, OriginalStrategicMovement.RecordSize);
        Assert.Equal(0x1388, OriginalStrategicMovement.GenerationIntervalMilliseconds);
        Assert.Equal(0x64, OriginalStrategicMovement.GenerationRollLimit);
        Assert.Equal(0x60, OriginalStrategicMovement.GenerationStartThreshold);

        Assert.Equal(0x00, OriginalStrategicMovement.ActiveOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PathCompleteOffset);
        Assert.Equal(0x14, OriginalStrategicMovement.TargetLocationOffset);
        Assert.Equal(0x18, OriginalStrategicMovement.WaypointCountOffset);
        Assert.Equal(0x1C, OriginalStrategicMovement.SwordsmenOffset);
        Assert.Equal(0x20, OriginalStrategicMovement.HalberdiersOffset);
        Assert.Equal(0x24, OriginalStrategicMovement.KnightsOffset);
        Assert.Equal(0x28, OriginalStrategicMovement.OriginLocationOffset);
        Assert.Equal(0x2C, OriginalStrategicMovement.LordOffset);
        Assert.Equal(0x34, OriginalStrategicMovement.ModeOffset);
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
        Assert.Equal(300, OriginalStrategicMovement.RetargetFieldArmyRange);
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
        Assert.Equal(0x06, OriginalStrategicMovement.PersonFlagsOffset);
        Assert.Equal(0x07, OriginalStrategicMovement.PersonAssignmentOffset);
        Assert.Equal(0x08, OriginalStrategicMovement.PersonXOffset);
        Assert.Equal(0x0A, OriginalStrategicMovement.PersonYOffset);
        Assert.Equal(0x0C, OriginalStrategicMovement.PersonLordRatingOffset);
        Assert.Equal(0x0D, OriginalStrategicMovement.PersonListNextOffset);
        Assert.Equal(0x01, OriginalStrategicMovement.HouseholdEligibleFlag);
        Assert.Equal([4, 6, 7, 5, 2, 2, 4, 3, 3, 1, 9, 3, 3, 6],
            OriginalStrategicMovement.InitialActiveHouseholdCounts);
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
        Assert.False(OriginalStrategicMovement.TryGetPropertyRoute(0, 12, out _));
        Assert.False(OriginalStrategicMovement.TryGetPropertyRoute(12, 0, out _));

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

    [Theory]
    [InlineData(0, 0, 0, 0, 1)]
    [InlineData(0, 3, 0, 0, 1)]
    [InlineData(0, 4, 1, 1, 1)]
    [InlineData(7, 11, 9, 9, 9)]
    [InlineData(2, 255, 65, 65, 65)]
    public void OriginalStrategicMovementUsesLordQuarterAndActiveHousehold(
        int household, int lordRating, int swordsmen, int halberdiers, int knights)
    {
        var force = OriginalStrategicMovement.InitialForces(household, lordRating);

        Assert.Equal(new StrategicTroopCounts(swordsmen, halberdiers, knights), force);
        Assert.Equal(swordsmen + halberdiers + knights, force.Total);
    }
}
