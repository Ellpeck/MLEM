using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using NUnit.Framework;

namespace MLEM.Tests;

public class UiTests : GameTestFixture {

    [Test]
    public void TestInvalidPanel() {
        var invalidPanel = new Panel(Anchor.Center, Vector2.Zero, Vector2.Zero) {
            SetWidthBasedOnChildren = true,
            SetHeightBasedOnChildren = true
        };
        invalidPanel.AddChild(new Paragraph(Anchor.AutoRight, 1, "This is some test text!", true));
        invalidPanel.AddChild(new VerticalSpace(1));
        Assert.Throws<ArithmeticException>(() => this.AddAndUpdate(invalidPanel, out _, out _));
    }

    [Test]
    public void TestOddlyAlignedPanel() {
        var oddPanel = new Panel(Anchor.Center, Vector2.One, Vector2.Zero, true) {SetWidthBasedOnChildren = true};
        oddPanel.AddChild(new Group(Anchor.TopCenter, new Vector2(100), false));
        oddPanel.AddChild(new Group(Anchor.AutoRight, new Vector2(120), false));
        this.AddAndUpdate(oddPanel, out _, out _);
        Assert.AreEqual(120 + 10, oddPanel.DisplayArea.Width);
    }

    [Test]
    public void TestComplexPanel() {
        var group = new Group(Anchor.TopLeft, Vector2.One, false);
        var panel = group.AddChild(new Panel(Anchor.Center, new Vector2(150, 150), Vector2.Zero, false, true, false) {
            ChildPadding = new Padding(5, 5, 5, 5)
        });
        for (var i = 0; i < 5; i++) {
            var button = panel.AddChild(new Button(Anchor.AutoLeft, new Vector2(1)) {
                SetHeightBasedOnChildren = true,
                Padding = new Padding(0, 0, 0, 1),
                ChildPadding = new Padding(3)
            });
            button.AddChild(new Group(Anchor.AutoLeft, new Vector2(0.5F, 30), false) {
                CanBeMoused = false
            });
        }
        this.AddAndUpdate(group, out _, out _);

        // group has 1 panel with 1 scroll bar, and the panel's 10 children
        Assert.AreEqual(1, group.GetChildren().Count());
        Assert.AreEqual(12, group.GetChildren(regardGrandchildren: true).Count());

        // panel 1 scroll bar and 5 buttons, each button has 1 group, so 11 grandchildren
        Assert.AreEqual(6, panel.GetChildren().Count());
        Assert.AreEqual(11, panel.GetChildren(regardGrandchildren: true).Count());

        var testBtn = panel.GetChildren<Button>().First();
        // panel's width is 150, minus child padding of 5 on each side, and scroll bar's width of 5 and gap of 1
        const int panelContentWidth = 150 - 5 - 5 - 5 - 1;
        Assert.AreEqual(testBtn.DisplayArea.Width, panelContentWidth);
        // button's width, minus child padding of 3 left and 3 right, divided by 2 because of group's width
        Assert.AreEqual(testBtn.GetChildren<Group>().Single().DisplayArea.Width, (panelContentWidth - 3 - 3) * 0.5F);
    }

    [Test]
    public void TestAbsoluteAutoSize() {
        var parent = new Panel(Anchor.AutoLeft, new Vector2(200, 100), Vector2.Zero) {
            ChildPadding = Padding.Empty
        };
        var el1 = parent.AddChild(new Button(Anchor.AutoLeft, new Vector2(0.5F, 0.75F)) {
            AutoSizeAddedAbsolute = new Vector2(-50, 25)
        });
        var el2 = parent.AddChild(new Button(Anchor.AutoLeft, new Vector2(0.25F, -0.5F)) {
            AutoSizeAddedAbsolute = new Vector2(-25, 50)
        });
        this.AddAndUpdate(parent, out _, out _);

        Assert.AreEqual(0.5F * 200 - 50, el1.DisplayArea.Width);
        Assert.AreEqual(0.75F * 100 + 25, el1.DisplayArea.Height);

        const float el2Width = 0.25F * 200 - 25;
        Assert.AreEqual(el2Width, el2.DisplayArea.Width);
        Assert.AreEqual(0.5F * el2Width + 50, el2.DisplayArea.Height);
    }

