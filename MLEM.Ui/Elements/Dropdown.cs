using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A dropdown component to use inside of a <see cref="UiSystem"/>.
    /// A dropdown is a component that contains a hidden panel which is displayed upon pressing the dropdown button.
    /// </summary>
    public class Dropdown : Button {

        /// <summary>
        /// The panel that this dropdown contains. It will be displayed upon pressing the dropdown button.
        /// </summary>
        public Panel Panel { get; private set; }
        /// <summary>
        /// The <see cref="Image"/> that contains the currently active arrow texture for this dropdown, which is either <see cref="ClosedArrowTexture"/> or <see cref="OpenedArrowTexture"/>.
        /// </summary>
        public Image Arrow { get; private set; }
        /// <summary>
        /// This property stores whether the dropdown is currently opened or not
        /// </summary>
        public bool IsOpen {
            get => !this.Panel.IsHidden;
            set {
                this.Panel.IsHidden = !value;
                this.UpdateArrowStyle();
                this.OnOpenedOrClosed?.Invoke(this);
            }
        }
        /// <summary>
        /// An event that is invoked when <see cref="IsOpen"/> changes
        /// </summary>
        public GenericCallback OnOpenedOrClosed;

        /// <summary>
        /// A style property containing the <see cref="Padding"/> that should be passed to the <see cref="Arrow"/> child element.
        /// </summary>
        public StyleProp<Padding> ArrowPadding {
            get => this.arrowPadding;
            set {
                this.arrowPadding = value;
                this.UpdateArrowStyle();
            }
        }
        /// <summary>
        /// A style property containing the <see cref="TextureRegion"/> that should be displayed on this dropdown's <see cref="Arrow"/> when the dropdown is closed (<see cref="IsOpen"/> is <see langword="false"/>).
        /// </summary>
        public StyleProp<TextureRegion> ClosedArrowTexture {
            get => this.closedArrowTexture;
            set {
                this.closedArrowTexture = value;
                this.UpdateArrowStyle();
            }
        }
        /// <summary>
        /// A style property containing the <see cref="TextureRegion"/> that should be displayed on this dropdown's <see cref="Arrow"/> when the dropdown is open (<see cref="IsOpen"/> is <see langword="true"/>).
        /// </summary>
        public StyleProp<TextureRegion> OpenedArrowTexture {
            get => this.openedArrowTexture;
            set {
                this.openedArrowTexture = value;
                this.UpdateArrowStyle();
            }
        }

        private StyleProp<TextureRegion> openedArrowTexture;
        private StyleProp<TextureRegion> closedArrowTexture;
        private StyleProp<Padding> arrowPadding;

        /// <summary>
        /// Creates a new dropdown with the given settings and no text or tooltip.
        /// </summary>
        /// <param name="anchor">The dropdown's anchor</param>
        /// <param name="size">The dropdown button's size</param>
        /// <param name="panelHeight">The height of the <see cref="Panel"/>. If this is 0, the panel will be set to <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        /// <param name="scrollPanel">Whether this dropdown's <see cref="Panel"/> should automatically add a scroll bar to scroll towards elements that are beyond the area it covers.</param>
        /// <param name="autoHidePanelScrollbar">Whether this dropdown's <see cref="Panel"/>'s scroll bar should be hidden automatically if the panel does not contain enough children to allow for scrolling. This only has an effect if <paramref name="scrollPanel"/> is <see langword="true"/>.</param>
        public Dropdown(Anchor anchor, Vector2 size, float panelHeight = 0, bool scrollPanel = false, bool autoHidePanelScrollbar = true) : base(anchor, size) {
            this.Initialize(panelHeight, scrollPanel, autoHidePanelScrollbar);
        }

        /// <summary>
        /// Creates a new dropdown with the given settings
        /// </summary>
        /// <param name="anchor">The dropdown's anchor</param>
        /// <param name="size">The dropdown button's size</param>
        /// <param name="text">The text displayed on the dropdown button</param>
        /// <param name="tooltipText">The text displayed as a tooltip when hovering over the dropdown button</param>
        /// <param name="panelHeight">The height of the <see cref="Panel"/>. If this is 0, the panel will be set to <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        /// <param name="scrollPanel">Whether this dropdown's <see cref="Panel"/> should automatically add a scroll bar to scroll towards elements that are beyond the area it covers.</param>
        /// <param name="autoHidePanelScrollbar">Whether this dropdown's <see cref="Panel"/>'s scroll bar should be hidden automatically if the panel does not contain enough children to allow for scrolling. This only has an effect if <paramref name="scrollPanel"/> is <see langword="true"/>.</param>
        public Dropdown(Anchor anchor, Vector2 size, string text = null, string tooltipText = null, float panelHeight = 0, bool scrollPanel = false, bool autoHidePanelScrollbar = true) : base(anchor, size, text, tooltipText) {
            this.Initialize(panelHeight, scrollPanel, autoHidePanelScrollbar);
        }

        /// <summary>
        /// Creates a new dropdown with the given settings
        /// </summary>
        /// <param name="anchor">The dropdown's anchor</param>
        /// <param name="size">The dropdown button's size</param>
        /// <param name="textCallback">The text displayed on the dropdown button</param>
        /// <param name="tooltipTextCallback">The text displayed as a tooltip when hovering over the dropdown button</param>
        /// <param name="panelHeight">The height of the <see cref="Panel"/>. If this is 0, the panel will be set to <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        /// <param name="scrollPanel">Whether this dropdown's <see cref="Panel"/> should automatically add a scroll bar to scroll towards elements that are beyond the area it covers.</param>
        /// <param name="autoHidePanelScrollbar">Whether this dropdown's <see cref="Panel"/>'s scroll bar should be hidden automatically if the panel does not contain enough children to allow for scrolling. This only has an effect if <paramref name="scrollPanel"/> is <see langword="true"/>.</param>
        public Dropdown(Anchor anchor, Vector2 size, Paragraph.TextCallback textCallback = null, Paragraph.TextCallback tooltipTextCallback = null, float panelHeight = 0, bool scrollPanel = false, bool autoHidePanelScrollbar = true) : base(anchor, size, textCallback, tooltipTextCallback) {
            this.Initialize(panelHeight, scrollPanel, autoHidePanelScrollbar);
        }

        /// <summary>
        /// Adds an element to this dropdown's <see cref="Panel"/>
        /// </summary>
        /// <param name="element">The element to add</param>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Element.Children"/> list</param>
        public void AddElement(Element element, int index = -1) {
            this.Panel.AddChild(element, index);
            // Since the dropdown causes elements to be over each other,
            // usual gamepad code doesn't apply
            element.GetGamepadNextElement = (dir, usualNext) => {
                if (dir == Direction2.Left || dir == Direction2.Right)
                    return null;
                if (dir == Direction2.Up) {
                    var prev = element.GetOlderSibling(e => e.CanBeSelected);
                    return prev ?? this;
                } else if (dir == Direction2.Down) {
                    return element.GetSiblings(e => e.CanBeSelected && e.GetOlderSibling(s => s.CanBeSelected) == element).FirstOrDefault();
                }
                return usualNext;
            };
        }

        /// <summary>
        /// Adds a pressable <see cref="Paragraph"/> element to this dropdown's <see cref="Panel"/>
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="pressed">The resulting paragraph's <see cref="Element.OnPressed"/> event</param>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Element.Children"/> list</param>
        public Element AddElement(string text, GenericCallback pressed = null, int index = -1) {
            return this.AddElement(p => text, pressed, index);
        }

        /// <summary>
        /// Adds a pressable <see cref="Paragraph"/> element to this dropdown's <see cref="Panel"/>.
        /// By default, the paragraph's text color will change from <see cref="Color.White"/> to <see cref="Color.LightGray"/> when hovering over it.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="pressed">The resulting paragraph's <see cref="Element.OnPressed"/> event</param>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Element.Children"/> list</param>
        public Element AddElement(Paragraph.TextCallback text, GenericCallback pressed = null, int index = -1) {
            var paragraph = new Paragraph(Anchor.AutoLeft, 1, text) {
                CanBeMoused = true,
                CanBeSelected = true,
                PositionOffset = new Vector2(0, 1)
            };
            if (pressed != null)
                paragraph.OnPressed += pressed;
            paragraph.OnMouseEnter += e => paragraph.TextColor = Color.LightGray;
            paragraph.OnMouseExit += e => paragraph.TextColor = Color.White;
            this.AddElement(paragraph, index);
            return paragraph;
        }

        private void Initialize(float panelHeight, bool scrollPanel, bool autoHidePanelScrollbar) {
            this.Panel = this.AddChild(new Panel(Anchor.TopCenter, Vector2.Zero, panelHeight == 0, scrollPanel, autoHidePanelScrollbar) {
                IsHidden = true
            });
            this.Arrow = this.AddChild(new Image(Anchor.CenterRight, new Vector2(-1, 1), this.ClosedArrowTexture));
            this.UpdateArrowStyle();
            this.OnAreaUpdated += e => {
                this.Panel.Size = new Vector2(e.Area.Width / e.Scale, panelHeight);
                this.Panel.PositionOffset = new Vector2(0, e.Area.Height / e.Scale);
            };
            this.OnOpenedOrClosed += e => this.Priority = this.IsOpen ? 10000 : 0;
            this.OnPressed += e => {
                this.IsOpen = !this.IsOpen;
                // close other dropdowns in the same root when we open
                if (this.IsOpen) {
                    this.Root.Element.AndChildren(o => {
                        if (o != this && o is Dropdown d && d.IsOpen)
                            d.IsOpen = false;
                    });
                }
            };
            this.GetGamepadNextElement = (dir, usualNext) => {
                // Force navigate down to our first child if we're open
                if (this.IsOpen && dir == Direction2.Down)
                    return this.Panel.Children.FirstOrDefault(c => c.CanBeSelected) ?? usualNext;
                return usualNext;
            };
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.ArrowPadding = this.ArrowPadding.OrStyle(style.DropdownArrowPadding);
            this.ClosedArrowTexture = this.ClosedArrowTexture.OrStyle(style.DropdownClosedArrowTexture);
            this.OpenedArrowTexture = this.OpenedArrowTexture.OrStyle(style.DropdownOpenedArrowTexture);
            this.UpdateArrowStyle();
        }

        private void UpdateArrowStyle() {
            this.Arrow.Padding = this.Arrow.Padding.OrStyle(this.ArrowPadding, 1);
            this.Arrow.Texture = this.IsOpen ? this.OpenedArrowTexture : this.ClosedArrowTexture;
        }

    }
}
