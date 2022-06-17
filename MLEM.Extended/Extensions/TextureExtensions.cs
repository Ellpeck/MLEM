using MLEM.Extensions;
using MLEM.Textures;
using MonoGame.Extended.TextureAtlases;

namespace MLEM.Extended.Extensions {
    /// <summary>
    /// A set of extensions for converting texture-related types between MLEM and MonoGame.Extended.
    /// </summary>
    public static class TextureExtensions {

        /// <summary>
        /// Converts a MLEM <see cref="NinePatch"/> to a MonoGame.Extended <see cref="NinePatchRegion2D"/>.
        /// </summary>
        /// <param name="patch">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static NinePatchRegion2D ToExtended(this NinePatch patch) {
            return new NinePatchRegion2D(patch.Region.ToExtended(), patch.Padding.Left.Floor(), patch.Padding.Top.Floor(), patch.Padding.Right.Floor(), patch.Padding.Bottom.Floor());
        }

        /// <summary>
        /// Converts a MLEM <see cref="TextureRegion"/> to a MonoGame.Extended <see cref="TextureRegion2D"/>.
        /// </summary>
        /// <param name="region">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static TextureRegion2D ToExtended(this TextureRegion region) {
            return new TextureRegion2D(region.Name, region.Texture, region.U, region.V, region.Width, region.Height);
        }

        /// <summary>
        /// Converts a MonoGame.Extended <see cref="NinePatchRegion2D"/> to a MLEM <see cref="NinePatch"/>.
        /// </summary>
        /// <param name="patch">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static NinePatch ToMlem(this NinePatchRegion2D patch) {
            return new NinePatch(((TextureRegion2D) patch).ToMlem(), patch.LeftPadding, patch.RightPadding, patch.TopPadding, patch.BottomPadding);
        }

        /// <summary>
        /// Converts a MonoGame.Extended <see cref="TextureRegion2D"/> to a MLEM <see cref="TextureRegion"/>.
        /// </summary>
        /// <param name="region">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static TextureRegion ToMlem(this TextureRegion2D region) {
            return new TextureRegion(region.Texture, region.Bounds) {Name = region.Name};
        }

    }
}
