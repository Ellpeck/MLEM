using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Ui.Elements {
    public class Group : Element {

        public Group(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = true) : base(anchor, size) {
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.CanBeSelected = false;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            this.UpdateAreaIfDirty();
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

    }
}