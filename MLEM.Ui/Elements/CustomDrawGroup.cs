using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Ui.Elements {
    public class CustomDrawGroup : Group {

        public Matrix? Transform;
        public TransformCallback TransformGetter;
        private BeginDelegate beginImpl;
        private bool isDefaultBegin;
        public BeginDelegate BeginImpl {
            get => this.beginImpl;
            set {
                this.beginImpl = value ?? ((element, time, batch, alpha, blendState, samplerState, matrix) => batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix));
                this.isDefaultBegin = value == null;
            }
        }

        public CustomDrawGroup(Anchor anchor, Vector2 size, TransformCallback transformGetter = null, BeginDelegate beginImpl = null, bool setHeightBasedOnChildren = true) :
            base(anchor, size, setHeightBasedOnChildren) {
            this.TransformGetter = transformGetter ?? ((element, time, matrix) => Matrix.Identity);
            this.BeginImpl = beginImpl;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var transform = this.Transform ?? this.TransformGetter(this, time, matrix);
            var customDraw = !this.isDefaultBegin || transform != Matrix.Identity;
            var mat = matrix * transform;
            if (customDraw) {
                // end the usual draw so that we can begin our own
                batch.End();
                this.BeginImpl(this, time, batch, alpha, blendState, samplerState, mat);
            }
            // draw child components in custom begin call
            base.Draw(time, batch, alpha, blendState, samplerState, mat);
            if (customDraw) {
                // end our draw
                batch.End();
                // begin the usual draw again for other elements
                batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix);
            }
        }

        public void ScaleOrigin(float scale, Vector2? origin = null) {
            this.Transform = Matrix.CreateScale(scale, scale, 0) * Matrix.CreateTranslation(new Vector3((1 - scale) * (origin ?? this.DisplayArea.Center), 0));
        }

        public delegate void BeginDelegate(CustomDrawGroup element, GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix);

        public delegate Matrix TransformCallback(CustomDrawGroup element, GameTime time, Matrix matrix);

    }
}