using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;
using static MLEM.Extensions.TextureExtensions;

namespace MLEM.Data {
    /// <summary>
    /// A runtime texture packer provides the user with the ability to combine multiple <see cref="Texture2D"/> instances into a single texture.
    /// Packing textures in this manner allows for faster rendering, as fewer texture swaps are required.
    /// The resulting texture segments are returned as <see cref="TextureRegion"/> instances.
    /// </summary>
    public class RuntimeTexturePacker : IDisposable {

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

        private readonly List<Request> texturesToPack = new List<Request>();
        private readonly List<Request> alreadyPackedTextures = new List<Request>();
        private readonly Dictionary<Point, Point> firstPossiblePosForSizeCache = new Dictionary<Point, Point>();
        private readonly Dictionary<Texture2D, TextureData> dataCache = new Dictionary<Texture2D, TextureData>();
        private readonly bool autoIncreaseMaxWidth;
        private readonly bool forcePowerOfTwo;
        private readonly bool forceSquare;
        private readonly bool disposeTextures;

        private int maxWidth;

        /// <summary>
        /// Creates a new runtime texture packer with the given settings.
        /// </summary>
        /// <param name="maxWidth">The maximum width that the packed texture can have. Defaults to 2048.</param>
        /// <param name="autoIncreaseMaxWidth">Whether the maximum width should be increased if there is a texture to be packed that is wider than <see cref="maxWidth"/>. Defaults to false.</param>
        /// <param name="forcePowerOfTwo">Whether the resulting <see cref="PackedTexture"/> should have a width and height that is a power of two.</param>
        /// <param name="forceSquare">Whether the resulting <see cref="PackedTexture"/> should be square regardless of required size.</param>
        /// <param name="disposeTextures">Whether the original textures submitted to this texture packer should be disposed after packing.</param>
        public RuntimeTexturePacker(int maxWidth = 2048, bool autoIncreaseMaxWidth = false, bool forcePowerOfTwo = false, bool forceSquare = false, bool disposeTextures = false) {
            this.maxWidth = maxWidth;
            this.autoIncreaseMaxWidth = autoIncreaseMaxWidth;
            this.forcePowerOfTwo = forcePowerOfTwo;
            this.forceSquare = forceSquare;
            this.disposeTextures = disposeTextures;
        }

        /// <summary>
        /// Adds a new <see cref="UniformTextureAtlas"/> to this texture packer to be packed.
        /// The passed <see cref="Action{T}"/> is invoked in <see cref="Pack"/> and provides the caller with the resulting dictionary of texture regions on the <see cref="PackedTexture"/>, mapped to their x and y positions on the original <see cref="UniformTextureAtlas"/>.
        /// Note that the resulting data cannot be converted back into a <see cref="UniformTextureAtlas"/>, since the resulting texture regions might be scattered throughout the <see cref="PackedTexture"/>.
        /// </summary>
        /// <param name="atlas">The texture atlas to pack.</param>
        /// <param name="result">The result callback which will receive the resulting texture regions.</param>
        /// <param name="padding">The padding that the texture should have around itself. This can be useful if texture bleeding issues occur due to texture coordinate rounding.</param>
        /// <param name="padWithPixels">Whether the texture's padding should be filled with a copy of the texture's border, rather than transparent pixels. This value only has an effect if <paramref name="padding"/> is greater than 0.</param>
        /// <param name="ignoreTransparent">Whether completely transparent texture regions in the <paramref name="atlas"/> should be ignored. If this is true, they will not be part of the <paramref name="result"/> collection either.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to add data to a packer that has already been packed, or when trying to add a texture width a width greater than the defined max width.</exception>
        public void Add(UniformTextureAtlas atlas, Action<Dictionary<Point, TextureRegion>> result, int padding = 0, bool padWithPixels = false, bool ignoreTransparent = false) {
            var addedRegions = new List<TextureRegion>();
            var resultRegions = new Dictionary<Point, TextureRegion>();
            for (var x = 0; x < atlas.RegionAmountX; x++) {
                for (var y = 0; y < atlas.RegionAmountY; y++) {
                    var pos = new Point(x, y);
                    var region = atlas[pos];

                    if (ignoreTransparent) {
                        if (this.IsTransparent(region))
                            continue;
                    }

                    this.Add(region, r => {
                        resultRegions.Add(pos, r);
                        if (resultRegions.Count >= addedRegions.Count)
                            result.Invoke(resultRegions);
                    }, padding, padWithPixels);
                    addedRegions.Add(region);
                }
            }
        }

