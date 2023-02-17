using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Input {
    /// <summary>
    /// A class that contains all of the necessary tools to create a text input field or box.
    /// This text input features single- and <see cref="Multiline"/> input, <see cref="InputRule"/> sets, free caret movement, copying and pasting, and more.
    /// To use this class, <see cref="OnTextInput"/>, <see cref="Update"/> and <see cref="Draw"/> have to be called regularly.
    /// While this class is used by MLEM.Ui's TextField, it is designed to be used for custom or external UI systems.
    /// </summary>
    public class TextInput {

        /// <summary>
        /// A <see cref="Rule"/> that allows any visible character and spaces
        /// </summary>
        public static readonly Rule DefaultRule = (input, add) => {
            foreach (var c in add) {
                if (char.IsControl(c) && (!input.Multiline || c != '\n'))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows letters
        /// </summary>
        public static readonly Rule OnlyLetters = (input, add) => {
            foreach (var c in add) {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows numerals
        /// </summary>
        public static readonly Rule OnlyNumbers = (input, add) => {
            foreach (var c in add) {
                if (!char.IsNumber(c))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows letters and numerals
        /// </summary>
        public static readonly Rule LettersNumbers = (input, add) => {
            foreach (var c in add) {
                if (!char.IsLetter(c) || !char.IsNumber(c))
                    return false;
            }
            return true;
        };
        /// <summary>
        /// A <see cref="Rule"/> that only allows characters not contained in <see cref="Path.GetInvalidPathChars"/>
        /// </summary>
        public static readonly Rule PathNames = (input, add) => add.IndexOfAny(Path.GetInvalidPathChars()) < 0;
        /// <summary>
        /// A <see cref="Rule"/> that only allows characters not contained in <see cref="Path.GetInvalidFileNameChars"/>
        /// </summary>
        public static readonly Rule FileNames = (input, add) => add.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;

        /// <summary>
        /// This text input's current text
        /// </summary>
        public string Text => this.text.ToString();
        /// <summary>
        /// The length that this <see cref="TextInput"/>'s text has.
        /// </summary>
        public int Length => this.text.Length;
        /// <summary>
        /// An event that gets called when <see cref="Text"/> changes, either through input, or through a manual change.
        /// </summary>
        public TextChanged OnTextChange;
        /// <summary>
        /// The rule used for text input.
        /// Rules allow only certain characters to be allowed inside of a text input.
        /// </summary>
        public Rule InputRule;
        /// <summary>
        /// The position of the caret within the text.
        /// This is always between 0 and the <see cref="string.Length"/> of <see cref="Text"/>
        /// </summary>
        public int CaretPos {
            get => this.caretPos;
            set {
                var val = (int) MathHelper.Clamp(value, 0F, this.text.Length);
                if (this.caretPos != val) {
                    this.caretPos = val;
                    this.caretBlinkTimer = 0;
                    this.SetTextDataDirty(false);
                }
            }
        }
        /// <summary>
        /// The line of text that the caret is currently on.
        /// This can only be only non-0 if <see cref="Multiline"/> is true.
        /// </summary>
        public int CaretLine {
            get {
                this.UpdateTextDataIfDirty();
                return this.caretLine;
            }
        }
        /// <summary>
        /// The position in the current <see cref="CaretLine"/> that the caret is currently on.
        /// If <see cref="Multiline"/> is false, this value is always equal to <see cref="CaretPos"/>.
        /// </summary>
        public int CaretPosInLine {
            get {
                this.UpdateTextDataIfDirty();
                return this.caretPosInLine;
            }
        }
        /// <summary>
        /// A character that should be displayed instead of this text input's <see cref="Text"/> content.
        /// The amount of masking characters displayed will be equal to the <see cref="Text"/>'s length.
        /// This behavior is useful for password inputs or similar.
        /// </summary>
        public char? MaskingCharacter {
            get => this.maskingCharacter;
            set {
                if (this.maskingCharacter != value) {
                    this.maskingCharacter = value;
                    this.SetTextDataDirty(false);
                }
            }
        }
        /// <summary>
        /// The maximum amount of characters that can be input into this text input.
        /// If this is set, the length of <see cref="Text"/> will never exceed this value.
        /// </summary>
        public int? MaximumCharacters;
        /// <summary>
        /// Whether this text input should support multi-line editing.
        /// If this is true, pressing <see cref="Keys.Enter"/> will insert a new line into the <see cref="Text"/> if the <see cref="InputRule"/> allows it.
        /// Additionally, text will be rendered with horizontal soft wraps, and lines that are outside of the text input's bounds will be hidden.
        /// </summary>
        public bool Multiline {
            get => this.multiline;
            set {
                if (this.multiline != value) {
                    this.multiline = value;
                    this.SetTextDataDirty(false);
                }
            }
        }
        /// <summary>
        /// The font that this text input is currently using.
        /// </summary>
        public GenericFont Font {
            get => this.font;
            set {
                if (this.font != value) {
                    this.font = value;
                    this.SetTextDataDirty(false);
                }
            }
        }
        /// <summary>
        /// The scale that this text input's text has.
        /// In <see cref="Draw"/>, this is multiplied with the drawScale parameter.
        /// </summary>
        public float TextScale {
            get => this.textScale;
            set {
                if (this.textScale != value) {
                    this.textScale = value;
                    this.SetTextDataDirty(false);
                }
            }
        }
        /// <summary>
        /// The size of this text input, which is the size that this text input's <see cref="Text"/> will be bounded to in <see cref="Draw"/>.
        /// Note that <see cref="TextScale"/> gets applied to the text for calculations involving this size.
        /// </summary>
        public Vector2 Size {
            get => this.size;
            set {
                if (this.size != value) {
                    this.size = value;
                    this.SetTextDataDirty(false);
                }
            }
        }
        /// <summary>
        /// A function that is invoked when a string of text should be copied to the clipboard.
        /// MLEM.Ui uses the TextCopy package for this, but other options are available.
        /// </summary>
        public Action<string> CopyToClipboardFunction;
        /// <summary>
        /// A function that is invoked when a string of text should be pasted from the clipboard.
        /// MLEM.Ui uses the TextCopy package for this, but other options are available.
        /// </summary>
        public Func<string> PasteFromClipboardFunction;

        private readonly StringBuilder text = new StringBuilder();

        private char? maskingCharacter;
        private double caretBlinkTimer;
        private string displayedText;
        private string[] splitText;
        private int textOffset;
        private int lineOffset;
        private int caretPos;
        private int caretLine;
        private int caretPosInLine;
        private float caretDrawOffset;
        private bool multiline;
        private GenericFont font;
        private float textScale;
        private Vector2 size;
        private bool textDataDirty;

        /// <summary>
        /// Creates a new text input with the given settings.
        /// </summary>
        /// <param name="font">The <see cref="Font"/> to use.</param>
        /// <param name="size">The <see cref="Size"/> to set.</param>
        /// <param name="textScale">The <see cref="TextScale"/> to set.</param>
        /// <param name="inputRule">The <see cref="InputRule"/> to set.</param>
        /// <param name="copyToClipboardFunction">The <see cref="CopyToClipboardFunction"/>to set.</param>
        /// <param name="pasteFromClipboardFunction">The <see cref="PasteFromClipboardFunction"/> to set.</param>
        public TextInput(GenericFont font, Vector2 size, float textScale, Rule inputRule = null, Action<string> copyToClipboardFunction = null, Func<string> pasteFromClipboardFunction = null) {
            this.InputRule = inputRule ?? TextInput.DefaultRule;
            this.CopyToClipboardFunction = copyToClipboardFunction;
            this.PasteFromClipboardFunction = pasteFromClipboardFunction;
            this.Font = font;
            this.Size = size;
            this.TextScale = textScale;
        }

        /// <summary>
        /// A method that should be called when the given text should be entered into this text input.
        /// This method is designed to be used with <see cref="MlemPlatform.AddTextInputListener"/> or the TextInput event provided by MonoGame and FNA.
        /// </summary>
        /// <param name="key">The key that was pressed.</param>
        /// <param name="character">The character that the <paramref name="key"/> represents.</param>
        /// <returns>Whether text was successfully input.</returns>
        public bool OnTextInput(Keys key, char character) {
            // FNA's text input event doesn't supply keys, so we handle this in Update
#if !FNA
            if (key == Keys.Back) {
                if (this.CaretPos > 0) {
                    this.CaretPos--;
                    this.RemoveText(this.CaretPos, 1);
                    return true;
                }
            } else if (key == Keys.Delete) {
                return this.RemoveText(this.CaretPos, 1);
            } else if (this.Multiline && key == Keys.Enter) {
                return this.InsertText('\n');
            } else {
                return this.InsertText(character);
            }
            return false;
#else
            return this.InsertText(character);
#endif
        }

        /// <summary>
        /// Updates this text input, including querying input using the given <see cref="InputHandler"/> and updating the caret's blink timer.
        /// </summary>
        /// <param name="time">The current game time.</param>
        /// <param name="input">The input handler to use for input querying.</param>
        public void Update(GameTime time, InputHandler input) {
            this.UpdateTextDataIfDirty();

#if FNA
            // FNA's text input event doesn't supply keys, so we handle this here
            if (this.CaretPos > 0 && input.TryConsumePressed(Keys.Back)) {
                this.CaretPos--;
                this.RemoveText(this.CaretPos, 1);
            } else if (this.CaretPos < this.text.Length && input.TryConsumePressed(Keys.Delete)) {
                this.RemoveText(this.CaretPos, 1);
            } else if (this.Multiline && input.TryConsumePressed(Keys.Enter)) {
                this.InsertText('\n');
            } else
#endif
            if (this.CaretPos > 0 && input.TryConsumePressed(Keys.Left)) {
                this.CaretPos--;
            } else if (this.CaretPos < this.text.Length && input.TryConsumePressed(Keys.Right)) {
                this.CaretPos++;
            } else if (this.Multiline && input.IsPressedAvailable(Keys.Up) && this.MoveCaretToLine(this.CaretLine - 1)) {
                input.TryConsumePressed(Keys.Up);
            } else if (this.Multiline && input.IsPressedAvailable(Keys.Down) && this.MoveCaretToLine(this.CaretLine + 1)) {
                input.TryConsumePressed(Keys.Down);
            } else if (this.CaretPos != 0 && input.TryConsumePressed(Keys.Home)) {
                this.CaretPos = 0;
            } else if (this.CaretPos != this.text.Length && input.TryConsumePressed(Keys.End)) {
                this.CaretPos = this.text.Length;
            } else if (input.IsModifierKeyDown(ModifierKey.Control)) {
                if (input.IsPressedAvailable(Keys.V)) {
                    var clip = this.PasteFromClipboardFunction?.Invoke();
                    if (clip != null) {
                        this.InsertText(clip, true);
                        input.TryConsumePressed(Keys.V);
                    }
                } else if (input.TryConsumePressed(Keys.C)) {
                    // until there is text selection, just copy the whole content
                    this.CopyToClipboardFunction?.Invoke(this.Text);
                }
            }

            this.caretBlinkTimer += time.ElapsedGameTime.TotalSeconds;
            if (this.caretBlinkTimer >= 1)
                this.caretBlinkTimer = 0;
        }

        /// <summary>
        /// Draws this text input's displayed <see cref="Text"/> along with its caret, if <paramref name="caretWidth"/> is greater than 0.
        /// </summary>
        /// <param name="batch">The sprite batch to draw with.</param>
        /// <param name="textPos">The position to draw the text at.</param>
        /// <param name="drawScale">The draw scale, which is multiplied with <see cref="TextScale"/> before drawing.</param>
        /// <param name="caretWidth">The width that the caret should have, which is multiplied with <paramref name="drawScale"/> before drawing.</param>
        /// <param name="textColor">The color to draw the text and caret with.</param>
        public void Draw(SpriteBatch batch, Vector2 textPos, float drawScale, float caretWidth, Color textColor) {
            this.UpdateTextDataIfDirty();

            var scale = this.TextScale * drawScale;
            this.Font.DrawString(batch, this.displayedText, textPos, textColor, 0, Vector2.Zero, scale, SpriteEffects.None, 0);

            if (caretWidth > 0 && this.caretBlinkTimer < 0.5F) {
                var caretDrawPos = textPos + new Vector2(this.caretDrawOffset * scale, 0);
                if (this.Multiline)
                    caretDrawPos.Y += this.Font.LineHeight * (this.CaretLine - this.lineOffset) * scale;
                batch.Draw(batch.GetBlankTexture(), new RectangleF(caretDrawPos, new Vector2(caretWidth * drawScale, this.Font.LineHeight * scale)), null, textColor);
            }
        }

        /// <summary>
        /// Replaces this text input's text with the given text.
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
            this.SetTextDataDirty();
        }

        /// <summary>
        /// Inserts the given text at the <see cref="CaretPos"/>.
        /// If the resulting <see cref="Text"/> exceeds <see cref="MaximumCharacters"/>, the end will be cropped to fit.
        /// </summary>
        /// <param name="text">The text to insert</param>
        /// <param name="removeMismatching">If any characters that don't match the <see cref="InputRule"/> should be left out</param>
        public bool InsertText(object text, bool removeMismatching = false) {
            var strg = text?.ToString() ?? string.Empty;
            if (!this.FilterText(ref strg, removeMismatching))
                return false;
            if (this.MaximumCharacters != null && this.text.Length + strg.Length > this.MaximumCharacters)
                strg = strg.Substring(0, this.MaximumCharacters.Value - this.text.Length);
            this.text.Insert(this.CaretPos, strg);
            this.CaretPos += strg.Length;
            this.SetTextDataDirty();
            return true;
        }

        /// <summary>
        /// Removes the given amount of text at the given index
        /// </summary>
        /// <param name="index">The index</param>
        /// <param name="length">The amount of text to remove</param>
        public bool RemoveText(int index, int length) {
            if (index < 0 || index >= this.text.Length)
                return false;
            this.text.Remove(index, length);
            // ensure that caret pos is still in bounds
            this.CaretPos = this.CaretPos;
            this.SetTextDataDirty();
            return true;
        }

        /// <summary>
        /// Moves the <see cref="CaretPos"/> to the given line, if it exists.
        /// Additionally maintains the <see cref="CaretPosInLine"/> roughly based on the visual distance that the caret has from the left border of the current <see cref="CaretLine"/>.
        /// </summary>
        /// <param name="line">The line to move the caret to</param>
        /// <returns>True if the caret was moved, false if it was not (which indicates that the line with the given <paramref name="line"/> index does not exist)</returns>
        public bool MoveCaretToLine(int line) {
            this.UpdateTextDataIfDirty();
            var (destStart, destEnd) = this.GetLineBounds(line);
            if (destEnd > 0) {
                // find the position whose distance from the start is closest to the current distance from the start
                var destAccum = "";
                while (destAccum.Length < destEnd - destStart) {
                    if (this.Font.MeasureString(destAccum).X >= this.caretDrawOffset) {
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

        private bool FilterText(ref string text, bool removeMismatching) {
            var result = new StringBuilder();
            foreach (var codePoint in new CodePointSource(text)) {
                var character = char.ConvertFromUtf32(codePoint);
                // don't include control characters
                if (character.Length == 1 && char.IsControl(character, 0))
                    continue;
                if (this.InputRule(this, character)) {
                    result.Append(character);
                } else if (!removeMismatching) {
                    // if we don't remove mismatching characters, we just fail
                    return false;
                }
            }
            text = result.ToString();
            return true;
        }

        private void SetTextDataDirty(bool textChanged = true) {
            this.textDataDirty = true;
            if (textChanged)
                this.OnTextChange?.Invoke(this, this.Text);
        }

        private void UpdateTextDataIfDirty() {
            if (!this.textDataDirty || this.Font == null)
                return;
            this.textDataDirty = false;

            var visualText = this.text;
            if (this.MaskingCharacter != null)
                visualText = new StringBuilder(visualText.Length).Append(this.MaskingCharacter.Value, visualText.Length);

            if (this.Multiline) {
                // soft wrap if we're multiline
                this.splitText = this.Font.SplitStringSeparate(visualText, this.Size.X, this.TextScale).ToArray();
                this.displayedText = string.Join("\n", this.splitText);
                this.UpdateCaretData();

                if (this.Font.MeasureString(this.displayedText).Y * this.TextScale > this.Size.Y) {
                    var maxLines = (this.Size.Y / (this.Font.LineHeight * this.TextScale)).Floor();
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
                            if (visualText[originalIndex] == '\n')
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
                this.splitText = null;
                this.lineOffset = 0;
                // not multiline, so scroll horizontally based on caret position
                if (this.Font.MeasureString(visualText).X * this.TextScale > this.Size.X) {
                    if (this.textOffset > this.CaretPos) {
                        // if we're moving the caret to the left
                        this.textOffset = this.CaretPos;
                    } else {
                        // if we're moving the caret to the right
                        var importantArea = visualText.ToString(this.textOffset, Math.Min(this.CaretPos, visualText.Length) - this.textOffset);
                        var bound = this.CaretPos - this.Font.TruncateString(importantArea, this.Size.X, this.TextScale, true).Length;
                        if (this.textOffset < bound)
                            this.textOffset = bound;
                    }
                    var visible = visualText.ToString(this.textOffset, visualText.Length - this.textOffset);
                    this.displayedText = this.Font.TruncateString(visible, this.Size.X, this.TextScale);
                } else {
                    this.displayedText = visualText.ToString();
                    this.textOffset = 0;
                }
                this.UpdateCaretData();
            }
        }

        private void UpdateCaretData() {
            if (this.splitText != null) {
                // the code below will never execute if our text is empty, so reset our caret position fully
                if (this.splitText.Length <= 0) {
                    this.caretLine = 0;
                    this.caretPosInLine = 0;
                    this.caretDrawOffset = 0;
                    return;
                }

                var line = 0;
                var index = 0;
                for (var d = 0; d < this.splitText.Length; d++) {
                    var startOfLine = 0;
                    var split = this.splitText[d];
                    for (var i = 0; i <= split.Length; i++) {
                        if (index == this.CaretPos) {
                            this.caretLine = line;
                            this.caretPosInLine = i - startOfLine;
                            this.caretDrawOffset = this.Font.MeasureString(split.Substring(startOfLine, this.CaretPosInLine)).X;
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
                this.caretLine = 0;
                this.caretPosInLine = this.CaretPos;
                this.caretDrawOffset = this.Font.MeasureString(this.displayedText.Substring(0, this.CaretPos - this.textOffset)).X;
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
        /// A delegate method used for <see cref="TextInput.OnTextChange"/>
        /// </summary>
        /// <param name="input">The text input whose text changed</param>
        /// <param name="text">The new text</param>
        public delegate void TextChanged(TextInput input, string text);

        /// <summary>
        /// A delegate method used for <see cref="InputRule"/>.
        /// It should return whether the given text can be added to the text input.
        /// </summary>
        /// <param name="input">The text input</param>
        /// <param name="textToAdd">The text that is tried to be added</param>
        public delegate bool Rule(TextInput input, string textToAdd);

    }
}
