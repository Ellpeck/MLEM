using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Misc;
using MLEM.Sound;
using MLEM.Textures;
using MLEM.Ui.Elements;

namespace MLEM.Ui.Style {
    /// <summary>
    /// The style settings for a <see cref="UiSystem"/>.
    /// Each <see cref="Element"/> uses these style settings by default, however you can also change these settings per element using the elements' individual style settings.
    /// Additional styles for built-in or custom element types can easily be added using <see cref="AddCustomStyle{T}"/>.
    /// </summary>
    public class UiStyle : GenericDataHolder {

        /// <summary>
        /// The texture that is rendered on top of the <see cref="UiControls.SelectedElement"/>
        /// </summary>
        public NinePatch SelectionIndicator;
        /// <summary>
        /// A <see cref="UiAnimation"/> that is played when the mouse enters an element.
        /// </summary>
        public UiAnimation MouseEnterAnimation;
        /// <summary>
        /// A <see cref="UiAnimation"/> that is played when the mouse exists an element.
        /// </summary>
        public UiAnimation MouseExitAnimation;
        /// <summary>
        /// The texture that the <see cref="Button"/> element uses
        /// </summary>
        public NinePatch ButtonTexture;
        /// <summary>
        /// The color that the <see cref="Button"/> element renders
        /// </summary>
        public Color ButtonColor = Color.White;
        /// <summary>
        /// The texture that the <see cref="Button"/> element uses when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// Note that, if you just want to change the button's color when hovered, use <see cref="ButtonHoveredColor"/>.
        /// </summary>
        public NinePatch ButtonHoveredTexture;
        /// <summary>
        /// The color that the <see cref="Button"/> element renders with when it is moused over (<see cref="Element.IsMouseOver"/>)
        /// </summary>
        public Color ButtonHoveredColor = Color.LightGray;
        /// <summary>
        /// The texture that the <see cref="Button"/> element uses when it <see cref="Button.IsDisabled"/>
        /// </summary>
        public NinePatch ButtonDisabledTexture;
        /// <summary>
        /// The color that the <see cref="Button"/> element uses when it <see cref="Button.IsDisabled"/>
        /// </summary>
        public Color ButtonDisabledColor = Color.Gray;
        /// <summary>
        /// The texture that the <see cref="Panel"/> element uses
        /// </summary>
        public NinePatch PanelTexture;
        /// <summary>
        /// The color that the <see cref="Panel"/> element draws with.
        /// </summary>
        public Color PanelColor = Color.White;
        /// <summary>
        /// The <see cref="Element.ChildPadding"/> to apply to a <see cref="Panel"/> by default
        /// </summary>
        public Padding PanelChildPadding = new Vector2(5);
        /// <summary>
        /// The amount that a <see cref="Panel"/>'s scrollable area is moved per single movement of the scroll wheel
        /// </summary>
        public float PanelStepPerScroll = 10;
        /// <summary>
        /// The size of the scroller of a <see cref="Panel"/>'s scroll bar
        /// </summary>
        public Vector2 PanelScrollerSize = new Vector2(5, 10);
        /// <summary>
        /// The amount of pixels of room there should be between a <see cref="Panel"/>'s scroll bar and the rest of its content
        /// </summary>
        public float PanelScrollBarOffset = 1;
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
        public Color TextFieldHoveredColor = Color.LightGray;
        /// <summary>
        /// The x position that a <see cref="TextField"/>'s text should start rendering at, based on the x position of the text field
        /// </summary>
        public float TextFieldTextOffsetX = 4;
        /// <summary>
        /// The width that a <see cref="TextField"/>'s caret should render with
        /// </summary>
        public float TextFieldCaretWidth = 0.5F;
        /// <summary>
        /// The background texture that the <see cref="ScrollBar"/> element uses
        /// </summary>
        public NinePatch ScrollBarBackground;
        /// <summary>
        /// The texture that the scroll indicator of the <see cref="ScrollBar"/> element uses
        /// </summary>
        public NinePatch ScrollBarScrollerTexture;
        /// <summary>
        /// Whether or not a <see cref="ScrollBar"/> should use smooth scrolling
        /// </summary>
        public bool ScrollBarSmoothScrolling;
        /// <summary>
        /// The factor with which a <see cref="ScrollBar"/>'s smooth scrolling happens
        /// </summary>
        public float ScrollBarSmoothScrollFactor = 0.75F;
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
        public Color CheckboxHoveredColor = Color.LightGray;
        /// <summary>
        /// The texture that the <see cref="Checkbox"/> element uses when it <see cref="Checkbox.IsDisabled"/>.
        /// </summary>
        public NinePatch CheckboxDisabledTexture;
        /// <summary>
        /// The color that the <see cref="Checkbox"/> element uses when it <see cref="Checkbox.IsDisabled"/>.
        /// </summary>
        public Color CheckboxDisabledColor = Color.Gray;
        /// <summary>
        /// The texture that the <see cref="Checkbox"/> element renders on top of its regular texture when it is <see cref="Checkbox.Checked"/>
        /// </summary>
        public TextureRegion CheckboxCheckmark;
        /// <summary>
        /// The width of the space between a <see cref="Checkbox"/> and its <see cref="Checkbox.Label"/>
        /// </summary>
        public float CheckboxTextOffsetX = 2;
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
        public Color RadioHoveredColor = Color.LightGray;
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
        public Vector2 TooltipOffset = new Vector2(8, 16);
        /// <summary>
        /// The offset of the <see cref="Tooltip"/> element's top center coordinate from the bottom center of the element snapped to when <see cref="Tooltip.DisplayInAutoNavMode"/> is true.
        /// </summary>
        public Vector2 TooltipAutoNavOffset = new Vector2(0, 8);
        /// <summary>
        /// The color that the text of a <see cref="Tooltip"/> should have
        /// </summary>
        public Color TooltipTextColor = Color.White;
        /// <summary>
        /// The amount of time that the mouse has to be over an element with a <see cref="Tooltip"/> for the tooltip to appear
        /// </summary>
        public TimeSpan TooltipDelay = TimeSpan.Zero;
        /// <summary>
        /// The width of a <see cref="Tooltip"/>'s default text <see cref="Paragraph"/>
        /// </summary>
        public float TooltipTextWidth = 50;
        /// <summary>
        /// The <see cref="Element.ChildPadding"/> to apply to a <see cref="Tooltip"/> by default
        /// </summary>
        public Padding TooltipChildPadding = new Vector2(2);
        /// <summary>
        /// The texture that the <see cref="ProgressBar"/> element uses for its background
        /// </summary>
        public NinePatch ProgressBarTexture;
        /// <summary>
        /// The color that the <see cref="ProgressBar"/> element renders with
        /// </summary>
        public Color ProgressBarColor = Color.White;
        /// <summary>
        /// The padding that the <see cref="ProgressBar"/> uses for its progress texture (<see cref="ProgressBarProgressTexture"/>)
        /// </summary>
        public Vector2 ProgressBarProgressPadding = new Vector2(1);
        /// <summary>
        /// The texture that the <see cref="ProgressBar"/> uses for displaying its progress
        /// </summary>
        public NinePatch ProgressBarProgressTexture;
        /// <summary>
        /// The color that the <see cref="ProgressBar"/> renders its progress texture with
        /// </summary>
        public Color ProgressBarProgressColor = Color.Red;
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
        /// The <see cref="TextAlignment"/> that a <see cref="Paragraph"/> should use by default.
        /// </summary>
        public TextAlignment TextAlignment;
        /// <summary>
        /// The <see cref="SoundEffectInfo"/> that should be played when an element's <see cref="Element.OnPressed"/> and <see cref="Element.OnSecondaryPressed"/> events are called.
        /// Note that this sound is only played if the callbacks have any subscribers.
        /// </summary>
        public SoundEffectInfo ActionSound;
        /// <summary>
        /// The color that a <see cref="Paragraph"/>'s <see cref="Paragraph.Link"/> codes should have.
        /// This value is passed to <see cref="LinkCode"/>.
        /// </summary>
        public Color? LinkColor = Color.CornflowerBlue;
        /// <summary>
        /// A set of additional fonts that can be used for the <c>&lt;f FontName&gt;</c> formatting code
        /// </summary>
        public Dictionary<string, GenericFont> AdditionalFonts = new Dictionary<string, GenericFont>();

