using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using MLEM.Font;
using MLEM.Formatting.Codes;
using MLEM.Graphics;
using MLEM.Misc;

namespace MLEM.Formatting {
    /// <summary>
    /// A text formatter is used for drawing text using <see cref="GenericFont"/> that contains different colors, bold/italic sections and animations.
    /// To format a string of text, use the codes as specified in the constructor. To tokenize and render a formatted string, use <see cref="Tokenize"/>.
    /// </summary>
    public class TextFormatter : GenericDataHolder {

        /// <summary>
        /// The formatting codes that this text formatter uses.
        /// The <see cref="Regex"/> defines how the formatting code should be matched.
        /// </summary>
        public readonly Dictionary<Regex, Code.Constructor> Codes = new Dictionary<Regex, Code.Constructor>();
        /// <summary>
        /// The macros that this text formatter uses.
        /// A macro is a <see cref="Regex"/> that turns a snippet of text into another snippet of text.
        /// Macros can resolve recursively and can resolve into formatting codes.
        /// </summary>
        public readonly Dictionary<Regex, Macro> Macros = new Dictionary<Regex, Macro>();

        /// <summary>
        /// The line thickness used by this text formatter, which determines how the default <see cref="UnderlineCode"/>-based formatting codes are drawn.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float LineThickness = 1 / 16F;
        /// <summary>
        /// The underline offset used by this text formatter, which determines how the default <see cref="UnderlineCode"/> is drawn.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float UnderlineOffset = 0.85F;
        /// <summary>
        /// The strikethrough offset used by this text formatter, which determines how the default <see cref="UnderlineCode"/>'s strikethrough variant is drawn.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float StrikethroughOffset = 0.55F;
        /// <summary>
        /// The default subscript offset used by this text formatter, which determines how the default <see cref="SubSupCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float DefaultSubOffset = 0.15F;
        /// <summary>
        /// The default superscript offset used by this text formatter, which determines how the default <see cref="SubSupCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float DefaultSupOffset = -0.25F;
        /// <summary>
        /// The default shadow color used by this text formatter, which determines how the default <see cref="ShadowCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public Color DefaultShadowColor = Color.Black;
        /// <summary>
        /// The default shadow offset used by this text formatter, which determines how the default <see cref="ShadowCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public Vector2 DefaultShadowOffset = new Vector2(2);
        /// <summary>
        /// The default wobbly modifier used by this text formatter, which determines how the default <see cref="WobblyCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float DefaultWobblyModifier = 5;
        /// <summary>
        /// The default wobbly modifier used by this text formatter, which determines how the default <see cref="WobblyCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float DefaultWobblyHeight = 1 / 8F;
        /// <summary>
        /// The default outline thickness used by this text formatter, which determines how the default <see cref="OutlineCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public float DefaultOutlineThickness = 2;
        /// <summary>
        /// The default outline color used by this text formatter, which determines how the default <see cref="OutlineCode"/> is drawn if no custom value is used.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public Color DefaultOutlineColor = Color.Black;
        /// <summary>
        /// Whether the default outline used by this text formatter should also draw outlines diagonally, which determines how the default <see cref="OutlineCode"/> is drawn if no custom value is used. Non-diagonally drawn outlines might generally look better when using a pixelart font.
        /// Note that this value only has an effect on the default formatting codes created through the <see cref="TextFormatter(bool, bool, bool, bool)"/> constructor.
        /// </summary>
        public bool OutlineDiagonals = true;

