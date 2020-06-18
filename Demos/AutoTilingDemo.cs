using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Misc;
using MLEM.Startup;

namespace Demos {
    /// <summary>
    /// This is a demo for <see cref="AutoTiling"/>.
    /// </summary>
    public class AutoTilingDemo : Demo {

        private Texture2D texture;
        private string[] layout;

        public AutoTilingDemo(MlemGame game) : base(game) {
        }

        public override void LoadContent() {
            base.LoadContent();
            // The layout of the texture is important for auto tiling to work correctly.
            // It needs to be laid out as follows: Five tiles aligned horizontally within the texture file, with the following information
            // 1. The texture used for filling big areas
            // 2. The texture used for straight, horizontal borders, with the borders facing away from the center
            // 3. The texture used for outer corners, with the corners facing away from the center
            // 4. The texture used for straight, vertical borders, with the borders facing away from the center
            // 5. The texture used for inner corners, with the corners facing away from the center
            // Note that the AutoTiling.png texture contains an example of this layout
            this.texture = LoadContent<Texture2D>("Textures/AutoTiling");

            // in this example, a simple string array is used for layout purposes. As the AutoTiling method allows any kind of
            // comparison, the actual implementation can vary (for instance, based on a more in-depth tile map)
            this.layout = new[] {
                "XXX X",
                "XXXXX",
                "XX   ",
                "XXXX ",
                " X   "
            };
        }

        public override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);

            // the texture region supplied to the AutoTiling method should only encompass the first filler tile's location and size
            const int tileSize = 8;
            var region = new Rectangle(0, 0, tileSize, tileSize);

            // drawing the auto tiles
            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(10));
            for (var x = 0; x < 5; x++) {
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
                        if (x1 + xOff < 0 || y1 + yOff < 0 || x1 + xOff >= 5 || y1 + yOff >= 5)
                            return false;
                        // check if the neighboring tile is also grass (X)
                        return this.layout[y1 + yOff][x1 + xOff] == 'X';
                    }

                    AutoTiling.DrawAutoTile(this.SpriteBatch, new Vector2(x, y) * tileSize, this.texture, region, ConnectsTo, Color.White);
                }
            }
            this.SpriteBatch.End();

            base.DoDraw(gameTime);
        }

    }
}