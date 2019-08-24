using Microsoft.Xna.Framework;
using MLEM.Input;

namespace MLEM.Ui.Elements {
    public static class ElementHelper {

        public static Panel ShowInfoBox(UiSystem system, Anchor anchor, float width, string text, float buttonHeight = 10, string okText = "Okay") {
            var box = new Panel(anchor, new Vector2(width, 1), Vector2.Zero, true);
            box.AddChild(new Paragraph(Anchor.AutoLeft, 1, text));
            box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, buttonHeight), okText) {
                OnClicked = (element, button) => {
                    if (button == MouseButton.Left)
                        system.Remove("InfoBox");
                },
                PositionOffset = new Vector2(0, 1)
            });
            system.Add("InfoBox", box);
            return box;
        }

        public static Group[] MakeColumns(Element parent, Anchor anchor, Vector2 totalSize, int amount, bool setHeightBasedOnChildren = true) {
            var group = new Group(anchor, totalSize, setHeightBasedOnChildren);
            var cols = new Group[amount];
            for (var i = 0; i < amount; i++) {
                cols[i] = new Group(Anchor.AutoInline, new Vector2(totalSize.X / amount, totalSize.Y), setHeightBasedOnChildren);
                group.AddChild(cols[i]);
            }
            if (parent != null)
                parent.AddChild(group);
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