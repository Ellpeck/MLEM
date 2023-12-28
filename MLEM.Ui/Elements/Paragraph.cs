using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Graphics;
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
                this.SetTextDirty();
            }
        }
        /// <summary>
        /// The tokenized version of the <see cref="Text"/>
        /// </summary>
        public TokenizedString TokenizedText {
            get {
                this.CheckTextChange();
                this.TokenizeIfNecessary();
                return this.tokenizedText;
            }
        }
        /// <summary>
        /// The color that the text will be rendered with
        /// </summary>
        public StyleProp<Color> TextColor;
        /// <summary>
        /// The scale that the text will be rendered with.
        /// To add a multiplier rather than changing the scale directly, use <see cref="TextScaleMultiplier"/>.
        /// </summary>
        public StyleProp<float> TextScale {
            get => this.textScale;
            set {
                this.textScale = value;
                this.SetTextDirty();
            }
        }
        /// <summary>
        /// A multiplier that will be applied to <see cref="TextScale"/>.
        /// To change the text scale itself, use <see cref="TextScale"/>.
        /// </summary>
        public float TextScaleMultiplier {
            get => this.textScaleMultiplier;
            set {
                if (this.textScaleMultiplier != value) {
                    this.textScaleMultiplier = value;
                    this.SetTextDirty();
                }
            }
        }
        /// <summary>
        /// The text to render inside of this paragraph.
        /// Use <see cref="GetTextCallback"/> if the text changes frequently.
        /// </summary>
        public string Text {
            get {
                this.CheckTextChange();
                return this.displayedText;
            }
            set {
                this.explicitlySetText = value;
                this.CheckTextChange();
            }
        }
        /// <summary>
        /// If this paragraph should automatically adjust its width based on the width of the text within it
        /// </summary>
        public bool AutoAdjustWidth {
            get => this.autoAdjustWidth;
            set {
                if (this.autoAdjustWidth != value) {
                    this.autoAdjustWidth = value;
                    this.SetAreaDirty();
                }
            }
        }
        /// <summary>
        /// Whether this paragraph should be truncated instead of split if the displayed <see cref="Text"/>'s width exceeds the provided width.
        /// When the string is truncated, the <see cref="Ellipsis"/> is added to its end.
        /// </summary>
        public bool TruncateIfLong {
            get => this.truncateIfLong;
            set {
                if (this.truncateIfLong != value) {
                    this.truncateIfLong = value;
                    this.SetAlignSplitDirty();
                }
            }
        }
        /// <summary>
        /// The ellipsis characters to use if <see cref="TruncateIfLong"/> is enabled and the string is truncated.
        /// If this is set to an empty string, no ellipsis will be attached to the truncated string.
        /// </summary>
        public string Ellipsis {
            get => this.ellipsis;
            set {
                if (this.ellipsis != value) {
                    this.ellipsis = value;
                    this.SetAlignSplitDirty();
                }
            }
        }
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
        public StyleProp<TextAlignment> Alignment {
            get => this.alignment;
            set {
                this.alignment = value;
                this.SetTextDirty();
            }
        }
        /// <summary>
        /// The inclusive index in this paragraph's <see cref="Text"/> to start drawing at.
        /// This value is passed to <see cref="TokenizedString.Draw"/>.
        /// </summary>
        public int? DrawStartIndex;
        /// <summary>
        /// The exclusive index in this paragraph's <see cref="Text"/> to stop drawing at.
        /// This value is passed to <see cref="TokenizedString.Draw"/>.
        /// </summary>
        public int? DrawEndIndex;

        /// <inheritdoc />
        public override bool IsHidden => base.IsHidden || string.IsNullOrWhiteSpace(this.Text);

        private string displayedText;
        private string explicitlySetText;
        private StyleProp<TextAlignment> alignment;
        private StyleProp<GenericFont> regularFont;
        private StyleProp<float> textScale;
        private TokenizedString tokenizedText;
        private float? lastAlignSplitWidth;
        private float? lastAlignSplitScale;
        private string ellipsis = "...";
        private bool truncateIfLong;
        private float textScaleMultiplier = 1;
        private bool autoAdjustWidth;

        /// <summary>
        /// Creates a new paragraph with the given settings.
        /// </summary>
        /// <param name="anchor">The paragraph's anchor</param>
        /// <param name="width">The paragraph's width. Note that its height is automatically calculated.</param>
        /// <param name="textCallback">The paragraph's text</param>
        /// <param name="alignment">The paragraph's text alignment.</param>
        /// <param name="autoAdjustWidth">Whether the paragraph's width should automatically be calculated based on the text within it.</param>
        public Paragraph(Anchor anchor, float width, TextCallback textCallback, TextAlignment alignment, bool autoAdjustWidth = false) : this(anchor, width, textCallback, autoAdjustWidth) {
            this.Alignment = alignment;
        }

        /// <summary>
        /// Creates a new paragraph with the given settings.
        /// </summary>
        /// <param name="anchor">The paragraph's anchor</param>
        /// <param name="width">The paragraph's width. Note that its height is automatically calculated.</param>
        /// <param name="text">The paragraph's text</param>
        /// <param name="alignment">The paragraph's text alignment.</param>
        /// <param name="autoAdjustWidth">Whether the paragraph's width should automatically be calculated based on the text within it.</param>
        public Paragraph(Anchor anchor, float width, string text, TextAlignment alignment, bool autoAdjustWidth = false) : this(anchor, width, text, autoAdjustWidth) {
            this.Alignment = alignment;
        }

        /// <summary>
        /// Creates a new paragraph with the given settings.
        /// </summary>
        /// <param name="anchor">The paragraph's anchor</param>
        /// <param name="width">The paragraph's width. Note that its height is automatically calculated.</param>
        /// <param name="textCallback">The paragraph's text</param>
        /// <param name="autoAdjustWidth">Whether the paragraph's width should automatically be calculated based on the text within it.</param>
        public Paragraph(Anchor anchor, float width, TextCallback textCallback, bool autoAdjustWidth = false) : this(anchor, width, string.Empty, autoAdjustWidth) {
            this.GetTextCallback = textCallback;
        }

        /// <summary>
        /// Creates a new paragraph with the given settings.
        /// </summary>
        /// <param name="anchor">The paragraph's anchor</param>
        /// <param name="width">The paragraph's width. Note that its height is automatically calculated.</param>
        /// <param name="text">The paragraph's text</param>
        /// <param name="autoAdjustWidth">Whether the paragraph's width should automatically be calculated based on the text within it.</param>
        public Paragraph(Anchor anchor, float width, string text, bool autoAdjustWidth = false) : base(anchor, new Vector2(width, 0)) {
            this.Text = text;
            this.AutoAdjustWidth = autoAdjustWidth;
            this.CanBeSelected = false;
            this.CanBeMoused = false;
        }

        /// <inheritdoc />
        public override void SetAreaAndUpdateChildren(RectangleF area) {
            base.SetAreaAndUpdateChildren(area);
            // in case an outside source sets our area, we still want to display our text correctly
            this.AlignAndSplitIfNecessary(area.Size);
        }

        /// <inheritdoc />
        protected override Vector2 CalcActualSize(RectangleF parentArea) {
            var size = base.CalcActualSize(parentArea);
            this.CheckTextChange();
            this.TokenizeIfNecessary();
            this.AlignAndSplitIfNecessary(size);
            var textSize = this.tokenizedText.GetArea(Vector2.Zero, this.TextScale * this.TextScaleMultiplier * this.Scale).Size;
            // if we auto-adjust our width, then we would also split the same way with our adjusted width, so cache that
            if (this.AutoAdjustWidth)
                this.lastAlignSplitWidth = textSize.X;
            return new Vector2(this.AutoAdjustWidth ? textSize.X + this.ScaledPadding.Width : size.X, textSize.Y + this.ScaledPadding.Height);
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            this.TokenizedText?.Update(time);
            base.Update(time);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            var pos = this.DisplayArea.Location + new Vector2(this.GetAlignmentOffset(), 0);
            var sc = this.TextScale * this.TextScaleMultiplier * this.Scale;
            var color = this.TextColor.OrDefault(Color.White) * alpha;
            this.TokenizedText.Draw(time, batch, pos, this.RegularFont, color, sc, 0, this.DrawStartIndex, this.DrawEndIndex);
            base.Draw(time, batch, alpha, context);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.RegularFont = this.RegularFont.OrStyle(style.Font ?? throw new NotSupportedException("Paragraphs cannot use ui styles that don't have a font. Please supply a custom font by setting UiStyle.Font."));
            this.TextScale = this.TextScale.OrStyle(style.TextScale);
            this.TextColor = this.TextColor.OrStyle(style.TextColor);
            this.Alignment = this.Alignment.OrStyle(style.TextAlignment);
        }

        private void SetTextDirty() {
            this.tokenizedText = null;
            // only set our area dirty if our size changed as a result of this action
            if (!this.AreaDirty && (this.System == null || !this.CalcActualSize(this.ParentArea).Equals(this.DisplayArea.Size, Element.Epsilon)))
                this.SetAreaDirty();
        }

        private void CheckTextChange() {
            var newText = this.GetTextCallback?.Invoke(this) ?? this.explicitlySetText;
            if (this.displayedText == newText)
                return;
            var emptyChanged = string.IsNullOrWhiteSpace(this.displayedText) != string.IsNullOrWhiteSpace(newText);
            this.displayedText = newText;
            if (emptyChanged)
                this.SetAreaDirty();
            this.SetTextDirty();
        }

        private float GetAlignmentOffset() {
            switch (this.Alignment.Value) {
                case TextAlignment.Center:
                    return this.DisplayArea.Width / 2;
                case TextAlignment.Right:
                    return this.DisplayArea.Width;
            }
            return 0;
        }

        private void TokenizeIfNecessary() {
            if (this.tokenizedText != null)
                return;

            // tokenize the text
            this.tokenizedText = this.System.TextFormatter.Tokenize(this.RegularFont, this.Text, this.Alignment);
            this.SetAlignSplitDirty();

            // add links to the paragraph
            this.RemoveChildren(c => c is Link);
            foreach (var link in this.tokenizedText.Tokens.Where(t => t.AppliedCodes.Any(c => c is LinkCode)))
                this.AddChild(new Link(Anchor.TopLeft, link, this.TextScale * this.TextScaleMultiplier));
        }

        private void AlignAndSplitIfNecessary(Vector2 size) {
            var width = size.X - this.ScaledPadding.Width;
            var scale = this.TextScale * this.TextScaleMultiplier * this.Scale;

            if (this.lastAlignSplitWidth?.Equals(width, Element.Epsilon) == true && this.lastAlignSplitScale?.Equals(scale, Element.Epsilon) == true)
                return;
            this.lastAlignSplitWidth = width;
            this.lastAlignSplitScale = scale;

            if (this.TruncateIfLong) {
                this.tokenizedText.Truncate(this.RegularFont, width, scale, this.Ellipsis, this.Alignment);
            } else {
                this.tokenizedText.Split(this.RegularFont, width, scale, this.Alignment);
            }
        }

        private void SetAlignSplitDirty() {
            this.lastAlignSplitWidth = null;
            this.lastAlignSplitScale = null;
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
                var location = this.Parent.DisplayArea.Location;
                if (this.Parent is Paragraph p)
                    location.X += p.GetAlignmentOffset();
                foreach (var rect in this.Token.GetArea(location, this.Scale * this.textScale)) {
                    if (rect.Contains(this.TransformInverse(position)))
                        return this;
                }
                return null;
            }

        }

    }
}
