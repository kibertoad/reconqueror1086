using RefurbishedDinosaurs.Core.Paths;

namespace Conqueror.Game;

public static class GamePathResolver
{
    public const string ApplicationDataDirectory = "ReConquerorAD1086";
    public const string UserContentDirectory = "UserContent";
    private static readonly RestorationPathOptions Paths = new(ApplicationDataDirectory, UserContentDirectory);

    public static string ResolveUserContent(
        string applicationDirectory,
        string currentDirectory,
        string localApplicationData) =>
        RestorationPaths.ResolveImportedContent(Paths, applicationDirectory, currentDirectory, localApplicationData);

    public static string ResolveStateRoot(string localApplicationData) =>
        RestorationPaths.ResolveStateRoot(Paths, localApplicationData);

}
