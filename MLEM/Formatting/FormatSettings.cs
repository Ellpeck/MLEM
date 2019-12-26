using Microsoft.Xna.Framework;

namespace MLEM.Formatting {
    public class FormatSettings {

        public static readonly FormatSettings Default = new FormatSettings();

        public float WobbleModifier = 5;
        public float WobbleHeightModifier = 1 / 8F;
        public float TypingSpeed = 20;
        public Color DropShadowColor = Color.Black;
        public Vector2 DropShadowOffset = new Vector2(2);

    }
}