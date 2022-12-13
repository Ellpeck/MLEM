using Microsoft.Xna.Framework.Graphics;
using MLEM.Data.Content;
using MLEM.Font;
using MLEM.Startup;

namespace Tests;

public class TestGame : MlemGame {

    public RawContentManager RawContent { get; private set; }

    private TestGame() {}

    protected override void LoadContent() {
        base.LoadContent();
        this.RawContent = new RawContentManager(this.Services, this.Content.RootDirectory);
        this.UiSystem.Style.Font = new GenericSpriteFont(MlemGame.LoadContent<SpriteFont>("TestFont"));
    }

    public static TestGame Create() {
        var game = new TestGame();
        game.RunOneFrame();
        return game;
    }

}
