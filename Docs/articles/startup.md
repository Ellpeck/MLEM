# MLEM.Startup

**MLEM.Startup** is a simple package that contains a `MlemGame` class which extends MonoGame's `Game`. This class contains additional properties that most games created with MonoGame and MLEM will have:
- An [InputHandler](https://github.com/Ellpeck/MLEM/wiki/Input-Handler)
- A `SpriteBatch` and `GraphicsDeviceManager`
- A [UiSystem](https://github.com/Ellpeck/MLEM/wiki/MLEM.Ui)
- Some delegate callbacks for loading, updating and drawing that allow additional code to be executed from outside the game class

Additionally, it comes with the [Coroutine](https://www.nuget.org/packages/Coroutine) package preinstalled. The Coroutine package allows creating and running operations alongside the regular game loop without asynchrony. It comes with a `CoroutineEvents` class that contains two types of events that are automatically invoked by `MlemGame`. For more information on how this is useful, see [the Coroutine README](https://github.com/Ellpeck/Coroutine/blob/main/README.md).