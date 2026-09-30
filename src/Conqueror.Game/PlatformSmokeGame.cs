using Microsoft.Xna.Framework;

namespace Conqueror.Game;

// Assetless native-platform diagnostic. It exercises a real graphics device
// without entering gameplay or loading owner-imported resources.
internal sealed class PlatformSmokeGame : Microsoft.Xna.Framework.Game
{
    public PlatformSmokeGame()
    {
        _ = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 320,
            PreferredBackBufferHeight = 200
        };
        Window.Title = "ReConqueror platform diagnostic";
    }

    protected override void Initialize()
    {
        base.Initialize();
        Exit();
    }
}
