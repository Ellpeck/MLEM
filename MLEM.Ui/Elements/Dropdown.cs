using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A dropdown component to use inside of a <see cref="UiSystem"/>.
    /// A dropdown is a component that contains a hidden panel which is displayed upon pressing the dropdown button.
    /// </summary>
    public class Dropdown : Button {

        /// <summary>
        /// The panel that this dropdown contains. It will be displayed upon pressing the dropdown button.
        /// </summary>
        public readonly Panel Panel;

        /// <summary>
        /// This property stores whether the dropdown is currently opened or not
        /// </summary>
        public bool IsOpen {
            get => !this.Panel.IsHidden;
            set {
                this.Panel.IsHidden = !value;
                this.OnOpenedOrClosed?.Invoke(this);
            }
        }
        /// <summary>
        /// An event that is invoked when <see cref="IsOpen"/> changes
        /// </summary>
        public GenericCallback OnOpenedOrClosed;

        /// <summary>
        /// Creates a new dropdown with the given settings
        /// </summary>
        /// <param name="anchor">The dropdown's anchor</param>
        /// <param name="size">The dropdown button's size</param>
        /// <param name="text">The text displayed on the dropdown button</param>
        /// <param name="tooltipText">The text displayed as a tooltip when hovering over the dropdown button</param>
        public Dropdown(Anchor anchor, Vector2 size, string text = null, string tooltipText = null) : base(anchor, size, text, tooltipText) {
            this.Panel = this.AddChild(new Panel(Anchor.TopCenter, Vector2.Zero, Vector2.Zero, true) {
                IsHidden = true
            });
            this.OnAreaUpdated += e => {
                this.Panel.Size = new Vector2(e.Area.Width / e.Scale, 0);
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
                    return this.Panel.GetChildren().FirstOrDefault(c => c.CanBeSelected) ?? usualNext;
                return usualNext;
            };
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

    }
}
