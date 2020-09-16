# Raw Content Manager

Sometimes, there's a good reason for wanting to load game assets directly rather than using the MonoGame Content Pipeline, which packs files into a binary `xnb` format. Those reasons include, for example, making your game easily moddable or allowing for texture packs.

The **MLEM.Data** package contains a solution for this: `RawContentManager`.

## What it does
A raw content manager works very similarly to a regular `ContentManager`: You can load different types of assets through the `Load<T>` method, and they will automatically be managed and disposed when the game closes.

However, the `RawContentManager` loads assets in their usual file type, rather than `xnb`, meaning that they don't have to be compiled using the Content Pipeline first. 

## How to use it
To create a new raw content manager, simply call its constructor in your `LoadContent` method. Optionally, you can add it as a game component, which will automatically dispose it when the game closes.
```cs
protected override void LoadContent() {
    this.rawContent = new RawContentManager(this.Services);
    this.Components.Add(this.rawContent);

    // load other content here
}
```

Then, you can simply load an asset in your `Content` directory like you would with the regular `ContentManager`:
```cs
this.testTexture = this.rawContent.Load<Texture2D>("Textures/Test");
```

## Adding more content types
By default, the raw content manager supports the following types, as long as their files are appended with one of the supported file extensions:
- `Texture2D` (png, bmp, gif, jpg, tif, dds)
- `SoundEffect` (ogg, wav, mp3)
- `Song` (gg, wav, mp3)
- Any XML files (xml)
- Any JSON files (json)

To add more content types that can be loaded by the raw content manager, you simply have to extend either `RawContentReader` or the generic version, `RawContentReader<T>`. For example, this is a content reader that loads a `txt` file as a string:
```cs
using System.IO;
using MLEM.Data.Content;

namespace Test {
    public class StringReader : RawContentReader<string> {

        public override string[] GetFileExtensions() {
            // return an array of supported file extensions
            return new[] {"txt"};
        }

        protected override string Read(RawContentManager manager, string assetPath, Stream stream, string existing) {
            // use the stream (or the asset path) here and
            // return the loaded content file
            using (var reader = new StreamReader(stream))
                return reader.ReadToEnd();
        }

    }
}
```
As `RawContentManager` automatically collects all raw content readers in the loaded assemblies, you don't have to register your custom reader anywhere.