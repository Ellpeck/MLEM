using System.Diagnostics.Tracing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Textures {
    public class TextureRegion {

        public readonly Texture2D Texture;
        public readonly Rectangle Area;
        public Point Position => this.Area.Location;
        public int U => this.Area.X;
        public int V => this.Area.Y;
        public Point Size => this.Area.Size;
        public int Width => this.Area.Width;
        public int Height => this.Area.Height;

        public TextureRegion(Texture2D texture, Rectangle area) {
            this.Texture = texture;
            this.Area = area;
        }

        public TextureRegion(Texture2D texture) : this(texture, new Rectangle(0, 0, texture.Width, texture.Height)) {
        }

        public TextureRegion(Texture2D texture, int u, int v, int width, int height) : this(texture, new Rectangle(u, v, width, height)) {
        }

        public TextureRegion(Texture2D texture, Point uv, Point size) : this(texture, new Rectangle(uv, size)) {
        }

    }

    public static class TextureRegionExtensions {

        public static void Draw(this SpriteBatch batch, TextureRegion texture, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture.Texture, position, texture.Area, color, rotation, origin, scale, effects, layerDepth);
        }

        public static void Draw(this SpriteBatch batch, TextureRegion texture, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture, position, color, rotation, origin, new Vector2(scale), SpriteEffects.None, layerDepth);
        }

        public static void Draw(this SpriteBatch batch, TextureRegion texture, Rectangle destinationRectangle, Color color, float rotation, Vector2 origin, SpriteEffects effects, float layerDepth) {
            batch.Draw(texture.Texture, destinationRectangle, texture.Area, color, rotation, origin, effects, layerDepth);
        }

        public static void Draw(this SpriteBatch batch, TextureRegion texture, Vector2 position, Color color) {
            batch.Draw(texture, position, color, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        }

        public static void Draw(this SpriteBatch batch, TextureRegion texture, Rectangle destinationRectangle, Color color) {
            batch.Draw(texture, destinationRectangle, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }

    }
}