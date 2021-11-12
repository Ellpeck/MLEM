using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Misc {
    /// <summary>
    /// A static sprite batch is a variation of <see cref="SpriteBatch"/> that keeps all batched items in a <see cref="VertexBuffer"/>, allowing for them to be drawn multiple times.
    /// To add items to a static sprite batch, use <see cref="BeginBatch"/> to begin batching, <see cref="ClearBatch"/> to clear currently batched items, <c>Add</c> and its various overloads to add batch items, <see cref="Remove"/> to remove them again, and <see cref="EndBatch"/> to end batching.
    /// To draw the batched items, call <see cref="Draw"/>.
    /// Unlike a <see cref="SpriteBatch"/>, items added to a static sprite batch will be drawn in an arbitrary order. If depth sorting is desired, the <see cref="GraphicsDeviceManager"/>'s <see cref="GraphicsDeviceManager.PreferredDepthStencilFormat"/> should be modified to include depth. 
    /// </summary>
    public class StaticSpriteBatch : IDisposable {

        // this maximum is limited by indices being a short
        private const int MaxBatchItems = short.MaxValue / 6;
        private static readonly VertexPositionColorTexture[] Data = new VertexPositionColorTexture[MaxBatchItems * 4];

        /// <summary>
        /// The amount of vertices that are currently batched
        /// </summary>
        public int Vertices => this.items.Count * 4;

        private readonly GraphicsDevice graphicsDevice;
        private readonly SpriteEffect spriteEffect;

        private readonly List<VertexBuffer> vertexBuffers = new List<VertexBuffer>();
        private readonly ISet<Item> items = new HashSet<Item>();
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

            // if we didn't add or remove any batch items, we don't have to recalculate anything
            if (!this.batchChanged)
                return;
            this.batchChanged = false;

            // ensure we have enough vertex buffers
            var requiredBuffers = (this.items.Count / (float) MaxBatchItems).Ceil();
            while (this.vertexBuffers.Count < requiredBuffers)
                this.vertexBuffers.Add(new VertexBuffer(this.graphicsDevice, VertexPositionColorTexture.VertexDeclaration, MaxBatchItems * 4, BufferUsage.WriteOnly));

            // fill vertex buffers
            var dataIndex = 0;
            var arrayIndex = 0;
            foreach (var item in this.items) {
                Data[dataIndex++] = item.TopLeft;
                Data[dataIndex++] = item.TopRight;
                Data[dataIndex++] = item.BottomLeft;
                Data[dataIndex++] = item.BottomRight;
                if (dataIndex >= Data.Length) {
                    this.vertexBuffers[arrayIndex++].SetData(Data);
                    dataIndex = 0;
                }
            }
            if (dataIndex > 0)
                this.vertexBuffers[arrayIndex].SetData(Data);

            // ensure we have enough indices
            var maxItems = Math.Min(this.items.Count, MaxBatchItems);
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
            foreach (var buffer in this.vertexBuffers) {
                var tris = Math.Min(this.items.Count * 4 - totalIndex, buffer.VertexCount) / 4 * 2;
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

            if (rotation == 0) {
                return this.Add(texture, destinationRectangle.Location.ToVector2() - origin, destinationRectangle.Size.ToVector2(), color, texTl, texBr, layerDepth);
            } else {
                return this.Add(texture, destinationRectangle.Location.ToVector2(), -origin, destinationRectangle.Size.ToVector2(), (float) Math.Sin(rotation), (float) Math.Cos(rotation), color, texTl, texBr, layerDepth);
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
            this.texture = null;
            this.batchChanged = true;
        }

        /// <inheritdoc />
        public void Dispose() {
            this.spriteEffect.Dispose();
            this.indices?.Dispose();
            foreach (var buffer in this.vertexBuffers)
                buffer.Dispose();
            GC.SuppressFinalize(this);
        }

        private Item Add(Texture2D texture, Vector2 pos, Vector2 offset, Vector2 size, float sin, float cos, Color color, Vector2 texTl, Vector2 texBr, float depth) {
            return this.Add(texture,
                new VertexPositionColorTexture(new Vector3(pos.X + offset.X * cos - offset.Y * sin, pos.Y + offset.X * sin + offset.Y * cos, depth), color, texTl),
                new VertexPositionColorTexture(new Vector3(pos.X + (offset.X + size.X) * cos - offset.Y * sin, pos.Y + (offset.X + size.X) + offset.Y * cos, depth), color, new Vector2(texBr.X, texTl.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X + offset.X * cos - (offset.Y + size.Y) * sin, pos.Y + offset.X * sin + (offset.Y + size.Y) * cos, depth), color, new Vector2(texTl.X, texBr.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X + (offset.X + size.X) * cos - (offset.Y + size.Y) * sin, pos.Y + (offset.X + size.X) * sin + (offset.Y + size.Y) * cos, depth), color, texBr));
        }

        private Item Add(Texture2D texture, Vector2 pos, Vector2 size, Color color, Vector2 texTl, Vector2 texBr, float depth) {
            return this.Add(texture,
                new VertexPositionColorTexture(new Vector3(pos, depth), color, texTl),
                new VertexPositionColorTexture(new Vector3(pos.X + size.X, pos.Y, depth), color, new Vector2(texBr.X, texTl.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X, pos.Y + size.Y, depth), color, new Vector2(texTl.X, texBr.Y)),
                new VertexPositionColorTexture(new Vector3(pos.X + size.X, pos.Y + size.Y, depth), color, texBr));
        }

        private Item Add(Texture2D texture, VertexPositionColorTexture tl, VertexPositionColorTexture tr, VertexPositionColorTexture bl, VertexPositionColorTexture br) {
            if (!this.batching)
                throw new InvalidOperationException("Not batching");
            if (this.texture != null && this.texture != texture)
                throw new ArgumentException("Cannot use multiple textures in one batch");
            var item = new Item(tl, tr, bl, br);
            this.items.Add(item);
            this.texture = texture;
            this.batchChanged = true;
            return item;
        }

        /// <summary>
        /// A struct that represents an item added to a <see cref="StaticSpriteBatch"/> using <c>Add</c> or any of its overloads.
        /// An item returned after adding can be removed using <see cref="Remove"/>.
        /// </summary>
        public class Item {

            internal readonly VertexPositionColorTexture TopLeft;
            internal readonly VertexPositionColorTexture TopRight;
            internal readonly VertexPositionColorTexture BottomLeft;
            internal readonly VertexPositionColorTexture BottomRight;

            internal Item(VertexPositionColorTexture topLeft, VertexPositionColorTexture topRight, VertexPositionColorTexture bottomLeft, VertexPositionColorTexture bottomRight) {
                this.TopLeft = topLeft;
                this.TopRight = topRight;
                this.BottomLeft = bottomLeft;
                this.BottomRight = bottomRight;
            }

        }

    }
}