using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Textures {
    /// <summary>
    /// This class represents a part of a texture.
    /// </summary>
    public class TextureRegion : GenericDataHolder {

        /// <summary>
        /// The texture that this region is a part of
        /// </summary>
        public readonly Texture2D Texture;
        /// <summary>
        /// The area that is covered by this texture region
        /// </summary>
        public readonly Rectangle Area;
        /// <summary>
        /// The top left corner of this texture region
        /// </summary>
        public Point Position => this.Area.Location;
        /// <summary>
        /// The x coordinate of the top left corner of this texture region
        /// </summary>
        public int U => this.Area.X;
        /// <summary>
        /// The y coordinate of the top left corner of this texture region
        /// </summary>
        public int V => this.Area.Y;
        /// <summary>
        /// The size of this texture region
        /// </summary>
        public Point Size => this.Area.Size;
        /// <summary>
        /// The width of this texture region
        /// </summary>
        public int Width => this.Area.Width;
        /// <summary>
        /// The height of this texture region
        /// </summary>
        public int Height => this.Area.Height;

        /// <summary>
        /// Creates a new texture region from a texture and a rectangle which defines the region's area
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="area">The area that this texture region should cover</param>
        public TextureRegion(Texture2D texture, Rectangle area) {
            this.Texture = texture;
            this.Area = area;
        }

        /// <summary>
        /// Creates a new texture region that spans the entire texture
        /// </summary>
        /// <param name="texture">The texture to use</param>
        public TextureRegion(Texture2D texture) : this(texture, new Rectangle(0, 0, texture.Width, texture.Height)) {
        }

        /// <summary>
        /// Creates a new texture region based on a texture and area coordinates
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="u">The x coordinate of the top left corner of this area</param>
        /// <param name="v">The y coordinate of the top left corner of this area</param>
        /// <param name="width">The width of this area</param>
        /// <param name="height">The height of this area</param>
        public TextureRegion(Texture2D texture, int u, int v, int width, int height) : this(texture, new Rectangle(u, v, width, height)) {
        }

        /// <summary>
        /// Creates a new texture region based on a texture, a position and a size
        /// </summary>
        /// <param name="texture">The texture to use</param>
        /// <param name="uv">The top left corner of this area</param>
        /// <param name="size">The size of this area</param>
        public TextureRegion(Texture2D texture, Point uv, Point size) : this(texture, new Rectangle(uv, size)) {
        }

    }

    /// <summary>
    /// This class provides a set of extension methods for dealing with <see cref="TextureRegion"/>
    /// </summary>
    public static class TextureRegionExtensions {

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture.Texture, position, texture.Area, color, rotation, origin, scale, effects, layerDepth);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture, position, color, rotation, origin, new Vector2(scale), effects, layerDepth);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture.Texture, destinationRectangle, texture.Area, color, rotation, origin, effects, layerDepth);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, RectangleF destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture.Texture, destinationRectangle, texture.Area, color, rotation, origin, effects, layerDepth);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, Vector2 position, Color color) {
            batch.Draw(texture, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, Rectangle destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

        /// <inheritdoc cref="SpriteBatch.Draw(Texture2D, Vector2, Rectangle?, Color, float, Vector2, Vector2, SpriteEffects, float)"/>
        public static void Draw(this SpriteBatch batch, TextureRegion texture, RectangleF destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

    }
}