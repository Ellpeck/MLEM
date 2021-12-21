using Microsoft.Xna.Framework;
using MLEM.Ui.Style;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A radio button element to use inside of a <see cref="UiSystem"/>.
    /// A radio button is a variation of a <see cref="Checkbox"/> that causes all other radio buttons in the same <see cref="Group"/> to be deselected upon selection.
    /// </summary>
    public class RadioButton : Checkbox {

        /// <summary>
        /// The group that this radio button has.
        /// All other radio buttons in the same <see cref="RootElement"/> that have the same group will be deselected when this radio button is selected.
        /// </summary>
        public string Group;

        /// <summary>
        /// Creates a new radio button with the given settings
        /// </summary>
        /// <param name="anchor">The radio button's anchor</param>
        /// <param name="size">The radio button's size</param>
        /// <param name="label">The label to display next to the radio button</param>
        /// <param name="defaultChecked">If the radio button should be checked by default</param>
        /// <param name="group">The group that the radio button has</param>
        public RadioButton(Anchor anchor, Vector2 size, string label, bool defaultChecked = false, string group = "") :
            base(anchor, size, label, defaultChecked) {
            this.Group = group;

            // don't += because we want to override the checking + unchecking behavior of Checkbox
            this.OnPressed = element => {
                this.Checked = true;
                foreach (var sib in this.GetSiblings()) {
                    if (sib is RadioButton radio && radio.Group == this.Group)
                        radio.Checked = false;
                }
            };
        }

        /// <inheritdoc />
        protected override void InitStyle(UiStyle style) {
            base.InitStyle(style);
            this.Texture = this.Texture.OrStyle(style.RadioTexture);
            this.HoveredTexture = this.HoveredTexture.OrStyle(style.RadioHoveredTexture);
            this.HoveredColor = this.HoveredColor.OrStyle(style.RadioHoveredColor);
            this.Checkmark = this.Checkmark.OrStyle(style.RadioCheckmark);
        }

    }
}