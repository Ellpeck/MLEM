using System;
using System.IO;
using System.Text.RegularExpressions;
using FontStashSharp;
using MLEM.Cameras;
using MLEM.Data;
using MLEM.Data.Content;
using MLEM.Extended.Font;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace Sandbox;

public class GameImpl : MlemGame {

    private Camera camera;
    /*private TiledMap map;
    private IndividualTiledMapRenderer mapRenderer;
    private TiledMapCollisions collisions;*/
    private RawContentManager rawContent;
    private TokenizedString tokenized;

    public GameImpl() {
        this.IsMouseVisible = true;
        this.Window.ClientSizeChanged += (o, args) => {
            Console.WriteLine("Size changed");
        };
    }

    protected override void LoadContent() {
        base.LoadContent();

        this.Components.Add(this.rawContent = new RawContentManager(this.Services));

        /*this.map = MlemGame.LoadContent<TiledMap>("Tiled/Map");
        this.mapRenderer = new IndividualTiledMapRenderer(this.map);
        this.collisions = new TiledMapCollisions(this.map);*/

        this.camera = new Camera(this.GraphicsDevice) {
            AutoScaleWithScreen = true,
            Scale = 2,
            /*LookingPosition = new Vector2(25, 25) * this.map.GetTileSize(),*/
            MinScale = 0.25F,
            MaxScale = 4
        };

        var tex = this.rawContent.Load<Texture2D>("Textures/Test");
        using (var data = tex.GetTextureData()) {
            var textureData = data;
            textureData[1, 9] = Color.Pink;
            textureData[textureData.FromIndex(textureData.ToIndex(25, 9))] = Color.Yellow;
        }

        var system = new FontSystem();
        system.AddFont(File.ReadAllBytes("Content/Fonts/Cadman_Roman.otf"));
        //var font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont"));
        //var font = new GenericBitmapFont(LoadContent<BitmapFont>("Fonts/Regular"));
        var font = new GenericStashFont(system.GetFont(32));
        var spriteFont = new GenericSpriteFont(MlemGame.LoadContent<SpriteFont>("Fonts/TestFont"));
        this.UiSystem.Style = new UntexturedStyle(this.SpriteBatch) {
            Font = font,
            TextScale = 0.5F,
            PanelTexture = new NinePatch(new TextureRegion(tex, 0, 8, 24, 24), 8),
            ButtonTexture = new NinePatch(new TextureRegion(tex, 24, 8, 16, 16), 4)
        };
        this.UiSystem.AutoScaleReferenceSize = new Point(1280, 720);
        this.UiSystem.AutoScaleWithScreen = true;
        this.UiSystem.GlobalScale = 5;

        /*this.OnDraw += (g, time) => {
            const string strg = "This is a test string\nto test things\n\nMany things are being tested, like the ability\nfor this font to agree\n\nwith newlines";
            this.SpriteBatch.Begin();
            spriteFont.DrawString(this.SpriteBatch, strg, new Vector2(600, 100), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            font.DrawString(this.SpriteBatch, strg, new Vector2(600, 100), Color.White, 0, Vector2.Zero, 2, SpriteEffects.None, 0);
            this.SpriteBatch.End();
        };*/

        var panel = new Panel(Anchor.Center, new Vector2(0, 100), Vector2.Zero) {SetWidthBasedOnChildren = true};
        panel.AddChild(new Button(Anchor.AutoLeft, new Vector2(100, 10)));
        panel.AddChild(new Button(Anchor.AutoCenter, new Vector2(80, 10)));
        //this.UiSystem.Add("Panel", panel);

        panel.SetData("TestKey", new Vector2(10, 2));
        //Console.WriteLine(panel.GetData<Vector2>("TestKey"));

        var obj = new Test(Vector2.One, "test") {
            Vec = new Vector2(10, 20),
            Point = new Point(20, 30),
            Dir = Direction2.Left,
            OtherTest = new Test(Vector2.One, "other") {
                Vec = new Vector2(70, 30),
                Dir = Direction2.Right
            }
        };
        Console.WriteLine(obj);

        for (var i = 0; i < 360; i += 45) {
            var rad = MathHelper.ToRadians(i);
            var vec = new Vector2((float) Math.Sin(rad), (float) Math.Cos(rad));
            var dir = vec.ToDirection();
            Console.WriteLine(vec + " -> " + dir);
        }

        var writer = new StringWriter();
        this.Content.GetJsonSerializer().Serialize(writer, obj);
        //Console.WriteLine(writer.ToString());
        // {"Vec":"10 20","Point":"20 30","Rectangle":"1 2 3 4","RectangleF":"4 5 6 7"}

        // Also:
        //this.Content.AddJsonConverter(new CustomConverter());

        var res = this.Content.LoadJson<Test>("Test");
        Console.WriteLine("The res is " + res);

        var gradient = this.SpriteBatch.GenerateGradientTexture(Color.Green, Color.Red, Color.Blue, Color.Yellow);
        /*this.OnDraw += (game, time) => {
            this.SpriteBatch.Begin();
            this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), new Rectangle(640 - 4, 360 - 4, 8, 8), Color.Green);

            this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), new Rectangle(200, 400, 200, 400), Color.Green);
            font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1), new Vector2(200, 400), Color.White);
            font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1, ellipsis: "..."), new Vector2(200, 450), Color.White);
            font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1, true), new Vector2(200, 500), Color.White);
            font.DrawString(this.SpriteBatch, font.TruncateString("This is a very long string", 200, 1, true, "..."), new Vector2(200, 550), Color.White);

            this.SpriteBatch.Draw(gradient, new Rectangle(300, 100, 200, 200), Color.White);
            this.SpriteBatch.End();
        };*/

        var sc = 4;
        var formatter = new TextFormatter();
        formatter.AddImage("Test", new TextureRegion(tex, 0, 8, 24, 24));
        formatter.Macros.Add(new Regex("<testmacro>"), (f, m, r) => "<test1>");
        formatter.Macros.Add(new Regex("<test1>"), (f, m, r) => "<test2> blue");
        formatter.Macros.Add(new Regex("<test2>"), (f, m, r) => "<c Blue>");
        var strg = "This text uses a bunch of non-breaking~spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <testmacro> text</c>.";
        //var strg = "Lorem Ipsum <i Test> is simply dummy text of the <i Test> printing and typesetting <i Test> industry. Lorem Ipsum has been the industry's standard dummy text <i Test> ever since the <i Test> 1500s, when <i Test><i Test><i Test><i Test><i Test><i Test><i Test> an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        //var strg = "Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book. It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised in the 1960s with the release of Letraset sheets containing Lorem Ipsum passages, and more recently with desktop publishing software like Aldus PageMaker including versions of Lorem Ipsum.";
        //var strg = "This is <u>a test of the underlined formatting code</u>!";
        this.tokenized = formatter.Tokenize(font, strg);
        this.tokenized.Split(font, 400, sc);

        var square = this.SpriteBatch.GenerateSquareTexture(Color.Yellow);
        var round = this.SpriteBatch.GenerateCircleTexture(Color.Green, 128);

        var region = new TextureRegion(round) {Pivot = new Vector2(0.5F)};
        var region2 = new TextureRegion(round);

        var atlas = this.Content.LoadTextureAtlas("Textures/Furniture");
        foreach (var r in atlas.Regions)
            Console.WriteLine(r.Name + ": " + r.U + " " + r.V + " " + r.Width + " " + r.Height + " " + r.PivotPixels);

        this.OnDraw += (g, time) => {
            this.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            //this.SpriteBatch.Draw(square, new Rectangle(10, 10, 400, 400), Color.White);
            //this.SpriteBatch.Draw(round, new Rectangle(10, 10, 400, 400), Color.White);
            this.SpriteBatch.Draw(region, new Vector2(50, 50), Color.White, 0, Vector2.Zero, 0.5F, SpriteEffects.None, 0);
            this.SpriteBatch.Draw(region2, new Vector2(50, 50), Color.Yellow * 0.5F, 0, Vector2.Zero, 0.5F, SpriteEffects.None, 0);
            this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), new Vector2(50, 50), Color.Pink);

