using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

var passed = 0;
var failed = 0;
void Check(bool condition, string name)
{
    if (condition) { Console.WriteLine("PASS  " + name); passed++; }
    else { Console.Error.WriteLine("FAIL  " + name); failed++; }
}

try
{

Check(Balance.Crops[CropType.Vegetables] == new CropBalance(1, 10, 10), "vegetable balance");
Check(Balance.Crops[CropType.Beans].HarvestRevenueAt50 == 25, "bean harvest balance");
Check(Balance.Forest[ForestIndustry.GoldMine] == new ForestBalance(400, 36, 10), "gold mine balance");
Check(Balance.Units[UnitType.Halberdiers] == new UnitBalance(23, 15, 8, 5), "halberdier balance");
Check(Balance.Counter(UnitType.Knights) == UnitType.Swordsmen && Balance.Counter(UnitType.Swordsmen) == UnitType.Halberdiers && Balance.Counter(UnitType.Halberdiers) == UnitType.Knights, "unit counter cycle");
Check(Balance.ScaleFrom50(28, 20, 50) == 28 && Balance.ScaleFrom50(28, 20, 100) == 20, "productivity price curve");
Check(Balance.Buildings.All(x => x.Key == x.Value.Kind) && Balance.Buildings.Values.Select(x => x.Name).Distinct().Count() == Balance.Buildings.Count, "building definitions valid");
Check(Balance.Equipment.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == Balance.Equipment.Length, "equipment definitions have unique names");
Check(Balance.Courtships.Select(x => x.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count() == Balance.Courtships.Length && Balance.Courtships.All(x => x.Rewards.Select(r => r.Win).Distinct().Count() == x.Rewards.Length), "courtship definitions valid");
var rewardItems = Balance.Courtships.SelectMany(x => x.Rewards).Where(x => x.Item is not null).Select(x => x.Item!).ToHashSet(StringComparer.OrdinalIgnoreCase);
Check(Balance.Victories[VictoryKind.Dragon].RequiredItems.All(rewardItems.Contains), "victory items obtainable from definitions");
Check(Balance.Strategy == new StrategicDefinition(80, 98, 9), "strategic warfare definitions");
Check(Balance.TournamentOpponents.Length == 5 && Balance.TournamentOpponents.All(x => x.Wager is >= 20 and <= 80 && x.Swordsmen + x.Halberdiers + x.Knights == 8), "tournament opponent definitions valid");
Check(ImportedArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count() == ImportedArt.Definitions.Count, "imported art roles are unique definitions");
Check(ImportedAnimations.Definitions.Any(x => x is { Role: "Interface.Cursor", IdSuffix: ":ffmouse.csf" }), "original cursor role is definition driven");
Check(EstatePresentationDefinitions.From(null).Controls.Select(x => x.Action).Distinct().Count() == Enum.GetValues<EstateControlAction>().Length, "estate controls are unique definitions");
Check(EstatePresentationDefinitions.TileAtlases.Count == 3 && EstatePresentationDefinitions.TileFrames.Count == Enum.GetValues<EstateTerrainKind>().Length, "seasonal estate tile atlases are definition driven");
Check(FarmPresentationDefinitions.Commands.Select(x => x.Key).Distinct().Count() == FarmPresentationDefinitions.Commands.Count, "farm commands are unique definitions");
Check(FarmPresentationDefinitions.Layouts.Select(x => x.Section).SequenceEqual(Enum.GetValues<FarmPresentationDefinitions.Section>()), "four original fief-management variants are definition driven");
Check(FarmPresentationDefinitions.Layouts.All(x => ImportedLayouts.Definitions.Any(layout => layout.Role == x.LayoutRole && layout.IdSuffix == x.LayoutSuffix)), "fief-management HAT descriptors are imported");
Check(FarmPresentationDefinitions.Layouts.All(x => x.Okay.Width > 0 && x.Cancel.Width > 0), "fief-management confirmation controls are definition driven");
Check(FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Farm).Count == Enum.GetValues<CropType>().Length, "farm management rows match the executable label table");
Check(Enum.GetValues<FarmPresentationDefinitions.Section>().Select(section => FarmPresentationDefinitions.EntriesFor(section).Count).SequenceEqual([17, 17, 4, 7]), "fief-management label catalogs have recovered section sizes");
Check(BlacksmithPresentationDefinitions.Hotspots.All(x => !string.IsNullOrWhiteSpace(x.HoverLabel)), "confirmed visual scene hotspots define hover labels");
Check(HomePresentationDefinitions.Hotspots.Select(x => x.Action).SequenceEqual([SceneNavigationAction.Overview, SceneNavigationAction.Castle, SceneNavigationAction.Farm, SceneNavigationAction.Village, SceneNavigationAction.Forest, SceneNavigationAction.WarPlanning, SceneNavigationAction.Exit, SceneNavigationAction.Jump, SceneNavigationAction.Map, SceneNavigationAction.Orders]), "executable-ordered Home hotspots are definition driven");
Check(WarPlanningPresentationDefinitions.ArmyFrame(4, false, false) == 14 && WarPlanningPresentationDefinitions.SendSpyUnavailableFrame == 21, "War Planning control frames match the decoded CSF sequence");
Check(CharacterCreationDefinitions.Options.Select(x => x.Action).Distinct().Count() == Enum.GetValues<CharacterCreationAction>().Length, "character option actions are unique definitions");
Check(CharacterCreationDefinitions.PregeneratedCharacters.Count == Balance.Templates.Length && CharacterCreationDefinitions.HeraldicColors.Select(x => x.Name).SequenceEqual(["Red", "Green", "Blue"]), "original character selection hotspots are defined");

string[] syntheticCue =
[
    "FILE \"disc.bin\" BINARY", "  TRACK 01 MODE1/2352", "    INDEX 01 00:00:00",
    "  TRACK 02 AUDIO", "    INDEX 01 00:02:00", "  TRACK 03 AUDIO", "    INDEX 01 00:03:10"
];
var cueTracks = CueSheet.Tracks(syntheticCue);
Check(CueSheet.DataTrackSectors(syntheticCue) == 150 && cueTracks.Length == 3 && cueTracks[2].StartSector == 235, "cue sheet parses data and audio boundaries");
Check(Throws<InvalidDataException>(() => CueSheet.Tracks(["not a cue sheet"])), "invalid cue sheet fails cleanly");

var soundBankFixture = new byte[15];
BinaryPrimitives.WriteUInt32LittleEndian(soundBankFixture, DynamixSoundBankDecoder.Magic);
BinaryPrimitives.WriteUInt32LittleEndian(soundBankFixture.AsSpan(4), 3);
BinaryPrimitives.WriteUInt32LittleEndian(soundBankFixture.AsSpan(8), 11025);
soundBankFixture[12] = 0x7f; soundBankFixture[13] = 0x80; soundBankFixture[14] = 0x81;
var soundBank = DynamixSoundBankDecoder.Decode(soundBankFixture);
Check(soundBank.Samples is [{ SampleRate: 11025, Samples.Length: 3 }], "Dynamix sound bank parses bounded rate-tagged samples");
Check(soundBank.Samples[0].ToPcm16LittleEndian().SequenceEqual(new byte[] { 0, 255, 0, 0, 0, 1 }), "unsigned 8-bit samples convert to signed 16-bit PCM");
Check(Throws<InvalidDataException>(() => DynamixSoundBankDecoder.Decode(soundBankFixture[..^1])), "Dynamix sound bank rejects truncated samples");
Check(Throws<InvalidDataException>(() => LinearExecutableFixupReader.ReadInternalFixups(new byte[128])), "linear executable fixup parser rejects missing headers");

var lzwFixture = PackLsbCodes([65, 66, 257, 259], 9);
Check(Encoding.ASCII.GetString(DynamixCompression.DecodeLzw(lzwFixture, 7)) == "ABABABA", "Dynamix LZW expands dictionary and special code");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeLzw(lzwFixture, 8)), "Dynamix LZW rejects truncated streams cleanly");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeLzw(lzwFixture, 7, 6)), "Dynamix LZW enforces expanded-size limit");
var kind2Fixture = PackMsbCodes([(256, 9), (65, 9), (66, 9), (258, 9), (260, 9), (257, 9)]);
Check(Encoding.ASCII.GetString(DynamixCompression.DecodeKind2(kind2Fixture, 7)) == "ABABABA", "Dynamix kind-2 handles clear, dictionary, special, and end codes");
(int Count, int Width)[] kind2WidthRuns = [(254, 9), (512, 10), (1_024, 11), (2_048, 12), (4_096, 13), (8_193, 14)];
var kind2GrowthCodes = new List<(int Code, int Width)> { (256, 9) };
foreach (var (count, width) in kind2WidthRuns) kind2GrowthCodes.AddRange(Enumerable.Repeat((0, width), count));
kind2GrowthCodes.Add((257, 14));
Check(DynamixCompression.DecodeKind2(PackMsbCodes(kind2GrowthCodes), kind2WidthRuns.Sum(run => run.Count)).SequenceEqual(new byte[kind2WidthRuns.Sum(run => run.Count)]), "Dynamix kind-2 grows from nine through fourteen bits at executable-confirmed boundaries");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(kind2Fixture[..^1], 7)), "Dynamix kind-2 rejects a stream without its end code");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(PackMsbCodes([(256, 9), (300, 9), (257, 9)]), 1)), "Dynamix kind-2 rejects undefined dictionary codes");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeKind2(kind2Fixture, 7, 6)), "Dynamix kind-2 enforces expanded-size limit");
var kind1Source = new byte[] { 3, 0, 0x80, 2, 3, 2, 0, 0x40, 5 };
var kind1Blocks = DynamixCompression.ReadKind1Blocks(kind1Source);
Check(kind1Blocks.SequenceEqual([new DynamixCompressedBlock(0, 2, 3, DynamixBlockStorage.Stored), new DynamixCompressedBlock(1, 7, 2, DynamixBlockStorage.Compressed)]), "Dynamix kind-1 block framing and storage markers");
Check(DynamixCompression.DecodeKind1Block(kind1Source, kind1Blocks[0], 2).SequenceEqual(new byte[] { 2, 3 }), "Dynamix stored kind-1 block decodes verbatim");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeKind1Block(kind1Source, kind1Blocks[1], 1)), "Dynamix compressed block rejects a truncated header");
Check(Throws<InvalidDataException>(() => DynamixCompression.DecodeKind1Block(kind1Source, kind1Blocks[0], 1)), "Dynamix stored block requires its exact output slice length");
var compressedKind1 = new byte[] { 13, 0, 0x40, 0x00, 0x18, 0x00, (byte)'A', (byte)'B', (byte)'C', 0, 0x30, 0, 0, 0, (byte)'Z' };
Check(Encoding.ASCII.GetString(DynamixCompression.DecodeKind1(compressedKind1, 22)) == "ABCABC" + new string('Z', 16), "Dynamix kind-1 literal, dictionary-copy, and run tokens decode");
Check(DynamixCompression.ExpectedKind1BlockCount(32_769) == 3 && DynamixCompression.ExpectedKind1BlockSize(32_769, 2) == 1, "Dynamix kind-1 output divides into 16 KiB slices");
Check(Throws<InvalidDataException>(() => DynamixCompression.ReadKind1Blocks([4, 0, 1, 2])), "Dynamix kind-1 framing rejects truncated blocks");
Check(Throws<InvalidDataException>(() => DynamixCompression.ReadKind1Blocks([1, 0, 0x20])), "Dynamix kind-1 framing rejects unknown block markers");
var pcx = PcxDecoder.Decode(CreateSyntheticPcx());
Check(pcx.Width == 3 && pcx.Height == 1 && pcx.Indices.SequenceEqual(new byte[] { 1, 1, 2 }), "indexed PCX dimensions, RLE, and row padding decode");
Check(pcx.ToRgba().SequenceEqual(new byte[] { 10, 20, 30, 255, 10, 20, 30, 255, 40, 50, 60, 255 }), "indexed PCX palette expands to RGBA");
Check(Throws<InvalidDataException>(() => new PcxImage(2, 2, [1], new byte[768]).ToRgba()), "indexed PCX rejects inconsistent decoded buffers");
var brokenPcx = CreateSyntheticPcx(); brokenPcx[^769] = 0;
Check(Throws<InvalidDataException>(() => PcxDecoder.Decode(brokenPcx)), "indexed PCX rejects a missing palette marker");
var csf = new CsfSequence(CreateSyntheticCsf());
Check(csf.Chunks.SequenceEqual([new CsfChunk(0, 14, 3), new CsfChunk(1, 17, 2)]) && csf.ReadChunk(csf.Chunks[1]).SequenceEqual(new byte[] { 4, 5 }), "CSF chunk table and payload boundaries decode");
var dimensionCsf = new CsfSequence(CreateSyntheticCsf([80, 0, 90, 0]));
Check(dimensionCsf.ReadDimensionHeader(dimensionCsf.Chunks[0]) == new CsfDimensionHeader(80, 90), "CSF dimension header reads bounded little-endian values");
var frameCsf = new CsfSequence(CreateSyntheticCsf(CreateSyntheticCsfFrame()));
var csfFrame = frameCsf.DecodeFrame(frameCsf.Chunks[0]);
Check(csfFrame is { Width: 5, Height: 2 } && csfFrame.Indices.SequenceEqual(new byte[] { 0, 7, 8, 9, 9, 0, 0, 0, 0, 0 }), "CSF scanline skip, literal, and fill operations decode");
Check(csfFrame.Alpha.SequenceEqual(new byte[] { 0, 255, 255, 255, 255, 0, 0, 0, 0, 0 }), "CSF skipped pixels remain transparent");
Check(Throws<InvalidDataException>(() => new CsfFrame(2, 2, [1], [255], 0, 0, 0).ToRgba(new byte[768])), "CSF RGBA conversion rejects inconsistent decoded buffers");
Check(Throws<InvalidDataException>(() => frameCsf.DecodeFrame(frameCsf.Chunks[0], 9)), "CSF decoder enforces pixel limit");
var malformedFrameCsf = new CsfSequence(CreateSyntheticCsf([1, 0, 1, 0, 1, 1, 2, 0]));
Check(Throws<InvalidDataException>(() => malformedFrameCsf.DecodeFrame(malformedFrameCsf.Chunks[0])), "CSF decoder rejects scanlines beyond declared width");
var brokenCsf = CreateSyntheticCsf(); BinaryPrimitives.WriteUInt32LittleEndian(brokenCsf.AsSpan(10, 4), 20);
Check(Throws<InvalidDataException>(() => new CsfSequence(brokenCsf)), "CSF rejects chunks outside the resource");
var palette = IndexedPaletteDecoder.Decode(Enumerable.Range(0, IndexedPalette.ByteSize).Select(x => (byte)x).ToArray());
Check(palette.Rgb.Length == 768 && palette.Rgb[767] == 255, "indexed RGB palette decodes 256 colors");
Check(Throws<InvalidDataException>(() => IndexedPaletteDecoder.Decode(new byte[767])), "indexed RGB palette requires exact length");

