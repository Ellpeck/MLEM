using Microsoft.Xna.Framework;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A vertical space element for use inside of a <see cref="UiSystem"/>.
    /// A vertical space is an invisible element that can be used to add vertical space between paragraphs or other elements.
    /// </summary>
    public class VerticalSpace : Element {

        /// <summary>
        /// Creates a new vertical space with the given settings
        /// </summary>
        /// <param name="height">The height of the vertical space</param>
        public VerticalSpace(int height) : base(Anchor.AutoCenter, new Vector2(1, height)) {
            this.CanBeSelected = false;
        }

    }
}