        /// <summary>
        /// Creates a new text formatter with an optional set of default formatting codes.
        /// </summary>
        /// <param name="hasFontModifiers">Whether default font modifier codes should be added, including bold, italic, strikethrough, shadow, subscript, and more.</param>
        /// <param name="hasColors">Whether default color codes should be added, including all <see cref="Color"/> values and the ability to use custom colors.</param>
        /// <param name="hasAnimations">Whether default animation codes should be added, namely the wobbly animation.</param>
        /// <param name="hasMacros">Whether default macros should be added, including TeX's ~ non-breaking space and more.</param>
        public TextFormatter(bool hasFontModifiers = true, bool hasColors = true, bool hasAnimations = true, bool hasMacros = true) {
            // general font modifier codes
            if (hasFontModifiers) {
                this.Codes.Add(new Regex("<b>"), (f, m, r) => new FontCode(m, r, fnt => fnt.Bold));
                this.Codes.Add(new Regex("<i>"), (f, m, r) => new FontCode(m, r, fnt => fnt.Italic));
                this.Codes.Add(new Regex(@"<s(?: #([0-9\w]{6,8}) (([+-.0-9]*)))?>"), (f, m, r) => new ShadowCode(m, r,
                    ColorHelper.TryFromHexString(m.Groups[1].Value, out var color) ? color : this.DefaultShadowColor,
                    float.TryParse(m.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var offset) ? new Vector2(offset) : this.DefaultShadowOffset));
                this.Codes.Add(new Regex("<u>"), (f, m, r) => new UnderlineCode(m, r, this.LineThickness, this.UnderlineOffset));
                this.Codes.Add(new Regex("<st>"), (f, m, r) => new UnderlineCode(m, r, this.LineThickness, this.StrikethroughOffset));
                this.Codes.Add(new Regex(@"<sub(?: ([+-.0-9]+))?>"), (f, m, r) => new SubSupCode(m, r,
                    float.TryParse(m.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var off) ? off : this.DefaultSubOffset));
                this.Codes.Add(new Regex(@"<sup(?: ([+-.0-9]+))?>"), (f, m, r) => new SubSupCode(m, r,
                    float.TryParse(m.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var off) ? -off : this.DefaultSupOffset));
                this.Codes.Add(new Regex(@"<o(?: #([0-9\w]{6,8}) (([+-.0-9]*)))?>"), (f, m, r) => new OutlineCode(m, r,
                    ColorHelper.TryFromHexString(m.Groups[1].Value, out var color) ? color : this.DefaultOutlineColor,
                    float.TryParse(m.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var thickness) ? thickness : this.DefaultOutlineThickness,
                    this.OutlineDiagonals));
            }

            // color codes
            if (hasColors) {
                foreach (var c in typeof(Color).GetProperties()) {
                    if (c.GetGetMethod().IsStatic) {
                        var value = (Color) c.GetValue(null);
                        this.Codes.Add(new Regex($"<c {c.Name}>"), (f, m, r) => new ColorCode(m, r, value));
                    }
                }
                this.Codes.Add(new Regex(@"<c #([0-9\w]{6,8})>"), (f, m, r) => new ColorCode(m, r,
                    ColorHelper.TryFromHexString(m.Groups[1].Value, out var color) ? color : Color.Red));
            }

            // animation codes
            if (hasAnimations) {
                this.Codes.Add(new Regex("<a wobbly(?: ([+-.0-9]*) ([+-.0-9]*))?>"), (f, m, r) => new WobblyCode(m, r,
                    float.TryParse(m.Groups[1].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var mod) ? mod : this.DefaultWobblyModifier,
                    float.TryParse(m.Groups[2].Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var heightMod) ? heightMod : this.DefaultWobblyHeight));
            }

            // control codes
            this.Codes.Add(new Regex(@"</(\w+)>"), (f, m, r) => new SimpleEndCode(m, r, m.Groups[1].Value));

            // macros
            if (hasMacros) {
                this.Macros.Add(new Regex("~"), (f, m, r) => GenericFont.Nbsp.ToString());
                this.Macros.Add(new Regex("<n>"), (f, m, r) => '\n'.ToString());
            }
        }