var cddaBytes = Enumerable.Range(0, CddaWave.BytesPerSector * 2).Select(x => (byte)(x % 251)).ToArray();
using (var cddaSource = new MemoryStream(cddaBytes))
using (var wave = new MemoryStream())
{
    CddaWave.Write(cddaSource, wave, 1, 1);
    var bytes = wave.ToArray();
    Check(bytes.Length == CddaWave.BytesPerSector + 44 && Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" && Encoding.ASCII.GetString(bytes, 8, 4) == "WAVE", "CDDA writer creates PCM RIFF header");
    Check(bytes[44] == cddaBytes[CddaWave.BytesPerSector] && BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(40, 4)) == CddaWave.BytesPerSector, "CDDA writer preserves exact sector samples");
}

var isoPath = Path.Combine(Path.GetTempPath(), $"conqueror-iso-{Guid.NewGuid():N}.bin");
try
{
    File.WriteAllBytes(isoPath, CreateSyntheticRawIso());
    using var rawImage = new RawMode1Image(isoPath, 20);
    var syntheticIso = new Iso9660(rawImage);
    var textFile = syntheticIso.Files.Single();
    Check(textFile.Path == "TEST.TXT" && Encoding.ASCII.GetString(syntheticIso.ReadFile(textFile)) == "DATA", "synthetic raw ISO is traversed and read");
    Check(Throws<EndOfStreamException>(() => rawImage.Read(20L * 2048, 1)), "raw image rejects reads past data track");
}
finally
{
    if (File.Exists(isoPath)) File.Delete(isoPath);
}