    [Test]
    public void TestStyle() {
        var style = new StyleProp<string>();
        Assert.AreEqual(null, style.Value);
        style = style.OrStyle("from style");
        Assert.AreEqual("from style", style.Value);
        style = "custom";
        Assert.AreEqual("custom", style.Value);
        style = style.OrStyle("from style again");
        Assert.AreEqual("custom", style.Value);

        var copy = style.OrStyle("copy from style", byte.MaxValue);
        var weakCopy = style.OrStyle("weak copy");
        Assert.AreEqual("copy from style", copy.Value);
        Assert.AreEqual("custom", weakCopy.Value);
        Assert.AreEqual("custom", style.Value);
    }

    [Test]
    public void TestAutoAreaPerformanceDeep() {
        for (var i = 1; i <= 100; i++) {
            var main = new Group(Anchor.TopLeft, new Vector2(50));
            var group = main;
            for (var g = 0; g < i; g++)
                group = group.AddChild(new Group(Anchor.TopLeft, Vector2.One));
            this.AddAndUpdate(main, out var addTime, out var updateTime);
            var allChildren = main.GetChildren(regardGrandchildren: true);
            TestContext.WriteLine($"{allChildren.Count()} children, took {addTime.TotalMilliseconds * 1000000}ns to add, {updateTime.TotalMilliseconds * 1000000}ns to update, metrics {this.Game.UiSystem.Metrics}");
        }
    }

    [Test]
    public void TestAutoAreaPerformanceSideBySide() {
        for (var i = 1; i <= 100; i++) {
            var main = new Group(Anchor.TopLeft, new Vector2(50));
            for (var g = 0; g < i; g++)
                main.AddChild(new Group(Anchor.AutoInlineIgnoreOverflow, new Vector2(1F / i, 1)));
            this.AddAndUpdate(main, out var addTime, out var updateTime);
            var allChildren = main.GetChildren(regardGrandchildren: true);
            TestContext.WriteLine($"{allChildren.Count()} children, took {addTime.TotalMilliseconds * 1000000}ns to add, {updateTime.TotalMilliseconds * 1000000}ns to update, metrics {this.Game.UiSystem.Metrics}");
        }
    }

    [Test]
    public void TestAutoAreaPerformanceRandom() {
        for (var i = 0; i <= 1000; i += 100) {
            var random = new Random(93829345);
            var main = new Group(Anchor.TopLeft, new Vector2(50));
            var group = main;
            for (var g = 0; g < i; g++) {
                var newGroup = group.AddChild(new Group(Anchor.TopLeft, Vector2.One));
                if (random.NextSingle() <= 0.25F)
                    group = newGroup;
            }
            this.AddAndUpdate(main, out var addTime, out var updateTime);
            var allChildren = main.GetChildren(regardGrandchildren: true);
            TestContext.WriteLine($"{allChildren.Count()} children, took {addTime.TotalMilliseconds * 1000000}ns to add, {updateTime.TotalMilliseconds * 1000000}ns to update, metrics {this.Game.UiSystem.Metrics}");
        }
    }

    // Stack overflow related to panel scrolling and scrollbar auto-hiding
    [Test]
    public void TestIssue27([Values(5, 50, 15)] int numChildren) {
        var group = new SquishingGroup(Anchor.TopLeft, Vector2.One);

        var centerGroup = new ScissorGroup(Anchor.TopCenter, Vector2.One);
        var centerPanel = new Panel(Anchor.TopRight, Vector2.One);
        centerPanel.DrawColor = Color.Red;
        centerPanel.Padding = new MLEM.Maths.Padding(5);
        centerGroup.AddChild(centerPanel);
        group.AddChild(centerGroup);

        var leftColumn = new Panel(Anchor.TopLeft, new Vector2(500, 1), scrollOverflow: true);
        group.AddChild(leftColumn);
        for (var i = 0; i < numChildren; i++) {
            var c = new Panel(Anchor.AutoLeft, new Vector2(1, 30));
            c.DrawColor = Color.Green;
            c.Padding = new MLEM.Maths.Padding(5);
            leftColumn.AddChild(c);
        }

        var bottomPane = new Panel(Anchor.BottomCenter, new Vector2(1, 500));
        group.AddChild(bottomPane);

        Assert.DoesNotThrow(() => this.AddAndUpdate(group, out _, out _));
    }

