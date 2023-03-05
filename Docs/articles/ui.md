# MLEM.Ui

**MLEM.Ui** is a Ui framework for MonoGame and FNA that features elements with automatic positioning and sizing. It contains various ready-made element types like buttons, paragraphs, text fields and more, along with the ability to easily create custom controls. It supports **mouse**, **keyboard**, **gamepad** and **touch** input with little to no additional setup work required.

To see some of what MLEM.Ui can do, you can check out [the demo](https://github.com/Ellpeck/MLEM/blob/main/Demos/UiDemo.cs) as well.

## Setting it up
To get set up with MLEM.Ui, there are only a few things that need to be done in your Game class:
```cs
public UiSystem UiSystem;

protected override void LoadContent() {
    // Load your other content here

    // Initialize the Ui system
    this.UiSystem = new UiSystem(this.Window, this.GraphicsDevice, new UntexturedStyle(this.SpriteBatch));
}

protected override void Update(GameTime gameTime) {
    // Update the Ui system
    this.UiSystem.Update(gameTime);
}

protected override void Draw(GameTime gameTime) {
    this.GraphicsDevice.Clear(Color.CornflowerBlue);
    // Do your regular game drawing here

    // Call Draw at the end to draw the Ui on top of your game
    this.UiSystem.Draw(gameTime, this.SpriteBatch);
}
```

### Text Input
On desktop devices, MonoGame provides the `Window.TextInput` event that gets called automatically with the correct characters for the keys that you're pressing, even for non-American keyboards. However, this function doesn't exist on other devices. Similarly, MonoGame provides the `KeyboardInput` class for showing an on-screen keyboard on mobile devices and consoles, but not on desktop.

To make MLEM compatible with all devices without publishing a separate version for each MonoGame platform, you have to set up the `MlemPlatform` class based on the system you're using MLEM.Ui with. This has to be done *before* initializing your `UiSystem`.

DesktopGL and WindowsDX using MonoGame:
```cs
MlemPlatform.Current = new MlemPlatform.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
```
Desktop using FNA:
```cs
MlemPlatform.Current = new MlemPlatform.DesktopFna(a => TextInputEXT.TextInput += a);
```
Mobile devices and consoles:
```cs
MlemPlatform.Current = new MlemPlatform.Mobile(KeyboardInput.Show, l => this.StartActivity(new Intent(Intent.ActionView, Uri.Parse(l))));
```
If you're not using text input, you can leave the platform uninitialized or just set it to a stub one like so:
```cs
MlemPlatform.Current = new MlemPlatform.None();
```
Initializing the platform in this way also allows for links in paragraphs to be clickable, causing a browser or explorer window to be opened on desktop or mobile devices.

For more info on MLEM's platform-related code, you can also check out MlemPlatform's [documentation](xref:MLEM.Misc.MlemPlatform).

## Setting the style
By default, MLEM.Ui's controls look pretty bland, since it doesn't ship with any fonts or textures for any of its controls. To change the style of your ui, simply expand your `new UntexturedStyle(this.SpriteBatch)` call to include fonts and textures of your choosing, for example:
```cs
var style = new UntexturedStyle(this.SpriteBatch) {
    Font = new GenericSpriteFont(this.Content.Load<SpriteFont>("Fonts/ExampleFont")),
    ButtonTexture = new NinePatch(this.Content.Load<Texture2D>("Textures/ExampleTexture"), padding: 1)
};
```
Note that MLEM.Ui is also compatible with [MonoGame.Extended](http://www.monogameextended.net/)'s Bitmap Fonts by installing **MLEM.Extended** and using `GenericBitmapFont` instead.

### Scaling
To change the scaling of your ui, you can use the `UiSystem`'s `Scale` property. Additionally, you can enable `AutoScaleWithScreen` to cause the entire ui to scale automatically when resizing the game window.

## Adding elements
To add elements to your ui, you first have to add a **root element**. A root element can be any type of element, but to add it to the ui system, you have to give it a name:
```cs
var panel = new Panel(Anchor.Center, size: new Vector2(100, 100), positionOffset: Vector2.Zero);
this.UiSystem.Add("ExampleUi", panel);
```
After that, any more elements that you want to add to the ui can be added as **child elements** to the root. Each child element can then have more child elements added to it, and so on.

The **anchor** of an element determines its position relative to its parent. To change an element's position within its parent, you can use `PositionOffset`.

This is an example of a simple information box with a paragraph of text, followed by a button to close the box:
```cs
var box = new Panel(Anchor.Center, new Vector2(100, 1), Vector2.Zero, setHeightBasedOnChildren: true);
box.AddChild(new Paragraph(Anchor.AutoLeft, 1, "This is some example text!"));
box.AddChild(new Button(Anchor.AutoCenter, new Vector2(0.5F, 20), "Okay") {
    OnPressed = element => this.UiSystem.Remove("InfoBox"),
    PositionOffset = new Vector2(0, 1)
});
this.UiSystem.Add("InfoBox", box);
```

### About sizing
Note that, when setting the width and height of any element, there are some things to note:
- Each element has a `SetWidthBasedOnChildren` and a `SetHeightBasedOnChildren` property, which allow them to change their size automatically based on their content
- When specifying a width or height *lower than or equal to 1*, it is seen as a percentage based on the parent's size instead. For example, a paragraph with a width of `0.5F` inside of a panel width a width of `200` will be `100` units wide.
- When specifying a width *lower than 0*, it is seen as a percentage based on the element's height, and vice versa. For example, a panel with a width of `200` and a height of `-2` will be `400` units tall.

A lot of other ways to modify the size of an object are available as well, including `TreatSizeAsMaximum`, `TreatSizeAsMinimum`, `PreventParentSpill` and more. For more information, the `Element` [documentation](xref:MLEM.Ui.Elements.Element) contains descriptions of all fields, properties and methods.
