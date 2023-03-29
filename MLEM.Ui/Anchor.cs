using MLEM.Ui.Elements;

namespace MLEM.Ui {
    /// <summary>
    /// Represents a location for an <see cref="Element"/> to attach to within its parent (or within the screen's viewport if it is the <see cref="RootElement"/>).
    /// </summary>
    public enum Anchor {

        /// <summary>
        /// Attach to the top left corner of the parent
        /// </summary>
        TopLeft,
        /// <summary>
        /// Attach to the center of the top edge of the parent
        /// </summary>
        TopCenter,
        /// <summary>
        /// Attach to the top right corner of the parent
        /// </summary>
        TopRight,
        /// <summary>
        /// Attach to the center of the left edge of the parent
        /// </summary>
        CenterLeft,
        /// <summary>
        /// Attach to the center position of the parent
        /// </summary>
        Center,
        /// <summary>
        /// Attach to the center of the right edge of the parent
        /// </summary>
        CenterRight,
        /// <summary>
        /// Attach to the bottom left corner of the parent
        /// </summary>
        BottomLeft,
        /// <summary>
        /// Attach to the center of the bottom edge of the parent
        /// </summary>
        BottomCenter,
        /// <summary>
        /// Attach to the bottom right corner of the parent
        /// </summary>
        BottomRight,

        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor will cause an element to be placed below its older sibling, aligned to the left edge of its parent.
        /// </summary>
        AutoLeft,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor will cause an element to be placed below its older sibling, aligned to the horizontal center of its parent.
        /// </summary>
        AutoCenter,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor will cause an element to be placed below its older sibling, aligned to the right edge of its parent.
        /// </summary>
        AutoRight,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor will cause an element to be placed at the top right of its older sibling, or at the start of the next line if there is no space to the right of its older sibling.
        /// </summary>
        AutoInline,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor is an overflow-ignoring version of <see cref="AutoInline"/>, meaning that the element will never be forced into the next line.
        /// Note that, when using this property, it is very easy to cause an element to overflow out of its parent container.
        /// </summary>
        AutoInlineIgnoreOverflow,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor will cause an element to be placed at the center right of its older sibling, or at the start of the next line if there is no space to the right of its older sibling.
        /// </summary>
        AutoInlineCenter,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor is an overflow-ignoring version of <see cref="AutoInlineCenter"/>, meaning that the element will never be forced into the next line.
        /// Note that, when using this property, it is very easy to cause an element to overflow out of its parent container.
        /// </summary>
        AutoInlineCenterIgnoreOverflow,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor will cause an element to be placed at the bottom right of its older sibling, or at the start of the next line if there is no space to the right of its older sibling.
        /// </summary>
        AutoInlineBottom,
        /// <summary>
        /// This is an auto-anchoring value.
        /// This anchor is an overflow-ignoring version of <see cref="AutoInlineBottom"/>, meaning that the element will never be forced into the next line.
        /// Note that, when using this property, it is very easy to cause an element to overflow out of its parent container.
        /// </summary>
        AutoInlineBottomIgnoreOverflow

    }
}
