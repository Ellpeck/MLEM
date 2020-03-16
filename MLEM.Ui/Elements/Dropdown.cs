using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Misc;

namespace MLEM.Ui.Elements {
    public class Dropdown : Button {

        public readonly Panel Panel;
        public bool IsOpen {
            get => !this.Panel.IsHidden;
            set {
                this.Panel.IsHidden = !value;
                this.OnOpenedOrClosed?.Invoke(this);
            }
        }
        public GenericCallback OnOpenedOrClosed;

        public Dropdown(Anchor anchor, Vector2 size, string text = null, string tooltipText = null, float tooltipWidth = 50) : base(anchor, size, text, tooltipText, tooltipWidth) {
            this.Panel = this.AddChild(new Panel(Anchor.TopCenter, size, Vector2.Zero, true) {
                IsHidden = true
            });
            this.OnAreaUpdated += e => this.Panel.PositionOffset = new Vector2(0, e.Area.Height / this.Scale);
            this.OnOpenedOrClosed += e => this.Priority = this.IsOpen ? 10000 : 0;
            this.OnPressed += e => this.IsOpen = !this.IsOpen;
        }

        public void AddElement(Element element) {
            this.Panel.AddChild(element);
            // Since the dropdown causes elements to be over each other,
            // usual gamepad code doesn't apply
            element.GetGamepadNextElement = (dir, usualNext) => {
                if (dir == Direction2.Up) {
                    var prev = element.GetOlderSibling();
                    if (prev != null)
                        return prev;
                } else if (dir == Direction2.Down) {
                    var next = element.GetSiblings(e => e.GetOlderSibling() == element).FirstOrDefault();
                    if (next != null)
                        return next;
                }
                return usualNext;
            };
        }

        public void AddElement(string text, GenericCallback pressed = null) {
            this.AddElement(p => text, pressed);
        }

        public void AddElement(Paragraph.TextCallback text, GenericCallback pressed = null) {
            var paragraph = new Paragraph(Anchor.AutoLeft, 1, text) {
                CanBeMoused = true,
                CanBeSelected = true,
                PositionOffset = new Vector2(0, 1)
            };
            if (pressed != null)
                paragraph.OnPressed += pressed;
            paragraph.OnMouseEnter += e => paragraph.TextColor = Color.LightGray;
            paragraph.OnMouseExit += e => paragraph.TextColor = Color.White;
            this.AddElement(paragraph);
        }

    }
}