using Microsoft.Xna.Framework;
using MLEM.Extensions;
using MonoGame.Extended;

namespace MLEM.Extended.Extensions {
    /// <summary>
    /// A set of extension methods that convert MLEM types to MonoGame.Extended types and vice versa.
    /// </summary>
    public static class NumberExtensions {

        /// <summary>
        /// Converts a MLEM <see cref="Misc.RectangleF"/> to a MonoGame.Extended <see cref="RectangleF"/>.
        /// </summary>
        /// <param name="rect">The rectangle to convert</param>
        /// <returns>The converted rectangle</returns>
        public static RectangleF ToExtended(this Misc.RectangleF rect) {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <summary>
        /// Converts a MonoGame.Extended <see cref="RectangleF"/> to a MLEM <see cref="Misc.RectangleF"/>.
        /// </summary>
        /// <param name="rect">The rectangle to convert</param>
        /// <returns>The converted rectangle</returns>
        public static Misc.RectangleF ToMlem(this RectangleF rect) {
            return new Misc.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        /// <inheritdoc cref="MLEM.Extensions.NumberExtensions.Penetrate"/>
        public static bool Penetrate(this RectangleF rect, RectangleF other, out Vector2 normal, out float penetration) {
            return rect.ToMlem().Penetrate(other.ToMlem(), out normal, out penetration);
        }

    }
}