using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Misc;

namespace MLEM.Formatting.Codes {
    public class Code : GenericDataHolder {

        public readonly Regex Regex;
        public readonly Match Match;
        public Token Token { get; internal set; }

        protected Code(Match match, Regex regex) {
            this.Match = match;
            this.Regex = regex;
        }

        public virtual bool EndsHere(Code other) {
            return other.GetType() == this.GetType();
        }

        public virtual Color? GetColor(Color defaultPick) {
            return null;
        }

        public virtual GenericFont GetFont(GenericFont defaultPick) {
            return null;
        }

        public virtual void Update(GameTime time) {
        }

        public virtual string GetReplacementString(GenericFont font) {
            return string.Empty;
        }

        public virtual bool DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, int indexInToken, ref Vector2 pos, GenericFont font, ref Color color, ref float scale, float depth) {
            return false;
        }

        public virtual void DrawSelf(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
        }

        public delegate Code Constructor(TextFormatter formatter, Match match, Regex regex);

    }
}