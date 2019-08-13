using System;
using System.Collections.Generic;
using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Input;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace Examples {
    public class GameImpl : MlemGame {

        private Texture2D testTexture;
        private NinePatch testPatch;

        public GameImpl() {
            this.IsMouseVisible = true;
        }

        protected override void LoadContent() {
            this.testTexture = LoadContent<Texture2D>("Textures/Test");
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);
            base.LoadContent();

            var style = new UntexturedStyle(this.SpriteBatch) {
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")),
                TextScale = 0.1F,
                PanelTexture = this.testPatch,
                ButtonTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4),
                TextFieldTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4),
                ScrollBarBackground = new NinePatch(new TextureRegion(this.testTexture, 12, 0, 4, 8), 1, 1, 2, 2),
                ScrollBarScrollerTexture = new NinePatch(new TextureRegion(this.testTexture, 8, 0, 4, 8), 1, 1, 2, 2),
                CheckboxTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4),
                CheckboxCheckmark = new TextureRegion(this.testTexture, 24, 0, 8, 8),
                RadioTexture = new NinePatch(new TextureRegion(this.testTexture, 16, 0, 8, 8), 3),
                RadioCheckmark = new TextureRegion(this.testTexture, 32, 0, 8, 8)
            };
            var untexturedStyle = this.UiSystem.Style;
            this.UiSystem.Style = style;
            this.UiSystem.GlobalScale = 5;

            var root = new Panel(Anchor.Center, new Vector2(80, 100), Vector2.Zero, false, true, new Point(5, 10));
            this.UiSystem.Add("Test", root);

            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is a small demo for MLEM.Ui, a user interface library that is part of (M)LEM (L)ibrary by (E)llpeck for (M)onoGame."));
            var image = root.AddChild(new Image(Anchor.AutoCenter, new Vector2(50, 50), new TextureRegion(this.testTexture, 0, 0, 8, 8)) {IsHidden = true, Padding = new Point(3)});
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Toggle Test Image", "This button shows a grass tile as a test image to show the automatic anchoring of objects.") {
                OnClicked = (element, button) => {
                    if (button == MouseButton.Left)
                        image.IsHidden = !image.IsHidden;
                }
            });
            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Note that the default style does not contain any textures or font files and, as such, is quite bland. However, the default style is quite easy to override."));
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Change Style") {
                OnClicked = (element, button) => {
                    if (button == MouseButton.Left)
                        this.UiSystem.Style = this.UiSystem.Style == untexturedStyle ? style : untexturedStyle;
                },
                PositionOffset = new Vector2(0, 1),
                HasCustomStyle = true,
                Texture = this.testPatch,
                HoveredColor = Color.LightGray
            });

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "Text input:", true));
            root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 10)) {PositionOffset = new Vector2(0, 1)});

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Zoom in or out:"));
            root.AddChild(new Button(Anchor.AutoLeft, new Vector2(10), "+") {
                OnClicked = (element, button) => {
                    if (element.Root.Scale < 2)
                        element.Root.Scale += 0.1F;
                }
            });
            root.AddChild(new Button(Anchor.AutoInline, new Vector2(10), "-") {
                OnClicked = (element, button) => {
                    if (element.Root.Scale > 0.5F)
                        element.Root.Scale -= 0.1F;
                },
                PositionOffset = new Vector2(1, 0)
            });

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Checkbox(Anchor.AutoLeft, new Vector2(1, 10), "Checkbox 1!"));
            root.AddChild(new Checkbox(Anchor.AutoLeft, new Vector2(1, 10), "Checkbox 2!") {PositionOffset = new Vector2(0, 1)});

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new RadioButton(Anchor.AutoLeft, new Vector2(1, 10), "Radio button 1!"));
            root.AddChild(new RadioButton(Anchor.AutoLeft, new Vector2(1, 10), "Radio button 2!") {PositionOffset = new Vector2(0, 1)});
        }

        protected override void Draw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);
            base.Draw(gameTime);
        }

    }
}