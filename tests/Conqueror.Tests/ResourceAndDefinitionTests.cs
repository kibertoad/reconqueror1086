using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed class ResourceAndDefinitionTests
{
    [Fact]
    public void Kind1DecodesLiteralCopyAndRunTokens()
    {
        byte[] compressed = [13, 0, 0x40, 0, 0x18, 0, (byte)'A', (byte)'B', (byte)'C', 0, 0x30, 0, 0, 0, (byte)'Z'];

        var decoded = DynamixCompression.DecodeKind1(compressed, 22);

        Assert.Equal("ABCABC" + new string('Z', 16), System.Text.Encoding.ASCII.GetString(decoded));
    }

    [Fact]
    public void Kind1RejectsCopiesBeforeOutputStart()
    {
        byte[] compressed = [7, 0, 0x40, 0, 0x80, 0, 0, 0x10];

        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind1(compressed, 3));
    }

    [Fact]
    public void Kind2DecodesControlCodesAndFourteenBitGrowthSafely()
    {
        var basic = PackMsbCodes([(256, 9), (65, 9), (66, 9), (258, 9), (260, 9), (257, 9)]);
        Assert.Equal("ABABABA", System.Text.Encoding.ASCII.GetString(DynamixCompression.DecodeKind2(basic, 7)));

        (int Count, int Width)[] widthRuns = [(254, 9), (512, 10), (1_024, 11), (2_048, 12), (4_096, 13), (8_193, 14)];
        var growthCodes = new List<(int Code, int Width)> { (256, 9) };
        foreach (var (count, width) in widthRuns) growthCodes.AddRange(Enumerable.Repeat((0, width), count));
        growthCodes.Add((257, 14));
        Assert.Equal(new byte[widthRuns.Sum(run => run.Count)], DynamixCompression.DecodeKind2(PackMsbCodes(growthCodes), widthRuns.Sum(run => run.Count)));

        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(basic[..^1], 7));
        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(PackMsbCodes([(256, 9), (300, 9), (257, 9)]), 1));
        Assert.Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(basic, 7, 6));
    }

    [Fact]
    public void DynamixSoundBankParsesBoundedRateTaggedSamples()
    {
        var bytes = new byte[4 + 8 + 3 + 8 + 2];
        BinaryPrimitives.WriteUInt32LittleEndian(bytes, DynamixSoundBankDecoder.Magic);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4), 3);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(8), 11025);
        bytes[12] = 0x7f; bytes[13] = 0x80; bytes[14] = 0x81;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(15), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(19), 22050);
        bytes[23] = 0; bytes[24] = 255;

        var bank = DynamixSoundBankDecoder.Decode(bytes);

        Assert.Equal(2, bank.Samples.Count);
        Assert.Equal(11025, bank.Samples[0].SampleRate);
        Assert.Equal(new byte[] { 0x7f, 0x80, 0x81 }, bank.Samples[0].Samples);
        Assert.Equal(22050, bank.Samples[1].SampleRate);
        Assert.Equal(new byte[] { 0x00, 0xFF, 0x00, 0x00, 0x00, 0x01 },
            bank.Samples[0].ToPcm16LittleEndian());
        Assert.Throws<InvalidDataException>(() => DynamixSoundBankDecoder.Decode(bytes[..^1]));
        bytes[0] = 0;
        Assert.Throws<InvalidDataException>(() => DynamixSoundBankDecoder.Decode(bytes));
    }

    [Fact]
    public void OriginalArtRolesAreDataDrivenAndUnique()
    {
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Title.Background", IdSuffix: ":fftitle.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Options", IdSuffix: ":char_ops.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Pregenerated", IdSuffix: ":pregen.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Load.Background", IdSuffix: ":loadgame.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Map.England", IdSuffix: ":engmap1.pcx" });
        Assert.Equal(ImportedArt.Definitions.Count, ImportedArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(ImportedAnimations.Definitions.Count,
            ImportedAnimations.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(ImportedSounds.Definitions.Count,
            ImportedSounds.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(ImportedSounds.Definitions,
            sound => sound is { Role: "Interface.Activate", IdSuffix: ":gameopts.666", SampleIndex: 0 });
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition is { Role: "Interface.Cursor", IdSuffix: ":ffmouse.csf", PaletteArtRole: "Estate.Shell" });
    }

    [Fact]
    public void CharacterCreationScreenIsDefinitionDriven()
    {
        Assert.Equal(Enum.GetValues<CharacterCreationAction>().Length, CharacterCreationDefinitions.Options.Count);
        Assert.Equal(CharacterCreationDefinitions.Options.Count, CharacterCreationDefinitions.Options.Select(x => x.Action).Distinct().Count());
        Assert.Equal(Balance.Templates.Length, CharacterCreationDefinitions.PregeneratedCharacters.Count);
        Assert.Equal(["Red", "Green", "Blue"], CharacterCreationDefinitions.HeraldicColors.Select(x => x.Name));
        Assert.All(CharacterCreationDefinitions.HeraldicColors, x => Assert.True(CharacterCreationDefinitions.Options[1].OriginalBounds.Contains(x.OriginalBounds.X, x.OriginalBounds.Y)));
        Assert.Contains(ImportedArt.Definitions, definition => definition.Role == "Dilemma.Background" && definition.IdSuffix == ":morality.pcx");
        Assert.Contains(ImportedLayouts.Definitions, definition => definition.Role == "Dilemma" && definition.IdSuffix == ":chargen.hat");
    }

    [Fact]
    public void OptionsHubActionsAndOriginalRegionsAreDataDriven()
    {
        Assert.Equal(Enum.GetValues<OptionsHubAction>().Length, OptionsHubDefinitions.Options.Count);
        Assert.Equal(OptionsHubDefinitions.Options.Count,
            OptionsHubDefinitions.Options.Select(option => option.Action).Distinct().Count());
        Assert.Equal(new UiBounds(15, 243, 255, 237),
            OptionsHubDefinitions.Options.Single(option => option.Action == OptionsHubAction.NewGame).OriginalBounds);
        Assert.Equal(11, OptionsHubDefinitions.Options.Single(option => option.Action == OptionsHubAction.Resume).HatRegionId);
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Options.Background" && definition.IdSuffix == ":optfin.pcx");
        Assert.Equal(5, OptionsHubDefinitions.Options.Count(option => option.Setting.HasValue));
        Assert.Equal(5, OptionsHubDefinitions.Options.Select(option => option.Setting).OfType<OptionsHubSetting>().Distinct().Count());
        var optionsAnimation = Assert.Single(ImportedAnimations.Definitions, definition => definition.Role == "Options.Widgets");
        Assert.Equal("Options.Background", optionsAnimation.PaletteArtRole);
    }

    [Fact]
    public void LocationScreenRolesAndShopControlsAreDataDriven()
    {
        var roles = ImportedArt.Definitions.ToDictionary(definition => definition.Role);
        Assert.Equal(":tactical.pcx", roles["Home.Office"].IdSuffix);
        Assert.Equal(":fiefmgmt.pcx", roles["Farm.Management"].IdSuffix);
        Assert.Equal(":forgesmi.pcx", roles["Blacksmith.Workshop"].IdSuffix);
        Assert.Equal(":swdtemp.pcx", roles["Shop.Inventory"].IdSuffix);

        Assert.Equal(Enum.GetValues<ShopControlAction>().Length, ShopPresentationDefinitions.Controls.Count);
        Assert.Equal(ShopPresentationDefinitions.Controls.Count,
            ShopPresentationDefinitions.Controls.Select(control => control.Action).Distinct().Count());
        Assert.Equal(Enum.GetValues<ShopOverlayState>().Length, ShopPresentationDefinitions.Overlays.Count);
        Assert.Equal(ShopPresentationDefinitions.Overlays.Count,
            ShopPresentationDefinitions.Overlays.Select(overlay => overlay.State).Distinct().Count());
        Assert.Equal(1, ShopPresentationDefinitions.ViewOverlay(true).Frame);
        Assert.Equal(0, ShopPresentationDefinitions.ViewOverlay(false).Frame);
        Assert.Equal(2, ShopPresentationDefinitions.TransactionOverlay(true).Frame);
        Assert.Equal(3, ShopPresentationDefinitions.TransactionOverlay(false).Frame);
        Assert.All(ShopPresentationDefinitions.Controls, control =>
        {
            Assert.InRange(control.Bounds.X, 0, 639);
            Assert.InRange(control.Bounds.Y, 0, 479);
            Assert.InRange(control.Bounds.X + control.Bounds.Width, 1, 640);
            Assert.InRange(control.Bounds.Y + control.Bounds.Height, 1, 480);
        });
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Home.Office" && definition.IdSuffix == ":fopts.hat");
        Assert.Equal(
            [(SceneNavigationAction.Overview, "Overview", 0, new UiBounds(248, 184, 64, 31)),
             (SceneNavigationAction.Castle, "Castle", 1, new UiBounds(172, 153, 72, 44)),
             (SceneNavigationAction.Farm, "Farm", 2, new UiBounds(277, 146, 17, 38)),
             (SceneNavigationAction.Village, "Village", 3, new UiBounds(295, 141, 18, 43)),
             (SceneNavigationAction.Forest, "Forest", 4, new UiBounds(314, 147, 15, 39)),
             (SceneNavigationAction.Orders, "Orders", 5, new UiBounds(359, 123, 35, 76)),
             (SceneNavigationAction.Map, "Map", 8, new UiBounds(200, 55, 68, 85))],
            HomePresentationDefinitions.Hotspots.Select(hotspot =>
                (hotspot.Action, hotspot.HoverLabel, hotspot.HatRegionId, hotspot.Bounds)));
        Assert.Equal(
            [(SceneNavigationAction.BlacksmithDialogue, "Blacksmith", new UiBounds(253, 109, 107, 164)),
             (SceneNavigationAction.Shop, "Buy/Sell", new UiBounds(54, 2, 180, 141))],
            BlacksmithPresentationDefinitions.Hotspots.Select(hotspot => (hotspot.Action, hotspot.HoverLabel, hotspot.Bounds)));
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Blacksmith.Dialogue" && definition.IdSuffix == ":comscrn1.pcx");
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Blacksmith.Portrait" && definition.IdSuffix == ":blacksmi.pcc");
        Assert.Equal(Enum.GetValues<BlacksmithDialogueAction>().Length,
            BlacksmithDialoguePresentationDefinitions.Commands.Count);
        Assert.Equal(BlacksmithDialoguePresentationDefinitions.Commands.Count,
            BlacksmithDialoguePresentationDefinitions.Commands.Select(command => command.Action).Distinct().Count());
        Assert.Equal(BlacksmithDialoguePresentationDefinitions.Commands.SelectMany(command => command.Keys).Count(),
            BlacksmithDialoguePresentationDefinitions.Commands.SelectMany(command => command.Keys).Distinct().Count());
    }

    [Fact]
    public void EstateShellLayoutAndTerrainAreDefinitionDriven()
    {
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Estate.Shell" && definition.IdSuffix == ":icontemp.pcx");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Estate" && definition.IdSuffix == ":iconmap.hat");
        var layout = EstatePresentationDefinitions.From(null);
        Assert.Equal(new UiBounds(19, 8, 370, 433), layout.MainViewport);
        Assert.Equal(new UiBounds(422, 4, 199, 159), layout.InsetMap);
        Assert.Equal(Enum.GetValues<EstateControlAction>().Length, layout.Controls.Count);
        Assert.Equal(layout.Controls.Count, layout.Controls.Select(control => control.Action).Distinct().Count());
        Assert.Equal(Enum.GetValues<EstatePanel>().Length, layout.Controls.Count(control => control.Panel.HasValue));
        Assert.Equal(Enum.GetValues<EstateTerrainKind>().Length, EstatePresentationDefinitions.TerrainStyles.Count);
        Assert.Equal(Enum.GetValues<EstateTerrainKind>().Length, EstatePresentationDefinitions.TileFrames.Count);
        Assert.Equal(Enum.GetValues<EstateSeason>().Length, EstatePresentationDefinitions.TileAtlases.Count);
        Assert.All(EstatePresentationDefinitions.TileFrames.Values, frame => Assert.InRange(frame, 0, 336));
        Assert.Equal(EstateSeason.SpringSummer, EstatePresentationDefinitions.SeasonFor(new DateTime(1086, 3, 1)));
        Assert.Equal(EstateSeason.Autumn, EstatePresentationDefinitions.SeasonFor(new DateTime(1086, 10, 1)));
        Assert.Equal(EstateSeason.Winter, EstatePresentationDefinitions.SeasonFor(new DateTime(1086, 1, 1)));
        Assert.All(EstatePresentationDefinitions.TileAtlases, atlas =>
            Assert.Contains(ImportedAnimations.Definitions,
                definition => definition.Role == atlas.Role && definition.IdSuffix == atlas.IdSuffix
                    && definition.PaletteArtRole == "Estate.Shell"));
        foreach (var index in Enumerable.Range(0, EstatePresentationDefinitions.Columns * EstatePresentationDefinitions.Rows))
        {
            var bounds = EstatePresentationDefinitions.TileSpriteBounds(layout.MainViewport, index);
            Assert.InRange(bounds.X, layout.MainViewport.X, layout.MainViewport.X + layout.MainViewport.Width - bounds.Width);
            Assert.InRange(bounds.Y, layout.MainViewport.Y, layout.MainViewport.Y + layout.MainViewport.Height - bounds.Height);
        }
        var firstTile = EstatePresentationDefinitions.TileSpriteBounds(layout.MainViewport, 0);
        var lastTile = EstatePresentationDefinitions.TileSpriteBounds(layout.MainViewport,
            EstatePresentationDefinitions.Columns * EstatePresentationDefinitions.Rows - 1);
        Assert.Equal(new UiBounds(layout.MainViewport.X, layout.MainViewport.Y, 80, 80), firstTile);
        Assert.Equal(layout.MainViewport.X + layout.MainViewport.Width, lastTile.X + lastTile.Width);
        Assert.Equal(layout.MainViewport.Y + layout.MainViewport.Height, lastTile.Y + lastTile.Height);
        foreach (var (location, index) in World.Locations.Select((location, index) => (location, index)))
        {
            var point = EstatePresentationDefinitions.InsetPoint(layout.InsetMap, location);
            Assert.Equal(index, EstatePresentationDefinitions.LocationAt(layout.InsetMap, point.X, point.Y));
        }

        var fief = new Fief { Houses = 1 };
        fief.Crops[CropType.Grain] = 2;
        var terrain = EstatePresentationDefinitions.TerrainFor(fief);
        Assert.Equal(EstatePresentationDefinitions.Columns * EstatePresentationDefinitions.Rows, terrain.Count);
        Assert.Equal(2, terrain.Count(kind => kind == EstateTerrainKind.Grain));
        Assert.Single(terrain, kind => kind == EstateTerrainKind.Settlement);
    }

    [Fact]
    public void FarmCommandsKeepInputBehaviorAndHelpInOneRegistry()
    {
        var commands = FarmPresentationDefinitions.Commands;
        Assert.Equal(commands.Count, commands.Select(command => command.Key).Distinct().Count());
        Assert.Equal(Enum.GetValues<CropType>(), commands.Select(command => command.Action).OfType<PlantFarmAction>().Select(action => action.Crop).Order());
        Assert.Equal(Enum.GetValues<ForestIndustry>(), commands.Select(command => command.Action).OfType<DevelopForestFarmAction>().Select(action => action.Industry).Order());
        Assert.Equal(Enum.GetValues<UnitType>(), commands.Select(command => command.Action).OfType<RecruitFarmAction>().Select(action => action.Unit).Order());
        Assert.Equal(6, commands.Count(command => command.Action is BuildFarmAction));
        Assert.Equal(2, commands.Count(command => command.Action is LeaveFarmAction));
        Assert.Equal(
            [(FarmPresentationDefinitions.Section.Castle, ":fcastle.hat", 18, 20, 21),
             (FarmPresentationDefinitions.Section.Village, ":fvillage.hat", 14, 17, 18),
             (FarmPresentationDefinitions.Section.Farm, ":ffarm.hat", 9, 11, 12),
             (FarmPresentationDefinitions.Section.Forest, ":fforest.hat", 8, 10, 11)],
            FarmPresentationDefinitions.Layouts.Select(layout =>
                (layout.Section, layout.LayoutSuffix, layout.AccountRowCount, layout.TerrainRegionId, layout.WealthRegionId)));
        Assert.All(FarmPresentationDefinitions.Layouts, layout =>
            Assert.Contains(ImportedLayouts.Definitions,
                imported => imported.Role == layout.LayoutRole && imported.IdSuffix == layout.LayoutSuffix));
        Assert.All(Enum.GetValues<FarmPresentationDefinitions.Section>(), section =>
        {
            var sectionCommands = FarmPresentationDefinitions.CommandsFor(section);
            Assert.Equal(2, sectionCommands.Count(command => command.Action is LeaveFarmAction));
            Assert.All(FarmPresentationDefinitions.HelpRowsFor(section), row => Assert.False(string.IsNullOrWhiteSpace(row)));
        });
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Castle),
            command => Assert.True(command.Action is BuildFarmAction or RecruitFarmAction or LeaveFarmAction));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Village),
            command => Assert.IsType<LeaveFarmAction>(command.Action));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Farm),
            command => Assert.True(command.Action is PlantFarmAction or LeaveFarmAction));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Forest),
            command => Assert.True(command.Action is DevelopForestFarmAction or LeaveFarmAction));

        var bytes = new byte[40 + 13 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 13);
        for (var id = 0; id < 13; id++)
        {
            var offset = 40 + id * 24;
            WriteInt(bytes, offset, id); WriteInt(bytes, offset + 4, id);
            WriteInt(bytes, offset + 8, id); WriteInt(bytes, offset + 12, 1);
            WriteInt(bytes, offset + 16, 1); WriteInt(bytes, offset + 20, 1);
        }
        var importedFarm = FarmPresentationDefinitions.LayoutFrom(
            FarmPresentationDefinitions.Section.Farm, new HatLayout(bytes));
        Assert.Equal(new UiBounds(11, 11, 1, 1), importedFarm.Terrain);
        Assert.Equal(new UiBounds(12, 12, 1, 1), importedFarm.Wealth);
        Assert.Equal(new UiBounds(9, 9, 1, 1), importedFarm.Okay);
        Assert.Equal(new UiBounds(10, 10, 1, 1), importedFarm.Cancel);
        Assert.Equal(FarmPresentationDefinitions.FooterAction.Okay,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 9, 9));
        Assert.Equal(FarmPresentationDefinitions.FooterAction.Cancel,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 10, 10));
        Assert.Null(FarmPresentationDefinitions.FooterActionAt(importedFarm, 20, 20));
    }

    [Fact]
    public void FiefManagementCheckpointRestoresPendingEconomyChanges()
    {
        var campaign = new Campaign();
        campaign.State.Player.Wealth = 10_000;
        var originalWealth = campaign.State.Player.Wealth;
        var originalJournal = campaign.State.Journal.Count;
        var checkpoint = FiefManagementCheckpoint.Capture(campaign.State);

        Assert.True(campaign.Build(BuildingKind.House));
        Assert.True(campaign.Plant(CropType.Beans));
        Assert.True(campaign.DevelopForest(ForestIndustry.Timber));
        Assert.True(campaign.Recruit(UnitType.Swordsmen));
        campaign.State.Player.Home.Population = 999;
        campaign.State.Player.Home.TaxRate = 75;
        campaign.State.Player.Home.Prospector = true;
        checkpoint.Restore(campaign.State);

        Assert.Equal(originalWealth, campaign.State.Player.Wealth);
        Assert.Equal((1200, 10, 0, false), (campaign.State.Player.Home.Population,
            campaign.State.Player.Home.TaxRate, campaign.State.Player.Home.Houses,
            campaign.State.Player.Home.Prospector));
        Assert.All(campaign.State.Player.Home.Crops.Values, value => Assert.Equal(0, value));
        Assert.All(campaign.State.Player.Home.Forest.Values, value => Assert.Equal(0, value));
        Assert.All(campaign.State.Player.Army.Units.Values, value => Assert.Equal(0, value));
        Assert.Equal(originalJournal, campaign.State.Journal.Count);
    }

    [Fact]
    public void WeaponStoreTableParsesBoundedSixLineRecords()
    {
        var text = "first.smk\r\n2\r\n7\r\n9\r\n500\r\nA synthetic sword.\r\n#\r\n3\r\n8\r\n10\r\n120\r\nSynthetic armor.";

        var resource = WeaponStoreDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(text));

        Assert.Equal(2, resource.Entries.Count);
        Assert.Equal(new WeaponStoreEntry(0, "first.smk", 2, 7, 9, 500, "A synthetic sword."), resource.Entries[0]);
        Assert.Equal(8, resource.Entries[1].ImageFrame);
    }

    [Fact]
    public void WeaponStoreTableAndEquipmentMappingsRejectAmbiguity()
    {
        var incomplete = System.Text.Encoding.ASCII.GetBytes("#\r\n1\r\n2");
        Assert.Throws<InvalidDataException>(() => WeaponStoreDecoder.Decode(incomplete));
        Assert.Throws<InvalidDataException>(() => WeaponStoreDecoder.Decode([0xFF]));

        var mapped = Balance.StoreEquipment.Select(item => item.OriginalStoreRecord).ToArray();
        Assert.All(mapped, record => Assert.NotNull(record));
        Assert.Equal(mapped.Length, mapped.Distinct().Count());
        Assert.Equal(Enumerable.Range(0, 39).Select(index => (int?)index), mapped);
        Assert.Equal(2, Balance.Equipment.Single(item => item.Name == "Battle Sword").OriginalStoreRecord);
        Assert.Equal(84, Balance.Equipment.Single(item => item.Name == "Fighter's Dagger").BuyPrice);
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition.Role == "Shop.Items" && definition.IdSuffix == ":swords.csf" && definition.PaletteArtRole == "Shop.Inventory");
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition.Role == "Shop.Controls" && definition.IdSuffix == ":buysell.csf" && definition.PaletteArtRole == "Shop.Inventory");
        Assert.True(new WeaponStoreEntry(0, "item.smk", 0, 0, 0, 1, "Item").HasMovie);
        Assert.False(new WeaponStoreEntry(0, "#", 0, 0, 0, 1, "Item").HasMovie);
    }

    [Fact]
    public void DilemmaChoiceHotspotsComeFromTheOriginalLayout()
    {
        byte[] bytes = new byte[40 + 3 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 3);
        for (var index = 0; index < 3; index++)
        {
            var offset = 40 + index * 24;
            WriteInt(bytes, offset, index); WriteInt(bytes, offset + 4, 10 + index * 100);
            WriteInt(bytes, offset + 8, 300); WriteInt(bytes, offset + 12, 90);
            WriteInt(bytes, offset + 16, 140); WriteInt(bytes, offset + 20, 1);
        }
        var choices = YouthDilemmaPresentationDefinitions.ChoicesFrom(new HatLayout(bytes));

        Assert.Equal([new UiBounds(10, 300, 90, 140), new UiBounds(110, 300, 90, 140), new UiBounds(210, 300, 90, 140)], choices);
        Assert.Equal(YouthDilemmaPresentationDefinitions.Continue,
            YouthDilemmaPresentationDefinitions.ContinueFrom(new HatLayout(bytes)));
    }

    [Fact]
    public void HatLayoutDecodesAndOverridesFallbackRegions()
    {
        var bytes = new byte[64];
        WriteInt(bytes, 0, 7); WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 1);
        System.Text.Encoding.ASCII.GetBytes("SCREEN.PCX").CopyTo(bytes, 24);
        bytes[37] = 0x6d; bytes[38] = 0xc0; bytes[39] = 0x45;
        WriteInt(bytes, 40, 0); WriteInt(bytes, 44, 69); WriteInt(bytes, 48, 18);
        WriteInt(bytes, 52, 119); WriteInt(bytes, 56, 133); WriteInt(bytes, 60, 1);

        var layout = new HatLayout(bytes);

        Assert.Equal((7, "SCREEN.PCX", 0x45c06d), (layout.ScreenId, layout.BackgroundName, layout.UnknownTag));
        Assert.Equal(new HatRegion(0, 69, 18, 119, 133, 1), layout.FindRegion(0));
        Assert.Equal(new UiBounds(69, 18, 119, 133), CharacterCreationDefinitions.PregeneratedFrom(layout)[0]);
    }

    [Fact]
    public void HatLayoutRejectsOutOfBoundsRegions()
    {
        var bytes = new byte[64];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 1);
        WriteInt(bytes, 44, 650); WriteInt(bytes, 48, 10); WriteInt(bytes, 52, 20); WriteInt(bytes, 56, 20);
        Assert.Throws<InvalidDataException>(() => new HatLayout(bytes));
    }

    [Fact]
    public void LinearExecutableInternalFixupsResolveObjectRelativeTargets()
    {
        var bytes = new byte[0x180];
        const int header = 0x40;
        bytes[header] = (byte)'L'; bytes[header + 1] = (byte)'E';
        WriteInt(bytes, header + 0x14, 2); WriteInt(bytes, header + 0x28, 0x1000);
        WriteInt(bytes, header + 0x40, 0xb0); WriteInt(bytes, header + 0x44, 2);
        WriteInt(bytes, header + 0x68, 0xe0); WriteInt(bytes, header + 0x6c, 0xec);
        var firstObject = header + 0xb0;
        WriteInt(bytes, firstObject, 0x1000); WriteInt(bytes, firstObject + 4, 0x10000);
        WriteInt(bytes, firstObject + 12, 1); WriteInt(bytes, firstObject + 16, 1);
        var secondObject = firstObject + 24;
        WriteInt(bytes, secondObject, 0x2000); WriteInt(bytes, secondObject + 4, 0x20000);
        WriteInt(bytes, secondObject + 12, 2); WriteInt(bytes, secondObject + 16, 1);
        var pageTable = header + 0xe0;
        WriteInt(bytes, pageTable, 0); WriteInt(bytes, pageTable + 4, 9); WriteInt(bytes, pageTable + 8, 9);
        var record = header + 0xec;
        bytes[record] = 7; bytes[record + 1] = 0x10;
        BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(record + 2), 0x20);
        bytes[record + 4] = 2;
        BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(record + 5), 0x17c0);

        Assert.Equal(
            [new LinearExecutableFixup(1, 0x10020, 7, 2, 0x17c0, false, false)],
            LinearExecutableFixupReader.ReadInternalFixups(bytes));
        Assert.Throws<InvalidDataException>(() => LinearExecutableFixupReader.ReadInternalFixups(bytes[..(record + 8)]));
    }

    [Fact]
    public void FiveSaveSlotsRoundTripAndRecognizeLegacySlotOne()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-slots-{Guid.NewGuid():N}");
        try
        {
            var slots = new CampaignSaveSlots(root);
            Assert.Equal(5, slots.Inspect().Count);
            Assert.All(slots.Inspect(), slot => Assert.False(slot.Exists));

            var campaign = new Campaign(Campaign.NewFromTemplate(0));
            campaign.State.Date = new DateTime(1087, 4, 3);
            slots.Save(campaign, 3);

            var third = slots.Inspect(3);
            Assert.True(third.IsValid);
            Assert.Equal(campaign.State.Player.Name, third.PlayerName);
            Assert.Equal(campaign.State.Date, third.CampaignDate);
            Assert.True(slots.TryLoad(3, out var loaded, out var error));
            Assert.Null(error);
            Assert.Equal(campaign.State.Player.Name, loaded!.State.Player.Name);

            campaign.Save(Path.Combine(root, "campaign.json"));
            Assert.True(slots.Inspect(1).IsValid);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SaveSlotsRejectInvalidNumbersAndReportCorruptFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-slots-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllText(Path.Combine(root, "campaign-2.json"), "not json");
            var slots = new CampaignSaveSlots(root);

            Assert.Throws<ArgumentOutOfRangeException>(() => slots.SlotPath(0));
            Assert.True(slots.Inspect(2).Exists);
            Assert.False(slots.Inspect(2).IsValid);
            Assert.False(slots.TryLoad(2, out var campaign, out var error));
            Assert.Null(campaign);
            Assert.NotNull(error);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void DilemmaTextIsParsedIntoDataDrivenChoicesAndOutcomes()
    {
        var dilemma = DilemmaTextDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(SyntheticDilemma()));

        Assert.Equal((7, 12, "SYNTHETIC", "D777.CSF", "A synthetic prompt continues here."), (dilemma.Number, dilemma.Age, dilemma.Title, dilemma.SceneFile, dilemma.Prompt));
        Assert.Equal(3, dilemma.Choices.Count);
        Assert.All(dilemma.Choices, choice => Assert.Equal(3, choice.Outcomes.Count));
        Assert.Equal(("STRENGTH", 17, 6), (dilemma.Choices[0].ScoringAttribute, dilemma.Choices[0].HighBreakpoint, dilemma.Choices[0].LowBreakpoint));
        Assert.Equal(new DilemmaAttributeChange("HONOR", 2), dilemma.Choices[0].Outcomes.Single(x => x.Outcome == DilemmaOutcome.Win).Changes.Single());
    }

    [Fact]
    public void ImportedDilemmaDefinitionsMapThroughTypedAttributes()
    {
        var resource = DilemmaTextDecoder.Decode(System.Text.Encoding.ASCII.GetBytes(SyntheticDilemma()));

        var definition = Assert.IsType<YouthDilemmaDefinition>(ImportedDilemmaAdapter.Convert(resource));

        Assert.Equal(CharacterAttribute.Strength, definition.Choices[0].ScoringAttribute);
        Assert.Equal("D777.CSF", definition.SceneFile);
        Assert.Equal(CharacterAttribute.Honor,
            definition.Choices[0].Outcomes[YouthDilemmaOutcome.Win].Changes.Single().Attribute);
    }

    [Theory]
    [InlineData(17, YouthDilemmaOutcome.Win)]
    [InlineData(6, YouthDilemmaOutcome.Draw)]
    [InlineData(5, YouthDilemmaOutcome.Lose)]
    public void DilemmaBreakpointsUseInclusiveOrderedBands(int score, YouthDilemmaOutcome expected)
    {
        var choice = new YouthDilemmaChoiceDefinition("Choice", CharacterAttribute.Strength, 6, 17,
            new Dictionary<YouthDilemmaOutcome, YouthDilemmaOutcomeDefinition>());

        Assert.Equal(expected, YouthDilemmaRules.Resolve(choice, score));
    }

    [Fact]
    public void CampaignPersistsSelectionAndAppliesImportedOutcomeChanges()
    {
        var state = Campaign.NewCustom("Test", 42);
        state.Player.Stats = state.Player.Stats with { Strength = 17, Intelligence = 8 };
        var campaign = new Campaign(state, 42);
        var number = campaign.CurrentYouthDilemmaNumber;
        var changes = new CharacterAttributeChange[]
        {
            new(CharacterAttribute.Strength, 1),
            new(CharacterAttribute.Intelligence, 2),
            new(CharacterAttribute.SwordExperience, 1),
            new(CharacterAttribute.Age, 1)
        };
        var choice = new YouthDilemmaChoiceDefinition("Choice", CharacterAttribute.Strength, 6, 17,
            new Dictionary<YouthDilemmaOutcome, YouthDilemmaOutcomeDefinition>
            {
                [YouthDilemmaOutcome.Win] = new("Won", changes),
                [YouthDilemmaOutcome.Draw] = new("Drew", []),
                [YouthDilemmaOutcome.Lose] = new("Lost", [])
            });
        var definition = new YouthDilemmaDefinition(number, 12, "Title", "Prompt", [choice]);

        Assert.Equal(number, campaign.CurrentYouthDilemmaNumber);
        var restoredState = System.Text.Json.JsonSerializer.Deserialize<CampaignState>(
            System.Text.Json.JsonSerializer.Serialize(state));
        Assert.Equal(number, Assert.IsType<CampaignState>(restoredState).ActiveYouthDilemmaNumber);
        var result = Assert.IsType<YouthDilemmaResult>(campaign.AnswerDilemma(definition, 0));

        Assert.InRange(number, 0, 4);
        Assert.Equal(YouthDilemmaOutcome.Win, result.Outcome);
        Assert.Equal((18, 10, 1, 13), (state.Player.Stats.Strength, state.Player.Stats.Intelligence,
            state.Player.SwordExperience, state.Player.Age));
        Assert.Equal(1, state.YouthDilemmasAnswered);
        Assert.Null(state.ActiveYouthDilemmaNumber);
        Assert.InRange(campaign.CurrentYouthDilemmaNumber, 5, 9);
    }

    [Fact]
    public void DilemmaTextRejectsUnboundedOrIncompleteData()
    {
        var bytes = System.Text.Encoding.ASCII.GetBytes(SyntheticDilemma());
        Assert.Throws<InvalidDataException>(() => DilemmaTextDecoder.Decode(bytes, bytes.Length - 1));
        Assert.Throws<InvalidDataException>(() => DilemmaTextDecoder.Decode("!HEADER\n7 D777.CSF\n"u8));
        Assert.Throws<InvalidDataException>(() => DilemmaTextDecoder.Decode([0xff]));
    }

    private static string SyntheticDilemma()
    {
        var text = new System.Text.StringBuilder("# AGE: 12\r\n# TITLE: SYNTHETIC\r\n!DILEMMA_NUMBER DILEMMA_SFG_FILE\r\n7 D777.CSF\r\n&DILEMMA TEXT\r\n^A synthetic prompt\r\n^continues here.\r\n");
        var attributes = new[] { "STRENGTH", "DEXTERITY", "NONE" };
        foreach (var choice in Enumerable.Range(1, 3))
        {
            text.Append("@RELEVANT SCORING ATTRIBUTE\r\n~").Append(attributes[choice - 1]).Append("\r\n");
            text.Append("%HIGH SCORING BREAKPOINT LOW SCORING BREAKPOINT\r\n17 6\r\n");
            foreach (var outcome in Enum.GetNames<DilemmaOutcome>())
            {
                text.Append("?DILEMMA CHOICE ").Append(choice).Append(' ').Append(outcome.ToUpperInvariant()).Append(" TEXT\r\n");
                text.Append("^Synthetic outcome text.\r\n*NUMBER OF ATTRIBUTES MODIFIED\r\n1\r\n$ATTRIBUTE MODIFIER\r\nHONOR 2\r\n");
            }
        }
        return text.Append('\u001a').ToString();
    }

    private static void WriteInt(byte[] target, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, 4), value);

    private static byte[] PackMsbCodes(IEnumerable<(int Code, int Width)> codes)
    {
        var values = codes.ToArray();
        var result = new byte[(values.Sum(value => value.Width) + 7) / 8];
        var bitPosition = 0;
        foreach (var (code, width) in values)
            for (var bit = width - 1; bit >= 0; bit--, bitPosition++)
                if ((code & (1 << bit)) != 0) result[bitPosition >> 3] |= (byte)(1 << (7 - (bitPosition & 7)));
        return result;
    }

    [Fact]
    public void BalanceDefinitionsRemainInternallyConsistent()
    {
        Assert.All(Balance.Buildings, x => Assert.Equal(x.Key, x.Value.Kind));
        Assert.Equal(UnitType.Swordsmen, Balance.Counter(UnitType.Knights));
        Assert.Equal(UnitType.Halberdiers, Balance.Counter(UnitType.Swordsmen));
        Assert.Equal(UnitType.Knights, Balance.Counter(UnitType.Halberdiers));
    }
}
