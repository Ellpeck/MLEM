using MLEM.Extensions;
using MLEM.Textures;
using MonoGame.Extended.Graphics;
using NinePatch = MLEM.Textures.NinePatch;
using ExtNinePatch = MonoGame.Extended.Graphics.NinePatch;

namespace MLEM.Extended.Extensions {
    /// <summary>
    /// A set of extensions for converting texture-related types between MLEM and MonoGame.Extended.
    /// </summary>
    public static class TextureExtensions {

        /// <summary>
        /// Converts a MLEM <see cref="Textures.NinePatch"/> to a MonoGame.Extended <see cref="ExtNinePatch"/>.
        /// </summary>
        /// <param name="patch">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static ExtNinePatch ToExtended(this NinePatch patch) {
            return patch.Region.ToExtended().CreateNinePatch(patch.Padding.Left.Floor(), patch.Padding.Top.Floor(), patch.Padding.Right.Floor(), patch.Padding.Bottom.Floor());
        }

        /// <summary>
        /// Converts a MLEM <see cref="TextureRegion"/> to a MonoGame.Extended <see cref="Texture2DRegion"/>.
        /// </summary>
        /// <param name="region">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static Texture2DRegion ToExtended(this TextureRegion region) {
            return new Texture2DRegion(region.Texture, region.U, region.V, region.Width, region.Height, region.Name);
        }

        /// <summary>
        /// Converts a MonoGame.Extended <see cref="Texture2DRegion"/> to a MLEM <see cref="TextureRegion"/>.
        /// </summary>
        /// <param name="region">The nine patch to convert</param>
        /// <returns>The converted nine patch</returns>
        public static TextureRegion ToMlem(this Texture2DRegion region) {
            return new TextureRegion(region.Texture, region.Bounds) {Name = region.Name};
        }

    }
}
