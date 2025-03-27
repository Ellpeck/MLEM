using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;
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
        /// The texture that this button uses while it <see cref="Element.IsSelected"/> and the <see cref="UiControls"/> are in auto-navigation mode (<see cref="UiControls.IsAutoNavMode"/>).
        /// If the selection indicator should not be drawn while the button is selected, keep in mind to additionally set <see cref="Element.SelectionIndicator"/> to <see langword="null"/>.
        /// If this is <see langword="null"/>, it uses its default <see cref="Texture"/> in that state.
        /// </summary>
        public StyleProp<NinePatch> SelectedTexture;
        /// <summary>
        /// The color that this button uses for drawing while it <see cref="Element.IsSelected"/> and the <see cref="UiControls"/> are in auto-navigation mode (<see cref="UiControls.IsAutoNavMode"/>).
        /// </summary>
        public StyleProp<Color> SelectedColor;
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
        /// <summary>
        /// Set this property to true to mark the button as disabled.
        /// A disabled button cannot be moused over, selected or pressed.
        /// If this value changes often, consider using <see cref="AutoDisableCondition"/>.
        /// </summary>
        public virtual bool IsDisabled {
            get => this.isDisabled || this.AutoDisableCondition?.Invoke(this) == true;
            set => this.isDisabled = value;
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
        /// Whether this button should be able to be selected even if it <see cref="IsDisabled"/>.
        /// If this is true, <see cref="CanBeSelected"/> will be able to return true even if <see cref="IsDisabled"/> is true.
        /// </summary>
        public bool CanSelectDisabled;
        /// <summary>
        /// An optional function that can be used to modify the result of <see cref="IsDisabled"/> automatically based on a user-defined condition. This removes the need to disable a button based on a condition in <see cref="Element.OnUpdated"/> or manually.
        /// Note that, if <see cref="IsDisabled"/>'s underlying value is set to <see langword="true"/> using <see cref="IsDisabled"/>, this function's result will be ignored.
        /// </summary>
        public Func<Button, bool> AutoDisableCondition;

        /// <inheritdoc />
        public override bool CanBeSelected => base.CanBeSelected && (this.CanSelectDisabled || !this.IsDisabled);
        /// <inheritdoc />
        public override bool CanBePressed => base.CanBePressed && !this.IsDisabled;

        private bool isDisabled;

        /// <summary>
        /// Creates a new button with the given settings and no text or tooltip.
        /// </summary>
        /// <param name="anchor">The button's anchor</param>
        /// <param name="size">The button's size</param>
        public Button(Anchor anchor, Vector2 size) : base(anchor, size) {}

        /// <summary>
        /// Creates a new button with the given settings
        /// </summary>
        /// <param name="anchor">The button's anchor</param>
        /// <param name="size">The button's size</param>
        /// <param name="text">The text that should be displayed on the button</param>
        /// <param name="tooltipText">The text that should be displayed in a <see cref="Tooltip"/> when hovering over this button</param>
        public Button(Anchor anchor, Vector2 size, string text = null, string tooltipText = null) : this(anchor, size) {
            if (text != null) {
                this.Text = new Paragraph(Anchor.Center, 1, text, true);
                this.Text.Padding = this.Text.Padding.OrStyle(new Padding(1), 1);
                this.AddChild(this.Text);
            }
            if (tooltipText != null)
                this.Tooltip = this.AddTooltip(tooltipText);
        }

        /// <summary>
        /// Creates a new button with the given settings
        /// </summary>
        /// <param name="anchor">The button's anchor</param>
        /// <param name="size">The button's size</param>
        /// <param name="textCallback">The text that should be displayed on the button</param>
        /// <param name="tooltipTextCallback">The text that should be displayed in a <see cref="Tooltip"/> when hovering over this button</param>
        public Button(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null) : this(anchor, size) {
            if (textCallback != null) {
                this.Text = new Paragraph(Anchor.Center, 1, textCallback, true);
                this.Text.Padding = this.Text.Padding.OrStyle(new Padding(1), 1);
                this.AddChild(this.Text);
            }
            if (tooltipTextCallback != null)
                this.Tooltip = this.AddTooltip(tooltipTextCallback);
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            var tex = this.Texture;
            var color = this.NormalColor;
            if (this.IsDisabled) {
                tex = this.DisabledTexture.OrDefault(tex);
                color = this.DisabledColor;
            } else if (this.IsMouseOver) {
                tex = this.HoveredTexture.OrDefault(tex);
                color = this.HoveredColor;
            } else if (this.IsSelected && this.Controls.IsAutoNavMode) {
                tex = this.SelectedTexture.OrDefault(tex);
                color = this.SelectedColor;
            }
            batch.Draw(tex, this.DisplayArea, color.Value * alpha, this.Scale);
            base.Draw(time, batch, alpha, context);
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = this.Texture.OrStyle(style.ButtonTexture);
            this.HoveredTexture = this.HoveredTexture.OrStyle(style.ButtonHoveredTexture);
            this.HoveredColor = this.HoveredColor.OrStyle(style.ButtonHoveredColor);
            this.DisabledTexture = this.DisabledTexture.OrStyle(style.ButtonDisabledTexture);
            this.DisabledColor = this.DisabledColor.OrStyle(style.ButtonDisabledColor);
        }

    }
}
