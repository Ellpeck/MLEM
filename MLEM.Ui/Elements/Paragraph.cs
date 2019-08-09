using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private float lineHeight;
        private string[] splitText;
        private readonly IGenericFont font;
        private readonly bool centerText;

        public float TextScale;
        public string Text {
            get => this.text;
            set {
                this.text = value;
                this.SetDirty();
            }

        }
        public IGenericFont Font => this.font ?? this.System.DefaultFont;

        public Paragraph(Anchor anchor, float width, string text, float textScale = 1, bool centerText = false, IGenericFont font = null) : base(anchor, new Vector2(width, 0)) {
            this.text = text;
            this.font = font;
            this.TextScale = textScale;
            this.centerText = centerText;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            var size = base.CalcActualSize(parentArea);
            this.splitText = this.Font.SplitString(this.text, size.X, this.TextScale).ToArray();

            this.lineHeight = 0;
            var height = 0F;
            foreach (var strg in this.splitText) {
                var strgHeight = this.Font.MeasureString(strg).Y * this.TextScale;
                height += strgHeight + 1;
                if (strgHeight > this.lineHeight)
                    this.lineHeight = strgHeight;
            }
            return new Point(size.X, height.Ceil());
        }

        public override void Draw(GameTime time, SpriteBatch batch, Color color) {
            base.Draw(time, batch, color);

            var pos = this.DisplayArea.Location.ToVector2();
            var offset = new Vector2();
            foreach (var line in this.splitText) {
                if (this.centerText) {
                    this.Font.DrawCenteredString(batch, line, pos + offset + new Vector2(this.DisplayArea.Width / 2, 0), this.TextScale, color);
                } else {
                    this.Font.DrawString(batch, line, pos + offset, color, 0, Vector2.Zero, this.TextScale, SpriteEffects.None, 0);
                }
                offset.Y += this.lineHeight + 1;
            }
        }

    }

}