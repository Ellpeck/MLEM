using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Misc;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A paragraph element for use inside of a <see cref="UiSystem"/>.
    /// A paragraph is an element that contains text.
    /// A paragraph's text can be formatted using the ui system's <see cref="UiSystem.TextFormatter"/>.
    /// </summary>
    public class Paragraph : Element {

        /// <summary>
        /// The font that this paragraph draws text with.
        /// To set its bold and italic font, use <see cref="GenericFont.Bold"/> and <see cref="GenericFont.Italic"/>.
        /// </summary>
        public StyleProp<GenericFont> RegularFont {
            get => this.regularFont;
            set {
                this.regularFont = value;
                this.SetAreaDirty();
                this.TokenizedText = null;
            }
        }
        /// <summary>
        /// The tokenized version of the <see cref="Text"/>
        /// </summary>
        public TokenizedString TokenizedText { get; private set; }
        /// <summary>
        /// The color that the text will be rendered with
        /// </summary>
        public StyleProp<Color> TextColor;
        /// <summary>
        /// The scale that the text will be rendered with.
        /// To add a multiplier rather than changing the scale directly, use <see cref="TextScaleMultiplier"/>.
        /// </summary>
        public StyleProp<float> TextScale;
        /// <summary>
        /// A multiplier that will be applied to <see cref="TextScale"/>.
        /// To change the text scale itself, use <see cref="TextScale"/>.
        /// </summary>
        public float TextScaleMultiplier = 1;
        /// <summary>
        /// The text to render inside of this paragraph.
        /// Use <see cref="GetTextCallback"/> if the text changes frequently.
        /// </summary>
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
                    this.TokenizedText = null;
                }
            }
        }
        /// <summary>
        /// If this paragraph should automatically adjust its width based on the width of the text within it
        /// </summary>
        public bool AutoAdjustWidth;
        /// <summary>
        /// Whether this paragraph should be truncated instead of split if the displayed <see cref="Text"/>'s width exceeds the provided width.
        /// When the string is truncated, the <see cref="Ellipsis"/> is added to its end.
        /// </summary>
        public bool TruncateIfLong;
        /// <summary>
        /// The ellipsis characters to use if <see cref="TruncateIfLong"/> is enabled and the string is truncated.
        /// If this is set to an empty string, no ellipsis will be attached to the truncated string.
        /// </summary>
        public string Ellipsis = "...";
        /// <summary>
        /// An event that gets called when this paragraph's <see cref="Text"/> is queried.
        /// Use this event for setting this paragraph's text if it changes frequently.
        /// </summary>
        public TextCallback GetTextCallback;
        /// <summary>
        /// The action that is executed if <see cref="Link"/> objects inside of this paragraph are pressed.
        /// By default, <see cref="MlemPlatform.OpenLinkOrFile"/> is executed.
        /// </summary>
        public Action<Link, LinkCode> LinkAction;
        /// <summary>
        /// The <see cref="TextAlignment"/> that this paragraph's text should be rendered with
        /// </summary>
        public TextAlignment Alignment {
            get => this.alignment;
            set {
                this.alignment = value;
                this.SetAreaDirty();
                this.TokenizedText = null;
            }
        }

        private string text;
        private TextAlignment alignment;
        private StyleProp<GenericFont> regularFont;

        /// <summary>
        /// Creates a new paragraph with the given settings.
        /// </summary>
        /// <param name="anchor">The paragraph's anchor</param>
        /// <param name="width">The paragraph's width. Note that its height is automatically calculated.</param>
        /// <param name="textCallback">The paragraph's text</param>
        /// <param name="autoAdjustWidth">Whether the paragraph's width should automatically be calculated based on the text within it.</param>
        public Paragraph(Anchor anchor, float width, TextCallback textCallback, bool autoAdjustWidth = false)
            : this(anchor, width, "", autoAdjustWidth) {
            this.GetTextCallback = textCallback;
            this.Text = textCallback(this);
            if (this.Text == null)
                this.IsHidden = true;
        }

        /// <inheritdoc cref="Paragraph(Anchor,float,TextCallback,bool)"/>
        public Paragraph(Anchor anchor, float width, string text, bool autoAdjustWidth = false) : base(anchor, new Vector2(width, 0)) {
            this.Text = text;
            if (this.Text == null)
                this.IsHidden = true;
            this.AutoAdjustWidth = autoAdjustWidth;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        /// <inheritdoc />
        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            var size = base.CalcActualSize(parentArea);
            this.ParseText(size);
            var (w, h) = this.TokenizedText.Measure(this.RegularFont) * this.TextScale * this.TextScaleMultiplier * this.Scale;
            return new Vector2(this.AutoAdjustWidth ? w + this.ScaledPadding.Width : size.X, h + this.ScaledPadding.Height);
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            this.QueryTextCallback();
            base.Update(time);
            if (this.TokenizedText != null)
                this.TokenizedText.Update(time);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, Effect effect, Matrix matrix) {
            var pos = this.DisplayArea.Location + new Vector2(this.GetAlignmentOffset(), 0);
            var sc = this.TextScale * this.TextScaleMultiplier * this.Scale;
            var color = this.TextColor.OrDefault(Color.White) * alpha;
            this.TokenizedText.Draw(time, batch, pos, this.RegularFont, color, sc, 0);
            base.Draw(time, batch, alpha, blendState, samplerState, depthStencilState, effect, matrix);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.RegularFont = this.RegularFont.OrStyle(style.Font ?? throw new NotSupportedException("Paragraphs cannot use ui styles that don't have a font. Please supply a custom font by setting UiStyle.Font."));
            this.TextScale = this.TextScale.OrStyle(style.TextScale);
            this.TextColor = this.TextColor.OrStyle(style.TextColor);
        }

        /// <summary>
        /// Parses this paragraph's <see cref="Text"/> into <see cref="TokenizedText"/>.
        /// Additionally, this method adds any <see cref="Link"/> elements for tokenized links in the text.
        /// </summary>
        /// <param name="size">The paragraph's default size</param>
        protected virtual void ParseText(Vector2 size) {
            if (this.TokenizedText == null) {
                // tokenize the text
                this.TokenizedText = this.System.TextFormatter.Tokenize(this.RegularFont, this.Text, this.Alignment);

                // add links to the paragraph
                this.RemoveChildren(c => c is Link);
                foreach (var link in this.TokenizedText.Tokens.Where(t => t.AppliedCodes.Any(c => c is LinkCode)))
                    this.AddChild(new Link(Anchor.TopLeft, link, this.TextScale * this.TextScaleMultiplier));
            }

            var width = size.X - this.ScaledPadding.Width;
            var scale = this.TextScale * this.TextScaleMultiplier * this.Scale;
            if (this.TruncateIfLong) {
                this.TokenizedText.Truncate(this.RegularFont, width, scale, this.Ellipsis, this.Alignment);
            } else {
                this.TokenizedText.Split(this.RegularFont, width, scale, this.Alignment);
            }
        }

        private void QueryTextCallback() {
            if (this.GetTextCallback != null)
                this.Text = this.GetTextCallback(this);
        }

        private float GetAlignmentOffset() {
            switch (this.Alignment) {
                case TextAlignment.Center:
                    return this.DisplayArea.Width / 2;
                case TextAlignment.Right:
                    return this.DisplayArea.Width;
            }
            return 0;
        }

        /// <summary>
        /// A delegate method used for <see cref="Paragraph.GetTextCallback"/>
        /// </summary>
        /// <param name="paragraph">The current paragraph</param>
        public delegate string TextCallback(Paragraph paragraph);

        /// <summary>
        /// A link is a sub-element of the <see cref="Paragraph"/> that is added onto it as a child for any tokens that contain <see cref="LinkCode"/>, to make them selectable and clickable.
        /// </summary>
        public class Link : Element {

            /// <summary>
            /// The token that this link represents
            /// </summary>
            public readonly Token Token;
            private readonly float textScale;

            /// <summary>
            /// Creates a new link element with the given settings
            /// </summary>
            /// <param name="anchor">The link's anchor</param>
            /// <param name="token">The token that this link represents</param>
            /// <param name="textScale">The scale that text is rendered with</param>
            public Link(Anchor anchor, Token token, float textScale) : base(anchor, Vector2.Zero) {
                this.Token = token;
                this.textScale = textScale;
                this.OnPressed += e => {
                    foreach (var code in token.AppliedCodes.OfType<LinkCode>()) {
                        if (this.Parent is Paragraph p && p.LinkAction != null) {
                            p.LinkAction.Invoke(this, code);
                        } else {
                            MlemPlatform.Current.OpenLinkOrFile(code.Match.Groups[1].Value);
                        }
                    }
                };
            }

            /// <inheritdoc />
            public override void ForceUpdateArea() {
                // set the position offset and size to the token's first area
                var area = this.Token.GetArea(Vector2.Zero, this.textScale).FirstOrDefault();
                if (this.Parent is Paragraph p)
                    area.Location += new Vector2(p.GetAlignmentOffset() / p.Scale, 0);
                this.PositionOffset = area.Location;
                this.IsHidden = area.IsEmpty;
                this.Size = area.Size;
                base.ForceUpdateArea();
            }

            /// <inheritdoc />
            public override Element GetElementUnderPos(Vector2 position) {
                var ret = base.GetElementUnderPos(position);
                if (ret != null)
                    return ret;
                // check if any of our token's parts are hovered
                foreach (var rect in this.Token.GetArea(this.Parent.DisplayArea.Location, this.Scale * this.textScale)) {
                    if (rect.Contains(this.TransformInverse(position)))
                        return this;
                }
                return null;
            }

        }

    }
}