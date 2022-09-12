using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#if FNA
using MLEM.Extensions;
using System.IO;
#endif

namespace MLEM.Graphics {
    /// <summary>
    /// A static sprite batch is a variation of <see cref="SpriteBatch"/> that keeps all batched items in a <see cref="VertexBuffer"/>, allowing for them to be drawn multiple times.
    /// To add items to a static sprite batch, use <see cref="BeginBatch"/> to begin batching, <see cref="ClearBatch"/> to clear currently batched items, <c>Add</c> and its various overloads to add batch items, <see cref="Remove"/> to remove them again, and <see cref="EndBatch"/> to end batching.
    /// To draw the batched items, call <see cref="Draw"/>.
    /// </summary>
    public class StaticSpriteBatch : IDisposable {

        // this maximum is limited by indices being a short
        private const int MaxBatchItems = short.MaxValue / 6;
        private static readonly VertexPositionColorTexture[] Data = new VertexPositionColorTexture[StaticSpriteBatch.MaxBatchItems * 4];

        /// <summary>
        /// The amount of vertices that are currently batched.
        /// </summary>
        public int Vertices => this.items.Count * 4;
        /// <summary>
        /// The amount of vertex buffers that this static sprite batch has.
        /// To see the amount of buffers that are actually in use, see <see cref="FilledBuffers"/>.
        /// </summary>
        public int Buffers => this.vertexBuffers.Count;
        /// <summary>
        /// The amount of textures that this static sprite batch is currently using.
        /// </summary>
        public int Textures => this.textures.Distinct().Count();
        /// <summary>
        /// The amount of vertex buffers that are currently filled in this static sprite batch.
        /// To see the amount of buffers that are available, see <see cref="Buffers"/>.
        /// </summary>
        public int FilledBuffers { get; private set; }

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteEffect spriteEffect;

        private readonly List<DynamicVertexBuffer> vertexBuffers = new List<DynamicVertexBuffer>();
        private readonly List<Texture2D> textures = new List<Texture2D>();
        private readonly ISet<Item> items = new HashSet<Item>();
        private IndexBuffer indices;
        private bool batching;
        private bool batchChanged;

        /// <summary>
        /// Creates a new static sprite batch with the given <see cref="GraphicsDevice"/>
        /// </summary>
        /// <param name="graphicsDevice">The graphics device to use for rendering</param>
        public StaticSpriteBatch(GraphicsDevice graphicsDevice) {
            this.graphicsDevice = graphicsDevice;
            this.spriteEffect = new SpriteEffect(graphicsDevice);
        }

        /// <summary>
        /// Begins batching.
        /// Call this method before calling <c>Add</c> or any of its overloads.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this batch is currently batching already</exception>
        public void BeginBatch() {
            if (this.batching)
                throw new InvalidOperationException("Already batching");
            this.batching = true;
        }

