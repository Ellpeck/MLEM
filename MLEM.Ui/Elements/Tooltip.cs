using System;
using Microsoft.Xna.Framework;
using MLEM.Input;
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
        /// The offset that this tooltip's top center coordinate should have from the bottom center of the element snapped to when <see cref="DisplayInAutoNavMode"/> is true.
        /// </summary>
        public StyleProp<Vector2> AutoNavOffset;
        /// <summary>
        /// The amount of time that the mouse has to be over an element before it appears
        /// </summary>
        public StyleProp<TimeSpan> Delay;
        /// <summary>
        /// The paragraph of text that this tooltip displays
        /// </summary>
        public Paragraph Paragraph;
        /// <summary>
        /// Determines whether this tooltip should display when <see cref="UiControls.IsAutoNavMode"/> is true, which is when the UI is being controlled using a keyboard or gamepad.
        /// If this tooltip is displayed in auto-nav mode, it will display below the selected element with the <see cref="AutoNavOffset"/> applied.
        /// </summary>
        public bool DisplayInAutoNavMode;
        /// <summary>
        /// The position that this tooltip should be following (or snapped to) instead of the <see cref="InputHandler.ViewportMousePosition"/>.
        /// If this value is unset, <see cref="InputHandler.ViewportMousePosition"/> will be used as the snap position.
        /// Note that <see cref="MouseOffset"/> is still applied with this value set.
        /// </summary>
        public virtual Vector2? SnapPosition { get; set; }

        /// <inheritdoc />
        public override bool IsHidden => this.autoHidden || base.IsHidden;

        private TimeSpan delayCountdown;
        private bool autoHidden;
        private Element snapElement;

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="text">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(string text = null, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            if (text != null)
                this.Paragraph = this.AddChild(new Paragraph(Anchor.TopLeft, 0, text));
            this.Init(elementToHover);
        }

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="textCallback">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(Paragraph.TextCallback textCallback, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            this.Paragraph = this.AddChild(new Paragraph(Anchor.TopLeft, 0, textCallback));
            this.Init(elementToHover);
        }

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);
            this.SnapPositionToMouse();

            if (this.delayCountdown > TimeSpan.Zero) {
                this.delayCountdown -= time.ElapsedGameTime;
                if (this.delayCountdown <= TimeSpan.Zero) {
                    this.IsHidden = false;
                    this.UpdateAutoHidden();
                }
            } else {
                this.UpdateAutoHidden();
            }
        }

        /// <inheritdoc />
        public override void ForceUpdateArea() {
            if (this.Parent != null)
                throw new NotSupportedException($"A tooltip shouldn't be the child of another element ({this.Parent})");
            base.ForceUpdateArea();
            this.SnapPositionToMouse();
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = this.Texture.OrStyle(style.TooltipBackground);
            this.MouseOffset = this.MouseOffset.OrStyle(style.TooltipOffset);
            this.AutoNavOffset = this.AutoNavOffset.OrStyle(style.TooltipAutoNavOffset);
            this.Delay = this.Delay.OrStyle(style.TooltipDelay);
            this.ChildPadding = this.ChildPadding.OrStyle(style.TooltipChildPadding);
            if (this.Paragraph != null) {
                this.Paragraph.TextColor = this.Paragraph.TextColor.OrStyle(style.TooltipTextColor, 1);
                this.Paragraph.Size = new Vector2(style.TooltipTextWidth, 0);
            }
        }

        /// <summary>
        /// Causes this tooltip's position to be snapped to the mouse position, or the <see cref="snapElement"/> if <see cref="DisplayInAutoNavMode"/> is true, or the <see cref="SnapPosition"/> if set.
        /// </summary>
        public void SnapPositionToMouse() {
            Vector2 snapPosition;
            if (this.snapElement != null) {
                // center our snap position below the snap element
                snapPosition = new Vector2(this.snapElement.DisplayArea.Center.X, this.snapElement.DisplayArea.Bottom) + this.AutoNavOffset;
                snapPosition.X -= this.DisplayArea.Width / 2F;
            } else {
                snapPosition = (this.SnapPosition ?? this.Input.ViewportMousePosition.ToVector2()) + this.MouseOffset.Value;
            }

            var viewport = this.System.Viewport;
            var offset = snapPosition / this.Scale;
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
        /// <returns>Whether this tooltip was successfully added, which is not the case if it is already being displayed currently.</returns>
        public bool Display(UiSystem system, string name) {
            if (system.Add(name, this) == null)
                return false;
            if (this.Delay <= TimeSpan.Zero) {
                this.IsHidden = false;
                this.SnapPositionToMouse();
            } else {
                this.IsHidden = true;
                this.delayCountdown = this.Delay;
            }
            this.autoHidden = false;
            return true;
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
            elementToHover.OnMouseEnter += e => this.Display(e.System, $"{e.GetType().Name}Tooltip");
            elementToHover.OnMouseExit += e => this.Remove();
            elementToHover.OnSelected += e => {
                if (this.DisplayInAutoNavMode) {
                    this.snapElement = e;
                    this.Display(e.System, $"{e.GetType().Name}Tooltip");
                }
            };
            elementToHover.OnDeselected += e => {
                if (this.DisplayInAutoNavMode) {
                    this.Remove();
                    this.snapElement = null;
                }
            };
        }

        private void Init(Element elementToHover) {
            if (this.Paragraph != null)
                this.Paragraph.AutoAdjustWidth = true;

            this.SetWidthBasedOnChildren = true;
            this.SetHeightBasedOnChildren = true;
            this.CanBeMoused = false;

            if (elementToHover != null)
                this.AddToElement(elementToHover);
        }

        private void UpdateAutoHidden() {
            var shouldBeHidden = true;
            foreach (var child in this.Children) {
                if (!child.IsHidden) {
                    shouldBeHidden = false;
                    break;
                }
            }
            if (this.autoHidden != shouldBeHidden) {
                this.autoHidden = shouldBeHidden;
                this.SetAreaDirty();
            }
        }

    }
}