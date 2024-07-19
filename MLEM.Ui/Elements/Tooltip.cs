using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Input;
using MLEM.Ui.Style;
using Color = Microsoft.Xna.Framework.Color;
using RectangleF = MLEM.Maths.RectangleF;
#if FNA
using MLEM.Maths;
#endif

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A tooltip element for use inside of a <see cref="UiSystem"/>.
    /// A tooltip is a <see cref="Panel"/> with a custom cursor that always follows the position of the mouse.
    /// Tooltips can easily be configured to be hooked onto an element, causing them to appear when it is moused, and disappear when it is not moused anymore.
    /// </summary>
    public class Tooltip : Panel {

        /// <summary>
        /// A list of <see cref="Elements.Paragraph"/> objects that this tooltip automatically manages.
        /// A paragraph that is contained in this list will automatically have the <see cref="UiStyle.TooltipTextWidth"/> and <see cref="UiStyle.TooltipTextColor"/> applied.
        /// To add a paragraph to both this list and to <see cref="Element.Children"/>, use <see cref="AddParagraph(Elements.Paragraph,int)"/>.
        /// </summary>
        public readonly List<Paragraph> Paragraphs = new List<Paragraph>();

        /// <summary>
        /// The offset that this tooltip should have from the mouse position
        /// </summary>
        public StyleProp<Vector2> MouseOffset;
        /// <summary>
        /// The offset that this tooltip  should have from the element snapped to when <see cref="DisplayInAutoNavMode"/> is true.
        /// </summary>
        public StyleProp<Vector2> AutoNavOffset;
        /// <summary>
        /// The anchor that should be used when this tooltip is displayed using the mouse. The <see cref="MouseOffset"/> will be applied.
        /// </summary>
        public StyleProp<Anchor> MouseAnchor;
        /// <summary>
        /// The anchor that should be used when this tooltip is displayed using auto-nav mode. The <see cref="AutoNavOffset"/> will be applied.
        /// </summary>
        public StyleProp<Anchor> AutoNavAnchor;
        /// <summary>
        /// If this is <see langword="true"/>, and the mouse is used, the tooltip will attach to the hovered element in a static position using the <see cref="AutoNavOffset"/> and <see cref="AutoNavAnchor"/> properties, rather than following the mouse cursor exactly.
        /// </summary>
        public StyleProp<bool> UseAutoNavBehaviorForMouse;
        /// <summary>
        /// The amount of time that the mouse has to be over an element before it appears
        /// </summary>
        public StyleProp<TimeSpan> Delay;
        /// <summary>
        /// The <see cref="Elements.Paragraph.TextColor"/> that this tooltip's <see cref="Paragraphs"/> should have
        /// </summary>
        public StyleProp<Color> ParagraphTextColor {
            get => this.paragraphTextColor;
            set {
                this.paragraphTextColor = value;
                this.UpdateParagraphsStyles();
            }
        }
        /// <summary>
        /// The <see cref="Elements.Paragraph.TextScale"/> that this tooltip's <see cref="Paragraphs"/> should have
        /// </summary>
        public StyleProp<float> ParagraphTextScale {
            get => this.paragraphTextScale;
            set {
                this.paragraphTextScale = value;
                this.UpdateParagraphsStyles();
            }
        }
        /// <summary>
        /// The width that this tooltip's <see cref="Paragraphs"/> should have
        /// </summary>
        public StyleProp<float> ParagraphWidth {
            get => this.paragraphWidth;
            set {
                this.paragraphWidth = value;
                this.UpdateParagraphsStyles();
            }
        }

        /// <summary>
        /// Determines whether this tooltip should display when <see cref="UiControls.IsAutoNavMode"/> is true, which is when the UI is being controlled using a keyboard or gamepad.
        /// If this tooltip is displayed in auto-nav mode, it will display below the selected element with the <see cref="AutoNavOffset"/> applied.
        /// </summary>
        public bool DisplayInAutoNavMode;
        /// <summary>
        /// The position that this tooltip should be following (or snapped to) instead of the <see cref="InputHandler.ViewportMousePosition"/>.
        /// If this value is unset, <see cref="InputHandler.ViewportMousePosition"/> will be used as the snap position.
        /// Note that <see cref="MouseOffset"/> is still applied with this value set.
        /// Note that, if <see cref="UseAutoNavBehaviorForMouse"/> is <see langword="true"/>, this value is ignored.
        /// </summary>
        public virtual Vector2? SnapPosition { get; set; }

        /// <inheritdoc />
        public override bool IsHidden => this.autoHidden || base.IsHidden;

        private TimeSpan delayCountdown;
        private bool autoHidden;
        private Element snapElement;
        private StyleProp<float> paragraphWidth;
        private StyleProp<float> paragraphTextScale;
        private StyleProp<Color> paragraphTextColor;

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="text">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(string text = null, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            if (text != null)
                this.AddParagraph(text);
            this.Init(elementToHover);
        }

        /// <summary>
        /// Creates a new tooltip with the given settings
        /// </summary>
        /// <param name="textCallback">The text to display on the tooltip</param>
        /// <param name="elementToHover">The element that should automatically cause the tooltip to appear and disappear when hovered and not hovered, respectively</param>
        public Tooltip(Paragraph.TextCallback textCallback, Element elementToHover = null) :
            base(Anchor.TopLeft, Vector2.One, Vector2.Zero) {
            this.AddParagraph(textCallback);
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
                    this.SnapPositionToMouse();
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
            this.MouseAnchor = this.MouseAnchor.OrStyle(style.TooltipMouseAnchor);
            this.AutoNavAnchor = this.AutoNavAnchor.OrStyle(style.TooltipAutoNavAnchor);
            this.UseAutoNavBehaviorForMouse = this.UseAutoNavBehaviorForMouse.OrStyle(style.TooltipUseAutoNavBehaviorForMouse);
            this.Delay = this.Delay.OrStyle(style.TooltipDelay);
            this.ParagraphTextColor = this.ParagraphTextColor.OrStyle(style.TooltipTextColor);
            this.ParagraphTextScale = this.ParagraphTextScale.OrStyle(style.TextScale);
            this.ParagraphWidth = this.ParagraphWidth.OrStyle(style.TooltipTextWidth);
            this.ChildPadding = this.ChildPadding.OrStyle(style.TooltipChildPadding);
            this.UpdateParagraphsStyles();
        }

        /// <summary>
        /// Adds the given paragraph to this tooltip's managed <see cref="Paragraphs"/> list, as well as to its children using <see cref="Element.AddChild{T}"/>.
        /// A paragraph that is contained in the <see cref="Paragraphs"/> list will automatically have the <see cref="UiStyle.TooltipTextWidth"/> and <see cref="UiStyle.TooltipTextColor"/> applied.
        /// </summary>
        /// <param name="paragraph">The paragraph to add</param>
        /// <returns>The added paragraph, for chaining</returns>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Element.Children"/> list</param>
        public Paragraph AddParagraph(Paragraph paragraph, int index = -1) {
            this.Paragraphs.Add(paragraph);
            this.AddChild(paragraph, index);
            this.UpdateParagraphStyle(paragraph);
            return paragraph;
        }

        /// <summary>
        /// Adds a new paragraph with the given text callback to this tooltip's managed <see cref="Paragraphs"/> list, as well as to its children using <see cref="Element.AddChild{T}"/>.
        /// A paragraph that is contained in the <see cref="Paragraphs"/> list will automatically have the <see cref="UiStyle.TooltipTextWidth"/> and <see cref="UiStyle.TooltipTextColor"/> applied.
        /// </summary>
        /// <param name="text">The text that the paragraph should display</param>
        /// <returns>The created paragraph, for chaining</returns>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Element.Children"/> list</param>
        public Paragraph AddParagraph(Paragraph.TextCallback text, int index = -1) {
            return this.AddParagraph(new Paragraph(Anchor.AutoLeft, 0, text), index);
        }

        /// <summary>
        /// Adds a new paragraph with the given text to this tooltip's managed <see cref="Paragraphs"/> list, as well as to its children using <see cref="Element.AddChild{T}"/>.
        /// A paragraph that is contained in the <see cref="Paragraphs"/> list will automatically have the <see cref="UiStyle.TooltipTextWidth"/> and <see cref="UiStyle.TooltipTextColor"/> applied.
        /// </summary>
        /// <param name="text">The text that the paragraph should display</param>
        /// <returns>The created paragraph, for chaining</returns>
        /// <param name="index">The index to add the child at, or -1 to add it to the end of the <see cref="Element.Children"/> list</param>
        public Paragraph AddParagraph(string text, int index = -1) {
            return this.AddParagraph(p => text, index);
        }

        /// <summary>
        /// Causes this tooltip's position to be snapped to the mouse position, or the element to snap to if <see cref="DisplayInAutoNavMode"/> is true, or the <see cref="SnapPosition"/> if set.
        /// </summary>
        public void SnapPositionToMouse() {
            Vector2 snapPosition;
            if (this.snapElement != null) {
                snapPosition = this.GetSnapOffset(this.AutoNavAnchor, this.snapElement.DisplayArea, this.AutoNavOffset);
            } else {
                var mouseBounds = new RectangleF(this.SnapPosition ?? this.Input.ViewportMousePosition.ToVector2(), Vector2.Zero);
                snapPosition = this.GetSnapOffset(this.MouseAnchor, mouseBounds, this.MouseOffset);
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
            // mouse controls
            elementToHover.OnMouseEnter += e => {
                if (this.UseAutoNavBehaviorForMouse)
                    this.snapElement = e;
                this.Display(e.System, $"{e.GetType().Name}Tooltip");
            };
            elementToHover.OnMouseExit += e => {
                this.Remove();
                if (this.UseAutoNavBehaviorForMouse)
                    this.snapElement = null;
            };

            // auto-nav controls
            elementToHover.OnSelected += e => {
                if (this.DisplayInAutoNavMode && e.Controls.IsAutoNavMode) {
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

        private void UpdateParagraphsStyles() {
            foreach (var paragraph in this.Paragraphs)
                this.UpdateParagraphStyle(paragraph);
        }

        private void UpdateParagraphStyle(Paragraph paragraph) {
            paragraph.TextColor = paragraph.TextColor.OrStyle(this.ParagraphTextColor, 1);
            paragraph.TextScale = paragraph.TextScale.OrStyle(this.ParagraphTextScale, 1);
            paragraph.Size = new Vector2(this.ParagraphWidth, 0);
            paragraph.AutoAdjustWidth = true;
        }

        private Vector2 GetSnapOffset(Anchor anchor, RectangleF snapBounds, Vector2 offset) {
            switch (anchor) {
                case Anchor.TopLeft:
                    return snapBounds.Location - this.DisplayArea.Size - offset;
                case Anchor.TopCenter:
                    return new Vector2(snapBounds.Center.X - this.DisplayArea.Width / 2F, snapBounds.Top - this.DisplayArea.Height) - offset;
                case Anchor.TopRight:
                    return new Vector2(snapBounds.Right + offset.X, snapBounds.Top - this.DisplayArea.Height - offset.Y);
                case Anchor.CenterLeft:
                    return new Vector2(snapBounds.X - this.DisplayArea.Width - offset.X, snapBounds.Center.Y - this.DisplayArea.Height / 2 + offset.Y);
                case Anchor.Center:
                    return snapBounds.Center - this.DisplayArea.Size / 2 + offset;
                case Anchor.CenterRight:
                    return new Vector2(snapBounds.Right, snapBounds.Center.Y - this.DisplayArea.Height / 2) + offset;
                case Anchor.BottomLeft:
                    return new Vector2(snapBounds.X - this.DisplayArea.Width - offset.X, snapBounds.Bottom + offset.Y);
                case Anchor.BottomCenter:
                    return new Vector2(snapBounds.Center.X - this.DisplayArea.Width / 2F, snapBounds.Bottom) + offset;
                case Anchor.BottomRight:
                    return snapBounds.Location + snapBounds.Size + offset;
                default:
                    throw new NotSupportedException($"Tooltip anchors don't support the {anchor} value");
            }
        }

    }
}
