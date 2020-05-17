using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Formatting.Codes;
using MLEM.Misc;

namespace MLEM.Formatting {
    public class Token : GenericDataHolder {

        public readonly Code[] AppliedCodes;
        public readonly int Index;
        public readonly int RawIndex;
        public readonly string Substring;
        public string DisplayString => this.SplitSubstring ?? this.Substring;
        public readonly string RawSubstring;
        internal RectangleF[] Area;
        internal string SplitSubstring;

        public Token(Code[] appliedCodes, int index, int rawIndex, string substring, string rawSubstring) {
            this.AppliedCodes = appliedCodes;
            this.Index = index;
            this.RawIndex = rawIndex;
            this.Substring = substring;
            this.RawSubstring = rawSubstring;

            foreach (var code in appliedCodes)
                code.Token = this;
        }

        public Color? GetColor(Color defaultPick) {
            return this.AppliedCodes.Select(c => c.GetColor(defaultPick)).FirstOrDefault(c => c.HasValue);
        }

        public GenericFont GetFont(GenericFont defaultPick) {
            return this.AppliedCodes.Select(c => c.GetFont(defaultPick)).FirstOrDefault(f => f != null);
        }

        public void DrawSelf(GameTime time, SpriteBatch batch, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            foreach (var code in this.AppliedCodes)
                code.DrawSelf(time, batch, pos, font, color, scale, depth);
        }

        public void DrawCharacter(GameTime time, SpriteBatch batch, char c, string cString, int indexInToken, Vector2 pos, GenericFont font, Color color, float scale, float depth) {
            foreach (var code in this.AppliedCodes) {
                if (code.DrawCharacter(time, batch, c, cString, indexInToken, ref pos, font, ref color, ref scale, depth))
                    return;
            }

            // if no code drew, we have to do it ourselves
            font.DrawString(batch, cString, pos, color, 0, Vector2.Zero, scale, SpriteEffects.None, depth);
        }

        public IEnumerable<RectangleF> GetArea(Vector2 stringPos, float scale) {
            return this.Area.Select(a => new RectangleF(stringPos + a.Location * scale, a.Size * scale));
        }

    }
}