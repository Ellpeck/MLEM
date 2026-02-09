using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data.Content;
using MLEM.Font;
using MLEM.Startup;
using NUnit.Framework;

namespace MLEM.Tests;

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

        // make sure that the viewport is always the same size, since RunOneFrame doesn't ensure window size is correct
        this.UiSystem.Viewport = new Rectangle(0, 0, 1280, 720);
        // we use precompiled fonts and kni uses a different asset compilation system, so we just have both stored
        this.UiSystem.Style.Font = new GenericSpriteFont(MlemGame.LoadContent<SpriteFont>(
#if KNI
            "TestFontKni"
#else
            "TestFont"
#endif
        ));
        // make sure we catch a potential ui stack overflow as part of the tests by ensuring a sufficient execution stack
        this.UiSystem.OnElementAreaDirty += _ => RuntimeHelpers.EnsureSufficientExecutionStack();
        this.UiSystem.OnElementAreaUpdated += _ => RuntimeHelpers.EnsureSufficientExecutionStack();
    }

    public static TestGame Create() {
        var game = new TestGame();
        game.RunOneFrame();
        return game;
    }

}

public class GameTestFixture {

    protected TestGame Game { get; private set; }

    [SetUp]
    public void SetUpGame() {
        this.Game = TestGame.Create();
    }

    [TearDown]
    public void TearDownGame() {
        this.Game?.Dispose();
        this.Game = null;
    }

}
