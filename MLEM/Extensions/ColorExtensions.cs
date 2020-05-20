using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace MLEM.Extensions {
    public static class ColorExtensions {

        /// <summary>
        /// Returns an inverted version of the color.
        /// </summary>
        /// <param name="color">The color to invert</param>
        /// <returns>The inverted color</returns>
        public static Color Invert(this Color color) {
            return new Color(Math.Abs(255 - color.R), Math.Abs(255 - color.G), Math.Abs(255 - color.B), color.A);
        }

        /// <summary>
        /// Parses a hexadecimal number into a color.
        /// The number should be in the format <c>0xaarrggbb</c>.
        /// </summary>
        /// <param name="value">The number to parse</param>
        /// <returns>The resulting color</returns>
        public static Color FromHex(uint value) {
            return new Color((int) (value >> 16 & 0xFF), (int) (value >> 8 & 0xFF), (int) (value >> 0 & 0xFF), (int) (value >> 24 & 0xFF));
        }

        /// <summary>
        /// Parses a hexadecimal string into a color.
        /// The string can optionally start with a <c>#</c>.
        /// </summary>
        /// <param name="value">The string to parse</param>
        /// <returns>The resulting color</returns>
        public static Color FromHex(string value) {
            if (value.StartsWith("#"))
                value = value.Substring(1);
            return FromHex(uint.Parse(value, NumberStyles.HexNumber));
        }

        /// <summary>
        /// Copies the alpha value from <see cref="other"/> into this color.
        /// </summary>
        /// <param name="color">The color</param>
        /// <param name="other">The color to copy the alpha from</param>
        /// <returns>The <see cref="color"/> with <see cref="other"/>'s alpha value</returns>
        public static Color CopyAlpha(this Color color, Color other) {
            return color * (other.A / 255F);
        }

    }
}