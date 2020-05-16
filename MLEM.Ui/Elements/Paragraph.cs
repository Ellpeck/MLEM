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
        [Obsolete("Use the new GenericFont.Bold and GenericFont.Italic instead")]
        public StyleProp<GenericFont> BoldFont;
        [Obsolete("Use the new GenericFont.Bold and GenericFont.Italic instead")]
        public StyleProp<GenericFont> ItalicFont;
        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public StyleProp<FormatSettings> FormatSettings;
        public TokenizedString TokenizedText { get; private set; }

        public StyleProp<Color> TextColor;
        public StyleProp<float> TextScale;
        public string Text {
            get {
                this.QueryTextCallback();
                return this.text;
            }
            set {
                if (this.text != value) {
                    this.text = value;
                    this.IsHidden = string.IsNullOrWhiteSpace(this.text);
                    this.SetAreaDirty();
                    // force text to be re-tokenized
                    this.TokenizedText = null;
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
        }

        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            var size = base.CalcActualSize(parentArea);
            this.ParseText(size);

            // old formatting stuff
            if (this.Formatting.Count > 0) {
                var textDims = this.RegularFont.Value.MeasureString(this.splitText) * this.TextScale * this.Scale;
                return new Vector2(this.AutoAdjustWidth ? textDims.X + this.ScaledPadding.Width : size.X, textDims.Y + this.ScaledPadding.Height);
            }

            var dims = this.TokenizedText.Measure(this.RegularFont) * this.TextScale * this.Scale;
            return new Vector2(this.AutoAdjustWidth ? dims.X + this.ScaledPadding.Width : size.X, dims.Y + this.ScaledPadding.Height);
        }

        public override void Update(GameTime time) {
            this.QueryTextCallback();
            base.Update(time);
            
            this.TimeIntoAnimation += time.ElapsedGameTime;

            if (this.TokenizedText != null)
                this.TokenizedText.Update(time);
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

        protected virtual void ParseText(Vector2 size) {
            // old formatting stuff
            this.splitText = this.RegularFont.Value.SplitString(this.Text.RemoveFormatting(this.RegularFont.Value), size.X - this.ScaledPadding.Width, this.TextScale * this.Scale);
            this.Formatting = this.Text.GetFormattingCodes(this.RegularFont.Value);

            if (this.TokenizedText == null)
                this.TokenizedText = this.System.TextFormatter.Tokenize(this.RegularFont, this.Text);

            this.TokenizedText.Split(this.RegularFont, size.X - this.ScaledPadding.Width, this.TextScale * this.Scale);
            var linkTokens = this.TokenizedText.Tokens.Where(t => t.AppliedCodes.Any(c => c is LinkCode)).ToArray();
            // this basically checks if there are any tokens that have an area that doesn't have a link element associated with it
            if (linkTokens.Any(t => !t.GetArea(Vector2.Zero, this.TextScale).All(a => this.GetChildren<Link>(c => c.PositionOffset == a.Location && c.Size == a.Size).Any()))) {
                this.RemoveChildren(c => c is Link);
                foreach (var link in linkTokens) {
                    var areas = link.GetArea(Vector2.Zero, this.TextScale).ToArray();
                    for (var i = 0; i < areas.Length; i++) {
                        var area = areas[i];
                        this.AddChild(new Link(Anchor.TopLeft, link, area.Size) {
                            PositionOffset = area.Location,
                            // only allow selecting the first part of a link
                            CanBeSelected = i == 0
                        });
                    }
                }
            }
        }

        private void QueryTextCallback() {
            if (this.GetTextCallback != null)
                this.Text = this.GetTextCallback(this);
        }

        public delegate string TextCallback(Paragraph paragraph);

        [Obsolete("Use the new text formatting system in MLEM.Formatting instead")]
        public delegate string TextModifier(string text);

        public class Link : Element {

            public readonly Token Token;

            public Link(Anchor anchor, Token token, Vector2 size) : base(anchor, size) {
                this.Token = token;
                this.OnPressed += e => {
                    foreach (var code in token.AppliedCodes.OfType<LinkCode>()) {
                        try {
                            Process.Start(code.Match.Groups[1].Value);
                        } catch (Exception) {
                            // ignored
                        }
                    }
                };
            }

        }

    }
}