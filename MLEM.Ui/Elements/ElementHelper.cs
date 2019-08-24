using Microsoft.Xna.Framework;
using MLEM.Input;

namespace MLEM.Ui.Elements {
    public static class ElementHelper {

        public static Group[] MakeColumns(Element parent, Vector2 totalSize, int amount, bool setHeightBasedOnChildren = true) {
            var cols = new Group[amount];
            for (var i = 0; i < amount; i++) {
                cols[i] = new Group(Anchor.AutoInline, new Vector2(totalSize.X / amount, totalSize.Y), setHeightBasedOnChildren);
                if (parent != null)
                    parent.AddChild(cols[i]);
            }
            return cols;
        }

        public static Group NumberField(Anchor anchor, Vector2 size, int defaultValue = 0, int stepPerClick = 1, TextField.Rule rule = null, TextField.TextChanged onTextChange = null) {
            var group = new Group(anchor, size, false);

            var field = new TextField(Anchor.TopLeft, Vector2.One, rule ?? TextField.OnlyNumbers);
            field.OnTextChange = onTextChange;
            field.AppendText(defaultValue.ToString());
            group.AddChild(field);
            group.OnAreaUpdated += e => field.Size = new Vector2((e.Area.Width - e.Area.Height / 2) / e.Scale, 1);

            var upButton = new Button(Anchor.TopRight, Vector2.One, "+") {
                OnClicked = (element, button) => {
                    if (button == MouseButton.Left) {
                        var text = field.Text.ToString();
                        if (int.TryParse(text, out var val))
                            field.SetText(val + stepPerClick);
                    }
                }
            };
            group.AddChild(upButton);
            group.OnAreaUpdated += e => upButton.Size = new Vector2(e.Area.Height / 2 / e.Scale);

            var downButton = new Button(Anchor.BottomRight, Vector2.One, "-") {
                OnClicked = (element, button) => {
                    if (button == MouseButton.Left) {
                        var text = field.Text.ToString();
                        if (int.TryParse(text, out var val))
                            field.SetText(val - stepPerClick);
                    }
                }
            };
            group.AddChild(downButton);
            group.OnAreaUpdated += e => downButton.Size = new Vector2(e.Area.Height / 2 / e.Scale);

            return group;
        }

    }
}