        private readonly Dictionary<Type, Action<Element>> elementStyles = new Dictionary<Type, Action<Element>>();

        /// <summary>
        /// Creates a new set of style settings with the default values.
        /// </summary>
        public UiStyle() {}

        /// <summary>
        /// Creates a new set of style settings with values inherited from the given <paramref name="original"/> style settings.
        /// </summary>
        /// <param name="original">The original style settings, to copy into the new instance.</param>
        public UiStyle(UiStyle original) {
            this.SelectionIndicator = original.SelectionIndicator;
            this.MouseEnterAnimation = original.MouseEnterAnimation;
            this.MouseExitAnimation = original.MouseExitAnimation;
            this.ButtonTexture = original.ButtonTexture;
            this.ButtonColor = original.ButtonColor;
            this.ButtonHoveredTexture = original.ButtonHoveredTexture;
            this.ButtonHoveredColor = original.ButtonHoveredColor;
            this.ButtonDisabledTexture = original.ButtonDisabledTexture;
            this.ButtonDisabledColor = original.ButtonDisabledColor;
            this.PanelTexture = original.PanelTexture;
            this.PanelColor = original.PanelColor;
            this.PanelChildPadding = original.PanelChildPadding;
            this.PanelStepPerScroll = original.PanelStepPerScroll;
            this.PanelScrollerSize = original.PanelScrollerSize;
            this.PanelScrollBarOffset = original.PanelScrollBarOffset;
            this.TextFieldTexture = original.TextFieldTexture;
            this.TextFieldHoveredTexture = original.TextFieldHoveredTexture;
            this.TextFieldHoveredColor = original.TextFieldHoveredColor;
            this.TextFieldTextOffsetX = original.TextFieldTextOffsetX;
            this.TextFieldCaretWidth = original.TextFieldCaretWidth;
            this.ScrollBarBackground = original.ScrollBarBackground;
            this.ScrollBarScrollerTexture = original.ScrollBarScrollerTexture;
            this.ScrollBarSmoothScrolling = original.ScrollBarSmoothScrolling;
            this.ScrollBarSmoothScrollFactor = original.ScrollBarSmoothScrollFactor;
            this.CheckboxTexture = original.CheckboxTexture;
            this.CheckboxHoveredTexture = original.CheckboxHoveredTexture;
            this.CheckboxHoveredColor = original.CheckboxHoveredColor;
            this.CheckboxDisabledTexture = original.CheckboxDisabledTexture;
            this.CheckboxDisabledColor = original.CheckboxDisabledColor;
            this.CheckboxCheckmark = original.CheckboxCheckmark;
            this.CheckboxTextOffsetX = original.CheckboxTextOffsetX;
            this.RadioTexture = original.RadioTexture;
            this.RadioHoveredTexture = original.RadioHoveredTexture;
            this.RadioHoveredColor = original.RadioHoveredColor;
            this.RadioCheckmark = original.RadioCheckmark;
            this.TooltipBackground = original.TooltipBackground;
            this.TooltipOffset = original.TooltipOffset;
            this.TooltipAutoNavOffset = original.TooltipAutoNavOffset;
            this.TooltipTextColor = original.TooltipTextColor;
            this.TooltipDelay = original.TooltipDelay;
            this.TooltipTextWidth = original.TooltipTextWidth;
            this.TooltipChildPadding = original.TooltipChildPadding;
            this.ProgressBarTexture = original.ProgressBarTexture;
            this.ProgressBarColor = original.ProgressBarColor;
            this.ProgressBarProgressPadding = original.ProgressBarProgressPadding;
            this.ProgressBarProgressTexture = original.ProgressBarProgressTexture;
            this.ProgressBarProgressColor = original.ProgressBarProgressColor;
            this.Font = original.Font;
            this.TextScale = original.TextScale;
            this.TextColor = original.TextColor;
            this.TextAlignment = original.TextAlignment;
            this.ActionSound = original.ActionSound;
            this.LinkColor = original.LinkColor;
            this.AdditionalFonts = new Dictionary<string, GenericFont>(original.AdditionalFonts);
            this.elementStyles = new Dictionary<Type, Action<Element>>(original.elementStyles);
        }

