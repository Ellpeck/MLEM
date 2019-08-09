using Microsoft.Xna.Framework;

namespace MLEM.Ui.Elements {
    public class VerticalSpace : Element {

        public VerticalSpace(int height) : base(Anchor.AutoCenter, new Vector2(1, height)) {
        }

    }
}