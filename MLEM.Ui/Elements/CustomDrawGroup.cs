using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A <see cref="Group"/> that can have custom drawing parameters.
    /// Custom drawing parameters include a <see cref="Transform"/> matrix, as well as a custom <see cref="SpriteBatch.Begin"/> call.
    /// All <see cref="Element.Children"/> of the custom draw group will be drawn with the custom parameters.
    /// </summary>
    public class CustomDrawGroup : Group {

        /// <summary>
        /// This custom draw group's transform matrix
        /// </summary>
        public Matrix? Transform;
        /// <summary>
        /// A callback for retrieving this group's <see cref="Transform"/> automatically
        /// </summary>
        public TransformCallback TransformGetter;
        private BeginDelegate beginImpl;
        private bool isDefaultBegin;
        /// <summary>
        /// The call that this custom draw group should make to <see cref="SpriteBatch"/> to begin drawing.
        /// </summary>
        public BeginDelegate BeginImpl {
            get => this.beginImpl;
            set {
                this.beginImpl = value ?? ((element, time, batch, alpha, blendState, samplerState, matrix) => batch.Begin(SpriteSortMode.Deferred, blendState, samplerState, null, null, null, matrix));
                this.isDefaultBegin = value == null;
            }
        }

        /// <summary>
        /// Creates a new custom draw group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="transformGetter">The group's <see cref="TransformGetter"/></param>
        /// <param name="beginImpl">The group's <see cref="BeginImpl"/></param>
        /// <param name="setHeightBasedOnChildren">Whether this group should automatically calculate its height based on its children</param>
        public CustomDrawGroup(Anchor anchor, Vector2 size, TransformCallback transformGetter = null, BeginDelegate beginImpl = null, bool setHeightBasedOnChildren = true) :
            base(anchor, size, setHeightBasedOnChildren) {
            this.TransformGetter = transformGetter ?? ((element, time, matrix) => Matrix.Identity);
            this.BeginImpl = beginImpl;
        }

        /// <inheritdoc />
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

        /// <summary>
        /// Scales this custom draw group's <see cref="Transform"/> matrix based on the given scale and origin.
        /// </summary>
        /// <param name="scale">The scale to use</param>
        /// <param name="origin">The origin to use for scaling, or null to use this element's center point</param>
        public void ScaleOrigin(float scale, Vector2? origin = null) {
            this.Transform = Matrix.CreateScale(scale, scale, 1) * Matrix.CreateTranslation(new Vector3((1 - scale) * (origin ?? this.DisplayArea.Center), 0));
        }

        /// <summary>
        /// A delegate method used for <see cref="CustomDrawGroup.BeginImpl"/>
        /// </summary>
        /// <param name="element">The custom draw group</param>
        /// <param name="time">The game's time</param>
        /// <param name="batch">The sprite batch used for drawing</param>
        /// <param name="alpha">This element's draw alpha</param>
        /// <param name="blendState">The blend state used for drawing</param>
        /// <param name="samplerState">The sampler state used for drawing</param>
        /// <param name="matrix">The transform matrix used for drawing</param>
        public delegate void BeginDelegate(CustomDrawGroup element, GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix);

        /// <summary>
        /// A delegate method used for <see cref="CustomDrawGroup.TransformGetter"/>
        /// </summary>
        /// <param name="element">The element whose transform to get</param>
        /// <param name="time">The game's time</param>
        /// <param name="matrix">The regular transform matrix</param>
        public delegate Matrix TransformCallback(CustomDrawGroup element, GameTime time, Matrix matrix);

    }
}