using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Data {
    /// <summary>
    /// A runtime texture packer provides the user with the ability to combine multiple <see cref="Texture2D"/> instances into a single texture.
    /// Packing textures in this manner allows for faster rendering, as fewer texture swaps are required.
    /// The resulting texture segments are returned as <see cref="TextureRegion"/> instances.
    /// </summary>
    public class RuntimeTexturePacker {

        private readonly List<Request> textures = new List<Request>();

        /// <summary>
        /// The generated packed texture.
        /// This value is null before <see cref="Pack"/> is called.
        /// </summary>
        public Texture2D PackedTexture { get; private set; }
        /// <summary>
        /// The time that it took to calculate the required areas the last time that <see cref="Pack"/> was called
        /// </summary>
        public TimeSpan LastCalculationTime { get; private set; }
        /// <summary>
        /// The time that it took to copy the texture data from the invidiual textures onto the <see cref="PackedTexture"/> the last time that <see cref="Pack"/> was called
        /// </summary>
        public TimeSpan LastPackTime { get; private set; }
        /// <summary>
        /// The time that <see cref="Pack"/> took the last time it was called
        /// </summary>
        public TimeSpan LastTotalTime => this.LastCalculationTime + this.LastPackTime;
        private readonly int maxWidth;

        /// <summary>
        /// Creates a new runtime texture packer with the given settings
        /// </summary>
        /// <param name="maxWidth">The maximum width that the packed texture can have. Defaults to 2048.</param>
        public RuntimeTexturePacker(int maxWidth = 2048) {
            this.maxWidth = maxWidth;
        }

        /// <summary>
        /// Adds a new texture to this texture packer to be packed.
        /// The passed <see cref="Action{T}"/> is invoked in <see cref="Pack"/> and provides the caller with the resulting texture region on the <see cref="PackedTexture"/>.
        /// </summary>
        /// <param name="texture">The texture to pack</param>
        /// <param name="result">The result callback which will receive the resulting texture region</param>
        public void Add(Texture2D texture, Action<TextureRegion> result) {
            this.Add(new TextureRegion(texture), result);
        }

        /// <summary>
        /// Adds a new <see cref="TextureRegion"/> to this texture packer to be packed.
        /// The passed <see cref="Action{T}"/> is invoked in <see cref="Pack"/> and provides the caller with the resulting texture region on the <see cref="PackedTexture"/>.
        /// </summary>
        /// <param name="texture">The texture to pack</param>
        /// <param name="result">The result callback which will receive the resulting texture region</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to add data to a packer that has already been packed, or when trying to add a texture width a width greater than the defined max width</exception>
        public void Add(TextureRegion texture, Action<TextureRegion> result) {
            if (this.PackedTexture != null)
                throw new InvalidOperationException("Cannot add texture to a texture packer that is already packed");
            if (texture.Width > this.maxWidth)
                throw new InvalidOperationException($"Cannot add texture with width {texture.Width} to a texture packer with max width {this.maxWidth}");
            this.textures.Add(new Request(texture, result));
        }

        /// <summary>
        /// Packs all of the textures and texture regions added using <see cref="Add(Microsoft.Xna.Framework.Graphics.Texture2D,System.Action{MLEM.Textures.TextureRegion})"/> into one texture.
        /// The resulting texture will be stored in <see cref="PackedTexture"/>.
        /// All of the result callbacks that were added will also be invoked.
        /// </summary>
        /// <param name="device">The graphics device to use for texture generation</param>
        /// <exception cref="InvalidOperationException">Thrown when calling this method on a texture packer that has already been packed</exception>
        public void Pack(GraphicsDevice device) {
            if (this.PackedTexture != null)
                throw new InvalidOperationException("Cannot pack a texture packer that is already packed");

            // set pack areas for each request
            var stopwatch = Stopwatch.StartNew();
            foreach (var request in this.textures.OrderByDescending(t => t.Texture.Width * t.Texture.Height)) {
                var area = this.FindFreeArea(new Point(request.Texture.Width, request.Texture.Height));
                request.PackedArea = area;
            }
            stopwatch.Stop();
            this.LastCalculationTime = stopwatch.Elapsed;

            // generate texture based on required size
            var width = this.textures.Max(t => t.PackedArea.Right);
            var height = this.textures.Max(t => t.PackedArea.Bottom);
            this.PackedTexture = new Texture2D(device, width, height);
            device.Disposing += (o, a) => this.PackedTexture.Dispose();

            // copy texture data onto the packed texture
            stopwatch.Restart();
            using (var data = this.PackedTexture.GetTextureData()) {
                foreach (var request in this.textures)
                    CopyRegion(data, request);
            }
            stopwatch.Stop();
            this.LastPackTime = stopwatch.Elapsed;

            // invoke callbacks
            foreach (var request in this.textures)
                request.Result.Invoke(new TextureRegion(this.PackedTexture, request.PackedArea));
        }

        private Rectangle FindFreeArea(Point size) {
            var pos = new Point(0, 0);
            var lowestY = int.MaxValue;
            while (true) {
                var intersected = false;
                var area = new Rectangle(pos, size);
                foreach (var tex in this.textures) {
                    if (tex.PackedArea.Intersects(area)) {
                        pos.X = tex.PackedArea.Right;
                        // when we move down, we want to move down by the smallest intersecting texture's height
                        if (lowestY > tex.PackedArea.Bottom)
                            lowestY = tex.PackedArea.Bottom;
                        intersected = true;
                        break;
                    }
                }
                if (!intersected)
                    return area;
                if (pos.X + size.X > this.maxWidth) {
                    pos.X = 0;
                    pos.Y = lowestY;
                    lowestY = int.MaxValue;
                }
            }
        }

        private static void CopyRegion(TextureExtensions.TextureData destination, Request request) {
            using (var data = request.Texture.Texture.GetTextureData()) {
                for (var x = 0; x < request.Texture.Width; x++) {
                    for (var y = 0; y < request.Texture.Height; y++) {
                        var dest = request.PackedArea.Location + new Point(x, y);
                        var src = request.Texture.Position + new Point(x, y);
                        destination[dest] = data[src];
                    }
                }
            }
        }

        private class Request {

            public readonly TextureRegion Texture;
            public readonly Action<TextureRegion> Result;
            public Rectangle PackedArea;

            public Request(TextureRegion texture, Action<TextureRegion> result) {
                this.Texture = texture;
                this.Result = result;
            }

        }

    }
}