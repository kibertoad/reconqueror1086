namespace Conqueror.Core;

public sealed record CampaignSaveSlot(
    int Number,
    bool Exists,
    bool IsValid,
    string PlayerName,
    DateTime? CampaignDate,
    string? Error);

public sealed class CampaignSaveSlots
{
    public const int SlotCount = 5;

    private readonly string _root;

    public CampaignSaveSlots(string root)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(root);
        _root = Path.GetFullPath(root);
    }

    public IReadOnlyList<CampaignSaveSlot> Inspect() =>
        Enumerable.Range(1, SlotCount).Select(Inspect).ToArray();

    public CampaignSaveSlot Inspect(int number)
    {
        var path = ExistingPath(number);
        if (path is null) return new(number, false, false, "EMPTY", null, null);

        try
        {
            var campaign = Campaign.Load(path);
            return new(number, true, true, campaign.State.Player.Name, campaign.State.Date, null);
        }
        catch (Exception error) when (IsUnreadableSave(error))
        {
            return new(number, true, false, "UNREADABLE", null, error.Message);
        }
    }

    public void Save(Campaign campaign, int number)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        campaign.Save(SlotPath(number));
    }

    public bool TryLoad(int number, out Campaign? campaign, out string? error)
    {
        var path = ExistingPath(number);
        if (path is null)
        {
            campaign = null;
            error = "THAT SAVE SLOT IS EMPTY";
            return false;
        }

        try
        {
            campaign = Campaign.Load(path);
            error = null;
            return true;
        }
        catch (Exception loadError) when (IsUnreadableSave(loadError))
        {
            campaign = null;
            error = "THAT SAVE SLOT CANNOT BE READ";
            return false;
        }
    }

    public string SlotPath(int number)
    {
        Validate(number);
        return Path.Combine(_root, $"campaign-{number}.json");
    }

    private string? ExistingPath(int number)
    {
        var path = SlotPath(number);
        if (File.Exists(path)) return path;

        // Compatibility with the single-save prototype used before the five-slot UI.
        var legacy = Path.Combine(_root, "campaign.json");
        return number == 1 && File.Exists(legacy) ? legacy : null;
    }

    private static void Validate(int number)
    {
        if (number is < 1 or > SlotCount) throw new ArgumentOutOfRangeException(nameof(number), number, "Save slot must be between 1 and 5.");
    }

    private static bool IsUnreadableSave(Exception error) => error is
        IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException
        or NotSupportedException or ArgumentException or NullReferenceException or KeyNotFoundException or OverflowException;
}
