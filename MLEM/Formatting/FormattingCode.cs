using Microsoft.Xna.Framework;
using MLEM.Animations;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Textures;

namespace MLEM.Formatting {
    public class FormattingCode {

        public readonly Type CodeType;
        public readonly Color Color;
        public readonly TextStyle Style;
        public readonly SpriteAnimation Icon;
        public readonly TextAnimation.DrawCharacter Animation;

        public FormattingCode(Color color) {
            this.Color = color;
            this.CodeType = Type.Color;
        }

        public FormattingCode(TextStyle style) {
            this.Style = style;
            this.CodeType = Type.Style;
        }

        public FormattingCode(TextureRegion icon) :
            this(new SpriteAnimation(0, icon)) {
        }

        public FormattingCode(SpriteAnimation icon) {
            this.Icon = icon;
            this.CodeType = Type.Icon;
        }

        public FormattingCode(TextAnimation.DrawCharacter animation) {
            this.Animation = animation;
            this.CodeType = Type.Animation;
        }

        public virtual string GetReplacementString(IGenericFont font) {
            return this.CodeType == Type.Icon ? SpriteFontExtensions.GetWidthString(font, font.LineHeight) : string.Empty;
        }

        public enum Type {

            Color,
            Style,
            Icon,
            Animation

        }

    }

    public enum TextStyle {

        Regular,
        Bold,
        Italic,
        Shadow

    }
}