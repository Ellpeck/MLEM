using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Formatting.Codes {
    public class Code : GenericDataHolder {

        public readonly Match Match;
        public Token Token { get; internal set; }

        public Code(Match match) {
            this.Match = match;
        }

        public virtual bool EndsHere(Code other) {
            return other.GetType() == this.GetType();
        }

        public virtual Color? GetColor() {
            return null;
        }

        public virtual GenericFont GetFont() {
            return null;
        }

        public virtual void Update(GameTime time) {
        }

        public virtual bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            return false;
        }

        public delegate Code Constructor(TextFormatter formatter, Match match);

    }
}