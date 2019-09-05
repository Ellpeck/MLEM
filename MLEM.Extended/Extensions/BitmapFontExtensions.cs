using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MonoGame.Extended.BitmapFonts;

namespace MLEM.Extended.Extensions {
    public static class BitmapFontExtensions {

        public static string SplitString(this BitmapFont font, string text, float width, float scale) {
            return SpriteFontExtensions.SplitString(s => font.MeasureString(s).Width, text, width, scale);
        }

        public static string TruncateString(this BitmapFont font, string text, float width, float scale, bool fromBack = false) {
            return SpriteFontExtensions.TruncateString(s => font.MeasureString(s).Width, text, width, scale, fromBack);
        }

    }
}