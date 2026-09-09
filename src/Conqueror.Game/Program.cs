using Conqueror.Game;

if (args.Contains("--smoke-test", StringComparer.OrdinalIgnoreCase)) return;

using var game = new ConquerorGame();
game.Run();
