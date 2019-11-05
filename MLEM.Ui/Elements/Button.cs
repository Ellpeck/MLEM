using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Button : Element {

        public StyleProp<NinePatch> Texture;
        public StyleProp<NinePatch> HoveredTexture;
        public StyleProp<Color> HoveredColor;
        public Paragraph Text;
        public Tooltip Tooltip;

        public Button(Anchor anchor, Vector2 size, string text = null, string tooltipText = null, float tooltipWidth = 50) : base(anchor, size) {
            if (text != null) {
                this.Text = new Paragraph(Anchor.Center, 1, text, true);
                this.AddChild(this.Text);
            }
            if (tooltipText != null)
                this.Tooltip = new Tooltip(tooltipWidth, tooltipText, this);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, BlendState blendState, SamplerState samplerState, Matrix matrix) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                tex = this.HoveredTexture.OrDefault(tex);
                color = (Color) this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea, color, this.Scale);
            base.Draw(time, batch, alpha, blendState, samplerState, matrix);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture.SetFromStyle(style.ButtonTexture);
            this.HoveredTexture.SetFromStyle(style.ButtonHoveredTexture);
            this.HoveredColor.SetFromStyle(style.ButtonHoveredColor);
        }

    }
}