using System;
using System.Collections.Generic;
using System.Linq;
using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace Demos {
    /// <remarks>
    /// NOTE: This ui demo derives from <see cref="MlemGame"/>. To use MLEM.Ui, it's not required to use MLEM.Startup (which MlemGame is a part of).
    /// If using your own game class that derives from <see cref="Game"/>, however, you will have to do a few additional things to get MLEM.Ui up and running:
    /// - Create an instance of <see cref="UiSystem"/>
    /// - Call the instance's Update method in your game's Update method
    /// - Call the instance's DrawEarly method before clearing your <see cref="GraphicsDevice"/>
    /// - Call the instance's Draw method in your game's Draw method
    /// </remarks>
    public class UiDemo : MlemGame {

        private Texture2D testTexture;
        private NinePatch testPatch;

        public UiDemo() {
            this.IsMouseVisible = true;
        }

        protected override void LoadContent() {
            this.testTexture = LoadContent<Texture2D>("Textures/Test");
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);
            base.LoadContent();

            // create a new style
            // this one derives form UntexturedStyle so that stuff like the hover colors don't have to be set again
            var style = new UntexturedStyle(this.SpriteBatch) {
                // when using a SpriteFont, use GenericSpriteFont. When using a MonoGame.Extended BitmapFont, use GenericBitmapFont.
                // Wrapping fonts like this allows for both types to be usable within MLEM.Ui easily
                Font = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFont")),
                BoldFont = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFontBold")),
                ItalicFont = new GenericSpriteFont(LoadContent<SpriteFont>("Fonts/TestFontItalic")),
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
            // set the defined style as the current one
            this.UiSystem.Style = style;
            // scale every ui up by 5
            this.UiSystem.GlobalScale = 5;
            // set the ui system to automatically scale with screen size
            // this will cause all ui elements to be scaled based on the reference resolution (AutoScaleReferenceSize)
            // by default, the reference resolution is set to the initial screen size, however this value can be changed through the ui system
            this.UiSystem.AutoScaleWithScreen = true;

            // create the root panel that all the other components sit on and add it to the ui system
            var root = new Panel(Anchor.Center, new Vector2(80, 100), Vector2.Zero, false, true, new Point(5, 10));
            this.UiSystem.Add("Test", root);

            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is a small demo for MLEM.Ui, a user interface library that is part of (M)LEM (L)ibrary by (E)llpeck for (M)onoGame.") {LineSpace = 1.5F});
            var image = root.AddChild(new Image(Anchor.AutoCenter, new Vector2(50, 50), new TextureRegion(this.testTexture, 0, 0, 8, 8)) {IsHidden = true, Padding = new Point(3)});
            // Setting the x or y coordinate of the size to 1 or a lower number causes the width or height to be a percentage of the parent's width or height
            // (for example, setting the size's x to 0.75 would make the element's width be 0.75*parentWidth)
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
                // set HasCustomStyle to true before changing style information so that, when changing the style globally
                // (like above), these custom values don't get undone
                HasCustomStyle = true,
                Texture = this.testPatch,
                HoveredColor = Color.LightGray
            });

            root.AddChild(new VerticalSpace(3));
            // a paragraph with formatting codes. To see them all or to add more, check the TextFormatting class
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1,"Paragraphs can also contain [Blue]formatting codes[White], including colors and [Italic]text styles[Regular]. The names of all [Orange]MonoGame Colors[White] can be used, as well as the codes [Italic]Italic[Regular] and [Bold]Bold[Regular]. \n[Italic]Even [CornflowerBlue]Cornflower Blue[White] works!"));

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "Text input:", true));
            root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 10)) {
                PositionOffset = new Vector2(0, 1),
                PlaceholderText = "Click here to input text"
            });

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "Numbers-only input:", true));
            root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 10), TextField.OnlyNumbers) {PositionOffset = new Vector2(0, 1)});

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

            var tooltip = new Tooltip(50, "This is a test tooltip to see the window bounding") {IsHidden = true};
            this.UiSystem.Add("TestTooltip", tooltip);
            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Toggle Test Tooltip") {
                OnClicked = (element, button) => {
                    if (button == MouseButton.Left)
                        tooltip.IsHidden = !tooltip.IsHidden;
                }
            });

            var slider = new Slider(Anchor.AutoLeft, new Vector2(1, 10), 5, 1);
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, paragraph => "Slider is at " + (slider.CurrentValue * 100).Floor() + "%") {PositionOffset = new Vector2(0, 1)});
            root.AddChild(slider);

            // This button uses a coroutine from my Coroutine NuGet package (which is included with MLEM.Startup)
            // but the important thing it does is change its visual scale and offset (check the method below for more info)
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Wobble", "This button wobbles around visually when clicked, but this doesn't affect its actual size and positioning") {
                OnClicked = (element, button) => CoroutineHandler.Start(this.WobbleButton(element)),
                PositionOffset = new Vector2(0, 1)
            });

            // Below are some querying examples that help you find certain elements easily

            var children = root.GetChildren();
            var totalChildren = root.GetChildren(regardChildrensChildren: true);
            Console.WriteLine($"The root has {children.Count()} children, but there are {totalChildren.Count()} when regarding children's children");

            var textFields = root.GetChildren<TextField>();
            Console.WriteLine($"The root has {textFields.Count()} text fields");

            var paragraphs = root.GetChildren<Paragraph>();
            var totalParagraphs = root.GetChildren<Paragraph>(regardChildrensChildren: true);
            Console.WriteLine($"The root has {paragraphs.Count()} paragraphs, but there are {totalParagraphs.Count()} when regarding children's children");

            var autoWidthChildren = root.GetChildren(e => e.Size.X == 1);
            var autoWidthButtons = root.GetChildren<Button>(e => e.Size.X == 1);
            Console.WriteLine($"The root has {autoWidthChildren.Count()} auto-width children, {autoWidthButtons.Count()} of which are buttons");
        }

        // This method is used by the wobbling button (see above)
        // Note that this particular example makes use of the Coroutine package
        private IEnumerator<Wait> WobbleButton(Element button) {
            var counter = 0F;
            while (counter < 10) {
                // The imporant bit is that it changes its added display scale and offset, allowing the button to still maintain the
                // correct position and scaling for both anchoring and interacting purposes, but to show any kind of animation visually
                // This could be useful, for example, to create a little feedback effect to clicking it where it changes size for a second
                button.AddedDisplayScale = new Vector2((float) Math.Sin(counter));
                button.AddedDisplayOffset = new Vector2((float) Math.Sin(counter / 2) * 4, 0);
                counter += 0.1F;
                yield return new WaitSeconds(0.01F);
            }
            button.AddedDisplayScale = Vector2.Zero;
            button.AddedDisplayOffset = Vector2.Zero;
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.DoDraw(gameTime);
        }

    }
}