using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting {
    public static class TextAnimation {

        public static float WobbleModifier = 5;
        public static float WobbleHeightModifier = 1 / 8F;
        public static float TypingSpeed = 20;

        public static readonly DrawCharacter Default = (font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth, timeIntoAnimation) => {
            font.DrawString(batch, charSt, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };

        public static readonly DrawCharacter Wobbly = (font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth, timeIntoAnimation) => {
            var offset = new Vector2(0, (float) Math.Sin(index + timeIntoAnimation.TotalSeconds * WobbleModifier) * font.LineHeight * WobbleHeightModifier * scale);
            font.DrawString(batch, charSt, position + offset, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };

        public static readonly DrawCharacter Typing = (font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth, timeIntoAnimation) => {
            if (timeIntoAnimation.TotalSeconds * TypingSpeed > index - effectStartIndex + 1)
                font.DrawString(batch, charSt, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };

        public delegate void DrawCharacter(IGenericFont font, SpriteBatch batch, string totalText, int index, int effectStartIndex, string charSt, Vector2 position, Color color, float scale, float layerDepth, TimeSpan timeIntoAnimation);

    }
}