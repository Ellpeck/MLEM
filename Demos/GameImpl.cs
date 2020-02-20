using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace Demos {
    public class GameImpl : MlemGame {

        private static readonly Dictionary<string, Func<MlemGame, Demo>> Demos = new Dictionary<string, Func<MlemGame, Demo>>();
        private Demo activeDemo;
        private double fpsTime;
        private int lastFps;
        private int fpsCounter;

        static GameImpl() {
            Demos.Add("Ui", game => new UiDemo(game));
            Demos.Add("Animation and Texture Atlas", game => new AnimationDemo(game));
            Demos.Add("Auto Tiling", game => new AutoTilingDemo(game));
            Demos.Add("Pathfinding", game => new PathfindingDemo(game));
        }

        public GameImpl() {
            this.IsMouseVisible = true;
        }

        protected override void LoadContent() {
            base.LoadContent();

            var tex = LoadContent<Texture2D>("Textures/Test");
            this.UiSystem.Style = new UntexturedStyle(this.SpriteBatch) {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")),
                TextScale = 0.1F,
                PanelTexture = new NinePatch(new TextureRegion(tex, 0, 8, 24, 24), 8),
                ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4),
                ScrollBarBackground = new NinePatch(new TextureRegion(tex, 12, 0, 4, 8), 1, 1, 2, 2),
                ScrollBarScrollerTexture = new NinePatch(new TextureRegion(tex, 8, 0, 4, 8), 1, 1, 2, 2),
            };
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            var selection = new Panel(Anchor.Center, new Vector2(100, 80), Vector2.Zero, false, true, new Point(5, 10));
            this.UiSystem.Add("DemoSelection", selection);

            var backButton = new Button(Anchor.TopRight, new Vector2(30, 10), "Back") {
                OnPressed = e => {
                    this.activeDemo.Clear();
                    this.activeDemo = null;
                    this.UiSystem.Remove("BackButton");
                    this.UiSystem.Add("DemoSelection", selection);
                }
            };

            selection.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Select the demo you want to see below. Check the demos' source code for more in-depth explanations of their functionality."));
            selection.AddChild(new VerticalSpace(5));
            foreach (var demo in Demos) {
                selection.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), demo.Key) {
                    OnPressed = e => {
                        this.UiSystem.Remove("DemoSelection");
                        this.UiSystem.Add("BackButton", backButton);

                        this.activeDemo = demo.Value.Invoke(this);
                        this.activeDemo.LoadContent();
                    },
                    PositionOffset = new Vector2(0, 1)
                });
            }
            
            this.UiSystem.Add("Fps", new Paragraph(Anchor.TopLeft, 1, p => "FPS: " + this.lastFps));
        }

        protected override void DoDraw(GameTime gameTime) {
            if (this.activeDemo != null) {
                this.activeDemo.DoDraw(gameTime);
            } else {
                this.GraphicsDevice.Clear(Color.CornflowerBlue);
            }
            base.DoDraw(gameTime);

            this.fpsCounter++;
            this.fpsTime += gameTime.ElapsedGameTime.TotalSeconds;
            if (this.fpsTime >= 1) {
                this.fpsTime -= 1;
                this.lastFps = this.fpsCounter;
                this.fpsCounter = 0;
            }
        }

        protected override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);
            if (this.activeDemo != null)
                this.activeDemo.Update(gameTime);
        }

    }
}