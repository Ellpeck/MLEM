using System;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using NUnit.Framework;
using Tests.Stub;

namespace Tests {
    public class UiTests {

        [Test]
        public void TestInvalidPanel() {
            var invalidPanel = new Panel(Anchor.Center, Vector2.Zero, Vector2.Zero) {
                SetWidthBasedOnChildren = true,
                SetHeightBasedOnChildren = true
            };
            invalidPanel.AddChild(new Paragraph(Anchor.AutoRight, 1, "This is some test text!", true));
            invalidPanel.AddChild(new VerticalSpace(1));
            Assert.Throws<ArithmeticException>(() => AddAndUpdate(invalidPanel));
        }

        [Test]
        public void TestComplexPanel() {
            var group = new Group(Anchor.TopLeft, Vector2.One, false);
            var panel = group.AddChild(new Panel(Anchor.Center, new Vector2(150, 150), Vector2.Zero, false, true, new Point(5, 10), false) {
                ChildPadding = new Padding(5, 10, 5, 5)
            });
            for (var i = 0; i < 5; i++) {
                var button = panel.AddChild(new Button(Anchor.AutoLeft, new Vector2(1)) {
                    SetHeightBasedOnChildren = true,
                    Padding = new Padding(0, 0, 0, 1),
                    ChildPadding = new Vector2(3)
                });
                button.AddChild(new Group(Anchor.AutoLeft, new Vector2(0.5F, 30), false) {
                    CanBeMoused = false
                });
            }
            AddAndUpdate(group);

            // group has 1 panel with 1 scroll bar, and the panel's 10 children
            Assert.AreEqual(1, group.GetChildren().Count());
            Assert.AreEqual(12, group.GetChildren(regardGrandchildren: true).Count());

            // panel 1 scroll bar and 5 buttons, each button has 1 group, so 11 grandchildren
            Assert.AreEqual(6, panel.GetChildren().Count());
            Assert.AreEqual(11, panel.GetChildren(regardGrandchildren: true).Count());

            var testBtn = panel.GetChildren<Button>().First();
            // panel's width is 150, minus child padding of 5 left and 10 right
            Assert.AreEqual(testBtn.DisplayArea.Width, 150 - 5 - 10);
            // button's width, minus child padding of 3 left and 3 right, divided by 2 because of group's width
            Assert.AreEqual(testBtn.GetChildren<Group>().Single().DisplayArea.Width, (150 - 5 - 10 - 3 - 3) * 0.5F);
        }

        private static void AddAndUpdate(Element element) {
            var ui = new UiSystem(null, new StubStyle(), null, false);
            ui.Viewport = new Rectangle(0, 0, 1280, 720);
            ui.Add("Test", element);
            element.ForceUpdateArea();
        }

    }
}