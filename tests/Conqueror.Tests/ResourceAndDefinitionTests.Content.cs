using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed partial class ResourceAndDefinitionTests
{
    [Fact]
    public void ControllerBindingsProvideEdgeTriggeredNavigationAndContextActions()
    {
        var released = default(GamePadState);
        var accept = new GamePadState(Vector2.Zero, Vector2.Zero, 0, 0, Buttons.A);
        Assert.True(ControllerInputBindings.IsPressed(Keys.Enter, ControllerInputContext.General, accept, released));
        Assert.False(ControllerInputBindings.IsPressed(Keys.Enter, ControllerInputContext.General, accept, accept));

        Assert.Equal([Buttons.DPadUp, Buttons.LeftThumbstickUp],
            ControllerInputBindings.ButtonsFor(Keys.Up, ControllerInputContext.General));
        Assert.Contains(Buttons.X, ControllerInputBindings.ButtonsFor(Keys.F, ControllerInputContext.Home));
        Assert.Empty(ControllerInputBindings.ButtonsFor(Keys.F, ControllerInputContext.General));
        Assert.Contains(Buttons.RightShoulder,
            ControllerInputBindings.ButtonsFor(Keys.P, ControllerInputContext.Map));
        Assert.Contains(Buttons.RightShoulder,
            ControllerInputBindings.ButtonsFor(Keys.D5, ControllerInputContext.Dialogue));
        Assert.Contains(Buttons.LeftThumbstickUp,
            ControllerInputBindings.ButtonsFor(Keys.W, ControllerInputContext.Siege));
        Assert.Contains(Buttons.A,
            ControllerInputBindings.ButtonsFor(Keys.A, ControllerInputContext.FieldBattle));

        var moved = ControllerInputBindings.MovePointer(new Vector2(638, 2), new Vector2(1, 1), 1);
        Assert.Equal(new Vector2(639, 0), moved);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ControllerInputBindings.MovePointer(Vector2.Zero, Vector2.Zero, -.01));
        var trigger = new GamePadState(Vector2.Zero, Vector2.Zero, 0, 1, Buttons.None);
        Assert.True(ControllerInputBindings.PrimaryPointerPressed(trigger, released));
        Assert.True(ControllerInputBindings.PrimaryPointerReleased(released, trigger));
        var secondaryTrigger = new GamePadState(Vector2.Zero, Vector2.Zero, 1, 0, Buttons.None);
        Assert.True(ControllerInputBindings.SecondaryPointerPressed(secondaryTrigger, released));
    }

    [Fact]
    public void ImportedContentVerificationDetectsDamageAndUnsafeManifestRecords()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-verify-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var relative = Path.Combine("Decoded", "fixture.bin");
            var path = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, [1, 2, 3, 4]);
            var sourceHash = new string('a', 64);
            var valid = new ImportedAsset("fixture", relative, "resource", 4, ResourceHash.Sha256(path));
            var manifest = new ImportManifest(1, sourceHash, [valid]);
            Assert.True(ImportManifestVerifier.Verify(root, manifest).IsValid);

            File.WriteAllBytes(path, [4, 3, 2, 1]);
            var damaged = ImportManifestVerifier.Verify(root, manifest);
            Assert.False(damaged.IsValid);
            Assert.Contains(damaged.Issues, issue => issue.Reason == "SHA-256 mismatch");

            var unsafeManifest = new ImportManifest(1, sourceHash,
            [
                valid,
                valid with { Path = "../escape.bin" },
                valid with { Id = "bad-hash", Path = Path.Combine("Decoded", "other.bin"), Sha256 = "bad" }
            ]);
            var unsafeResult = ImportManifestVerifier.Verify(root, unsafeManifest);
            Assert.False(unsafeResult.IsValid);
            Assert.Contains(unsafeResult.Issues, issue => issue.Reason.Contains("escapes", StringComparison.Ordinal));
            Assert.Contains(unsafeResult.Issues, issue => issue.Reason.Contains("duplicate", StringComparison.Ordinal));
            Assert.Contains(unsafeResult.Issues, issue => issue.Reason.Contains("SHA-256", StringComparison.Ordinal));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GeneratedContentInstallationIsAtomicIncrementalAndManifestScoped()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-install-{Guid.NewGuid():N}");
        try
        {
            var relative = Path.Combine("Decoded", "fixture.bin");
            var first = GeneratedContentInstaller.InstallBytes(root, relative, [1, 2, 3]);
            Assert.True(first.Changed);
            var unchanged = GeneratedContentInstaller.InstallBytes(root, relative, [1, 2, 3]);
            Assert.False(unchanged.Changed);
            var replaced = GeneratedContentInstaller.InstallGenerated(root, relative, stream => stream.Write([4, 5, 6, 7]));
            Assert.True(replaced.Changed);
            Assert.Equal([4, 5, 6, 7], File.ReadAllBytes(replaced.Path));
            Assert.Empty(Directory.EnumerateFiles(Path.GetDirectoryName(replaced.Path)!, "*.tmp"));

            var unlisted = Path.Combine(root, "keep.txt");
            File.WriteAllText(unlisted, "mine");
            var asset = new ImportedAsset("fixture", relative, "resource", replaced.Size, replaced.Sha256);
            var manifest = new ImportManifest(1, new string('a', 64), [asset]);
            manifest.Write(Path.Combine(root, "manifest.json"));

            var unsafeManifest = manifest with { Assets = [asset, asset with { Id = "unsafe", Path = "../outside.bin" }] };
            Assert.Throws<InvalidDataException>(() => ImportedContentUninstaller.Remove(root, unsafeManifest));
            Assert.True(File.Exists(replaced.Path));

            Assert.Equal(1, ImportedContentUninstaller.Remove(root, manifest));
            Assert.False(File.Exists(replaced.Path));
            Assert.False(File.Exists(Path.Combine(root, "manifest.json")));
            Assert.True(File.Exists(unlisted));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void SupportedOriginalReleaseIsIdentifiedByExactSourceImageHash()
    {
        Assert.Equal("GOG English release", SupportedOriginalReleases.NameForSourceImage(
            SupportedOriginalReleases.GogEnglishSourceImageSha256.ToUpperInvariant()));
        Assert.Null(SupportedOriginalReleases.NameForSourceImage(new string('0', 64)));
    }

    [Fact]
    public void ImportDiskPlanningAccountsForNewFilesAndAtomicReplacementScratch()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-space-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            File.WriteAllBytes(Path.Combine(root, "existing.bin"), [1]);
            var plan = ImportDiskPlanner.Calculate(root,
            [
                new("existing.bin", 400),
                new(Path.Combine("new", "one.bin"), 100),
                new(Path.Combine("new", "two.bin"), 200)
            ]);
            Assert.Equal((700, 300, 400, 700),
                (plan.InstalledBytes, plan.NewBytes, plan.ReplacementScratchBytes, plan.RequiredAvailableBytes));
            Assert.Throws<InvalidDataException>(() => ImportDiskPlanner.Calculate(root,
                [new("same.bin", 1), new("SAME.BIN", 1)]));
            Assert.Throws<InvalidDataException>(() => ImportDiskPlanner.Calculate(root,
                [new("../escape.bin", 1)]));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void GameSettingsPersistAndRecoverThePreviousValidGeneration()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-settings-{Guid.NewGuid():N}");
        try
        {
            var path = Path.Combine(root, "settings.json");
            var store = new GameSettingsStore(path);
            Assert.Equal(new GameSettings(), store.Load());

            var first = new GameSettings
            {
                CdMusic = false,
                SoundEffects = true,
                Speech = false,
                Animation = true,
                Fullscreen = true,
                MusicVolume = .7f,
                EffectsVolume = .4f,
                SpeechVolume = .8f,
                ReducedMotion = true
            };
            store.Save(first);
            Assert.Equal(first, store.Load());
            var second = first with { SoundEffects = false, Fullscreen = false };
            store.Save(second);
            Assert.True(File.Exists(store.BackupPath));
            Assert.Equal(second, store.Load());

            File.WriteAllText(path, "corrupt");
            Assert.Equal(first, store.Load());
            File.WriteAllText(store.BackupPath, "corrupt too");
            Assert.Equal(new GameSettings(), store.Load());
            Assert.Throws<InvalidDataException>(() => store.Save(first with
                { Version = GameSettingsStore.CurrentVersion + 1 }));

            var legacyJson = "{\"Version\":1,\"CdMusic\":false,\"Fullscreen\":true}";
            File.WriteAllText(path, legacyJson);
            File.Delete(store.BackupPath);
            var migrated = store.Load();
            Assert.Equal(GameSettingsStore.CurrentVersion, migrated.Version);
            Assert.False(migrated.CdMusic);
            Assert.True(migrated.Fullscreen);
            Assert.Equal(.35f, migrated.MusicVolume);
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void PresentationScalingPreservesAspectRatioAndReversesPointerCoordinates()
    {
        Assert.Equal(new UiBounds(240, 0, 1440, 1080), PresentationScaling.Destination(1920, 1080, false));
        Assert.Equal(new UiBounds(448, 156, 1024, 768), PresentationScaling.Destination(1920, 1080, true));
        Assert.Equal(new UiBounds(0, 0, 800, 600), PresentationScaling.Destination(800, 600, true));

        var destination = PresentationScaling.Destination(1920, 1080, false);
        Assert.Equal((0, 0), PresentationScaling.ToVirtual(destination.X, destination.Y, destination));
        Assert.Equal((512, 384), PresentationScaling.ToVirtual(960, 540, destination));
        Assert.Equal((1023, 767), PresentationScaling.ToVirtual(destination.X + destination.Width - 1,
            destination.Y + destination.Height - 1, destination));
        Assert.True(PresentationScaling.ToVirtual(0, 0, destination).X < 0);
        Assert.True(PresentationScaling.ToLogical(destination.X - 1, destination.Y, destination, 640, 480).X < 0);
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

    private static byte[] ConversationNode(
        IReadOnlyList<string> strings,
        IReadOnlyList<int> targets,
        int continuationNodeId = 0,
        IReadOnlyList<int>? nodeActions = null,
        IReadOnlyList<IReadOnlyList<int>>? responseActions = null)
    {
        var encoded = strings.Select(System.Text.Encoding.ASCII.GetBytes).ToArray();
        var result = new byte[0x348 + encoded.Sum(bytes => bytes.Length + 1)];
        result[0x48] = checked((byte)targets.Count);
        result[0x08] = checked((byte)(strings.Count == 0 ? 0 : strings.Count - 2 - targets.Count));
        result[0x49] = 0x65; result[0x4a] = 0x3a; result[0x4b] = 0x5c;
        for (var response = 0; response < 5; response++)
        for (var action = 0; action < 30; action++)
            WriteInt(result, 0x60 + response * 0x78 + action * 4, -1);
        for (var action = 0; action < 30; action++) WriteInt(result, 0x2b8 + action * 4, -1);
        if (targets.Count == 0) WriteInt(result, 0x4c, continuationNodeId);
        for (var index = 0; index < targets.Count; index++) WriteInt(result, 0x4c + index * 4, targets[index]);
        for (var index = 0; index < (nodeActions?.Count ?? 0); index++) WriteInt(result, 0x2b8 + index * 4, nodeActions![index]);
        for (var response = 0; response < (responseActions?.Count ?? 0); response++)
        for (var action = 0; action < responseActions![response].Count; action++)
            WriteInt(result, 0x60 + response * 0x78 + action * 4, responseActions[response][action]);
        var position = 0x348;
        foreach (var bytes in encoded)
        {
            bytes.CopyTo(result, position);
            position += bytes.Length + 1;
        }
        return result;
    }

    private static void WriteInt(byte[] target, int offset, int value) => BinaryPrimitives.WriteInt32LittleEndian(target.AsSpan(offset, 4), value);

    private static (byte[] Viewer, byte[] Scenario, byte[] Map, byte[] Blocks) SyntheticScene()
    {
        var names = new[] { "ground", "arched door", "Secret Passage", "meal", "bag of coins", "knight", "champion" };
        var viewer = new byte[DynamixSceneDecoder.ViewerSize];
        WriteInt(viewer, 0, 10 << 8);
        WriteInt(viewer, 4, 20 << 8);
        WriteInt(viewer, 8, 48);
        WriteInt(viewer, 12, 16384);

        var scenario = new byte[DynamixSceneDecoder.ScenarioSize];
        WriteInt(scenario, 20, 32);
        WriteInt(scenario, 24, names.Length);
        WriteInt(scenario, 28, 1);

        var blocks = new byte[names.Length * DynamixSceneDecoder.BlockSize];
        for (var index = 0; index < names.Length; index++)
        {
            var offset = index * DynamixSceneDecoder.BlockSize;
            System.Text.Encoding.ASCII.GetBytes(names[index]).CopyTo(blocks, offset + 78);
            WriteInt(blocks, offset + 44, 12 + index);
            WriteInt(blocks, offset + 48, 13 + index);
            WriteInt(blocks, offset + 52, 14 + index);
            WriteInt(blocks, offset + 56, 15 + index);
            blocks[offset + 94] = 0xcc;
            blocks[offset + 95] = 0xcc;
        }

        var map = new byte[DynamixSceneDecoder.MapSize];
        SetSceneCell(map, 11, 20, 1);
        SetSceneCell(map, 10, 21, 2);
        SetSceneCell(map, 12, 20, 3);
        SetSceneCell(map, 9, 20, 4);
        SetSceneCell(map, 13, 20, 5);
        SetSceneCell(map, 14, 20, 6);
        return (viewer, scenario, map, blocks);
    }

    private static void SetSceneCell(byte[] map, int x, int y, ushort block) =>
        BinaryPrimitives.WriteUInt16LittleEndian(map.AsSpan((x * DynamixScene.MapHeight + y) * 2, 2), block);

    private static void SetSceneBlockName(byte[] blocks, int index, string name)
    {
        var field = blocks.AsSpan(index * DynamixSceneDecoder.BlockSize + 78, 16);
        field.Clear();
        System.Text.Encoding.ASCII.GetBytes(name).CopyTo(field);
    }

    private sealed class TestActionState(IReadOnlyList<int> initial) : IDynamixActionState
    {
        public List<int> Variables { get; } = [.. initial];
        private readonly Dictionary<int, int> _items = [];
        public bool TryGetVariable(int scope, int index, out int value)
        {
            value = 0;
            if (scope != 0 || (uint)index >= (uint)Variables.Count) return false;
            value = Variables[index];
            return true;
        }
        public bool TrySetVariable(int scope, int index, int value)
        {
            if (scope != 0 || (uint)index >= (uint)Variables.Count) return false;
            Variables[index] = value;
            return true;
        }
        public bool TryAddItem(int index) { _items[index] = _items.GetValueOrDefault(index) + 1; return true; }
        public bool TryClearItem(int index) => _items.Remove(index);
        public bool HasItem(int index) => _items.GetValueOrDefault(index) != 0;
    }

    private static byte[] SyntheticSmacker()
    {
        var source = new byte[142];
        "SMK2"u8.CopyTo(source);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(4, 4), 196);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(8, 4), 204);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(12, 4), 2);
        BinaryPrimitives.WriteInt32LittleEndian(source.AsSpan(16, 4), 100);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(24, 4), 4096);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(52, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(56, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(60, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(64, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(68, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(72, 4), 0xC000_0000u | 22050);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(104, 4), 13);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(108, 4), 12);
        source[112] = 1;
        source[113] = 2;
        source[118] = 2;
        source[122] = 0xFE;
        source[123] = 0xFF;
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(130, 4), 8);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(134, 4), 3);
        return source;
    }

    private static byte[] SyntheticSmackerVideo()
    {
        var source = new byte[120];
        "SMK2"u8.CopyTo(source);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(4, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(8, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(12, 4), 1);
        BinaryPrimitives.WriteInt32LittleEndian(source.AsSpan(16, 4), 100);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(52, 4), 7);
        for (var offset = 56; offset <= 68; offset += 4)
            BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(offset, 4), 4);
        BinaryPrimitives.WriteUInt32LittleEndian(source.AsSpan(104, 4), 5);
        source[109] = 0x08;
        source[115] = 0x80;
        return source;
    }

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
