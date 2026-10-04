using Conqueror.Game;
using Xunit;

namespace Conqueror.Tests;

public sealed class GamePathResolverTests
{
    [Fact]
    public void UserContentResolutionSupportsDevelopmentPortableAndInstalledLayouts()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-layout-{Guid.NewGuid():N}");
        var application = Path.Combine(root, "application");
        var checkout = Path.Combine(root, "checkout");
        var userData = Path.Combine(root, "user-data");
        var applicationContent = Path.Combine(application, "UserContent");
        var checkoutContent = Path.Combine(checkout, "UserContent");
        var parentContent = Path.Combine(root, "UserContent");
        try
        {
            Directory.CreateDirectory(applicationContent);
            Directory.CreateDirectory(checkoutContent);
            Directory.CreateDirectory(parentContent);
            Assert.Equal(applicationContent, GamePathResolver.ResolveUserContent(application, checkout, userData));
            Directory.Delete(applicationContent);
            Assert.Equal(parentContent, GamePathResolver.ResolveUserContent(application, checkout, userData));
            Directory.Delete(parentContent);
            Assert.Equal(checkoutContent, GamePathResolver.ResolveUserContent(application, checkout, userData));
            Directory.Delete(checkoutContent);
            Assert.Equal(Path.Combine(userData, GamePathResolver.ApplicationDataDirectory, "UserContent"),
                GamePathResolver.ResolveUserContent(application, checkout, userData));
            Assert.Equal(Path.Combine(userData, GamePathResolver.ApplicationDataDirectory),
                GamePathResolver.ResolveStateRoot(userData));
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData("", "/current", "/user-data")]
    [InlineData("/application", "", "/user-data")]
    [InlineData("/application", "/current", "")]
    public void MissingRootsAreRejected(string application, string current, string localData)
    {
        Assert.Throws<ArgumentException>(() =>
            GamePathResolver.ResolveUserContent(application, current, localData));
    }
}
