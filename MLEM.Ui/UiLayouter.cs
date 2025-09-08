using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    /// <summary>
    /// Generic tools and utilities for layouting <see cref="ILayoutItem"/> objects for automatic positioning and sizing.
    /// For the recommended ui system implementation that uses this layouter, see <see cref="UiSystem"/> and specifically <see cref="Element"/>.
    /// </summary>
    public static class UiLayouter {

        /// <summary>
        /// Lays out the given <paramref name="item"/> based on the information specified in its <see cref="ILayoutItem"/> interface.
        /// </summary>
        /// <param name="item">The item to lay out.</param>
        /// <param name="epsilon">An epsilon value used in layout item size, position and resulting area calculations to mitigate floating point rounding inaccuracies.</param>
        /// <typeparam name="T"></typeparam>
        public static void Layout<T>(T item, float epsilon = 0) where T : class, ILayoutItem {
            var recursion = 0;
            UpdateDisplayArea();

            void UpdateDisplayArea(Vector2? overrideSize = null) {
                var parentArea = item.ParentArea;
                var parentCenterX = parentArea.X + parentArea.Width / 2;
                var parentCenterY = parentArea.Y + parentArea.Height / 2;

                var intendedSize = item.CalcActualSize(parentArea);
                var newSize = overrideSize ?? intendedSize;
                var pos = new Vector2();

                switch (item.Anchor) {
                    case Anchor.TopLeft:
                    case Anchor.AutoLeft:
                    case Anchor.AutoInline:
                    case Anchor.AutoInlineCenter:
                    case Anchor.AutoInlineBottom:
                    case Anchor.AutoInlineIgnoreOverflow:
                    case Anchor.AutoInlineCenterIgnoreOverflow:
                    case Anchor.AutoInlineBottomIgnoreOverflow:
                        pos.X = parentArea.X + item.ScaledOffset.X;
                        pos.Y = parentArea.Y + item.ScaledOffset.Y;
                        break;
                    case Anchor.TopCenter:
                    case Anchor.AutoCenter:
                        pos.X = parentCenterX - newSize.X / 2 + item.ScaledOffset.X;
                        pos.Y = parentArea.Y + item.ScaledOffset.Y;
                        break;
                    case Anchor.TopRight:
                    case Anchor.AutoRight:
                        pos.X = parentArea.Right - newSize.X - item.ScaledOffset.X;
                        pos.Y = parentArea.Y + item.ScaledOffset.Y;
                        break;
                    case Anchor.CenterLeft:
                        pos.X = parentArea.X + item.ScaledOffset.X;
                        pos.Y = parentCenterY - newSize.Y / 2 + item.ScaledOffset.Y;
                        break;
                    case Anchor.Center:
                        pos.X = parentCenterX - newSize.X / 2 + item.ScaledOffset.X;
                        pos.Y = parentCenterY - newSize.Y / 2 + item.ScaledOffset.Y;
                        break;
                    case Anchor.CenterRight:
                        pos.X = parentArea.Right - newSize.X - item.ScaledOffset.X;
                        pos.Y = parentCenterY - newSize.Y / 2 + item.ScaledOffset.Y;
                        break;
                    case Anchor.BottomLeft:
                        pos.X = parentArea.X + item.ScaledOffset.X;
                        pos.Y = parentArea.Bottom - newSize.Y - item.ScaledOffset.Y;
                        break;
                    case Anchor.BottomCenter:
                        pos.X = parentCenterX - newSize.X / 2 + item.ScaledOffset.X;
                        pos.Y = parentArea.Bottom - newSize.Y - item.ScaledOffset.Y;
                        break;
                    case Anchor.BottomRight:
                        pos.X = parentArea.Right - newSize.X - item.ScaledOffset.X;
                        pos.Y = parentArea.Bottom - newSize.Y - item.ScaledOffset.Y;
                        break;
                }

                if (item.Anchor.IsAuto()) {
                    if (item.Anchor.IsInline()) {
                        var anchorEl = UiLayouter.GetOlderSibling(item, e => !e.IsHidden && e.CanAutoAnchorsAttach);
                        if (anchorEl != null) {
                            var anchorElArea = anchorEl.AutoAnchorArea;
                            var newX = anchorElArea.Right + item.ScaledOffset.X;
                            // with awkward ui scale values, floating point rounding can cause an layout item that would usually
                            // be positioned correctly to be pushed into the next line due to a very small deviation
                            if (item.Anchor.IsIgnoreOverflow() || newX + newSize.X <= parentArea.Right + epsilon) {
                                pos.X = newX;
                                pos.Y = anchorElArea.Y + item.ScaledOffset.Y;
                                if (item.Anchor == Anchor.AutoInlineCenter || item.Anchor == Anchor.AutoInlineCenterIgnoreOverflow) {
                                    pos.Y += (anchorElArea.Height - newSize.Y) / 2;
                                } else if (item.Anchor == Anchor.AutoInlineBottom || item.Anchor == Anchor.AutoInlineBottomIgnoreOverflow) {
                                    pos.Y += anchorElArea.Height - newSize.Y;
                                }
                            } else {
                                // inline anchors that overflow into the next line act like AutoLeft
                                var newlineAnchorEl = UiLayouter.GetLowestOlderSibling(item, e => !e.IsHidden && e.CanAutoAnchorsAttach);
                                if (newlineAnchorEl != null)
                                    pos.Y = newlineAnchorEl.AutoAnchorArea.Bottom + item.ScaledOffset.Y;
                            }
                        }
                    } else {
                        // auto anchors keep their x coordinates from the switch above
                        var anchorEl = UiLayouter.GetLowestOlderSibling(item, e => !e.IsHidden && e.CanAutoAnchorsAttach);
                        if (anchorEl != null)
                            pos.Y = anchorEl.AutoAnchorArea.Bottom + item.ScaledOffset.Y;
                    }
                }

                if (item.PreventParentSpill) {
                    if (pos.X < parentArea.X)
                        pos.X = parentArea.X;
                    if (pos.Y < parentArea.Y)
                        pos.Y = parentArea.Y;
                    if (pos.X + newSize.X > parentArea.Right)
                        newSize.X = parentArea.Right - pos.X;
                    if (pos.Y + newSize.Y > parentArea.Bottom)
                        newSize.Y = parentArea.Bottom - pos.Y;
                }

                item.SetAreaAndUpdateChildren(new RectangleF(pos, newSize));

                if (item.SetWidthBasedOnChildren || item.SetHeightBasedOnChildren) {
                    T foundChild = null;
                    var autoSize = item.UnscrolledArea.Size;

                    if (item.SetHeightBasedOnChildren) {
                        var lowest = UiLayouter.GetLowestChild(item, e => !e.IsHidden);
                        if (lowest != null) {
                            if (lowest.Anchor.IsTopAligned()) {
                                autoSize.Y = lowest.UnscrolledArea.Bottom - pos.Y + item.ScaledChildPadding.Bottom;
                            } else {
                                autoSize.Y = lowest.UnscrolledArea.Height + item.ScaledChildPadding.Height;
                            }
                            foundChild = lowest;
                        } else {
                            autoSize.Y = 0;
                        }
                    }

                    if (item.SetWidthBasedOnChildren) {
                        var rightmost = UiLayouter.GetRightmostChild(item, e => !e.IsHidden);
                        if (rightmost != null) {
                            if (rightmost.Anchor.IsLeftAligned()) {
                                autoSize.X = rightmost.UnscrolledArea.Right - pos.X + item.ScaledChildPadding.Right;
                            } else {
                                autoSize.X = rightmost.UnscrolledArea.Width + item.ScaledChildPadding.Width;
                            }
                            foundChild = rightmost;
                        } else {
                            autoSize.X = 0;
                        }
                    }

                    if (item.TreatSizeAsMinimum) {
                        autoSize = Vector2.Max(autoSize, intendedSize);
                    } else if (item.TreatSizeAsMaximum) {
                        autoSize = Vector2.Min(autoSize, intendedSize);
                    }

                    // we want to leave some leeway to prevent float rounding causing an infinite loop
                    if (!autoSize.Equals(item.UnscrolledArea.Size, epsilon)) {
                        recursion++;
                        item.OnLayoutRecursion(recursion, foundChild);
                        UpdateDisplayArea(autoSize);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the given layout item's lowest child layout item (in terms of y position) that matches the given condition.
        /// </summary>
        /// <param name="item">The item whose lowest child to retrieve.</param>
        /// <param name="condition">The condition to match</param>
        /// <param name="total">Whether to evaluate based on the child's <see cref="ILayoutItem.GetTotalCoveredArea"/>, rather than its <see cref="ILayoutItem.UnscrolledArea"/>.</param>
        /// <returns>The lowest layout item, or null if no such layout item exists</returns>
        public static T GetLowestChild<T>(T item, Func<T, bool> condition = null, bool total = false) where T : class, ILayoutItem {
            T lowest = null;
            var lowestX = float.MinValue;
            foreach (var child in item.Children) {
                if (condition != null && !condition((T) child))
                    continue;
                var covered = total ? child.GetTotalCoveredArea(true) : child.UnscrolledArea;
                var x = !child.Anchor.IsTopAligned() ? covered.Height : covered.Bottom;
                if (x >= lowestX) {
                    lowest = (T) child;
                    lowestX = x;
                }
            }
            return lowest;
        }

        /// <summary>
        /// Returns the given layout item's rightmost child (in terms of x position) that matches the given condition.
        /// </summary>
        /// <param name="item">The item whose rightmost child to retrieve.</param>
        /// <param name="condition">The condition to match</param>
        /// <param name="total">Whether to evaluate based on the child's <see cref="ILayoutItem.GetTotalCoveredArea"/>, rather than its <see cref="ILayoutItem.UnscrolledArea"/>.</param>
        /// <returns>The rightmost layout item, or null if no such layout item exists</returns>
        public static T GetRightmostChild<T>(T item, Func<T, bool> condition = null, bool total = false) where T : class, ILayoutItem {
            T rightmost = null;
            var rightmostX = float.MinValue;
            foreach (var child in item.Children) {
                if (condition != null && !condition((T) child))
                    continue;
                var covered = total ? child.GetTotalCoveredArea(true) : child.UnscrolledArea;
                var x = !child.Anchor.IsLeftAligned() ? covered.Width : covered.Right;
                if (x >= rightmostX) {
                    rightmost = (T) child;
                    rightmostX = x;
                }
            }
            return rightmost;
        }

        /// <summary>
        /// Returns the given layout item's lowest sibling that is also older (lower in its parent's <see cref="ILayoutItem.Children"/> list) than the given layout item that also matches the given condition.
        /// The returned layout item's <see cref="ILayoutItem.Parent"/> will always be equal to the given layout item's <see cref="ILayoutItem.Parent"/>.
        /// </summary>
        /// <param name="item">The item whose lowest older sibling to retrieve.</param>
        /// <param name="condition">The condition to match</param>
        /// <param name="total">Whether to evaluate based on the child's <see cref="ILayoutItem.GetTotalCoveredArea"/>, rather than its <see cref="ILayoutItem.UnscrolledArea"/>.</param>
        /// <returns>The lowest older sibling of the given layout item, or null if no such layout item exists</returns>
        public static T GetLowestOlderSibling<T>(T item, Func<T, bool> condition = null, bool total = false) where T : class, ILayoutItem {
            if (item.Parent == null)
                return null;
            T lowest = null;
            foreach (var child in item.Parent.Children) {
                if (child == item)
                    break;
                if (condition != null && !condition((T) child))
                    continue;
                if (lowest == null || (total ? child.GetTotalCoveredArea(true) : child.UnscrolledArea).Bottom >= lowest.UnscrolledArea.Bottom)
                    lowest = (T) child;
            }
            return lowest;
        }

        /// <summary>
        /// Returns the given layout item's first older sibling that matches the given condition.
        /// The returned layout item's <see cref="ILayoutItem.Parent"/> will always be equal to the given layout item's <see cref="ILayoutItem.Parent"/>.
        /// </summary>
        /// <param name="item">The item whose older siblings to retrieve.</param>
        /// <param name="condition">The condition to match</param>
        /// <returns>The older sibling, or null if no such layout item exists</returns>
        public static T GetOlderSibling<T>(T item, Func<T, bool> condition = null) where T : class, ILayoutItem {
            if (item.Parent == null)
                return null;
            T older = null;
            foreach (var child in item.Parent.Children) {
                if (child == item)
                    break;
                if (condition != null && !condition((T) child))
                    continue;
                older = (T) child;
            }
            return older;
        }

        /// <summary>
        /// Returns all of the given layout item's children of the given type that match the given condition.
        /// Optionally, the entire tree of children (grandchildren) can be searched.
        /// </summary>
        /// <param name="item">The item whose children to retrieve.</param>
        /// <param name="condition">The condition to match</param>
        /// <param name="regardGrandchildren">If this value is true, children of children of the item are also searched</param>
        /// <param name="ignoreFalseGrandchildren">If this value is true, children for which the condition does not match will not have their children searched</param>
        /// <typeparam name="T">The type of children to search for</typeparam>
        /// <returns>All children that match the condition</returns>
        public static IEnumerable<T> GetChildren<T>(ILayoutItem item, Func<T, bool> condition = null, bool regardGrandchildren = false, bool ignoreFalseGrandchildren = false) where T : class, ILayoutItem {
            foreach (var child in item.Children) {
                var applies = child is T t && (condition == null || condition(t));
                if (applies)
                    yield return (T) child;
                if (regardGrandchildren && (!ignoreFalseGrandchildren || applies)) {
                    foreach (var cc in UiLayouter.GetChildren(child, condition, true, ignoreFalseGrandchildren))
                        yield return cc;
                }
            }
        }

    }

    /// <summary>
    /// A layout item is an object that can be layouted automatically using <see cref="UiLayouter"/>.
    /// The layoug item system is used by the <see cref="UiSystem"/>, specifically the <see cref="Element"/> class.
    /// </summary>
    public interface ILayoutItem {

        /// <summary>
        /// This layout item's parent layout item.
        /// If this layout item has no parent (it is the root item of an item tree), this value is <c>null</c>.
        /// </summary>
        ILayoutItem Parent { get; }
        /// <summary>
        /// A collection of all of this layout item's direct children.
        /// </summary>
        IEnumerable<ILayoutItem> Children { get; }

        /// <summary>
        /// The area that this layout item's <see cref="Parent"/> takes up, or, if it has no parent, the viewport or the total ui area.
        /// This value is the one that is passed to <see cref="CalcActualSize"/> during <see cref="UiLayouter.Layout{T}"/>.
        /// </summary>
        RectangleF ParentArea { get; }
        /// <summary>
        /// This layout item's area, without respecting potential offsets from scrolling panels.
        /// This area is updated automatically to fit this layout item's sizing and positioning properties.
        /// </summary>
        RectangleF UnscrolledArea { get; }
        /// <summary>
        /// Returns the area that should be used for determining where auto-anchoring children should attach.
        /// </summary>
        RectangleF AutoAnchorArea { get; }
        /// <summary>
        /// The child padding that this layout item has.
        /// The child padding moves any <see cref="Children"/> added to this layout item inwards by the given amount in each direction.
        /// </summary>
        Padding ScaledChildPadding { get; }
        /// <summary>
        /// This layout item's offset from its default position, which is dictated by its <see cref="Anchor"/>.
        /// Note that, depending on the side that the layout item is anchored to, this offset moves it in a different direction.
        /// </summary>
        Vector2 ScaledOffset { get; }
        /// <summary>
        /// The <see cref="Anchor"/> that this layout item uses for positioning within its parent
        /// </summary>
        Anchor Anchor { get; }

        /// <summary>
        /// Set this property to true to cause this layout item's width to be automatically calculated based on the area that its <see cref="Children"/> take up.
        /// To use this layout item's <see cref="System.Drawing.Size"/>'s X coordinate as a minimum or maximum width rather than ignoring it, set <see cref="TreatSizeAsMinimum"/> or <see cref="TreatSizeAsMaximum"/> to true.
        /// </summary>
        bool SetWidthBasedOnChildren { get; }
        /// <summary>
        /// Set this property to true to cause this layout item's height to be automatically calculated based on the area that its <see cref="Children"/> take up.
        /// To use this layout item's <see cref="System.Drawing.Size"/>'s Y coordinate as a minimum or maximum height rather than ignoring it, set <see cref="TreatSizeAsMinimum"/> or <see cref="TreatSizeAsMaximum"/> to true.
        /// </summary>
        bool SetHeightBasedOnChildren { get; }
        /// <summary>
        /// If this field is set to true, and <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled, the resulting width or height will always be greather than or equal to this layout item's <see cref="System.Drawing.Size"/>.
        /// For example, if an layout item's <see cref="System.Drawing.Size"/>'s Y coordinate is set to 20, but there is only one child with a height of 10 in it, the layout item's height would be shrunk to 10 if this value was false, but would remain at 20 if it was true.
        /// Note that this value only has an effect if <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled.
        /// </summary>
        bool TreatSizeAsMinimum { get; }
        /// <summary>
        /// If this field is set to true, and <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/>are enabled, the resulting width or height weill always be less than or equal to this layout item's <see cref="System.Drawing.Size"/>.
        /// Note that this value only has an effect if <see cref="SetWidthBasedOnChildren"/> or <see cref="SetHeightBasedOnChildren"/> are enabled.
        /// </summary>
        bool TreatSizeAsMaximum { get; }
        /// <summary>
        /// Set this field to true to cause this layout item's final display area to never exceed that of its <see cref="Parent"/>.
        /// If the resulting area is too large, the size of this layout item is shrunk to fit the target area.
        /// This can be useful if an layout item should fill the remaining area of a parent exactly.
        /// </summary>
        bool PreventParentSpill { get; }
        /// <summary>
        /// Set this property to <c>true</c> to cause this layout item to be hidden.
        /// Hidden layout items don't receive input events, aren't rendered and don't factor into auto-anchoring.
        /// </summary>
        bool IsHidden { get; }
        /// <summary>
        /// Set this field to false to cause auto-anchored siblings to ignore this layout item as a possible anchor point.
        /// </summary>
        bool CanAutoAnchorsAttach { get; }

        /// <summary>
        /// Calculates the actual size that this layout item should take up, based on the area that its parent encompasses.
        /// By default, this is based on the information specified in <see cref="System.Drawing.Size"/>'s documentation.
        /// </summary>
        /// <param name="parentArea">This parent's area, or the ui system's viewport if it has no parent</param>
        /// <returns>The actual size of this layout item, any scaling into account</returns>
        Vector2 CalcActualSize(RectangleF parentArea);

        /// <summary>
        /// Returns the total covered area of this layout item, which is its area (or <see cref="UnscrolledArea"/>), unioned with all of the total covered areas of its <see cref="Children"/>.
        /// The returned area is only different from this layout item's area (or <see cref="UnscrolledArea"/>) if it has any <see cref="Children"/> that are outside of this layout item's area, or are bigger than this layout item.
        /// </summary>
        /// <param name="unscrolled">Whether to use layout items' <see cref="UnscrolledArea"/> (instead of their potentially modified area through scrolling panels).</param>
        /// <returns>This layout item's total covered area.</returns>
        RectangleF GetTotalCoveredArea(bool unscrolled);

        /// <summary>
        /// Sets this layout item's area to the given <see cref="RectangleF"/>.
        /// This method should also update all of this layout item's <see cref="Children"/>'s areas.
        /// Note that this method does not take into account any auto-sizing, anchoring or positioning, and so it should be invoked sparingly, if at all.
        /// </summary>
        /// <seealso cref="UiLayouter.Layout{T}"/>
        void SetAreaAndUpdateChildren(RectangleF area);

        /// <summary>
        /// A method called by <see cref="UiLayouter.Layout{T}"/> when a layout item's size is being recalculated based on its children.
        /// </summary>
        /// <param name="recursion">The current recursion depth.</param>
        /// <param name="relevantChild">The child that triggered the layout recursion.</param>
        void OnLayoutRecursion(int recursion, ILayoutItem relevantChild);

    }
}
