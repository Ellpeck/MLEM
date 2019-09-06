using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private readonly FormattedString formattedText = new FormattedString();
        private IGenericFont regularFont;
        private IGenericFont boldFont;
        private IGenericFont italicFont;

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
        public TimeSpan TimeIntoAnimation;

        public Paragraph(Anchor anchor, float width, TextCallback textCallback, bool centerText = false)
            : this(anchor, width, "", centerText) {
            this.GetTextCallback = textCallback;
            this.Text = textCallback(this);
        }

        public Paragraph(Anchor anchor, float width, string text, bool centerText = false) : base(anchor, new Vector2(width, 0)) {
            this.text = text;
            this.AutoAdjustWidth = centerText;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            var size = base.CalcActualSize(parentArea);

            var sc = this.TextScale * this.Scale;
            this.formattedText.Value = this.regularFont.SplitString(this.text.RemoveFormatting(), size.X - this.ScaledPadding.X * 2, sc);

            var textDims = this.regularFont.MeasureString(this.formattedText) * sc;
            return new Point(this.AutoAdjustWidth ? textDims.X.Ceil() + this.ScaledPadding.X * 2 : size.X, textDims.Y.Ceil() + this.ScaledPadding.Y * 2);
        }

        public override void Update(GameTime time) {
            base.Update(time);
            if (this.GetTextCallback != null)
                this.Text = this.GetTextCallback(this);
            this.TimeIntoAnimation += time.ElapsedGameTime;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha) {
            if (this.Background != null)
                batch.Draw(this.Background, this.Area, this.BackgroundColor * alpha);

            var pos = this.DisplayArea.Location.ToVector2();
            var sc = this.TextScale * this.Scale;
            this.formattedText.Draw(this.regularFont, batch, pos, this.TextColor * alpha, sc, this.boldFont, this.italicFont, 0, this.TimeIntoAnimation);
            base.Draw(time, batch, alpha);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale = style.TextScale;
            this.regularFont = style.Font;
            this.boldFont = style.BoldFont ?? style.Font;
            this.italicFont = style.ItalicFont ?? style.Font;
        }

        public delegate string TextCallback(Paragraph paragraph);

    }

}