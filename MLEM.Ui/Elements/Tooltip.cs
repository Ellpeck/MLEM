using System;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Tooltip : Group {

        public Vector2 MouseOffset = new Vector2(2, 3);
        public Paragraph Paragraph;

        public Tooltip(float width, string text = null, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One) {
            if (text != null) {
                this.Paragraph = this.AddChild(new Paragraph(Anchor.TopLeft, width, text));
                this.Paragraph.AutoAdjustWidth = true;
                this.Paragraph.Padding = new Vector2(2);
                this.SetWidthBasedOnChildren = true;
            }
            this.CanBeMoused = false;

            if (elementToHover != null) {
                elementToHover.OnMouseEnter += element => {
                    element.System.Add(element.GetType().Name + "Tooltip", this);
                    this.SnapPositionToMouse();
                };
                elementToHover.OnMouseExit += element => {
                    if (this.System != null)
                        this.System.Remove(element.GetType().Name + "Tooltip");
                };
            }
        }

        public override void Update(GameTime time) {
            base.Update(time);
            this.SnapPositionToMouse();
        }

        public override void ForceUpdateArea() {
            if (this.Parent != null)
                throw new NotSupportedException($"A tooltip shouldn't be the child of another element ({this.Parent})");
            base.ForceUpdateArea();
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            if (this.Paragraph != null) {
                this.Paragraph.Background.SetFromStyle(style.TooltipBackground);
                this.Paragraph.BackgroundColor.SetFromStyle(style.TooltipBackgroundColor);
            }
        }

        public void SnapPositionToMouse() {
            var viewport = this.System.Viewport.Size;
            var offset = this.Input.MousePosition.ToVector2() / this.Scale + this.MouseOffset;
            if (offset.X < 0)
                offset.X = 0;
            if (offset.Y < 0)
                offset.Y = 0;
            if (offset.X * this.Scale + this.Area.Width >= viewport.X)
                offset.X = (viewport.X - this.Area.Width) / this.Scale;
            if (offset.Y * this.Scale + this.Area.Height >= viewport.Y)
                offset.Y = (viewport.Y - this.Area.Height) / this.Scale;
            this.PositionOffset = offset;
        }
    }
}