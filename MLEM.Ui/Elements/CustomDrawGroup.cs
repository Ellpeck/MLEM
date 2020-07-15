using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// A <see cref="Group"/> that can have custom drawing parameters.
    /// Custom drawing parameters include a <see cref="Element.Transform"/> matrix, as well as a custom <see cref="SpriteBatch.Begin"/> call.
    /// All <see cref="Element.Children"/> of the custom draw group will be drawn with the custom parameters.
    /// </summary>
    [Obsolete("CustomDrawGroup mechanics have been moved down to Element, which means custom draw groups are no longer necessary.")]
    public class CustomDrawGroup : Group {

        /// <summary>
        /// Creates a new custom draw group with the given settings
        /// </summary>
        /// <param name="anchor">The group's anchor</param>
        /// <param name="size">The group's size</param>
        /// <param name="transformGetter">The group's <see cref="Element.TransformGetter"/></param>
        /// <param name="beginImpl">The group's <see cref="Element.BeginImpl"/></param>
        /// <param name="setHeightBasedOnChildren">Whether this group should automatically calculate its height based on its children</param>
        public CustomDrawGroup(Anchor anchor, Vector2 size, TransformCallback transformGetter = null, BeginDelegate beginImpl = null, bool setHeightBasedOnChildren = true) :
            base(anchor, size, setHeightBasedOnChildren) {
            this.TransformGetter = transformGetter ?? ((element, time, matrix) => Matrix.Identity);
            this.BeginImpl = beginImpl;
        }

        ///<inheritdoc cref="Element.ScaleTransform"/>
        [Obsolete("Use ScaleTransform instead")]
        public void ScaleOrigin(float scale, Vector2? origin = null) {
            this.ScaleTransform(scale, origin);
        }

    }
}