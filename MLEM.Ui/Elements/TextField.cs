using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Graphics;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
using TextCopy;
#endif

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A text field element for use inside of a <see cref="UiSystem"/>.
    /// A text field is a selectable element that can be typed in, as well as copied and pasted from.
    /// If an on-screen keyboard is required, then this text field will automatically open an on-screen keyboard using <see cref="MlemPlatform.OpenOnScreenKeyboard"/>.
    /// This class interally uses MLEM's <see cref="TextInput"/>.
    /// </summary>
    public class TextField : Element {

        /// <inheritdoc cref="TextInput.DefaultRule"/>
        public static readonly Rule DefaultRule = (field, add) => TextInput.DefaultRule(field.textInput, add);
        /// <inheritdoc cref="TextInput.OnlyLetters"/>
        public static readonly Rule OnlyLetters = (field, add) => TextInput.OnlyLetters(field.textInput, add);
        /// <inheritdoc cref="TextInput.OnlyNumbers"/>
        public static readonly Rule OnlyNumbers = (field, add) => TextInput.OnlyNumbers(field.textInput, add);
        /// <inheritdoc cref="TextInput.LettersNumbers"/>
        public static readonly Rule LettersNumbers = (field, add) => TextInput.LettersNumbers(field.textInput, add);
        /// <inheritdoc cref="TextInput.PathNames"/>
        public static readonly Rule PathNames = (field, add) => TextInput.PathNames(field.textInput, add);
        /// <inheritdoc cref="TextInput.FileNames"/>
        public static readonly Rule FileNames = (field, add) => TextInput.FileNames(field.textInput, add);

#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
        /// <summary>
        /// An event that is raised when an exception is thrown while trying to copy or paste clipboard contents using TextCopy.
        /// If no event handlers are added, the exception is ignored.
        /// </summary>
        public static event Action<Exception> OnCopyPasteException;
#endif

        /// <summary>
        /// The color that this text field's text should display with
        /// </summary>
        public StyleProp<Color> TextColor;
        /// <summary>
        /// The color that the <see cref="PlaceholderText"/> should display with
        /// </summary>
        public StyleProp<Color> PlaceholderColor;
        /// <summary>
        /// This text field's texture
        /// </summary>
        public StyleProp<NinePatch> Texture;
        /// <summary>
        /// This text field's texture while it is hovered
        /// </summary>
        public StyleProp<NinePatch> HoveredTexture;
        /// <summary>
        /// The color that this text field should display with while it is hovered
        /// </summary>
        public StyleProp<Color> HoveredColor;
        /// <summary>
        /// The scale that this text field should render text with
        /// </summary>
        public StyleProp<float> TextScale {
            get => this.textScale;
            set {
                this.textScale = value;
                this.textInput.TextScale = value;
            }
        }
        /// <summary>
        /// The font that this text field should display text with
        /// </summary>
        public StyleProp<GenericFont> Font {
            get => this.font;
            set {
                this.font = value;
                this.textInput.Font = value;
            }
        }
        /// <summary>
        /// The x position that text should start rendering at, based on the x position of this text field.
        /// </summary>
        public StyleProp<float> TextOffsetX;
        /// <summary>
        /// The width that the caret should render with, in pixels
        /// </summary>
        public StyleProp<float> CaretWidth;

        /// <inheritdoc cref="TextInput.Text"/>
        public string Text => this.textInput.Text;
        /// <inheritdoc cref="TextInput.OnTextChange"/>
        public TextChanged OnTextChange;
        /// <inheritdoc cref="TextInput.InputRule"/>
        public Rule InputRule;
        /// <inheritdoc cref="TextInput.CaretPos"/>
        public int CaretPos {
            get => this.textInput.CaretPos;
            set => this.textInput.CaretPos = value;
        }
        /// <inheritdoc cref="TextInput.CaretLine"/>
        public int CaretLine => this.textInput.CaretLine;
        /// <inheritdoc cref="TextInput.CaretPosInLine"/>
        public int CaretPosInLine => this.textInput.CaretPosInLine;
        /// <inheritdoc cref="TextInput.MaskingCharacter"/>
        public char? MaskingCharacter {
            get => this.textInput.MaskingCharacter;
            set => this.textInput.MaskingCharacter = value;
        }
        /// <inheritdoc cref="TextInput.MaximumCharacters"/>
        public int? MaximumCharacters {
            get => this.textInput.MaximumCharacters;
            set => this.textInput.MaximumCharacters = value;
        }
        /// <inheritdoc cref="TextInput.Multiline"/>
        public bool Multiline {
            get => this.textInput.Multiline;
            set => this.textInput.Multiline = value;
        }

#if FNA
        /// <inheritdoc />
        // we need to make sure that the enter press doesn't get consumed by our press function so that it still works in TextInput
        public override bool CanBePressed => base.CanBePressed && !this.IsSelected;
#endif

        /// <summary>
        /// The text that displays in this text field if <see cref="Text"/> is empty
        /// </summary>
        public string PlaceholderText;
        /// <summary>
        /// The title of the <c>KeyboardInput</c> field on mobile devices and consoles
        /// </summary>
        public string MobileTitle;
        /// <summary>
        /// The description of the <c>KeyboardInput</c> field on mobile devices and consoles
        /// </summary>
        public string MobileDescription;

        private readonly TextInput textInput;
        private StyleProp<GenericFont> font;
        private StyleProp<float> textScale;

        /// <summary>
        /// Creates a new text field with the given settings
        /// </summary>
        /// <param name="anchor">The text field's anchor</param>
        /// <param name="size">The text field's size</param>
        /// <param name="rule">The text field's input rule</param>
        /// <param name="font">The font to use for drawing text</param>
        /// <param name="text">The text that the text field should contain by default</param>
        /// <param name="multiline">Whether the text field should support multi-line editing</param>
        public TextField(Anchor anchor, Vector2 size, Rule rule = null, GenericFont font = null, string text = null, bool multiline = false) : base(anchor, size) {
            this.textInput = new TextInput(null, Vector2.Zero, 1
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
                , null, s => {
                    try {
                        ClipboardService.SetText(s);
                    } catch (Exception e) {
                        TextField.OnCopyPasteException?.Invoke(e);
                    }
                }, () => {
                    try {
                        return ClipboardService.GetText();
                    } catch (Exception e) {
                        TextField.OnCopyPasteException?.Invoke(e);
                        return null;
                    }
                }
#endif
            ) {
                OnTextChange = (i, s) => this.OnTextChange?.Invoke(this, s),
                InputRule = (i, s) => this.InputRule.Invoke(this, s)
            };

            this.InputRule = rule ?? TextField.DefaultRule;
            this.Multiline = multiline;
            if (font != null)
                this.Font = font;
            if (text != null)
                this.SetText(text, true);

            MlemPlatform.EnsureExists();

            this.OnPressed += async e => {
                var title = this.MobileTitle ?? this.PlaceholderText;
                var result = await MlemPlatform.Current.OpenOnScreenKeyboard(title, this.MobileDescription, this.Text, false);
                if (result != null)
                    this.SetText(this.Multiline ? result : result.Replace('\n', ' '), true);
            };
            this.OnTextInput += (element, key, character) => {
                if (this.IsSelected && !this.IsHidden)
                    this.textInput.OnTextInput(key, character);
            };
            this.OnDeselected += e => this.CaretPos = 0;
            this.OnSelected += e => this.CaretPos = this.textInput.Length;
        }

        /// <inheritdoc />
        public override void SetAreaAndUpdateChildren(RectangleF area) {
            base.SetAreaAndUpdateChildren(area);
            this.textInput.Size = this.DisplayArea.Size / this.Scale - new Vector2(2 * this.TextOffsetX);
            this.textInput.TextScale = this.TextScale;
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);
            if (this.IsSelected && !this.IsHidden)
                this.textInput.Update(time, this.Input);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                tex = this.HoveredTexture.OrDefault(tex);
                color = (Color) this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea, color, this.Scale);

            var lineHeight = this.Font.Value.LineHeight * this.TextScale * this.Scale;
            var textPos = this.DisplayArea.Location + new Vector2(
                this.TextOffsetX * this.Scale,
                this.Multiline ? this.TextOffsetX * this.Scale : this.DisplayArea.Height / 2 - lineHeight / 2);
            if (this.textInput.Length > 0 || this.IsSelected) {
                this.textInput.Draw(batch, textPos, this.Scale, this.IsSelected ? this.CaretWidth : 0, this.TextColor.OrDefault(Color.White) * alpha);
            } else if (this.PlaceholderText != null) {
                this.Font.Value.DrawString(batch, this.PlaceholderText, textPos, this.PlaceholderColor.OrDefault(Color.Gray) * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);
            }
            base.Draw(time, batch, alpha, context);
        }

        /// <inheritdoc cref="TextInput.SetText"/>
        public void SetText(object text, bool removeMismatching = false) {
            this.textInput.SetText(text, removeMismatching);
        }

        /// <inheritdoc cref="TextInput.InsertText"/>
        public void InsertText(object text, bool removeMismatching = false) {
            this.textInput.InsertText(text, removeMismatching);
        }

        /// <inheritdoc cref="TextInput.RemoveText"/>
        public void RemoveText(int index, int length) {
            this.textInput.RemoveText(index, length);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale = this.TextScale.OrStyle(style.TextScale);
            this.Font = this.Font.OrStyle(style.Font);
            this.Texture = this.Texture.OrStyle(style.TextFieldTexture);
            this.HoveredTexture = this.HoveredTexture.OrStyle(style.TextFieldHoveredTexture);
            this.HoveredColor = this.HoveredColor.OrStyle(style.TextFieldHoveredColor);
            this.TextOffsetX = this.TextOffsetX.OrStyle(style.TextFieldTextOffsetX);
            this.CaretWidth = this.CaretWidth.OrStyle(style.TextFieldCaretWidth);
        }

        /// <summary>
        /// A delegate method used for <see cref="TextField.OnTextChange"/>
        /// </summary>
        /// <param name="field">The text field whose text changed</param>
        /// <param name="text">The new text</param>
        public delegate void TextChanged(TextField field, string text);

        /// <summary>
        /// A delegate method used for <see cref="InputRule"/>.
        /// It should return whether the given text can be added to the text field.
        /// </summary>
        /// <param name="field">The text field</param>
        /// <param name="textToAdd">The text that is tried to be added</param>
        public delegate bool Rule(TextField field, string textToAdd);

    }
}
