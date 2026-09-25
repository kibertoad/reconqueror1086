using System.Buffers.Binary;

namespace Conqueror.Resources;

public enum DynamixExpressionOperator
{
    NotEqual = 1,
    LessThanOrEqual = 2,
    GreaterThanOrEqual = 3,
    And = 4,
    Or = 5,
    GreaterThan = 6,
    LessThan = 7,
    Equal = 8
}

public enum DynamixValueKind { Expression = 1, Literal = 2, Function = 3 }
public enum DynamixActionKind { When = 1, IfElse = 2, Evaluate = 3 }

public sealed record DynamixValueNode(
    int SourceOffset,
    DynamixValueKind Kind,
    int Value,
    bool Invert,
    IReadOnlyList<int> ArgumentExpressionOffsets);

public sealed record DynamixExpressionNode(
    int SourceOffset,
    IReadOnlyList<int> ValueOffsets,
    IReadOnlyList<DynamixExpressionOperator> Operators);

public sealed record DynamixActionNode(
    int SourceOffset,
    DynamixActionKind Kind,
    int ExpressionOffset,
    IReadOnlyList<int> BranchActionOffsets);

public sealed record DynamixActionGroup(
    int Id,
    int SourceOffset,
    IReadOnlyList<int> ActionOffsets);

public sealed class DynamixActionTreeDatabase(
    int indexHeaderValue,
    IReadOnlyDictionary<int, DynamixActionGroup> groups,
    IReadOnlyDictionary<int, DynamixActionNode> actions,
    IReadOnlyDictionary<int, DynamixExpressionNode> expressions,
    IReadOnlyDictionary<int, DynamixValueNode> values)
{
    public int IndexHeaderValue { get; } = indexHeaderValue;
    public IReadOnlyDictionary<int, DynamixActionGroup> Groups { get; } = groups;
    public IReadOnlyDictionary<int, DynamixActionNode> Actions { get; } = actions;
    public IReadOnlyDictionary<int, DynamixExpressionNode> Expressions { get; } = expressions;
    public IReadOnlyDictionary<int, DynamixValueNode> Values { get; } = values;
    public DynamixActionGroup? Find(int id) => Groups.GetValueOrDefault(id);
}

// FMT-TALK-003 to FMT-TALK-007.
/// <summary>Bounded structural decoder for the original ALL.TMI/ALL.TMB recursive action trees.</summary>
public static class DynamixActionTreeDecoder
{
    private const int IndexHeaderSize = 4;
    private const int IndexRecordSize = 8;
    private const int MaximumGroups = 100_000;
    private const int MaximumGroupActions = 1_024;
    private const int MaximumExpressionValues = 512;
    private const int MaximumFunctionArguments = 32;
    private const int MaximumDepth = 256;

    public static DynamixActionTreeDatabase Decode(ReadOnlyMemory<byte> body, ReadOnlySpan<byte> index)
    {
        if (index.Length < IndexHeaderSize || (index.Length - IndexHeaderSize) % IndexRecordSize != 0)
            throw new InvalidDataException("Action-tree index has an invalid length.");
        var headerValue = ReadInt(index, 0, "Action-tree index header is truncated.");
        var actualCount = (index.Length - IndexHeaderSize) / IndexRecordSize;
        if (actualCount is < 1 or > MaximumGroups)
            throw new InvalidDataException("Action-tree index count is invalid.");

        var entries = new (int Id, int Offset)[actualCount];
        var ids = new HashSet<int>();
        for (var i = 0; i < actualCount; i++)
        {
            var offset = IndexHeaderSize + i * IndexRecordSize;
            var id = ReadInt(index, offset, "Action-tree index record is truncated.");
            var bodyOffset = ReadInt(index, offset + 4, "Action-tree index record is truncated.");
            if (id < 1 || !ids.Add(id))
                throw new InvalidDataException("Action-tree identifiers must be unique and positive.");
            ValidateOffset(body.Span, bodyOffset, 4, "Action-tree group offset is invalid.");
            entries[i] = (id, bodyOffset);
        }

        var parser = new Parser(body);
        var groups = new Dictionary<int, DynamixActionGroup>(actualCount);
        foreach (var entry in entries)
            groups.Add(entry.Id, parser.ParseGroup(entry.Id, entry.Offset));
        return new DynamixActionTreeDatabase(headerValue, groups, parser.Actions, parser.Expressions, parser.Values);
    }

