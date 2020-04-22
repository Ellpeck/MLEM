using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Content {
    public class RawContentManager : ContentManager, IGameComponent {

        private static readonly RawContentReader[] Readers = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsSubclassOf(typeof(RawContentReader)) && !t.IsAbstract)
            .Select(t => t.GetConstructor(Type.EmptyTypes).Invoke(null))
            .Cast<RawContentReader>().ToArray();

        private readonly List<IDisposable> disposableAssets = new List<IDisposable>();

        public readonly GraphicsDevice GraphicsDevice;

        public RawContentManager(IServiceProvider serviceProvider, string rootDirectory = "Content") :
            base(serviceProvider, rootDirectory) {
            if (serviceProvider.GetService(typeof(IGraphicsDeviceService)) is IGraphicsDeviceService s)
                this.GraphicsDevice = s.GraphicsDevice;
        }

        public override T Load<T>(string assetName) {
            if (this.LoadedAssets.TryGetValue(assetName, out var ret) && ret is T t)
                return t;
            return this.Read<T>(assetName, default);
        }

        private static RawContentReader GetReader<T>() {
            var reader = Readers.FirstOrDefault(r => r.CanRead(typeof(T)));
            if (reader == null)
                throw new ContentLoadException($"{typeof(T)} has no RawContentReader");
            return reader;
        }

        protected override void ReloadAsset<T>(string originalAssetName, T currentAsset) {
            this.Read(originalAssetName, currentAsset);
        }

        private T Read<T>(string assetName, T existing) {
            var reader = GetReader<T>();
            foreach (var ext in reader.GetFileExtensions()) {
                var file = new FileInfo(Path.Combine(this.RootDirectory, $"{assetName}.{ext}"));
                if (!file.Exists)
                    continue;
                using (var stream = file.OpenRead()) {
                    var read = reader.Read(this, assetName, stream, typeof(T), existing);
                    if (!(read is T t))
                        throw new ContentLoadException($"{reader} returned non-{typeof(T)} for asset {assetName}");
                    this.LoadedAssets[assetName] = t;
                    if (t is IDisposable d && !this.disposableAssets.Contains(d))
                        this.disposableAssets.Add(d);
                    return t;
                }
            }
            throw new ContentLoadException($"Asset {assetName} not found");
        }

        public override void Unload() {
            foreach (var d in this.disposableAssets)
                d.Dispose();
            this.disposableAssets.Clear();
            base.Unload();
        }

        public void Initialize() {
        }

    }
}