using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A group element to be used inside of a <see cref="UiSystem"/>.
    /// A group is an element that has no rendering or interaction on its own, but that can aid with automatic placement of child elements.
    /// If a grouping whose children scroll, and which has a <see cref="ScrollBar"/>, is desired, a <see cref="Panel"/> with its <see cref="Panel.Texture"/> set to <see langword="null"/> can be used.
    /// </summary>
    public class Group : Element {

        /// <summary>
        /// Creates a new group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="setHeightBasedOnChildren">Whether the group's height should be based on its children's height, see <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        public Group(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = true) : this(anchor, size, false, setHeightBasedOnChildren) {}

        /// <summary>
        /// Creates a new group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="setWidthBasedOnChildren">Whether the group's width should be based on its children's width, see <see cref="Element.SetWidthBasedOnChildren"/>.</param>
        /// <param name="setHeightBasedOnChildren">Whether the group's height should be based on its children's height, see <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        public Group(Anchor anchor, Vector2 size, bool setWidthBasedOnChildren, bool setHeightBasedOnChildren) : base(anchor, size) {
            this.SetWidthBasedOnChildren = setWidthBasedOnChildren;
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.CanBeSelected = false;
        }

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            // since the group never accesses its own area when drawing, we have to update it manually
            this.UpdateAreaIfDirty();
            base.Draw(time, batch, alpha, context);
        }

    }
}
