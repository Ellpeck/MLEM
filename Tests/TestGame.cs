using Microsoft.Xna.Framework.Graphics;
using MLEM.Data.Content;
using MLEM.Font;
using MLEM.Startup;

namespace Tests;

public class TestGame : MlemGame {

    public RawContentManager RawContent { get; private set; }

    private TestGame() {
#if KNI
        // allow textures larger than 4096x4096 for our texture packer tests
        this.GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.FL11_0;
#endif
    }

    protected override void LoadContent() {
        base.LoadContent();
        this.RawContent = new RawContentManager(this.Services, this.Content.RootDirectory);
        // we use precompiled fonts and kni uses a different asset compilation system, so we just have both stored
        this.UiSystem.Style.Font = new GenericSpriteFont(MlemGame.LoadContent<SpriteFont>(
#if KNI
            "TestFontKni"
#else
            "TestFont"
#endif
        ));
    }

    public static TestGame Create() {
        var game = new TestGame();
        game.RunOneFrame();
        return game;
    }

}
