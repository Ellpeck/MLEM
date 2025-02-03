# Font Extensions

The **MLEM** base package features several additional font manipulation methods, including line splitting and text truncating. These abilities can be accessed through generic fonts.

## Generic fonts
MLEM features the `GenericFont` class along with a `GenericSpriteFont` implementation. This is used by the **MLEM.Ui** package, but it can also be used separately to have more control over font rendering.

Additionally, the **MLEM.Extended** package provides the following generic fonts:
- `GenericBitmapFont`, which uses [MonoGame.Extended](http://www.monogameextended.net/)'s `BitmapFont`
- `GenericStashFont`, which uses [FontStashSharp](https://github.com/rds1983/FontStashSharp)'s fonts

### Creating a generic font
To create a generic font, simply create an instance of the desired generic font class when loading your game:
```cs
// Using MonoGame SpriteFonts
var spriteFont = new GenericSpriteFont(this.Content.Load<SpriteFont>("Fonts/ExampleFont"));
```

## Line splitting
Using generic fonts, a long line of text can be split into multiple lines based on a maximum width in pixels. The split text that is returned is delimited by `\n` (newline characters).
```cs
var split = spriteFont.SplitString("This is a really long line of text [...]", width: 100, scale: 1);
spriteFont.DrawString(this.SpriteBatch, split, new Vector2(10, 10), Color.White);
```

Alternatively, the `SplitStringSeparate` method returns a collection of strings, where each entry represents a place where a split has been introduced. Using this method, you can differentiate between pre-existing newline characters and newly introduced ones.
```cs
var split = spriteFont.SplitStringSeparate("This is a line of text that contains\nnewline characters!", width: 10, scale: 1);
// returns something like ["This is a line of ", "text that contains\nnewline characters!"]
```

## Truncating
Using generic fonts, a long line of text can also be truncated to fit a certain width in pixels. The remaining text that doesn't fit will simply be chopped off of the end (or start) of the string.
```cs
// Truncate from the front
var truncFront = spriteFont.TruncateString("This is a really long line of text [...]", width: 100, fromBack: false, scale: 1);
// Truncate from the back
var truncBack = spriteFont.TruncateString("This is a really long line of text [...]", width: 100, fromBack: true, scale: 1);
```
