using System.Text;
using Conqueror.Game;
using Conqueror.Resources;
using Xunit;

namespace Conqueror.Tests;

public sealed class SharedRuntimeIntegrationTests
{
    [Fact]
    public void SettingsKeepMigratedBackupAndRejectOversizedFiles()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-shared-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var path = Path.Combine(root, "settings.json");
            var legacy = "{\"Version\":1,\"CdMusic\":false,\"MusicVolume\":2}";
            File.WriteAllText(path, legacy, new UTF8Encoding(true));
            var store = new GameSettingsStore(path);
            Assert.False(store.Load().CdMusic);
            Assert.Equal(1f, store.Load().MusicVolume);
            store.Save(new GameSettings());
            File.WriteAllText(path, new string(' ', 64 * 1024 + 1));
            Assert.False(store.Load().CdMusic);
            Assert.Equal(GameSettingsStore.CurrentVersion, store.Load().Version);
            File.WriteAllText(store.BackupPath, new string(' ', 64 * 1024 + 1));
            Assert.Equal(new GameSettings(), store.Load());
        }
        finally { Directory.Delete(root, true); }
    }

    [Theory]
    [InlineData("C:relative.bin")]
    [InlineData("CON.bin")]
    [InlineData("folder/file. ")]
    [InlineData("../outside.bin")]
    public void ImportOperationsRejectNonportablePathsBeforeWriting(string relative)
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-path-{Guid.NewGuid():N}");
        Assert.Throws<InvalidDataException>(() => ResourcePaths.SafeTarget(root, relative));
        Assert.Throws<InvalidDataException>(() => GeneratedContentInstaller.InstallBytes(root, relative, [1]));
        Assert.Throws<InvalidDataException>(() => ImportDiskPlanner.Calculate(root, [new(relative, 1)]));
        Assert.False(Directory.Exists(root));
    }

    [Fact]
    public void ContentDiscoverySearchesParentOfApplicationWithTrailingSeparator()
    {
        var root = Path.Combine(Path.GetTempPath(), $"conqueror-discovery-{Guid.NewGuid():N}");
        try
        {
            var content = Path.Combine(root, "UserContent");
            Directory.CreateDirectory(content);
            var application = Path.Combine(root, "app") + Path.DirectorySeparatorChar;
            Assert.Equal(content, GamePathResolver.ResolveUserContent(application,
                Path.Combine(root, "elsewhere"), Path.Combine(root, "state")));
        }
        finally { Directory.Delete(root, true); }
    }
}
