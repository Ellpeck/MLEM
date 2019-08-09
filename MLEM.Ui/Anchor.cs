namespace MLEM.Ui {
    public enum Anchor {

        TopLeft,
        TopCenter,
        TopRight,
        CenterLeft,
        Center,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight,

        AutoLeft, // below older sibling, aligned to the left
        AutoCenter, // below older sibling, aligned to the center
        AutoRight, //below older sibling, aligned to the right
        AutoInline, // right of older sibling or below if overflows
        AutoInlineIgnoreOverflow // right of older sibling at all time

    }
}