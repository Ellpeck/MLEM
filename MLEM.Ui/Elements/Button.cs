using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A button element for use inside of a <see cref="UiSystem"/>.
    /// A button element can be pressed, hovered over and that can be disabled.
    /// </summary>
    public class Button : Element {

        /// <summary>
        /// The button's texture
        /// </summary>
        public StyleProp<NinePatch> Texture;
        /// <summary>
        /// The color that the button draws its texture with
        /// </summary>
        public StyleProp<Color> NormalColor = Color.White;
        /// <summary>
        /// The texture that the button uses while being moused over.
        /// If this is null, it uses its default <see cref="Texture"/>.
        /// </summary>
        public StyleProp<NinePatch> HoveredTexture;
        /// <summary>
        /// The color that the button uses for drawing while being moused over
        /// </summary>
        public StyleProp<Color> HoveredColor;
        /// <summary>
        /// The texture that the button uses when it <see cref="IsDisabled"/>.
        /// If this is null, it uses its default <see cref="Texture"/>.
        /// </summary>
        public StyleProp<NinePatch> DisabledTexture;
        /// <summary>
        /// The color that the button uses for drawing when it <see cref="IsDisabled"/>
        /// </summary>
        public StyleProp<Color> DisabledColor;
        /// <summary>
        /// The <see cref="Paragraph"/> of text that is displayed on the button.
        /// Note that this is only nonnull by default if the constructor was passed a nonnull text.
        /// </summary>
        public Paragraph Text;
        /// <summary>
        /// The <see cref="Tooltip"/> that is displayed when hovering over the button.
        /// Note that this is only nonnull by default if the constructor was passed a nonnull tooltip text.
        /// </summary>
        public Tooltip Tooltip;

        private bool isDisabled;
        /// <summary>
        /// Set this property to true to mark the button as disabled.
        /// A disabled button cannot be moused over, selected or pressed.
        /// </summary>
        public bool IsDisabled {
            get => this.isDisabled;
            set {
                this.isDisabled = value;
                this.CanBePressed = !value;
                this.CanBeSelected = !value;
            }
        }
        /// <summary>
        /// Whether this button's <see cref="Text"/> should be truncated if it exceeds this button's width.
        /// Defaults to false.
        /// </summary>
        public bool TruncateTextIfLong {
            get => this.Text?.TruncateIfLong ?? false;
            set {
                if (this.Text != null)
                    this.Text.TruncateIfLong = value;
            }
        }

        /// <summary>
        /// Creates a new button with the given settings
        /// </summary>
        /// <param name="anchor">The button's anchor</param>
        /// <param name="size">The button's size</param>
        /// <param name="text">The text that should be displayed on the button</param>
        /// <param name="tooltipText">The text that should be displayed in a <see cref="Tooltip"/> when hovering over this button</param>
        public Button(Anchor anchor, Vector2 size, string text = null, string tooltipText = null) : base(anchor, size) {
            if (text != null) {
                this.Text = new Paragraph(Anchor.Center, 1, text, true) {Padding = new Vector2(1)};
                this.AddChild(this.Text);
            }
            if (tooltipText != null)
                this.Tooltip = this.AddTooltip(tooltipText);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var tex = this.Texture;
            var color = (Color) this.NormalColor * alpha;
            if (this.IsDisabled) {
                tex = this.DisabledTexture.OrDefault(tex);
                color = (Color) this.DisabledColor * alpha;
            } else if (this.IsMouseOver) {
                tex = this.HoveredTexture.OrDefault(tex);
                color = (Color) this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea, color, this.Scale);
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture.SetFromStyle(style.ButtonTexture);
            this.HoveredTexture.SetFromStyle(style.ButtonHoveredTexture);
            this.HoveredColor.SetFromStyle(style.ButtonHoveredColor);
            this.DisabledTexture.SetFromStyle(style.ButtonDisabledTexture);
            this.DisabledColor.SetFromStyle(style.ButtonDisabledColor);
        }

    }
}