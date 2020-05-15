using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Input;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private string splitText;
        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public FormattingCodeCollection Formatting;
        public StyleProp<GenericFont> RegularFont;
        public StyleProp<GenericFont> BoldFont;
        public StyleProp<GenericFont> ItalicFont;
        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public StyleProp<FormatSettings> FormatSettings;
        public readonly TextFormatter Formatter;
        public TokenizedString TokenizedText { get; private set; }
        public Token HoveredToken { get; private set; }

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
        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public TextModifier RenderedTextModifier = text => text;
        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
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

            this.Formatter = new TextFormatter(() => this.BoldFont, () => this.ItalicFont);
            this.Formatter.Codes.Add(new Regex("<l ([^>]+)>"), (f, m, r) => new LinkCode(m, r, 1 / 16F, 0.85F, t => t == this.HoveredToken, l => {
                if (!this.Input.IsPressed(MouseButton.Left))
                    return;
                try {
                    Process.Start(l.Match.Groups[1].Value);
                } catch (Exception) {
                    // ignored
                }
            }));
        }

        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            var size = base.CalcActualSize(parentArea);
            var sc = this.TextScale * this.Scale;

            // old formatting stuff
            this.splitText = this.RegularFont.Value.SplitString(this.text.RemoveFormatting(this.RegularFont.Value), size.X - this.ScaledPadding.Width, sc);
            this.Formatting = this.text.GetFormattingCodes(this.RegularFont.Value);
            if (this.Formatting.Count > 0) {
                var textDims = this.RegularFont.Value.MeasureString(this.splitText) * sc;
                return new Vector2(this.AutoAdjustWidth ? textDims.X + this.ScaledPadding.Width : size.X, textDims.Y + this.ScaledPadding.Height);
            }

            this.TokenizedText = this.Formatter.Tokenize(this.RegularFont, this.text);
            this.TokenizedText.Split(this.RegularFont, size.X - this.ScaledPadding.Width, sc);
            this.CanBeMoused = this.TokenizedText.AllCodes.OfType<LinkCode>().Any();

            var dims = this.TokenizedText.Measure(this.RegularFont) * sc;
            return new Vector2(this.AutoAdjustWidth ? dims.X + this.ScaledPadding.Width : size.X, dims.Y + this.ScaledPadding.Height);
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

            if (this.TokenizedText != null) {
                this.TokenizedText.Update(time);
                this.HoveredToken = this.TokenizedText.GetTokenUnderPos(this.RegularFont, this.DisplayArea.Location, this.Input.MousePosition.ToVector2(), this.TextScale * this.Scale);
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var pos = this.DisplayArea.Location;
            var sc = this.TextScale * this.Scale;

            var color = this.TextColor.OrDefault(Color.White) * alpha;
            // legacy formatting stuff
            if (this.Formatting.Count > 0) {
                var toRender = this.RenderedTextModifier(this.splitText);
                this.RegularFont.Value.DrawFormattedString(batch, pos, toRender, this.Formatting, color, sc, this.BoldFont.Value, this.ItalicFont.Value, 0, this.TimeIntoAnimation, this.FormatSettings);
            } else {
                this.TokenizedText.Draw(time, batch, pos, this.RegularFont, color, sc, 0);
            }
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.TextScale.SetFromStyle(style.TextScale);
            this.RegularFont.SetFromStyle(style.Font);
            this.BoldFont.SetFromStyle(style.BoldFont ?? style.Font);
            this.ItalicFont.SetFromStyle(style.ItalicFont ?? style.Font);
            this.FormatSettings.SetFromStyle(style.FormatSettings);
        }

        public delegate string TextCallback(Paragraph paragraph);

        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public delegate string TextModifier(string text);

    }
}