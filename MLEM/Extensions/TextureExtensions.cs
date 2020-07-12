using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="Texture2D"/>
    /// </summary>
    public static class TextureExtensions {

        /// <summary>
        /// Returns a new instance of <see cref="TextureData"/> which allows easily managing a texture's data with texture coordinates rather than indices.
        /// When this is used in a using statement, the texture data is automatically stored back in the texture at the end.
        /// </summary>
        /// <param name="texture">The texture whose data to get</param>
        /// <returns>The texture's data</returns>
        public static TextureData GetTextureData(this Texture2D texture) {
            return new TextureData(texture);
        }

        /// <summary>
        /// A struct that represents the data of a texture, accessed through <see cref="TextureExtensions.GetTextureData"/>.
        /// </summary>
        public class TextureData : IDisposable {

            private readonly Texture2D texture;
            private readonly Color[] data;
            /// <summary>
            /// Returns the color at the given x,y position of the texture, where 0,0 represents the bottom left.
            /// </summary>
            /// <param name="x">The x coordinate of the texture location</param>
            /// <param name="y">The y coordinate of the texture location</param>
            public Color this[int x, int y] {
                get => this.data[this.ToIndex(x, y)];
                set => this.data[this.ToIndex(x, y)] = value;
            }
            /// <inheritdoc cref="this[int,int]"/>
            public Color this[Point point] {
                get => this[point.X, point.Y];
                set => this[point.X, point.Y] = value;
            }

            /// <summary>
            /// Creates a new texture data instance for the given texture.
            /// Note that this can more easily be invoked using <see cref="TextureExtensions.GetTextureData"/>.
            /// </summary>
            /// <param name="texture">The texture whose data to get</param>
            public TextureData(Texture2D texture) {
                this.texture = texture;
                this.data = new Color[texture.Width * texture.Height];
                this.texture.GetData(this.data);
            }

            /// <summary>
            /// Stores this texture data back into the underlying texture
            /// </summary>
            public void Store() {
                this.texture.SetData(this.data);
            }

            /// <summary>
            /// Converts the given x,y texture coordinate to the corresponding index in the <see cref="Texture2D.GetData{T}(T[])"/> array.
            /// </summary>
            /// <param name="x">The x coordinate</param>
            /// <param name="y">The y coordinate</param>
            /// <returns>The corresponding texture array index</returns>
            /// <exception cref="ArgumentException">If the given coordinate is out of bounds</exception>
            public int ToIndex(int x, int y) {
                if (!this.IsInBounds(x, y))
                    throw new ArgumentException();
                return y * this.texture.Width + x;
            }

            /// <summary>
            /// Converts the given index from the <see cref="Texture2D.GetData{T}(T[])"/> array into the corresponding x,y texture coordinate.
            /// </summary>
            /// <param name="index">The texture array index</param>
            /// <returns>The corresponding texture coordinate</returns>
            /// <exception cref="ArgumentException">If the given index is out of bounds</exception>
            public Point FromIndex(int index) {
                if (index < 0 || index >= this.data.Length)
                    throw new ArgumentException();
                return new Point(index % this.texture.Width, index / this.texture.Width);
            }

            /// <summary>
            /// Checks if the given x,y texture coordinates is within the bounds of the underlying texture.
            /// </summary>
            /// <param name="x">The x coordinate</param>
            /// <param name="y">The y coordinate</param>
            /// <returns>Whether the given coordinate is within bounds of the underlying texture</returns>
            public bool IsInBounds(int x, int y) {
                return x >= 0 && y >= 0 && x < this.texture.Width && y < this.texture.Height;
            }

            /// <inheritdoc />
            public void Dispose() {
                this.Store();
            }

        }

    }
}