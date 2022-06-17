using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Coroutine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
using MLEM.Ui.Parsers;
using MLEM.Ui.Style;

namespace Demos {
    public class UiDemo : Demo {

        private Texture2D testTexture;
        private NinePatch testPatch;
        private Panel root;

        public UiDemo(MlemGame game) : base(game) {}

        public override void LoadContent() {
            this.testTexture = Demo.LoadContent<Texture2D>("Textures/Test");
            this.testPatch = new NinePatch(new TextureRegion(this.testTexture, 0, 8, 24, 24), 8);
            base.LoadContent();

            // create a new style
            // this one derives form UntexturedStyle so that stuff like the hover colors don't have to be set again
            var style = new UntexturedStyle(this.SpriteBatch) {
                // when using a SpriteFont, use GenericSpriteFont. When using a MonoGame.Extended BitmapFont, use GenericBitmapFont.
                // Wrapping fonts like this allows for both types to be usable within MLEM.Ui easily
                // Supplying a bold and an italic version is optional
                Font = new GenericSpriteFont(
                    Demo.LoadContent<SpriteFont>("Fonts/TestFont"),
                    Demo.LoadContent<SpriteFont>("Fonts/TestFontBold"),
                    Demo.LoadContent<SpriteFont>("Fonts/TestFontItalic")),
                TextScale = 0.1F,
                PanelTexture = this.testPatch,
                ButtonTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4),
                TextFieldTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4),
                ScrollBarBackground = new NinePatch(new TextureRegion(this.testTexture, 12, 0, 4, 8), 1, 1, 2, 2),
                ScrollBarScrollerTexture = new NinePatch(new TextureRegion(this.testTexture, 8, 0, 4, 8), 1, 1, 2, 2),
                CheckboxTexture = new NinePatch(new TextureRegion(this.testTexture, 24, 8, 16, 16), 4),
                CheckboxCheckmark = new TextureRegion(this.testTexture, 24, 0, 8, 8),
                RadioTexture = new NinePatch(new TextureRegion(this.testTexture, 16, 0, 8, 8), 3),
                RadioCheckmark = new TextureRegion(this.testTexture, 32, 0, 8, 8),
                AdditionalFonts = {{"Monospaced", new GenericSpriteFont(Demo.LoadContent<SpriteFont>("Fonts/MonospacedFont"))}},
                LinkColor = Color.CornflowerBlue
            };
            var untexturedStyle = new UntexturedStyle(this.SpriteBatch) {
                TextScale = style.TextScale,
                Font = style.Font
            };
            // set the defined style as the current one
            this.UiSystem.Style = style;
            // scale every ui up by 5
            this.UiSystem.GlobalScale = 5;
            // set the ui system to automatically scale with screen size
            // this will cause all ui elements to be scaled based on the reference resolution (AutoScaleReferenceSize)
            // by default, the reference resolution is set to the initial screen size, however this value can be changed through the ui system
            this.UiSystem.AutoScaleWithScreen = true;

