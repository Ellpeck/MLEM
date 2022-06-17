using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MLEM.Pathfinding;
using MLEM.Startup;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Demos {
    public class PathfindingDemo : Demo {

        private bool[,] world;
        private AStar2 pathfinder;
        private List<Point> path;
        private Button regenerateButton;

        public PathfindingDemo(MlemGame game) : base(game) {}

        private async void Init() {
            this.path = null;

            // generate a simple random world for testing, where true is walkable area, and false is a wall
            var random = new Random();
            this.world = new bool[50, 50];
            for (var x = 0; x < 50; x++) {
                for (var y = 0; y < 50; y++) {
                    if (random.NextDouble() >= 0.25)
                        this.world[x, y] = true;
                }
            }

            // Create a cost function, which determines how expensive (or difficult) it should be to move from a given position
            // to the next, adjacent position. In our case, the only restriction should be walls and out-of-bounds positions, which
            // both have a cost of AStar2.InfiniteCost, meaning they are completely unwalkable. 
            // If your game contains harder-to-move-on areas like, say, a muddy pit, you can return a higher cost value for those
            // locations. If you want to scale your cost function differently, you can specify a different default cost in your 
            // pathfinder's constructor
            float Cost(Point pos, Point nextPos) {
                if (nextPos.X < 0 || nextPos.Y < 0 || nextPos.X >= 50 || nextPos.Y >= 50)
                    return float.PositiveInfinity;
                return this.world[nextPos.X, nextPos.Y] ? 1 : float.PositiveInfinity;
            }

            // Actually initialize the pathfinder with the cost function, as well as specify if moving diagonally between tiles should be
            // allowed or not (in this case it's not)
            this.pathfinder = new AStar2(Cost, false);

            // Now find a path from the top left to the bottom right corner and store it in a variable
            // If no path can be found after the maximum amount of tries (10000 by default), the pathfinder will abort and return no path (null)
            var foundPath = await this.pathfinder.FindPathAsync(Point.Zero, new Point(49, 49));
            this.path = foundPath != null ? foundPath.ToList() : null;

            // print out some info
            Console.WriteLine("Pathfinding took " + this.pathfinder.LastTriesNeeded + " tries and " + this.pathfinder.LastTimeNeeded.TotalSeconds + " seconds");
            if (this.path == null) {
                Console.WriteLine("Couldn't find a path, trying again");
                this.Init();
            }
        }

        public override void LoadContent() {
            this.regenerateButton = new Button(Anchor.TopCenter, new Vector2(30, 10), "Regenerate") {
                OnPressed = e => this.Init()
            };
            this.UiRoot.AddChild(this.regenerateButton);
            this.Init();
        }

        public override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.White);

            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(14));
            var tex = this.SpriteBatch.GetBlankTexture();
            // draw the world with simple shapes
            for (var x = 0; x < 50; x++) {
                for (var y = 0; y < 50; y++) {
                    if (!this.world[x, y]) {
                        this.SpriteBatch.Draw(tex, new Rectangle(x, y, 1, 1), Color.Black);
                    }
                }
            }
            // draw the path
            // in a real game, you'd obviously make your characters walk along the path instead of drawing it
            if (this.path != null) {
                for (var i = 1; i < this.path.Count; i++) {
                    var (firstX, firstY) = this.path[i - 1];
                    var (secondX, secondY) = this.path[i];
                    this.SpriteBatch.Draw(tex, RectangleF.FromCorners(new Vector2(firstX + 0.25F, firstY + 0.25F), new Vector2(secondX + 0.75F, secondY + 0.75F)), Color.Blue);
                }
            }
            this.SpriteBatch.End();

            base.DoDraw(gameTime);
        }

        public override void Clear() {
            this.UiRoot.RemoveChild(this.regenerateButton);
        }

    }
}
