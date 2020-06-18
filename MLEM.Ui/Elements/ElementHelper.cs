using Microsoft.Xna.Framework;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    /// <summary>
    /// This class contains a set of helper methods that aid in creating special kinds of compound <see cref="Element"/> types for use inside of a <see cref="UiSystem"/>.
    /// </summary>
    public static class ElementHelper {

        /// <summary>
        /// Creates a button with an image on the left side of its text.
        /// </summary>
        /// <param name="anchor">The button's anchor</param>
        /// <param name="size">The button's size</param>
        /// <param name="texture">The texture of the image to render on the button</param>
        /// <param name="text">The text to display on the button</param>
        /// <param name="tooltipText">The text of the button's tooltip</param>
        /// <param name="tooltipWidth">The width of the button's tooltip</param>
        /// <param name="imagePadding">The <see cref="Element.Padding"/> of the button's image</param>
        /// <returns>An image button</returns>
        public static Button ImageButton(Anchor anchor, Vector2 size, TextureRegion texture, string text = null, string tooltipText = null, float tooltipWidth = 50, float imagePadding = 2) {
            var button = new Button(anchor, size, text, tooltipText, tooltipWidth);
            var image = new Image(Anchor.CenterLeft, Vector2.One, texture) {Padding = new Vector2(imagePadding)};
            button.OnAreaUpdated += e => image.Size = new Vector2(e.Area.Height, e.Area.Height) / e.Scale;
            button.AddChild(image, 0);
            return button;
        }

        /// <summary>
        /// Creates a panel that contains a paragraph of text and a button to close the panel.
        /// The panel is part of a group, which causes elements in the background (behind and around the panel) to not be clickable, leaving only the "close" button.
        /// </summary>
        /// <param name="system">The ui system to add the panel to, optional.</param>
        /// <param name="anchor">The anchor of the panel</param>
        /// <param name="width">The width of the panel</param>
        /// <param name="text">The text to display on the panel</param>
        /// <param name="buttonHeight">The height of the "close" button</param>
        /// <param name="okText">The text on the "close" button</param>
        /// <returns>An info box panel</returns>
        public static Panel ShowInfoBox(UiSystem system, Anchor anchor, float width, string text, float buttonHeight = 10, string okText = "Okay") {
            var group = new Group(Anchor.TopLeft, Vector2.One, false);
            var box = group.AddChild(new Panel(anchor, new Vector2(width, 1), Vector2.Zero, true));
            box.AddChild(new Paragraph(Anchor.AutoLeft, 1, text));
            var button = box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, buttonHeight), okText) {
                OnPressed = element => system.Remove("InfoBox"),
                PositionOffset = new Vector2(0, 1)
            });
            var root = system.Add("InfoBox", group);
            root.SelectElement(button);
            return box;
        }

        /// <summary>
        /// Creates an array of groups with a fixed width that can be used to create a column structure
        /// </summary>
        /// <param name="parent">The element the groups should be added to, optional.</param>
        /// <param name="totalSize">The total width of all of the groups combined</param>
        /// <param name="amount">The amount of groups to split the total size into</param>
        /// <param name="setHeightBasedOnChildren">Whether the groups should set their heights based on their children's heights</param>
        /// <returns>An array of columns</returns>
        public static Group[] MakeColumns(Element parent, Vector2 totalSize, int amount, bool setHeightBasedOnChildren = true) {
            var cols = new Group[amount];
            for (var i = 0; i < amount; i++) {
                var anchor = i == amount - 1 ? Anchor.AutoInlineIgnoreOverflow : Anchor.AutoInline;
                cols[i] = new Group(anchor, new Vector2(totalSize.X / amount, totalSize.Y), setHeightBasedOnChildren);
                if (parent != null)
                    parent.AddChild(cols[i]);
            }
            return cols;
        }

        /// <summary>
        /// Creates a <see cref="TextField"/> with a + and a - button next to it, to allow for easy number input.
        /// </summary>
        /// <param name="anchor">The text field's anchor</param>
        /// <param name="size">The size of the text field</param>
        /// <param name="defaultValue">The default content of the text field</param>
        /// <param name="stepPerClick">The value that is added or removed to the text field's value when clicking the + or - buttons</param>
        /// <param name="rule">The rule for text input. <see cref="TextField.OnlyNumbers"/> by default.</param>
        /// <param name="onTextChange">A callback that is invoked when the text field's text changes</param>
        /// <returns>A group that contains the number field</returns>
        public static Group NumberField(Anchor anchor, Vector2 size, int defaultValue = 0, int stepPerClick = 1, TextField.Rule rule = null, TextField.TextChanged onTextChange = null) {
            var group = new Group(anchor, size, false);

            var field = new TextField(Anchor.TopLeft, Vector2.One, rule ?? TextField.OnlyNumbers);
            field.OnTextChange = onTextChange;
            field.SetText(defaultValue.ToString());
            group.AddChild(field);
            group.OnAreaUpdated += e => field.Size = new Vector2((e.Area.Width - e.Area.Height / 2) / e.Scale, 1);

            var upButton = new Button(Anchor.TopRight, Vector2.One, "+") {
                OnPressed = element => {
                    var text = field.Text;
                    if (int.TryParse(text, out var val))
                        field.SetText(val + stepPerClick);
                }
            };
            group.AddChild(upButton);
            group.OnAreaUpdated += e => upButton.Size = new Vector2(e.Area.Height / 2 / e.Scale);

            var downButton = new Button(Anchor.BottomRight, Vector2.One, "-") {
                OnPressed = element => {
                    var text = field.Text;
                    if (int.TryParse(text, out var val))
                        field.SetText(val - stepPerClick);
                }
            };
            group.AddChild(downButton);
            group.OnAreaUpdated += e => downButton.Size = new Vector2(e.Area.Height / 2 / e.Scale);

            return group;
        }

    }
}