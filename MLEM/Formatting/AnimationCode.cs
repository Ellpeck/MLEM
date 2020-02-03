using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting {
    public class AnimationCode : FormattingCode {

        public static readonly DrawCharacter Default = (code, settings, font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth) => {
            font.DrawString(batch, charSt, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };
        public static readonly DrawCharacter Wobbly = (code, settings, font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth) => {
            var offset = new Vector2(0, (float) Math.Sin(index + code.Time.TotalSeconds * settings.WobbleModifier) * font.LineHeight * settings.WobbleHeightModifier * scale);
            font.DrawString(batch, charSt, position + offset, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };
        public static readonly DrawCharacter Typing = (code, settings, font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth) => {
            if (code.Time.TotalSeconds * settings.TypingSpeed > index - effectStartIndex + 1)
                font.DrawString(batch, charSt, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };
        public static readonly AnimationCode DefaultCode = new AnimationCode(Default);

        public readonly DrawCharacter Draw;
        public TimeSpan Time;

        public AnimationCode(DrawCharacter draw) : base(Type.Animation) {
            this.Draw = draw;
        }

        public override void Update(GameTime time) {
            this.Time += time.ElapsedGameTime;
        }

        public override void Reset() {
            this.Time = TimeSpan.Zero;
        }

        public delegate void DrawCharacter(AnimationCode code, FormatSettings settings, IGenericFont font, SpriteBatch batch, string totalText, int index, int effectStartIndex, string charSt, Vector2 position, Color color, float scale, float layerDepth);

    }
}