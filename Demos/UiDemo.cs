using System;
using System.Collections.Generic;
using System.Linq;
using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Input;
using MLEM.Misc;
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
    public class UiDemo : Demo {

        private Texture2D testTexture;
        private NinePatch testPatch;

        public UiDemo(MlemGame game) : base(game) {
        }

        public override void LoadContent() {
            this.testTexture = LoadContent<Texture2D>("Textures/Test");
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);
            var tree = new TextureRegion(LoadContent<Texture2D>("Textures/Tree"));
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
            var untexturedStyle = new UntexturedStyle(this.SpriteBatch);
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

            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is a small demo for MLEM.Ui, a user interface library that is part of (M)LEM (L)ibrary by (E)llpeck for (M)onoGame."));
            var image = root.AddChild(new Image(Anchor.AutoCenter, new Vector2(50, 50), new TextureRegion(this.testTexture, 0, 0, 8, 8)) {IsHidden = true, Padding = new Point(3)});
            // Setting the x or y coordinate of the size to 1 or a lower number causes the width or height to be a percentage of the parent's width or height
            // (for example, setting the size's x to 0.75 would make the element's width be 0.75*parentWidth)
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Toggle Test Image", "This button shows a grass tile as a test image to show the automatic anchoring of objects.") {
                OnPressed = element => image.IsHidden = !image.IsHidden
            });

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Note that the default style does not contain any textures or font files and, as such, is quite bland. However, the default style is quite easy to override."));
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Change Style") {
                OnPressed = element => this.UiSystem.Style = this.UiSystem.Style == untexturedStyle ? style : untexturedStyle,
                PositionOffset = new Vector2(0, 1),
                // set HasCustomStyle to true before changing style information so that, when changing the style globally
                // (like above), these custom values don't get undone
                HasCustomStyle = true,
                Texture = this.testPatch,
                HoveredColor = Color.LightGray,
                SelectionIndicator = style.SelectionIndicator
            });

            root.AddChild(new VerticalSpace(3));

            // a paragraph with formatting codes. To see them all or to add more, check the TextFormatting class
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Paragraphs can also contain [Blue]formatting codes[White], including colors and [Italic]text styles[Regular]. The names of all [Orange]MonoGame Colors[White] can be used, as well as the codes [Italic]Italic[Regular] and [Bold]Bold[Regular]. \n[Italic]Even [CornflowerBlue]Cornflower Blue[White] works!"));

            // adding some custom image formatting codes
            // note that all added formatting codes need to be lowercase, while their casing doesn't matter when used
            TextFormatting.FormattingCodes["grass"] = new FormattingCode(image.Texture);
            TextFormatting.FormattingCodes["tree"] = new FormattingCode(tree);
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Additionally, you can create custom formatting codes that contain [Grass] images! Note that these images have to be square, or [Tree] bad things happen."));

            var animatedPar = root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Defining text animations as formatting codes is also possible, including [Wobbly]wobbly text[Unanimated] as well as a [Typing]dialogue-esc typing effect by default. Of course, more animations can be added though."));
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Reset Typing Animation") {
                // to reset any animation, simply change the paragraph's TimeIntoAnimation
                OnPressed = e => animatedPar.TimeIntoAnimation = TimeSpan.Zero
            });

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
                OnPressed = element => {
                    if (element.Root.Scale < 2)
                        element.Root.Scale += 0.1F;
                }
            });
            root.AddChild(new Button(Anchor.AutoInline, new Vector2(10), "-") {
                OnPressed = element => {
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
            this.UiSystem.Add("TestTooltip", tooltip).CanSelectContent = false;
            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Toggle Test Tooltip") {
                OnPressed = element => tooltip.IsHidden = !tooltip.IsHidden
            });

            var slider = new Slider(Anchor.AutoLeft, new Vector2(1, 10), 5, 1) {
                StepPerScroll = 0.01F
            };
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, paragraph => "Slider is at " + (slider.CurrentValue * 100).Floor() + "%") {PositionOffset = new Vector2(0, 1)});
            root.AddChild(slider);

            // Check the WobbleButton method for an explanation of how this button works
            var group = root.AddChild(new CustomDrawGroup(Anchor.AutoLeft, new Vector2(1, 0)));
            group.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Wobble Me", "This button wobbles around visually when clicked, but this doesn't affect its actual size and positioning") {
                OnPressed = element => CoroutineHandler.Start(this.WobbleButton(group)),
                PositionOffset = new Vector2(0, 1)
            });
            root.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Transform Ui", "This button causes the entire ui to be transformed (both in positioning, rotation and scale). As this is an easily pulled off operation, it can be used for animations and other gimmicks.") {
                OnPressed = element => {
                    if (element.Root.Transform == Matrix.Identity) {
                        element.Root.Transform = Matrix.CreateScale(0.75F) * Matrix.CreateRotationZ(0.25F) * Matrix.CreateTranslation(50, -10, 0);
                    } else {
                        element.Root.Transform = Matrix.Identity;
                    }
                },
                PositionOffset = new Vector2(0, 1)
            });

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Progress bars in multiple orientations:"));
            var bar1 = root.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(1, 8), Direction2.Right, 10) {PositionOffset = new Vector2(0, 1)});
            CoroutineHandler.Start(this.WobbleProgressBar(bar1));
            var bar2 = root.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(1, 8), Direction2.Left, 10) {PositionOffset = new Vector2(0, 1)});
            CoroutineHandler.Start(this.WobbleProgressBar(bar2));
            var bar3 = root.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(8, 30), Direction2.Down, 10) {PositionOffset = new Vector2(0, 1)});
            CoroutineHandler.Start(this.WobbleProgressBar(bar3));
            var bar4 = root.AddChild(new ProgressBar(Anchor.AutoInline, new Vector2(8, 30), Direction2.Up, 10) {PositionOffset = new Vector2(1, 1)});
            CoroutineHandler.Start(this.WobbleProgressBar(bar4));

            root.AddChild(new VerticalSpace(3));
            var dropdown = root.AddChild(new Dropdown(Anchor.AutoLeft, new Vector2(1, 10), "Dropdown Menu"));
            dropdown.AddElement("First Option");
            dropdown.AddElement("Second Option");
            dropdown.AddElement("Third Option");
            dropdown.AddElement(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Button Option"));

            root.AddChild(new VerticalSpace(3));
            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "There are also some additional \"components\" which are created as combinations of other components. You can find all of them in the ElementHelper class. Here are some examples:"));
            root.AddChild(ElementHelper.NumberField(Anchor.AutoLeft, new Vector2(1, 10))).PositionOffset = new Vector2(0, 1);

            root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "There is an easy helper method to make any amount of same-sized columns:") {PositionOffset = new Vector2(0, 1)});
            var cols = ElementHelper.MakeColumns(root, new Vector2(1), 3);
            cols[0].AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is the first column"));
            cols[1].AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is the second column"));
            cols[2].AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is the third column"));

            root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Show Info Box") {
                OnPressed = element => ElementHelper.ShowInfoBox(this.UiSystem, Anchor.Center, 100, "This is an easy info box that you can open with just one line of code! It automatically closes when you press the button below as well."),
                PositionOffset = new Vector2(0, 1)
            });

            root.AddChild(ElementHelper.ImageButton(Anchor.AutoLeft, new Vector2(1, 10), tree, "Button with image")).PositionOffset = new Vector2(0, 1);

            // Below are some querying examples that help you find certain elements easily

            var children = root.GetChildren();
            var totalChildren = root.GetChildren(regardGrandchildren: true);
            Console.WriteLine($"The root has {children.Count()} children, but there are {totalChildren.Count()} when regarding children's children");

            var textFields = root.GetChildren<TextField>();
            Console.WriteLine($"The root has {textFields.Count()} text fields");

            var paragraphs = root.GetChildren<Paragraph>();
            var totalParagraphs = root.GetChildren<Paragraph>(regardGrandchildren: true);
            Console.WriteLine($"The root has {paragraphs.Count()} paragraphs, but there are {totalParagraphs.Count()} when regarding children's children");

            var autoWidthChildren = root.GetChildren(e => e.Size.X == 1);
            var autoWidthButtons = root.GetChildren<Button>(e => e.Size.X == 1);
            Console.WriteLine($"The root has {autoWidthChildren.Count()} auto-width children, {autoWidthButtons.Count()} of which are buttons");
        }

        // This method is used by the wobbling button (see above)
        // Note that this particular example makes use of the Coroutine package, which is not required but makes demonstration easier
        private IEnumerator<Wait> WobbleButton(CustomDrawGroup group) {
            var counter = 0F;
            while (counter < 4 * Math.PI) {
                // A custom draw group allows the implementation of any sort of custom rendering for all of its child components
                // This includes simply changing the transform matrix like here, but also applying custom effects and doing
                // anything else that can be done in the SpriteBatch's Begin call.
                // Note that changing visual features like this
                // has no effect on the ui's actual interaction behavior (mouse position interpretation, for example), but it can
                // be a great way to accomplish feedback animations for buttons and so on.
                group.Transform = Matrix.CreateTranslation((float) Math.Sin(counter / 2) * 10 * group.Scale, 0, 0);
                counter += 0.1F;
                yield return new WaitSeconds(0.01F);
            }
            group.Transform = Matrix.Identity;
        }

        private IEnumerator<Wait> WobbleProgressBar(ProgressBar bar) {
            var reducing = false;
            while (bar.Root != null) {
                if (reducing) {
                    bar.CurrentValue -= 0.1F;
                    if (bar.CurrentValue <= 0)
                        reducing = false;
                } else {
                    bar.CurrentValue += 0.1F;
                    if (bar.CurrentValue >= bar.MaxValue)
                        reducing = true;
                }
                yield return new WaitSeconds(0.01F);
            }
        }

        public override void Clear() {
            this.UiSystem.Remove("Test");
            this.UiSystem.Remove("TestTooltip");
        }

        public override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.DoDraw(gameTime);
        }

    }
}