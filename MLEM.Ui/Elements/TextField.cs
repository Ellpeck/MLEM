using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;
using TextCopy;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A text field element for use inside of a <see cref="UiSystem"/>.
    /// A text field is a selectable element that can be typed in, as well as copied and pasted from.
    /// If <see cref="TextInputWrapper.RequiresOnScreenKeyboard"/> is enabled, then this text field will automatically open an on-screen keyboard when pressed using MonoGame's <c>KeyboardInput</c> class.
    /// </summary>
    public class TextField : Element {

        /// <summary>
        /// A <see cref="Rule"/> that allows any visible character and spaces
        /// </summary>
        public static readonly Rule DefaultRule = (field, add) => !add.Any(char.IsControl);
        /// <summary>
        /// A <see cref="Rule"/> that only allows letters
        /// </summary>
        public static readonly Rule OnlyLetters = (field, add) => add.All(char.IsLetter);
        /// <summary>
        /// A <see cref="Rule"/> that only allows numerals
        /// </summary>
        public static readonly Rule OnlyNumbers = (field, add) => add.All(char.IsNumber);
        /// <summary>
        /// A <see cref="Rule"/> that only allows letters and numerals
        /// </summary>
        public static readonly Rule LettersNumbers = (field, add) => add.All(c => char.IsLetter(c) || char.IsNumber(c));
        /// <summary>
        /// A <see cref="Rule"/> that only allows characters not contained in <see cref="Path.GetInvalidPathChars"/>
        /// </summary>
        public static readonly Rule PathNames = (field, add) => add.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        /// <summary>
        /// A <see cref="Rule"/> that only allows characters not contained in <see cref="Path.GetInvalidFileNameChars"/>
        /// </summary>
        public static readonly Rule FileNames = (field, add) => add.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

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
        public StyleProp<float> TextScale;
        /// <summary>
        /// The font that this text field should display text with
        /// </summary>
        public StyleProp<GenericFont> Font;
        private readonly StringBuilder text = new StringBuilder();
        /// <summary>
        /// This text field's current text
        /// </summary>
        public string Text => this.text.ToString();
        /// <summary>
        /// The text that displays in this text field if <see cref="Text"/> is empty
        /// </summary>
        public string PlaceholderText;
        /// <summary>
        /// An event that gets called when <see cref="Text"/> changes, either through input, or through a manual change.
        /// </summary>
        public TextChanged OnTextChange;
        /// <summary>
        /// The x position that text should start rendering at, based on the x position of this text field.
        /// </summary>
        public float TextOffsetX = 4;
        /// <summary>
        /// The width that the caret should render with.
        /// </summary>
        public float CaretWidth = 0.5F;
        private double caretBlinkTimer;
        private string displayedText;
        private int textOffset;
        /// <summary>
        /// The rule used for text input.
        /// Rules allow only certain characters to be allowed inside of a text field.
        /// </summary>
        public Rule InputRule;
        /// <summary>
        /// The title of the <c>KeyboardInput</c> field on mobile devices and consoles
        /// </summary>
        public string MobileTitle;
        /// <summary>
        /// The description of the <c>KeyboardInput</c> field on mobile devices and consoles
        /// </summary>
        public string MobileDescription;
        private int caretPos;
        /// <summary>
        /// The position of the caret within the text.
        /// This is always between 0 and the <see cref="string.Length"/> of <see cref="Text"/>
        /// </summary>
        public int CaretPos {
            get {
                this.CaretPos = MathHelper.Clamp(this.caretPos, 0, this.text.Length);
                return this.caretPos;
            }
            set {
                if (this.caretPos != value) {
                    this.caretPos = value;
                    this.HandleTextChange(false);
                }
            }
        }

        /// <summary>
        /// Creates a new text field with the given settings
        /// </summary>
        /// <param name="anchor">The text field's anchor</param>
        /// <param name="size">The text field's size</param>
        /// <param name="rule">The text field's input rule</param>
        /// <param name="font">The font to use for drawing text</param>
        public TextField(Anchor anchor, Vector2 size, Rule rule = null, GenericFont font = null) : base(anchor, size) {
            this.InputRule = rule ?? DefaultRule;
            if (font != null)
                this.Font.Set(font);

            TextInputWrapper.EnsureExists();
            if (TextInputWrapper.Current.RequiresOnScreenKeyboard) {
                this.OnPressed += async e => {
                    var title = this.MobileTitle ?? this.PlaceholderText;
                    var result = await TextInputWrapper.Current.OpenOnScreenKeyboard(title, this.MobileDescription, this.Text, false);
                    if (result != null)
                        this.SetText(result.Replace('\n', ' '), true);
                };
            }
            this.OnTextInput += (element, key, character) => {
                if (!this.IsSelected || this.IsHidden)
                    return;
                if (key == Keys.Back) {
                    if (this.CaretPos > 0) {
                        this.CaretPos--;
                        this.RemoveText(this.CaretPos, 1);
                    }
                } else if (key == Keys.Delete) {
                    this.RemoveText(this.CaretPos, 1);
                } else {
                    this.InsertText(character);
                }
            };
            this.OnDeselected += e => this.CaretPos = 0;
            this.OnSelected += e => this.CaretPos = this.text.Length;
        }

        private void HandleTextChange(bool textChanged = true) {
            // not initialized yet
            if (!this.Font.HasValue())
                return;
            var length = this.Font.Value.MeasureString(this.text.ToString()).X * this.TextScale;
            var maxWidth = this.DisplayArea.Width / this.Scale - this.TextOffsetX * 2;
            if (length > maxWidth) {
                // if we're moving the caret to the left
                if (this.textOffset > this.CaretPos) {
                    this.textOffset = this.CaretPos;
                } else {
                    // if we're moving the caret to the right
                    var importantArea = this.text.ToString(this.textOffset, Math.Min(this.CaretPos, this.text.Length) - this.textOffset);
                    var bound = this.CaretPos - this.Font.Value.TruncateString(importantArea, maxWidth, this.TextScale, true).Length;
                    if (this.textOffset < bound) {
                        this.textOffset = bound;
                    }
                }
                var visible = this.text.ToString(this.textOffset, this.text.Length - this.textOffset);
                this.displayedText = this.Font.Value.TruncateString(visible, maxWidth, this.TextScale);
            } else {
                this.displayedText = this.Text;
                this.textOffset = 0;
            }

            if (textChanged)
                this.OnTextChange?.Invoke(this, this.Text);
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);

            // handle first initialization if not done
            if (this.displayedText == null)
                this.HandleTextChange(false);

            if (!this.IsSelected || this.IsHidden)
                return;

            if (this.Input.IsKeyPressed(Keys.Left)) {
                this.CaretPos--;
            } else if (this.Input.IsKeyPressed(Keys.Right)) {
                this.CaretPos++;
            } else if (this.Input.IsKeyPressed(Keys.Home)) {
                this.CaretPos = 0;
            } else if (this.Input.IsKeyPressed(Keys.End)) {
                this.CaretPos = this.text.Length;
            } else if (this.Input.IsModifierKeyDown(ModifierKey.Control)) {
                if (this.Input.IsKeyPressed(Keys.V)) {
                    var clip = ClipboardService.GetText();
                    if (clip != null)
                        this.InsertText(clip);
                } else if (this.Input.IsKeyPressed(Keys.C)) {
                    // until there is text selection, just copy the whole content
                    ClipboardService.SetText(this.Text);
                }
            }

            this.caretBlinkTimer += time.ElapsedGameTime.TotalSeconds;
            if (this.caretBlinkTimer >= 1)
                this.caretBlinkTimer = 0;
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                tex = this.HoveredTexture.OrDefault(tex);
                color = (Color) this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea, color, this.Scale);

            if (this.displayedText != null) {
                var lineHeight = this.Font.Value.LineHeight * this.TextScale * this.Scale;
                var textPos = this.DisplayArea.Location + new Vector2(this.TextOffsetX * this.Scale, this.DisplayArea.Height / 2 - lineHeight / 2);
                if (this.text.Length > 0 || this.IsSelected) {
                    var textColor = this.TextColor.OrDefault(Color.White);
                    this.Font.Value.DrawString(batch, this.displayedText, textPos, textColor * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);
                    if (this.IsSelected && this.caretBlinkTimer >= 0.5F) {
                        var textSize = this.Font.Value.MeasureString(this.displayedText.Substring(0, this.CaretPos - this.textOffset)) * this.TextScale * this.Scale;
                        batch.Draw(batch.GetBlankTexture(), new RectangleF(textPos.X + textSize.X, textPos.Y, this.CaretWidth * this.Scale, lineHeight), null, textColor * alpha);
                    }
                } else if (this.PlaceholderText != null) {
                    this.Font.Value.DrawString(batch, this.PlaceholderText, textPos, this.PlaceholderColor.OrDefault(Color.Gray) * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);
                }
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        /// <summary>
        /// Replaces this text field's text with the given text.
        /// </summary>
        /// <param name="text">The new text</param>
        /// <param name="removeMismatching">If any characters that don't match the <see cref="InputRule"/> should be left out</param>
        public void SetText(object text, bool removeMismatching = false) {
            if (removeMismatching) {
                var result = new StringBuilder();
                foreach (var c in text.ToString()) {
                    if (this.InputRule(this, c.ToCachedString()))
                        result.Append(c);
                }
                text = result.ToString();
            } else if (!this.InputRule(this, text.ToString()))
                return;
            this.text.Clear();
            this.text.Append(text);
            this.CaretPos = this.text.Length;
            this.HandleTextChange();
        }

        /// <summary>
        /// Inserts the given text at the <see cref="CaretPos"/>
        /// </summary>
        /// <param name="text">The text to insert</param>
        public void InsertText(object text) {
            var strg = text.ToString();
            if (!this.InputRule(this, strg))
                return;
            this.text.Insert(this.CaretPos, strg);
            this.CaretPos += strg.Length;
            this.HandleTextChange();
        }

        /// <summary>
        /// Removes the given amount of text at the given index
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="length">The amount of text to remove</param>
        public void RemoveText(int index, int length) {
            if (index < 0 || index >= this.text.Length)
                return;
            this.text.Remove(index, length);
            this.HandleTextChange();
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale.SetFromStyle(style.TextScale);
            this.Font.SetFromStyle(style.Font);
            this.Texture.SetFromStyle(style.TextFieldTexture);
            this.HoveredTexture.SetFromStyle(style.TextFieldHoveredTexture);
            this.HoveredColor.SetFromStyle(style.TextFieldHoveredColor);
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