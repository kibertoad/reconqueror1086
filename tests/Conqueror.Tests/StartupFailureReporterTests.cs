using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed class StartupFailureReporterTests
{
    [Fact]
    public void StartupMessageExplainsHowToRecoverMissingResources()
    {
        var content = Path.Combine("installation", "UserContent");
        var message = StartupFailureReporter.BuildUserMessage(
            new FileNotFoundException("Original resources are not installed."),
            content,
            Path.Combine("user", "Logs", "startup-error.log"));

        Assert.Contains("could not start", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Original resources are not installed", message, StringComparison.Ordinal);
        Assert.Contains("Import or Manage Original Conqueror Resources", message, StringComparison.Ordinal);
        Assert.Contains(Path.GetFullPath(content), message, StringComparison.Ordinal);
        Assert.Contains("startup-error.log", message, StringComparison.Ordinal);
    }
}
