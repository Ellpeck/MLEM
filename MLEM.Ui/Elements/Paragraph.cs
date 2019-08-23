using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Textures;
using MLEM.Ui.Format;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Paragraph : Element {

        private string text;
        private float lineHeight;
        private float longestLineLength;
        private string[] splitText;
        private Dictionary<int, FormattingCode> codeLocations;
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
        public float LineSpace = 1;

        public Paragraph(Anchor anchor, float width, TextCallback textCallback, bool centerText = false)
            : this(anchor, width, "", centerText) {
            this.GetTextCallback = textCallback;
            this.Text = textCallback(this);
        }

        public Paragraph(Anchor anchor, float width, string text, bool centerText = false) : base(anchor, new Vector2(width, 0)) {
            this.text = text;
            this.AutoAdjustWidth = centerText;
            this.IgnoresMouse = true;
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            var size = base.CalcActualSize(parentArea);
            var sc = this.TextScale * this.Scale;
            this.splitText = this.regularFont.SplitString(this.text.RemoveFormatting(), size.X - this.ScaledPadding.X * 2, sc).ToArray();
            this.codeLocations = this.text.GetFormattingCodes();

            this.lineHeight = 0;
            this.longestLineLength = 0;
            foreach (var strg in this.splitText) {
                var strgScale = this.regularFont.MeasureString(strg) * sc;
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
            var sc = this.TextScale * this.Scale;

            // if we don't have any formatting codes, then we don't need to do complex drawing
            if (this.codeLocations.Count <= 0) {
                foreach (var line in this.splitText) {
                    this.regularFont.DrawString(batch, line, pos + off, this.TextColor * alpha, 0, Vector2.Zero, sc, SpriteEffects.None, 0);
                    off.Y += this.lineHeight;
                }
            } else {
                // if we have formatting codes, we need to go through each index and see how it should be drawn
                var characterCounter = 0;
                var currColor = this.TextColor;
                var currFont = this.regularFont;

                foreach (var line in this.splitText) {
                    var lineOffset = new Vector2();
                    foreach (var c in line) {
                        // check if the current character's index has a formatting code
                        this.codeLocations.TryGetValue(characterCounter, out var code);
                        if (code != null) {
                            // if so, apply it
                            if (code.IsColorCode) {
                                currColor = code.Color;
                            } else {
                                switch (code.Style) {
                                    case TextStyle.Regular:
                                        currFont = this.regularFont;
                                        break;
                                    case TextStyle.Bold:
                                        currFont = this.boldFont;
                                        break;
                                    case TextStyle.Italic:
                                        currFont = this.italicFont;
                                        break;
                                }
                            }
                        }

                        var cSt = c.ToString();
                        currFont.DrawString(batch, cSt, pos + off + lineOffset, currColor * alpha, 0, Vector2.Zero, sc, SpriteEffects.None, 0);

                        // get the width based on the regular font so that the split text doesn't overshoot the borders
                        // this is a bit of a hack, but bold fonts shouldn't be that much thicker so it won't look bad
                        lineOffset.X += this.regularFont.MeasureString(cSt).X * sc;
                        characterCounter++;
                    }
                    // spaces are replaced by newline characters, account for that
                    characterCounter++;
                    off.Y += this.lineHeight;
                }
            }
            base.Draw(time, batch, alpha, offset);
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