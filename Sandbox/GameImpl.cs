using System;
using System.Collections.Generic;
using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Cameras;
using MLEM.Extended.Extensions;
using MLEM.Extended.Tiled;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using MonoGame.Extended.Tiled;

namespace Sandbox {
    public class GameImpl : MlemGame {

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
                ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4)
            };
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            var screen = new Panel(Anchor.Center, new Vector2(200, 100), Vector2.Zero, false, true, new Point(5, 10));
            screen.IsHidden = false;
            screen.OnUpdated += (element, time) => {
                if (this.InputHandler.IsKeyPressed(Keys.Escape))
                    CoroutineHandler.Start(PlayAnimation(screen));
            };
            this.UiSystem.Add("Screen", screen);

            for (var i = 0; i < 100; i++) {
                screen.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), i.ToString()));
            }
        }

        private static IEnumerator<Wait> PlayAnimation(Panel screen) {
            var show = screen.IsHidden;
            screen.IsHidden = false;
            var time = 0;
            const float total = 20;
            while (time <= total) {
                yield return new WaitEvent(CoroutineEvents.Update);
                var percent = show ? time / total : 1 - time / total;
                screen.Root.Scale = percent;
                time++;
            }
            if (!show)
                screen.IsHidden = true;
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            base.DoDraw(gameTime);
        }

    }
}