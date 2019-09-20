using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Ui.Elements {
    public class CustomDrawGroup : Group {

        public Matrix Transform;
        public BeginDelegate BeginImpl;

        public CustomDrawGroup(Anchor anchor, Vector2 size, Matrix? transform = null, BeginDelegate beginImpl = null, bool setHeightBasedOnChildren = true) :
            base(anchor, size, setHeightBasedOnChildren) {
            this.Transform = transform ?? Matrix.Identity;
            this.BeginImpl = beginImpl ?? ((element, time, batch, alpha, blendState, samplerState, matrix) => batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix));
        }

        public delegate void BeginDelegate(CustomDrawGroup element, GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix);

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            // end the usual draw so that we can begin our own
            batch.End();
            var mat = matrix * this.Transform;
            this.BeginImpl(this, time, batch, alpha, blendState, samplerState, mat);
            // draw child components in custom begin call
            base.Draw(time, batch, alpha, blendState, samplerState, mat);
            // end our draw
            batch.End();
            // begin the usual draw again for other elements
            batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix);
        }

    }
}