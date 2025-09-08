using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MLEM.Input;
using MLEM.Maths;
using MLEM.Misc;
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
        /// <param name="imagePadding">The <see cref="Element.Padding"/> of the button's image</param>
        /// <returns>An image button</returns>
        public static Button ImageButton(Anchor anchor, Vector2 size, TextureRegion texture, string text = null, string tooltipText = null, float imagePadding = 2) {
            var button = new Button(anchor, size, text, tooltipText);
            var image = new Image(Anchor.CenterLeft, Vector2.One, texture);
            image.Padding = image.Padding.OrStyle(new Padding(imagePadding), 1);
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
                parent?.AddChild(cols[i]);
            }
            return cols;
        }

        /// <summary>
        /// Creates an array of groups with a fixed width and height that can be used to create a grid with equally sized boxes.
        /// </summary>
        /// <param name="parent">The element the groups should be added to, can be <see langword="null"/>.</param>
        /// <param name="totalSize">The total size that the grid should take up.</param>
        /// <param name="width">The width of the grid, or the amount of columns it should have.</param>
        /// <param name="height">The height of the grid, or the amount of rows it should have.</param>
        /// <returns>The created grid.</returns>
        public static Group[,] MakeGrid(Element parent, Vector2 totalSize, int width, int height) {
            var grid = new Group[width, height];
            for (var y = 0; y < height; y++) {
                for (var x = 0; x < width; x++) {
                    var anchor = x == 0 ? Anchor.AutoLeft : Anchor.AutoInlineIgnoreOverflow;
                    grid[x, y] = new Group(anchor, new Vector2(totalSize.X / width, totalSize.Y / height), false);
                    parent?.AddChild(grid[x, y]);
                }
            }
            return grid;
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

        /// <inheritdoc cref="KeybindButton(MLEM.Ui.Anchor,Microsoft.Xna.Framework.Vector2,MLEM.Input.Keybind,MLEM.Input.InputHandler,string,MLEM.Input.Keybind,string,System.Func{MLEM.Input.GenericInput,string},int,System.Func{MLEM.Input.GenericInput,System.Collections.Generic.IEnumerable{MLEM.Input.GenericInput},bool})"/>
        public static Button KeybindButton(Anchor anchor, Vector2 size, Keybind keybind, InputHandler inputHandler, string activePlaceholder, GenericInput unbindKey = default, string unboundPlaceholder = "", Func<GenericInput, string> inputName = null, int index = 0, Func<GenericInput, IEnumerable<GenericInput>, bool> isKeybindAllowed = null) {
            return ElementHelper.KeybindButton(anchor, size, keybind, inputHandler, activePlaceholder, new Keybind(unbindKey), unboundPlaceholder, inputName, index, isKeybindAllowed);
        }

        /// <summary>
        /// Creates a <see cref="Button"/> that acts as a way to input a custom value for a <see cref="Keybind"/>.
        /// Note that only the <see cref="Keybind.Combination"/> at index <paramref name="index"/> of the given keybind is displayed and edited, all others are ignored.
        /// Inputting custom keybinds using this element supports <see cref="ModifierKey"/>-based modifiers and any <see cref="GenericInput"/>-based keys.
        /// The keybind button's current state can be retrieved using the "Active" <see cref="GenericDataHolder.GetData{T}"/> key, which stores a bool that indicates whether the button is currently waiting for a new value to be assigned.
        /// </summary>
        /// <param name="anchor">The button's anchor</param>
        /// <param name="size">The button's size</param>
        /// <param name="keybind">The keybind that this button should represent</param>
        /// <param name="inputHandler">The input handler to query inputs with</param>
        /// <param name="activePlaceholder">A placeholder text that is displayed while the keybind is being edited</param>
        /// <param name="unbind">An optional keybind to press that allows the keybind value to be unbound, clearing the combination</param>
        /// <param name="unboundPlaceholder">A placeholder text that is displayed if the keybind is unbound</param>
        /// <param name="inputName">An optional function to give each input a display name that is easier to read. If this is null, <see cref="GenericInput.ToString"/> is used.</param>
        /// <param name="index">The index in the <paramref name="keybind"/> that the button should display. Note that, if the index is greater than the amount of combinations, combinations entered using this button will be stored at an earlier index.</param>
        /// <param name="isKeybindAllowed">A function that can optionally determine whether a given input and modifier combination is allowed. If this is null, all combinations are allowed.</param>
        /// <returns>A keybind button with the given settings</returns>
        public static Button KeybindButton(Anchor anchor, Vector2 size, Keybind keybind, InputHandler inputHandler, string activePlaceholder, Keybind unbind = default, string unboundPlaceholder = "", Func<GenericInput, string> inputName = null, int index = 0, Func<GenericInput, IEnumerable<GenericInput>, bool> isKeybindAllowed = null) {
            string GetCurrentName() {
                return keybind.TryGetCombination(index, out var combination) ? combination.ToString(" + ", inputName) : unboundPlaceholder;
            }

            var button = new Button(anchor, size, GetCurrentName());
            var activeNext = false;
            button.OnPressed = e => {
                button.Text.Text = activePlaceholder;
                activeNext = true;
            };
            button.OnUpdated = (e, time) => {
                if (activeNext) {
                    button.SetData("Active", true);
                    activeNext = false;
                } else if (button.GetData<bool>("Active")) {
                    if (unbind != null && unbind.TryConsumePressed(inputHandler)) {
                        keybind.Remove((c, i) => i == index);
                        button.Text.Text = unboundPlaceholder;
                        button.SetData("Active", false);
                    } else if (inputHandler.InputsPressed.Length > 0) {
                        var key = inputHandler.InputsPressed.FirstOrDefault(i => !i.IsModifier());
                        if (key != default) {
                            var mods = inputHandler.InputsDown.Where(i => i.IsModifier()).ToArray();
                            if (isKeybindAllowed == null || isKeybindAllowed(key, mods)) {
                                keybind.Remove((c, i) => i == index).Insert(index, key, mods);
                                button.Text.Text = GetCurrentName();
                                button.SetData("Active", false);
                            }
                        }
                    }
                } else {
                    button.Text.Text = GetCurrentName();
                }
            };
            return button;
        }

    }

    /// <summary>
    /// This class contains a set of extensions for dealing with <see cref="Element"/> objects
    /// </summary>
    public static class ElementExtensions {

        /// <summary>
        /// Adds a new <see cref="Tooltip"/> to the given element using the <see cref="Tooltip(Paragraph.TextCallback,Element)"/> constructor
        /// </summary>
        /// <param name="element">The element to add the tooltip to</param>
        /// <param name="textCallback">The text to display on the tooltip</param>
        /// <returns>The created tooltip instance</returns>
        public static Tooltip AddTooltip(this Element element, Paragraph.TextCallback textCallback) {
            return element.AddTooltip(new Tooltip(textCallback));
        }

        /// <summary>
        /// Adds a new <see cref="Tooltip"/> to the given element using the <see cref="Tooltip(string,Element)"/> constructor
        /// </summary>
        /// <param name="element">The element to add the tooltip to</param>
        /// <param name="text">The text to display on the tooltip</param>
        /// <returns>The created tooltip instance</returns>
        public static Tooltip AddTooltip(this Element element, string text) {
            return element.AddTooltip(new Tooltip(text));
        }

        /// <summary>
        /// Adds the given <see cref="Tooltip"/> to the given element
        /// </summary>
        /// <param name="element">The element to add the tooltip to</param>
        /// <param name="tooltip">The tooltip to add</param>
        /// <returns>The passed tooltip, for chaining</returns>
        public static Tooltip AddTooltip(this Element element, Tooltip tooltip) {
            tooltip.AddToElement(element);
            return tooltip;
        }

        /// <summary>
        /// Returns whether the given <see cref="Anchor"/> is automatic. The anchors <see cref="Anchor.AutoLeft"/>, <see cref="Anchor.AutoCenter"/>, <see cref="Anchor.AutoRight"/>, and any anchor that <see cref="IsInline"/> will return true.
        /// </summary>
        /// <param name="anchor">The anchor to query.</param>
        /// <returns>Whether the given anchor is automatic.</returns>
        public static bool IsAuto(this Anchor anchor) {
            return anchor == Anchor.AutoLeft || anchor == Anchor.AutoCenter || anchor == Anchor.AutoRight || anchor.IsInline();
        }

        /// <summary>
        /// Returns whether the given <see cref="Anchor"/> is inline. The anchors <see cref="Anchor.AutoInline"/>, <see cref="Anchor.AutoInlineCenter"/>, <see cref="Anchor.AutoInlineBottom"/>, and any anchor that <see cref="IsIgnoreOverflow"/> will return true.
        /// </summary>
        /// <param name="anchor">The anchor to query.</param>
        /// <returns>Whether the given anchor is inline.</returns>
        public static bool IsInline(this Anchor anchor) {
            return anchor == Anchor.AutoInline || anchor == Anchor.AutoInlineCenter || anchor == Anchor.AutoInlineBottom || anchor.IsIgnoreOverflow();
        }

        /// <summary>
        /// Returns whether the given <see cref="Anchor"/> ignores overflow. The anchors <see cref="Anchor.AutoInlineIgnoreOverflow"/>, <see cref="Anchor.AutoInlineCenterIgnoreOverflow"/>, and <see cref="Anchor.AutoInlineBottomIgnoreOverflow"/> will return true.
        /// </summary>
        /// <param name="anchor">The anchor to query.</param>
        /// <returns>Whether the given anchor ignores overflow.</returns>
        public static bool IsIgnoreOverflow(this Anchor anchor) {
            return anchor == Anchor.AutoInlineIgnoreOverflow || anchor == Anchor.AutoInlineCenterIgnoreOverflow || anchor == Anchor.AutoInlineBottomIgnoreOverflow;
        }

        /// <summary>
        /// Returns whether the given <see cref="Anchor"/> is left-aligned for the purpose of <see cref="Element.GetRightmostChild"/>. The anchors <see cref="Anchor.TopLeft"/>, <see cref="Anchor.CenterLeft"/>, <see cref="Anchor.BottomLeft"/>, <see cref="Anchor.AutoLeft"/>, and any anchor that <see cref="IsInline"/> will return true.
        /// </summary>
        /// <param name="anchor">The anchor to query.</param>
        /// <returns>Whether the given anchor is left-aligned.</returns>
        public static bool IsLeftAligned(this Anchor anchor) {
            return anchor == Anchor.TopLeft || anchor == Anchor.CenterLeft || anchor == Anchor.BottomLeft || anchor == Anchor.AutoLeft || anchor.IsInline();
        }

        /// <summary>
        /// Returns whether the given <see cref="Anchor"/> is top-aligned for the purpose of <see cref="Element.GetLowestChild"/>. The anchors <see cref="Anchor.TopLeft"/>, <see cref="Anchor.TopCenter"/>, <see cref="Anchor.TopRight"/>, <see cref="Anchor.AutoLeft"/>, <see cref="Anchor.AutoCenter"/>, <see cref="Anchor.AutoRight"/>, <see cref="Anchor.AutoInline"/> and <see cref="Anchor.AutoInlineIgnoreOverflow"/> will return true.
        /// </summary>
        /// <param name="anchor">The anchor to query.</param>
        /// <returns>Whether the given anchor is top-aligned.</returns>
        public static bool IsTopAligned(this Anchor anchor) {
            return anchor == Anchor.TopLeft || anchor == Anchor.TopCenter || anchor == Anchor.TopRight || anchor == Anchor.AutoLeft || anchor == Anchor.AutoCenter || anchor == Anchor.AutoRight || anchor == Anchor.AutoInline || anchor == Anchor.AutoInlineIgnoreOverflow;
        }

    }
}