var resPath = Path.Combine(Path.GetTempPath(), $"conqueror-res-{Guid.NewGuid():N}.res");
try
{
    File.WriteAllBytes(resPath, CreateSyntheticDynamixArchive());
    var archive = new DynamixArchive(resPath);
    var entry = archive.Entries.Single();
    Check(entry.Name == "Greeting" && entry.IsStored && entry.Offset == 8 && Encoding.ASCII.GetString(archive.ReadDecoded(entry)) == "HELLO", "Dynamix archive directory and stored entry decode");
    var corrupt = File.ReadAllBytes(resPath);
    BinaryPrimitives.WriteUInt32LittleEndian(corrupt.AsSpan(13 + 4 + 48, 4), uint.MaxValue);
    File.WriteAllBytes(resPath, corrupt);
    Check(Throws<InvalidDataException>(() => new DynamixArchive(resPath)), "Dynamix archive rejects out-of-bounds entry");
    var memoryArchive = new DynamixArchive(CreateSyntheticDynamixArchive(), "synthetic.res");
    Check(memoryArchive.SourceName == "synthetic.res" && Encoding.ASCII.GetString(memoryArchive.ReadStored(memoryArchive.Entries[0])) == "HELLO", "Dynamix archive parses in-memory disc resources");
}
finally
{
    if (File.Exists(resPath)) File.Delete(resPath);
}

