# Text Formatting

The **MLEM** package contains a simple text formatting system that supports coloring, bold and italic font modifiers, in-text icons and text animations.

Text formatting makes use of [generic fonts](font_extensions.md), and [MLEM.Ui](ui.md)'s `Paragraph` supports text formatting out of the box, but using it for your own text rendering is very simple.

[The demo](https://github.com/Ellpeck/MLEM/blob/main/Demos/TextFormattingDemo.cs) features plenty of examples of the formatting codes that are available by default, as well as examples of the ability to add custom codes and interact with formatted text.

## Formatting Codes
To format your text, you can insert *formatting codes* into it. Almost all of these codes are single letters surrounded by `<>`, and some formatting codes can accept additional parameters after their letter representation.

By default, the following formatting options are available:
- **Colors** using `<c ColorName>`. All default MonoGame colors are supported, for example `<c CornflowerBlue>`. Reset using `</c>`.
- **Bold** and *italic* text using `<b>` and `<i>`, respectively. Reset using `</b>` and `</i>`.
- **Drop shadows** using `<s>`. Optional parameters for the shadow's color and positional offset are accepted: `<s #AARRGGBB 2.5>`. Reset using `</s>`.
- **Underlined** and **strikethrough** text using `<u>` and `<st>`, respectively. Reset using `</u>` and `</st>`.
- **Subscript** and **superscript** text using `<sub>` and `<sup>`, respectively. Reset using `</sub>` and `</sup>`.
- **Text outlines** using `<o>`. Optional parameters for the outlines' color and thickness are accepted as well: `<o #ff0000 4>`. Reset using `</o>`.
- A wobbly sine wave **animation** using `<a wobbly>`. Optional parameters for the wobble's intensity and height are accepted: `<a wobbly 10 0.25>`. Reset using `</a>`.

When using [MLEM.Ui](ui.md)'s `Paragraph`, these additional formatting options are available by default:
- Hoverable and clickable links using `<l Url>`. Reset using `</l>`.
- Inline font changes using `<f FontName>`, with custom fonts gathered from `UiStyle.AdditionalFonts`. Reset using `</f>`.

If you only want to use your own formatting codes in your text formatter, the constructor allows disabling some or all of the default ones.

## Getting Your Text Ready
To get your text ready for rendering with formatting codes, it has to be tokenized. For that, you need to create a new text formatter first. Additionally, you need to have a [generic font](font_extensions.md) ready:
```cs
var font = new GenericSpriteFont(this.Content.Load<SpriteFont>("Fonts/ExampleFont"));
var formatter = new TextFormatter();
```
You can then tokenize your string like so:
```cs
var tokenizedString = formatter.Tokenize(font, "This is a <c Green>formatted</c> string!");
```
Additionally, if you want your tokenized string to be split based on a certain maximum width automatically, you can split it like so:
```cs
tokenizedString.Split(font, maxWidth, scale);
``` 

## Drawing the Formatted Text
To draw your tokenized text, all you have to do is call its `Draw` method like so:
```cs
tokenizedString.Draw(gameTime, spriteBatch, position, font, color, scale, depth); 
```
Note that, if your tokenized text contains any animations, you have to update the tokenized string every `Update` call like so:
```cs
tokenizedString.Update(gameTime);
```

## Interacting with Formatted Text
The `TokenizedString` class also features several methods for querying and interacting with the drawing of formatted text:
```cs 
// the token that is under queryPosition if the string is drawn at position
var tokenUnderPos = tokenizedString.GetTokenUnderPos(position, queryPosition, scale);

foreach (var token in tokenizedString.Tokens) {
    // the area that the given token takes up
    var area = token.GetArea(position, scale);
}
```

## Adding Custom Codes
Adding custom formatting codes is easy! There are two things that a custom formatting code requires:
- A class that extends `Code` that does what your formatting code should do (we'll use `MyCustomCode` in this case)
- A regex that determines what strings your formatting code matches
- A formatting code constructor that creates a new instance of your code's class

You can then register your formatting code like this:
```cs
formatter.Codes.Add(new Regex("<matchme>"), (form, match, regex) => new MyCustomCode(match, regex));
```

To add an in-text image formatting code, you can use the `ImageCodeExtensions.AddImage` extension. All you have to do is supply the texture region and a name:
```cs
formatter.AddImage("ImageName", new TextureRegion(texture, 0, 0, 8, 8));
```
After doing so, the image can be displayed using the code `<i ImageName>`.

## Macros
The text formatting system additionally supports macros: Regular expressions that cause the matched text to expand into a different string. Macros are resolved recursively (up to 64 times), meaning that you can have macros that resolve into other macros as well.

By default, the following macros are available:
- `~` expands into a non-breaking space, much like in LaTeX.
- `<n>` expands into a newline character, if you like visual consistency with the other codes.

Adding custom macros is very similar to adding custom formatting codes:
```cs
formatter.Macros.Add(new Regex("matchme"), (form, match, regex) => "replacement string");
```
