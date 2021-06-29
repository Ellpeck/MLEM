# Changelog
MLEM uses [semantic versioning](https://semver.org/).

This changelog also contains information on versions that have yet to be released. The changelog for unreleased versions might be edited over time as new features get added, changed or removed. To see the newest released version's code, check out the [release branch](https://github.com/Ellpeck/MLEM/tree/release).

Jump to version:
- [5.0.0](#500)

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
- Allow NinePatches to be drawn tiled rather than stretched
- Added the ability for Direction2 to be used as flags
- Made Padding and Direction2 DataContracts
- Expose the viewport of cameras
- Greatly improved the efficiency of line splitting for GenericFont and TokenizedString
- Improved performance of TextFormatter tokenization
- Replaced TextInputWrapper with a more refined MlemPlatform that includes the ability to open links on various platforms
- Allow for underline and shadow formatting codes to be mixed with font changing codes
- Exposed Keybind Combinations

Fixes
- Fixed the input handler querying input when the window is inactive
- Fixed UnderlineCode ending in the wrong places because it was marked as a font-changing code

Removals
- Removed the array-based GetRandomEntry method
- Removed obsolete ColorExtension methods

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
- Stop a panel's scroll bar from being removed from its children list automatically
- Removed unnecessary GraphicsDevice references from UiSystem
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