var campaign = new Campaign(Campaign.NewFromTemplate(1));
Check(campaign.State.Player.Wealth == 490, "Ronald starting wealth");
Check(campaign.Borrow(200) && campaign.State.Player.Debt == 300, "50 percent loan interest");
Check(!campaign.Borrow(1), "one outstanding loan only");

var fief = campaign.State.Player.Home;
campaign.Build("Steward"); campaign.Build("Beadle"); campaign.Build("Priest"); campaign.Build("Monastery"); campaign.Build("Woodward");
Check(fief.Productivity() == 90, "staff productivity order");
campaign.Build("Church");
Check(fief.Productivity() == 100, "productivity capped at 100");
Check(!campaign.Build(BuildingKind.Church) && campaign.Build(BuildingKind.House), "generic building interpreter");

var custom = Campaign.NewCustom("Test", 42);
Check(custom.Player.Stats.Strength is >= 2 and <= 12 && custom.Player.Stats.Intelligence is >= 2 and <= 12
    && custom.Player.Age == 12 && custom.Player.Wealth == 240, "custom character bounds");
var youth = new Campaign(custom);
var firstDilemmaNumber = youth.CurrentYouthDilemmaNumber;
Check(firstDilemmaNumber is >= 0 and <= 4 && youth.CurrentYouthDilemmaNumber == firstDilemmaNumber, "youth dilemma selection is stable within the age group");
var beforeYouth = youth.State.Player.Stats;
Check(youth.AnswerDilemma(1) && youth.State.Player.Wealth == 260 && youth.State.Player.Stats.Piety == Math.Max(0, beforeYouth.Piety - 1), "youth dilemma effects");
Check(youth.State.Player.Age == 13 && youth.CurrentYouthDilemmaNumber is >= 5 and <= 9, "youth dilemma selection advances by five-definition age groups");
for (var i = 1; i < Youth.Dilemmas.Length; i++) youth.AnswerDilemma(2);
Check(youth.State.YouthDilemmasAnswered == 6 && !youth.AnswerDilemma(0), "six dilemma limit");
Check(Balance.Equipment.Single(x => x.Name == "Full Plate").Armor == 35, "full plate protection");
Check(Balance.Equipment.Single(x => x.Name == "Kingslayer Sword").BuyPrice == 4000, "Kingslayer price");

var shop = new Campaign(Campaign.NewFromTemplate(1));
Check(shop.BuyEquipment("Spiked Mace") && shop.State.Player.Inventory.Weapon == "Spiked Mace", "blacksmith buy and equip");
Check(shop.SellEquipment("Spiked Mace") && shop.State.Player.Wealth == 490 - 90 + 67, "75 percent resale truncation");
shop.State.Player.Army.Units[UnitType.Knights] = 8;
var retreatLosses = shop.Retreat();
Check(retreatLosses is >= 4 and <= 6, "retreat loses half or more");

