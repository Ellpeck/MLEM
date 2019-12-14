using MLEM.Extensions;
using MLEM.Textures;
using MonoGame.Extended.TextureAtlases;

namespace MLEM.Extended.Extensions {
    public static class TextureExtensions {

        public static NinePatchRegion2D ToExtended(this NinePatch patch) {
            return new NinePatchRegion2D(patch.Region.ToExtended(), patch.Padding.Left.Floor(), patch.Padding.Top.Floor(), patch.Padding.Right.Floor(), patch.Padding.Bottom.Floor());
        }

        public static TextureRegion2D ToExtended(this TextureRegion region) {
            return new TextureRegion2D(region.Texture, region.U, region.V, region.Width, region.Height);
        }

        public static NinePatch ToMlem(this NinePatchRegion2D patch) {
            return new NinePatch(new TextureRegion(patch.Texture, patch.Bounds), patch.LeftPadding, patch.RightPadding, patch.TopPadding, patch.BottomPadding);
        }

        public static TextureRegion ToMlem(this TextureRegion2D region) {
            return new TextureRegion(region.Texture, region.Bounds);
        }

    }
}