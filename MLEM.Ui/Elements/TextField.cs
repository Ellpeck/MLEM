using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Graphics;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;
using TextCopy;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A text field element for use inside of a <see cref="UiSystem"/>.
    /// A text field is a selectable element that can be typed in, as well as copied and pasted from.
    /// If an on-screen keyboard is required, then this text field will automatically open an on-screen keyboard using <see cref="MlemPlatform.OpenOnScreenKeyboard"/>.
    /// </summary>
    public class TextField : Element {

        /// <summary>
        /// A <see cref="Rule"/> that allows any visible character and spaces
        /// </summary>
        public static readonly Rule DefaultRule = (field, add) => {
            foreach (var c in add) {
                if (char.IsControl(c) && (!field.Multiline || c != '\n'))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows letters
        /// </summary>
        public static readonly Rule OnlyLetters = (field, add) => {
            foreach (var c in add) {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows numerals
        /// </summary>
        public static readonly Rule OnlyNumbers = (field, add) => {
            foreach (var c in add) {
                if (!char.IsNumber(c))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows letters and numerals
        /// </summary>
        public static readonly Rule LettersNumbers = (field, add) => {
            foreach (var c in add) {
                if (!char.IsLetter(c) || !char.IsNumber(c))
                    return false;
            }
            return true;
        };
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
        public StyleProp<float> TextOffsetX;
        /// <summary>
        /// The width that the caret should render with, in pixels
        /// </summary>
        public StyleProp<float> CaretWidth;
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
        /// <summary>
        /// The position of the caret within the text.
        /// This is always between 0 and the <see cref="string.Length"/> of <see cref="Text"/>
        /// </summary>
        public int CaretPos {
            get => this.caretPos;
            set {
                var val = MathHelper.Clamp(value, 0, this.text.Length);
                if (this.caretPos != val) {
                    this.caretPos = val;
                    this.caretBlinkTimer = 0;
                    this.HandleTextChange(false);
                }
            }
        }
        /// <summary>
        /// The line of text that the caret is currently on.
        /// This can only be only non-0 if <see cref="Multiline"/> is true.
        /// </summary>
        public int CaretLine { get; private set; }
        /// <summary>
        /// The position in the current <see cref="CaretLine"/> that the caret is currently on.
        /// If <see cref="Multiline"/> is false, this value is always equal to <see cref="CaretPos"/>.
        /// </summary>
        public int CaretPosInLine { get; private set; }
        /// <summary>
        /// A character that should be displayed instead of this text field's <see cref="Text"/> content.
        /// The amount of masking characters displayed will be equal to the <see cref="Text"/>'s length.
        /// This behavior is useful for password fields or similar.
        /// </summary>
        public char? MaskingCharacter {
            get => this.maskingCharacter;
            set {
                this.maskingCharacter = value;
                this.HandleTextChange(false);
            }
        }
        /// <summary>
        /// The maximum amount of characters that can be input into this text field.
        /// If this is set, the length of <see cref="Text"/> will never exceed this value.
        /// </summary>
        public int? MaximumCharacters;
        /// <summary>
        /// Whether this text field should support multi-line editing.
        /// If this is true, pressing <see cref="Keys.Enter"/> will insert a new line into the <see cref="Text"/> if the <see cref="InputRule"/> allows it.
        /// Additionally, text will be rendered with horizontal soft wraps, and lines that are outside of the text field's bounds will be hidden.
        /// </summary>
        public bool Multiline {
            get => this.multiline;
            set {
                this.multiline = value;
                this.HandleTextChange(false);
            }
        }

        private readonly StringBuilder text = new StringBuilder();

        private char? maskingCharacter;
        private double caretBlinkTimer;
        private string displayedText;
        private string[] splitText;
        private int textOffset;
        private int lineOffset;
        private int caretPos;
        private float caretDrawOffset;
        private bool multiline;

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
            this.InputRule = rule ?? TextField.DefaultRule;
            this.Multiline = multiline;
            if (font != null)
                this.Font = font;
            if (text != null)
                this.SetText(text, true);

            MlemPlatform.EnsureExists();

            this.OnPressed += OnPressed;
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
                } else if (this.Multiline && key == Keys.Enter) {
                    this.InsertText('\n');
                } else {
                    this.InsertText(character);
                }
            };
            this.OnDeselected += e => this.CaretPos = 0;
            this.OnSelected += e => this.CaretPos = this.text.Length;

            async void OnPressed(Element e) {
                var title = this.MobileTitle ?? this.PlaceholderText;
                var result = await MlemPlatform.Current.OpenOnScreenKeyboard(title, this.MobileDescription, this.Text, false);
                if (result != null)
                    this.SetText(this.Multiline ? result : result.Replace('\n', ' '), true);
            }
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);

            // handle first initialization if not done
            if (this.displayedText == null)
                this.HandleTextChange(false);

            if (!this.IsSelected || this.IsHidden)
                return;

            if (this.CaretPos > 0 && this.Input.TryConsumePressed(Keys.Left)) {
                this.CaretPos--;
            } else if (this.CaretPos < this.text.Length && this.Input.TryConsumePressed(Keys.Right)) {
                this.CaretPos++;
            } else if (this.Multiline && this.Input.IsKeyPressedAvailable(Keys.Up) && this.MoveCaretToLine(this.CaretLine - 1)) {
                this.Input.TryConsumeKeyPressed(Keys.Up);
            } else if (this.Multiline && this.Input.IsKeyPressedAvailable(Keys.Down) && this.MoveCaretToLine(this.CaretLine + 1)) {
                this.Input.TryConsumeKeyPressed(Keys.Down);
            } else if (this.CaretPos != 0 && this.Input.TryConsumeKeyPressed(Keys.Home)) {
                this.CaretPos = 0;
            } else if (this.CaretPos != this.text.Length && this.Input.TryConsumeKeyPressed(Keys.End)) {
                this.CaretPos = this.text.Length;
            } else if (this.Input.IsModifierKeyDown(ModifierKey.Control)) {
                if (this.Input.IsKeyPressedAvailable(Keys.V)) {
                    var clip = ClipboardService.GetText();
                    if (clip != null) {
                        this.InsertText(clip, true);
                        this.Input.TryConsumeKeyPressed(Keys.V);
                    }
                } else if (this.Input.TryConsumeKeyPressed(Keys.C)) {
                    // until there is text selection, just copy the whole content
                    ClipboardService.SetText(this.Text);
                }
            }

            this.caretBlinkTimer += time.ElapsedGameTime.TotalSeconds;
            if (this.caretBlinkTimer >= 1)
                this.caretBlinkTimer = 0;
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

            if (this.displayedText != null) {
                var lineHeight = this.Font.Value.LineHeight * this.TextScale * this.Scale;
                var offset = new Vector2(
                    this.TextOffsetX * this.Scale,
                    this.Multiline ? this.TextOffsetX * this.Scale : this.DisplayArea.Height / 2 - lineHeight / 2);
                var textPos = this.DisplayArea.Location + offset;
                if (this.text.Length > 0 || this.IsSelected) {
                    var textColor = this.TextColor.OrDefault(Color.White);
                    this.Font.Value.DrawString(batch, this.displayedText, textPos, textColor * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);

                    if (this.IsSelected && this.caretBlinkTimer < 0.5F) {
                        var caretDrawPos = textPos + new Vector2(this.caretDrawOffset * this.TextScale * this.Scale, 0);
                        if (this.Multiline)
                            caretDrawPos.Y += this.Font.Value.LineHeight * (this.CaretLine - this.lineOffset) * this.TextScale * this.Scale;
                        batch.Draw(batch.GetBlankTexture(), new RectangleF(caretDrawPos, new Vector2(this.CaretWidth * this.Scale, lineHeight)), null, textColor * alpha);
                    }
                } else if (this.PlaceholderText != null) {
                    this.Font.Value.DrawString(batch, this.PlaceholderText, textPos, this.PlaceholderColor.OrDefault(Color.Gray) * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);
                }
            }
            base.Draw(time, batch, alpha, context);
        }

        /// <summary>
        /// Replaces this text field's text with the given text.
        /// If the resulting <see cref="Text"/> exceeds <see cref="MaximumCharacters"/>, the end will be cropped to fit.
        /// </summary>
        /// <param name="text">The new text</param>
        /// <param name="removeMismatching">If any characters that don't match the <see cref="InputRule"/> should be left out</param>
        public void SetText(object text, bool removeMismatching = false) {
            var strg = text?.ToString() ?? string.Empty;
            if (!this.FilterText(ref strg, removeMismatching))
                return;
            if (this.MaximumCharacters != null && strg.Length > this.MaximumCharacters)
                strg = strg.Substring(0, this.MaximumCharacters.Value);
            this.text.Clear();
            this.text.Append(strg);
            this.CaretPos = this.text.Length;
            this.HandleTextChange();
        }

        /// <summary>
        /// Inserts the given text at the <see cref="CaretPos"/>.
        /// If the resulting <see cref="Text"/> exceeds <see cref="MaximumCharacters"/>, the end will be cropped to fit.
        /// </summary>
        /// <param name="text">The text to insert</param>
        /// <param name="removeMismatching">If any characters that don't match the <see cref="InputRule"/> should be left out</param>
        public void InsertText(object text, bool removeMismatching = false) {
            var strg = text?.ToString() ?? string.Empty;
            if (!this.FilterText(ref strg, removeMismatching))
                return;
            if (this.MaximumCharacters != null && this.text.Length + strg.Length > this.MaximumCharacters)
                strg = strg.Substring(0, this.MaximumCharacters.Value - this.text.Length);
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
            // ensure that caret pos is still in bounds
            this.CaretPos = this.CaretPos;
            this.HandleTextChange();
        }

        /// <summary>
        /// Moves the <see cref="CaretPos"/> to the given line, if it exists.
        /// Additionally maintains the <see cref="CaretPosInLine"/> roughly based on the visual distance that the caret has from the left border of the current <see cref="CaretLine"/>.
        /// </summary>
        /// <param name="line">The line to move the caret to</param>
        /// <returns>True if the caret was moved, false if it was not (which indicates that the line with the given <paramref name="line"/> index does not exist)</returns>
        public bool MoveCaretToLine(int line) {
            var (destStart, destEnd) = this.GetLineBounds(line);
            if (destEnd > 0) {
                // find the position whose distance from the start is closest to the current distance from the start
                var destAccum = "";
                while (destAccum.Length < destEnd - destStart) {
                    if (this.Font.Value.MeasureString(destAccum).X >= this.caretDrawOffset) {
                        this.CaretPos = destStart + destAccum.Length;
                        return true;
                    }
                    destAccum += this.text[destStart + destAccum.Length];
                }
                // if we don't find a proper position, just move to the end of the destination line
                this.CaretPos = destEnd;
                return true;
            }
            return false;
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

        private bool FilterText(ref string text, bool removeMismatching) {
            if (removeMismatching) {
                var result = new StringBuilder();
                foreach (var c in text) {
                    if (this.InputRule(this, c.ToCachedString()))
                        result.Append(c);
                }
                text = result.ToString();
            } else if (!this.InputRule(this, text))
                return false;
            return true;
        }

        private void HandleTextChange(bool textChanged = true) {
            // not initialized yet
            if (!this.Font.HasValue())
                return;
            var maxWidth = this.DisplayArea.Width / this.Scale - this.TextOffsetX * 2;
            if (this.Multiline) {
                // soft wrap if we're multiline
                this.splitText = this.Font.Value.SplitStringSeparate(this.text, maxWidth, this.TextScale).ToArray();
                this.displayedText = string.Join("\n", this.splitText);
                this.UpdateCaretData();

                var maxHeight = this.DisplayArea.Height / this.Scale - this.TextOffsetX * 2;
                if (this.Font.Value.MeasureString(this.displayedText).Y * this.TextScale > maxHeight) {
                    var maxLines = (maxHeight / (this.Font.Value.LineHeight * this.TextScale)).Floor();
                    if (this.lineOffset > this.CaretLine) {
                        // if we're moving up
                        this.lineOffset = this.CaretLine;
                    } else if (this.CaretLine >= maxLines) {
                        // if we're moving down
                        var limit = this.CaretLine - (maxLines - 1);
                        if (limit > this.lineOffset)
                            this.lineOffset = limit;
                    }
                    // calculate resulting string
                    var ret = new StringBuilder();
                    var lines = 0;
                    var originalIndex = 0;
                    for (var i = 0; i < this.displayedText.Length; i++) {
                        if (lines >= this.lineOffset) {
                            if (ret.Length <= 0)
                                this.textOffset = originalIndex;
                            ret.Append(this.displayedText[i]);
                        }
                        if (this.displayedText[i] == '\n') {
                            lines++;
                            if (this.text[originalIndex] == '\n')
                                originalIndex++;
                        } else {
                            originalIndex++;
                        }
                        if (lines - this.lineOffset >= maxLines)
                            break;
                    }
                    this.displayedText = ret.ToString();
                } else {
                    this.lineOffset = 0;
                    this.textOffset = 0;
                }
            } else {
                // not multiline, so scroll horizontally based on caret position
                if (this.Font.Value.MeasureString(this.text).X * this.TextScale > maxWidth) {
                    if (this.textOffset > this.CaretPos) {
                        // if we're moving the caret to the left
                        this.textOffset = this.CaretPos;
                    } else {
                        // if we're moving the caret to the right
                        var importantArea = this.text.ToString(this.textOffset, Math.Min(this.CaretPos, this.text.Length) - this.textOffset);
                        var bound = this.CaretPos - this.Font.Value.TruncateString(importantArea, maxWidth, this.TextScale, true).Length;
                        if (this.textOffset < bound)
                            this.textOffset = bound;
                    }
                    var visible = this.text.ToString(this.textOffset, this.text.Length - this.textOffset);
                    this.displayedText = this.Font.Value.TruncateString(visible, maxWidth, this.TextScale);
                } else {
                    this.displayedText = this.Text;
                    this.textOffset = 0;
                }
                this.UpdateCaretData();
            }

            if (this.MaskingCharacter != null)
                this.displayedText = new string(this.MaskingCharacter.Value, this.displayedText.Length);

            if (textChanged)
                this.OnTextChange?.Invoke(this, this.Text);
        }

        private void UpdateCaretData() {
            if (this.splitText != null) {
                var line = 0;
                var index = 0;
                for (var d = 0; d < this.splitText.Length; d++) {
                    var startOfLine = 0;
                    var split = this.splitText[d];
                    for (var i = 0; i <= split.Length; i++) {
                        if (index == this.CaretPos) {
                            this.CaretLine = line;
                            this.CaretPosInLine = i - startOfLine;
                            this.caretDrawOffset = this.Font.Value.MeasureString(split.Substring(startOfLine, this.CaretPosInLine)).X;
                            return;
                        }
                        if (i < split.Length) {
                            // manual splits
                            if (split[i] == '\n') {
                                startOfLine = i + 1;
                                line++;
                            }
                            index++;
                        }
                    }
                    // max width splits
                    line++;
                }
            } else if (this.displayedText != null) {
                this.CaretLine = 0;
                this.CaretPosInLine = this.CaretPos;
                this.caretDrawOffset = this.Font.Value.MeasureString(this.displayedText.Substring(0, this.CaretPos - this.textOffset)).X;
            }
        }

        private (int, int) GetLineBounds(int boundLine) {
            if (this.splitText != null) {
                var line = 0;
                var index = 0;
                var startOfLineIndex = 0;
                for (var d = 0; d < this.splitText.Length; d++) {
                    var split = this.splitText[d];
                    for (var i = 0; i < split.Length; i++) {
                        index++;
                        if (split[i] == '\n') {
                            if (boundLine == line)
                                return (startOfLineIndex, index - 1);
                            line++;
                            startOfLineIndex = index;
                        }
                    }
                    if (boundLine == line)
                        return (startOfLineIndex, index - 1);
                    line++;
                    startOfLineIndex = index;
                }
            }
            return default;
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