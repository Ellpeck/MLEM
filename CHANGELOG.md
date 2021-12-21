# Changelog
MLEM tries to adhere to [semantic versioning](https://semver.org/). Breaking changes are written in **bold**.

Jump to version:
- [5.2.0](#520)
- [5.1.0](#510)
- [5.0.0](#500)

## 5.2.0
### MLEM
Additions
- Added a strikethrough formatting code
- Added GenericFont SplitStringSeparate which differentiates between existing newline characters and splits due to maximum width
- Added StaticSpriteBatch class
- Added missing easing functions Quart and Quint to Easings
- Added RotationVector extension methods for Matrix and Quaternion
- Added DrawExtendedAutoTile to the AutoTiling class

Improvements
- **Moved AutoTiling to Graphics namespace**
- Cache TokenizedString inner offsets for non-Left text alignments to improve performance
- Exposed Camera's RoundPosition
- Exposed the epsilon value used by Camera
- Added Padding.Empty
- Throw an exception when text formatter macros resolve recursively too many times
- Allow using StaticSpriteBatch for AutoTiling
- Made TextFormatter string size based on the currently active font rather than the default one
- Allow storing multiple texture regions per SpriteAnimation frame

Fixes
- Fixed some end-of-line inconsistencies when using the Right text alignment

Removals
- **Removed deprecated Misc versions of SoundEffectInfo and SoundEffectInstanceHandler**

### MLEM.Ui
Additions
- Allow specifying a maximum amount of characters for a TextField
- Added a multiline editing mode to TextField
- Added a formatting code to allow for inline font changes
- Added a SquishingGroup element
- Added UiMetrics

Improvements
- **Made Image ScaleToImage take ui scale into account**
- **Added style properties for a lot of hardcoded default element styles**
- **Allow setting a custom effect and depth stencil state for ui drawing**
- **Made StyleProp immutable**
- Exposed the epsilon value used by Element calculations
- Allow style properties to set style values with a higher priority, which allows elements to style their default children
- Allow changing the entire ui style for a single element
- Skip unnecessary area updates for elements with dirty parents
- Calculate panel scroll bar height based on content height
- Remember the location that a scroll bar scroller was grabbed in when scrolling
- Automatically set area dirty when changing child padding or paragraph fonts

Fixes
- Fixed VerticalSpace height parameter being an integer
- Fixed text not being pasted into a text field at all if it contains characters that don't match the input rule
- Fixed panels that don't auto-hide their scroll bars ignoring their width for child padding
- Fixed some inconsistencies with element transformations and mouse interaction

Removals
- **Removed ScrollBar ScrollerOffset**

### MLEM.Data
Additions
- Allow RuntimeTexturePacker to automatically dispose submitted textures when packing
- Added JsonTypeSafeWrapper and JsonTypeSafeGenericDataHolder

Improvements
- **Use TitleContainer for opening streams where possible**
- Set GraphicsResource Name when loading assets using RawContentManager

Removals
- Marked features related to Lidgren.Network as obsolete

### MLEM.Startup
Additions
- Added virtual InitializeDefaultUiStyle to MlemGame
- Added PreDraw and PreUpdate events and coroutine events

## 5.1.0
### MLEM
Additions
- Added RotateBy to Direction2Helper

Improvements
- **Moved ColorHelper.Invert to ColorExtensions.Invert**
- **Allow enumerating SoundEffectInstanceHandler entries**
- Improved NinePatch memory usage
- Moved sound-related classes into Sound namespace
- Added customizable overloads for Keybind, Combination and GenericInput ToString methods
- Removed LINQ Any and All usage in various methods to improve memory usage
- Improved KeysExtensions memory usage

Fixes
- Set default values for InputHandler held and pressed keys to avoid an exception if buttons are held in the very first frame
- Fixed GenericFont MeasureString using incorrect width for Zwsp and OneEmSpace
- Fixed tiled NinePatches missing pixels with some scales

### MLEM.Ui
Additions
- Added a masking character to TextField to allow for password-style text fields

Improvements
- **Explicitly disallow creating Paragraphs without fonts to make starting out with MLEM.Ui less confusing**
- Removed LINQ Any and All usage in various methods to improve memory usage
- Allow adding Link children to non-Paragraph elements

Fixes
- Fixed a crash if a paragraph has a link formatting code, but no font
- Fixed tooltips with custom text scale not snapping to the mouse correctly in their first displayed frame
- Fixed tooltips not displaying correctly with auto-hiding paragraphs
- Fixed rounding errors causing AutoInline elements to be pushed into the next line with some ui scales

### MLEM.Extended
Improvements
- Use FontStashSharp's built-in LineHeight property for GenericStashFont

### MLEM.Data
Additions
- Added the ability to specify a coordinate offset in data texture atlases

Improvements
- Improved RawContentManager's reader loading and added better exception handling
- Improved CopyExtensions construction speed
- Improved DynamicEnum caching

Fixes
- Fixed DynamicEnum AddFlag going into an infinite loop

## 5.0.0
### MLEM
Additions
- Added some Collection extensions, namely for dealing with combinations
- Added repeat-ignoring versions of IsKeyPressed and IsGamepadButtonPressed
- Added SoundExtensions
- Added string truncation to TokenizedString
- Added a sprite batch extension to generate a gradient
- Added InputsDown and InputsPressed properties to InputHandler
- Added text alignment options to tokenized strings

Improvements
- **Replaced TextInputWrapper with a more refined MlemPlatform that includes the ability to open links on various platforms**
- Allow NinePatches to be drawn tiled rather than stretched
- Added the ability for Direction2 to be used as flags
- Made Padding and Direction2 DataContracts
- Expose the viewport of cameras
- Greatly improved the efficiency of line splitting for GenericFont and TokenizedString
- Improved performance of TextFormatter tokenization
- Allow for underline and shadow formatting codes to be mixed with font changing codes
- Exposed Keybind Combinations

Fixes
- Fixed the input handler querying input when the window is inactive
- Fixed UnderlineCode ending in the wrong places because it was marked as a font-changing code

Removals
- **Removed the array-based GetRandomEntry method**
- **Removed obsolete ColorExtension methods**

### MLEM.Ui
Additions
- Added a text scale multiplier value to Paragraph
- Added an option to limit auto-height and auto-width in elements to a maximum and minimum size
- Added the ability to set a custom viewport for ui systems
- Added string truncation to Paragraph
- Added a simple way to change the action that is executed when a link is pressed in a paragraph
- Added events for when a root element is added or removed
- Added an ElementHelper method to create a keybind button
- Added text alignment options to paragraphs

Improvements
- **Removed unnecessary GraphicsDevice references from UiSystem**
- Stop a panel's scroll bar from being removed from its children list automatically
- Dispose of panels' render targets to avoid memory leaks
- Allow changing the color that a panel renders its texture with

Fixes
- Fixed auto-sized elements doing too many area update calculations
- Fixed a rare stack overflow where scroll bars could get stuck in an auto-hide loop
- Fixed auto-sized elements without children not updating their size correctly
- Fixed panels drawing children early within the render target (instead of regularly)

### MLEM.Extended
Additions
- Added GenericFont compatibility for FontStashSharp
- Added a method to make sidescrolling collision detection easier with TiledMapCollisions
- Added some more TiledMapExtension utility methods

Improvements
- Reversed the y loop in GetCollidingTiles to account for gravity which is usually more important

Fixes
- Fixed some number parsing not using the invariant culture

### MLEM.Data
Additions
- Added StaticJsonConverter
- Added DynamicEnum, a cursed custom enumeration class that supports arbitrarily many values

Fixes
- Fixed some number parsing not using the invariant culture
- Fixed RawContentManager crashing with dynamic assemblies present