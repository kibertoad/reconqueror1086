using Conqueror.Core;
using Conqueror.Resources;

namespace Conqueror.Game;

public static class OriginalConversationBindings
{
    public const int DragonLairDiscoveryVariable = OriginalCampaignVariables.DragonLairDiscovery;
    public const int JoustOutcomeVariable = OriginalCampaignVariables.JoustOutcome;
    public const int LadyColorsVariable = OriginalCampaignVariables.LadyColors;

    public static readonly IReadOnlyDictionary<int, string> LadyColors = new Dictionary<int, string>
    {
        [1] = "Wendessa",
        [2] = "Victoria",
        [3] = "Anna Lisa",
        [4] = "Valetta",
        [5] = "Jane"
    };

    public static readonly IReadOnlyDictionary<int, CharacterAttribute> Attributes =
        new Dictionary<int, CharacterAttribute>
        {
            [0] = CharacterAttribute.Wealth,
            [2] = CharacterAttribute.Honor,
            [3] = CharacterAttribute.Fame,
            [5] = CharacterAttribute.Piety,
            [6] = CharacterAttribute.Strength,
            [7] = CharacterAttribute.Stamina,
            [8] = CharacterAttribute.Intelligence
        };

    public static readonly IReadOnlyDictionary<int, string> Items = new Dictionary<int, string>
    {
        [0] = "Holy Chalice",
        [1] = "Dagger",
        [2] = "Sword",
        [3] = "Hammer",
        [4] = "Flange",
        [5] = "Medallion",
        [6] = "Dagger",
        [7] = "Knight's Sword",
        [8] = "Hammer",
        [9] = "Lance",
        [10] = "Dragon Slaying Lance",
        [11] = "Proclamation",
        [12] = "Dagger",
        [13] = "Sword",
        [14] = "Dragon Stone",
        [15] = "Shield of St. George",
        [16] = "Ax",
        [17] = "Arm Band",
        [18] = "Sword",
        [19] = "Orchid of Essex",
        [20] = "Dragon Slaying Armor",
        [21] = "Book of Hours",
        [22] = "Title To Armor",
        [23] = "Note From Gilbert"
    };

    public static void SynchronizeTournamentState(CampaignState campaign)
    {
        if (campaign.ConversationVariables.Count <= LadyColorsVariable) return;
        campaign.Player.LadyColors = LadyColors.GetValueOrDefault(
            campaign.ConversationVariables[LadyColorsVariable]);
    }

    public static void RecordJoustResult(CampaignState campaign, string? lady, bool won)
    {
        if (!RecordLadyColors(campaign, lady)) return;
        campaign.ConversationVariables[JoustOutcomeVariable] = won ? 2 : 1;
    }

    public static bool RecordLadyColors(CampaignState campaign, string? lady)
    {
        if (lady is null || campaign.ConversationVariables.Count <= LadyColorsVariable) return false;
        var color = LadyColors.FirstOrDefault(entry => entry.Value.Equals(lady, StringComparison.OrdinalIgnoreCase)).Key;
        if (color == 0) return false;
        campaign.ConversationVariables[LadyColorsVariable] = color;
        return true;
    }
}

public sealed class ImportedConversationActionState(CampaignState campaign) : IDynamixActionState
{
    public void Initialize(IReadOnlyList<int> initialValues)
    {
        if (campaign.ConversationVariables.Count == 0)
            campaign.ConversationVariables.AddRange(initialValues);
        foreach (var item in campaign.ConversationItems.Where(entry => entry.Value > 0))
            if (OriginalConversationBindings.Items.TryGetValue(item.Key, out var name))
                campaign.Player.Inventory.Items.Add(name);
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
        if (OriginalConversationBindings.Attributes.TryGetValue(index, out var attribute))
        {
            value = CharacterAttributes.Read(campaign.Player, attribute);
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
        if (OriginalConversationBindings.Attributes.TryGetValue(index, out var attribute))
        {
            SetAttribute(attribute, value);
            return true;
        }
        campaign.ConversationAttributes[index] = value;
        return true;
    }

    public bool TryAddItem(int index)
    {
        if (index < 0) return false;
        campaign.ConversationItems[index] = unchecked(campaign.ConversationItems.GetValueOrDefault(index) + 1);
        if (OriginalConversationBindings.Items.TryGetValue(index, out var name))
            campaign.Player.Inventory.Items.Add(name);
        return true;
    }

    public bool TryClearItem(int index)
    {
        if (index < 0) return false;
        campaign.ConversationItems.Remove(index);
        if (OriginalConversationBindings.Items.TryGetValue(index, out var name)
            && !campaign.ConversationItems.Any(entry => entry.Value > 0
                && OriginalConversationBindings.Items.GetValueOrDefault(entry.Key) == name))
            campaign.Player.Inventory.Items.Remove(name);
        return true;
    }

    public bool HasItem(int index) => index >= 0 && (campaign.ConversationItems.GetValueOrDefault(index) != 0
        || OriginalConversationBindings.Items.TryGetValue(index, out var name)
            && campaign.Player.Inventory.Items.Contains(name));

    private void SetAttribute(CharacterAttribute attribute, int value)
    {
        var player = campaign.Player;
        value = attribute == CharacterAttribute.Wealth ? Math.Max(0, value) : Math.Clamp(value, 0, 20);
        player.Stats = attribute switch
        {
            CharacterAttribute.Strength => player.Stats with { Strength = value },
            CharacterAttribute.Intelligence => player.Stats with { Intelligence = value },
            CharacterAttribute.Piety => player.Stats with { Piety = value },
            CharacterAttribute.Stamina => player.Stats with { Stamina = value },
            CharacterAttribute.Honor => player.Stats with { Honor = value },
            _ => player.Stats
        };
        if (attribute == CharacterAttribute.Fame) player.Fame = value;
        if (attribute == CharacterAttribute.Wealth) player.Wealth = value;
    }
}
