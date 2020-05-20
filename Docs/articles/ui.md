**MLEM.Ui** is a Ui framework for MonoGame that features elements with automatic positioning and sizing. It contains various ready-made element types like buttons, paragraphs, text fields and more, along with the ability to easily create custom controls. It supports **mouse**, **keyboard**, **gamepad** and **touch** input with little to no additional setup work required.

To see some of what MLEM.Ui can do, you can check out [the demo](https://github.com/Ellpeck/MLEM/blob/master/Demos/UiDemo.cs) as well.

# Setting it up
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
    // DrawEarly needs to be called before clearing your graphics context
    this.UiSystem.DrawEarly(gameTime, this.SpriteBatch);
    
    this.GraphicsDevice.Clear(Color.CornflowerBlue);
    // Do your regular game drawing here
    
    // Call Draw at the end to draw the Ui on top of your game
    this.UiSystem.Draw(gameTime, this.SpriteBatch);
}
```

## Text Input
Text input is a bit weird in MonoGame. On Desktop devices, you have the `Window.TextInput` event that gets called automatically with the correct characters for the keys that you're pressing, even for non-American keyboards. However, this function doesn't just *not work* on other devices, it doesn't exist there at all. So, to make MLEM.Ui compatible with all devices without publishing a separate version for each MonoGame system, you have to set up the text input wrapper yourself, based on the system you're using MLEM.Ui with. This has to be done *before* initializing your `UiSystem`.

DesktopGL:
```cs
TextInputWrapper.Current = new TextInputWrapper.DesktopGl<TextInputEventArgs>((w, c) => w.TextInput += c);
```
Mobile devices and consoles:
```cs
TextInputWrapper.Current = new TextInputWrapper.Mobile();
```
Other systems. Note that, for this implementation, its `Update()` method also has to be called every game update tick. It only supports an American keyboard layout due to the way that it is implemented:
```cs
TextInputWrapper.Current = new TextInputWrapper.Primitive();
```
If you're not using text input, you can just set the wrapper to a stub one like so:
```cs
TextInputWrapper.Current = new TextInputWrapper.None();
```

# Setting the style
By default, MLEM.Ui's controls look pretty bland, since it doesn't ship with any fonts or textures for any of its controls. To change the style of your ui, simply expand your `new UntexturedStyle(this.SpriteBatch)` call to include fonts and textures of your choosing, for example:
```cs
var style = new UntexturedStyle(this.SpriteBatch) {
    Font = new GenericSpriteFont(this.Content.Load<SpriteFont>("Fonts/ExampleFont")),
    ButtonTexture = new NinePatch(this.Content.Load<Texture2D>("Textures/ExampleTexture"), padding: 1)
};
```
Note that MLEM.Ui is also compatible with [MonoGame.Extended](http://www.monogameextended.net/)'s Bitmap Fonts by installing MLEM.Extended and using `GenericBitmapFont` instead.

## Scaling
To change the scaling of your ui, you can use the `UiSystem`'s `Scale` property. Additionally, you can enable `AutoScaleWithScreen` to cause the entire ui to scale automatically when resizing the game window.

# Adding elements
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

## About sizing
Note that, when setting the width and height of any element, there are some things to note:
- Each element has a `SetWidthBasedOnChildren` and a `SetHeightBasedOnChildren` property, which allow them to change their size automatically based on their content
- When specifying a width or height *lower than or equal to 1*, it is seen as a percentage based on the parent's size instead. For example, a paragraph with a width of `0.5F` inside of a panel width a width of `200` will be `100` units wide.
