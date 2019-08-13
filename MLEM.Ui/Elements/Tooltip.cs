using System;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Tooltip : Paragraph {

        public Vector2 MouseOffset = new Vector2(2, 3);

        public Tooltip(float width, string text, IGenericFont font = null) :
            base(Anchor.TopLeft, width, text, false, font) {
            this.Padding = new Point(2);
        }

        public override void Update(GameTime time) {
            base.Update(time);
            this.PositionOffset = this.MousePos.ToVector2() / this.Scale + this.MouseOffset;
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