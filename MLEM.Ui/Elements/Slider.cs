using Microsoft.Xna.Framework;

namespace MLEM.Ui.Elements {
    public class Slider : ScrollBar {

        public Slider(Anchor anchor, Vector2 size, int scrollerSize, float maxValue) :
            base(anchor, size, scrollerSize, maxValue, true) {
        }

    }
}