var traveler = new Campaign(Campaign.NewFromTemplate(1));
var york = Array.FindIndex(World.Locations, x => x.Name == "York");
var travelDays = traveler.TravelTo(york);
Check(travelDays == World.TravelDays(0, york) && traveler.State.Date == new DateTime(1086, 3, 1).AddDays(travelDays), "travel advances calendar");
traveler.State.Player.Army.Units[UnitType.Halberdiers] = 1;
traveler.State.Player.SetArmyFieldState(0, true, york);
Check(traveler.StartSiege(york), "siege requires arrival and army");
traveler.WinSiege();
Check(traveler.State.ConqueredLocations.Contains(york) && traveler.State.Player.Fiefs == 2, "location conquest persists");

var warPlanner = new Campaign(Campaign.NewFromTemplate(1));
var warCheckpoint = new WarPlanningCheckpoint(warPlanner);
Check(warPlanner.AdjustArmyCompany(3, UnitType.Swordsmen, 1) && warPlanner.State.Player.ArmyAt(3).Total == 100,
    "war planning raises hundred-serf companies in any of five divisions");
Check(warPlanner.FieldArmy(3) && warPlanner.ToggleArmyMembership(3), "war planning fields and joins a selected division");
warCheckpoint.Restore(warPlanner);
Check(warPlanner.State.Player.ArmyAt(3).Total == 0 && warPlanner.State.Player.JoinedArmyIndex == 0,
    "war planning cancellation restores every division and membership");

var spyCampaign = new Campaign(Campaign.NewFromTemplate(1));
Check(!spyCampaign.HasGarrisonIntel(york) && spyCampaign.SendSpy(york), "spy reveals a hostile garrison");
Check(spyCampaign.State.Player.Wealth == 410 && spyCampaign.HasGarrisonIntel(york) && spyCampaign.GarrisonAt(york) == World.Locations[york].Garrison, "spy report uses persistent garrison data");
Check(!spyCampaign.SendSpy(york) && spyCampaign.State.Player.Wealth == 410, "known garrison is not charged twice");

var invader = new Campaign(Campaign.NewFromTemplate(2), 1086);
foreach (var type in Enum.GetValues<UnitType>()) invader.State.Player.ArmyAt(3).Units[type] = 40;
Check(invader.FieldArmy(3) && invader.ToggleArmyMembership(3), "selected division can accompany the player");
invader.TravelTo(york);
Check(invader.HasPendingFieldBattle && invader.State.PendingFriendlyArmyIndex == 3
    && invader.State.Player.ArmyLocationAt(3) == york && !invader.StartSiege(york),
    "hostile approach intercepts the accompanying division before siege");
var strategicSave = Path.Combine(Path.GetTempPath(), $"conqueror-strategy-{Guid.NewGuid():N}.json");
invader.Save(strategicSave);
var loadedInvasion = Campaign.Load(strategicSave);
File.Delete(strategicSave);
Check(loadedInvasion.HasPendingFieldBattle && loadedInvasion.State.PendingFriendlyArmyIndex == 3
    && loadedInvasion.GarrisonAt(york) == World.Locations[york].Garrison,
    "save preserves the selected division in an interrupted invasion");
var invasion = invader.CreateFieldBattle();
invasion.IssueAll(UnitOrder.Captains);
for (var i = 0; i < 300 && invasion.Outcome == FieldBattleOutcome.InProgress; i++) invasion.Tick();
Check(invader.FinishFieldBattle(invasion) == FieldBattleOutcome.Victory && invader.GarrisonAt(york) == 0
    && invader.State.Player.ArmyAt(3).Total > 0 && invader.State.Player.Army.Total == 0,
    "field victory updates the selected division and clears the persistent garrison");
Check(invader.StartSiege(york), "cleared road permits castle assault");

var withdrawingInvader = new Campaign(Campaign.NewFromTemplate(2), 1086);
foreach (var type in Enum.GetValues<UnitType>()) withdrawingInvader.State.Player.Army.Units[type] = 10;
withdrawingInvader.TravelTo(york);
var withdrawal = withdrawingInvader.CreateFieldBattle();
withdrawal.IssueAll(UnitOrder.Withdraw);
for (var i = 0; i < 10 && withdrawal.Outcome == FieldBattleOutcome.InProgress; i++) withdrawal.Tick();
withdrawingInvader.FinishFieldBattle(withdrawal);
Check(withdrawingInvader.State.CurrentLocation == 0 && !withdrawingInvader.HasPendingFieldBattle, "withdrawal falls back along travel route");
Check(withdrawingInvader.GarrisonAt(york) <= World.Locations[york].Garrison, "enemy field attrition persists after withdrawal");

var tournament = new Campaign(Campaign.NewFromTemplate(1));
tournament.State.CurrentLocation = World.TournamentIndex(tournament.State.Date);
Check(tournament.IsTournamentHere, "monthly tournament location");
var wealthBeforeJoust = tournament.State.Player.Wealth;
Check(tournament.Joust(0, 0) && tournament.State.Player.Wealth == wealthBeforeJoust + 20 && tournament.State.Player.TournamentWinnings == 20, "winning joust settles selected wager");
for (var i = 0; i < 3; i++) tournament.Joust(0);
Check(tournament.State.JoustsThisTournament == 3, "three jousts per tournament");
Check(tournament.TournamentSkirmish() is not null && tournament.TournamentSkirmish() is null, "one skirmish per tournament");

