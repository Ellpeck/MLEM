using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public class Button : Element {

        public static NinePatch DefaultTexture;
        public static NinePatch DefaultHoveredTexture;
        public static Color DefaultHoveredColor = Color.LightGray;

        public NinePatch Texture;
        public NinePatch HoveredTexture;
        public Color HoveredColor;
        public AutoScaledText Text;

        public Button(Anchor anchor, Vector2 size, NinePatch texture = null, string text = null, Color? hoveredColor = null, NinePatch hoveredTexture = null) : base(anchor, size) {
            this.Texture = texture ?? DefaultTexture;
            this.HoveredTexture = hoveredTexture ?? DefaultHoveredTexture;
            this.HoveredColor = hoveredColor ?? DefaultHoveredColor;

            if (text != null) {
                this.Text = new AutoScaledText(Anchor.Center, Vector2.One, text) {
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