namespace Conqueror.Resources;

public interface IDynamixActionState
{
    bool TryGetVariable(int scope, int index, out int value);
    bool TrySetVariable(int scope, int index, int value);
    bool TryAddItem(int index);
    bool TryClearItem(int index);
    bool HasItem(int index);
}

public sealed record DynamixActionExecutionResult(bool Success, int? RedirectNodeId);

// RULE-TALK-002, RULE-TALK-003.
/// <summary>Executes the expression and branch grammar used by ALL.TMB.</summary>
public sealed class DynamixActionInterpreter(DynamixActionTreeDatabase database, IDynamixActionState state)
{
    private const int MaximumExecutionDepth = 256;

    public DynamixActionExecutionResult Execute(IEnumerable<int> groupIds)
    {
        ArgumentNullException.ThrowIfNull(groupIds);
        int? redirect = null;
        var success = true;
        foreach (var id in groupIds)
        {
            var group = database.Find(id);
            if (group is null)
            {
                success = false;
                continue;
            }
            foreach (var actionOffset in group.ActionOffsets)
                success &= ExecuteAction(actionOffset, ref redirect, 0);
        }
        return new DynamixActionExecutionResult(success, redirect);
    }

    private bool ExecuteAction(int offset, ref int? redirect, int depth)
    {
        if (depth > MaximumExecutionDepth) throw new InvalidDataException("Action execution exceeds the depth limit.");
        var action = database.Actions[offset];
        var value = EvaluateExpression(action.ExpressionOffset, ref redirect, depth + 1);
        return action.Kind switch
        {
            DynamixActionKind.Evaluate => true,
            DynamixActionKind.When => value == 0 || ExecuteAction(action.BranchActionOffsets[0], ref redirect, depth + 1),
            DynamixActionKind.IfElse => ExecuteAction(action.BranchActionOffsets[value != 0 ? 0 : 1], ref redirect, depth + 1),
            _ => false
        };
    }

    private int EvaluateExpression(int offset, ref int? redirect, int depth)
    {
        if (depth > MaximumExecutionDepth) throw new InvalidDataException("Expression execution exceeds the depth limit.");
        var expression = database.Expressions[offset];
        var result = EvaluateValue(expression.ValueOffsets[0], ref redirect, depth + 1);
        for (var index = 0; index < expression.Operators.Count; index++)
        {
            var right = EvaluateValue(expression.ValueOffsets[index + 1], ref redirect, depth + 1);
            result = expression.Operators[index] switch
            {
                DynamixExpressionOperator.NotEqual => result != right ? 1 : 0,
                DynamixExpressionOperator.LessThanOrEqual => result <= right ? 1 : 0,
                DynamixExpressionOperator.GreaterThanOrEqual => result >= right ? 1 : 0,
                DynamixExpressionOperator.And => result != 0 && right != 0 ? 1 : 0,
                DynamixExpressionOperator.Or => result != 0 || right != 0 ? 1 : 0,
                DynamixExpressionOperator.GreaterThan => result > right ? 1 : 0,
                DynamixExpressionOperator.LessThan => result < right ? 1 : 0,
                DynamixExpressionOperator.Equal => result == right ? 1 : 0,
                _ => throw new InvalidDataException("Action expression operator is invalid.")
            };
        }
        return result;
    }

    private int EvaluateValue(int offset, ref int? redirect, int depth)
    {
        if (depth > MaximumExecutionDepth) throw new InvalidDataException("Value execution exceeds the depth limit.");
        var value = database.Values[offset];
        var result = value.Kind switch
        {
            DynamixValueKind.Expression => EvaluateExpression(value.Value, ref redirect, depth + 1),
            DynamixValueKind.Literal => value.Value,
            DynamixValueKind.Function => EvaluateFunctionValue(value, ref redirect, depth),
            _ => throw new InvalidDataException("Action value kind is invalid.")
        };
        return value.Invert ? result == 0 ? 1 : 0 : result;
    }

    private int EvaluateFunctionValue(DynamixValueNode value, ref int? redirect, int depth)
    {
        var arguments = new int[value.ArgumentExpressionOffsets.Count];
        for (var index = 0; index < arguments.Length; index++)
            arguments[index] = EvaluateExpression(value.ArgumentExpressionOffsets[index], ref redirect, depth + 1);
        return EvaluateFunction(value.Value, arguments, ref redirect);
    }

    private int EvaluateFunction(int id, int[] arguments, ref int? redirect)
    {
        switch (id)
        {
            case 3 when arguments.Length == 1:
                redirect = arguments[0];
                return 1;
            case 4 when arguments.Length == 3:
                if (arguments[2] == int.MinValue) return 0;
                if (!state.TrySetVariable(arguments[0], arguments[1], arguments[2])) return 0;
                return arguments[0] == 0 ? 1 : 0;
            case 5 when arguments.Length == 3:
                if (!state.TryGetVariable(arguments[0], arguments[1], out var current)) return 0;
                return state.TrySetVariable(arguments[0], arguments[1], unchecked(current + arguments[2])) ? 1 : 0;
            case 6 when arguments.Length == 2:
                return state.TryGetVariable(arguments[0], arguments[1], out var value) ? value : 0;
            case 7 when arguments.Length == 2:
                return state.TryAddItem(arguments[1]) ? 1 : 0;
            case 8 when arguments.Length == 2:
                return state.TryClearItem(arguments[1]) ? 1 : 0;
            case 9 when arguments.Length == 2:
                return state.HasItem(arguments[1]) ? 1 : 0;
            // PLACEHOLDER: RULE-TALK-003. The original fails the action on an unknown function;
            // here it reads 0 and the action goes on.
            default:
                return 0;
        }
    }
}
