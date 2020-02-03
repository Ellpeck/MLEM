using Microsoft.Xna.Framework;
using MLEM.Animations;
using MLEM.Font;
using MLEM.Textures;

namespace MLEM.Formatting {
    public class FormattingCode {

        public readonly Type CodeType;
        public readonly Color Color;
        public readonly TextStyle Style;
        public readonly SpriteAnimation Icon;

        protected FormattingCode(Type type) {
            this.CodeType = type;
        }

        public FormattingCode(Color color) : this(Type.Color) {
            this.Color = color;
        }

        public FormattingCode(TextStyle style) : this(Type.Style) {
            this.Style = style;
        }

        public FormattingCode(TextureRegion icon) :
            this(new SpriteAnimation(0, icon)) {
        }

        public FormattingCode(SpriteAnimation icon) : this(Type.Icon) {
            this.Icon = icon;
        }

        public virtual string GetReplacementString(IGenericFont font) {
            return this.CodeType == Type.Icon ? TextFormatting.GetOneEmString(font) : string.Empty;
        }

        public virtual void Update(GameTime time) {
            if (this.CodeType == Type.Icon)
                this.Icon.Update(time);
        }

        public virtual void Reset() {
            if (this.CodeType == Type.Icon)
                this.Icon.Restart();
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