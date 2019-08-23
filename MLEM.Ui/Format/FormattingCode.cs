using Microsoft.Xna.Framework;

namespace MLEM.Ui.Format {
    public class FormattingCode {

        public readonly Color Color;
        public readonly TextStyle Style;
        public readonly bool IsColorCode;

        public FormattingCode(Color color) {
            this.Color = color;
            this.IsColorCode = true;
        }

        public FormattingCode(TextStyle style) {
            this.Style = style;
            this.IsColorCode = false;
        }

    }

    public enum TextStyle {

        Regular,
        Bold,
        Italic

    }
}