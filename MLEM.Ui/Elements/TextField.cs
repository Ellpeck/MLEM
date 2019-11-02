using System;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Input;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class TextField : Element {

        public static readonly Rule DefaultRule = (field, add) => !add.Any(char.IsControl);
        public static readonly Rule OnlyLetters = (field, add) => add.All(char.IsLetter);
        public static readonly Rule OnlyNumbers = (field, add) => add.All(char.IsNumber);
        public static readonly Rule LettersNumbers = (field, add) => add.All(c => char.IsLetter(c) || char.IsNumber(c));

        public StyleProp<NinePatch> Texture;
        public StyleProp<NinePatch> HoveredTexture;
        public StyleProp<Color> HoveredColor;
        public StyleProp<float> TextScale;
        public StyleProp<IGenericFont> Font;
        private readonly StringBuilder text = new StringBuilder();
        public string Text => this.text.ToString();
        public string PlaceholderText;
        public TextChanged OnTextChange;
        public float TextOffsetX = 4;
        private double caretBlinkTimer;
        private string displayedText;
        private int textOffset;
        public Rule InputRule;
        public string MobileTitle;
        public string MobileDescription;
        private int caretPos;
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

        public TextField(Anchor anchor, Vector2 size, Rule rule = null, IGenericFont font = null) : base(anchor, size) {
            this.InputRule = rule ?? DefaultRule;
            if (font != null)
                this.Font.Set(font);

            if (WindowExtensions.SupportsTextInput()) {
                this.OnTextInput += (element, key, character) => {
                    if (!this.IsSelected)
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
            } else {
                this.OnPressed += async e => {
                    if (!KeyboardInput.IsVisible) {
                        var title = this.MobileTitle ?? this.PlaceholderText;
                        var result = await KeyboardInput.Show(title, this.MobileDescription, this.Text);
                        if (result != null)
                            this.SetText(result.Replace('\n', ' '), true);
                    }
                };
            }
            this.OnDeselected += e => this.CaretPos = 0;
            this.OnSelected += e => this.CaretPos = this.text.Length;
        }

        private void HandleTextChange(bool textChanged = true) {
            // not initialized yet
            if (this.Font.Value == null)
                return;
            var length = this.Font.Value.MeasureString(this.text).X * this.TextScale;
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

        public override void Update(GameTime time) {
            base.Update(time);

            // handle first initialization if not done
            if (this.displayedText == null)
                this.HandleTextChange(false);

            if (this.Input.IsKeyPressed(Keys.Left)) {
                this.CaretPos--;
            } else if (this.Input.IsKeyPressed(Keys.Right)) {
                this.CaretPos++;
            } else if (this.Input.IsKeyPressed(Keys.Home)) {
                this.CaretPos = 0;
            } else if (this.Input.IsKeyPressed(Keys.End)) {
                this.CaretPos = this.text.Length;
            }

            this.caretBlinkTimer += time.ElapsedGameTime.TotalSeconds;
            if (this.caretBlinkTimer >= 1)
                this.caretBlinkTimer = 0;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                if (this.HoveredTexture.Value != null)
                    tex = this.HoveredTexture;
                color = (Color) this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea, color, this.Scale);

            if (this.displayedText != null) {
                var textPos = this.DisplayArea.Location + new Vector2(this.TextOffsetX * this.Scale, this.DisplayArea.Height / 2);
                if (this.text.Length > 0 || this.IsSelected) {
                    var caret = this.IsSelected ? this.caretBlinkTimer >= 0.5F ? "|" : " " : "";
                    var display = this.displayedText.Insert(this.CaretPos - this.textOffset, caret);
                    this.Font.Value.DrawCenteredString(batch, display, textPos, this.TextScale * this.Scale, Color.White * alpha, false, true);
                } else if (this.PlaceholderText != null) {
                    this.Font.Value.DrawCenteredString(batch, this.PlaceholderText, textPos, this.TextScale * this.Scale, Color.Gray * alpha, false, true);
                }
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        public void SetText(object text, bool removeMismatching = false) {
            if (removeMismatching) {
                var result = new StringBuilder();
                foreach (var c in text.ToString()) {
                    if (this.InputRule(this, c.ToString()))
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

        public void InsertText(object text) {
            var strg = text.ToString();
            if (!this.InputRule(this, strg))
                return;
            this.text.Insert(this.CaretPos, strg);
            this.CaretPos += strg.Length;
            this.HandleTextChange();
        }

        public void RemoveText(int index, int length) {
            if (index < 0 || index >= this.text.Length)
                return;
            this.text.Remove(index, length);
            this.HandleTextChange();
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale.SetFromStyle(style.TextScale);
            this.Font.SetFromStyle(style.Font);
            this.Texture.SetFromStyle(style.TextFieldTexture);
            this.HoveredTexture.SetFromStyle(style.TextFieldHoveredTexture);
            this.HoveredColor.SetFromStyle(style.TextFieldHoveredColor);
        }

        public delegate void TextChanged(TextField field, string text);

        public delegate bool Rule(TextField field, string textToAdd);

    }
}