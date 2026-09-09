using Conqueror.Game;

if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
{
    Console.WriteLine("Conqueror.Game startup check passed.");
    return 0;
}

string? userContentRoot = null;
string? stateRoot = null;
var platformSmokeTest = args.Contains("--platform-smoke-test", StringComparer.OrdinalIgnoreCase);
try
{
    var contentArgument = Array.FindIndex(args,
        value => value.Equals("--user-content", StringComparison.OrdinalIgnoreCase));
    var configuredContent = Environment.GetEnvironmentVariable("RECONQUEROR_USER_CONTENT") ??
        Environment.GetEnvironmentVariable("CONQUEROR_USER_CONTENT");
    userContentRoot = contentArgument >= 0 && contentArgument + 1 < args.Length
        ? Path.GetFullPath(args[contentArgument + 1])
        : !string.IsNullOrWhiteSpace(configuredContent)
            ? Path.GetFullPath(configuredContent)
            : GamePathResolver.ResolveUserContent(
                AppContext.BaseDirectory,
                Environment.CurrentDirectory,
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
    stateRoot = GamePathResolver.ResolveStateRoot(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

    using var game = new ConquerorGame(userContentRoot, stateRoot);
    if (platformSmokeTest) return 0;
    game.Run();
    return 0;
}
catch (Exception exception)
{
    if (platformSmokeTest)
    {
        Console.Error.WriteLine(exception);
        return 1;
    }
    StartupFailureReporter.Report(exception, userContentRoot, stateRoot);
    return 1;
}
