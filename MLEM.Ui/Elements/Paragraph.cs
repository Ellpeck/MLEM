using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private float lineHeight;
        private float longestLineLength;
        private string[] splitText;
        private IGenericFont font;
        private readonly bool centerText;

        public NinePatch Background;
        public Color BackgroundColor;
        public Color TextColor = Color.White;
        public float TextScale;
        public string Text {
            get => this.text;
            set {
                if (this.text != value) {
                    this.text = value;
                    this.SetAreaDirty();
                }
            }
        }
        public bool AutoAdjustWidth;
        public TextCallback GetTextCallback;
        public float LineSpace = 1;

        public Paragraph(Anchor anchor, float width, TextCallback textCallback, bool centerText = false, IGenericFont font = null)
            : this(anchor, width, "", centerText, font) {
            this.GetTextCallback = textCallback;
            this.Text = textCallback(this);
        }

        public Paragraph(Anchor anchor, float width, string text, bool centerText = false, IGenericFont font = null) : base(anchor, new Vector2(width, 0)) {
            this.text = text;
            this.font = font;
            this.centerText = centerText;
            this.IgnoresMouse = true;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            var size = base.CalcActualSize(parentArea);
            this.splitText = this.font.SplitString(this.text, size.X - this.ScaledPadding.X * 2, this.TextScale * this.Scale).ToArray();

            this.lineHeight = 0;
            this.longestLineLength = 0;
            foreach (var strg in this.splitText) {
                var strgScale = this.font.MeasureString(strg) * this.TextScale * this.Scale;
                if (strgScale.Y + 1 > this.lineHeight)
                    this.lineHeight = strgScale.Y + 1;
                if (strgScale.X > this.longestLineLength)
                    this.longestLineLength = strgScale.X;
            }
            this.lineHeight *= this.LineSpace;
            return new Point(this.AutoAdjustWidth ? this.longestLineLength.Ceil() + this.ScaledPadding.X * 2 : size.X, (this.lineHeight * this.splitText.Length).Ceil() + this.ScaledPadding.Y * 2);
        }

        public override void Update(GameTime time) {
            base.Update(time);
            if (this.GetTextCallback != null)
                this.Text = this.GetTextCallback(this);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            if (this.Background != null)
                batch.Draw(this.Background, this.Area.OffsetCopy(offset), this.BackgroundColor * alpha);

            var pos = this.DisplayArea.Location.ToVector2();
            var off = offset.ToVector2();
            foreach (var line in this.splitText) {
                if (this.centerText) {
                    this.font.DrawCenteredString(batch, line, pos + off + new Vector2(this.DisplayArea.Width / 2, 0), this.TextScale * this.Scale, this.TextColor * alpha);
                } else {
                    this.font.DrawString(batch, line, pos + off, this.TextColor * alpha, 0, Vector2.Zero, this.TextScale * this.Scale, SpriteEffects.None, 0);
                }
                off.Y += this.lineHeight;
            }
            base.Draw(time, batch, alpha, offset);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale = style.TextScale;
            this.font = style.Font;
        }

        public delegate string TextCallback(Paragraph paragraph);

    }

}