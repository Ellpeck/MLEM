# Frequently Asked Questions About MLEM.Ui

This article compiles some frequently asked questions about MLEM.Ui and tries to answer them succinctly. If you haven't checked out the [introductory article](ui.md) to MLEM.Ui yet, it's recommended you do that first!

## How Do I Structure My UI Code?

Of course, you can structure your UI code any way you like, but here are two suggestions: the way that MLEM.Ui was designed for, and the way that many users have chosen to do it instead!

### "Intended" Way
MLEM.Ui was initially developed with the goal of not requiring users to create custom classes that extend any preexisting UI elements or even the `Element` class itself. The original idea was that users would write their UI code "on the fly", largely using [object initializers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/object-and-collection-initializers) to configure elements, and any frequently used constructs would be put into static methods that returned a group, panel or other element with a set of children, similarly to the [ElementHelper class](xref:MLEM.Ui.Elements.ElementHelper).

```cs 
private void InitializeMyUi() {
    var box = InfoBox("This is some example text!");
    this.UiSystem.Add("InfoBox", box);
}

private static Element InfoBox(string text) {
    var box = new Panel(Anchor.Center, new Vector2(100, 1), Vector2.Zero, setHeightBasedOnChildren: true);
    box.AddChild(new Paragraph(Anchor.AutoLeft, 1, text));
    box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") {
        OnPressed = element => element.System.Remove("InfoBox"),
        PositionOffset = new Vector2(0, 1)
    });
    return box;
}
```

### "Custom Classes" Way
I've since noticed many users opting for a custom class structure instead, where they will extend `Panel` or `Group` and add all the required child elements in their constructor. This is, of course, also a great way of structuring your UI, but with the way that the MLEM.Ui API works, it [poses some issues](https://github.com/Ellpeck/MLEM/issues/43) for the virtual methods and properties frequently used for element configuration. To get around this issue, you can always make your custom classes `sealed` or call the `base` versions of the properties and methods that configure your elements instead.

```cs
private void InitializeMyUi() {
    var box = new InfoBox("This is some example text!");
    this.UiSystem.Add("InfoBox", box);
}

private sealed class InfoBox : Panel {

    public InfoBox(string text) : base(Anchor.Center, new Vector2(100, 1), Vector2.Zero, setHeightBasedOnChildren: true) {
        this.AddChild(new Paragraph(Anchor.AutoLeft, 1, text));
        this.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") {
            OnPressed = element => element.System.Remove("InfoBox"),
            PositionOffset = new Vector2(0, 1)
        });
    }

}
```
