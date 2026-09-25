using System.Buffers.Binary;
using System.Text;

namespace Conqueror.Resources;

public sealed record DynamixConversationResponse(
    string Text,
    int TargetNodeId,
    IReadOnlyList<int> ActionIds);

public sealed record DynamixConversationNode(
    int Id,
    int SourceOffset,
    string? PortraitFile,
    string? Speaker,
    IReadOnlyList<string> PromptVariants,
    IReadOnlyList<DynamixConversationResponse> Responses,
    int? ContinuationNodeId,
    IReadOnlyList<int> ActionIds);

public sealed class DynamixConversationDatabase(IReadOnlyDictionary<int, DynamixConversationNode> nodes)
{
    public IReadOnlyDictionary<int, DynamixConversationNode> Nodes { get; } = nodes;
    public DynamixConversationNode? Find(int id) => Nodes.GetValueOrDefault(id);
}

// FMT-TALK-001, FMT-TALK-002. The strings are split at NULs; the original reads them by the header lengths.
/// <summary>Bounded decoder for the original ALL.CBF conversation records and ALL.CIF node index.</summary>
public static class DynamixConversationDecoder
{
    private const int IndexRecordSize = 8;
    private const int NodeHeaderSize = 0x348;
    private const int PromptVariantCountOffset = 0x08;
    private const int ResponseCountOffset = 0x48;
    private const int ResponseTargetsOffset = 0x4c;
    private const int ResponseActionOffset = 0x60;
    private const int ResponseSlotSize = 0x78;
    private const int NodeActionOffset = 0x2b8;
    private const int ActionSlotCount = 30;
    private const int MaximumNodes = 100_000;
    private const int MaximumResponses = 5;

