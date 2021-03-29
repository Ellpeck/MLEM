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
        /// The amount of time that the mouse has to be over an element before it appears
        /// </summary>
        public StyleProp<TimeSpan> Delay;
        /// <summary>
        /// The paragraph of text that this tooltip displays
        /// </summary>
        public Paragraph Paragraph;

        private TimeSpan delayCountdown;

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="width">The width of the tooltip</param>
        /// <param name="text">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(float width, string text = null, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            if (text != null)
                this.Paragraph = this.AddChild(new Paragraph(Anchor.TopLeft, width, text));
            this.Init(elementToHover);
        }

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="width">The width of the tooltip</param>
        /// <param name="textCallback">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(float width, Paragraph.TextCallback textCallback, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            this.Paragraph = this.AddChild(new Paragraph(Anchor.TopLeft, width, textCallback));
            this.Init(elementToHover);
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);
            this.SnapPositionToMouse();

            if (this.IsHidden && this.delayCountdown > TimeSpan.Zero) {
                this.delayCountdown -= time.ElapsedGameTime;
                if (this.delayCountdown <= TimeSpan.Zero)
                    this.IsHidden = false;
            }
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
            this.Delay.SetFromStyle(style.TooltipDelay);
            // we can't set from style here since it's a different element
            this.Paragraph?.TextColor.Set(style.TooltipTextColor);
        }

        /// <summary>
        /// Causes this tooltip's position to be snapped to the mouse position.
        /// </summary>
        public void SnapPositionToMouse() {
            var viewport = this.System.Viewport;
            var offset = (this.Input.MousePosition.ToVector2() + this.MouseOffset.Value) / this.Scale;
            if (offset.X < viewport.X)
                offset.X = viewport.X;
            if (offset.Y < viewport.Y)
                offset.Y = viewport.Y;
            if (offset.X * this.Scale + this.Area.Width >= viewport.Right)
                offset.X = (viewport.Right - this.Area.Width) / this.Scale;
            if (offset.Y * this.Scale + this.Area.Height >= viewport.Bottom)
                offset.Y = (viewport.Bottom - this.Area.Height) / this.Scale;
            this.PositionOffset = offset;
        }

        /// <summary>
        /// Adds this tooltip to the given <see cref="UiSystem"/> and either displays it directly or starts the <see cref="Delay"/> timer.
        /// </summary>
        /// <param name="system">The system to add this tooltip to</param>
        /// <param name="name">The name that this tooltip should use</param>
        public void Display(UiSystem system, string name) {
            system.Add(name, this);
            if (this.Delay <= TimeSpan.Zero) {
                this.IsHidden = false;
                this.SnapPositionToMouse();
            } else {
                this.IsHidden = true;
                this.delayCountdown = this.Delay;
            }
        }

        /// <summary>
        /// Removes this tooltip from its <see cref="UiSystem"/> and resets the <see cref="Delay"/> timer, if there is one.
        /// </summary>
        public void Remove() {
            this.delayCountdown = TimeSpan.Zero;
            if (this.System != null)
                this.System.Remove(this.Root.Name);
        }

        /// <summary>
        /// Adds this tooltip instance to the given <see cref="Element"/>, making it display when it is moused over
        /// </summary>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public void AddToElement(Element elementToHover) {
            elementToHover.OnMouseEnter += element => {
                // only display the tooltip if there is anything in it
                if (this.Children.Any(c => !c.IsHidden))
                    this.Display(element.System, element.GetType().Name + "Tooltip");
            };
            elementToHover.OnMouseExit += element => this.Remove();
        }

        private void Init(Element elementToHover) {
            if (this.Paragraph != null)
                this.Paragraph.AutoAdjustWidth = true;

            this.SetWidthBasedOnChildren = true;
            this.SetHeightBasedOnChildren = true;
            this.ChildPadding = new Vector2(2);
            this.CanBeMoused = false;

            if (elementToHover != null)
                this.AddToElement(elementToHover);
        }

    }
}