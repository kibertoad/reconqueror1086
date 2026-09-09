namespace Conqueror.Core;

public sealed record CampaignSaveSlot(
    int Number,
    bool Exists,
    bool IsValid,
    string PlayerName,
    DateTime? CampaignDate,
    string? Error,
    bool RecoveredFromBackup = false);

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
        Validate(number);
        return InspectPaths(number, ExistingPaths(number));
    }

    public CampaignSaveSlot InspectAutosave() => InspectPaths(0, ExistingAutosavePaths());

    private static CampaignSaveSlot InspectPaths(int number, IReadOnlyList<(string Path, bool IsBackup)> paths)
    {
        if (paths.Count == 0) return new(number, false, false, "EMPTY", null, null);

        Exception? firstError = null;
        foreach (var candidate in paths)
            try
            {
                var campaign = Campaign.Load(candidate.Path);
                return new(number, true, true, campaign.State.Player.Name, campaign.State.Date, null, candidate.IsBackup);
            }
            catch (Exception error) when (IsUnreadableSave(error))
            {
                firstError ??= error;
            }
        return new(number, true, false, "UNREADABLE", null, firstError?.Message);
    }

    public void Save(Campaign campaign, int number)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        var path = SlotPath(number);
        SaveTo(campaign, path, BackupPath(number));
    }

    public void SaveAutosave(Campaign campaign) => SaveTo(campaign, AutosavePath, AutosaveBackupPath);

    public bool TryLoad(int number, out Campaign? campaign, out string? error)
    {
        Validate(number);
        return TryLoadPaths(ExistingPaths(number), "THAT SAVE SLOT IS EMPTY", out campaign, out error);
    }

    public bool TryLoadAutosave(out Campaign? campaign, out string? error) => TryLoadPaths(
        ExistingAutosavePaths(), "NO AUTOSAVE IS AVAILABLE", out campaign, out error);

    private static bool TryLoadPaths(IReadOnlyList<(string Path, bool IsBackup)> paths, string emptyError,
        out Campaign? campaign, out string? error)
    {
        if (paths.Count == 0)
        {
            campaign = null;
            error = emptyError;
            return false;
        }

        foreach (var candidate in paths)
        {
            try
            {
                campaign = Campaign.Load(candidate.Path);
                error = null;
                return true;
            }
            catch (Exception loadError) when (IsUnreadableSave(loadError)) { }
        }
        campaign = null;
        error = "THAT SAVE SLOT CANNOT BE READ";
        return false;
    }

    public string SlotPath(int number)
    {
        Validate(number);
        return Path.Combine(_root, $"campaign-{number}.json");
    }

    public string BackupPath(int number)
    {
        Validate(number);
        return SlotPath(number) + ".bak";
    }

    public string AutosavePath => Path.Combine(_root, "autosave.json");
    public string AutosaveBackupPath => AutosavePath + ".bak";

    private IReadOnlyList<(string Path, bool IsBackup)> ExistingPaths(int number)
    {
        var result = new List<(string, bool)>();
        var path = SlotPath(number);
        if (File.Exists(path)) result.Add((path, false));
        var backup = BackupPath(number);
        if (File.Exists(backup)) result.Add((backup, true));

        // Compatibility with the single-save prototype used before the five-slot UI.
        var legacy = Path.Combine(_root, "campaign.json");
        if (number == 1 && File.Exists(legacy)) result.Add((legacy, false));
        return result;
    }

    private IReadOnlyList<(string Path, bool IsBackup)> ExistingAutosavePaths()
    {
        var result = new List<(string, bool)>();
        if (File.Exists(AutosavePath)) result.Add((AutosavePath, false));
        if (File.Exists(AutosaveBackupPath)) result.Add((AutosaveBackupPath, true));
        return result;
    }

    private static void SaveTo(Campaign campaign, string path, string backup)
    {
        ArgumentNullException.ThrowIfNull(campaign);
        if (File.Exists(path) && IsReadable(path)) ReplaceBackup(path, backup);
        campaign.Save(path);
    }

    private static bool IsReadable(string path)
    {
        try { Campaign.Load(path); return true; }
        catch (Exception error) when (IsUnreadableSave(error)) { return false; }
    }

    private static void ReplaceBackup(string source, string destination)
    {
        var temporaryPath = destination + $".{Guid.NewGuid():N}.tmp";
        try
        {
            File.Copy(source, temporaryPath, overwrite: false);
            File.Move(temporaryPath, destination, overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    private static void Validate(int number)
    {
        if (number is < 1 or > SlotCount) throw new ArgumentOutOfRangeException(nameof(number), number, "Save slot must be between 1 and 5.");
    }

    private static bool IsUnreadableSave(Exception error) => error is
        IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException
        or NotSupportedException or ArgumentException or NullReferenceException or KeyNotFoundException or OverflowException;
}