var romance = new Campaign(Campaign.NewFromTemplate(2));
for (var win = 0; win < 5; win++)
{
    romance.State.Date = new DateTime(1086, 3, 1).AddMonths(win);
    romance.State.CurrentLocation = World.TournamentIndex(romance.State.Date);
    Check(romance.RequestColors("Jane") && romance.Joust(0), $"Jane courtship win {win + 1}");
}
Check(romance.State.Player.Inventory.Items.Contains("Dragon Slaying Lance"), "courtship reward ladder interpreted");

var dragonCampaign = new Campaign(Campaign.NewFromTemplate(2));
dragonCampaign.State.Player.Inventory.Items.Add("Dragon Slaying Lance");
dragonCampaign.State.Player.Inventory.Items.Add("Dragon Slaying Armor");
dragonCampaign.State.Player.Inventory.Items.Add("Shield of St. George");
Check(!dragonCampaign.AttemptDragon(), "dragon challenge requires the moor");
dragonCampaign.State.CurrentLocation = World.Locations.Length - 1;
Check(dragonCampaign.AttemptDragon() && dragonCampaign.State.Victory == VictoryKind.Dragon, "dragon victory route");

var crownCampaign = new Campaign(Campaign.NewFromTemplate(2));
crownCampaign.State.Player.Army.Units[UnitType.Knights] = 1;
Check(!crownCampaign.AttemptCrown(), "crown challenge requires London");
crownCampaign.State.CurrentLocation = 1;
Check(crownCampaign.AttemptCrown() && crownCampaign.State.Victory == VictoryKind.Crown, "crown victory route");

var siegePlayer = Campaign.NewFromTemplate(2).Player;
siegePlayer.Army.Units[UnitType.Halberdiers] = 20;
foreach (var name in new[] { "Full Plate", "Heraldic Shield", "Great War Helm" })
{
    var item = Balance.Equipment.Single(x => x.Name == name); siegePlayer.Inventory.Items.Add(name); siegePlayer.Inventory.Equip(item);
}
var siege = new SiegeSession(siegePlayer, 12, 99);
Check(siege.Enemies.Count == SiegeSession.Rules.BaseEnemies + 4 && siege.AlliesStarted == 3, "siege generated from definitions");
Check(siege.ArmorRating() == 65, "equipped armor plus gambeson protection");
Check(siege.TileAt(4, 5) == SiegeTile.Door && siege.TileAt(8, 7) == SiegeTile.SecretDoor, "doors and secret rooms generated");
Check(siege.Shoot() == SiegeAction.NoAmmunition, "crossbow requires weapon and bolts");
Check(siege.Move(true) != SiegeAction.Blocked && siege.Move(true) != SiegeAction.Blocked && siege.Move(true) == SiegeAction.Blocked, "walls block first-person movement");

var fieldFriendly = new Army(); var fieldEnemy = new Army();
fieldFriendly.Units[UnitType.Swordsmen] = 8; fieldFriendly.Units[UnitType.Halberdiers] = 8; fieldFriendly.Units[UnitType.Knights] = 8;
fieldEnemy.Units[UnitType.Swordsmen] = 4; fieldEnemy.Units[UnitType.Halberdiers] = 4; fieldEnemy.Units[UnitType.Knights] = 4;
var field = new FieldBattleSession(fieldFriendly, fieldEnemy, 1086);
field.Issue(UnitType.Swordsmen, UnitOrder.FlankLeft); field.Tick();
Check(field.Friendly.Single(x => x.Type == UnitType.Swordsmen).Y == 1, "field formation obeys flank order");
field.IssueAll(UnitOrder.Captains);
for (var i = 0; i < 200 && field.Outcome == FieldBattleOutcome.InProgress; i++) field.Tick();
Check(field.Outcome == FieldBattleOutcome.Victory && field.FriendlySurvivors().Total > 0, $"captains resolve tactical battle (outcome {field.Outcome}, friendly {field.FriendlySurvivors().Total}, enemy {field.EnemySurvivors().Total})");

var withdrawing = new FieldBattleSession(fieldFriendly, fieldEnemy, 42);
withdrawing.IssueAll(UnitOrder.Withdraw);
for (var i = 0; i < 5 && withdrawing.Outcome == FieldBattleOutcome.InProgress; i++) withdrawing.Tick();
Check(withdrawing.Outcome == FieldBattleOutcome.Withdrawn, "formations withdraw from battlefield edge");

