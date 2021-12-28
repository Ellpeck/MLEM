using Microsoft.Xna.Framework;
using MLEM.Animations;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A sprite animation image for use inside of a <see cref="UiSystem"/>.
    /// A sprite animation image is an <see cref="Image"/> that displays a <see cref="SpriteAnimation"/> or <see cref="SpriteAnimationGroup"/>.
    /// </summary>
    public class SpriteAnimationImage : Image {

        /// <summary>
        /// The sprite animation group that is displayed by this image
        /// </summary>
        public SpriteAnimationGroup Group;

        /// <summary>
        /// Creates a new sprite animation image with the given settings
        /// </summary>
        /// <param name="anchor">The image's anchor</param>
        /// <param name="size">The image's size</param>
        /// <param name="group">The sprite animation group to display</param>
        /// <param name="scaleToImage">Whether this image element should scale to the texture</param>
        public SpriteAnimationImage(Anchor anchor, Vector2 size, SpriteAnimationGroup group, bool scaleToImage = false) :
            base(anchor, size, group.CurrentRegion, scaleToImage) {
            this.Group = group;
        }

        /// <summary>
        /// Creates a new sprite animation image with the given settings
        /// </summary>
        /// <param name="anchor">The image's anchor</param>
        /// <param name="size">The image's size</param>
        /// <param name="animation">The sprite group to display</param>
        /// <param name="scaleToImage">Whether this image element should scale to the texture</param>
        public SpriteAnimationImage(Anchor anchor, Vector2 size, SpriteAnimation animation, bool scaleToImage = false) : this(anchor, size, new SpriteAnimationGroup().Add(animation, () => true), scaleToImage) {}

        /// <inheritdoc />
        public override void Update(GameTime time) {
            base.Update(time);
            this.Group.Update(time);
            this.Texture = this.Group.CurrentRegion;
        }

    }
}