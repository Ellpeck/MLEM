using Microsoft.Xna.Framework;
using MLEM.Input;
using MLEM.Textures;

namespace MLEM.Ui.Elements {
    public static class ElementHelper {

        public static Button ImageButton(Anchor anchor, Vector2 size, TextureRegion texture, string text = null, string tooltipText = null, float tooltipWidth = 50, float imagePadding = 2) {
            var button = new Button(anchor, size, text, tooltipText, tooltipWidth);
            var image = new Image(Anchor.CenterLeft, Vector2.One, texture) {Padding = new Vector2(imagePadding)};
            button.OnAreaUpdated += e => image.Size = new Vector2(e.Area.Height, e.Area.Height) / e.Scale;
            button.AddChild(image, 0);
            return button;
        }

        public static Panel ShowInfoBox(UiSystem system, Anchor anchor, float width, string text, float buttonHeight = 10, string okText = "Okay") {
            var box = new Panel(anchor, new Vector2(width, 1), Vector2.Zero, true);
            box.AddChild(new Paragraph(Anchor.AutoLeft, 1, text));
            var button = box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, buttonHeight), okText) {
                OnPressed = element => system.Remove("InfoBox"),
                PositionOffset = new Vector2(0, 1)
            });
            var root = system.Add("InfoBox", box);
            root.SelectElement(button);
            return box;
        }

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

        public static Group NumberField(Anchor anchor, Vector2 size, int defaultValue = 0, int stepPerClick = 1, TextField.Rule rule = null, TextField.TextChanged onTextChange = null) {
            var group = new Group(anchor, size, false);

            var field = new TextField(Anchor.TopLeft, Vector2.One, rule ?? TextField.OnlyNumbers);
            field.OnTextChange = onTextChange;
            field.SetText(defaultValue.ToString());
            group.AddChild(field);
            group.OnAreaUpdated += e => field.Size = new Vector2((e.Area.Width - e.Area.Height / 2) / e.Scale, 1);

            var upButton = new Button(Anchor.TopRight, Vector2.One, "+") {
                OnPressed = element => {
                    var text = field.Text.ToString();
                    if (int.TryParse(text, out var val))
                        field.SetText(val + stepPerClick);
                }
            };
            group.AddChild(upButton);
            group.OnAreaUpdated += e => upButton.Size = new Vector2(e.Area.Height / 2 / e.Scale);

            var downButton = new Button(Anchor.BottomRight, Vector2.One, "-") {
                OnPressed = element => {
                    var text = field.Text.ToString();
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