var contentRoot = Path.Combine(Path.GetTempPath(), $"conqueror-content-{Guid.NewGuid():N}");
var oldContentRoot = Environment.GetEnvironmentVariable("CONQUEROR_USER_CONTENT");
try
{
    Directory.CreateDirectory(Path.Combine(contentRoot, "Audio"));
    File.WriteAllBytes(Path.Combine(contentRoot, "Audio", "track02.wav"), [1, 2, 3, 4]);
    File.WriteAllBytes(Path.Combine(contentRoot, "portrait.pcc"), CreateSyntheticPcx());
    File.WriteAllBytes(Path.Combine(contentRoot, "animation.csf"), CreateSyntheticCsf(CreateSyntheticCsfFrame()));
    File.WriteAllBytes(Path.Combine(contentRoot, "screen.pal"), new byte[IndexedPalette.ByteSize]);
    File.WriteAllBytes(Path.Combine(contentRoot, "interface.666"), soundBankFixture);
    File.WriteAllText(Path.Combine(contentRoot, "dilem7.dat"), CreateSyntheticDilemma());
    var manifest = new
    {
        Version = 1,
        SourceImageSha256 = "test-source",
        Assets = new[]
        {
            new { Id = "CDDA/TRACK02", Path = "Audio/track02.wav", Kind = "audio", Size = 4, Sha256 = "test" },
            new { Id = "IMAGE", Path = "portrait.pcc", Kind = "image", Size = CreateSyntheticPcx().Length, Sha256 = "test" },
            new { Id = "ANIMATION", Path = "animation.csf", Kind = "indexed-animation", Size = 0, Sha256 = "test" },
            new { Id = "PALETTE", Path = "screen.pal", Kind = "palette", Size = IndexedPalette.ByteSize, Sha256 = "test" },
            new { Id = "SOUND", Path = "interface.666", Kind = "sound-bank", Size = soundBankFixture.Length, Sha256 = "test" },
            new { Id = "C1086.GOB#177:dilem7.dat", Path = "dilem7.dat", Kind = "resource", Size = 0, Sha256 = "test" },
            new { Id = "UNSAFE", Path = "../outside.bin", Kind = "resource", Size = 0, Sha256 = "test" }
        }
    };
    File.WriteAllText(Path.Combine(contentRoot, "manifest.json"), JsonSerializer.Serialize(manifest));
    Environment.SetEnvironmentVariable("CONQUEROR_USER_CONTENT", contentRoot);
    var catalog = ImportedContentCatalog.Discover();
    using var importedTrack = catalog?.Open("CDDA/TRACK02");
    Check(catalog?.Count == 7 && importedTrack?.Length == 4 && catalog.Ids("audio").SequenceEqual(["CDDA/TRACK02"]), "imported content manifest is discoverable");
    Check(catalog?.FindId("image", "aGe") == "IMAGE" && catalog.FindId("audio", "aGe") is null, "imported content finds role candidates by kind and suffix");
    Check(catalog?.DecodePcx("IMAGE") is { Width: 3, Height: 1 }, "runtime catalog decodes imported PCX-compatible images");
    var importedSequence = catalog?.DecodeCsf("ANIMATION");
    Check(importedSequence?.DecodeFrame(importedSequence.Chunks[0]) is { Width: 5, Height: 2 }, "runtime catalog decodes imported CSF frame sequences");
    Check(catalog?.DecodePalette("PALETTE")?.Rgb.Length == IndexedPalette.ByteSize, "runtime catalog decodes imported RGB palettes");
    Check(catalog?.DecodeSoundBank("SOUND")?.Samples is [{ SampleRate: 11025, Samples.Length: 3 }], "runtime catalog decodes imported sound banks");
    var dialogue = catalog is null ? null : new ImportedDialogueRepository(catalog).GetDilemma(7);
    Check(dialogue is { Number: 7, Age: 12, Choices.Count: 3 }, "runtime dialogue repository loads a dilemma by stable number");
    Check(catalog is not null && new ImportedDialogueRepository(catalog).GetDilemmasForAge(12).Select(x => x.Number).SequenceEqual([7]), "runtime dialogue repository groups definitions by declared age");
    Check(catalog?.Open("UNSAFE") is null, "imported content rejects paths outside its root");
}
finally
{
    Environment.SetEnvironmentVariable("CONQUEROR_USER_CONTENT", oldContentRoot);
    if (Directory.Exists(contentRoot)) Directory.Delete(contentRoot, true);
}

}
catch (Exception error)
{
    Console.Error.WriteLine($"FAIL  unexpected {error.GetType().Name}: {error.Message}");
    failed++;
}

Console.WriteLine();
Console.WriteLine($"Result: {passed} passed, {failed} failed, {passed + failed} total.");
Environment.ExitCode = failed == 0 ? 0 : 1;

static string CreateSyntheticDilemma()
{
    var text = new StringBuilder("# AGE: 12\r\n# TITLE: SYNTHETIC\r\n!DILEMMA_NUMBER DILEMMA_SFG_FILE\r\n7 D777.CSF\r\n&DILEMMA TEXT\r\n^Synthetic prompt.\r\n");
    var attributes = new[] { "STRENGTH", "DEXTERITY", "NONE" };
    foreach (var choice in Enumerable.Range(1, 3))
    {
        text.Append("@RELEVANT SCORING ATTRIBUTE\r\n~").Append(attributes[choice - 1]).Append("\r\n");
        text.Append("%HIGH SCORING BREAKPOINT LOW SCORING BREAKPOINT\r\n17 6\r\n");
        foreach (var outcome in Enum.GetNames<DilemmaOutcome>())
        {
            text.Append("?DILEMMA CHOICE ").Append(choice).Append(' ').Append(outcome.ToUpperInvariant()).Append(" TEXT\r\n");
            text.Append("^Synthetic outcome.\r\n*NUMBER OF ATTRIBUTES MODIFIED\r\n1\r\n$ATTRIBUTE MODIFIER\r\nHONOR 1\r\n");
        }
    }
    return text.Append('\u001a').ToString();
}

static bool Throws<T>(Action action) where T : Exception
{
    try { action(); return false; }
    catch (T) { return true; }
}