    private sealed class Parser(ReadOnlyMemory<byte> body)
    {
        private readonly HashSet<(char Kind, int Offset)> _active = [];
        public Dictionary<int, DynamixActionNode> Actions { get; } = [];
        public Dictionary<int, DynamixExpressionNode> Expressions { get; } = [];
        public Dictionary<int, DynamixValueNode> Values { get; } = [];

        public DynamixActionGroup ParseGroup(int id, int offset)
        {
            var count = ReadCount(offset, MaximumGroupActions, "Action-tree group action count is invalid.");
            ValidateOffset(body.Span, offset + 4, checked(count * 4), "Action-tree group actions are truncated.");
            var actionOffsets = ReadOffsets(offset + 4, count);
            foreach (var actionOffset in actionOffsets) ParseAction(actionOffset, 0);
            return new DynamixActionGroup(id, offset, actionOffsets);
        }

        private DynamixActionNode ParseAction(int offset, int depth)
        {
            if (Actions.TryGetValue(offset, out var cached)) return cached;
            Enter('A', offset, depth);
            try
            {
                ValidateOffset(body.Span, offset, 12, "Action-tree action record is truncated.");
                var kindValue = ReadBodyInt(offset);
                if (!Enum.IsDefined(typeof(DynamixActionKind), kindValue))
                    throw new InvalidDataException("Action-tree action kind is invalid.");
                var kind = (DynamixActionKind)kindValue;
                var expressionOffset = ReadBodyInt(offset + 4);
                var branchCount = ReadBodyInt(offset + 8);
                var expectedBranches = kind switch
                {
                    DynamixActionKind.When => 1,
                    DynamixActionKind.IfElse => 2,
                    DynamixActionKind.Evaluate => 0,
                    _ => throw new InvalidDataException("Action-tree action kind is invalid.")
                };
                if (branchCount != expectedBranches)
                    throw new InvalidDataException("Action-tree action branch count does not match its kind.");
                ValidateOffset(body.Span, offset + 12, checked(branchCount * 4), "Action-tree action branches are truncated.");
                var branchOffsets = ReadOffsets(offset + 12, branchCount);
                ParseExpression(expressionOffset, depth + 1);
                foreach (var branchOffset in branchOffsets) ParseAction(branchOffset, depth + 1);
                var result = new DynamixActionNode(offset, kind, expressionOffset, branchOffsets);
                Actions.Add(offset, result);
                return result;
            }
            finally { Exit('A', offset); }
        }

        private DynamixExpressionNode ParseExpression(int offset, int depth)
        {
            if (Expressions.TryGetValue(offset, out var cached)) return cached;
            Enter('E', offset, depth);
            try
            {
                ValidateOffset(body.Span, offset, 8, "Action-tree expression header is truncated.");
                var valueCount = ReadBodyInt(offset);
                var operatorCount = ReadBodyInt(offset + 4);
                if (valueCount is < 1 or > MaximumExpressionValues || operatorCount != valueCount - 1)
                    throw new InvalidDataException("Action-tree expression arity is invalid.");
                var payloadBytes = checked((valueCount + operatorCount) * 4);
                ValidateOffset(body.Span, offset + 8, payloadBytes, "Action-tree expression payload is truncated.");
                var valueOffsets = ReadOffsets(offset + 8, valueCount);
                var operators = new DynamixExpressionOperator[operatorCount];
                for (var i = 0; i < operatorCount; i++)
                {
                    var value = ReadBodyInt(offset + 8 + valueCount * 4 + i * 4);
                    if (!Enum.IsDefined(typeof(DynamixExpressionOperator), value))
                        throw new InvalidDataException("Action-tree expression operator is invalid.");
                    operators[i] = (DynamixExpressionOperator)value;
                }
                foreach (var valueOffset in valueOffsets) ParseValue(valueOffset, depth + 1);
                var result = new DynamixExpressionNode(offset, valueOffsets, operators);
                Expressions.Add(offset, result);
                return result;
            }
            finally { Exit('E', offset); }
        }