    // Removing and re-adding to a scrolling panel causes a stack overflow
    [Test]
    public void TestIssue29StackOverflow([Values(5, 50, 15)] int numChildren) {
        var group = new SquishingGroup(Anchor.TopLeft, Vector2.One);

        var centerGroup = new ScissorGroup(Anchor.TopCenter, Vector2.One);
        var centerPanel = new Panel(Anchor.TopRight, Vector2.One);
        centerPanel.DrawColor = Color.Red;
        centerPanel.Padding = new MLEM.Maths.Padding(5);
        centerGroup.AddChild(centerPanel);
        group.AddChild(centerGroup);

        var leftColumn = new SquishingGroup(Anchor.TopLeft, new Vector2(500, 1));
        group.AddChild(leftColumn);
        var namePanel = new Panel(Anchor.TopLeft, new Vector2(1, 50), true);
        var test = new Panel(Anchor.TopCenter, new Vector2(1, 30));
        test.DrawColor = Color.Red;
        namePanel.AddChild(test);
        var listView = new Panel(Anchor.TopLeft, new Vector2(1, 1), false, true);
        leftColumn.AddChild(listView);
        leftColumn.AddChild(namePanel);

        var bottomPane = new Panel(Anchor.BottomCenter, new Vector2(1, 500));
        group.AddChild(bottomPane);

        Repopulate();
        Assert.DoesNotThrow(() => this.AddAndUpdate(group, out _, out _));
        Repopulate();
        Assert.DoesNotThrow(() => UiTests.ForceUpdate(group, out _));

        void Repopulate() {
            listView.RemoveChildren();
            for (var i = 0; i < numChildren; i++) {
                var c = new Panel(Anchor.AutoLeft, new Vector2(1, 30));
                c.DrawColor = Color.Green;
                c.Padding = new MLEM.Maths.Padding(5);
                listView.AddChild(c);
            }
        }
    }

    // Adding children causes the scroll bar to disappear when it shouldn't
    [Test]
    public void TestIssue29Inconsistencies() {
        var group = new SquishingGroup(Anchor.TopLeft, Vector2.One);

        var centerGroup = new ScissorGroup(Anchor.TopCenter, Vector2.One);
        var centerPanel = new Panel(Anchor.TopRight, Vector2.One);
        centerPanel.DrawColor = Color.Red;
        centerPanel.Padding = new MLEM.Maths.Padding(5);
        centerGroup.AddChild(centerPanel);
        group.AddChild(centerGroup);

        var listView = new Panel(Anchor.TopLeft, new Vector2(1, 1), false, true);
        group.AddChild(listView);

        var bottomPane = new Panel(Anchor.BottomCenter, new Vector2(1, 500));
        group.AddChild(bottomPane);

        Assert.DoesNotThrow(() => this.AddAndUpdate(group, out _, out _));

        var appeared = false;
        for (var i = 0; i < 100; i++) {
            var c = new Panel(Anchor.AutoLeft, new Vector2(1, 50));
            c.DrawColor = Color.Green;
            c.Padding = new MLEM.Maths.Padding(5);
            listView.AddChild(c);
            Console.WriteLine($"Adding child, up to {i}");

            Assert.DoesNotThrow(() => UiTests.ForceUpdate(group, out _));
            if (appeared) {
                Assert.False(listView.ScrollBar.IsHidden, $"Fail bar was hidden after {i} children");
            } else if (!listView.ScrollBar.IsHidden) {
                appeared = true;
            }
        }
        Assert.True(appeared, "Scroll bar never appeared");
    }

    // removing a button whose paragraph has a custom style throws an exception
    [Test]
    public void TestIssue40() {
        var style = new UntexturedStyle(this.Game.SpriteBatch) {Font = this.Game.UiSystem.Style.Font};
        var secondStyle = new UiStyle(style) {Font = this.Game.UiSystem.Style.Font};
        var button = new Button(Anchor.Center, new Vector2(0.5f, 0.5f), "Test text") {
            Text = {Style = secondStyle}
        };
        this.AddAndUpdate(button, out _, out _);
        this.Game.UiSystem.Remove("Test");
    }

