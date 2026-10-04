using RefurbishedDinosaurs.Core.Diagnostics;

namespace Conqueror.Game;

// The game's text and locations for the shared startup failure report.
public static class StartupFailureReporter
{
    public const string ApplicationTitle = "ReConqueror A.D. 1086";

    private const string RecoveryInstruction =
        "If original game resources are missing or damaged, run “Import or Manage Original Conqueror Resources” from the Start menu.";

    public static string BuildUserMessage(Exception exception, string? userContentRoot, string? logPath = null) =>
        StartupFailure.BuildMessage(Options(stateRoot: null), exception, userContentRoot, logPath);

    public static void Report(Exception exception, string? userContentRoot, string? stateRoot) =>
        StartupFailure.Report(Options(stateRoot), exception, userContentRoot);

    // The failure being reported may be the one that stopped the state directory from resolving.
    private static StartupFailureOptions Options(string? stateRoot) =>
        new(ApplicationTitle, Path.Combine(stateRoot ?? StateRootOrTemp(), "Logs"), RecoveryInstruction);

    private static string StateRootOrTemp()
    {
        try
        {
            return GamePathResolver.ResolveStateRoot(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
        }
        catch (ArgumentException)
        {
            return Path.Combine(Path.GetTempPath(), GamePathResolver.ApplicationDataDirectory);
        }
    }
}