        private DynamixValueNode ParseValue(int offset, int depth)
        {
            if (Values.TryGetValue(offset, out var cached)) return cached;
            Enter('V', offset, depth);
            try
            {
                ValidateOffset(body.Span, offset, 16, "Action-tree value record is truncated.");
                var kindValue = ReadBodyInt(offset);
                if (!Enum.IsDefined(typeof(DynamixValueKind), kindValue))
                    throw new InvalidDataException("Action-tree value kind is invalid.");
                var kind = (DynamixValueKind)kindValue;
                var value = ReadBodyInt(offset + 4);
                var invertValue = ReadBodyInt(offset + 8);
                if (invertValue is not (-1 or 1))
                    throw new InvalidDataException("Action-tree value inversion flag is invalid.");
                var argumentCount = ReadBodyInt(offset + 12);
                if (kind != DynamixValueKind.Function && argumentCount != 0)
                    throw new InvalidDataException("Non-function action-tree value declares arguments.");
                if (argumentCount is < 0 or > MaximumFunctionArguments)
                    throw new InvalidDataException("Action-tree function argument count is invalid.");
                ValidateOffset(body.Span, offset + 16, checked(argumentCount * 4), "Action-tree function arguments are truncated.");
                var arguments = ReadOffsets(offset + 16, argumentCount);
                if (kind == DynamixValueKind.Expression) ParseExpression(value, depth + 1);
                foreach (var argument in arguments) ParseExpression(argument, depth + 1);
                var result = new DynamixValueNode(offset, kind, value, invertValue == 1, arguments);
                Values.Add(offset, result);
                return result;
            }
            finally { Exit('V', offset); }
        }

        private int ReadCount(int offset, int maximum, string message)
        {
            ValidateOffset(body.Span, offset, 4, message);
            var count = ReadBodyInt(offset);
            if (count is < 0 || count > maximum) throw new InvalidDataException(message);
            return count;
        }

        private int[] ReadOffsets(int offset, int count)
        {
            var offsets = new int[count];
            for (var i = 0; i < count; i++)
            {
                offsets[i] = ReadBodyInt(offset + i * 4);
                ValidateOffset(body.Span, offsets[i], 4, "Action-tree child offset is invalid.");
            }
            return offsets;
        }

        private int ReadBodyInt(int offset) => BinaryPrimitives.ReadInt32LittleEndian(body.Span.Slice(offset, 4));

        private void Enter(char kind, int offset, int depth)
        {
            if (depth > MaximumDepth) throw new InvalidDataException("Action-tree nesting exceeds the depth limit.");
            if (!_active.Add((kind, offset))) throw new InvalidDataException("Action-tree graph contains a recursive cycle.");
        }

        private void Exit(char kind, int offset) => _active.Remove((kind, offset));
    }

    private static int ReadInt(ReadOnlySpan<byte> source, int offset, string message)
    {
        if ((uint)offset > (uint)source.Length || source.Length - offset < 4)
            throw new InvalidDataException(message);
        return BinaryPrimitives.ReadInt32LittleEndian(source.Slice(offset, 4));
    }

    private static void ValidateOffset(ReadOnlySpan<byte> body, int offset, int length, string message)
    {
        if (offset < 0 || length < 0 || offset > body.Length - length)
            throw new InvalidDataException(message);
    }
}
