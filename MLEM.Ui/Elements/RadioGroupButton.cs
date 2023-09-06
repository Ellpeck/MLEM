using Microsoft.Xna.Framework;
using MLEM.Ui.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A radio button element to use inside of a <see cref="UiSystem"/>.
    /// A radio button is a variation of a <see cref="Checkbox"/> that causes all other radio buttons in the same <see cref="RadioGroup"/> to be deselected upon selection.
    /// </summary>
    public class RadioGroupButton:Checkbox {

        /// <summary>
        /// The group that this radio button has.
        /// All other radio buttons in the same <see cref="RadioGroup"/> that have the same group will be deselected when this radio button is selected.
        /// </summary>
        public RadioGroup Group;

        /// <summary>
        /// Creates a new radio button with the given settings
        /// </summary>
        /// <param name="anchor">The radio button's anchor</param>
        /// <param name="size">The radio button's size</param>
        /// <param name="label">The label to display next to the radio button</param>
        /// <param name="defaultChecked">If the radio button should be checked by default</param>
        /// <param name="group">The group that the radio button has. Can be null</param>
        public RadioGroupButton(Anchor anchor, Vector2 size, string label, bool defaultChecked = false, RadioGroup group = null) :
            base(anchor, size, label, defaultChecked) {
            this.Group = group;

            // don't += because we want to override the checking/unchecking behavior of Checkbox
            this.OnPressed = element => {
                this.Checked = true;
                this.Root.Element.AndChildren(e => {
                    group.Selection = this;
                });
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
    /// <summary>
    /// A group of radio group buttons
    /// </summary>
    public class RadioGroup {
        //can be null
        private RadioGroupButton selection;
        /// <summary>
        /// The currently selected item, set to null to unselect all
        /// </summary>
        public RadioGroupButton Selection{ get => selection; set {
            if (selection != null) this.selection.Checked = false;
            selection = value;
            if(selection != null) {
                this.selection.Group = this;
                this.selection.Checked = true;
            }
        }}
    }
}
