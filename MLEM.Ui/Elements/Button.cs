using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class Button : Element {

        public NinePatch Texture;
        public NinePatch HoveredTexture;
        public Color HoveredColor;
        public AutoScaledText Text;

        public Button(Anchor anchor, Vector2 size, NinePatch texture, string text = null, NinePatch hoveredTexture = null, Color? hoveredColor = null) : base(anchor, size) {
            this.Texture = texture;
            this.HoveredTexture = hoveredTexture;
            this.HoveredColor = hoveredColor ?? Color.White;

            if (text != null) {
                this.Text = new AutoScaledText(Anchor.Center, Vector2.One, text, true) {
                    IgnoresMouse = true
                };
                this.AddChild(this.Text);
            }
        }

        public override void Draw(GameTime time, SpriteBatch batch, Color color) {
            var tex = this.Texture;
            if (this.IsMouseOver) {
                if (this.HoveredTexture != null)
                    tex = this.HoveredTexture;
                color = this.HoveredColor.CopyAlpha(color);
            }
            batch.Draw(tex, this.DisplayArea, color);
            base.Draw(time, batch, color);
        }

    }
}