using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="Color"/> objects.
    /// </summary>
    public static class ColorExtensions {

        /// <summary>
        /// Copies the alpha value from another color into this color and returns the result.
        /// </summary>
        /// <param name="color">The color.</param>
        /// <param name="other">The color to copy the alpha from.</param>
        /// <returns>The first color with the second color's alpha value.</returns>
        public static Color CopyAlpha(this Color color, Color other) {
            return color * (other.A / 255F);
        }

        /// <summary>
        /// Returns an inverted version of this color.
        /// </summary>
        /// <param name="color">The color to invert.</param>
        /// <returns>The inverted color.</returns>
        public static Color Invert(this Color color) {
            return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
        }

        /// <summary>
        /// Multiplies this color with another color and returns the result.
        /// </summary>
        /// <param name="color">The first color.</param>
        /// <param name="other">The second color.</param>
        /// <returns>The two colors multiplied together.</returns>
        public static Color Multiply(this Color color, Color other) {
            return new Color(color.ToVector4() * other.ToVector4());
        }

        /// <summary>
        /// Returns the hexadecimal representation of this color as a string in the format <c>#AARRGGBB</c>, or optionally <c>AARRGGBB</c>, without the pound symbol.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <param name="hash">Whether a # should prepend the string.</param>
        /// <returns>The resulting hex string.</returns>
        public static string ToHexStringRgba(this Color color, bool hash = true) {
            return $"{(hash ? "#" : string.Empty)}{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        /// <summary>
        /// Returns the hexadecimal representation of this color as a string in the format <c>#RRGGBB</c>, or optionally <c>RRGGBB</c>, without the pound symbol.
        /// The alpha channel is ignored.
        /// </summary>
        /// <param name="color">The color to convert.</param>
        /// <param name="hash">Whether a # should prepend the string.</param>
        /// <returns>The resulting hex string.</returns>
        public static string ToHexStringRgb(this Color color, bool hash = true) {
            return $"{(hash ? "#" : string.Empty)}{color.R:X2}{color.G:X2}{color.B:X2}";
        }

    }

    /// <summary>
    /// A set of utility methods for dealing with <see cref="Color"/> objects.
    /// </summary>
    public static class ColorHelper {

        /// <summary>
        /// Parses a hexadecimal number into an rgba color.
        /// The number should be in the format <c>0xaarrggbb</c>.
        /// </summary>
        /// <param name="value">The number to parse.</param>
        /// <returns>The resulting color.</returns>
        public static Color FromHexRgba(int value) {
            return new Color(value >> 16 & 0xFF, value >> 8 & 0xFF, value >> 0 & 0xFF, value >> 24 & 0xFF);
        }

        /// <summary>
        /// Parses a hexadecimal number into an rgb color with 100% alpha.
        /// The number should be in the format <c>0xrrggbb</c>.
        /// </summary>
        /// <param name="value">The number to parse.</param>
        /// <returns>The resulting color.</returns>
        public static Color FromHexRgb(int value) {
            return new Color(value >> 16 & 0xFF, value >> 8 & 0xFF, value >> 0 & 0xFF);
        }

        /// <summary>
        /// Parses a hexadecimal string into a color and throws a <see cref="FormatException"/> if parsing fails.
        /// The string can either be formatted as <c>RRGGBB</c> or <c>AARRGGBB</c> and can optionally start with a <c>#</c>.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <returns>The resulting color.</returns>
        /// <exception cref="FormatException">Thrown if parsing fails.</exception>
        public static Color FromHexString(string value) {
            if (!ColorHelper.TryFromHexString(value, out var val))
                throw new FormatException($"Cannot parse hex string {value}");
            return val;
        }

        /// <summary>
        /// Tries to parse a hexadecimal string into a color and returns whether a color was successfully parsed.
        /// The string can either be formatted as <c>RRGGBB</c> or <c>AARRGGBB</c> and can optionally start with a <c>#</c>.
        /// </summary>
        /// <param name="value">The string to parse.</param>
        /// <param name="color">The resulting color.</param>
        /// <returns>Whether parsing was successful.</returns>
        public static bool TryFromHexString(string value, out Color color) {
            if (value.StartsWith("#"))
                value = value.Substring(1);
            if (int.TryParse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var val)) {
                color = value.Length > 6 ? ColorHelper.FromHexRgba(val) : ColorHelper.FromHexRgb(val);
                return true;
            }
            color = default;
            return false;
        }

    }
}
