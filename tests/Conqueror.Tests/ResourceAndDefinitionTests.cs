using Conqueror.Core;
using Conqueror.Game;
using Conqueror.Resources;
using System.Buffers.Binary;
using Xunit;

namespace Conqueror.Tests;

public sealed class ResourceAndDefinitionTests
{
    [Theory]
    [InlineData("scene.RES", true)]
    [InlineData("scene.low", true)]
    [InlineData("image.PCX", false)]
    public void DynamixContainerExtensionsIncludeLowSceneArchives(string path, bool expected)
    {
        Assert.Equal(expected, DynamixArchive.HasContainerExtension(path));
    }

    [Fact]
    public void DecodedArchiveFoldersPreserveExtensionsToSeparateSceneTiers()
    {
        Assert.Equal("BAR0.LOW", ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.LOW"));
        Assert.Equal("BAR0.RES", ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.RES"));
        Assert.NotEqual(ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.LOW"),
            ResourcePaths.DecodedArchiveFolder("CONQUER/BAR0.RES"));
        Assert.Throws<InvalidDataException>(() => ResourcePaths.DecodedArchiveFolder(".."));
    }

    [Theory]
    [InlineData("TEX000 16 16", 256, 0, 16, 16)]
    [InlineData("TEX081 128 156", 19968, 81, 128, 156)]
    [InlineData("tex241 64 91", 5824, 241, 64, 91)]
    public void SceneTextureNamesBoundRawIndexedPixelDimensions(
        string name, int length, int index, int width, int height)
    {
        var texture = DynamixSceneTextureDecoder.Decode(name, new byte[length]);
        Assert.Equal((index, width, height, length),
            (texture.Index, texture.Width, texture.Height, texture.Indices.Length));
    }

    [Fact]
    public void SceneTextureDecoderRejectsMalformedOrInconsistentResources()
    {
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("PIC000 16 16", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX+01 16 16", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX000 +16 16", new byte[256]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX000 16 16", new byte[255]));
        Assert.Throws<InvalidDataException>(() => DynamixSceneTextureDecoder.Decode("TEX000 11", new byte[11]));
    }

    [Fact]
    public void ConversationDatabaseDecodesIndexedPromptsResponsesAndLinks()
    {
        var first = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings.", "Ask about the dragon.", "Farewell."],
            [1102, 0], nodeActions: [10, 11], responseActions: [[20], []]);
        var second = ConversationNode(["BARKEEP.PCC", "Bartender", "Good day."], [], 1101);
        var body = first.Concat(second).ToArray();
        var index = new byte[16];
        WriteInt(index, 0, 1102); WriteInt(index, 4, first.Length);
        WriteInt(index, 8, 1101); WriteInt(index, 12, 0);

        var database = DynamixConversationDecoder.Decode(body, index);

        Assert.Equal(2, database.Nodes.Count);
        var gerard = Assert.IsType<DynamixConversationNode>(database.Find(1101));
        Assert.Equal(("GERARD.PCC", "Earl Gerard"), (gerard.PortraitFile, gerard.Speaker));
        Assert.Equal(["Greetings."], gerard.PromptVariants);
        Assert.Equal(["Ask about the dragon.", "Farewell."], gerard.Responses.Select(response => response.Text));
        Assert.Equal([1102, 0], gerard.Responses.Select(response => response.TargetNodeId));
        Assert.Equal([20], gerard.Responses[0].ActionIds);
        Assert.Empty(gerard.Responses[1].ActionIds);
        Assert.Equal([10, 11], gerard.ActionIds);
        var barkeep = Assert.IsType<DynamixConversationNode>(database.Find(1102));
        Assert.Equal(("Bartender", 1101), (barkeep.Speaker, barkeep.ContinuationNodeId));
        Assert.Null(gerard.ContinuationNodeId);
    }

    [Fact]
    public void ConversationDatabaseRejectsMalformedIndicesMarkersTextAndLinks()
    {
        var valid = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings.", "Continue."], [0]);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(valid, new byte[7]));

        var duplicateIndex = new byte[16];
        WriteInt(duplicateIndex, 0, 1); WriteInt(duplicateIndex, 4, 0);
        WriteInt(duplicateIndex, 8, 1); WriteInt(duplicateIndex, 12, 0);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(valid, duplicateIndex));

        var oneIndex = new byte[8];
        WriteInt(oneIndex, 0, 1); WriteInt(oneIndex, 4, 0);
        var badMarker = valid.ToArray(); badMarker[0x49] = 0;
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badMarker, oneIndex));
        var badPromptCount = valid.ToArray(); badPromptCount[0x08]++;
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badPromptCount, oneIndex));
        var badText = valid.ToArray(); badText[^1] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badText, oneIndex));
        var badLink = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings.", "Continue."], [99]);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badLink, oneIndex));
        var badContinuation = ConversationNode(["GERARD.PCC", "Earl Gerard", "Greetings."], [], 99);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badContinuation, oneIndex));
        var badAction = valid.ToArray(); WriteInt(badAction, 0x2b8, -2);
        Assert.Throws<InvalidDataException>(() => DynamixConversationDecoder.Decode(badAction, oneIndex));
    }

    [Fact]
    public void ImportedConversationSessionTraversesSelectorsResponsesAndTimedContinuations()
    {
        var nodes = new Dictionary<int, DynamixConversationNode>
        {
            [1] = new(1, 0, null, null, [], [], 2, []),
            [2] = new(2, 0, "GERARD.PCC", "Earl Gerard", ["First.", "Second."],
                [new DynamixConversationResponse("Continue.", 3, [])], null, []),
            [3] = new(3, 0, "BARKEEP.PCC", "Bartender", ["Farewell."], [], 0, [])
        };
        var session = new ImportedConversationSession(new DynamixConversationDatabase(nodes));

        Assert.True(session.Start(1, count => count - 1));
        Assert.Equal((2, "Second."), (session.CurrentNode?.Id, session.Prompt));
        Assert.True(session.ChooseResponse(0, _ => 0));
        Assert.Equal((3, "Farewell."), (session.CurrentNode?.Id, session.Prompt));
        Assert.False(session.Continue(_ => 0));
        Assert.True(session.IsComplete);

        var cycle = new ImportedConversationSession(new DynamixConversationDatabase(
            new Dictionary<int, DynamixConversationNode> { [1] = new(1, 0, null, null, [], [], 1, []) }));
        Assert.Throws<InvalidDataException>(() => cycle.Start(1, _ => 0));
    }

    [Fact]
    public void ActionTreeDatabaseDecodesIndexedRecursiveExpressionsWithinBounds()
    {
        var body = new byte[104];
        WriteInt(body, 0, 1); WriteInt(body, 4, 8);
        WriteInt(body, 8, (int)DynamixActionKind.IfElse);
        WriteInt(body, 12, 28); WriteInt(body, 16, 2);
        WriteInt(body, 20, 80); WriteInt(body, 24, 92);
        WriteInt(body, 28, 2); WriteInt(body, 32, 1);
        WriteInt(body, 36, 48); WriteInt(body, 40, 64);
        WriteInt(body, 44, (int)DynamixExpressionOperator.Equal);
        WriteInt(body, 48, (int)DynamixValueKind.Literal); WriteInt(body, 52, 7); WriteInt(body, 56, -1);
        WriteInt(body, 64, (int)DynamixValueKind.Literal); WriteInt(body, 68, 7); WriteInt(body, 72, 1);
        WriteInt(body, 80, (int)DynamixActionKind.Evaluate); WriteInt(body, 84, 28);
        WriteInt(body, 92, (int)DynamixActionKind.Evaluate); WriteInt(body, 96, 28);
        var index = new byte[12];
        WriteInt(index, 0, 1); WriteInt(index, 4, 1101); WriteInt(index, 8, 0);

        var database = DynamixActionTreeDecoder.Decode(body, index);

        var group = Assert.IsType<DynamixActionGroup>(database.Find(1101));
        Assert.Equal([8], group.ActionOffsets);
        Assert.Equal(DynamixActionKind.IfElse, database.Actions[8].Kind);
        Assert.Equal([80, 92], database.Actions[8].BranchActionOffsets);
        Assert.Equal([DynamixExpressionOperator.Equal], database.Expressions[28].Operators);
        Assert.False(database.Values[48].Invert);
        Assert.True(database.Values[64].Invert);
        Assert.Equal(1, database.IndexHeaderValue);
        Assert.Equal((3, 1, 2), (database.Actions.Count, database.Expressions.Count, database.Values.Count));

        var badOperator = body.ToArray(); WriteInt(badOperator, 44, 9);
        Assert.Throws<InvalidDataException>(() => DynamixActionTreeDecoder.Decode(badOperator, index));
        Assert.Throws<InvalidDataException>(() => DynamixActionTreeDecoder.Decode(body, index[..^1]));
    }

    [Fact]
    public void PixelTextWrapsAtWordsAndKeepsOversizedWordsIntact()
    {
        Assert.Equal("ONE TWO\nTHREE", PixelTextLayout.Wrap("ONE TWO THREE", 7));
        Assert.Equal("SUPERCALIFRAGILISTIC", PixelTextLayout.Wrap("SUPERCALIFRAGILISTIC", 5));
        Assert.Equal("ONE\n\nTWO", PixelTextLayout.Wrap(" ONE  \r\n\r\n TWO ", 8));
        Assert.Throws<ArgumentOutOfRangeException>(() => PixelTextLayout.Wrap("ONE", 0));
    }

    [Fact]
    public void VariableTableAndActionInterpreterApplyPersistentMutationAndRedirect()
    {
        var variableBytes = new byte[20];
        WriteInt(variableBytes, 0, 2); WriteInt(variableBytes, 4, 4); WriteInt(variableBytes, 8, 5);
        WriteInt(variableBytes, 12, 3); WriteInt(variableBytes, 16, 0);
        var table = DynamixVariableTableDecoder.Decode(variableBytes);
        Assert.Equal([3, 0], table.InitialValues);
        Assert.Throws<InvalidDataException>(() => DynamixVariableTableDecoder.Decode(variableBytes[..^1]));

        var values = new Dictionary<int, DynamixValueNode>
        {
            [1] = new(1, DynamixValueKind.Literal, 0, false, []),
            [2] = new(2, DynamixValueKind.Literal, 1, false, []),
            [3] = new(3, DynamixValueKind.Function, 5, false, [10, 11, 11]),
            [4] = new(4, DynamixValueKind.Function, 6, false, [10, 11]),
            [5] = new(5, DynamixValueKind.Function, 3, false, [13]),
            [6] = new(6, DynamixValueKind.Function, 3, false, [14])
        };
        var expressions = new Dictionary<int, DynamixExpressionNode>
        {
            [10] = new(10, [1], []), [11] = new(11, [2], []),
            [12] = new(12, [3], []), [13] = new(13, [2], []),
            [14] = new(14, [1], []),
            [15] = new(15, [4, 2], [DynamixExpressionOperator.GreaterThanOrEqual]),
            [16] = new(16, [5], []), [17] = new(17, [6], [])
        };
        var actions = new Dictionary<int, DynamixActionNode>
        {
            [20] = new(20, DynamixActionKind.Evaluate, 12, []),
            [21] = new(21, DynamixActionKind.IfElse, 15, [22, 23]),
            [22] = new(22, DynamixActionKind.Evaluate, 16, []),
            [23] = new(23, DynamixActionKind.Evaluate, 17, [])
        };
        var database = new DynamixActionTreeDatabase(100,
            new Dictionary<int, DynamixActionGroup> { [99] = new(99, 0, [20, 21]) }, actions, expressions, values);
        var state = new TestActionState([3, 0]);

        var result = new DynamixActionInterpreter(database, state).Execute([99]);

        Assert.True(result.Success);
        Assert.Equal(1, state.Variables[1]);
        Assert.Equal(1, result.RedirectNodeId);
        Assert.False(new DynamixActionInterpreter(database, state).Execute([5011]).Success);

        var campaign = new CampaignState { Player = new Player { Wealth = 12 } };
        var campaignState = new ImportedConversationActionState(campaign);
        campaignState.Initialize(table.InitialValues);
        Assert.True(campaignState.TryGetVariable(1, 0, out var wealth));
        Assert.Equal(12, wealth);
        Assert.True(campaignState.TrySetVariable(1, 0, -4));
        Assert.Equal(0, campaign.Player.Wealth);
        Assert.Equal([3, 0], campaign.ConversationVariables);
    }

    [Fact]
    public void OriginalConversationSelectorsAndItemsUseTypedCampaignState()
    {
        var campaign = new CampaignState
        {
            Player = new Player
            {
                Wealth = 120,
                Fame = 4,
                Stats = new CharacterStats(11, 12, 13, 14, 15, 16)
            },
            ConversationItems = { [21] = 1 }
        };
        var state = new ImportedConversationActionState(campaign);

        state.Initialize([]);

        Assert.Equal(7, OriginalConversationBindings.Attributes.Count);
        Assert.Equal(24, OriginalConversationBindings.Items.Count);
        Assert.Contains("Book of Hours", campaign.Player.Inventory.Items);
        Assert.True(state.TryGetVariable(1, 2, out var honor));
        Assert.True(state.TryGetVariable(1, 3, out var fame));
        Assert.True(state.TryGetVariable(1, 5, out var piety));
        Assert.True(state.TryGetVariable(1, 6, out var strength));
        Assert.True(state.TryGetVariable(1, 7, out var stamina));
        Assert.True(state.TryGetVariable(1, 8, out var intelligence));
        Assert.Equal((15, 4, 13, 11, 14, 16), (honor, fame, piety, strength, stamina, intelligence));

        Assert.True(state.TrySetVariable(1, 2, 30));
        Assert.True(state.TrySetVariable(1, 3, -5));
        Assert.Equal((20, 0), (campaign.Player.Stats.Honor, campaign.Player.Fame));
        Assert.True(state.TryAddItem(10));
        Assert.Contains("Dragon Slaying Lance", campaign.Player.Inventory.Items);
        Assert.True(state.HasItem(10));
        Assert.True(state.TryClearItem(10));
        Assert.DoesNotContain("Dragon Slaying Lance", campaign.Player.Inventory.Items);
        campaign.Player.Inventory.Items.Add("Shield of St. George");
        Assert.True(state.HasItem(15));
    }

    [Fact]
    public void SmackerMovieHeaderAndFrameIndexAreBounded()
    {
        var movie = SmackerMovieDecoder.Decode(SyntheticSmacker());

        Assert.Equal((2, 196, 204, 2), (movie.Version, movie.Width, movie.Height, movie.Frames.Count));
        Assert.Equal(TimeSpan.FromMilliseconds(100), movie.FrameDuration);
        Assert.Equal((114, 4), (movie.TreeOffset, movie.TreeLength));
        var track = Assert.Single(movie.AudioTracks);
        Assert.Equal((0, 22050, 4096, true, false, false),
            (track.Index, track.SampleRate, track.MaximumDecodedBytes,
                track.IsCompressed, track.Is16Bit, track.IsStereo));
        Assert.Equal((118, 12, true, (byte)1),
            (movie.Frames[0].Offset, movie.Frames[0].Length, movie.Frames[0].IsKeyFrame, movie.Frames[0].Flags));
        Assert.Equal((130, 12, false, (byte)2),
            (movie.Frames[1].Offset, movie.Frames[1].Length, movie.Frames[1].IsKeyFrame, movie.Frames[1].Flags));
    }

    [Fact]
    public void SmackerFrameLayoutDecodesPaletteAndAudioPacketBoundaries()
    {
        var source = SyntheticSmacker();
        var movie = SmackerMovieDecoder.Decode(source);
        var first = SmackerMovieDecoder.DecodeFrameLayout(movie, 0, source, new byte[768]);

        Assert.True(first.PaletteChanged);
        Assert.Empty(first.AudioPackets);
        Assert.Equal((126, 4), (first.Video.Offset, first.Video.Length));

        var second = SmackerMovieDecoder.DecodeFrameLayout(movie, 1, source, first.Palette);
        Assert.False(second.PaletteChanged);
        var audio = Assert.Single(second.AudioPackets);
        Assert.Equal((0, 3, 134, 4),
            (audio.TrackIndex, audio.DecodedLength, audio.Data.Offset, audio.Data.Length));
        Assert.Equal((138, 4), (second.Video.Offset, second.Video.Length));
    }

    [Fact]
    public void SmackerStreamReaderRetainsOnlyIndexTreesAndOneFrame()
    {
        var source = SyntheticSmacker();
        using var stream = new MemoryStream(source);
        using var reader = new SmackerMovieStream(stream, leaveOpen: true);
        var frameBuffer = new byte[reader.MaximumFrameLength];

        Assert.Equal(source.AsSpan(114, 4).ToArray(), reader.TreeData);
        Assert.Equal(12, reader.ReadFrame(1, frameBuffer));
        var frame = SmackerMovieDecoder.DecodeFramePayload(
            reader.Movie, 1, frameBuffer, new byte[768]);
        var audio = Assert.Single(frame.AudioPackets);
        Assert.Equal((4, 4, 8, 4),
            (audio.Data.Offset, audio.Data.Length, frame.Video.Offset, frame.Video.Length));
        Assert.True(stream.CanRead);
    }

    [Fact]
    public void SmackerPackedMonoAudioDecodesPredictiveHuffmanSamples()
    {
        var packet = new byte[7];
        BinaryPrimitives.WriteUInt32LittleEndian(packet, 3);
        packet[4] = 0x29;
        packet[5] = 0xA0;
        packet[6] = 0x02;
        var track = new SmackerAudioTrack(0, 22050, 4096, true, false, false);

        var decoded = SmackerAudioDecoder.Decode(packet, track);

        Assert.Equal([10, 11, 12], decoded.Samples);
        Assert.Equal([0, 0x8A, 0, 0x8B, 0, 0x8C], decoded.ToPcm16LittleEndian());
    }

    [Fact]
    public void SmackerPackedAudioRejectsTruncationAndProfileMismatch()
    {
        var track = new SmackerAudioTrack(0, 22050, 4096, true, false, false);
        Assert.Throws<InvalidDataException>(() => SmackerAudioDecoder.Decode([3, 0, 0, 0, 1], track));
        Assert.Throws<InvalidDataException>(() => SmackerAudioDecoder.Decode([3, 0, 0, 0, 0x2B, 0xA0, 0x02], track));
        Assert.Throws<NotSupportedException>(() => SmackerAudioDecoder.Decode([3, 0, 0, 0, 1], track with { IsStereo = true }));
    }

    [Fact]
    public void SmackerVideoTreesDecodeFourByFourBlocksIntoPriorFrameBuffer()
    {
        var source = SyntheticSmackerVideo();
        var movie = SmackerMovieDecoder.Decode(source);
        var layout = SmackerMovieDecoder.DecodeFrameLayout(movie, 0, source, new byte[768]);
        var decoder = new SmackerVideoDecoder(movie, source);
        var indices = Enumerable.Repeat((byte)9, 16).ToArray();

        decoder.DecodeFrame(source.AsSpan(layout.Video.Offset, layout.Video.Length), indices, true);

        Assert.All(indices, value => Assert.Equal(0, value));
    }

    [Fact]
    public void SmackerMovieRejectsInvalidHeadersAndFrameExtents()
    {
        var badMagic = SyntheticSmacker();
        badMagic[0] = (byte)'X';
        Assert.Throws<InvalidDataException>(() => SmackerMovieDecoder.Decode(badMagic));

        var badExtent = SyntheticSmacker();
        BinaryPrimitives.WriteUInt32LittleEndian(badExtent.AsSpan(108, 4), 16);
        Assert.Throws<InvalidDataException>(() => SmackerMovieDecoder.Decode(badExtent));

        var badDimensions = SyntheticSmacker();
        BinaryPrimitives.WriteUInt32LittleEndian(badDimensions.AsSpan(4, 4), 195);
        Assert.Throws<InvalidDataException>(() => SmackerMovieDecoder.Decode(badDimensions));
    }

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
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Campaign.Briefing", IdSuffix: ":fluff.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Village.Inn", IdSuffix: ":innpeopl.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Load.Background", IdSuffix: ":loadgame.pcx" });
        Assert.Contains(ImportedArt.Definitions, x => x is { Role: "Map.England", IdSuffix: ":engmap1.pcx" });
        Assert.Equal(ImportedArt.Definitions.Count, ImportedArt.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Equal(ImportedAnimations.Definitions.Count,
            ImportedAnimations.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
        Assert.Contains(ImportedMovies.Definitions,
            definition => definition is { Role: "Title.Intro", IdSuffix: "/title.smk" });
        Assert.Contains(ImportedMovies.Definitions,
            definition => definition is { Role: "Options.Credits", IdSuffix: "/creditzz.smk" });
        Assert.Equal(ImportedMovies.Definitions.Count,
            ImportedMovies.Definitions.Select(x => x.Role).Distinct(StringComparer.OrdinalIgnoreCase).Count());
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
        Assert.Equal(OptionsHubDefinitions.EnabledStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: true, pressed: false));
        Assert.Equal(OptionsHubDefinitions.EnabledPressedStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: true, pressed: true));
        Assert.Equal(OptionsHubDefinitions.DisabledStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: false, pressed: false));
        Assert.Equal(OptionsHubDefinitions.DisabledPressedStatusFrame, OptionsHubDefinitions.StatusFrame(enabled: false, pressed: true));
        var optionsAnimation = Assert.Single(ImportedAnimations.Definitions, definition => definition.Role == "Options.Widgets");
        Assert.Equal("Options.Background", optionsAnimation.PaletteArtRole);
    }

    [Fact]
    public void OriginalCursorFramesRetainTheirDecodedOrder()
    {
        Assert.Equal(OriginalCursorDefinitions.FrameCount, Enum.GetValues<OriginalCursorKind>().Length);
        Assert.Equal(Enumerable.Range(0, OriginalCursorDefinitions.FrameCount),
            Enum.GetValues<OriginalCursorKind>().Select(OriginalCursorDefinitions.Frame));
    }

    [Fact]
    public void PracticeMenuUsesOriginalRegionAndExecutableLabelOrder()
    {
        Assert.Equal(Enum.GetValues<PracticeAction>().Length, PracticePresentationDefinitions.Options.Count);
        Assert.Equal(["War", "Joust", "Melee", "Exit", "Castle Skirmish"],
            PracticePresentationDefinitions.Options.Select(option => option.Label));
        Assert.Equal(Enumerable.Range(0, 5),
            PracticePresentationDefinitions.Options.Select(option => option.HatRegionId));
        Assert.Equal(new UiBounds(1, 1, 234, 170),
            PracticePresentationDefinitions.Options.Single(option => option.Action == PracticeAction.CastleSkirmish).OriginalBounds);
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Practice.Background" && definition.IdSuffix == ":practice.pcx");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Practice" && definition.IdSuffix == ":practice.hat");
        Assert.Contains(ImportedMovies.Definitions,
            definition => definition.Role == "Practice.Joust" && definition.IdSuffix == "/jousprac.smk");
    }

    [Fact]
    public void PracticeCombatAdaptersCreateIsolatedRepeatableSessions()
    {
        var war = PracticeCombatDefinitions.CreateWar(seed: 7);
        Assert.Equal(32, war.Friendly.Sum(squad => squad.Count));
        Assert.Equal(32, war.Enemy.Sum(squad => squad.Count));
        Assert.Equal(3, war.Friendly.Count());
        Assert.Equal(3, war.Enemy.Count());

        var melee = PracticeCombatDefinitions.CreateMelee(seed: 7);
        var castle = PracticeCombatDefinitions.CreateCastleSkirmish(seed: 7);
        Assert.Equal(7, melee.Enemies.Count);
        Assert.Equal(11, castle.Enemies.Count);
        Assert.NotSame(melee, PracticeCombatDefinitions.CreateMelee(seed: 7));
        Assert.NotSame(castle, PracticeCombatDefinitions.CreateCastleSkirmish(seed: 7));
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
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Home.Overview" && definition.IdSuffix == ":f_over.pcx");
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Home.WarPlanning" && definition.IdSuffix == ":warplan.pcx");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Home.Overview" && definition.IdSuffix == ":foview.hat");
        Assert.Contains(ImportedLayouts.Definitions,
            definition => definition.Role == "Home.WarPlanning" && definition.IdSuffix == ":fwarplan.hat");
        Assert.Contains(ImportedAnimations.Definitions,
            definition => definition.Role == "Home.WarPlanning.Controls" && definition.IdSuffix == ":warplan.csf"
                && definition.PaletteArtRole == "Home.WarPlanning");
        Assert.Equal([0, 3, 6, 9, 12], Enumerable.Range(0, 5)
            .Select(index => WarPlanningPresentationDefinitions.ArmyFrame(index, true, true)));
        Assert.Equal([2, 5, 8, 11, 14], Enumerable.Range(0, 5)
            .Select(index => WarPlanningPresentationDefinitions.ArmyFrame(index, false, false)));
        Assert.Equal(5, WarPlanningPresentationDefinitions.Fallback.ArmyButtons.Count);
        Assert.Equal(3, WarPlanningPresentationDefinitions.Fallback.UnitRows.Count);
        Assert.Equal(
            [(SceneNavigationAction.Overview, "Overview", 0, new UiBounds(248, 184, 64, 31)),
             (SceneNavigationAction.Castle, "Castle", 1, new UiBounds(172, 153, 72, 44)),
             (SceneNavigationAction.Farm, "Farm", 2, new UiBounds(277, 146, 17, 38)),
             (SceneNavigationAction.Village, "Village", 3, new UiBounds(295, 141, 18, 43)),
             (SceneNavigationAction.Forest, "Forest", 4, new UiBounds(314, 147, 15, 39)),
             (SceneNavigationAction.WarPlanning, "War Planning", 5, new UiBounds(359, 123, 35, 76)),
             (SceneNavigationAction.Exit, "Exit", 6, new UiBounds(0, 113, 66, 210)),
             (SceneNavigationAction.Jump, "JUMP!!", 7, new UiBounds(568, 84, 72, 200)),
             (SceneNavigationAction.Map, "Map", 8, new UiBounds(200, 55, 68, 85)),
             (SceneNavigationAction.Orders, "Orders", 9, new UiBounds(332, 180, 26, 37))],
            HomePresentationDefinitions.Hotspots.Select(hotspot =>
                (hotspot.Action, hotspot.HoverLabel, hotspot.HatRegionId, hotspot.Bounds)));
        Assert.Equal(
            [(SceneNavigationAction.BlacksmithDialogue, "Blacksmith", new UiBounds(253, 109, 107, 164)),
             (SceneNavigationAction.Shop, "Buy/Sell", new UiBounds(54, 2, 180, 141))],
            BlacksmithPresentationDefinitions.Hotspots.Select(hotspot => (hotspot.Action, hotspot.HoverLabel, hotspot.Bounds)));
        Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == "Dialogue.Frame" && definition.IdSuffix == ":comscrn1.pcx");
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
                (layout.Section, layout.LayoutSuffix, layout.AccountRowCount, layout.TerrainRegionId, layout.FullScreenRegionId)));
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
            command => Assert.True(command.Action is BuildFarmAction or LeaveFarmAction));
        Assert.All(FarmPresentationDefinitions.CommandsFor(FarmPresentationDefinitions.Section.Village),
            command => Assert.True(command.Action is BuildFarmAction or LeaveFarmAction));
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
        Assert.Equal(new UiBounds(12, 12, 1, 1), importedFarm.FullScreen);
        Assert.Equal(9, importedFarm.Rows.Count);
        Assert.Equal(new UiBounds(0, 0, 1, 1), importedFarm.Rows[0]);
        Assert.Equal(new UiBounds(9, 9, 1, 1), importedFarm.Okay);
        Assert.Equal(new UiBounds(10, 10, 1, 1), importedFarm.Cancel);
        Assert.Equal(FarmPresentationDefinitions.FooterAction.Okay,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 9, 9));
        Assert.Equal(FarmPresentationDefinitions.FooterAction.Cancel,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 10, 10));
        Assert.Equal(FarmPresentationDefinitions.FooterAction.FullScreen,
            FarmPresentationDefinitions.FooterActionAt(importedFarm, 12, 12));
        Assert.Null(FarmPresentationDefinitions.FooterActionAt(importedFarm, 20, 20));

        Assert.Equal(["Wall", "Tower", "Great Hall", "Servant Room", "Guardhouse", "Gate House", "Storehouse",
            "Chapel", "Well", "Stable", "Steward", "Beadle", "Guard Captain", "Guard", "Priest", "Mason", "Serf"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Castle).Select(entry => entry.Label));
        Assert.Equal(["Clear Land", "Road", "Mill", "Tavern", "Bakery", "Inn", "Carpenter", "Smith", "Tanner",
            "Merchant", "Church", "Monastery", "Barber", "Houses", "Livestock", "Horses", "Granary"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Village).Select(entry => entry.Label));
        Assert.Equal(["Grain", "Beans", "Vegetables", "Fruit"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Farm).Select(entry => entry.Label));
        Assert.Equal(["Cut Timber", "Iron Mine", "Woodward", "Coal Mine", "Gold Mine", "Silver Mine", "Prospector"],
            FarmPresentationDefinitions.EntriesFor(FarmPresentationDefinitions.Section.Forest).Select(entry => entry.Label));
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
    public void WarPlanningUsesFiveArmiesAndHundredSerfCompanies()
    {
        var campaign = new Campaign();
        var player = campaign.State.Player;
        Assert.Equal(Player.ArmyDivisionLimit - 1, player.AdditionalArmies.Count);

        Assert.True(campaign.AdjustArmyCompany(4, UnitType.Knights, 1));
        Assert.Equal(100, player.ArmyAt(4).Units[UnitType.Knights]);
        Assert.Equal(1100, player.AvailableSerfs);
        Assert.True(campaign.AdjustArmyCompany(4, UnitType.Knights, -1));
        Assert.Equal(0, player.ArmyAt(4).Total);
        Assert.False(campaign.AdjustArmyCompany(4, UnitType.Knights, -1));

        player.Home.Population = 10_000;
        for (var company = 0; company < WarPlanningCheckpoint.MaximumCompaniesPerArmy; company++)
            Assert.True(campaign.AdjustArmyCompany(0, UnitType.Swordsmen, 1));
        Assert.Equal(6000, player.Army.Total);
        Assert.False(campaign.AdjustArmyCompany(0, UnitType.Swordsmen, 1));

        player.AdditionalArmies[3].Location = 2;
        Assert.False(campaign.AdjustArmyCompany(4, UnitType.Swordsmen, 1));
    }

    [Fact]
    public void CaptainCommandedDivisionTravelsFightsAndSurvivesSaveLoadIndependently()
    {
        var campaign = new Campaign(Campaign.NewFromTemplate(2), 1086);
        var york = Array.FindIndex(World.Locations, location => location.Name == "York");
        foreach (var type in Enum.GetValues<UnitType>()) campaign.State.Player.ArmyAt(2).Units[type] = 40;
        Assert.True(campaign.FieldArmy(2));
        Assert.False(campaign.DispatchArmy(0, york));
        Assert.True(campaign.DispatchArmy(2, york));

        var order = Assert.IsType<StrategicArmyOrder>(campaign.ArmyOrderAt(2));
        var path = Path.Combine(Path.GetTempPath(), $"conqueror-army-order-{Guid.NewGuid():N}.json");
        try
        {
            campaign.Save(path);
            var loaded = Campaign.Load(path);
            Assert.Equal(order, loaded.ArmyOrderAt(2));
        }
        finally
        {
            File.Delete(path);
        }

        campaign.AdvanceDays((order.Arrives - campaign.State.Date).Days - 1);
        Assert.Equal(0, campaign.State.Player.ArmyLocationAt(2));
        Assert.NotNull(campaign.ArmyOrderAt(2));
        campaign.AdvanceDays(1);

        Assert.Null(campaign.ArmyOrderAt(2));
        Assert.Equal(york, campaign.State.Player.ArmyLocationAt(2));
        Assert.True(campaign.GarrisonAt(york) < World.Locations[york].Garrison);
        Assert.Equal(0, campaign.State.Player.Army.Total);
        Assert.Contains(campaign.State.Journal, entry => entry.Contains("Captain's report", StringComparison.Ordinal));
    }

    [Fact]
    public void ArmyRosterRepairsLegacySlotsAndRejectsOverflow()
    {
        var player = new Player { AdditionalArmies = [] };
        player.EnsureArmyRoster();
        Assert.Equal(["Army 2", "Army 3", "Army 4", "Army 5"],
            player.AdditionalArmies.Select(army => army.Name));

        player.AdditionalArmies.Add(new StrategicArmyDivision { Name = "Army 6" });
        Assert.Throws<InvalidDataException>(player.EnsureArmyRoster);
    }

    [Fact]
    public void WarPlanningCheckpointRollsBackAllDivisionsSpiesAndMembership()
    {
        var campaign = new Campaign();
        campaign.State.Player.Wealth = 500;
        var checkpoint = new WarPlanningCheckpoint(campaign);

        Assert.True(campaign.AdjustArmyCompany(2, UnitType.Halberdiers, 1));
        campaign.State.Player.SetArmyName(2, "Northern Guard");
        Assert.True(campaign.FieldArmy(2));
        Assert.True(campaign.ToggleArmyMembership(2));
        Assert.True(campaign.AssignSpy());
        checkpoint.Restore(campaign);

        var player = campaign.State.Player;
        Assert.Equal(500, player.Wealth);
        Assert.Equal(0, player.ArmyAt(2).Total);
        Assert.Equal("Army 3", player.ArmyNameAt(2));
        Assert.False(player.ArmyIsFielded(2));
        Assert.Equal(0, player.JoinedArmyIndex);
        Assert.Equal(0, player.ActiveSpies);
        Assert.Empty(campaign.State.Journal);
    }

    [Fact]
    public void ActiveSpiesProduceRecurringMonthlyIntel()
    {
        var campaign = new Campaign(seed: 17);
        campaign.State.Player.Wealth = 500;

        Assert.True(campaign.AssignSpy());
        Assert.Equal((420, 1), (campaign.State.Player.Wealth, campaign.State.Player.ActiveSpies));
        campaign.SettleMonth();

        Assert.Single(campaign.State.SpiedLocations);
        Assert.Contains(campaign.State.Journal, entry => entry.Contains("A spy reports", StringComparison.Ordinal));
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
        Assert.Equal(new UiBounds(69, 18, 119, 133),
            HomePresentationDefinitions.HotspotsFrom(layout).Single(hotspot => hotspot.HatRegionId == 0).Bounds);
        Assert.Equal(new UiBounds(69, 18, 119, 133),
            WarPlanningPresentationDefinitions.From(layout).ArmyButtons[0]);
    }

    [Fact]
    public void HomeDeskBooksRemainThreeDistinctDescriptorDrivenHotspots()
    {
        var bytes = new byte[40 + 10 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 10);
        for (var id = 0; id < 10; id++)
        {
            var offset = 40 + id * 24;
            WriteInt(bytes, offset, id);
            WriteInt(bytes, offset + 4, 10 + id * 20);
            WriteInt(bytes, offset + 8, 100 + id);
            WriteInt(bytes, offset + 12, 11 + id);
            WriteInt(bytes, offset + 16, 21 + id);
            WriteInt(bytes, offset + 20, 1);
        }

        var books = HomePresentationDefinitions.HotspotsFrom(new HatLayout(bytes))
            .Where(hotspot => hotspot.Action is SceneNavigationAction.Farm
                or SceneNavigationAction.Village or SceneNavigationAction.Forest)
            .ToArray();

        Assert.Equal(
            [(SceneNavigationAction.Farm, 2, new UiBounds(50, 102, 13, 23)),
             (SceneNavigationAction.Village, 3, new UiBounds(70, 103, 14, 24)),
             (SceneNavigationAction.Forest, 4, new UiBounds(90, 104, 15, 25))],
            books.Select(book => (book.Action, book.HatRegionId, book.Bounds)));
    }

    [Fact]
    public void InnPatronHotspotsAndFooterComeFromTheOriginalDescriptor()
    {
        var bytes = new byte[40 + 12 * 24];
        WriteInt(bytes, 12, 640); WriteInt(bytes, 16, 480); WriteInt(bytes, 20, 12);
        for (var id = 0; id < 12; id++)
        {
            var offset = 40 + id * 24;
            WriteInt(bytes, offset, id);
            WriteInt(bytes, offset + 4, 5 + id * 10);
            WriteInt(bytes, offset + 8, 20 + id);
            WriteInt(bytes, offset + 12, 30 + id);
            WriteInt(bytes, offset + 16, 40 + id);
            WriteInt(bytes, offset + 20, 1);
        }

        var layout = InnPresentationDefinitions.From(new HatLayout(bytes));

        Assert.Equal(InnPresentationDefinitions.PatronCount, layout.Patrons.Count);
        Assert.Equal(Enumerable.Range(0, 10), layout.Patrons.Select(patron => patron.HatRegionId));
        Assert.Equal(["Frederick", "Gerard", "Barkeep", "Otto", "Hugh", "Gilbert", "Nellie", "Richard", "Ivo", "Albert"],
            layout.Patrons.Select(patron => patron.Name));
        Assert.Equal([1900, 1100, 3200, 3500, 1600, 1400, 3000, 1698, 3300, 3600],
            layout.Patrons.Select(patron => patron.ConversationRootNodeId));
        Assert.All(layout.Patrons, patron => Assert.Contains(ImportedArt.Definitions,
            definition => definition.Role == patron.PortraitRole && definition.IdSuffix == patron.PortraitSuffix));
        Assert.Equal(new UiBounds(75, 27, 37, 47), layout.Patrons[7].Bounds);
        Assert.Equal(new UiBounds(105, 30, 40, 50), layout.ExitBounds);
        Assert.Equal(new UiBounds(115, 31, 41, 51), layout.HoverLabelBounds);
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
            campaign.State.Player.AdditionalArmies[1].Name = "March Wardens";
            campaign.State.Player.AdditionalArmies[1].Force.Units[UnitType.Knights] = 200;
            campaign.State.Player.AdditionalArmies[1].IsFielded = true;
            campaign.State.Player.JoinedArmyIndex = 2;
            campaign.State.Player.ActiveSpies = 3;
            slots.Save(campaign, 3);

            var third = slots.Inspect(3);
            Assert.True(third.IsValid);
            Assert.Equal(campaign.State.Player.Name, third.PlayerName);
            Assert.Equal(campaign.State.Date, third.CampaignDate);
            Assert.True(slots.TryLoad(3, out var loaded, out var error));
            Assert.Null(error);
            Assert.Equal(campaign.State.Player.Name, loaded!.State.Player.Name);
            Assert.Equal(("March Wardens", 200, true, 2, 3),
                (loaded.State.Player.AdditionalArmies[1].Name,
                 loaded.State.Player.AdditionalArmies[1].Force.Units[UnitType.Knights],
                 loaded.State.Player.AdditionalArmies[1].IsFielded,
                 loaded.State.Player.JoinedArmyIndex,
                 loaded.State.Player.ActiveSpies));

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
    public void SaveSlotsVersionAtomicWritesAndRecoverThePreviousSave()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-recovery-{Guid.NewGuid():N}");
        try
        {
            var slots = new CampaignSaveSlots(root);
            var campaign = new Campaign(Campaign.NewFromTemplate(0));
            campaign.State.Player.Wealth = 111;
            slots.Save(campaign, 4);
            Assert.Contains($"\"SchemaVersion\": {Campaign.CurrentSaveSchemaVersion}", File.ReadAllText(slots.SlotPath(4)));

            campaign.State.Player.Wealth = 222;
            slots.Save(campaign, 4);
            Assert.True(File.Exists(slots.BackupPath(4)));
            File.WriteAllText(slots.SlotPath(4), "interrupted");

            var info = slots.Inspect(4);
            Assert.True(info.IsValid);
            Assert.True(info.RecoveredFromBackup);
            Assert.True(slots.TryLoad(4, out var recovered, out var error));
            Assert.Null(error);
            Assert.Equal(111, recovered!.State.Player.Wealth);
            Assert.Empty(Directory.EnumerateFiles(root, "*.tmp"));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    [Fact]
    public void UnversionedSavesMigrateAndFutureSchemasAreRejected()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-schema-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(root);
            var legacyPath = Path.Combine(root, "legacy.json");
            File.WriteAllText(legacyPath, System.Text.Json.JsonSerializer.Serialize(Campaign.NewFromTemplate(0)));
            Assert.Equal(Campaign.CurrentSaveSchemaVersion, Campaign.Load(legacyPath).State.SchemaVersion);

            var future = Campaign.NewFromTemplate(0);
            future.SchemaVersion = Campaign.CurrentSaveSchemaVersion + 1;
            var futurePath = Path.Combine(root, "future.json");
            File.WriteAllText(futurePath, System.Text.Json.JsonSerializer.Serialize(future));
            Assert.Throws<InvalidDataException>(() => Campaign.Load(futurePath));
        }
        finally
        {
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
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
