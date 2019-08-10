using Microsoft.Xna.Framework;

namespace MLEM.Ui.Elements {
    public class Group : Element {

        public Group(Anchor anchor, Vector2 size) : base(anchor, size) {
            this.IgnoresMouse = true;
        }

    }
}