using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting {
    public struct FormatState {

        public static readonly DrawCharacter DefaultDrawBehavior = (state, batch, totalText, index, charSt, position, scale, layerDepth) => {
            state.Font.DrawString(batch, charSt, position, state.Color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };
        public FormattingCodeCollection FormattingCodes { get; internal set; }
        public IGenericFont RegularFont { get; internal set; }
        public IGenericFont ItalicFont { get; internal set; }
        public IGenericFont BoldFont { get; internal set; }
        public FormatSettings Settings { get; internal set; }
        public int CurrentIndex { get; internal set; }
        public Vector2 InnerOffset { get; internal set; }

        public Color Color;
        public IGenericFont Font;
        public DrawCharacter DrawBehavior;

        public void PrependBehavior(DrawCharacter behavior) {
            this.DrawBehavior = behavior + this.DrawBehavior;
        }

        public delegate void DrawCharacter(FormatState state, SpriteBatch batch, string totalText, int index, string charSt, Vector2 position, float scale, float layerDepth);

    }
}