            //this.SpriteBatch.FillRectangle(new RectangleF(400, 20, 400, 1000), Color.Green);
            //font.DrawString(this.SpriteBatch, this.tokenized.DisplayString, new Vector2(400, 20), Color.White * 0.25F, 0, Vector2.Zero, sc, SpriteEffects.None, 0);
            //this.tokenized.Draw(time, this.SpriteBatch, new Vector2(400, 20), font, Color.White, sc, 0);
            //this.SpriteBatch.DrawGrid(new Vector2(30, 30), new Vector2(40, 60), new Point(10, 5), Color.Yellow, 3);
            this.SpriteBatch.End();
        };
        this.OnUpdate += (g, time) => {
            if (this.InputHandler.IsPressed(Keys.W)) {
                this.tokenized = formatter.Tokenize(font, strg);
                this.tokenized.Split(font, this.InputHandler.IsModifierKeyDown(ModifierKey.Shift) ? 400 : 500, sc);
            }
            this.tokenized.Update(time);
        };

        /*var testPanel = new Panel(Anchor.Center, new Vector2(0.5F, 100), Vector2.Zero);
        testPanel.AddChild(new Button(Anchor.AutoLeft, new Vector2(0.25F, -1)));
        testPanel.AddChild(new Button(Anchor.AutoLeft, new Vector2(2500, 1)) {PreventParentSpill = true});
        this.UiSystem.Add("Test", testPanel);

        var invalidPanel = new Panel(Anchor.Center, Vector2.Zero, Vector2.Zero) {
            SetWidthBasedOnChildren = true,
            SetHeightBasedOnChildren = true
        };
        invalidPanel.AddChild(new Paragraph(Anchor.AutoRight, 1, "This is some test text!", true));
        invalidPanel.AddChild(new VerticalSpace(1));
        this.UiSystem.Add("Invalid", invalidPanel);*/

        /*var loadGroup = new Group(Anchor.TopLeft, Vector2.One, false);
        var loadPanel = loadGroup.AddChild(new Panel(Anchor.Center, new Vector2(150, 150), Vector2.Zero, false, true, false) {
            ChildPadding = new Padding(5, 10, 5, 5)
        });
        for (var i = 0; i < 1; i++) {
            var button = loadPanel.AddChild(new Button(Anchor.AutoLeft, new Vector2(1)) {
                SetHeightBasedOnChildren = true,
                Padding = new Padding(0, 0, 0, 1),
                ChildPadding = new Padding(3)
            });
            button.AddChild(new Group(Anchor.AutoLeft, new Vector2(0.5F, 30), false) {
                CanBeMoused = false
            });
        }
        var par = loadPanel.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is another\ntest string\n\nwith many lines\nand many more!"));
        par.OnUpdated = (e, time) => {
            GenericFont newFont = Input.IsModifierKeyDown(ModifierKey.Shift) ? spriteFont : font;
            if (newFont != par.RegularFont.Value) {
                par.TextScaleMultiplier = newFont == font ? 1 : 0.5F;
                par.RegularFont = newFont;
                par.ForceUpdateArea();
            }
        };
        par.OnDrawn = (e, time, batch, a) => batch.DrawRectangle(e.DisplayArea.ToExtended(), Color.Red);
        this.UiSystem.Add("Load", loadGroup);*/

        /*var spillPanel = new Panel(Anchor.Center, new Vector2(100), Vector2.Zero);
        var squishingGroup = spillPanel.AddChild(new SquishingGroup(Anchor.TopLeft, Vector2.One));
        squishingGroup.AddChild(new Button(Anchor.TopLeft, new Vector2(30), "TL") {
            OnUpdated = (e, time) => e.IsHidden = Input.IsKeyDown(Keys.D1),
            Priority = 10
        }).SetData("Ref", "TL");
        squishingGroup.AddChild(new Button(Anchor.TopRight, new Vector2(30), "TR") {
            OnUpdated = (e, time) => e.IsHidden = Input.IsKeyDown(Keys.D2),
            Priority = 20
        }).SetData("Ref", "TR");
        squishingGroup.AddChild(new Button(Anchor.BottomLeft, new Vector2(30), "BL") {
            OnUpdated = (e, time) => e.IsHidden = Input.IsKeyDown(Keys.D3),
            Priority = 30
        }).SetData("Ref", "BL");
        squishingGroup.AddChild(new Button(Anchor.BottomRight, new Vector2(30), "BR") {
            OnUpdated = (e, time) => e.IsHidden = Input.IsKeyDown(Keys.D4),
            Priority = 40
        }).SetData("Ref", "BR");
        squishingGroup.AddChild(new Button(Anchor.Center, Vector2.Zero, "0") {
            PositionOffset = new Vector2(-10, -5),
            Size = new Vector2(60, 55),
            OnPressed = e => {
                e.Priority = 100 - e.Priority;
                ((Button) e).Text.Text = e.Priority.ToString();
                e.SetAreaDirty();
            }
        }).SetData("Ref", "Main");
        this.UiSystem.Add("SpillTest", spillPanel);*/

        var regularFont = spriteFont.Font;
        var genericFont = spriteFont;

        var index = 0;
        var pos = new Vector2(100, 20);
        var scale = 1F;
        var origin = Vector2.Zero;
        var rotation = 0F;
        var effects = SpriteEffects.None;

        this.OnDraw += (g, time) => {
            const string testString = "This is a\ntest string\n\twith long lines.\nLet's write some more stuff. Let's\r\nsplit lines weirdly.";
            if (MlemGame.Input.IsKeyPressed(Keys.I)) {
                index++;
                if (index == 1) {
                    scale = 2;
                } else if (index == 2) {
                    origin = new Vector2(15, 15);
                } else if (index == 3) {
                    rotation = 0.25F;
                } else if (index == 4) {
                    effects = SpriteEffects.FlipHorizontally;
                } else if (index == 5) {
                    effects = SpriteEffects.FlipVertically;
                } else if (index == 6) {
                    effects = SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically;
                }
            }

            this.SpriteBatch.Begin();
            if (MlemGame.Input.IsKeyDown(Keys.LeftShift)) {
                this.SpriteBatch.DrawString(regularFont, testString, pos, Color.Red, rotation, origin, scale, effects, 0);
            } else {
                genericFont.DrawString(this.SpriteBatch, testString, pos, Color.Green, rotation, origin, scale, effects, 0);
            }
            this.SpriteBatch.End();
        };

        /*var viewport = new BoxingViewportAdapter(this.Window, this.GraphicsDevice, 1280, 720);
        var newPanel = new Panel(Anchor.TopLeft, new Vector2(200, 100), new Vector2(10, 10));
        newPanel.AddChild(new Button(Anchor.TopLeft, new Vector2(100, 20), "Text", "Tooltip text"));
        this.UiSystem.Add("Panel", newPanel);

        var keybindPanel = new Panel(Anchor.BottomRight, new Vector2(130, 150), new Vector2(5));
        for (var i = 0; i < 15; i++) {
            var button = keybindPanel.AddChild(new Button(default, default, i.ToString()));
            button.Anchor = Anchor.AutoInline;
            button.Padding = new Padding(0.5F);
            button.SetHeightBasedOnChildren = false;
            button.Size = new Vector2(30, 50);
        }
        this.UiSystem.Add("Keybinds", keybindPanel);

        var packer = new RuntimeTexturePacker();
        var regions = new List<TextureRegion>();
        packer.Add(new UniformTextureAtlas(tex, 16, 16), r => {
            regions.AddRange(r.Values);
            Console.WriteLine($"Returned {r.Count} regions: {string.Join(", ", r.Select(kv => kv.Key + ": " + kv.Value.Area))}");
        }, 1, true, true);
        packer.Add(this.Content.LoadTextureAtlas("Textures/Furniture"), r => {
            regions.AddRange(r.Values);
            Console.WriteLine($"Returned {r.Count} regions: {string.Join(", ", r.Select(kv => kv.Key + ": " + kv.Value.Area))}");
        }, 1, true);
        packer.Pack(this.GraphicsDevice);

        using (var stream = File.Create("_Packed.png"))
            packer.PackedTexture.SaveAsPng(stream, packer.PackedTexture.Width, packer.PackedTexture.Height);

        this.OnDraw += (g, t) => {
            this.SpriteBatch.Begin(samplerState: SamplerState.PointClamp);
            var x = 0;
            var y = 10;
            foreach (var r in regions) {
                const int sc = 5;
                this.SpriteBatch.DrawRectangle(new Vector2(x, y), new Vector2(r.Width * sc, r.Height * sc), Color.Green);
                this.SpriteBatch.Draw(r, new Vector2(x, y), Color.White, 0, Vector2.Zero, sc, SpriteEffects.None, 0);
                x += r.Width * sc + 1;
                if (x >= 1000) {
                    x = 0;
                    y += 250;
                }
            }
            this.SpriteBatch.End();
        };*/

        var widthPanel = new Panel(Anchor.Center, Vector2.One, Vector2.Zero, true) {SetWidthBasedOnChildren = true};
        for (var i = 0; i < 5; i++)
            widthPanel.AddChild(new Paragraph(Anchor.AutoCenter, 100000, "Test String " + Math.Pow(10, i), true) {
                OnUpdated = (e, time) => {
                    if (Input.IsPressed(Keys.A)) {
                        e.Anchor = (Anchor) (((int) e.Anchor + 1) % EnumHelper.GetValues<Anchor>().Length);
                        Console.WriteLine(e.Anchor);
                    }
                }
            });
        this.UiSystem.Add("WidthTest", widthPanel);
    }

    protected override void DoUpdate(GameTime gameTime) {
        base.DoUpdate(gameTime);
        if (this.InputHandler.IsKeyPressed(Keys.F11))
            this.GraphicsDeviceManager.SetFullscreen(!this.GraphicsDeviceManager.IsFullScreen);

        var delta = this.InputHandler.ScrollWheel - this.InputHandler.LastScrollWheel;
        if (delta != 0) {
            this.camera.Zoom(0.1F * Math.Sign(delta), this.InputHandler.ViewportMousePosition.ToVector2());
        }

        /*if (Input.InputsDown.Length > 0)
            Console.WriteLine("Down: " + string.Join(", ", Input.InputsDown));*/
        /*if (MlemGame.Input.InputsPressed.Length > 0)
            Console.WriteLine("Pressed: " + string.Join(", ", MlemGame.Input.InputsPressed));
        MlemGame.Input.HandleKeyboardRepeats = false;
        Console.WriteLine("Down time: " + MlemGame.Input.GetDownTime(Keys.A));
        Console.WriteLine("Time since press: " + MlemGame.Input.GetTimeSincePress(Keys.A));
        Console.WriteLine("Up time: " + MlemGame.Input.GetUpTime(Keys.A));*/
    }

    protected override void DoDraw(GameTime gameTime) {
        this.GraphicsDevice.Clear(Color.Black);
        this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, this.camera.ViewMatrix);
        /*this.mapRenderer.Draw(this.SpriteBatch, this.camera.GetVisibleRectangle().ToExtended());

        foreach (var tile in this.collisions.GetCollidingTiles(new RectangleF(0, 0, this.map.Width, this.map.Height))) {
            foreach (var area in tile.Collisions) {
                this.SpriteBatch.DrawRectangle(area.Position * this.map.GetTileSize(), area.Size * this.map.GetTileSize(), Color.Red);
            }
        }*/

        this.SpriteBatch.End();
        base.DoDraw(gameTime);
    }

    private class Test {

        public Vector2 Vec;
        public Point Point;
        public Direction2 Dir { get; set; }
        public Test OtherTest;

        public Test(Vector2 test, string test2) {
            Console.WriteLine("Constructed with " + test + ", " + test2);
        }

        public override string ToString() {
            return $"{this.GetHashCode()}: {nameof(this.Vec)}: {this.Vec}, {nameof(this.Point)}: {this.Point}, {nameof(this.OtherTest)}: {this.OtherTest}, {nameof(this.Dir)}: {this.Dir}";
        }

    }

}
