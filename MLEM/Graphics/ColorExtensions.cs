using System;
using System.Globalization;
using Microsoft.Xna.Framework;

namespace MLEM.Graphics {
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

        /// <summary>
        /// Converts the given <paramref name="color"/> into HSL representation and returns the result as a value tuple with values between 0 and 1.
        /// </summary>
        /// <remarks>This code is adapted from https://gist.github.com/mjackson/5311256.</remarks>
        /// <param name="color">The color to convert.</param>
        /// <returns>The resulting HSL color as a value tuple containing H, S and L, each between 0 and 1.</returns>
        public static (float H, float S, float L) ToHsl(this Color color) {
            var r = color.R / 255F;
            var g = color.G / 255F;
            var b = color.B / 255F;

            var max = Math.Max(Math.Max(r, g), b);
            var min = Math.Min(Math.Min(r, g), b);
            float h, s, l = (max + min) / 2;

            if (max == min) {
                h = s = 0; // achromatic
            } else {
                var d = max - min;
                s = l > 0.5F ? d / (2 - max - min) : d / (max + min);
                if (r == max) {
                    h = (g - b) / d + (g < b ? 6 : 0);
                } else if (g == max) {
                    h = (b - r) / d + 2;
                } else {
                    h = (r - g) / d + 4;
                }
                h /= 6;
            }

            return (h, s, l);
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

        /// <summary>
        ///Converts the given HSL color to an RGB <see cref="Color"/> and returns the result.
        /// </summary>
        /// <remarks>This code is adapted from https://gist.github.com/mjackson/5311256.</remarks>
        /// <param name="color">The HSL color to convert, as a value tuple that contains H, S and L values, each between 0 and 1.</param>
        /// <returns>The resulting RGB color.</returns>
        public static Color FromHsl((float H, float S, float L) color) {
            return ColorHelper.FromHsl(color.H, color.S, color.L);
        }

        /// <summary>
        ///Converts the given HSL values to an RGB <see cref="Color"/> and returns the result.
        /// </summary>
        /// <remarks>This code is adapted from https://gist.github.com/mjackson/5311256.</remarks>
        /// <param name="h">The H component of the HSL color, between 0 and 1.</param>
        /// <param name="s">The S component of the HSL color, between 0 and 1.</param>
        /// <param name="l">The L component of the HSL color, between 0 and 1.</param>
        /// <returns>The resulting RGB color.</returns>
        public static Color FromHsl(float h, float s, float l) {
            float r, g, b;

            if (s == 0) {
                r = g = b = l; // achromatic
            } else {
                var q = l < 0.5F ? l * (1 + s) : l + s - l * s;
                var p = 2 * l - q;

                r = ColorHelper.HueToRgb(p, q, h + 1 / 3F);
                g = ColorHelper.HueToRgb(p, q, h);
                b = ColorHelper.HueToRgb(p, q, h - 1 / 3F);
            }

            return new Color(r, g, b);
        }

        private static float HueToRgb(float p, float q, float t) {
            if (t < 0)
                t += 1;
            if (t > 1)
                t -= 1;
            if (t < 1 / 6F)
                return p + (q - p) * 6 * t;
            if (t < 1 / 2F)
                return q;
            if (t < 2 / 3F)
                return p + (q - p) * (2 / 3F - t) * 6;
            return p;
        }

    }
}
