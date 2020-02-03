using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace MLEM.Formatting {
    public static class TextAnimation {

        public static readonly DrawCharacter Default = (settings, font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth, timeIntoAnimation) => {
            font.DrawString(batch, charSt, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };

        public static readonly DrawCharacter Wobbly = (settings, font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth, timeIntoAnimation) => {
            var offset = new Vector2(0, (float) Math.Sin(index + timeIntoAnimation.TotalSeconds * settings.WobbleModifier) * font.LineHeight * settings.WobbleHeightModifier * scale);
            font.DrawString(batch, charSt, position + offset, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };

        public static readonly DrawCharacter Typing = (settings, font, batch, totalText, index, effectStartIndex, charSt, position, color, scale, layerDepth, timeIntoAnimation) => {
            if (timeIntoAnimation.TotalSeconds * settings.TypingSpeed > index - effectStartIndex + 1)
                font.DrawString(batch, charSt, position, color, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth);
        };

        public delegate void DrawCharacter(FormatSettings settings, IGenericFont font, SpriteBatch batch, string totalText, int index, int effectStartIndex, string charSt, Vector2 position, Color color, float scale, float layerDepth, TimeSpan timeIntoAnimation);

    }
}