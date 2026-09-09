using Conqueror.Game;

if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase)) return;

var contentArgument = Array.FindIndex(args, value => value.Equals("--user-content", StringComparison.OrdinalIgnoreCase));
var configuredContent = Environment.GetEnvironmentVariable("RECONQUEROR_USER_CONTENT") ??
    Environment.GetEnvironmentVariable("CONQUEROR_USER_CONTENT");
var userContentRoot = contentArgument >= 0 && contentArgument + 1 < args.Length
    ? Path.GetFullPath(args[contentArgument + 1])
    : !string.IsNullOrWhiteSpace(configuredContent)
        ? Path.GetFullPath(configuredContent)
        : GamePathResolver.ResolveUserContent(
            AppContext.BaseDirectory,
            Environment.CurrentDirectory,
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
var stateRoot = GamePathResolver.ResolveStateRoot(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));

using var game = new ConquerorGame(userContentRoot, stateRoot);
game.Run();
