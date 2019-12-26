using Microsoft.Xna.Framework;

namespace MLEM.Formatting {
    public class FormatSettings {

        public static readonly FormatSettings Default = new FormatSettings();

        public float WobbleModifier;
        public float WobbleHeightModifier;
        public float TypingSpeed;
        public Color DropShadowColor;
        public Vector2 DropShadowOffset;

        public FormatSettings() {
            this.WobbleModifier = 5;
            this.WobbleHeightModifier = 1 / 8F;
            this.TypingSpeed = 20;
            this.DropShadowColor = Color.Black;
            this.DropShadowOffset = new Vector2(2);
        }

    }
}