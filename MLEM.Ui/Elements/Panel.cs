using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Panel : Element {

        public NinePatch Texture;

        public Panel(Anchor anchor, Vector2 size, Point positionOffset, bool setHeightBasedOnChildren = false, NinePatch texture = null) : base(anchor, size) {
            this.Texture = texture;
            this.PositionOffset = positionOffset;
            this.SetHeightBasedOnChildren = setHeightBasedOnChildren;
            this.ChildPadding = new Point(5);
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha) {
            batch.Draw(this.Texture, this.DisplayArea, Color.White * alpha);
            base.Draw(time, batch, alpha);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = style.PanelTexture;
        }

    }
}