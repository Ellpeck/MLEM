namespace MLEM.Formatting {
    /// <summary>
    /// An enumeration that represents a set of alignment options for <see cref="TokenizedString"/> objects and MLEM.Ui paragraphs.
    /// </summary>
    public enum TextAlignment {

        /// <summary>
        /// Left alignment, which is also the default value
        /// </summary>
        Left,
        /// <summary>
        /// Center alignment
        /// </summary>
        Center,
        /// <summary>
        /// Right alignment.
        /// In this alignment option, trailing spaces are ignored to ensure that visual alignment is consistent.
        /// </summary>
        Right

    }
}