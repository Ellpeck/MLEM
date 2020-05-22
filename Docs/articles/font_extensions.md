# Font Extensions

The **MLEM** base package features several additional font manipulation methods, including line splitting and text truncating. These abilities can be accessed through generic fonts.

## Generic fonts
MLEM features the `GenericFont` class along with a `GenericSpriteFont` implementation. This is used by the MLEM.Ui package, but it can also be used separately to have more control over font rendering.

The reason generic fonts exist is to provide the ability to use both MonoGame `SpriteFonts` and [MonoGame.Extended](http://www.monogameextended.net/) `BitmapFonts` for the additionally provided behavior. To access the latter, the **MLEM.Extended** package needs to be installed as well.

### Creating a generic font
To create a generic font, simply create an instance of `GenericSpriteFont` or `GenericBitmapFont` when loading your game:
```cs
// Using MonoGame SpriteFonts
var spriteFont = new GenericSpriteFont(this.Content.Load<SpriteFont>("Fonts/ExampleFont"));

// Using MonoGame.Extended BitmapFonts
var bitmapFont = new GenericBitmapFont(this.Content.Load<BitmapFont>("Fonts/ExampleBitmapFont"));
```

## Line splitting
Using generic fonts, a long line of text can be split into multiple lines based on a maximum width in pixels. The split text that is returned is delimited by `\n` (newline characters).
```cs
var split = spriteFont.SplitString("This is a really long line of text [...]", width: 100, scale: 1);
spriteFont.DrawString(this.SpriteBatch, split, new Vector2(10, 10), Color.White);
```

## Truncating
Using generic fonts, a long line of text can also be truncated to fit a certain width in pixels. The remaining text that doesn't fit will simply be chopped off of the end (or start) of the string.
```cs
// Truncate from the front
var truncFront = spriteFont.TruncateString("This is a really long line of text [...]", width: 100, fromBack: false, scale: 1);
// Truncate from the back
var truncBack = spriteFont.TruncateString("This is a really long line of text [...]", width: 100, fromBack: true, scale: 1);
```