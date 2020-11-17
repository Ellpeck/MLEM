using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A tooltip element for use inside of a <see cref="UiSystem"/>.
    /// A tooltip is a <see cref="Panel"/> with a custom cursor that always follows the position of the mouse.
    /// Tooltips can easily be configured to be hooked onto an element, causing them to appear when it is moused, and disappear when it is not moused anymore.
    /// </summary>
    public class Tooltip : Panel {

        /// <summary>
        /// The offset that this tooltip's top left corner should have from the mouse position
        /// </summary>
        public StyleProp<Vector2> MouseOffset;
        /// <summary>
        /// The paragraph of text that this tooltip displays
        /// </summary>
        public Paragraph Paragraph;

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="width">The width of the tooltip</param>
        /// <param name="text">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(float width, string text = null, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            if (text != null) {
                this.Paragraph = this.AddChild(new Paragraph(Anchor.TopLeft, width, text));
                this.Paragraph.AutoAdjustWidth = true;
            }
            this.SetWidthBasedOnChildren = true;
            this.SetHeightBasedOnChildren = true;
            this.ChildPadding = new Vector2(2);
            this.CanBeMoused = false;

            if (elementToHover != null) {
                elementToHover.OnMouseEnter += element => {
                    // only display the tooltip if there is anything in it
                    if (this.Children.All(c => c.IsHidden))
                        return;
                    element.System.Add(element.GetType().Name + "Tooltip", this);
                    this.SnapPositionToMouse();
                };
                elementToHover.OnMouseExit += element => {
                    if (this.System != null)
                        this.System.Remove(this.Root.Name);
                };
            }
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);
            this.SnapPositionToMouse();
        }

        /// <inheritdoc />
        public override void ForceUpdateArea() {
            if (this.Parent != null)
                throw new NotSupportedException($"A tooltip shouldn't be the child of another element ({this.Parent})");
            base.ForceUpdateArea();
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture.SetFromStyle(style.TooltipBackground);
            this.MouseOffset.SetFromStyle(style.TooltipOffset);
            // we can't set from style here since it's a different element
            this.Paragraph?.TextColor.Set(style.TooltipTextColor);
        }

        /// <summary>
        /// Causes this tooltip's position to be snapped to the mouse position.
        /// </summary>
        public void SnapPositionToMouse() {
            var viewport = this.System.Viewport.Size;
            var offset = (this.Input.MousePosition.ToVector2() + this.MouseOffset.Value) / this.Scale;
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