    public static DynamixConversationDatabase Decode(ReadOnlySpan<byte> body, ReadOnlySpan<byte> index)
    {
        if (index.Length == 0 || index.Length % IndexRecordSize != 0)
            throw new InvalidDataException("Conversation index length is not a non-empty sequence of records.");
        var count = index.Length / IndexRecordSize;
        if (count > MaximumNodes) throw new InvalidDataException("Conversation index exceeds the node limit.");

        var entries = new (int Id, int Offset)[count];
        var ids = new HashSet<int>();
        var offsets = new HashSet<int>();
        for (var i = 0; i < count; i++)
        {
            var record = index.Slice(i * IndexRecordSize, IndexRecordSize);
            var id = BinaryPrimitives.ReadInt32LittleEndian(record);
            var offset = BinaryPrimitives.ReadInt32LittleEndian(record[4..]);
            if (id < 0 || !ids.Add(id)) throw new InvalidDataException("Conversation node identifiers must be unique and non-negative.");
            if (offset < 0 || offset > body.Length - NodeHeaderSize || !offsets.Add(offset))
                throw new InvalidDataException("Conversation node offsets must be unique and bound a complete header.");
            entries[i] = (id, offset);
        }

        var byOffset = entries.OrderBy(entry => entry.Offset).ToArray();
        if (byOffset[0].Offset != 0) throw new InvalidDataException("Conversation records do not begin at body offset zero.");
        var nodes = new Dictionary<int, DynamixConversationNode>(count);
        for (var i = 0; i < byOffset.Length; i++)
        {
            var entry = byOffset[i];
            var end = i + 1 < byOffset.Length ? byOffset[i + 1].Offset : body.Length;
            if (end - entry.Offset < NodeHeaderSize) throw new InvalidDataException("Conversation record is shorter than its fixed header.");
            var record = body.Slice(entry.Offset, end - entry.Offset);
            if (record[ResponseCountOffset + 1] != 0x65
                || record[ResponseCountOffset + 2] != 0x3a
                || record[ResponseCountOffset + 3] != 0x5c)
                throw new InvalidDataException("Conversation record marker is invalid.");
            var responseCount = record[ResponseCountOffset];
            if (responseCount > MaximumResponses) throw new InvalidDataException("Conversation response count exceeds the format limit.");
            int? continuationNodeId = responseCount == 0
                ? BinaryPrimitives.ReadInt32LittleEndian(record.Slice(ResponseTargetsOffset, 4))
                : null;

            var targets = new int[responseCount];
            for (var response = 0; response < responseCount; response++)
                targets[response] = BinaryPrimitives.ReadInt32LittleEndian(
                    record.Slice(ResponseTargetsOffset + response * 4, 4));

            var strings = DecodeStrings(record[NodeHeaderSize..]);
            var nodeActions = DecodeActionIds(record.Slice(NodeActionOffset, ActionSlotCount * 4));
            if (strings.Count == 0)
            {
                if (responseCount != 0) throw new InvalidDataException("Empty conversation node declares responses.");
                if (record[PromptVariantCountOffset] != 0)
                    throw new InvalidDataException("Empty conversation node declares prompt variants.");
                nodes.Add(entry.Id, new(entry.Id, entry.Offset, null, null, [], [], continuationNodeId, nodeActions));
                continue;
            }
            if (strings.Count < 3 + responseCount)
                throw new InvalidDataException("Conversation node lacks a portrait, speaker, prompt, or response label.");

            var promptCount = strings.Count - 2 - responseCount;
            if (record[PromptVariantCountOffset] != promptCount)
                throw new InvalidDataException("Conversation prompt-variant count does not match its string table.");
            var responses = new DynamixConversationResponse[responseCount];
            for (var response = 0; response < responseCount; response++)
                responses[response] = new DynamixConversationResponse(
                    strings[2 + promptCount + response],
                    targets[response],
                    DecodeActionIds(record.Slice(
                        ResponseActionOffset + response * ResponseSlotSize, ActionSlotCount * 4)));
            nodes.Add(entry.Id, new(entry.Id, entry.Offset, strings[0], strings[1],
                strings.Skip(2).Take(promptCount).ToArray(), responses, continuationNodeId, nodeActions));
        }

        foreach (var node in nodes.Values)
        {
            if (node.ContinuationNodeId is not (null or 0) && !nodes.ContainsKey(node.ContinuationNodeId.Value))
                throw new InvalidDataException("Conversation continuation targets an unknown node.");
            foreach (var response in node.Responses)
                if (response.TargetNodeId != 0 && !nodes.ContainsKey(response.TargetNodeId))
                    throw new InvalidDataException("Conversation response targets an unknown node.");
        }

        return new DynamixConversationDatabase(nodes);
    }

    private static IReadOnlyList<int> DecodeActionIds(ReadOnlySpan<byte> source)
    {
        var actions = new List<int>();
        for (var offset = 0; offset < source.Length; offset += 4)
        {
            var id = BinaryPrimitives.ReadInt32LittleEndian(source.Slice(offset, 4));
            if (id == -1) continue;
            if (id < 0) throw new InvalidDataException("Conversation action identifier is invalid.");
            actions.Add(id);
        }
        return actions;
    }

    private static IReadOnlyList<string> DecodeStrings(ReadOnlySpan<byte> source)
    {
        if (source.IsEmpty) return [];
        if (source[^1] != 0) throw new InvalidDataException("Conversation string table is not terminated.");
        var strings = new List<string>();
        var position = 0;
        while (position < source.Length)
        {
            var terminator = source[position..].IndexOf((byte)0);
            if (terminator < 0) throw new InvalidDataException("Conversation string is not terminated.");
            var bytes = source.Slice(position, terminator);
            if (!bytes.IsEmpty)
            {
                foreach (var value in bytes)
                    if (value is not (9 or 10 or 13) && value is < 0x20 or > 0x7e)
                        throw new InvalidDataException("Conversation text contains a non-ASCII control or character.");
                strings.Add(Encoding.ASCII.GetString(bytes));
            }
            position += terminator + 1;
        }
        return strings;
    }
}
