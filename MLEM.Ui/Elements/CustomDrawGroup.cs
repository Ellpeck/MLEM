using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Ui.Elements {
    public class CustomDrawGroup : Group {

        public Matrix? Transform;
        public TransformCallback TransformGetter;
        public BeginDelegate BeginImpl;

        public CustomDrawGroup(Anchor anchor, Vector2 size, TransformCallback transformGetter = null, BeginDelegate beginImpl = null, bool setHeightBasedOnChildren = true) :
            base(anchor, size, setHeightBasedOnChildren) {
            this.TransformGetter = transformGetter ?? ((element, time, matrix) => Matrix.Identity);
            this.BeginImpl = beginImpl ?? ((element, time, batch, alpha, blendState, samplerState, matrix) => batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix));
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            // end the usual draw so that we can begin our own
            batch.End();
            var trans = this.Transform ?? this.TransformGetter(this, time, matrix);
            var mat = matrix * trans;
            this.BeginImpl(this, time, batch, alpha, blendState, samplerState, mat);
            // draw child components in custom begin call
            base.Draw(time, batch, alpha, blendState, samplerState, mat);
            // end our draw
            batch.End();
            // begin the usual draw again for other elements
            batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix);
        }

        public delegate void BeginDelegate(CustomDrawGroup element, GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix);

        public delegate Matrix TransformCallback(CustomDrawGroup element, GameTime time, Matrix matrix);

    }
}