    [Test]
    public void TestToString() {
        var panel = new Panel(Anchor.TopLeft, Vector2.One);
        var group = panel.AddChild(new Group(Anchor.TopLeft, Vector2.One, false, false));
        var button = group.AddChild(new Button(Anchor.TopLeft, Vector2.One));

        Assert.AreEqual(button.ToString(), "Button 0 @ Group 0 @ Panel");
        this.AddAndUpdate(panel, out _, out _);
        Assert.AreEqual(button.ToString(), "Button 0 @ Group 0 @ Panel Test");

        button.DebugName = "Test Button";
        group.DebugName = "Test Group";
        Assert.AreEqual(button.ToString(), "Test Button 0 @ Test Group 0 @ Panel Test");
    }

    [Test]
    public void TestGroupDirtyPropagation() {
        var group = new Group(Anchor.BottomRight, Vector2.One) {SetWidthBasedOnChildren = true};
        var btn = group.AddChild(new Button(Anchor.AutoInlineBottomIgnoreOverflow, new Vector2(12)));
        var panel = group.AddChild(new Panel(Anchor.AutoRight, new Vector2(120, 175), Vector2.Zero, false, true) {PositionOffset = new Vector2(0, 1)});

        this.AddAndUpdate(group, out _, out _);
        Assert.AreEqual(btn.System.Viewport.Height - 175 - 1 - 12, btn.DisplayArea.Y);

        // hiding the panel should update the button's display area,
        // but due to elements that don't draw themselves previously not propagating updates correctly,
        // this was not the case
        panel.IsHidden = true;
        Assert.AreEqual(btn.System.Viewport.Height - 12, btn.DisplayArea.Y);
    }

    [Test]
    public void TestAreaUpdateAmounts() {
        var fixedSizePanel = new Panel(Anchor.TopLeft, Vector2.One);
        fixedSizePanel.AddChild(new Button(Anchor.TopLeft, new Vector2(12)));
        this.AddAndUpdate(fixedSizePanel, out _, out _);
        Assert.AreEqual(2, fixedSizePanel.System.Metrics.ActualAreaUpdates);

        var dynSizePanel = new Panel(Anchor.TopLeft, Vector2.One) {SetHeightBasedOnChildren = true};
        dynSizePanel.AddChild(new Button(Anchor.TopLeft, new Vector2(12)));
        this.AddAndUpdate(dynSizePanel, out _, out _);
        // panel update -> regular button update -> repeated panel update to set height -> repeated button update
        Assert.AreEqual(4, dynSizePanel.System.Metrics.ActualAreaUpdates);

        var groupWithPanel = new Group(Anchor.BottomRight, Vector2.One) {SetWidthBasedOnChildren = true};
        var panel = groupWithPanel.AddChild(new Panel(Anchor.TopLeft, Vector2.One) {SetHeightBasedOnChildren = true});
        panel.AddChild(new Button(Anchor.TopLeft, new Vector2(12)));
        this.AddAndUpdate(groupWithPanel, out _, out _);
        // group update -> panel update -> button update -> panel resize -> button update -> group resize -> panel update -> button update
        Assert.AreEqual(8, groupWithPanel.System.Metrics.ActualAreaUpdates);
    }

    private void AddAndUpdate(Element element, out TimeSpan addTime, out TimeSpan updateTime) {
        foreach (var root in this.Game.UiSystem.GetRootElements())
            this.Game.UiSystem.Remove(root.Name);
        this.Game.UiSystem.Metrics.ResetUpdates();

        var stopwatch = Stopwatch.StartNew();
        this.Game.UiSystem.Add("Test", element);
        stopwatch.Stop();
        addTime = stopwatch.Elapsed;

        UiTests.ForceUpdate(element, out updateTime);
    }

    private static void ForceUpdate(Element element, out TimeSpan updateTime) {
        var stopwatch = Stopwatch.StartNew();
        element.ForceUpdateArea();
        stopwatch.Stop();
        updateTime = stopwatch.Elapsed;
    }

}
