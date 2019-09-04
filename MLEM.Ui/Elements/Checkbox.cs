using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Input;
using MLEM.Textures;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class Checkbox : Element {

        public NinePatch Texture;
        public NinePatch HoveredTexture;
        public Color HoveredColor;
        public TextureRegion Checkmark;
        public Paragraph Label;
        public float TextOffsetX = 2;

        private bool checced;
        public bool Checked {
            get => this.checced;
            set {
                if (this.checced != value) {
                    this.checced = value;
                    this.OnCheckStateChange?.Invoke(this, this.checced);
                }
            }
        }
        public CheckStateChange OnCheckStateChange;

        public Checkbox(Anchor anchor, Vector2 size, string label, bool defaultChecked = false) : base(anchor, size) {
            this.checced = defaultChecked;
            this.OnPressed += element => this.Checked = !this.Checked;

            if (label != null) {
                this.Label = new Paragraph(Anchor.CenterLeft, 0, label);
                this.AddChild(this.Label);
            }
        }

        protected override Point CalcActualSize(Rectangle parentArea) {
            var size = base.CalcActualSize(parentArea);
            if (this.Label != null) {
                this.Label.Size = new Vector2((size.X - size.Y) / this.Scale - this.TextOffsetX, 1);
                this.Label.PositionOffset = new Vector2(size.Y / this.Scale + this.TextOffsetX, 0);
            }
            return size;
        }

        public override void Draw(GameTime time, SpriteBatch batch, float alpha) {
            var tex = this.Texture;
            var color = Color.White * alpha;
            if (this.IsMouseOver) {
                if (this.HoveredTexture != null)
                    tex = this.HoveredTexture;
                color = this.HoveredColor * alpha;
            }

            var boxDisplayArea = new Rectangle(this.DisplayArea.Location, new Point(this.DisplayArea.Height));
            batch.Draw(tex, boxDisplayArea, color, this.Scale);
            if (this.Checked)
                batch.Draw(this.Checkmark, boxDisplayArea, Color.White * alpha);
            base.Draw(time, batch, alpha);
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = style.CheckboxTexture;
            this.HoveredTexture = style.CheckboxHoveredTexture;
            this.HoveredColor = style.CheckboxHoveredColor;
            this.Checkmark = style.CheckboxCheckmark;
        }

        public delegate void CheckStateChange(Checkbox box, bool checced);

    }
}