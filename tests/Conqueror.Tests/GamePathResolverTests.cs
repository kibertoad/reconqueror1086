using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed class GamePathResolverTests
{
    [Fact]
    public void UserContentResolutionSupportsDevelopmentPortableAndInstalledLayouts()
    {
        var root = Path.GetPathRoot(Path.GetFullPath("."))!;
        var application = Path.Combine(root, "application");
        var checkout = Path.Combine(root, "checkout");
        var userData = Path.Combine(root, "user-data");

        Assert.Equal(Path.Combine(application, "UserContent"),
            GamePathResolver.ResolveUserContent(application, checkout, userData, true, false, false));
        Assert.Equal(Path.Combine(checkout, "UserContent"),
            GamePathResolver.ResolveUserContent(application, checkout, userData, false, false, true));
        Assert.Equal(Path.Combine(userData, GamePathResolver.ApplicationDataDirectory, "UserContent"),
            GamePathResolver.ResolveUserContent(application, checkout, userData, false, false, false));
    }

    [Fact]
    public void ParentPackageContentAndWritableStateUseStablePerPlatformPaths()
    {
        var root = Path.GetPathRoot(Path.GetFullPath("."))!;
        var package = Path.Combine(root, "package");
        var application = Path.Combine(package, "Game");
        var elsewhere = Path.Combine(root, "elsewhere");
        var userData = Path.Combine(root, "user-data");

        Assert.Equal(Path.Combine(package, "UserContent"),
            GamePathResolver.ResolveUserContent(application, elsewhere, userData, false, true, false));
        Assert.Equal(Path.Combine(userData, GamePathResolver.ApplicationDataDirectory),
            GamePathResolver.ResolveStateRoot(userData));
    }

    [Theory]
    [InlineData("", "/current", "/user-data")]
    [InlineData("/application", "", "/user-data")]
    [InlineData("/application", "/current", "")]
    public void MissingRootsAreRejected(string application, string current, string localData)
    {
        Assert.Throws<ArgumentException>(() =>
            GamePathResolver.ResolveUserContent(application, current, localData, false, false, false));
    }
}
