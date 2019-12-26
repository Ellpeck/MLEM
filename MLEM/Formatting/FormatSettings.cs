using Microsoft.Xna.Framework;

namespace MLEM.Formatting {
    public class FormatSettings {

        public static readonly FormatSettings Default = new FormatSettings {
            WobbleModifier = 5,
            WobbleHeightModifier = 1 / 8F,
            TypingSpeed = 20,
            DropShadowColor = Color.Black,
            DropShadowOffset = new Vector2(2)
        };

        public float WobbleModifier;
        public float WobbleHeightModifier;
        public float TypingSpeed;
        public Color DropShadowColor;
        public Vector2 DropShadowOffset;

    }
}