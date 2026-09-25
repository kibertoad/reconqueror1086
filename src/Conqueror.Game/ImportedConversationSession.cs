using Conqueror.Resources;

namespace Conqueror.Game;

/// <summary>Traverses decoded original conversation links while keeping presentation selection deterministic in tests.</summary>
public sealed class ImportedConversationSession(
    DynamixConversationDatabase database,
    DynamixActionInterpreter? actions = null)
{
    public DynamixConversationNode? CurrentNode { get; private set; }
    public int PromptVariantIndex { get; private set; }
    public string? Prompt => CurrentNode is null ? null : CurrentNode.PromptVariants[PromptVariantIndex];
    public bool IsComplete => CurrentNode is null;

    public bool Start(int rootNodeId, Func<int, int> choosePrompt) => MoveTo(rootNodeId, choosePrompt);

    public bool ChooseResponse(int responseIndex, Func<int, int> choosePrompt)
    {
        var node = CurrentNode ?? throw new InvalidOperationException("Conversation is not active.");
        if ((uint)responseIndex >= (uint)node.Responses.Count)
            throw new ArgumentOutOfRangeException(nameof(responseIndex));
        var response = node.Responses[responseIndex];
        var result = actions?.Execute(response.ActionIds);
        return MoveTo(result?.RedirectNodeId ?? response.TargetNodeId, choosePrompt);
    }

    public bool Continue(Func<int, int> choosePrompt)
    {
        var node = CurrentNode ?? throw new InvalidOperationException("Conversation is not active.");
        if (node.Responses.Count != 0)
            throw new InvalidOperationException("A response must be selected before this conversation can continue.");
        return MoveTo(node.ContinuationNodeId ?? 0, choosePrompt);
    }

    // PLACEHOLDER: RULE-TALK-001. The original runs a node's actions after the player answers it, then
    // the response actions, and only then follows a redirect; running them on arrival is a guess.
    private bool MoveTo(int nodeId, Func<int, int> choosePrompt)
    {
        ArgumentNullException.ThrowIfNull(choosePrompt);
        var visited = new HashSet<int>();
        while (nodeId != 0)
        {
            if (!visited.Add(nodeId)) throw new InvalidDataException("Conversation continuation cycle has no visible node.");
            var node = database.Find(nodeId)
                ?? throw new InvalidDataException($"Conversation node {nodeId} is missing.");
            var result = actions?.Execute(node.ActionIds);
            if (result?.RedirectNodeId is { } redirect)
            {
                nodeId = redirect;
                continue;
            }
            if (node.PromptVariants.Count != 0)
            {
                var selected = choosePrompt(node.PromptVariants.Count);
                if ((uint)selected >= (uint)node.PromptVariants.Count)
                    throw new InvalidDataException("Conversation prompt selector returned an invalid variant.");
                CurrentNode = node;
                PromptVariantIndex = selected;
                return true;
            }
            nodeId = node.ContinuationNodeId ?? 0;
        }
        CurrentNode = null;
        PromptVariantIndex = 0;
        return false;
    }
}
