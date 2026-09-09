using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public sealed class ImportedConversationActionState(CampaignState campaign) : IDynamixActionState
{
    public void Initialize(IReadOnlyList<int> initialValues)
    {
        if (campaign.ConversationVariables.Count == 0)
            campaign.ConversationVariables.AddRange(initialValues);
    }

    public bool TryGetVariable(int scope, int index, out int value)
    {
        value = 0;
        if (index < 0) return false;
        if (scope == 0)
        {
            if (index >= campaign.ConversationVariables.Count) return false;
            value = campaign.ConversationVariables[index];
            return true;
        }
        if (scope != 1) return false;
        if (index == 0)
        {
            value = campaign.Player.Wealth;
            return true;
        }
        value = campaign.ConversationAttributes.GetValueOrDefault(index);
        return true;
    }

    public bool TrySetVariable(int scope, int index, int value)
    {
        if (index < 0) return false;
        if (scope == 0)
        {
            if (index >= campaign.ConversationVariables.Count) return false;
            campaign.ConversationVariables[index] = value;
            return true;
        }
        if (scope != 1) return false;
        if (index == 0)
        {
            campaign.Player.Wealth = Math.Max(0, value);
            return true;
        }
        campaign.ConversationAttributes[index] = value;
        return true;
    }

    public bool TryAddItem(int index)
    {
        if (index < 0) return false;
        campaign.ConversationItems[index] = unchecked(campaign.ConversationItems.GetValueOrDefault(index) + 1);
        return true;
    }

    public bool TryClearItem(int index)
    {
        if (index < 0) return false;
        campaign.ConversationItems.Remove(index);
        return true;
    }

    public bool HasItem(int index) => index >= 0 && campaign.ConversationItems.GetValueOrDefault(index) != 0;
}
