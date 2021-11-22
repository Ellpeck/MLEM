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

        /// <summary>
        /// Calculates the amount that the rectangle <paramref name="rect"/> is penetrating the rectangle <paramref name="other"/> by.
        /// If a penetration on both axes is occuring, the one with the lower value is returned.
        /// This is useful for collision detection, as it can be used to push colliding objects out of each other.
        /// </summary>
        /// <param name="rect">The rectangle to do the penetration</param>
        /// <param name="other">The rectangle that should be penetrated</param>
        /// <param name="normal">The direction that the penetration occured in</param>
        /// <param name="penetration">The amount that the penetration occured by, in the direction of <paramref name="normal"/></param>
        /// <returns>Whether or not a penetration occured</returns>
        public static bool Penetrate(this RectangleF rect, RectangleF other, out Vector2 normal, out float penetration) {
            return rect.ToMlem().Penetrate(other.ToMlem(), out normal, out penetration);
        }

    }
}