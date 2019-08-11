using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class AutoScaledText : Element {

        private IGenericFont font;
        private float textScale;
        private string text;

        public string Text {
            get => this.text;
            set {
                this.text = value;
                this.SetDirty();
            }
        }
        public Color Color = Color.White;

        public AutoScaledText(Anchor anchor, Vector2 size, string text, IGenericFont font = null) : base(anchor, size) {
            this.Text = text;
            this.font = font;
            this.IgnoresMouse = true;
        }

        public override void ForceUpdateArea() {
            base.ForceUpdateArea();

            this.textScale = 0;
            Vector2 measure;
            do {
                this.textScale += 0.1F;
                measure = this.font.MeasureString(this.Text) * this.textScale;
            } while (measure.X <= this.DisplayArea.Size.X / this.Scale && measure.Y <= this.DisplayArea.Size.Y / this.Scale);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha) {
            var pos = this.DisplayArea.Location.ToVector2() + this.DisplayArea.Size.ToVector2() / 2;
            this.font.DrawCenteredString(batch, this.Text, pos, this.textScale * this.Scale, this.Color * alpha, true, true);
            base.Draw(time, batch, alpha);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.font = style.Font;
        }

    }
}