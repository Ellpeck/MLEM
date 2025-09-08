using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Animations;
using MLEM.Maths;
using MLEM.Startup;
using MLEM.Textures;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Demos {
    public class AnimationDemo : Demo {

        private SpriteAnimationGroup group;
        private Direction2 facing = Direction2.Down;
        private Group buttons;
        private bool stop;

        public AnimationDemo(MlemGame game) : base(game) {}

        public override void LoadContent() {
            base.LoadContent();

            // create a uniform texture atlas with a width and height of 4
            // this means that, no matter how many pixels the texture has, it will always have 4 * 4 regions in total
            // this allows for texture artists to change the resolution of a texture atlas without every texture region
            // using it having wrong coordinates and/or sizes
            // the regions that are part of the atlas are then referenced by region coordinates rather than texture coordinates
            // (as seen below)
            var atlas = new UniformTextureAtlas(Demo.LoadContent<Texture2D>("Textures/Anim"), 4, 4);

            // create the four animations by supplying the time per frame, and the four regions used
            // note that you don't need to use a texture atlas for this, you can also simply supply the texture and the regions manually here
            var downAnim = new SpriteAnimation(0.2F, atlas[0, 0], atlas[0, 1], atlas[0, 2], atlas[0, 3]) {Name = "Down"};
            var upAnim = new SpriteAnimation(0.2F, atlas[1, 0], atlas[1, 1], atlas[1, 2], atlas[1, 3]) {Name = "Up"};
            var leftAnim = new SpriteAnimation(0.2F, atlas[2, 0], atlas[2, 1], atlas[2, 2], atlas[2, 3]) {Name = "Left"};
            var rightAnim = new SpriteAnimation(0.2F, atlas[3, 0], atlas[3, 1], atlas[3, 2], atlas[3, 3]) {Name = "Right"};

            // create a sprite animation group which manages a list of animations and figures out which one should
            // be playing right now based on supplied conditions
            // using a group isn't necessary, but highly recommended for things like character animations as it makes
            // it very easy to have different animations play at different times
            this.group = new SpriteAnimationGroup();
            // for example, the down animation should only play when we're facing down
            this.group.Add(downAnim, () => this.facing == Direction2.Down);
            this.group.Add(upAnim, () => this.facing == Direction2.Up);
            this.group.Add(leftAnim, () => this.facing == Direction2.Left);
            this.group.Add(rightAnim, () => this.facing == Direction2.Right);

            // you can also add a priority to an animation in the group (10 in this case, which is higher than the default of 0)
            // if two animations' playing conditions are both true, then the one with the higher priority will be picked to play
            // in this instance, a standing "animation" is displayed when we're facing down and also holding the space key
            this.group.Add(new SpriteAnimation(1F, atlas[0, 0]) {Name = "DownStanding"}, () => this.facing == Direction2.Down && this.stop, 10);
            this.group.Add(new SpriteAnimation(1F, atlas[1, 0]) {Name = "UpStanding"}, () => this.facing == Direction2.Up && this.stop, 10);
            this.group.Add(new SpriteAnimation(1F, atlas[2, 0]) {Name = "LeftStanding"}, () => this.facing == Direction2.Left && this.stop, 10);
            this.group.Add(new SpriteAnimation(1F, atlas[3, 0]) {Name = "RightStanding"}, () => this.facing == Direction2.Right && this.stop, 10);

            // you can also add a callback to see when the animation used changes
            this.group.OnAnimationChanged += (anim, newAnim) => {
                Console.WriteLine("Changing anim from " + (anim?.Name ?? "None") + " to " + (newAnim?.Name ?? "None"));
            };

            // some ui elements to interact with the character
            this.buttons = new Group(Anchor.TopCenter, Vector2.One) {SetWidthBasedOnChildren = true};
            this.UiRoot.AddChild(this.buttons);
            this.buttons.AddChild(new Button(Anchor.AutoInlineIgnoreOverflow, new Vector2(10), "^") {
                OnPressed = e => this.facing = Direction2.Up
            });
            this.buttons.AddChild(new Button(Anchor.AutoInlineIgnoreOverflow, new Vector2(10), "v") {
                OnPressed = e => this.facing = Direction2.Down
            });
            this.buttons.AddChild(new Button(Anchor.AutoInlineIgnoreOverflow, new Vector2(10), "<") {
                OnPressed = e => this.facing = Direction2.Left
            });
            this.buttons.AddChild(new Button(Anchor.AutoInlineIgnoreOverflow, new Vector2(10), ">") {
                OnPressed = e => this.facing = Direction2.Right
            });
            this.buttons.AddChild(new Button(Anchor.AutoCenter, new Vector2(30, 10), "Stand/Walk") {
                OnPressed = e => this.stop = !this.stop
            });
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
            // update the animation group
            // if not using a group, just update the animation itself here
            this.group.Update(gameTime);
        }

        public override void DoDraw(GameTime gameTime) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);

            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, Matrix.CreateScale(10));
            // draw the group's current region
            // if not using a group, just draw the animation's CurrentRegion here
            this.SpriteBatch.Draw(this.group.CurrentRegion, new Vector2(10, 10), Color.White);
            this.SpriteBatch.End();

            base.DoDraw(gameTime);
        }

        public override void Clear() {
            this.UiRoot.RemoveChild(this.buttons);
        }

    }
}
