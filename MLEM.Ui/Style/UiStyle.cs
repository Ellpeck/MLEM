using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Elements;

namespace MLEM.Ui.Style {
    /// <summary>
    /// The style settings for a <see cref="UiSystem"/>.
    /// Each <see cref="Element"/> uses these style settings by default, however you can also change these settings per element using the elements' individual style settings.
    /// Note that this class is a <see cref="GenericDataHolder"/>, meaning additional styles for custom components can easily be added using <see cref="GenericDataHolder.SetData"/>
    /// </summary>
    public class UiStyle : GenericDataHolder {

        /// <summary>
        /// The texture that is rendered on top of the <see cref="UiControls.SelectedElement"/>
        /// </summary>
        public NinePatch SelectionIndicator;
        /// <summary>
        /// The texture that the <see cref="Button"/> element uses
        /// </summary>
        public NinePatch ButtonTexture;
        /// <summary>
        /// The texture that the <see cref="Button"/> element uses when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// Note that, if you just want to change the button's color when hovered, use <see cref="ButtonHoveredColor"/>.
        /// </summary>
        public NinePatch ButtonHoveredTexture;
        /// <summary>
        /// The color that the <see cref="Button"/> element renders with when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public Color ButtonHoveredColor;
        /// <summary>
        /// The texture that the <see cref="Button"/> element uses when it <see cref="Button.IsDisabled"/>
        /// </summary>
        public NinePatch ButtonDisabledTexture;
        /// <summary>
        /// The color that the <see cref="Button"/> element uses when it <see cref="Button.IsDisabled"/>
        /// </summary>
        public Color ButtonDisabledColor;
        /// <summary>
        /// The texture that the <see cref="Panel"/> element uses
        /// </summary>
        public NinePatch PanelTexture;
        /// <summary>
        /// The texture that the <see cref="TextField"/> element uses
        /// </summary>
        public NinePatch TextFieldTexture;
        /// <summary>
        /// The texture that the <see cref="TextField"/> element uses when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public NinePatch TextFieldHoveredTexture;
        /// <summary>
        /// The color that the <see cref="TextField"/> renders with when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public Color TextFieldHoveredColor;
        /// <summary>
        /// The background texture that the <see cref="ScrollBar"/> element uses
        /// </summary>
        public NinePatch ScrollBarBackground;
        /// <summary>
        /// The texture that the scroll indicator of the <see cref="ScrollBar"/> element uses
        /// </summary>
        public NinePatch ScrollBarScrollerTexture;
        /// <summary>
        /// The texture that the <see cref="Checkbox"/> element uses
        /// </summary>
        public NinePatch CheckboxTexture;
        /// <summary>
        /// The texture that the <see cref="Checkbox"/> element uses when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public NinePatch CheckboxHoveredTexture;
        /// <summary>
        /// The color that the <see cref="Checkbox"/> element renders with when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public Color CheckboxHoveredColor;
        /// <summary>
        /// The texture that the <see cref="Checkbox"/> element renders on top of its regular texture when it is <see cref="Checkbox.Checked"/>
        /// </summary>
        public TextureRegion CheckboxCheckmark;
        /// <summary>
        /// The texture that the <see cref="RadioButton"/> element uses
        /// </summary>
        public NinePatch RadioTexture;
        /// <summary>
        /// The texture that the <see cref="RadioButton"/> element uses when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public NinePatch RadioHoveredTexture;
        /// <summary>
        /// The color that the <see cref="RadioButton"/> element renders with when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public Color RadioHoveredColor;
        /// <summary>
        /// The texture that the <see cref="RadioButton"/> renders on top of its regular texture when it is <see cref="Checkbox.Checked"/>
        /// </summary>
        public TextureRegion RadioCheckmark;
        /// <summary>
        /// The texture that the <see cref="Tooltip"/> uses for its background
        /// </summary>
        public NinePatch TooltipBackground;
        /// <summary>
        /// The offset of the <see cref="Tooltip"/> element's top left corner from the mouse position
        /// </summary>
        public Vector2 TooltipOffset;
        /// <summary>
        /// The color that the text of a <see cref="Tooltip"/> should have
        /// </summary>
        public Color TooltipTextColor = Color.White;
        /// <summary>
        /// The texture that the <see cref="ProgressBar"/> element uses for its background
        /// </summary>
        public NinePatch ProgressBarTexture;
        /// <summary>
        /// The color that the <see cref="ProgressBar"/> element renders with
        /// </summary>
        public Color ProgressBarColor;
        /// <summary>
        /// The padding that the <see cref="ProgressBar"/> uses for its progress texture (<see cref="ProgressBarProgressTexture"/>)
        /// </summary>
        public Vector2 ProgressBarProgressPadding;
        /// <summary>
        /// The texture that the <see cref="ProgressBar"/> uses for displaying its progress
        /// </summary>
        public NinePatch ProgressBarProgressTexture;
        /// <summary>
        /// The color that the <see cref="ProgressBar"/> renders its progress texture with
        /// </summary>
        public Color ProgressBarProgressColor;
        /// <summary>
        /// The font that <see cref="Paragraph"/> and other elements should use for rendering.
        /// Note that, to specify a bold and italic font for <see cref="TextFormatter"/>, you should use <see cref="GenericFont.Bold"/> and <see cref="GenericFont.Italic"/>.
        /// </summary>
        public GenericFont Font;
        /// <summary>
        /// The scale that text should be rendered with in <see cref="Paragraph"/> and other elements
        /// </summary>
        public float TextScale = 1;
        /// <summary>
        /// The color that the text of a <see cref="Paragraph"/> should have
        /// </summary>
        public Color TextColor = Color.White;
        /// <summary>
        /// The <see cref="SoundEffectInfo"/> that should be played when an element's <see cref="Element.OnPressed"/> and <see cref="Element.OnSecondaryPressed"/> events are called.
        /// Note that this sound is only played if the callbacks have any subscribers.
        /// </summary>
        public SoundEffectInfo ActionSound;

    }
}