        /// <summary>
        /// Adds an action to the given <see cref="Element"/> type <typeparamref name="T"/> that allows applying any kind of custom styling or behavior to it.
        /// Custom styles added in this manner can be applied to an element using <see cref="ApplyCustomStyle"/>.
        /// </summary>
        /// <param name="style">The style action to add.</param>
        /// <param name="add">Whether the <paramref name="style"/> function should be added to the existing style settings rather than replacing them.</param>
        /// <typeparam name="T">The <see cref="Element"/> type that the <paramref name="style"/> should apply to.</typeparam>
        public void AddCustomStyle<T>(Action<T> style, bool add = false) where T : Element {
            if (add && this.elementStyles.ContainsKey(typeof(T))) {
                this.elementStyles[typeof(T)] += Action;
            } else {
                this.elementStyles[typeof(T)] = Action;
            }

            void Action(Element e) {
                style.Invoke((T) e);
            }
        }

        /// <summary>
        /// Applies a set of custom styling actions to the given <paramref name="element"/> which were added through <see cref="AddCustomStyle{T}"/>.
        /// This method is automatically invoked in <see cref="Element.InitStyle"/>.
        /// </summary>
        /// <param name="element">The element to apply custom styling to.</param>
        /// <returns>Whether any custom styling exists for the given <paramref name="element"/>.</returns>
        public bool ApplyCustomStyle(Element element) {
            if (this.elementStyles.TryGetValue(element.GetType(), out var style)) {
                style?.Invoke(element);
                return true;
            }
            return false;
        }

    }
}
