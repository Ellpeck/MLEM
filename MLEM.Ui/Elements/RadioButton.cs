using Microsoft.Xna.Framework;
using MLEM.Input;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    public class RadioButton : Checkbox {

        public string Group;

        public RadioButton(Anchor anchor, Vector2 size, string label, bool defaultChecked = false, string group = "") :
            base(anchor, size, label, defaultChecked) {
            this.Group = group;

            // don't += because we want to override the checking + unchecking behavior of Checkbox
            this.OnPressed = element => {
                this.Checked = true;
                foreach (var sib in this.GetSiblings(true)) {
                    if (sib is RadioButton radio && radio.Group == this.Group)
                        radio.Checked = false;
                }
            };
        }

        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = style.RadioTexture;
            this.HoveredTexture = style.RadioHoveredTexture;
            this.HoveredColor = style.RadioHoveredColor;
            this.Checkmark = style.RadioCheckmark;
        }

    }
}