static byte[] CreateSyntheticRawIso()
{
    const int rawSector = 2352;
    const int payloadOffset = 16;
    const int payloadSize = 2048;
    var raw = new byte[20 * rawSector];
    var pvd = new byte[payloadSize];
    pvd[0] = 1; Encoding.ASCII.GetBytes("CD001").CopyTo(pvd, 1); pvd[6] = 1;
    WriteIsoRecord(pvd, 156, 17, payloadSize, 2, [0]);
    CopyPayload(raw, 16, pvd);

    var directory = new byte[payloadSize];
    var offset = WriteIsoRecord(directory, 0, 17, payloadSize, 2, [0]);
    offset += WriteIsoRecord(directory, offset, 17, payloadSize, 2, [1]);
    WriteIsoRecord(directory, offset, 18, 4, 0, Encoding.ASCII.GetBytes("TEST.TXT;1"));
    CopyPayload(raw, 17, directory);
    CopyPayload(raw, 18, Encoding.ASCII.GetBytes("DATA"));
    return raw;

    static void CopyPayload(byte[] target, int sector, byte[] payload) => Buffer.BlockCopy(payload, 0, target, sector * rawSector + payloadOffset, payload.Length);
}

static int WriteIsoRecord(byte[] target, int offset, uint extent, int size, byte flags, byte[] name)
{
    var length = 33 + name.Length + (name.Length % 2 == 0 ? 1 : 0);
    target[offset] = (byte)length;
    BinaryPrimitives.WriteUInt32LittleEndian(target.AsSpan(offset + 2, 4), extent);
    BinaryPrimitives.WriteUInt32BigEndian(target.AsSpan(offset + 6, 4), extent);
    BinaryPrimitives.WriteUInt32LittleEndian(target.AsSpan(offset + 10, 4), (uint)size);
    BinaryPrimitives.WriteUInt32BigEndian(target.AsSpan(offset + 14, 4), (uint)size);
    target[offset + 25] = flags;
    BinaryPrimitives.WriteUInt16LittleEndian(target.AsSpan(offset + 28, 2), 1);
    BinaryPrimitives.WriteUInt16BigEndian(target.AsSpan(offset + 30, 2), 1);
    target[offset + 32] = (byte)name.Length;
    name.CopyTo(target, offset + 33);
    return length;
}

static byte[] CreateSyntheticDynamixArchive()
{
    const int directoryOffset = 13;
    var bytes = new byte[directoryOffset + 4 + 52];
    ".RES"u8.CopyTo(bytes);
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4, 4), directoryOffset);
    "HELLO"u8.CopyTo(bytes.AsSpan(8, 5));
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(directoryOffset, 4), 1);
    var record = bytes.AsSpan(directoryOffset + 4, 52);
    "Greeting"u8.CopyTo(record);
    BinaryPrimitives.WriteUInt32LittleEndian(record.Slice(40, 4), 5);
    BinaryPrimitives.WriteUInt32LittleEndian(record.Slice(44, 4), 5);
    BinaryPrimitives.WriteUInt32LittleEndian(record.Slice(48, 4), 8);
    return bytes;
}

static byte[] PackLsbCodes(int[] codes, int width)
{
    var result = new byte[(codes.Length * width + 7) / 8];
    var bitPosition = 0;
    foreach (var code in codes)
    {
        for (var bit = 0; bit < width; bit++, bitPosition++)
            if ((code & (1 << bit)) != 0) result[bitPosition >> 3] |= (byte)(1 << (bitPosition & 7));
    }
    return result;
}

static byte[] PackMsbCodes(IEnumerable<(int Code, int Width)> codes)
{
    var values = codes.ToArray();
    var result = new byte[(values.Sum(value => value.Width) + 7) / 8];
    var bitPosition = 0;
    foreach (var (code, width) in values)
        for (var bit = width - 1; bit >= 0; bit--, bitPosition++)
            if ((code & (1 << bit)) != 0) result[bitPosition >> 3] |= (byte)(1 << (7 - (bitPosition & 7)));
    return result;
}

static byte[] CreateSyntheticPcx()
{
    var bytes = new byte[128 + 4 + 1 + 768];
    bytes[0] = 0x0A; bytes[1] = 5; bytes[2] = 1; bytes[3] = 8;
    BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(8, 2), 2);
    bytes[65] = 1;
    BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(66, 2), 4);
    bytes[128] = 0xC2; bytes[129] = 1; bytes[130] = 2; bytes[131] = 0;
    bytes[132] = 0x0C;
    bytes[132 + 3 + 1] = 10; bytes[132 + 3 + 2] = 20; bytes[132 + 3 + 3] = 30;
    bytes[132 + 6 + 1] = 40; bytes[132 + 6 + 2] = 50; bytes[132 + 6 + 3] = 60;
    return bytes;
}

static byte[] CreateSyntheticCsf(byte[]? firstChunk = null)
{
    firstChunk ??= [1, 2, 3];
    var bytes = new byte[14 + firstChunk.Length + 2];
    BinaryPrimitives.WriteUInt16LittleEndian(bytes, CsfSequence.ObservedMagic);
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(2), 2);
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(6), (uint)firstChunk.Length);
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(10), 2);
    firstChunk.CopyTo(bytes, 14);
    new byte[] { 4, 5 }.CopyTo(bytes, 14 + firstChunk.Length);
    return bytes;
}

static byte[] CreateSyntheticCsfFrame() =>
[
    5, 0, 2, 0,
    3,
    1, 1, 0,
    0, 2, 0, 7, 8,
    2, 2, 0, 9,
    1,
    1, 5, 0
];
