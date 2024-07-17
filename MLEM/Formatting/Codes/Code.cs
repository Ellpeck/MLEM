using System;
using System.Collections.Generic;
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
        /// The tokens that this formatting code is a part of.
        /// Note that this collection only has multiple entries if additional tokens have to be started while this code is still applied.
        /// </summary>
        public readonly List<Token> Tokens = new List<Token>();

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
        /// This is the opposite version of <see cref="EndsOther"/>.
        /// </summary>
        /// <param name="other">The code that is started here.</param>
        /// <returns>If this code should end here.</returns>
        public virtual bool EndsHere(Code other) {
            return other.GetType() == this.GetType();
        }

        /// <summary>
        /// Returns whether the <paramref name="other"/> <see cref="Code"/> should end when this formatting code starts.
        /// If this method returns true, a new <see cref="Token"/> is started at this code's position.
        /// This is the opposite version of <see cref="EndsHere"/>.
        /// </summary>
        /// <param name="other">The code that could end here.</param>
        /// <returns>Whether the <paramref name="other"/> code should end here.</returns>
        public virtual bool EndsOther(Code other) {
            return false;
        }

        /// <inheritdoc cref="Formatting.Token.GetColor"/>
        public virtual Color? GetColor(Color defaultPick) {
            return null;
        }

        /// <inheritdoc cref="Formatting.Token.GetFont"/>
        public virtual GenericFont GetFont(GenericFont defaultPick) {
            return null;
        }

        /// <inheritdoc cref="Token.GetSelfWidth"/>
        public virtual float GetSelfWidth(GenericFont font) {
            return 0;
        }

        /// <summary>
        /// Update this formatting code's animations etc.
        /// </summary>
        /// <param name="time">The game's time</param>
        public virtual void Update(GameTime time) {}

        /// <inheritdoc cref="Formatting.Token.DrawCharacter"/>
        public virtual bool DrawCharacter(GameTime time, SpriteBatch batch, int codePoint, string character, Token token, int indexInToken, Vector2 stringPos, ref Vector2 charPosOffset, GenericFont font, ref Color color, ref Vector2 scale, ref float rotation, ref Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize, Vector2 charSize) {
            return false;
        }

        /// <inheritdoc cref="Formatting.Token.DrawSelf"/>
        public virtual void DrawSelf(GameTime time, SpriteBatch batch, Token token, Vector2 stringPos, Vector2 charPosOffset, GenericFont font, Color color, Vector2 scale, float rotation, Vector2 origin, float depth, SpriteEffects effects, Vector2 stringSize) {}

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
