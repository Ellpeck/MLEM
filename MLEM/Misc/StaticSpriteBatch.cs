using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Misc {
    /// <summary>
    /// A static sprite batch is a variation of <see cref="SpriteBatch"/> that keeps all batched items in a <see cref="VertexBuffer"/>, allowing for them to be drawn multiple times.
    /// To add items to a static sprite batch, use <see cref="ClearBatch"/> to clear currently batched items, <see cref="BeginBatch"/> to begin batching, <c>Add</c> and its various overloads to add batch items and <see cref="EndBatch"/> to end batching.
    /// To draw the batched items, call <see cref="Draw"/>.
    /// </summary>
    public class StaticSpriteBatch : IDisposable {

        /// <summary>
        /// The amount of vertices that are currently batched
        /// </summary>
        public int Vertices => this.vertices.Count;

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteEffect spriteEffect;

        private readonly List<VertexBuffer> vertexBuffers = new List<VertexBuffer>();
        private readonly List<VertexPositionColorTexture> vertices = new List<VertexPositionColorTexture>();
        private IndexBuffer indices;
        private Texture2D texture;
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
        /// Note that, if <see cref="ClearBatch"/> was not called, items that are batched will be appended to the existing batch.
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
        /// <exception cref="InvalidOperationException">Thrown if this method is called before <see cref="BeginBatch"/> was called</exception>
        public void EndBatch() {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            this.batching = false;

            // if we didn't add any batch items, we don't have to recalculate anything
            if (!this.batchChanged)
                return;
            this.batchChanged = false;

            // this maximum is limited by indices being a short
            const int maxBatchItems = short.MaxValue / 6;

            // ensure we have enough vertex buffers
            var requiredBuffers = (this.vertices.Count / (maxBatchItems * 4F)).Ceil();
            while (this.vertexBuffers.Count < requiredBuffers)
                this.vertexBuffers.Add(new VertexBuffer(this.graphicsDevice, VertexPositionColorTexture.VertexDeclaration, maxBatchItems * 4, BufferUsage.WriteOnly));

            // fill vertex buffers
            var arrayIndex = 0;
            var totalIndex = 0;
            var data = new VertexPositionColorTexture[maxBatchItems * 4];
            while (totalIndex < this.vertices.Count) {
                var now = Math.Min(this.vertices.Count - totalIndex, data.Length);
                for (var i = 0; i < now; i++)
                    data[i] = this.vertices[totalIndex + i];
                this.vertexBuffers[arrayIndex++].SetData(data);
                totalIndex += now;
            }

            // ensure we have enough indices
            var maxItems = Math.Min(this.vertices.Count / 4, maxBatchItems);
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
                this.indices = new IndexBuffer(this.graphicsDevice, IndexElementSize.SixteenBits, newIndices.Length, BufferUsage.WriteOnly);
                this.indices.SetData(newIndices);
            }
        }

        /// <summary>
        /// Clears the batch, removing all currently batched vertices.
        /// After this operation, <see cref="Vertices"/> will return 0.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this batch is currently batching</exception>
        public void ClearBatch() {
            if (this.batching)
                throw new InvalidOperationException("Cannot clear while batching");
            this.vertices.Clear();
            this.texture = null;
            this.batchChanged = true;
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
            foreach (var buffer in this.vertexBuffers) {
                var tris = Math.Min(this.vertices.Count - totalIndex, buffer.VertexCount) / 4 * 2;
                if (tris <= 0)
                    break;
                this.graphicsDevice.SetVertexBuffer(buffer);
                if (effect != null) {
                    foreach (var pass in effect.CurrentTechnique.Passes) {
                        pass.Apply();
                        this.graphicsDevice.Textures[0] = this.texture;
                        this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, tris);
                    }
                } else {
                    this.graphicsDevice.Textures[0] = this.texture;
                    this.graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, tris);
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
        public void Add(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
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
                this.Add(texture, position - origin, size, color, texTl, texBr, layerDepth);
            } else {
                this.Add(texture, position, -origin, size, (float) Math.Sin(rotation), (float) Math.Cos(rotation), color, texTl, texBr, layerDepth);
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
        public void Add(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            this.Add(texture, position, sourceRectangle, color, rotation, origin, new Vector2(scale), effects, layerDepth);
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
        public void Add(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
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

            if (rotation == 0) {
                this.Add(texture, destinationRectangle.Location.ToVector2() - origin, destinationRectangle.Size.ToVector2(), color, texTl, texBr, layerDepth);
            } else {
                this.Add(texture, destinationRectangle.Location.ToVector2(), -origin, destinationRectangle.Size.ToVector2(), (float) Math.Sin(rotation), (float) Math.Cos(rotation), color, texTl, texBr, layerDepth);
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
        public void Add(Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color) {
            this.Add(texture, position, sourceRectangle, color, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="sourceRectangle">An optional region on the texture which will be rendered. If null - draws full texture.</param>
        /// <param name="color">A color mask.</param>
        public void Add(Texture2D texture, Rectangle destinationRectangle, Rectangle? sourceRectangle, Color color) {
            this.Add(texture, destinationRectangle, sourceRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Add(Texture2D texture, Vector2 position, Color color) {
            this.Add(texture, position, null, color);
        }

        /// <summary>
        /// Adds an item to this batch.
        /// Note that this batch needs to currently be batching, meaning <see cref="BeginBatch"/> has to have been called previously.
        /// </summary>
        /// <param name="texture">A texture.</param>
        /// <param name="destinationRectangle">The drawing bounds on screen.</param>
        /// <param name="color">A color mask.</param>
        public void Add(Texture2D texture, Rectangle destinationRectangle, Color color) {
            this.Add(texture, destinationRectangle, null, color);
        }

        /// <inheritdoc />
        public void Dispose() {
            this.spriteEffect.Dispose();
            GC.SuppressFinalize(this);
        }

        private void Add(Texture2D texture, Vector2 pos, Vector2 offset, Vector2 size, float sin, float cos, Color color, Vector2 texTl, Vector2 texBr, float depth) {
            this.Add(texture,
                new VertexPositionColorTexture(new Vector3(pos.X + offset.X * cos - offset.Y * sin, pos.Y + offset.X * sin + offset.Y * cos, depth), color, texTl),
                new VertexPositionColorTexture(new Vector3(pos.X + (offset.X + size.X) * cos - offset.Y * sin, pos.Y + (offset.X + size.X) + offset.Y * cos, depth), color, new Vector2(texBr.X, texTl.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X + offset.X * cos - (offset.Y + size.Y) * sin, pos.Y + offset.X * sin + (offset.Y + size.Y) * cos, depth), color, new Vector2(texTl.X, texBr.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X + (offset.X + size.X) * cos - (offset.Y + size.Y) * sin, pos.Y + (offset.X + size.X) * sin + (offset.Y + size.Y) * cos, depth), color, texBr));
        }

        private void Add(Texture2D texture, Vector2 pos, Vector2 size, Color color, Vector2 texTl, Vector2 texBr, float depth) {
            this.Add(texture,
                new VertexPositionColorTexture(new Vector3(pos, depth), color, texTl),
                new VertexPositionColorTexture(new Vector3(pos.X + size.X, pos.Y, depth), color, new Vector2(texBr.X, texTl.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X, pos.Y + size.Y, depth), color, new Vector2(texTl.X, texBr.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X + size.X, pos.Y + size.Y, depth), color, texBr));
        }

        private void Add(Texture2D texture, VertexPositionColorTexture tl, VertexPositionColorTexture tr, VertexPositionColorTexture bl, VertexPositionColorTexture br) {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            if (this.texture != null && this.texture != texture)
                throw new ArgumentException("Cannot use multiple textures in one batch");
            this.texture = texture;
            this.vertices.Add(tl);
            this.vertices.Add(tr);
            this.vertices.Add(bl);
            this.vertices.Add(br);
            this.batchChanged = true;
        }

    }
}