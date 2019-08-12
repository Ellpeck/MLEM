using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class TextField : Element {

        public NinePatch Texture;
        public NinePatch HoveredTexture;
        public Color HoveredColor;
        public float TextScale;
        public readonly StringBuilder Text = new StringBuilder();
        public TextChanged OnTextChange;
        public int MaxTextLength = int.MaxValue;
        public float TextOffsetX = 4;
        private IGenericFont font;
        private double caretBlinkTimer;
        private int textStartIndex;

        public TextField(Anchor anchor, Vector2 size, IGenericFont font = null) : base(anchor, size) {
            this.font = font;
            this.OnTextInput += (element, key, character) => {
                if (!this.IsSelected)
                    return;
                var textChanged = false;
                if (key == Keys.Back) {
                    if (this.Text.Length > 0) {
                        this.Text.Remove(this.Text.Length - 1, 1);
                        textChanged = true;
                    }
                } else if (!char.IsControl(character)) {
                    if (this.Text.Length < this.MaxTextLength) {
                        this.Text.Append(character);
                        textChanged = true;
                    }
                }
                if (textChanged) {
                    var length = this.font.MeasureString(this.Text).X * this.TextScale;
                    var maxWidth = this.DisplayArea.Width / this.Scale - this.TextOffsetX * 2;
                    if (length > maxWidth) {
                        for (var i = 0; i < this.Text.Length; i++) {
                            var substring = this.Text.ToString(i, this.Text.Length - i);
                            if (this.font.MeasureString(substring).X * this.TextScale <= maxWidth) {
                                this.textStartIndex = i;
                                break;
                            }
                        }
                    } else {
                        this.textStartIndex = 0;
                    }

                    this.OnTextChange?.Invoke(this, this.Text.ToString());
                }
            };
        }

        public override void Update(GameTime time) {
            base.Update(time);

            this.caretBlinkTimer += time.ElapsedGameTime.TotalSeconds;
            if (this.caretBlinkTimer >= 1)
                this.caretBlinkTimer = 0;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                if (this.HoveredTexture != null)
                    tex = this.HoveredTexture;
                color = this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea.OffsetCopy(offset), color, this.Scale);
            var caret = this.IsSelected && this.caretBlinkTimer >= 0.5F ? "|" : "";
            var text = this.Text.ToString(this.textStartIndex, this.Text.Length - this.textStartIndex) + caret;
            this.font.DrawCenteredString(batch, text, this.DisplayArea.Location.ToVector2() + new Vector2(offset.X + this.TextOffsetX * this.Scale, offset.Y + this.DisplayArea.Height / 2), this.TextScale * this.Scale, Color.White * alpha, false, true);
            base.Draw(time, batch, alpha, offset);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale = style.TextScale;
            this.font = style.Font;
            this.Texture = style.TextFieldTexture;
            this.HoveredTexture = style.TextFieldHoveredTexture;
            this.HoveredColor = style.TextFieldHoveredColor;
        }

        public delegate void TextChanged(TextField field, string text);

    }
}