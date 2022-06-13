# Changelog
MLEM tries to adhere to [semantic versioning](https://semver.org/). Breaking changes are written in **bold**.

Jump to version:
- [5.4.0 (Unreleased)](#540-unreleased)
- [5.3.0](#530)
- [5.2.0](#520)
- [5.1.0](#510)
- [5.0.0](#500)

## 5.4.0 (Unreleased)
### MLEM
Additions
- Added consuming variants of IsPressed methods to InputHandler and Keybind
- Added SpriteBatchContext struct and extensions
- Added InputHandler.InvertPressBehavior
- Added ReverseInput, ReverseOutput and AndThen to Easings
- Added an Enum constructor to GenericInput
- Added RandomPitchModifier and GetRandomPitch to SoundEffectInfo

Improvements
- Allow comparing Keybind and Combination based on the amount of modifiers they have
- Allow using multiple textures in a StaticSpriteBatch

### MLEM.Ui
Additions
- Added Element.AutoNavGroup which allows forming groups for auto-navigation
- Added UiMarkdownParser

Improvements
- Ensure that Element.IsMouseOver is always accurate by making it an auto-property
- Started using SpriteBatchContext for Draw and DrawTransformed methods
- Make use of the new consuming variants in InputHandler and Keybind to consume UiControls inputs
- Allow Tooltip to manage more than one paragraph and make it easier to add new lines
- Allow adding dropdown elements at a specified index
- Turned Tooltip paragraph styling into style properties
- Improved ElementHelper.AddTooltip overloads
- Don't query a paragraph's text callback in the constructor
- Allow manually hiding a paragraph without its text overriding the hidden state
- Added optional isKeybindAllowed parameter to KeybindButton

Fixes
- Fixed auto-nav tooltip displaying on the selected element even when not in auto-nav mode
- Fixed radio buttons not unchecking all other radio buttons with the same root element
- Fixed elements not being deselected when removed through RemoveChild
- Fixed elements sometimes staying hidden when they shouldn't in scrolling panels
- Fixed elements' OnDeselected events not being raised when CanBeSelected is set to false while selected
- Fixed gamepad auto-nav angle being incorrect for some elements

Removals
- Marked old Draw and DrawTransformed overloads as obsolete in favor of SpriteBatchContext ones
- Marked Tooltip.Paragraph as obsolete in favor of new Paragraphs collection

### MLEM.Extended
Additions
- Added LayerPositionF

Improvements
- Allow using a StaticSpriteBatch to render an IndividualTiledMapRenderer

### MLEM.Data
Additions
- Added the ability to add padding to RuntimeTexturePacker texture regions
- Added the ability to pack UniformTextureAtlas and DataTextureAtlas using RuntimeTexturePacker

Improvements
- Premultiply textures when using RawContentManager
- Allow enumerating all region names of a DataTextureAtlas
- Cache RuntimeTexturePacker texture data while packing to improve performance
- Greatly improved RuntimeTexturePacker performance

Fixes
- Fixed SoundEffectReader incorrectly claiming it could read ogg and mp3 files

## 5.3.0
### MLEM
Additions
- Added StringBuilder overloads to GenericFont
- Added ColorExtensions.Multiply
- Added SoundEffectInstanceHandler.Stop
- Added TextureRegion.OffsetCopy
- Added RectangleF.DistanceSquared and RectangleF.Distance
- Added GamepadExtensions.GetAnalogValue to get the analog value of any gamepad button
- Added InputHandler.TryGetDownTime

Improvements
- Generify GenericFont's string drawing
- Added InputHandler mouse and touch position querying that preserves the game's viewport
- Added float version of GetRandomWeightedEntry
- Allow LinkCode to specify a color to draw with
- Allow better control over the order and layout of a Keybind's combinations
- Allow setting a gamepad button deadzone in InputHandler
- Trigger InputHandler key and gamepad repeats for the most recently pressed input
- Added properties and constructors for existing operator overloads to GenericInput

Fixes
- **Fixed a formatting Code only knowing about the last Token that it is applied in**
- Fixed Code.Draw receiving the index in the current line rather than the current token
- Fixed StaticSpriteBatch handling rotated sprites incorrectly
- Fixed InputHandler.InputsPressed ignoring repeat events for keyboards and gamepads

Removals
- **Removed InputHandler.StoreAllActiveInputs and always store all active inputs**
- Renamed GenericFont.OneEmSpace to Emsp (and marked OneEmSpace as obsolete)

### MLEM.Ui
Additions
- Added Element.OnStyleInit event
- Added UiControls.AutoNavModeChanged event

Improvements
- Allow for checkboxes and radio buttons to be disabled
- Only set a paragraph's area dirty when a text change would cause it to change size
- Ensure that a panel gets notified of all relevant changes by calling OnChildAreaDirty for all grandchildren
- Avoid unnecessary panel updates by using an Epsilon comparison when scrolling children
- Allow setting a default text alignment for paragraphs in UiStyle
- Made custom values of Element.Style persist when a new ui style is set
- Update elements less aggressively when changing a ui system's style
- Automatically update all elements when changing a ui system's viewport
- Allow setting a default color for clickable links in UiStyle
- Allow ElementHelper's KeybindButton to query a combination at a given index
- Allow ElementHelper's KeybindButton to accept a Keybind for clearing a combination
- Automatically select the first element when a dropdown is opened in auto nav mode
- Improved gamepad navigation by employing angles between elements
- Prefer elements that have the same parent as the currently selected element when using gamepad navigation
- Allow specifying a custom position for a tooltip to snap to
- Allow tooltips to display for elements when selected in auto-nav mode

Fixes
- Fixed paragraph links having incorrect hover locations when using special text alignments
- Fixed the graphics device's viewport being ignored for mouse and touch queries
- Fixed auto-navigating panels not scrolling to the center of elements properly
- Fixed UiControls allowing for non-selectable or non-mouseable elements to be marked as selected or moused
- Fixed buttons and checkboxes changing their CanBeSelected and CanBePressed values when being disabled
- Fixed children of Panel scroll bars also being scrolled
- Fixed RootElement.CanSelectContent and Element.IsSelected returning incorrect results when CanBeSelected changes
- Fixed dropdowns with some non-selectable children failing to navigate when using gamepad controls
- Fixed UiMetrics.ForceAreaUpdateTime being inaccurate for nested elements
- Fixed tooltips sometimes ignoring manually set IsHidden values
- Fixed delayed tooltips sometimes displaying in the wrong location for one frame

Removals
- Marked StyleProp equality members as obsolete
- Marked Element.BeginDelegate and Element.BeginImpl as obsolete
- Marked Element.DrawEarly and UiSystem.DrawEarly as obsolete

### MLEM.Extended
Improvements
- Preserve texture region names when converting between MLEM and MG.Extended

### MLEM.Data
Improvements
- Rethrow exceptions when no RawContentManager readers could be constructed
- Make Newtonsoft.Json dependency optional

Removals
- Marked CopyExtensions as obsolete

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