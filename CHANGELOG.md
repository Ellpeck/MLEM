# Changelog
MLEM tries to adhere to [semantic versioning](https://semver.org/). Potentially breaking changes are written in **bold**.

Jump to version:
- [7.0.0](#700-in-development)
- [6.3.1](#631)
- [6.3.0](#630)
- [6.2.0](#620)
- [6.1.0](#610)
- [6.0.0](#600)
- [5.3.0](#530)
- [5.2.0](#520)
- [5.1.0](#510)
- [5.0.0](#500)

## 7.0.0 (In Development)

### MLEM
Additions
- **Added the ability for formatted (tokenized) strings to be drawn with custom rotation, origin and flipping**
- Added a RectangleF.FromCorners overload that accepts points

Improvements
- Allow NumberExtensions.GetPoints to include bottom and right coordinates

### MLEM.Ui
Additions
- Added the ability to set the anchor that should be used when a tooltip attaches to an element or the mouse
- Added the ability to display tooltips using the auto-nav style even when using the mouse
- Added the ScissorGroup element, which applies a scissor rectangle when drawing its content
- Added Panel.ScrollToTop and Panel.ScrollToBottom

Improvements
- **Include the SpriteBatchContext in OnDrawn, OnElementDrawn and OnSelectedElementDrawn**
- Allow scrolling panels to set height based on children by setting TreatSizeAsMaximum
- Track element area update recursion count in UiMetrics
- Made the Element.Children collection public

Fixes
- Fixed hidden scroll bars inhibiting scrolling on their parent panel
- Fixed scroll bars doing unnecessary calculations when hidden
- Fixed auto-sized elements sometimes updating their location based on outdated parent positions
- Fixed Panel.ScrollToElement not scrolling correctly when the panel's area is dirty

## 6.3.1

No code changes

## 6.3.0

### MLEM
Additions
- Added GraphicsExtensions.WithRenderTargets, a multi-target version of WithRenderTarget
- Added Zero, One, Linear and Clamp to Easings
- Added GetRandomEntry and GetRandomWeightedEntry to SingleRandom
- Added the ability to draw single corners of AutoTiling's extended auto tiles
- Added ColorHelper.TryFromHexString, a non-throwing version of FromHexString
- Added ToHexStringRgba and ToHexStringRgb to ColorExtensions

Improvements
- Stopped the text formatter throwing if a color can't be parsed
- Improved text formatter tokenization performance
- Allow using control and arrow keys to move the visible area of a text input
- Allow formatting codes applied later to override settings of earlier ones

Fixes
- Fixed TextInput not working correctly when using surrogate pairs
- Fixed InputHandler touch states being initialized incorrectly when touch handling is disabled
- Fixed empty NinePatch regions stalling when using tile mode
- Fixed bold and italic formatting code closing tags working on each other

### MLEM.Ui
Additions
- Added UiControls.NavType, which stores the most recently used type of ui navigation
- Added SetWidthBasedOnAspect and SetHeightBasedOnAspect to images
- Added the ability to set a custom SamplerState for images
- Added some useful additional constructors to various elements

Improvements
- Allow scrolling panels to contain other scrolling panels
- Allow dropdowns to have scrolling panels
- Improved Panel performance when adding and removing a lot of children
- Don't reset the caret position of a text field when selecting or deselecting it
- Improved UiParser.ParseImage with locks and a callback action

Fixes
- Fixed panels updating their relevant children too much when the scroll bar is hidden
- Fixed a stack overflow exception when a panel's scroll bar auto-hiding causes elements to gain height
- Fixed scrolling panels calculating their height incorrectly when their first child is hidden

### MLEM.Extended
Improvements
- Updated to FontStashSharp 1.3.0's API
- Expose character and line spacing in GenericStashFont

### MLEM.Data
Fixes
- Fixed various exception types not being wrapped by ContentLoadExceptions when loading raw or JSON content

## 6.2.0

### MLEM
Additions
- Added a simple outline formatting code
- Added the ability to add inverse modifiers to a Keybind
- Added GenericInput collections AllKeys, AllMouseButtons, AllButtons and AllInputs
- Added TextFormatter.StripAllFormatting

Improvements
- Increased TextFormatter macro recursion limit to 64
- Allow changing the default values used by default TextFormatter codes
- Allow setting ExternalGestureHandling through the InputHandler constructor
- Allow specifying start and end indices when drawing a TokenizedString
- Include control characters in TextInput FileNames and PathNames rules

Fixes
- Fixed control characters being included in TextInput
- Fixed TextInputs behaving incorrectly when switching between multiline and single-line modes
- Fixed TextInput drawing characters with the wrong width if a masking character is used
- Fixed a multiline TextInput's cursor not returning to the default position when the last character is removed
- Fixed GetRandomWeightedEntry distribution not being equal for equal weights

Removals
- Marked GetDownTime, GetUpTime and GetTimeSincePress in Keybind and Combination as obsolete

### MLEM.Ui
Additions
- Added AutoInlineCenter and AutoInlineBottom anchors
- Added UiAnimation system
- Added AddCustomStyle and ApplyCustomStyle to UiStyle to allow for easy custom styling of elements
- Added UiControls.PressElement
- Added TextField.EnterReceiver
- Added a copy constructor to UiStyle

Improvements
- Increased Element area calculation recursion limit to 64
- Improved the SquishingGroup algorithm by prioritizing each element's final size
- Allow specifying start and end indices when drawing a Paragraph
- Allow elements with larger children to influence a panel's scrollable area
- Remove all elements from a UiSystem when it is disposed
- Made elements' ui styles be inherited by their children

Fixes
- Fixed images not updating their hidden state properly when the displayed texture changes
- Fixed AutoInline elements overflowing into their parent if it's taller
- Fixed Paragraph and Checkbox not reacting to SquishingGroup sizing properly
- Fixed TextInput and Slider still reacting to input when they are selected, but not part of the active root
- Fixed dropdown menu panels not updating their width when the dropdown's width changes
- Fixed removing and later adding children to a scrolling panel showing the scroll bar erroneously

### MLEM.Data
Improvements
- Improved RuntimeTexturePacker performance for differently sized textures
- Allow querying the amount of RuntimeTexturePacker regions

## 6.1.0

### MLEM
Additions
- Added TokenizedString.Realign
- Added GetFlags and GetUniqueFlags to EnumHelper
- Added GetDownTime, GetUpTime, GetTimeSincePress, WasModifierDown and WasDown to Keybind and Combination
- Added the ability for UniformTextureAtlases to have padding for each region
- Added UniformTextureAtlas methods ToList and ToDictionary
- Added SingleRandom and SeedSource
- Added TokenizedString.GetArea
- Added InputHandler.WasPressedForLess and related methods as well as InputHandler.IsPressedIgnoreRepeats
- Added RandomExtensions.NextSingle with minimum and maximum values
- Added subscript and superscript formatting codes
- **Added the ability to find paths to one of multiple goals using AStar**

Improvements
- Improved EnumHelper.GetValues signature to return an array
- Allow using external gesture handling alongside InputHandler through ExternalGestureHandling
- Discard old data when updating a StaticSpriteBatch
- Multi-target net452, making MLEM compatible with MonoGame for consoles
- Allow retrieving the cost of a calculated path when using AStar
- Added trimming and AOT annotations and made MLEM trimmable
- Allow specifying percentage-based padding for a NinePatch
- Improved the way InputHandler down time calculation works
- Allow explicitly specifying each region for extended auto tiles
- Added a generic version of IGenericDataHolder.SetData
- Allow formatting codes to have an arbitrary custom width
- Allow initializing text formatters without default codes and macros
- **Drastically improved StaticSpriteBatch batching performance**
- **Made GenericFont and TokenizedString support UTF-32 characters like emoji**

Fixes
- Fixed TokenizedString handling trailing spaces incorrectly in the last line of non-left aligned text
- Fixed some TokenizedString tokens starting with a line break not being split correctly
- Fixed InputHandler maintaining old input states when input types are toggled off
- Fixed Combination.IsModifierDown querying one of its modifiers instead of all of them

Removals
- Removed DataContract attribute from GenericDataHolder
- Marked EnumHelper as obsolete due to its reimplementation in [DynamicEnums](https://www.nuget.org/packages/DynamicEnums)
- Marked Code.GetReplacementString as obsolete
- Marked TokenizedString.Measure as obsolete in favor of GetArea
- Marked non-GenericInput versions of IsDown, IsUp, IsPressed and related methods as obsolete in favor of GenericInput ones

### MLEM.Ui
Additions
- Added some extension methods for querying Anchor types
- Added Element.AutoSizeAddedAbsolute to allow for more granular control of auto-sizing
- Added Element.OnAddedToUi and Element.OnRemovedFromUi
- Added ScrollBar.MouseDragScrolling
- Added Panel.ScrollToElement
- Added ElementHelper.MakeGrid
- Added Button.AutoDisableCondition

Improvements
- Allow elements to auto-adjust their size even when their children are aligned oddly
- Close other dropdowns when opening a dropdown
- Generified UiMarkdownParser by adding abstract UiParser
- Multi-target net452, making MLEM compatible with MonoGame for consoles
- Added trimming and AOT annotations and made MLEM.Ui trimmable
- Ensure paragraphs display up-to-date versions of their text callbacks
- Set cornflower blue as the default link color
- Added TextField.OnCopyPasteException to allow handling exceptions thrown by TextCopy
- Avoid paragraphs splitting or truncating their text unnecessarily
- Automatically mark elements dirty when various member values are changed
- Allow initializing a ui system's text formatter without default codes and macros

Fixes
- Fixed parents of elements that prevent spill not being notified properly
- Fixed paragraphs sometimes not updating their position properly when hidden because they're empty
- Fixed panels sometimes not drawing children that came into view when their positions changed unexpectedly
- Fixed UiMarkdownParser not parsing formatting in headings and blockquotes
- Fixed Element.OnChildAdded and Element.OnChildRemoved being called for grandchildren when a child is added
- Fixed an exception when trying to force-update the area of an element without a ui system
- Fixed the scroll bar of an empty panel being positioned incorrectly
- Fixed UiControls maintaining old input states when input types are toggled off
- Fixed an occasional deadlock when a game is disposed with a scrolling Panel present
- Fixed UiStyle.LinkColor not being applied to the ui system when changed

Removals
- Marked Element.OnDisposed as obsolete in favor of the more predictable OnRemovedFromUi

### MLEM.Data
Additions
- Added data, from, and copy instructions to DataTextureAtlas
- Added the ability to add additional regions to a RuntimeTexturePacker after packing
- Added GetFlags, GetUniqueFlags and IsDefined to DynamicEnum
- Added DataTextureAtlas.ToDictionary

Improvements
- Allow data texture atlas pivots and offsets to be negative
- Made RuntimeTexturePacker restore texture region name and pivot when packing
- Multi-target net452, making MLEM compatible with MonoGame for consoles
- Added trimming and AOT annotations and made MLEM.Data trimmable
- Store a RuntimeTexturePacker packed texture region's source region
- Use JSON.NET attributes in favor of DataContract and DataMember
- Made JsonTypeSafeWrapper.Of generic to potentially avoid reflective instantiation

Fixes
- Fixed data texture atlases not allowing most characters in their region names

Removals
- Marked DynamicEnum as obsolete due to its reimplementation in [DynamicEnums](https://www.nuget.org/packages/DynamicEnums)

### MLEM.Extended
Additions
- Added Range extension methods GetPercentage and FromPercentage

Improvements
- Multi-target net452, making MLEM compatible with MonoGame for consoles
- Added trimming and AOT annotations and made MLEM.Extended trimmable
- **Made GenericBitmapFont and GenericStashFont support UTF-32 characters like emoji**

### MLEM.Startup
Improvements
- Multi-target net452, making MLEM compatible with MonoGame for consoles
- Added trimming and AOT annotations and made MLEM.Startup trimmable

## 6.0.0

### MLEM
Additions
- Added consuming variants of IsPressed methods to InputHandler and Keybind
- Added SpriteBatchContext struct and extensions
- Added InputHandler.InvertPressBehavior
- Added ReverseInput, ReverseOutput and AndThen to Easings
- Added an Enum constructor to GenericInput
- Added RandomPitchModifier and GetRandomPitch to SoundEffectInfo
- Added TextInput class, which is an isolated version of MLEM.Ui's TextField logic
- Added MLEM.FNA, which is fully compatible with FNA
- Added TryGetUpTime, GetUpTime, TryGetTimeSincePress and GetTimeSincePress to InputHandler

Improvements
- Allow comparing Keybind and Combination based on the amount of modifiers they have
- Allow using multiple textures in a StaticSpriteBatch
- Added GenericInput support for Buttons.None
- Improved the way terminating formatting codes work by introducing SimpleEndCode
- Allow RandomExtensions to operate on any ICollection

Removals
- Marked AStar.InfiniteCost as obsolete

### MLEM.Ui
Additions
- Added Element.AutoNavGroup which allows forming groups for auto-navigation
- Added UiMarkdownParser
- Added MLEM.Ui.FNA, which is fully compatible with FNA

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
- Allow manually setting a RootElement as CanBeActive

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
- Added MLEM.Extended.FNA, which is fully compatible with FNA

Improvements
- Allow using a StaticSpriteBatch to render an IndividualTiledMapRenderer

### MLEM.Data
Additions
- Added the ability to add padding to RuntimeTexturePacker texture regions
- Added the ability to pack UniformTextureAtlas and DataTextureAtlas using RuntimeTexturePacker
- Added MLEM.Data.FNA, which is fully compatible with FNA

Improvements
- Premultiply textures when using RawContentManager
- Allow enumerating all region names of a DataTextureAtlas
- Cache RuntimeTexturePacker texture data while packing to improve performance
- Greatly improved RuntimeTexturePacker performance
- Allow specifying multiple names for a DataTextureAtlas region

Fixes
- Fixed SoundEffectReader incorrectly claiming it could read ogg and mp3 files

### MLEM.Startup
Additions
- Added MLEM.Startup.FNA, which is fully compatible with FNA

### MLEM.Templates
Improvements
- Updated to MonoGame 3.8.1

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
