using Microsoft.Xna.Framework;
using MLEM.Animations;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class SpriteAnimationImage : Image {

        public SpriteAnimationGroup Group;

        public SpriteAnimationImage(Anchor anchor, Vector2 size, TextureRegion texture, SpriteAnimationGroup group, bool scaleToImage = false) :
            base(anchor, size, texture, scaleToImage) {
            this.Group = group;
        }

        public SpriteAnimationImage(Anchor anchor, Vector2 size, TextureRegion texture, SpriteAnimation animation, bool scaleToImage = false) :
            this(anchor, size, texture, new SpriteAnimationGroup().Add(animation, () => true), scaleToImage) {
        }

        public override void Update(GameTime time) {
            base.Update(time);
            this.Group.Update(time);
            this.Texture = this.Group.CurrentRegion;
        }

    }

}