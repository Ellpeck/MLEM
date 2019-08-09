using System;
using Microsoft.Xna.Framework;

namespace MLEM.Extensions {
    public static class ColorExtensions {

        public static Color Invert(this Color color) {
            return new Color(Math.Abs(255 - color.R), Math.Abs(255 - color.G), Math.Abs(255 - color.B), color.A);
        }

        public static Color FromHex(uint value) {
            return new Color((int) (value >> 16 & 0xFF), (int) (value >> 8 & 0xFF), (int) (value >> 0 & 0xFF), (int) (value >> 24 & 0xFF));
        }

        public static Color CopyAlpha(this Color color, Color other) {
            return color * (other.A / 255F);
        }

    }
}