        /// <summary>
        /// Ends batching.
        /// Call this method after calling <c>Add</c> or any of its overloads the desired number of times to add batched items.
        /// </summary>
        /// <param name="sortMode">The drawing order for sprite drawing. <see cref="SpriteSortMode.Texture" /> by default, since it is the best in terms of rendering performance. Note that <see cref="SpriteSortMode.Immediate"/> is not supported.</param>
        /// <exception cref="InvalidOperationException">Thrown if this method is called before <see cref="BeginBatch"/> was called.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the <paramref name="sortMode"/> is <see cref="SpriteSortMode.Immediate"/>, which is not supported.</exception>
        public void EndBatch(SpriteSortMode sortMode = SpriteSortMode.Texture) {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            if (sortMode == SpriteSortMode.Immediate)
                throw new ArgumentOutOfRangeException(nameof(sortMode), "Cannot use sprite sort mode Immediate for static batching");
            this.batching = false;

            // if we didn't add or remove any batch items, we don't have to recalculate anything
            if (!this.batchChanged)
                return;
            this.batchChanged = false;
            this.FilledBuffers = 0;
            this.textures.Clear();

            // order items according to the sort mode
            IEnumerable<Item> ordered = this.items;
            switch (sortMode) {
                case SpriteSortMode.Texture:
                    // SortingKey is internal, but this will do for batching the same texture together
                    ordered = ordered.OrderBy(i => i.Texture.GetHashCode());
                    break;
                case SpriteSortMode.BackToFront:
                    ordered = ordered.OrderBy(i => -i.Depth);
                    break;
                case SpriteSortMode.FrontToBack:
                    ordered = ordered.OrderBy(i => i.Depth);
                    break;
            }

            // fill vertex buffers
            var dataIndex = 0;
            Texture2D texture = null;
            foreach (var item in ordered) {
                // if the texture changes, we also have to start a new buffer!
                if (dataIndex > 0 && (item.Texture != texture || dataIndex >= StaticSpriteBatch.Data.Length)) {
                    this.FillBuffer(this.FilledBuffers++, texture, StaticSpriteBatch.Data);
                    dataIndex = 0;
                }
                StaticSpriteBatch.Data[dataIndex++] = item.TopLeft;
                StaticSpriteBatch.Data[dataIndex++] = item.TopRight;
                StaticSpriteBatch.Data[dataIndex++] = item.BottomLeft;
                StaticSpriteBatch.Data[dataIndex++] = item.BottomRight;
                texture = item.Texture;
            }
            if (dataIndex > 0)
                this.FillBuffer(this.FilledBuffers++, texture, StaticSpriteBatch.Data);

            // ensure we have enough indices
            var maxItems = Math.Min(this.items.Count, StaticSpriteBatch.MaxBatchItems);
            // each item has 2 triangles which each have 3 indices
            if (this.indices == null || this.indices.IndexCount < 6 * maxItems) {
                var newIndices = new short[6 * maxItems];
                var index = 0;
                for (var item = 0; item < maxItems; item++) {
                    // a square is made up of two triangles
                    // 0--1
                    // | /|
                    // |/ |
                    // 2--3
                    // top left triangle (0 -> 1 -> 2)
                    newIndices[index++] = (short) (item * 4 + 0);
                    newIndices[index++] = (short) (item * 4 + 1);
                    newIndices[index++] = (short) (item * 4 + 2);
                    // bottom right triangle (1 -> 3 -> 2)
                    newIndices[index++] = (short) (item * 4 + 1);
                    newIndices[index++] = (short) (item * 4 + 3);
                    newIndices[index++] = (short) (item * 4 + 2);
                }
                this.indices?.Dispose();
                this.indices = new IndexBuffer(this.graphicsDevice, IndexElementSize.SixteenBits, newIndices.Length, BufferUsage.WriteOnly);
                this.indices.SetData(newIndices);
            }
        }

