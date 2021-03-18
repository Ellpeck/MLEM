using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Data.Content;
using NUnit.Framework;
using Tests.Stub;

namespace Tests {
    public class TestDataTextureAtlas {

        [Test]
        public void Test() {
            var atlas = DataTextureAtlas.LoadAtlasData(null, new RawContentManager(new StubServices()), "Texture.atlas");
            Assert.AreEqual(atlas.Regions.Count(), 5);

            var table = atlas["LongTableUp"];
            Assert.AreEqual(table.Area, new Rectangle(0, 32, 64, 48));
            Assert.AreEqual(table.PivotPixels, new Vector2(16, 48 - 32));
        }

    }
}