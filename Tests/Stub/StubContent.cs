using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Content;
using NUnit.Framework;

namespace Tests.Stub {
    public class StubContent : ContentManager {

        private readonly Dictionary<string, object> assets;

        public StubContent(Dictionary<string, object> assets) : base(new StubServices()) {
            this.RootDirectory = Path.Combine(TestContext.CurrentContext.TestDirectory, "Content");
            this.assets = assets;
        }

        public override T Load<T>(string assetName) {
            return (T) this.assets[assetName];
        }

    }
}