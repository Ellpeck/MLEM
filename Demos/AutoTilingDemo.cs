using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Graphics;
using MLEM.Startup;

namespace Demos {
    /// <summary>
    /// This is a demo for <see cref="AutoTiling"/>.
    /// </summary>
    public class AutoTilingDemo : Demo {

        private const int TileSize = 8;
        private Texture2D texture;
        private string[] layout;

        public AutoTilingDemo(MlemGame game) : base(game) {
        }

        public override void LoadContent() {
            base.LoadContent();
            // The layout of the texture is important for auto tiling to work correctly, and is explained in the XML docs for the methods used
            this.texture = LoadContent<Texture2D>("Textures/AutoTiling");

            // in this example, a simple string array is used for layout purposes. As the AutoTiling method allows any kind of
            // comparison, the actual implementation can vary (for instance, based on a more in-depth tile map)
            this.layout = new[] {
                "XXX X ",
                "XXXXXX",
                "XXX  X",
                "XXXXX ",
                " X    "
            };
        }

        public override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);

            // drawing the auto tiles
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(10));
            for (var x = 0; x < 6; x++) {
                for (var y = 0; y < 5; y++) {
                    // don't draw non-grass tiles ( )
                    if (this.layout[y][x] != 'X')
                        continue;

                    var x1 = x;
                    var y1 = y;

                    // the connectsTo function determines for any given tile if it should connect to, that is, auto-tile with the
                    // neighbor in the supplied offset direction. In this example, the layout determines where grass tiles are (X)
                    // and where there are none ( ).
                    bool ConnectsTo(int xOff, int yOff) {
                        // don't auto-tile out of bounds
                        if (x1 + xOff < 0 || y1 + yOff < 0 || x1 + xOff >= 6 || y1 + yOff >= 5)
                            return false;
                        // check if the neighboring tile is also grass (X)
                        return this.layout[y1 + yOff][x1 + xOff] == 'X';
                    }

                    // the texture region supplied to the AutoTiling method should only encompass the first filler tile's location and size
                    AutoTiling.DrawAutoTile(this.SpriteBatch, new Vector2(x + 1, y + 1) * TileSize, this.texture, new Rectangle(0, 0, TileSize, TileSize), ConnectsTo, Color.White);

                    // when drawing extended auto-tiles, the same rules apply, but the source texture layout is different
                    AutoTiling.DrawExtendedAutoTile(this.SpriteBatch, new Vector2(x + 8, y + 1) * TileSize, this.texture, new Rectangle(0, TileSize * 2, TileSize, TileSize), ConnectsTo, Color.White);
                }
            }
            this.SpriteBatch.End();

            base.DoDraw(gameTime);
        }

    }
}