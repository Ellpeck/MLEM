using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Font;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class TextField : Element {

        public static NinePatch DefaultTexture;
        public static NinePatch DefaultHoveredTexture;
        public static Color DefaultHoveredColor = Color.LightGray;

        public NinePatch Texture = DefaultTexture;
        public NinePatch HoveredTexture = DefaultHoveredTexture;
        public Color HoveredColor = DefaultHoveredColor;
        public float TextScale;
        public readonly StringBuilder Text = new StringBuilder();
        public TextChanged OnTextChange;
        public int MaxTextLength = int.MaxValue;
        public float TextOffsetX = 4;
        private readonly IGenericFont font;
        private double caretBlinkTimer;

        public TextField(Anchor anchor, Vector2 size, IGenericFont font = null) : base(anchor, size) {
            this.font = font ?? Paragraph.DefaultFont;
            this.TextScale = Paragraph.DefaultTextScale;
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
                if (textChanged)
                    this.OnTextChange?.Invoke(this, this.Text.ToString());
            };
        }

        public override void Update(GameTime time) {
            base.Update(time);

            this.caretBlinkTimer += time.ElapsedGameTime.TotalSeconds;
            if (this.caretBlinkTimer >= 1)
                this.caretBlinkTimer = 0;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                if (this.HoveredTexture != null)
                    tex = this.HoveredTexture;
                color = this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea, color);
            var caret = this.IsSelected && this.caretBlinkTimer >= 0.5F ? "|" : "";
            this.font.DrawCenteredString(batch, this.Text + caret, this.DisplayArea.Location.ToVector2() + new Vector2(this.TextOffsetX, this.DisplayArea.Height / 2), this.TextScale, Color.White * alpha, false, true);
            base.Draw(time, batch, alpha);
        }

        public delegate void TextChanged(TextField field, string text);

    }
}