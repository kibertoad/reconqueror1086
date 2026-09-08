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
    public void OriginalArtRolesAreDataDrivenAndUnique()
    {
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Title.Background", IdSuffix: ":fftitle.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Options", IdSuffix: ":char_ops.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Character.Pregenerated", IdSuffix: ":pregen.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Load.Background", IdSuffix: ":loadgame.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Map.England", IdSuffix: ":engmap1.pcx" });
        Assert.Equal(ImportedArt.Definitions.Count, ImportedArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void CharacterCreationScreenIsDefinitionDriven()
    {
        Assert.Equal(Enum.GetValues<CharacterCreationAction>().Length, CharacterCreationDefinitions.Options.Count);
        Assert.Equal(CharacterCreationDefinitions.Options.Count, CharacterCreationDefinitions.Options.Select(x => x.Action).Distinct().Count());
        Assert.Equal(Balance.Templates.Length, CharacterCreationDefinitions.PregeneratedCharacters.Count);
        Assert.Equal(["Red", "Green", "Blue"], CharacterCreationDefinitions.HeraldicColors.Select(x => x.Name));
        Assert.All(CharacterCreationDefinitions.HeraldicColors, x => Assert.True(CharacterCreationDefinitions.Options[1].OriginalBounds.Contains(x.OriginalBounds.X, x.OriginalBounds.Y)));
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
