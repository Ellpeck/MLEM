using Microsoft.Xna.Framework;

namespace MLEM.Ui.Elements {
    public class Dropdown : Button {

        public readonly Panel Panel;
        public bool IsOpen {
            get => !this.Panel.IsHidden;
            set => this.Panel.IsHidden = !value;
        }

        public Dropdown(Anchor anchor, Vector2 size, string text = null, string tooltipText = null, float tooltipWidth = 50) : base(anchor, size, text, tooltipText, tooltipWidth) {
            this.Panel = this.AddChild(new Panel(Anchor.TopCenter, size, Vector2.Zero, true) {
                IsHidden = true
            });
            this.OnAreaUpdated += e => this.Panel.PositionOffset = new Vector2(0, e.Area.Height / this.Scale);
            this.Priority = 10000;
            this.OnPressed += e => this.IsOpen = !this.IsOpen;
        }

        public void AddElement(Element element) {
            this.Panel.AddChild(element);
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