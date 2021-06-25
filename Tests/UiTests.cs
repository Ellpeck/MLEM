using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Misc;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using NUnit.Framework;

namespace Tests {
    public class UiTests {

        private TestGame game;

        [SetUp]
        public void SetUp() {
            this.game = TestGame.Create();
        }

        [TearDown]
        public void TearDown() {
            this.game?.Dispose();
        }

        [Test]
        public void TestInvalidPanel() {
            var invalidPanel = new Panel(Anchor.Center, Vector2.Zero, Vector2.Zero) {
                SetWidthBasedOnChildren = true,
                SetHeightBasedOnChildren = true
            };
            invalidPanel.AddChild(new Paragraph(Anchor.AutoRight, 1, "This is some test text!", true));
            invalidPanel.AddChild(new VerticalSpace(1));
            Assert.Throws<ArithmeticException>(() => this.AddAndUpdate(invalidPanel));

            invalidPanel = new Panel(Anchor.Center, Vector2.Zero, Vector2.Zero, true);
            invalidPanel.AddChild(new Group(Anchor.CenterRight, new Vector2(10), false));
            invalidPanel.AddChild(new Group(Anchor.BottomLeft, new Vector2(10), false));
            Assert.Throws<InvalidOperationException>(() => this.AddAndUpdate(invalidPanel));
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
            this.AddAndUpdate(group);

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

        [Test]
        public void TestStyle() {
            var style = new StyleProp<string>();
            Assert.AreEqual(null, style.Value);
            style.SetFromStyle("from style");
            Assert.AreEqual("from style", style.Value);
            style.Set("custom");
            Assert.AreEqual("custom", style.Value);
            style.SetFromStyle("from style again");
            Assert.AreEqual("custom", style.Value);
        }

        [Test]
        public void TestAutoAreaPerformance() {
            var stopwatch = new Stopwatch();
            for (var i = 1; i <= 100; i++) {
                var totalUpdates = 0;
                var main = new Group(Anchor.TopLeft, new Vector2(50)) {
                    OnAreaUpdated = e => totalUpdates++
                };
                var group = main;
                for (var g = 0; g < i; g++) {
                    group = group.AddChild(new Group(Anchor.TopLeft, Vector2.One) {
                        OnAreaUpdated = e => totalUpdates++
                    });
                }
                stopwatch.Restart();
                this.AddAndUpdate(main);
                stopwatch.Stop();
                var allChildren = main.GetChildren(regardGrandchildren: true);
                TestContext.WriteLine($"{allChildren.Count()} children, {totalUpdates} updates total, took {stopwatch.Elapsed.TotalMilliseconds * 1000000}ns");
            }
        }

        private void AddAndUpdate(Element element) {
            foreach (var root in this.game.UiSystem.GetRootElements())
                this.game.UiSystem.Remove(root.Name);

            this.game.UiSystem.Add("Test", element);
            element.ForceUpdateArea();
        }

    }
}