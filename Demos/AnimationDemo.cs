using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MLEM.Animations;
using MLEM.Startup;
using MLEM.Textures;

namespace Demos {
    public class AnimationDemo : MlemGame {

        private SpriteAnimationGroup group;
        private int facing;

        protected override void LoadContent() {
            base.LoadContent();

            var tex = LoadContent<Texture2D>("Textures/Anim");
            
            // create the four animations by supplying the time per frame, the texture and the four regions used
            var downAnim = new SpriteAnimation(0.2F, tex, new Rectangle(0, 0, 8, 8), new Rectangle(0, 8, 8, 8), new Rectangle(0, 16, 8, 8), new Rectangle(0, 24, 8, 8));
            var upAnim = new SpriteAnimation(0.2F, tex, new Rectangle(8, 0, 8, 8), new Rectangle(8, 8, 8, 8), new Rectangle(8, 16, 8, 8), new Rectangle(8, 24, 8, 8));
            var leftAnim = new SpriteAnimation(0.2F, tex, new Rectangle(16, 0, 8, 8), new Rectangle(16, 8, 8, 8), new Rectangle(16, 16, 8, 8), new Rectangle(16, 24, 8, 8));
            var rightAnim = new SpriteAnimation(0.2F, tex, new Rectangle(24, 0, 8, 8), new Rectangle(24, 8, 8, 8), new Rectangle(24, 16, 8, 8), new Rectangle(24, 24, 8, 8));

            // create a sprite animation group which manages a list of animations and figures out which one should
            // be playing right now based on supplied conditions
            // using a group isn't necessary, but highly recommended for things like character animations as it makes
            // it very easy to have different animations play at different times
            this.group = new SpriteAnimationGroup();
            // for example, the down animation should only play when we're facing down (0 in this case)
            this.group.Add(downAnim, () => this.facing == 0);
            this.group.Add(upAnim, () => this.facing == 1);
            this.group.Add(leftAnim, () => this.facing == 2);
            this.group.Add(rightAnim, () => this.facing == 3);
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (this.InputHandler.IsKeyDown(Keys.Down))
                this.facing = 0;
            else if (this.InputHandler.IsKeyDown(Keys.Up))
                this.facing = 1;
            else if (this.InputHandler.IsKeyDown(Keys.Left))
                this.facing = 2;
            else if (this.InputHandler.IsKeyDown(Keys.Right))
                this.facing = 3;

            // update the animation group
            // if not using a group, just update the animation itself here
            this.group.Update(gameTime);
        }

        protected override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.Black);

            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: Matrix.CreateScale(10));
            // draw the group's current region
            // if not using a group, just draw the animation's CurrentRegion here
            this.SpriteBatch.Draw(this.group.CurrentRegion, new Vector2(10, 10), Color.White);
            this.SpriteBatch.End();

            base.DoDraw(gameTime);
        }

    }
}