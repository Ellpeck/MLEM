# Text Formatting

The **MLEM** package contains a simple text formatting system that supports coloring, bold and italic font modifiers, in-text icons and text animations.

Text formatting makes use of [generic fonts](font_extensions.md).

It should also be noted that [MLEM.Ui](ui.md)'s `Paragraph`s support text formatting out of the box.

## Formatting codes
To format your text, you can insert *formatting codes* into it. Almost all of these codes are single letters surrounded by `<>`, and some formatting codes can accept additional parameters after their letter representation.

By default, the following formatting options are available:
- Colors using `<c ColorName>`. All default MonoGame colors are supported, for example `<c CornflowerBlue>`. Reset using `</c>`.
- Bold and italic text using `<b>` and `<i>`, respectively. Reset using `</b>` and `</i>`.
- Drop shadows using `<s>`. Optional parameters for the shadow's color and positional offset are accepted: `<s #AARRGGBB 2.5>`. Reset using `</s>`.
- Underlined and strikethrough text using `<u>` and `<st>`, respectively. Reset using `</u>` and `</st>`.
- A wobbly sine wave animation using `<a wobbly>`. Optional parameters for the wobble's intensity and height are accepted: `<a wobbly 10 0.25>`. Reset using `</a>`.

## Getting your text ready
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
## Drawing the formatted text
To draw your tokenized text, all you have to do is call its `Draw` method like so:
```cs
tokenizedString.Draw(gameTime, spriteBatch, position, font, color, scale, depth); 
```
Note that, if your tokenized text contains any animations, you have to updated the tokenized string every `Update` call like so:
```cs
tokenizedString.Update(gameTime);
``` 
## Adding custom codes
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
The text formatting system additionally supports macros: Regular expressions that cause the matched text to expand into a different string. Macros are resolved recursively, meaning that you can have macros that resolve into other macros, and so on.

By default, the following macros are available:
- `~` expands into a non-breaking space, much like in LaTeX.

Adding custom macros is very similar to adding custom formatting codes:
```cs
formatter.Macros.Add(new Regex("matchme"), (form, match, regex) => "replacement string");
```