using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace Tests.Stub {
    public class StubContent : ContentManager {

        private readonly Dictionary<string, object> assets;

        public StubContent(Dictionary<string, object> assets) : base(new StubServices()) {
            this.assets = assets;
        }

        public override T Load<T>(string assetName) {
            return (T) this.assets[assetName];
        }

    }
}