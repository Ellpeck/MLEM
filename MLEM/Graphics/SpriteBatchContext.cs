using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Graphics {
    /// <summary>
    /// A sprite batch context is a set of information for a <see cref="SpriteBatch"/> to use, which encapsulates all of the information usually passed directly to <c>SpriteBatch.Begin</c>.
    /// To use a sprite batch context effectively, the extension methods in <see cref="SpriteBatchContextExtensions"/> should be used.
    /// </summary>
    public struct SpriteBatchContext {

        /// <summary>
        /// The drawing order for sprite and text drawing.
        /// </summary>
        public SpriteSortMode SortMode;
        /// <summary>
        /// State of the blending.
        /// </summary>
        public BlendState BlendState;
        /// <summary>
        /// State of the sampler.
        /// </summary>
        public SamplerState SamplerState;
        /// <summary>
        /// State of the depth-stencil buffer.
        /// </summary>
        public DepthStencilState DepthStencilState;
        /// <summary>
        /// State of the rasterization.
        /// </summary>
        public RasterizerState RasterizerState;
        /// <summary>
        /// A custom <see cref="T:Microsoft.Xna.Framework.Graphics.Effect" /> to override the default sprite effect.
        /// </summary>
        public Effect Effect;
        /// <summary>
        /// An optional matrix used to transform the sprite geometry.
        /// </summary>
        public Matrix TransformMatrix;

        /// <summary>
        /// Creates a new sprite batch context with the given parameters.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="F:Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred" /> by default.</param>
        /// <param name="blendState">State of the blending. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.BlendState.AlphaBlend" /> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.SamplerState.LinearClamp" /> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.DepthStencilState.None" /> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="F:Microsoft.Xna.Framework.Graphics.RasterizerState.CullCounterClockwise" /> if null.</param>
        /// <param name="effect">A custom <see cref="T:Microsoft.Xna.Framework.Graphics.Effect" /> to override the default sprite effect.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="P:Microsoft.Xna.Framework.Matrix.Identity" /> if null.</param>
        public SpriteBatchContext(SpriteSortMode sortMode = SpriteSortMode.Deferred, BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
            this.SortMode = sortMode;
            this.BlendState = blendState ?? BlendState.AlphaBlend;
            this.SamplerState = samplerState ?? SamplerState.LinearClamp;
            this.DepthStencilState = depthStencilState ?? DepthStencilState.None;
            this.RasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            this.Effect = effect;
            this.TransformMatrix = transformMatrix ?? Matrix.Identity;
        }

        /// <summary>
        /// Creates a new sprite batch context from the passed <see cref="GraphicsDevice"/>'s current information.
        /// This can be useful to retrieve some information about drawing right after a <see cref="SpriteBatch.End"/> call has occured.
        /// </summary>
        /// <param name="device">The graphics device to query data from.</param>
        /// <param name="sortMode">The drawing order for sprite and text drawing. <see cref="F:Microsoft.Xna.Framework.Graphics.SpriteSortMode.Deferred" /> by default.</param>
        /// <param name="effect">A custom <see cref="T:Microsoft.Xna.Framework.Graphics.Effect" /> to override the default sprite effect.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="P:Microsoft.Xna.Framework.Matrix.Identity" /> if null.</param>
        /// <returns>A new sprite batch context from the <paramref name="device"/>'s current information.</returns>
        public static SpriteBatchContext Current(GraphicsDevice device, SpriteSortMode sortMode = SpriteSortMode.Deferred, Effect effect = null, Matrix? transformMatrix = null) {
            return new SpriteBatchContext(sortMode, device.BlendState, device.SamplerStates[0], device.DepthStencilState, device.RasterizerState, effect, transformMatrix);
        }

    }

    /// <summary>
    /// A set of extensions for <see cref="SpriteBatchContext"/>.
    /// </summary>
    public static class SpriteBatchContextExtensions {

        /// <summary>
        /// Begins a new sprite and text batch with the specified <see cref="SpriteBatchContext"/>
        /// </summary>
        /// <param name="batch">The sprite batch to use for drawing.</param>
        /// <param name="context">The sprite batch context to use.</param>
        public static void Begin(this SpriteBatch batch, SpriteBatchContext context) {
            batch.Begin(context.SortMode, context.BlendState, context.SamplerState, context.DepthStencilState, context.RasterizerState, context.Effect, context.TransformMatrix);
        }

        /// <summary>
        /// Draws the given batch's content onto the <see cref="GraphicsDevice"/>'s current render target (or the back buffer) with the given settings.
        /// Note that this method should not be called while a regular <see cref="SpriteBatch"/> is currently active.
        /// </summary>
        /// <param name="batch">The static sprite batch to use for drawing.</param>
        /// <param name="context">The sprite batch context to use.</param>
        public static void Draw(this StaticSpriteBatch batch, SpriteBatchContext context) {
            batch.Draw(context.BlendState, context.SamplerState, context.DepthStencilState, context.RasterizerState, context.Effect, context.TransformMatrix);
        }

    }
}