            // create the root panel that all the other components sit on and add it to the ui system
            this.root = new Panel(Anchor.Center, new Vector2(80, 100), Vector2.Zero, false, true);
            this.root.ScrollBar.SmoothScrolling = true;
            // add the root to the demos' ui
            this.UiRoot.AddChild(this.root);

            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is a small demo for MLEM.Ui, a user interface library that is part of the MLEM Library for Extending MonoGame."));
            var image = this.root.AddChild(new Image(Anchor.AutoCenter, new Vector2(50, 50), new TextureRegion(this.testTexture, 0, 0, 8, 8)) {IsHidden = true, Padding = new Padding(3)});
            // Setting the x or y coordinate of the size to 1 or a lower number causes the width or height to be a percentage of the parent's width or height
            // (for example, setting the size's x to 0.75 would make the element's width be 0.75*parentWidth)
            this.root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Toggle Grass Image", "This button shows a grass tile above it to show the automatic anchoring of objects.") {
                OnPressed = element => image.IsHidden = !image.IsHidden
            });

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Note that the default style does not contain any textures and, as such, is quite bland. However, the default style is quite easy to override, as can be seen in the code for this demo."));
            this.root.AddChild(new Button(Anchor.AutoCenter, new Vector2(1, 10), "Change Style") {
                OnPressed = element => this.UiSystem.Style = this.UiSystem.Style == untexturedStyle ? style : untexturedStyle,
                PositionOffset = new Vector2(0, 1),
                Style = untexturedStyle
            });

            this.root.AddChild(new VerticalSpace(3));

            // a paragraph with formatting codes. To see them all or to add more, check the TextFormatting class
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Paragraphs can also contain <c Blue>formatting codes</c>, including colors and <i>text styles</i>. The names of all <c Orange>MonoGame Colors</c> can be used, as well as the codes <i>Italic</i>, <b>Bold</b>, <s>Drop Shadow'd</s> and <s><c Pink>mixed formatting</s></c>. You can also add additional fonts for things like\n<f Monospaced>void Code() {\n  // Code\n}</f>\n<i>Even <c #ff611f82>inline custom colors</c> work!</i>"));

            // adding some custom image formatting codes
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Additionally, you can create custom formatting codes that contain <i Grass> images and more!"));
            this.UiSystem.TextFormatter.AddImage("Grass", image.Texture);
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Defining text animations as formatting codes is also possible, including <a wobbly>wobbly text</a> at <a wobbly 8 0.25>different intensities</a>. Of course, more animations can be added though."));

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "Multiline text input:", true));
            this.root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 50), multiline: true) {
                PositionOffset = new Vector2(0, 1),
                PlaceholderText = "Click here to input text"
            });

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "Numbers-only input:", true));
            this.root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 10), TextField.OnlyNumbers) {PositionOffset = new Vector2(0, 1)});

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoCenter, 1, "Password-style input:", true));
            this.root.AddChild(new TextField(Anchor.AutoLeft, new Vector2(1, 10), text: "secret") {
                PositionOffset = new Vector2(0, 1),
                MaskingCharacter = '*'
            });

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Zoom in or out:"));
            this.root.AddChild(new Button(Anchor.AutoLeft, new Vector2(10), "+") {
                OnPressed = element => {
                    if (element.Root.Scale < 2)
                        element.Root.Scale += 0.1F;
                }
            });
            this.root.AddChild(new Button(Anchor.AutoInline, new Vector2(10), "-") {
                OnPressed = element => {
                    if (element.Root.Scale > 0.5F)
                        element.Root.Scale -= 0.1F;
                },
                PositionOffset = new Vector2(1, 0)
            });

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Checkbox(Anchor.AutoLeft, new Vector2(1, 10), "Checkbox 1!"));
            this.root.AddChild(new Checkbox(Anchor.AutoLeft, new Vector2(1, 10), "Checkbox 2!") {PositionOffset = new Vector2(0, 1)});

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new RadioButton(Anchor.AutoLeft, new Vector2(1, 10), "Radio button 1!"));
            this.root.AddChild(new RadioButton(Anchor.AutoLeft, new Vector2(1, 10), "Radio button 2!") {PositionOffset = new Vector2(0, 1)});

            var tooltip = new Tooltip("I am tooltip!") {IsHidden = true};
            this.UiSystem.Add("TestTooltip", tooltip);
            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Toggle Mouse Tooltip") {
                OnPressed = element => tooltip.IsHidden = !tooltip.IsHidden
            });
            var delayed = this.root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Delayed Tooltip") {PositionOffset = new Vector2(0, 1)});
            delayed.AddTooltip("This tooltip appears with a half second delay!").Delay = TimeSpan.FromSeconds(0.5);
            var condition = this.root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Hold Ctrl for Tooltip") {PositionOffset = new Vector2(0, 1)});
            condition.AddTooltip(p => this.InputHandler.IsModifierKeyDown(ModifierKey.Control) ? "This tooltip only appears when holding control!" : string.Empty);

            var slider = new Slider(Anchor.AutoLeft, new Vector2(1, 10), 5, 1) {
                StepPerScroll = 0.01F
            };
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, paragraph => "Slider is at " + (slider.CurrentValue * 100).Floor() + "%") {PositionOffset = new Vector2(0, 1)});
            this.root.AddChild(slider);

            // Check the WobbleButton method for an explanation of how this button works
            this.root.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Wobble Me", "This button wobbles around visually when clicked, but this doesn't affect its actual size and positioning") {
                OnPressed = element => CoroutineHandler.Start(UiDemo.WobbleButton(element)),
                PositionOffset = new Vector2(0, 1)
            });
            // Another button that shows animations!
            var fancyHoverTimer = 0D;
            var fancyButton = this.root.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Fancy Hover") {
                PositionOffset = new Vector2(0, 1),
                OnUpdated = (e, time) => {
                    if (e.IsMouseOver && fancyHoverTimer <= 0.5F)
                        return;
                    if (fancyHoverTimer > 0) {
                        fancyHoverTimer -= time.ElapsedGameTime.TotalSeconds * 3;
                        e.ScaleTransform(1 + (float) Math.Sin(fancyHoverTimer * MathHelper.Pi) * 0.05F);
                    } else {
                        e.Transform = Matrix.Identity;
                    }
                }
            });
            fancyButton.OnMouseEnter += e => fancyHoverTimer = 1;
            this.root.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 10), "Transform Ui", "This button causes the entire ui to be transformed (both in positioning, rotation and scale)") {
                OnPressed = element => {
                    if (element.Root.Transform == Matrix.Identity) {
                        element.Root.Transform = Matrix.CreateScale(0.75F) * Matrix.CreateRotationZ(0.25F) * Matrix.CreateTranslation(50, -10, 0);
                    } else {
                        element.Root.Transform = Matrix.Identity;
                    }
                },
                PositionOffset = new Vector2(0, 1)
            });

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "Progress bars!"));
            var bar1 = this.root.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(1, 8), Direction2.Right, 10) {PositionOffset = new Vector2(0, 1)});
            CoroutineHandler.Start(UiDemo.WobbleProgressBar(bar1));
            var bar2 = this.root.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(1, 8), Direction2.Left, 10) {PositionOffset = new Vector2(0, 1)});
            CoroutineHandler.Start(UiDemo.WobbleProgressBar(bar2));
            var bar3 = this.root.AddChild(new ProgressBar(Anchor.AutoLeft, new Vector2(8, 30), Direction2.Down, 10) {PositionOffset = new Vector2(0, 1)});
            CoroutineHandler.Start(UiDemo.WobbleProgressBar(bar3));
            var bar4 = this.root.AddChild(new ProgressBar(Anchor.AutoInline, new Vector2(8, 30), Direction2.Up, 10) {PositionOffset = new Vector2(1, 0)});
            CoroutineHandler.Start(UiDemo.WobbleProgressBar(bar4));

            this.root.AddChild(new VerticalSpace(3));
            var dropdown = this.root.AddChild(new Dropdown(Anchor.AutoLeft, new Vector2(1, 10), "Dropdown Menu"));
            dropdown.AddElement("First Option");
            dropdown.AddElement("Second Option");
            dropdown.AddElement("Third Option");
            dropdown.AddElement(new Paragraph(Anchor.AutoLeft, 1, "Dropdowns are basically just prioritized panels, so they can contain all controls, including paragraphs and"));
            dropdown.AddElement(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Buttons"));

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Disabled button", "This button can't be clicked or moved to using automatic navigation") {IsDisabled = true}).PositionOffset = new Vector2(0, 1);
            this.root.AddChild(new Checkbox(Anchor.AutoLeft, new Vector2(1, 10), "Disabled checkbox") {IsDisabled = true}).PositionOffset = new Vector2(0, 1);
            this.root.AddChild(new Button(Anchor.AutoLeft, new Vector2(1, 10), "Disabled tooltip button", "This button can't be clicked, but can be moved to using automatic navigation, and will display its tooltip even when done so.") {
                CanSelectDisabled = true,
                IsDisabled = true,
                Tooltip = {DisplayInAutoNavMode = true},
                PositionOffset = new Vector2(0, 1)
            });

            const string alignText = "Paragraphs can have <l Left>left</l> aligned text, <l Right>right</l> aligned text and <l Center>center</l> aligned text.";
            this.root.AddChild(new VerticalSpace(3));
            var alignPar = this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, alignText));
            alignPar.LinkAction = (l, c) => {
                if (Enum.TryParse<TextAlignment>(c.Match.Groups[1].Value, out var alignment))
                    alignPar.Alignment = alignment;
            };

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "MLEM.Ui also contains a simple Markdown parser, which can be useful for displaying things like changelogs in your game."));
            this.root.AddChild(new VerticalSpace(3));
            var parser = new UiMarkdownParser {GraphicsDevice = this.GraphicsDevice};
            using (var reader = new StreamReader(TitleContainer.OpenStream("Content/Markdown.md")))
                parser.ParseInto(reader.ReadToEnd(), this.root);

            this.root.AddChild(new VerticalSpace(3));
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, "The code for this demo contains some examples for how to query element data. This is the output of that:"));

            var children = this.root.GetChildren();
            var totalChildren = this.root.GetChildren(regardGrandchildren: true);
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, $"The root has <b>{children.Count()}</b> children, but there are <b>{totalChildren.Count()}</b> when regarding children's children"));

            var textFields = this.root.GetChildren<TextField>();
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, $"The root has <b>{textFields.Count()}</b> text fields"));

            var paragraphs = this.root.GetChildren<Paragraph>();
            var totalParagraphs = this.root.GetChildren<Paragraph>(regardGrandchildren: true);
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, $"The root has <b>{paragraphs.Count()}</b> paragraphs, but there are <b>{totalParagraphs.Count()}</b> when regarding children's children"));

            var autoWidthChildren = this.root.GetChildren(e => e.Size.X == 1);
            var autoWidthButtons = this.root.GetChildren<Button>(e => e.Size.X == 1);
            this.root.AddChild(new Paragraph(Anchor.AutoLeft, 1, $"The root has <b>{autoWidthChildren.Count()}</b> auto-width children, <b>{autoWidthButtons.Count()}</b> of which are buttons"));

            // select the first element for auto-navigation
            this.root.Root.SelectElement(this.root.GetChildren().First(c => c.CanBeSelected));
        }

        // This method is used by the wobbling button (see above)
        // Note that this particular example makes use of the Coroutine package, which is not required but makes demonstration easier
        private static IEnumerator<Wait> WobbleButton(Element button) {
            var counter = 0F;
            while (counter < 4 * Math.PI && button.Root != null) {
                // Every element allows the implementation of any sort of custom rendering for itself and all of its child components
                // This includes simply changing the transform matrix like here, but also applying custom effects and doing
                // anything else that can be done in the SpriteBatch's Begin call.
                // Note that changing visual features like this
                // has no effect on the ui's actual interaction behavior (mouse position interpretation, for example), but it can
                // be a great way to accomplish feedback animations for buttons and so on.
                button.Transform = Matrix.CreateTranslation((float) Math.Sin(counter / 2) * 10 * button.Scale, 0, 0);
                counter += 0.1F;
                yield return new Wait(0.01F);
            }
            button.Transform = Matrix.Identity;
        }

        private static IEnumerator<Wait> WobbleProgressBar(ProgressBar bar) {
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
                yield return new Wait(0.01F);
            }
        }

        public override void Clear() {
            this.root.Root.Element.RemoveChild(this.root);
            this.UiSystem.Remove("TestTooltip");
        }

        public override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.DoDraw(gameTime);
        }

    }
}
