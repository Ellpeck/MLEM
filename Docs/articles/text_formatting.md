The **MLEM** package contains a simple text formatting system that supports coloring, bold and italic font modifiers, in-text icons and text animations.

Text formatting makes use of [generic fonts](https://github.com/Ellpeck/MLEM/wiki/Font-Extensions).

It should also be noted that [MLEM.Ui](https://github.com/Ellpeck/MLEM/wiki/MLEM.Ui)'s `Paragraph`s support text formatting out of the box.

# Formatting codes
To format your text, you can insert *formatting codes* into it. These codes are surrounded by `[]` by default, however these delimiters can be changed like so:
```cs
TextFormatting.SetFormatIndicators('<', '>');
```

By default, the following formatting options are available:
- All default MonoGame `Color` values, for example `[CornflowerBlue]`, `[Red]` etc.
- `[Bold]` and `[Italic]` to switch to a bold or italic font (and `[Regular]` to switch back)
- `[Shadow]` to add a drop shadow to the text
- `[Wobbly]` for a sine wave text animation
- `[Typing]` for a typewriter-style text animation
- `[Unanimated]` to reset back to the default animation

# Getting your text ready
To actually display the text with formatting, you first need to gather the formatting data from the text. For performance reasons, this is best done when the text changes, and not every render frame.

To gather formatting data, you will need a [generic font](https://github.com/Ellpeck/MLEM/wiki/Font-Extensions). With it, you can gather the data in the form of a `FormattingCodeCollection` like so:
```cs
var font = new GenericSpriteFont(this.Content.Load<SpriteFont>("Fonts/ExampleFont"));
var text = "This is the [Blue]text[White] that should be [Wobbly]formatted[Unanimated].";
var data = text.GetFormattingCodes(font);
```

Additionally, you will need an *unformatted* version of the string you want to display. There are two reasons for this:
- Removing formatting data from the string each frame is unnecessarily expensive
- You can use generic fonts' `SplitString` method to split the text you want to display into multiple lines without the text's lengths being tainted by the formatting codes within the string.

You can remove a string's formatting data like so:
```cs
var unformatted = text.RemoveFormatting(font);
```

# Drawing the formatted text
Now that you have your font (`font`), the formatting data (`data`) and the unformatted string (`unformatted`), you can draw it like so:
```cs
font.DrawFormattedString(this.SpriteBatch, new Vector2(10, 10), unformatted, data, Color.White, scale: 1);
```

Additionally, the `DrawFormattedString` method accepts several optional parameters, including the font to be used for bold and italic text and more.

## Time into animation
If you want to display animated text, you need to pass a `TimeSpan` into `DrawFormattedString` that determines the amount of time that has passed throughout the animation. You can do this easily by passing the `GameTime`'s `TotalGameTime` span:
```cs
font.DrawFormattedString(this.SpriteBatch, new Vector2(10, 10), unformatted, data, Color.White, scale: 1,
    timeIntoAnimation: gameTime.TotalGameTime);
```

## Formatting settings
The `FormatSettings` class contains additional information for text formatting, like the speed of predefined animations and the color and offset of the drop shadow. To change these settings, you can simply pass an instance of `FormatSettings` to `DrawFormattedString`:
```cs
var settings = new FormatSettings {
    DropShadowColor = Color.Pink,
    WobbleHeightModifier = 1
};

font.DrawFormattedString(this.SpriteBatch, new Vector2(10, 10), unformatted, data, Color.White, scale: 1,
    formatSettings: settings);
```

# Adding custom codes
To add custom formatting codes (especially icons), you simple have to add to the `FormattingCodes` dictionary. The key is the name of the formatting code that goes between the delimiters (`[]`), and the value is an instance of `FormattingCode`:
```cs
// Creating an icon
TextFormatting.FormattingCodes["ExampleIcon"] = new FormattingCode(new TextureRegion(exampleTexture));
// Creating a color
TextFormatting.FormattingCodes["ExampleColor"] = new FormattingCode(new Color(10, 20, 30));
```