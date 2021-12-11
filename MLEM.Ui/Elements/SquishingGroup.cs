using System;
using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A squishing group is a <see cref="Group"/> whose <see cref="Element.Children"/> automatically get resized so that they do not overlap each other.
    /// The order in which elements are squished depends on their <see cref="Element.Priority"/>, where elements with a lower priority will move out of the way of elements with a higher priority.
    /// If all elements have the same priority, their addition order (their order in <see cref="Element.Children"/>) determines squish order.
    /// </summary>
    public class SquishingGroup : Group {

        /// <summary>
        /// Creates a new squishing group with the given settings.
        /// </summary>
        /// <param name="anchor">The group's anchor.</param>
        /// <param name="size">The group's size.</param>
        public SquishingGroup(Anchor anchor, Vector2 size) : base(anchor, size, false) {
        }

        /// <inheritdoc />
        public override void SetAreaAndUpdateChildren(RectangleF area) {
            base.SetAreaAndUpdateChildren(area);
            // we squish children in order of priority, since auto-anchoring is based on addition order
            for (var i = 0; i < this.SortedChildren.Count; i++) {
                var child = this.SortedChildren[i];
                if (SquishChild(child, out var squished))
                    child.SetAreaAndUpdateChildren(squished);
            }
        }

        /// <inheritdoc />
        protected override void OnChildAreaDirty(Element child) {
            base.OnChildAreaDirty(child);
            this.SetAreaDirty();
        }

        private static bool SquishChild(Element element, out RectangleF squishedArea) {
            squishedArea = default;
            if (element.IsHidden)
                return false;
            var pos = element.Area.Location;
            var size = element.Area.Size;
            foreach (var sibling in element.GetSiblings(e => !e.IsHidden)) {
                var siblingArea = sibling.Area;
                var leftIntersect = siblingArea.Right - pos.X;
                var rightIntersect = pos.X + size.X - siblingArea.Left;
                var bottomIntersect = siblingArea.Bottom - pos.Y;
                var topIntersect = pos.Y + size.Y - siblingArea.Top;
                if (leftIntersect > 0 && rightIntersect > 0 && bottomIntersect > 0 && topIntersect > 0) {
                    if (rightIntersect + leftIntersect < topIntersect + bottomIntersect) {
                        if (rightIntersect > leftIntersect) {
                            size.X -= siblingArea.Right - pos.X;
                            pos.X = Math.Max(pos.X, siblingArea.Right);
                        } else {
                            size.X = Math.Min(pos.X + size.X, siblingArea.Left) - pos.X;
                        }
                    } else {
                        if (topIntersect > bottomIntersect) {
                            size.Y -= siblingArea.Bottom - pos.Y;
                            pos.Y = Math.Max(pos.Y, siblingArea.Bottom);
                        } else {
                            size.Y = Math.Min(pos.Y + size.Y, siblingArea.Top) - pos.Y;
                        }
                    }
                }
            }
            if (!pos.Equals(element.Area.Location, Epsilon) || !size.Equals(element.Area.Size, Epsilon)) {
                squishedArea = new RectangleF(pos, size);
                return true;
            }
            return false;
        }

    }
}