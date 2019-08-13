using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private float lineHeight;
        private string[] splitText;
        private IGenericFont font;
        private readonly bool centerText;

        public float TextScale;
        public string Text {
            get => this.text;
            set {
                this.text = value;
                this.SetAreaDirty();
            }
        }

        public Paragraph(Anchor anchor, float width, string text, bool centerText = false, IGenericFont font = null) : base(anchor, new Vector2(width, 0)) {
            this.text = text;
            this.font = font;
            this.centerText = centerText;
            this.IgnoresMouse = true;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            var size = base.CalcActualSize(parentArea);
            this.splitText = this.font.SplitString(this.text, size.X, this.TextScale * this.Scale).ToArray();

            this.lineHeight = 0;
            var height = 0F;
            foreach (var strg in this.splitText) {
                var strgHeight = this.font.MeasureString(strg).Y * this.TextScale * this.Scale;
                height += strgHeight + 1;
                if (strgHeight > this.lineHeight)
                    this.lineHeight = strgHeight;
            }
            return new Point(size.X, height.Ceil());
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            var pos = this.DisplayArea.Location.ToVector2();
            var off = offset.ToVector2();
            foreach (var line in this.splitText) {
                if (this.centerText) {
                    this.font.DrawCenteredString(batch, line, pos + off + new Vector2(this.DisplayArea.Width / 2, 0), this.TextScale * this.Scale, Color.White * alpha);
                } else {
                    this.font.DrawString(batch, line, pos + off, Color.White * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);
                }
                off.Y += this.lineHeight + 1;
            }
            base.Draw(time, batch, alpha, offset);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale = style.TextScale;
            this.font = style.Font;
        }

    }

}