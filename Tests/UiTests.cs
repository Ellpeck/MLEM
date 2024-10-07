﻿using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Maths;
using MLEM.Ui;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;
using NUnit.Framework;

namespace Tests;

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
            var totalUpdates = 0;
            var main = new Group(Anchor.TopLeft, new Vector2(50)) {
                OnAreaUpdated = _ => totalUpdates++
            };
            var group = main;
            for (var g = 0; g < i; g++) {
                group = group.AddChild(new Group(Anchor.TopLeft, Vector2.One) {
                    OnAreaUpdated = _ => totalUpdates++
                });
            }
            this.AddAndUpdate(main, out var addTime, out var updateTime);
            var allChildren = main.GetChildren(regardGrandchildren: true);
            TestContext.WriteLine($"{allChildren.Count()} children, {totalUpdates} updates total, took {addTime.TotalMilliseconds * 1000000}ns to add, {updateTime.TotalMilliseconds * 1000000}ns to update");
        }
    }

    [Test]
    public void TestAutoAreaPerformanceSideBySide() {
        for (var i = 1; i <= 100; i++) {
            var totalUpdates = 0;
            var main = new Group(Anchor.TopLeft, new Vector2(50)) {
                OnAreaUpdated = _ => totalUpdates++
            };
            for (var g = 0; g < i; g++) {
                main.AddChild(new Group(Anchor.AutoInlineIgnoreOverflow, new Vector2(1F / i, 1)) {
                    OnAreaUpdated = _ => totalUpdates++
                });
            }
            this.AddAndUpdate(main, out var addTime, out var updateTime);
            var allChildren = main.GetChildren(regardGrandchildren: true);
            TestContext.WriteLine($"{allChildren.Count()} children, {totalUpdates} updates total, took {addTime.TotalMilliseconds * 1000000}ns to add, {updateTime.TotalMilliseconds * 1000000}ns to update");
        }
    }

    [Test]
    public void TestAutoAreaPerformanceRandom() {
        for (var i = 0; i <= 1000; i += 10) {
            var random = new Random(93829345);
            var totalUpdates = 0;
            var main = new Group(Anchor.TopLeft, new Vector2(50)) {
                OnAreaUpdated = _ => totalUpdates++
            };
            var group = main;
            for (var g = 0; g < i; g++) {
                var newGroup = group.AddChild(new Group(Anchor.TopLeft, Vector2.One) {
                    OnAreaUpdated = _ => totalUpdates++
                });
                if (random.NextSingle() <= 0.25F)
                    group = newGroup;
            }
            this.AddAndUpdate(main, out var addTime, out var updateTime);
            var allChildren = main.GetChildren(regardGrandchildren: true);
            TestContext.WriteLine($"{allChildren.Count()} children, {totalUpdates} updates total, took {addTime.TotalMilliseconds * 1000000}ns to add, {updateTime.TotalMilliseconds * 1000000}ns to update");
        }
    }

    private void AddAndUpdate(Element element, out TimeSpan addTime, out TimeSpan updateTime) {
        foreach (var root in this.game.UiSystem.GetRootElements())
            this.game.UiSystem.Remove(root.Name);

        var stopwatch = Stopwatch.StartNew();
        this.game.UiSystem.Add("Test", element);
        stopwatch.Stop();
        addTime = stopwatch.Elapsed;

        stopwatch.Restart();
        element.ForceUpdateArea();
        stopwatch.Stop();
        updateTime = stopwatch.Elapsed;
    }

}
