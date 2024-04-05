![The MLEM logo](https://raw.githubusercontent.com/Ellpeck/MLEM/main/Media/Banner.png)

**MLEM Library for Extending MonoGame and FNA** is a set of multipurpose libraries for the game frameworks [MonoGame](https://www.monogame.net/) and [FNA](https://fna-xna.github.io/) that provides abstractions, quality of life improvements and additional features like an extensive ui system and easy input handling.

MLEM is platform-agnostic and multi-targets .NET Standard 2.0, .NET 6.0 and .NET Framework 4.5.2, which makes it compatible with MonoGame and FNA on Desktop, mobile devices and consoles.

# What next?
- Get it on [NuGet](https://www.nuget.org/packages?q=ellpeck+mlem)
- Get prerelease builds on [BaGet](https://nuget.ellpeck.de/?q=mlem)
- See the source code on [GitHub](https://github.com/Ellpeck/MLEM)
- See tutorials and API documentation on [the website](https://mlem.ellpeck.de/)
- Check out [the demos](https://github.com/Ellpeck/MLEM/tree/main/Demos) on [Desktop](https://github.com/Ellpeck/MLEM/tree/main/Demos.DesktopGL) or [Android](https://github.com/Ellpeck/MLEM/tree/main/Demos.Android)
- See [the changelog](https://mlem.ellpeck.de/CHANGELOG.html) for information on updates
- Join [the Discord server](https://link.ellpeck.de/discordweb) to ask questions

# Packages
- **MLEM** is the base package, which provides various small addons and abstractions for MonoGame and FNA, including a text formatting system and simple input handling
- **MLEM.Ui** provides a mouse, keyboard, gamepad and touch ready Ui system that features automatic anchoring, sizing and several ready-to-use element types
- **MLEM.Extended** ties in with MonoGame.Extended and other MonoGame and FNA libraries
- **MLEM.Data** provides simple loading and processing of textures and other data, including the ability to load non-XNB content files easily
- **MLEM.Startup** combines MLEM with some other useful libraries into a quick Game startup class
- **MLEM.Templates** contains cross-platform project templates

# Made with MLEM
- [Touchy Tickets](https://ell.lt/touchytickets), a mobile idle game ([Source](https://git.ellpeck.de/Ellpeck/TouchyTickets))
- [A Breath of Spring Air](https://ellpeck.itch.io/a-breath-of-spring-air), a short platformer ([Source](https://git.ellpeck.de/Ellpeck/GreatSpringGameJam))
- [Don't Wake Up](https://ellpeck.itch.io/dont-wake-up), a short puzzle game ([Source](https://github.com/Ellpeck/DontLetGo))
- Pong Clone, a very simple pong clone demo ([Source](https://github.com/luanfagu/pong))
- [Tiny Life](https://tinylifegame.com), an isometric life simulation game ([Modding API](https://github.com/Ellpeck/TinyLifeExampleMod))

If you created a project with the help of MLEM, you can get it added to this list by submitting an issue or a pull request. If its source is public, other people will be able to use your project as an example, too!

# Gallery
Here are some images that show a couple of MLEM's features.

The [MLEM.Ui](https://mlem.ellpeck.de/articles/ui) demo in action:

![A gif showing various user interface elements from the MLEM.Ui demo](https://raw.githubusercontent.com/Ellpeck/MLEM/main/Media/Ui.gif)

MLEM's [text formatting system](https://mlem.ellpeck.de/articles/text_formatting), which is compatible with both MLEM.Ui and regular sprite batch rendering:

![An image showing text with various colors and other formatting](https://raw.githubusercontent.com/Ellpeck/MLEM/main/Media/Formatting.png)

# Friends of MLEM
There are several other libraries and tools that work well in combination with MonoGame, FNA and MLEM. Here are some of them:
- [Contentless](https://github.com/Ellpeck/Contentless), a tool that removes the need to add assets to the MonoGame Content Pipeline manually
- [GameBundle](https://github.com/Ellpeck/GameBundle), a tool that packages MonoGame and other .NET applications into several distributable formats
- [Coroutine](https://github.com/Ellpeck/Coroutine), a package that implements Unity-style coroutines for any project
- [DynamicEnums](https://github.com/Ellpeck/DynamicEnums), a package that provides enum-like single-instance values with many additional capabilities
- [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended), a package that also provides several additional features for MonoGame
