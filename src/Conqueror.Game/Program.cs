using Conqueror.Game;

string? userContentRoot = null;
string? stateRoot = null;
var platformSmokeTest = args.Contains("--platform-smoke-test", StringComparer.OrdinalIgnoreCase);
try
{
    var softwareRendering = SoftwareRenderer.Evaluate(
        args.Contains(SoftwareRenderer.Flag, StringComparer.OrdinalIgnoreCase),
        platformSmokeTest,
        Environment.GetEnvironmentVariable(SoftwareRenderer.DriverVariable));
    if (softwareRendering.Rejection is not null)
    {
        Console.Error.WriteLine(softwareRendering.Rejection);
        return 64;
    }
    if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase))
    {
        Console.WriteLine("Conqueror.Game startup check passed.");
        return 0;
    }
    if (softwareRendering.Enabled) SoftwareRenderer.Apply(softwareRendering.DriverPath!);
    if (platformSmokeTest)
    {
        using var diagnostic = new PlatformSmokeGame();
        diagnostic.Run();
        return 0;
    }
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

    var importedContent = ImportedContentCatalog.LoadRequired(userContentRoot);
    using var game = new ConquerorGame(importedContent, stateRoot);
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