        /// <summary>
        /// Adds a new <see cref="DataTextureAtlas"/> to this texture packer to be packed.
        /// The passed <see cref="Action{T}"/> is invoked in <see cref="Pack"/> and provides the caller with the resulting dictionary of texture regions on the <see cref="PackedTexture"/>, mapped to their name on the original <see cref="DataTextureAtlas"/>.
        /// Note that the resulting data cannot be converted back into a <see cref="DataTextureAtlas"/>, since the resulting texture regions might be scattered throughout the <see cref="PackedTexture"/>.
        /// </summary>
        /// <param name="atlas">The texture atlas to pack.</param>
        /// <param name="result">The result callback which will receive the resulting texture regions.</param>
        /// <param name="padding">The padding that the texture should have around itself. This can be useful if texture bleeding issues occur due to texture coordinate rounding.</param>
        /// <param name="padWithPixels">Whether the texture's padding should be filled with a copy of the texture's border, rather than transparent pixels. This value only has an effect if <paramref name="padding"/> is greater than 0.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to add data to a packer that has already been packed, or when trying to add a texture width a width greater than the defined max width.</exception>
        public void Add(DataTextureAtlas atlas, Action<Dictionary<string, TextureRegion>> result, int padding = 0, bool padWithPixels = false) {
            var atlasRegions = atlas.RegionNames.ToArray();
            var resultRegions = new Dictionary<string, TextureRegion>();
            foreach (var region in atlasRegions) {
                this.Add(atlas[region], r => {
                    resultRegions.Add(region, r);
                    if (resultRegions.Count >= atlasRegions.Length)
                        result.Invoke(resultRegions);
                }, padding, padWithPixels);
            }
        }

        /// <summary>
        /// Adds a new <see cref="Texture2D"/> to this texture packer to be packed.
        /// The passed <see cref="Action{T}"/> is invoked in <see cref="Pack"/> and provides the caller with the resulting texture region on the <see cref="PackedTexture"/>.
        /// </summary>
        /// <param name="texture">The texture to pack.</param>
        /// <param name="result">The result callback which will receive the resulting texture region.</param>
        /// <param name="padding">The padding that the texture should have around itself. This can be useful if texture bleeding issues occur due to texture coordinate rounding.</param>
        /// <param name="padWithPixels">Whether the texture's padding should be filled with a copy of the texture's border, rather than transparent pixels. This value only has an effect if <paramref name="padding"/> is greater than 0.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to add data to a packer that has already been packed, or when trying to add a texture width a width greater than the defined max width.</exception>
        public void Add(Texture2D texture, Action<TextureRegion> result, int padding = 0, bool padWithPixels = false) {
            this.Add(new TextureRegion(texture), result, padding, padWithPixels);
        }

        /// <summary>
        /// Adds a new <see cref="TextureRegion"/> to this texture packer to be packed.
        /// The passed <see cref="Action{T}"/> is invoked in <see cref="Pack"/> and provides the caller with the resulting texture region on the <see cref="PackedTexture"/>.
        /// </summary>
        /// <param name="texture">The texture region to pack.</param>
        /// <param name="result">The result callback which will receive the resulting texture region.</param>
        /// <param name="padding">The padding that the texture should have around itself. This can be useful if texture bleeding issues occur due to texture coordinate rounding.</param>
        /// <param name="padWithPixels">Whether the texture's padding should be filled with a copy of the texture's border, rather than transparent pixels. This value only has an effect if <paramref name="padding"/> is greater than 0.</param>
        /// <exception cref="InvalidOperationException">Thrown when trying to add data to a packer that has already been packed, or when trying to add a texture width a width greater than the defined max width.</exception>
        public void Add(TextureRegion texture, Action<TextureRegion> result, int padding = 0, bool padWithPixels = false) {
            if (this.PackedTexture != null)
                throw new InvalidOperationException("Cannot add texture to a texture packer that is already packed");
            var paddedWidth = texture.Width + 2 * padding;
            if (paddedWidth > this.maxWidth) {
                if (this.autoIncreaseMaxWidth) {
                    this.maxWidth = paddedWidth;
                } else {
                    throw new InvalidOperationException($"Cannot add texture with width {texture.Width} to a texture packer with max width {this.maxWidth}");
                }
            }
            this.texturesToPack.Add(new Request(texture, result, padding, padWithPixels));
        }

