using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A group element to be used inside of a <see cref="UiSystem"/>.
    /// A group is an element that has no rendering or interaction on its own, but that can aid with automatic placement of child elements.
    /// </summary>
    public class Group : Element {

        /// <summary>
        /// Creates a new group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="setHeightBasedOnChildren">Whether the group's height should be based on its children's height</param>
        public Group(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = true) : base(anchor, size) {
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