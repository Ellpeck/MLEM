using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private string splitText;
        private Dictionary<int, FormattingCode> codeLocations;
        public StyleProp<IGenericFont> RegularFont;
        public StyleProp<IGenericFont> BoldFont;
        public StyleProp<IGenericFont> ItalicFont;

        public StyleProp<NinePatch> Background;
        public StyleProp<Color> BackgroundColor;
        public StyleProp<Color> TextColor;
        public StyleProp<float> TextScale;
        public string Text {
            get => this.text;
            set {
                if (this.text != value) {
                    this.text = value;
                    this.IsHidden = string.IsNullOrWhiteSpace(this.text);
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
            if (this.Text == null)
                this.IsHidden = true;
        }

        public Paragraph(Anchor anchor, float width, string text, bool centerText = false) : base(anchor, new Vector2(width, 0)) {
            this.Text = text;
            if (this.Text == null)
                this.IsHidden = true;
            this.AutoAdjustWidth = centerText;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            var size = base.CalcActualSize(parentArea);

            var sc = this.TextScale * this.Scale;
            this.splitText = this.RegularFont.Value.SplitString(this.text.RemoveFormatting(), size.X - this.ScaledPadding.Width, sc);
            this.codeLocations = this.text.GetFormattingCodes();

            var textDims = this.RegularFont.Value.MeasureString(this.splitText) * sc;
            return new Vector2(this.AutoAdjustWidth ? textDims.X + this.ScaledPadding.Width : size.X, textDims.Y + this.ScaledPadding.Height);
        }

        public override void ForceUpdateArea() {
            if (this.GetTextCallback != null)
                this.Text = this.GetTextCallback(this);
            base.ForceUpdateArea();
        }

        public override void Update(GameTime time) {
            base.Update(time);
            if (this.GetTextCallback != null)
                this.Text = this.GetTextCallback(this);
            this.TimeIntoAnimation += time.ElapsedGameTime;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            if (this.Background.Value != null)
                batch.Draw(this.Background, this.Area, (Color) this.BackgroundColor * alpha);

            var pos = this.DisplayArea.Location;
            var sc = this.TextScale * this.Scale;

            var color = this.TextColor.OrDefault(Color.White) * alpha;
            // if we don't have any formatting codes, then we don't need to do complex drawing
            if (this.codeLocations.Count <= 0) {
                this.RegularFont.Value.DrawString(batch, this.splitText, pos, color, 0, Vector2.Zero, sc, SpriteEffects.None, 0);
            } else {
                // if we have formatting codes, we should do it
                this.RegularFont.Value.DrawFormattedString(batch, pos, this.splitText, this.codeLocations, color, sc, this.BoldFont.Value, this.ItalicFont.Value, 0, this.TimeIntoAnimation);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale.SetFromStyle(style.TextScale);
            this.RegularFont.SetFromStyle(style.Font);
            this.BoldFont.SetFromStyle(style.BoldFont ?? style.Font);
            this.ItalicFont.SetFromStyle(style.ItalicFont ?? style.Font);
        }

        public delegate string TextCallback(Paragraph paragraph);

    }
}