        /// <summary>
        /// Packs all of the textures and texture regions added using <see cref="Add(Microsoft.Xna.Framework.Graphics.Texture2D,System.Action{MLEM.Textures.TextureRegion},int,bool)"/> into one texture.
        /// The resulting texture will be stored in <see cref="PackedTexture"/>.
        /// All of the result callbacks that were added will also be invoked.
        /// </summary>
        /// <param name="device">The graphics device to use for texture generation</param>
        /// <exception cref="InvalidOperationException">Thrown when calling this method on a texture packer that has already been packed</exception>
        public void Pack(GraphicsDevice device) {
            if (this.PackedTexture != null)
                throw new InvalidOperationException("Cannot pack a texture packer that is already packed");

            // set pack areas for each request
            // we pack larger textures first, so that smaller textures can fit in the gaps that larger ones leave
            var stopwatch = Stopwatch.StartNew();
            foreach (var request in this.texturesToPack.OrderByDescending(t => t.Texture.Width * t.Texture.Height)) {
                request.PackedArea = this.FindFreeArea(request);
                // if this is the first position that this request fit in, no other requests of the same size will find a position before it
                this.firstPossiblePosForSizeCache[new Point(request.PackedArea.Width, request.PackedArea.Height)] = request.PackedArea.Location;
                this.alreadyPackedTextures.Add(request);
            }
            stopwatch.Stop();
            this.LastCalculationTime = stopwatch.Elapsed;

            // figure out texture size and generate texture
            var width = this.alreadyPackedTextures.Max(t => t.PackedArea.Right);
            var height = this.alreadyPackedTextures.Max(t => t.PackedArea.Bottom);
            if (this.forcePowerOfTwo) {
                width = RuntimeTexturePacker.ToPowerOfTwo(width);
                height = RuntimeTexturePacker.ToPowerOfTwo(height);
            }
            if (this.forceSquare)
                width = height = Math.Max(width, height);
            this.PackedTexture = new Texture2D(device, width, height);

            // copy texture data onto the packed texture
            stopwatch.Restart();
            using (var data = this.PackedTexture.GetTextureData()) {
                foreach (var request in this.alreadyPackedTextures)
                    this.CopyRegion(data, request);
            }
            stopwatch.Stop();
            this.LastPackTime = stopwatch.Elapsed;

            // invoke callbacks
            foreach (var request in this.alreadyPackedTextures) {
                var packedArea = request.PackedArea.Shrink(new Point(request.Padding, request.Padding));
                request.Result.Invoke(new TextureRegion(this.PackedTexture, packedArea) {
                    Pivot = request.Texture.Pivot,
                    Name = request.Texture.Name
                });
                if (this.disposeTextures)
                    request.Texture.Texture.Dispose();
            }

            this.ClearTempCollections();
        }

        /// <summary>
        /// Resets this texture packer, disposing its <see cref="PackedTexture"/> and readying it to be re-used
        /// </summary>
        public void Reset() {
            this.PackedTexture?.Dispose();
            this.PackedTexture = null;
            this.LastCalculationTime = TimeSpan.Zero;
            this.LastPackTime = TimeSpan.Zero;
            this.ClearTempCollections();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose() {
            this.Reset();
        }

        private Rectangle FindFreeArea(Request request) {
            var size = new Point(request.Texture.Width, request.Texture.Height);
            size.X += request.Padding * 2;
            size.Y += request.Padding * 2;

            var pos = this.firstPossiblePosForSizeCache.TryGetValue(size, out var first) ? first : Point.Zero;
            var lowestY = int.MaxValue;
            while (true) {
                var intersected = false;
                var area = new Rectangle(pos.X, pos.Y, size.X, size.Y);
                foreach (var tex in this.alreadyPackedTextures) {
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

        private void CopyRegion(TextureData destination, Request request) {
            var data = this.GetCachedTextureData(request.Texture.Texture);
            var location = request.PackedArea.Location + new Point(request.Padding, request.Padding);
            for (var x = -request.Padding; x < request.Texture.Width + request.Padding; x++) {
                for (var y = -request.Padding; y < request.Texture.Height + request.Padding; y++) {
                    Color srcColor;
                    if (!request.PadWithPixels && (x < 0 || y < 0 || x >= request.Texture.Width || y >= request.Texture.Height)) {
                        // if we're out of bounds and not padding with pixels, we make it transparent
                        srcColor = Color.Transparent;
                    } else {
                        // otherwise, we just use the closest pixel that is actually in bounds, causing the border pixels to be doubled up
                        var src = new Point((int) MathHelper.Clamp(x, 0F, request.Texture.Width - 1), (int) MathHelper.Clamp(y, 0F, request.Texture.Height - 1));
                        srcColor = data[request.Texture.Position + src];
                    }
                    destination[location + new Point(x, y)] = srcColor;
                }
            }
        }

        private TextureData GetCachedTextureData(Texture2D texture) {
            // we cache texture data in case multiple requests use the same underlying texture
            // this collection doesn't need to be disposed since we don't actually edit these textures
            if (!this.dataCache.TryGetValue(texture, out var data)) {
                data = texture.GetTextureData();
                this.dataCache.Add(texture, data);
            }
            return data;
        }

        private bool IsTransparent(TextureRegion region) {
            var data = this.GetCachedTextureData(region.Texture);
            for (var rX = 0; rX < region.Width; rX++) {
                for (var rY = 0; rY < region.Height; rY++) {
                    if (data[region.U + rX, region.V + rY] != Color.Transparent)
                        return false;
                }
            }
            return true;
        }

        private void ClearTempCollections() {
            this.texturesToPack.Clear();
            this.alreadyPackedTextures.Clear();
            this.firstPossiblePosForSizeCache.Clear();
            this.dataCache.Clear();
        }

        private static int ToPowerOfTwo(int value) {
            var ret = 1;
            while (ret < value)
                ret <<= 1;
            return ret;
        }

        private class Request {

            public readonly TextureRegion Texture;
            public readonly Action<TextureRegion> Result;
            public readonly int Padding;
            public readonly bool PadWithPixels;
            public Rectangle PackedArea;

            public Request(TextureRegion texture, Action<TextureRegion> result, int padding, bool padWithPixels) {
                this.Texture = texture;
                this.Result = result;
                this.Padding = padding;
                this.PadWithPixels = padWithPixels;
            }

        }

    }
}
