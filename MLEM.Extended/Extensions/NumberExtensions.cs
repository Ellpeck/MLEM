using MonoGame.Extended;

namespace MLEM.Extended.Extensions {
    public static class NumberExtensions {

        public static RectangleF ToExtended(this Misc.RectangleF rect) {
            return new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

        public static Misc.RectangleF ToMlem(this RectangleF rect) {
            return new Misc.RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
        }

    }
}