        /// <summary>
        /// Draws this batch's content onto the <see cref="GraphicsDevice"/>'s current render target (or the back buffer) with the given settings.
        /// Note that this method should not be called while a regular <see cref="SpriteBatch"/> is currently active.
        /// </summary>
        /// <param name="blendState">State of the blending. Uses <see cref="BlendState.AlphaBlend"/> if null.</param>
        /// <param name="samplerState">State of the sampler. Uses <see cref="SamplerState.LinearClamp"/> if null.</param>
        /// <param name="depthStencilState">State of the depth-stencil buffer. Uses <see cref="DepthStencilState.None"/> if null.</param>
        /// <param name="rasterizerState">State of the rasterization. Uses <see cref="RasterizerState.CullCounterClockwise"/> if null.</param>
        /// <param name="effect">A custom <see cref="Effect"/> to override the default sprite effect. Uses default sprite effect if null.</param>
        /// <param name="transformMatrix">An optional matrix used to transform the sprite geometry. Uses <see cref="Matrix.Identity"/> if null.</param>
        /// <exception cref="InvalidOperationException">Thrown if this batch is currently batching</exception>
        public void Draw(BlendState blendState = null, SamplerState samplerState = null, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, Effect effect = null, Matrix? transformMatrix = null) {
            if (this.batching)
                throw new InvalidOperationException("Cannot draw the batch while batching");

            this.graphicsDevice.BlendState = blendState ?? BlendState.AlphaBlend;
            this.graphicsDevice.SamplerStates[0] = samplerState ?? SamplerState.LinearClamp;
            this.graphicsDevice.DepthStencilState = depthStencilState ?? DepthStencilState.None;
            this.graphicsDevice.RasterizerState = rasterizerState ?? RasterizerState.CullCounterClockwise;
            this.graphicsDevice.Indices = this.indices;

            this.spriteEffect.TransformMatrix = transformMatrix;
            this.spriteEffect.CurrentTechnique.Passes[0].Apply();

            var totalIndex = 0;
            for (var i = 0; i < this.FilledBuffers; i++) {
                var buffer = this.vertexBuffers[i];
                var texture = this.textures[i];
                var verts = Math.Min(this.items.Count * 4 - totalIndex, buffer.VertexCount);

                this.graphicsDevice.SetVertexBuffer(buffer);
                if (effect != null) {
                    foreach (var pass in effect.CurrentTechnique.Passes) {
                        pass.Apply();
                        this.graphicsDevice.Textures[0] = texture;
                        this.DrawPrimitives(verts);
                    }
                } else {
                    this.graphicsDevice.Textures[0] = texture;
                    this.DrawPrimitives(verts);
                }

                totalIndex += buffer.VertexCount;
            }
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            origin *= scale;

            Vector2 size, texTl, texBr;
            if (sourceRectangle.HasValue) {
                var src = sourceRectangle.Value;
                size.X = src.Width * scale.X;
                size.Y = src.Height * scale.Y;
                texTl.X = src.X * (1F / texture.Width);
                texTl.Y = src.Y * (1F / texture.Height);
                texBr.X = (src.X + src.Width) * (1F / texture.Width);
                texBr.Y = (src.Y + src.Height) * (1F / texture.Height);
            } else {
                size.X = texture.Width * scale.X;
                size.Y = texture.Height * scale.Y;
                texTl = Vector2.Zero;
                texBr = Vector2.One;
            }

            if ((effects & SpriteEffects.FlipVertically) != 0)
                (texBr.Y, texTl.Y) = (texTl.Y, texBr.Y);
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
                (texBr.X, texTl.X) = (texTl.X, texBr.X);

            if (rotation == 0) {
                return this.Add(texture, position - origin, size, color, texTl, texBr, layerDepth);
            } else {
                return this.Add(texture, position, -origin, size, (float) Math.Sin(rotation), (float) Math.Cos(rotation), color, texTl, texBr, layerDepth);
            }
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this sprite.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            return this.Add(texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale), effects, layerDepth);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this sprite.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this sprite.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            Vector2 texTl, texBr;
            if (sourceRectangle.HasValue) {
                var src = sourceRectangle.Value;
                texTl.X = src.X * (1F / texture.Width);
                texTl.Y = src.Y * (1F / texture.Height);
                texBr.X = (src.X + src.Width) * (1F / texture.Width);
                texBr.Y = (src.Y + src.Height) * (1F / texture.Height);
                origin.X = origin.X * destinationRectangle.Width * (src.Width != 0 ? src.Width : 1F / texture.Width);
                origin.Y = origin.Y * destinationRectangle.Height * (src.Height != 0 ? src.Height : 1F / texture.Height);
            } else {
                texTl = Vector2.Zero;
                texBr = Vector2.One;
                origin.X = origin.X * destinationRectangle.Width * (1F / texture.Width);
                origin.Y = origin.Y * destinationRectangle.Height * (1F / texture.Height);
            }

