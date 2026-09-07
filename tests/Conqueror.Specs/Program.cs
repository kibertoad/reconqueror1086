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

string[] syntheticCue =
[
    "FILE \"disc.bin\" BINARY", "  TRACK 01 MODE1/2352", "    INDEX 01 00:00:00",
    "  TRACK 02 AUDIO", "    INDEX 01 00:02:00", "  TRACK 03 AUDIO", "    INDEX 01 00:03:10"
];
var cueTracks = CueSheet.Tracks(syntheticCue);
Check(CueSheet.DataTrackSectors(syntheticCue) == 150 && cueTracks.Length == 3 && cueTracks[2].StartSector == 235, "cue sheet parses data and audio boundaries");
Check(Throws<InvalidDataException>(() => CueSheet.Tracks(["not a cue sheet"])), "invalid cue sheet fails cleanly");

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
Check(custom.Player.Stats.Strength is >= 2 and <= 12 && custom.Player.Wealth == 240, "custom character bounds");
var youth = new Campaign(custom);
var beforeYouth = youth.State.Player.Stats;
Check(youth.AnswerDilemma(1) && youth.State.Player.Wealth == 260 && youth.State.Player.Stats.Piety == Math.Max(0, beforeYouth.Piety - 1), "youth dilemma effects");
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
Check(traveler.StartSiege(york), "siege requires arrival and army");
traveler.WinSiege();
Check(traveler.State.ConqueredLocations.Contains(york) && traveler.State.Player.Fiefs == 2, "location conquest persists");

var spyCampaign = new Campaign(Campaign.NewFromTemplate(1));
Check(!spyCampaign.HasGarrisonIntel(york) && spyCampaign.SendSpy(york), "spy reveals a hostile garrison");
Check(spyCampaign.State.Player.Wealth == 410 && spyCampaign.HasGarrisonIntel(york) && spyCampaign.GarrisonAt(york) == World.Locations[york].Garrison, "spy report uses persistent garrison data");
Check(!spyCampaign.SendSpy(york) && spyCampaign.State.Player.Wealth == 410, "known garrison is not charged twice");

var invader = new Campaign(Campaign.NewFromTemplate(2), 1086);
foreach (var type in Enum.GetValues<UnitType>()) invader.State.Player.Army.Units[type] = 40;
invader.TravelTo(york);
Check(invader.HasPendingFieldBattle && !invader.StartSiege(york), "hostile approach intercepts army before siege");
var strategicSave = Path.Combine(Path.GetTempPath(), $"conqueror-strategy-{Guid.NewGuid():N}.json");
invader.Save(strategicSave);
var loadedInvasion = Campaign.Load(strategicSave);
File.Delete(strategicSave);
Check(loadedInvasion.HasPendingFieldBattle && loadedInvasion.GarrisonAt(york) == World.Locations[york].Garrison, "save preserves interrupted invasion state");
var invasion = invader.CreateFieldBattle();
invasion.IssueAll(UnitOrder.Captains);
for (var i = 0; i < 300 && invasion.Outcome == FieldBattleOutcome.InProgress; i++) invasion.Tick();
Check(invader.FinishFieldBattle(invasion) == FieldBattleOutcome.Victory && invader.GarrisonAt(york) == 0, "field victory clears persistent garrison");
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
    var manifest = new
    {
        Version = 1,
        SourceImageSha256 = "test-source",
        Assets = new[]
        {
            new { Id = "CDDA/TRACK02", Path = "Audio/track02.wav", Kind = "audio", Size = 4, Sha256 = "test" },
            new { Id = "UNSAFE", Path = "../outside.bin", Kind = "resource", Size = 0, Sha256 = "test" }
        }
    };
    File.WriteAllText(Path.Combine(contentRoot, "manifest.json"), JsonSerializer.Serialize(manifest));
    Environment.SetEnvironmentVariable("CONQUEROR_USER_CONTENT", contentRoot);
    var catalog = ImportedContentCatalog.Discover();
    using var importedTrack = catalog?.Open("CDDA/TRACK02");
    Check(catalog?.Count == 2 && importedTrack?.Length == 4 && catalog.Ids("audio").SequenceEqual(["CDDA/TRACK02"]), "imported content manifest is discoverable");
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
