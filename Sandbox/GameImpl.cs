using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Cameras;
using MLEM.Content;
using MLEM.Data;
using MLEM.Extended.Extensions;
using MLEM.Extended.Tiled;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using Newtonsoft.Json.Linq;
using RectangleF = MonoGame.Extended.RectangleF;

namespace Sandbox {
    public class GameImpl : MlemGame {

        private Camera camera;
        private TiledMap map;
        private IndividualTiledMapRenderer mapRenderer;
        private TiledMapCollisions collisions;
        private RawContentManager rawContent;

        public GameImpl() {
            this.IsMouseVisible = true;
            this.Window.ClientSizeChanged += (o, args) => {
                Console.WriteLine("Size changed");
            };
        }

        protected override void LoadContent() {
            base.LoadContent();

            this.Components.Add(this.rawContent = new RawContentManager(this.Services));

            this.map = LoadContent<TiledMap>("Tiled/Map");
            this.mapRenderer = new IndividualTiledMapRenderer(this.map);
            this.collisions = new TiledMapCollisions(this.map);

            this.camera = new Camera(this.GraphicsDevice) {
                AutoScaleWithScreen = true,
                Scale = 2,
                LookingPosition = new Vector2(25, 25) * this.map.GetTileSize(),
                MinScale = 0.25F,
                MaxScale = 4
            };

            var tex = this.rawContent.Load<Texture2D>("Textures/Test");
            var font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont"));
            this.UiSystem.Style = new UntexturedStyle(this.SpriteBatch) {
                Font = font,
                TextScale = 0.1F,
                PanelTexture = new NinePatch(new TextureRegion(tex, 0, 8, 24, 24), 8),
                ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4)
            };
            this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
            this.UiSystem.AutoScaleWithScreen = true;
            this.UiSystem.GlobalScale = 5;

            var panel = new Panel(Anchor.Center, new Vector2(0, 100), Vector2.Zero) {SetWidthBasedOnChildren = true};
            panel.AddChild(new Button(Anchor.AutoLeft, new Vector2(100, 10)));
            panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(80, 10)));
            this.UiSystem.Add("Panel", panel);

            panel.SetData("TestKey", new Vector2(10, 2));
            Console.WriteLine(panel.GetData<Vector2>("TestKey"));

            var obj = new Test {
                Vec = new Vector2(10, 20),
                Point = new Point(20, 30),
                Rectangle = new Rectangle(1, 2, 3, 4),
                RectangleF = new RectangleF(4, 5, 6, 7).ToMlem(),
                Dir = Direction2.Left
            };

            var writer = new StringWriter();
            this.Content.GetJsonSerializer().Serialize(writer, obj);
            Console.WriteLine(writer.ToString());
            // {"Vec":"10 20","Point":"20 30","Rectangle":"1 2 3 4","RectangleF":"4 5 6 7"}

            // Also:
            //this.Content.AddJsonConverter(new CustomConverter());

            var res = this.Content.LoadJson<Test>("Test");
            Console.WriteLine(res);

            this.OnDraw += (game, time) => {
                this.SpriteBatch.Begin();
                font.DrawString(this.SpriteBatch, "Left Aligned\nover multiple lines", new Vector2(640, 0), TextAlign.Left, Color.White);
                font.DrawString(this.SpriteBatch, "Center Aligned\nover multiple lines", new Vector2(640, 100), TextAlign.Center, Color.White);
                font.DrawString(this.SpriteBatch, "Right Aligned\nover multiple lines", new Vector2(640, 200), TextAlign.Right, Color.White);
                font.DrawString(this.SpriteBatch, "Center Aligned on both axes", new Vector2(640, 360), TextAlign.CenterBothAxes, Color.White);
                this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), new Rectangle(640 - 4, 360 - 4, 8, 8), Color.Green);

                this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), new Rectangle(200, 400, 200, 400), Color.Green);
                font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1), new Vector2(200, 400), Color.White);
                font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1, ellipsis: "..."), new Vector2(200, 450), Color.White);
                font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1, true), new Vector2(200, 500), Color.White);
                font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1, true, "..."), new Vector2(200, 550), Color.White);
                this.SpriteBatch.End();
            };
        }

        protected override void DoUpdate(GameTime gameTime) {
            base.DoUpdate(gameTime);
            if (this.InputHandler.IsKeyPressed(Keys.F11))
                this.GraphicsDeviceManager.SetFullscreen(!this.GraphicsDeviceManager.IsFullScreen);

            var delta = this.InputHandler.ScrollWheel - this.InputHandler.LastScrollWheel;
            if (delta != 0) {
                this.camera.Zoom(0.1F * Math.Sign(delta), this.InputHandler.MousePosition.ToVector2());
            }
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, this.camera.ViewMatrix);
            this.mapRenderer.Draw(this.SpriteBatch, this.camera.GetVisibleRectangle().ToExtended());

            foreach (var tile in this.collisions.GetCollidingTiles(new RectangleF(0, 0, this.map.Width, this.map.Height))) {
                foreach (var area in tile.Collisions) {
                    this.SpriteBatch.DrawRectangle(area.Position * this.map.GetTileSize(), area.Size * this.map.GetTileSize(), Color.Red);
                }
            }

            this.SpriteBatch.End();
            base.DoDraw(gameTime);
        }

        private class Test {

            public Vector2 Vec;
            public Point Point;
            public Rectangle Rectangle;
            public MLEM.Misc.RectangleF RectangleF;
            public Direction2 Dir;

            public override string ToString() {
                return $"{nameof(this.Vec)}: {this.Vec}, {nameof(this.Point)}: {this.Point}, {nameof(this.Rectangle)}: {this.Rectangle}, {nameof(this.RectangleF)}: {this.RectangleF}, {nameof(this.Dir)}: {this.Dir}";
            }

        }

    }
}