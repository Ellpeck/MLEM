using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Maths;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A scissor group is a <see cref="Group"/> that sets the <see cref="GraphicsDevice.ScissorRectangle"/> before drawing its content (and thus, its <see cref="Element.Children"/>), preventing them from being drawn outside of this group's <see cref="Element.DisplayArea"/>'s bounds.
    /// </summary>
    public class ScissorGroup : Group {

        /// <summary>
        /// The rasterizer state that this scissor group should use when drawing.
        /// By default, <see cref="CullMode"/> is set to <see cref="CullMode.CullCounterClockwiseFace"/> in accordance with <see cref="SpriteBatch"/>'s default behavior, and <see cref="RasterizerState.ScissorTestEnable"/> is set to <see langword="true"/>.
        /// </summary>
        public readonly RasterizerState Rasterizer = new RasterizerState {
            // use the default cull mode from SpriteBatch, but with scissor test enabled
            CullMode = CullMode.CullCounterClockwiseFace,
            ScissorTestEnable = true
        };

        /// <summary>
        /// Creates a new scissor group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="setHeightBasedOnChildren">Whether the group's height should be based on its children's height, see <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        public ScissorGroup(Anchor anchor, Vector2 size, bool setHeightBasedOnChildren = true) : base(anchor, size, setHeightBasedOnChildren) {}

        /// <summary>
        /// Creates a new scissor group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="setWidthBasedOnChildren">Whether the group's width should be based on its children's width, see <see cref="Element.SetWidthBasedOnChildren"/>.</param>
        /// <param name="setHeightBasedOnChildren">Whether the group's height should be based on its children's height, see <see cref="Element.SetHeightBasedOnChildren"/>.</param>
        public ScissorGroup(Anchor anchor, Vector2 size, bool setWidthBasedOnChildren, bool setHeightBasedOnChildren) : base(anchor, size, setWidthBasedOnChildren, setHeightBasedOnChildren) {}

        /// <inheritdoc />
        public override void Draw(GameTime time, SpriteBatch batch, float alpha, SpriteBatchContext context) {
            batch.End();

            // apply our scissor rectangle
            var lastScissor = batch.GraphicsDevice.ScissorRectangle;
            batch.GraphicsDevice.ScissorRectangle = (Rectangle) this.DisplayArea.OffsetCopy(context.TransformMatrix.Translation.ToVector2());

            // enable scissor test
            var localContext = context;
            localContext.RasterizerState = this.Rasterizer;
            batch.Begin(localContext);

            base.Draw(time, batch, alpha, localContext);

            // revert back to previous behavior
            batch.End();
            batch.GraphicsDevice.ScissorRectangle = lastScissor;
            batch.Begin(context);
        }

    }
}
