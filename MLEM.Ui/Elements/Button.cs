using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Button : Element {

        public NinePatch Texture;
        public NinePatch HoveredTexture;
        public Color HoveredColor;
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

        public override void Draw(GameTime time, SpriteBatch batch, float alpha, Point offset) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                if (this.HoveredTexture != null)
                    tex = this.HoveredTexture;
                color = this.HoveredColor * alpha;
            }
            batch.Draw(tex, this.DisplayArea.OffsetCopy(offset), color, this.Scale);
            base.Draw(time, batch, alpha, offset);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = style.ButtonTexture;
            this.HoveredTexture = style.ButtonHoveredTexture;
            this.HoveredColor = style.ButtonHoveredColor;
        }

    }
}