            if ((effects & SpriteEffects.FlipVertically) != 0)
                (texBr.Y, texTl.Y) = (texTl.Y, texBr.Y);
            if ((effects & SpriteEffects.FlipHorizontally) != 0)
                (texBr.X, texTl.X) = (texTl.X, texBr.X);

            var destSize = new Vector2(destinationRectangle.Width, destinationRectangle.Height);
            if (rotation == 0) {
                return this.Add(texture, destinationRectangle.Location.ToVector2() - origin, destSize, color, texTl, texBr, layerDepth);
            } else {
                return this.Add(texture, destinationRectangle.Location.ToVector2(), -origin, destSize, (float) Math.Sin(rotation), (float) Math.Cos(rotation), color, texTl, texBr, layerDepth);
            }
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) {
            return this.Add(texture, position, sourceRectangle, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color) {
            return this.Add(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Vector2 position, Color color) {
            return this.Add(texture, position, null, color);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <returns>The <see cref="Item"/> that was created from the added data</returns>
        public Item Add(Texture2D texture, Rectangle destinationRectangle, Color color) {
            return this.Add(texture, destinationRectangle, null, color);
        }

        /// <summary>
        /// Removes the given item from this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <returns>Whether the item was successfully removed</returns>
        /// <exception cref="InvalidOperationException">Thrown if this method is called before <see cref="BeginBatch"/> was called</exception>
        public bool Remove(Item item) {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            if (this.items.Remove(item)) {
                this.batchChanged = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clears the batch, removing all currently batched vertices.
        /// After this operation, <see cref="Vertices"/> will return 0.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this method is called before <see cref="BeginBatch"/> was called</exception>
        public void ClearBatch() {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            this.items.Clear();
            this.textures.Clear();
            this.FilledBuffers = 0;
            this.batchChanged = true;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() {
            this.spriteEffect.Dispose();
            this.indices?.Dispose();
            foreach (var buffer in this.vertexBuffers)
                buffer.Dispose();
            GC.SuppressFinalize(this);
        }

        private Item Add(Texture2D texture, Vector2 pos, Vector2 offset, Vector2 size, float sin, float cos, Color color, Vector2 texTl, Vector2 texBr, float depth) {
            return this.Add(texture, depth,
                new VertexPositionColorTexture(new Vector3(
                        pos.X + offset.X * cos - offset.Y * sin,
                        pos.Y + offset.X * sin + offset.Y * cos, depth),
                    color, texTl),
                new VertexPositionColorTexture(new Vector3(
                        pos.X + (offset.X + size.X) * cos - offset.Y * sin,
                        pos.Y + (offset.X + size.X) * sin + offset.Y * cos, depth),
                    color, new Vector2(texBr.X, texTl.Y)),
                new VertexPositionColorTexture(new Vector3(
                        pos.X + offset.X * cos - (offset.Y + size.Y) * sin,
                        pos.Y + offset.X * sin + (offset.Y + size.Y) * cos, depth),
                    color, new Vector2(texTl.X, texBr.Y)),
                new VertexPositionColorTexture(new Vector3(
                        pos.X + (offset.X + size.X) * cos - (offset.Y + size.Y) * sin,
                        pos.Y + (offset.X + size.X) * sin + (offset.Y + size.Y) * cos, depth),
                    color, texBr));
        }

        private Item Add(Texture2D texture, Vector2 pos, Vector2 size, Color color, Vector2 texTl, Vector2 texBr, float depth) {
            return this.Add(texture, depth,
                new VertexPositionColorTexture(
                    new Vector3(pos, depth),
                    color, texTl),
                new VertexPositionColorTexture(
                    new Vector3(pos.X + size.X, pos.Y, depth),
                    color, new Vector2(texBr.X, texTl.Y)),
                new VertexPositionColorTexture(
                    new Vector3(pos.X, pos.Y + size.Y, depth),
                    color, new Vector2(texTl.X, texBr.Y)),
                new VertexPositionColorTexture(
                    new Vector3(pos.X + size.X, pos.Y + size.Y, depth),
                    color, texBr));
        }

        private Item Add(Texture2D texture, float depth, VertexPositionColorTexture tl, VertexPositionColorTexture tr, VertexPositionColorTexture bl, VertexPositionColorTexture br) {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            var item = new Item(texture, depth, tl, tr, bl, br);
            this.items.Add(item);
            this.batchChanged = true;
            return item;
        }

        private void FillBuffer(int index, Texture2D texture, VertexPositionColorTexture[] data) {
            if (this.vertexBuffers.Count <= index)
                this.vertexBuffers.Add(new DynamicVertexBuffer(this.graphicsDevice, VertexPositionColorTexture.VertexDeclaration, StaticSpriteBatch.MaxBatchItems * 4, BufferUsage.WriteOnly));
            this.vertexBuffers[index].SetData(data, 0, data.Length, SetDataOptions.Discard);
            this.textures.Insert(index, texture);
        }

        private void DrawPrimitives(int vertices) {
            #if FNA
            this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices, 0, vertices / 4 * 2);
            #else
            this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices / 4 * 2);
            #endif
        }

        /// <summary>
        /// A struct that represents an item added to a <see cref="StaticSpriteBatch"/> using <c>Add</c> or any of its overloads.
        /// An item returned after adding can be removed using <see cref="Remove"/>.
        /// </summary>
        public class Item {

            internal readonly Texture2D Texture;
            internal readonly float Depth;
            internal readonly VertexPositionColorTexture TopLeft;
            internal readonly VertexPositionColorTexture TopRight;
            internal readonly VertexPositionColorTexture BottomLeft;
            internal readonly VertexPositionColorTexture BottomRight;

            internal Item(Texture2D texture, float depth, VertexPositionColorTexture topLeft, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture bottomRight) {
                this.Texture = texture;
                this.Depth = depth;
                this.TopLeft = topLeft;
                this.TopRight = topRight;
                this.BottomLeft = bottomLeft;
                this.BottomRight = bottomRight;
            }

        }

        #if FNA
        private class SpriteEffect : Effect {

            private EffectParameter matrixParam;
            private Viewport lastViewport;
            private Matrix projection;

            public Matrix? TransformMatrix { get; set; }

            public SpriteEffect(GraphicsDevice device) : base(device, SpriteEffect.LoadEffectCode()) {
                this.CacheEffectParameters();
            }

            private SpriteEffect(SpriteEffect cloneSource) : base(cloneSource) {
                this.CacheEffectParameters();
            }

            public override Effect Clone() {
                return new SpriteEffect(this);
            }

            private void CacheEffectParameters() {
                this.matrixParam = this.Parameters["MatrixTransform"];
            }

            protected override void OnApply() {
                var vp = this.GraphicsDevice.Viewport;
                if (vp.Width != this.lastViewport.Width || vp.Height != this.lastViewport.Height) {
                    Matrix.CreateOrthographicOffCenter(0, vp.Width, vp.Height, 0, 0, -1, out this.projection);
                    this.projection.M41 += -0.5f * this.projection.M11;
                    this.projection.M42 += -0.5f * this.projection.M22;
                    this.lastViewport = vp;
                }

                if (this.TransformMatrix.HasValue) {
                    this.matrixParam.SetValue(this.TransformMatrix.GetValueOrDefault() * this.projection);
                } else {
                    this.matrixParam.SetValue(this.projection);
                }
            }

            private static byte[] LoadEffectCode() {
                using (var stream = typeof(Effect).Assembly.GetManifestResourceStream("Microsoft.Xna.Framework.Graphics.Effect.Resources.SpriteEffect.fxb")) {
                    using (var memory = new MemoryStream()) {
                        stream.CopyTo(memory);
                        return memory.ToArray();
                    }
                }
            }

        }
        #endif

    }
}
