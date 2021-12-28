using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Formatting.Codes {
    /// <summary>
    /// An instance of a formatting code that can be used for a <see cref="TextFormatter"/>.
    /// To add a new formatting code, see <see cref="TextFormatter.Codes"/>
    /// </summary>
    public class Code : GenericDataHolder {

        /// <summary>
        /// The regex that this code was created from
        /// </summary>
        public readonly Regex Regex;
        /// <summary>
        /// The match that this code encompasses
        /// </summary>
        public readonly Match Match;
        /// <summary>
        /// The token that this formatting code is a part of
        /// </summary>
        public Token Token { get; internal set; }

        /// <summary>
        /// Creates a new formatting code based on a formatting code regex and its match.
        /// </summary>
        /// <param name="match">The match</param>
        /// <param name="regex">The regex</param>
        protected Code(Match match, Regex regex) {
            this.Match = match;
            this.Regex = regex;
        }

        /// <summary>
        /// Returns whether this formatting code should end when the passed formatting code starts.
        /// If this method returns true, a new <see cref="Token"/> is started at its position.
        /// </summary>
        /// <param name="other">The code that is started here</param>
        /// <returns>If this code should end</returns>
        public virtual bool EndsHere(Code other) {
            return other.GetType() == this.GetType();
        }

        /// <inheritdoc cref="Formatting.Token.GetColor"/>
        public virtual Color? GetColor(Color defaultPick) {
            return null;
        }

        /// <inheritdoc cref="Formatting.Token.GetFont"/>
        public virtual GenericFont GetFont(GenericFont defaultPick) {
            return null;
        }

        /// <summary>
        /// Update this formatting code's animations etc.
        /// </summary>
        /// <param name="time">The game's time</param>
        public virtual void Update(GameTime time) {}

        /// <summary>
        /// Returns the string that this formatting code should be replaced with.
        /// Usually, you'll just want an empty string here, but some formatting codes (like <see cref="ImageCode"/>) require their space to be filled by spaces.
        /// </summary>
        /// <param name="font">The font that is used</param>
        /// <returns>The replacement string for this formatting code</returns>
        public virtual string GetReplacementString(GenericFont font) {
            return string.Empty;
        }

        /// <inheritdoc cref="Formatting.Token.DrawCharacter"/>
        public virtual bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            return false;
        }

        /// <inheritdoc cref="Formatting.Token.DrawSelf"/>
        public virtual void DrawSelf(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {}

        /// <summary>
        /// Creates a new formatting code from the given regex and regex match.
        /// <seealso cref="TextFormatter.Codes"/>
        /// </summary>
        /// <param name="formatter">The text formatter that created this code</param>
        /// <param name="match">The match for the code's regex</param>
        /// <param name="regex">The regex used to create this code</param>
        public delegate Code Constructor(TextFormatter formatter, Match match, Regex regex);

    }
}