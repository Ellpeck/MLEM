using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Data.Content {
    /// <summary>
    /// Represents a version of <see cref="ContentManager"/> that doesn't load content binary <c>xnb</c> files, but rather as their regular formats. 
    /// </summary>
    public class RawContentManager : ContentManager, IGameComponent {

        private static readonly RawContentReader[] Readers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsSubclassOf(typeof(RawContentReader)) && !t.IsAbstract)
            .Select(t => t.GetConstructor(Type.EmptyTypes).Invoke(null))
            .Cast<RawContentReader>().ToArray();

        private readonly List<IDisposable> disposableAssets = new List<IDisposable>();

        /// <summary>
        /// The graphics device that this content manager uses
        /// </summary>
        public readonly GraphicsDevice GraphicsDevice;

        /// <summary>
        /// Creates a new content manager with an optionally specified root directory.
        /// </summary>
        /// <param name="serviceProvider">The service provider of your game</param>
        /// <param name="rootDirectory">The root directory. Defaults to "Content"</param>
        public RawContentManager(IServiceProvider serviceProvider, string rootDirectory = "Content") :
            base(serviceProvider, rootDirectory) {
            if (serviceProvider.GetService(typeof(IGraphicsDeviceService)) is IGraphicsDeviceService s)
                this.GraphicsDevice = s.GraphicsDevice;
        }

        /// <summary>
        /// Loads a raw asset with the given name, based on the <see cref="ContentManager.RootDirectory"/>.
        /// If the asset was previously loaded using this method, the cached asset is merely returned.
        /// </summary>
        /// <param name="assetName">The path and name of the asset to load, without extension.</param>
        /// <typeparam name="T">The type of asset to load</typeparam>
        /// <returns>The asset, either loaded from the cache, or from disk.</returns>
        public override T Load<T>(string assetName) {
            if (this.LoadedAssets.TryGetValue(assetName, out var ret) && ret is T t)
                return t;
            return this.Read<T>(assetName, default);
        }

        /// <inheritdoc/>
        protected override void ReloadAsset<T>(string originalAssetName, T currentAsset) {
            this.Read(originalAssetName, currentAsset);
        }

        private T Read<T>(string assetName, T existing) {
            var triedFiles = new List<string>();
            foreach (var reader in Readers.Where(r => r.CanRead(typeof(T)))) {
                foreach (var ext in reader.GetFileExtensions()) {
                    var file = Path.Combine(this.RootDirectory, $"{assetName}.{ext}");
                    triedFiles.Add(file);
                    if (!File.Exists(file))
                        continue;
                    using (var stream = File.OpenRead(file)) {
                        var read = reader.Read(this, assetName, stream, typeof(T), existing);
                        if (!(read is T t))
                            throw new ContentLoadException($"{reader} returned non-{typeof(T)} for asset {assetName}");
                        this.LoadedAssets[assetName] = t;
                        if (t is IDisposable d && !this.disposableAssets.Contains(d))
                            this.disposableAssets.Add(d);
                        return t;
                    }
                }
            }
            throw new ContentLoadException($"Asset {assetName} not found. Tried files {string.Join(", ", triedFiles)}");
        }

        /// <inheritdoc/>
        public override void Unload() {
            foreach (var d in this.disposableAssets)
                d.Dispose();
            this.disposableAssets.Clear();
            base.Unload();
        }

        /// <inheritdoc/>
        public void Initialize() {
        }

    }
}