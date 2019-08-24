using System;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Tooltip : Paragraph {

        public Vector2 MouseOffset = new Vector2(2, 3);

        public Tooltip(float width, string text, Element elementToHover = null) :
            base(Anchor.TopLeft, width, text) {
            this.AutoAdjustWidth = true;
            this.Padding = new Point(2);

            if (elementToHover != null) {
                elementToHover.OnMouseEnter += element => element.System.Add(element.GetType().Name + "Tooltip", this);
                elementToHover.OnMouseExit += element => element.System.Remove(element.GetType().Name + "Tooltip");
            }
        }

        public override void Update(GameTime time) {
            base.Update(time);

            var viewport = this.System.Viewport.Size;
            var offset = this.MousePos.ToVector2() / this.Scale + this.MouseOffset;
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

        public override void ForceUpdateArea() {
            if (this.Parent != null)
                throw new NotSupportedException($"A tooltip shouldn't be the child of another element ({this.Parent})");
            base.ForceUpdateArea();
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Background = style.TooltipBackground;
            this.BackgroundColor = style.TooltipBackgroundColor;
        }

    }
}