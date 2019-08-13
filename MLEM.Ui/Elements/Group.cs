using Microsoft.Xna.Framework;

namespace MLEM.Ui.Elements {
    public class Group : Element {

        public Group(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = true) : base(anchor, size) {
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.IgnoresMouse = true;
        }

    }
}