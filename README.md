![The MLEM logo](https://raw.githubusercontent.com/Ellpeck/MLEM/main/Media/Banner.png)

**MLEM Library for Extending MonoGame, FNA and KNI** is a set of multipurpose libraries for the game frameworks [MonoGame](https://www.monogame.net/), [FNA](https://fna-xna.github.io/) and [KNI](https://github.com/kniEngine/kni) that provides abstractions, quality of life improvements and additional features like an extensive ui system and easy input handling.

MLEM is platform-agnostic and multi-targets .NET Standard 2.0, .NET 8.0 and .NET Framework 4.5.2, which makes it compatible with MonoGame, FNA and KNI on Desktop, mobile devices, consoles and web.

# What Next?
- Get it on [NuGet](https://www.nuget.org/packages?q=ellpeck+mlem)
- Get prerelease builds on [BaGet](https://nuget.ellpeck.de/?q=mlem)
- See the source code on [GitHub](https://github.com/Ellpeck/MLEM)
- See tutorials and API documentation on [the website](https://mlem.ellpeck.de/)
- Check out [the demos](https://github.com/Ellpeck/MLEM/tree/main/Demos) on [Desktop](https://github.com/Ellpeck/MLEM/tree/main/Demos.DesktopGL), [Android](https://github.com/Ellpeck/MLEM/tree/main/Demos.Android) or [Web](https://mlem.ellpeck.de/demo)
- See [the changelog](https://mlem.ellpeck.de/CHANGELOG.html) for information on updates
- Join [the Discord server](https://link.ellpeck.de/discordweb) to ask questions

# Packages
MLEM has a modular architecture, meaning you can pick and choose the components that you'd like to install and use. All other MLEM packages depend on the MLEM base package.

- **MLEM** is the base package, which provides various small addons and abstractions for MonoGame, FNA and KNI, including a text formatting system and simple input handling
- **MLEM.Ui** provides a mouse, keyboard, gamepad and touch ready Ui system that features automatic anchoring, sizing and several ready-to-use element types
- **MLEM.Extended** ties in with MonoGame.Extended and other MonoGame and FNA libraries
- **MLEM.Data** provides simple loading and processing of textures and other data, including the ability to load non-XNB content files easily
- **MLEM.Startup** combines MLEM with some other useful libraries into a quick Game startup class
- **MLEM.Templates** contains cross-platform project templates

# Made with MLEM
MLEM has been used to make some very cool games, and it's an honor to showcase them here! For screenshots of games that use MLEM.Ui in particular, see the [MLEM.Ui Gallery](https://mlem.ellpeck.de/articles/ui_gallery.html).

- [Touchy Tickets](https://ell.lt/touchytickets), a mobile idle game ([Source](https://git.ellpeck.de/Ellpeck/TouchyTickets))
- [A Breath of Spring Air](https://ellpeck.itch.io/a-breath-of-spring-air), a short platformer ([Source](https://git.ellpeck.de/Ellpeck/GreatSpringGameJam))
- [Don't Wake Up](https://ellpeck.itch.io/dont-wake-up), a short puzzle game ([Source](https://github.com/Ellpeck/DontLetGo))
- Pong Clone, a very simple pong clone demo ([Source](https://github.com/luanfagu/pong))
- [Tiny Life](https://tinylifegame.com), an isometric life simulation game ([Modding API](https://github.com/Ellpeck/TinyLifeExampleMod))
- [Vulcard](https://store.steampowered.com/app/3764530/Vulcard/), a two-player deckbuilding card battler
- [Foe Frenzy](https://store.steampowered.com/app/1194170/Foe_Frenzy/), a fast-paced battle party game

If you created a project with the help of MLEM, you can get it added to this list by submitting an issue or a pull request. If its source is public, other people will be able to use your project as an example, too!

# Gallery
Here are some images that show a couple of MLEM's features.

![A gif showing various user interface elements from the MLEM.Ui demo](https://raw.githubusercontent.com/Ellpeck/MLEM/main/Media/Ui.gif)

The [MLEM.Ui](https://mlem.ellpeck.de/articles/ui.html) demo in action. Also check out the [MLEM.Ui Gallery](https://mlem.ellpeck.de/articles/ui_gallery.html) for screenshots of games that use MLEM.Ui.

![An image showing text with various colors and other formatting](https://raw.githubusercontent.com/Ellpeck/MLEM/main/Media/Formatting.png)

MLEM's [text formatting system](https://mlem.ellpeck.de/articles/text_formatting.html), which is compatible with both MLEM.Ui and regular sprite batch rendering.

# Friends of MLEM
There are several other libraries and tools that work well in combination with MonoGame, FNA, KNI and MLEM. Here are some of them:
- [Contentless](https://github.com/Ellpeck/Contentless), a tool that removes the need to add assets to the MonoGame Content Pipeline manually
- [GameBundle](https://github.com/Ellpeck/GameBundle), a tool that packages MonoGame and other .NET applications into several distributable formats
- [Coroutine](https://github.com/Ellpeck/Coroutine), a package that implements Unity-style coroutines for any project
- [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended), a package that also provides several additional features for MonoGame
