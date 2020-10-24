using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;
using MLEM.Startup;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Demos {
    public class EasingsDemo : Demo {

        private static readonly FieldInfo[] EasingFields = typeof(Easings)
            .GetFields(BindingFlags.Public | BindingFlags.Static).ToArray();
        private static readonly Easings.Easing[] Easings = EasingFields
            .Select(f => (Easings.Easing) f.GetValue(null)).ToArray();
        private Group group;
        private int current;
        private float progress;

        public EasingsDemo(MlemGame game) : base(game) {
        }

        public override void LoadContent() {
            base.LoadContent();

            this.group = new Group(Anchor.TopCenter, Vector2.One) {SetWidthBasedOnChildren = true};
            this.group.AddChild(new Button(Anchor.AutoCenter, new Vector2(30, 10), "Next") {
                OnPressed = e => {
                    this.current = (this.current + 1) % Easings.Length;
                    this.progress = 0;
                }
            });
            this.group.AddChild(new Paragraph(Anchor.AutoCenter, 1, p => EasingFields[this.current].Name, true));
            this.UiRoot.AddChild(this.group);
        }

        public override void Clear() {
            this.group.Parent.RemoveChild(this.group);
        }

        public override void DoDraw(GameTime time) {
            this.GraphicsDevice.Clear(Color.CornflowerBlue);
            base.DoDraw(time);

            this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);
            var view = this.GraphicsDevice.Viewport;

            // graph the easing function
            var graphEase = Easings[this.current].ScaleInput(0, view.Width).ScaleOutput(-view.Height / 3, view.Height / 3);
            for (var x = 0; x < view.Width; x++) {
                var area = new RectangleF(x - 2, view.Height / 2 - graphEase(x) - 2, 4, 4);
                this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), area, Color.Green);
            }

            // draw a little dot to show what it would look like
            this.progress = (this.progress + (float) time.ElapsedGameTime.TotalSeconds / 2) % 1;
            var dotEase = Easings[this.current].AndReverse().ScaleOutput(0, view.Height / 4);
            var pos = new RectangleF(view.Width / 2 - 4, view.Height - 20 - dotEase(this.progress), 8, 8);
            this.SpriteBatch.Draw(this.SpriteBatch.GetBlankTexture(), pos, Color.Red);

            this.SpriteBatch.End();
        }

    }
}