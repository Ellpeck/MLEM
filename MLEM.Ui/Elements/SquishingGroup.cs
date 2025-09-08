using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Maths;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A squishing group is a <see cref="Group"/> whose <see cref="Element.Children"/> automatically get resized so that they do not overlap each other. Elements are squished in a way that maximizes the final area that each element retains compared to its original area.
    /// The order in which elements are squished depends on their <see cref="Element.Priority"/>, where elements with a lower priority will move out of the way of elements with a higher priority. If all elements have the same priority, their addition order (their order in <see cref="Element.Children"/>) determines squish order.
    /// </summary>
    public class SquishingGroup : Group {

        /// <summary>
        /// Creates a new squishing group with the given settings.
        /// </summary>
        /// <param name="anchor">The group's anchor.</param>
        /// <param name="size">The group's size.</param>
        public SquishingGroup(Anchor anchor, Vector2 size) : base(anchor, size, false) {}

        /// <inheritdoc />
        public override void SetAreaAndUpdateChildren(RectangleF area) {
            base.SetAreaAndUpdateChildren(area);
            // we squish children in order of priority, since auto-anchoring is based on addition order
            for (var i = 0; i < this.SortedChildren.Count; i++) {
                var child = this.SortedChildren[i];
                if (SquishingGroup.SquishChild(child, out var squished))
                    child.SetAreaAndUpdateChildren(squished);
            }
        }

        /// <inheritdoc />
        protected override void OnChildAreaDirty(Element child, bool grandchild) {
            base.OnChildAreaDirty(child, grandchild);
            if (!grandchild)
                this.SetAreaDirty();
        }

        private static bool SquishChild(Element element, out RectangleF squishedArea) {
            squishedArea = default;
            if (element.IsHidden)
                return false;
            var pos = element.Area.Location;
            var size = element.Area.Size;
            foreach (var sibling in element.GetSiblings(e => !e.IsHidden)) {
                var sibArea = sibling.Area;
                if (pos.X < sibArea.Right && sibArea.Left < pos.X + size.X && pos.Y < sibArea.Bottom && sibArea.Top < pos.Y + size.Y) {
                    var possible = new[] {
                        new RectangleF(Math.Max(pos.X, sibArea.Right), pos.Y, size.X - (sibArea.Right - pos.X), size.Y),
                        new RectangleF(pos.X, pos.Y, Math.Min(pos.X + size.X, sibArea.Left) - pos.X, size.Y),
                        new RectangleF(pos.X, Math.Max(pos.Y, sibArea.Bottom), size.X, size.Y - (sibArea.Bottom - pos.Y)),
                        new RectangleF(pos.X, pos.Y, size.X, Math.Min(pos.Y + size.Y, sibArea.Top) - pos.Y)
                    };
                    var biggest = possible.OrderByDescending(r => r.Width * r.Height).First();
                    pos = biggest.Location;
                    size = biggest.Size;
                }
            }
            if (!pos.Equals(element.Area.Location, Element.Epsilon) || !size.Equals(element.Area.Size, Element.Epsilon)) {
                squishedArea = new RectangleF(pos, size);
                return true;
            }
            return false;
        }

    }
}
