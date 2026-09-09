using System.Runtime.InteropServices;
using System.Text;

namespace Conqueror.Game;

public static class StartupFailureReporter
{
    public const string ApplicationTitle = "ReConqueror A.D. 1086";

    public static string BuildUserMessage(Exception exception, string? userContentRoot, string? logPath = null)
    {
        ArgumentNullException.ThrowIfNull(exception);
        var builder = new StringBuilder()
            .AppendLine($"{ApplicationTitle} could not start.")
            .AppendLine()
            .AppendLine(exception.Message);
        if (!string.IsNullOrWhiteSpace(userContentRoot))
            builder.AppendLine().AppendLine($"Original-resource folder: {Path.GetFullPath(userContentRoot)}");
        builder.AppendLine()
            .AppendLine("If original game resources are missing or damaged, run “Import or Manage Original Conqueror Resources” from the Start menu.");
        if (!string.IsNullOrWhiteSpace(logPath))
            builder.AppendLine().AppendLine($"Technical details: {logPath}");
        return builder.ToString().TrimEnd();
    }

    public static void Report(Exception exception, string? userContentRoot, string? stateRoot)
    {
        var logPath = TryWriteLog(exception, userContentRoot, stateRoot);
        var message = BuildUserMessage(exception, userContentRoot, logPath);
        Console.Error.WriteLine(message);
        Console.Error.WriteLine(exception);
        if (OperatingSystem.IsWindows())
            _ = MessageBoxW(IntPtr.Zero, message, ApplicationTitle, 0x10);
    }

    private static string? TryWriteLog(Exception exception, string? userContentRoot, string? stateRoot)
    {
        try
        {
            var directory = Path.Combine(
                stateRoot ?? GamePathResolver.ResolveStateRoot(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)),
                "Logs");
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "startup-error.log");
            File.WriteAllText(path,
                $"{DateTimeOffset.UtcNow:O}{Environment.NewLine}" +
                $"Original-resource folder: {userContentRoot ?? "(not resolved)"}{Environment.NewLine}" +
                exception);
            return path;
        }
        catch
        {
            return null;
        }
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int MessageBoxW(IntPtr window, string text, string caption, uint type);
}
