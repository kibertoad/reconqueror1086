namespace Conqueror.Game;

public static class GamePathResolver
{
    public const string ApplicationDataDirectory = "ReConquerorAD1086";
    public const string UserContentDirectory = "UserContent";

    public static string ResolveUserContent(
        string applicationDirectory,
        string currentDirectory,
        string localApplicationData) =>
        ResolveUserContent(
            applicationDirectory,
            currentDirectory,
            localApplicationData,
            Directory.Exists(Path.Combine(applicationDirectory, UserContentDirectory)),
            Directory.Exists(Path.Combine(Parent(applicationDirectory), UserContentDirectory)),
            Directory.Exists(Path.Combine(currentDirectory, UserContentDirectory)));

    public static string ResolveUserContent(
        string applicationDirectory,
        string currentDirectory,
        string localApplicationData,
        bool applicationContentExists,
        bool parentContentExists,
        bool currentContentExists)
    {
        ValidateRoot(applicationDirectory, nameof(applicationDirectory));
        ValidateRoot(currentDirectory, nameof(currentDirectory));
        ValidateRoot(localApplicationData, nameof(localApplicationData));

        if (applicationContentExists)
            return Path.Combine(applicationDirectory, UserContentDirectory);
        if (parentContentExists)
            return Path.Combine(Parent(applicationDirectory), UserContentDirectory);
        if (currentContentExists)
            return Path.Combine(currentDirectory, UserContentDirectory);
        return Path.Combine(localApplicationData, ApplicationDataDirectory, UserContentDirectory);
    }

    public static string ResolveStateRoot(string localApplicationData)
    {
        ValidateRoot(localApplicationData, nameof(localApplicationData));
        return Path.Combine(localApplicationData, ApplicationDataDirectory);
    }

    private static string Parent(string path) => Directory.GetParent(path)?.FullName ?? path;

    private static void ValidateRoot(string root, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(root)) throw new ArgumentException("A filesystem root is required.", parameterName);
    }
}