        /// <summary>
        /// Tokenizes a string, returning a tokenized string that is ready for splitting, measuring and drawing.
        /// </summary>
        /// <param name="font">The font to use for tokenization. Note that this font needs to be the same that will later be used for splitting, measuring and/or drawing.</param>
        /// <param name="s">The string to tokenize</param>
        /// <param name="alignment">The text alignment that should be used. This alignment can later be changed using <see cref="TokenizedString.Realign"/>.</param>
        /// <returns>The tokenized string.</returns>
        public TokenizedString Tokenize(GenericFont font, string s, TextAlignment alignment = TextAlignment.Left) {
            // resolve macros
            s = this.ResolveMacros(s);
            var tokens = new List<Token>();
            var applied = new List<Code>();
            var allCodes = new List<Code>();
            // add the formatting code right at the start of the string
            var firstCode = this.GetNextCode(s, 0, 0);
            if (firstCode != null)
                applied.Add(firstCode);
            var index = 0;
            var rawIndex = 0;
            while (rawIndex < s.Length) {
                var next = this.GetNextCode(s, rawIndex + 1);
                // if we've reached the end of the string
                if (next == null) {
                    var sub = s.Substring(rawIndex, s.Length - rawIndex);
                    tokens.Add(new Token(applied.ToArray(), index, rawIndex, TextFormatter.StripFormatting(sub, applied.Select(c => c.Regex)), sub));
                    break;
                }
                allCodes.Add(next);

                // create a new token for the content up to the next code
                var ret = s.Substring(rawIndex, next.Match.Index - rawIndex);
                var strippedRet = TextFormatter.StripFormatting(ret, applied.Select(c => c.Regex));
                tokens.Add(new Token(applied.ToArray(), index, rawIndex, strippedRet, ret));

                // move to the start of the next code
                rawIndex = next.Match.Index;
                index += strippedRet.Length;

                // remove all codes that are incompatible with the next one and apply it
                applied.RemoveAll(c => c.EndsHere(next) || next.EndsOther(c));
                applied.Add(next);
            }
            return new TokenizedString(font, alignment, s, TextFormatter.StripFormatting(s, allCodes.Select(c => c.Regex)), tokens.ToArray(), allCodes.ToArray());
        }

        /// <summary>
        /// Resolves the macros in the given string recursively, until no more macros can be resolved.
        /// This method is used by <see cref="Tokenize"/>, meaning that it does not explicitly have to be called when using text formatting.
        /// </summary>
        /// <param name="s">The string to resolve macros for</param>
        /// <returns>The final, recursively resolved string</returns>
        public string ResolveMacros(string s) {
            // resolve macros that resolve into macros
            var rec = 0;
            var ret = s;
            bool matched;
            do {
                matched = false;
                foreach (var macro in this.Macros) {
                    ret = macro.Key.Replace(ret, m => {
                        // if the match evaluator was queried, then we know we matched something
                        matched = true;
                        return macro.Value(this, m, macro.Key);
                    });
                }
                rec++;
                if (rec >= 64)
                    throw new ArithmeticException($"A string resolved macros recursively too many times. Does it contain any conflicting macros?\nOriginal: {s}\nCurrent: {ret}");
            } while (matched);
            return ret;
        }

        /// <summary>
        /// Strips all formatting codes from the given string, causing a string without any formatting codes to be returned.
        /// Note that, if a <see cref="TokenizedString"/> has already been created using <see cref="Tokenize"/>, it is more efficient to use <see cref="TokenizedString.String"/> or <see cref="TokenizedString.DisplayString"/>.
        /// </summary>
        /// <param name="s">The string to strip formatting codes from.</param>
        /// <returns>The stripped string.</returns>
        public string StripAllFormatting(string s) {
            return TextFormatter.StripFormatting(s, this.Codes.Keys);
        }

        private Code GetNextCode(string s, int index, int maxIndex = int.MaxValue) {
            var (constructor, match, regex) = this.Codes
                .Select(kv => (Constructor: kv.Value, Match: kv.Key.Match(s, index), Regex: kv.Key))
                .Where(kv => kv.Match.Success && kv.Match.Index <= maxIndex)
                .OrderBy(kv => kv.Match.Index)
                .FirstOrDefault();
            return constructor?.Invoke(this, match, regex);
        }

        private static string StripFormatting(string s, IEnumerable<Regex> codes) {
            foreach (var code in codes)
                s = code.Replace(s, string.Empty);
            return s;
        }

        /// <summary>
        /// Represents a text formatting macro. Used by <see cref="TextFormatter.Macros"/>.
        /// </summary>
        /// <param name="formatter">The text formatter that created this macro</param>
        /// <param name="match">The match for the macro's regex</param>
        /// <param name="regex">The regex used to create this macro</param>
        public delegate string Macro(TextFormatter formatter, Match match, Regex regex);

    }
}
