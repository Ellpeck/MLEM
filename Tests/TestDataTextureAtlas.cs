using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Textures;
using NUnit.Framework;
using Tests.Stub;

namespace Tests {
    public class TestDataTextureAtlas {

        [Test]
        public void Test() {
            const string data = @"
SimpleDeskUp
loc 0 0 48 32
piv 16 16
SimpleDeskRight
loc 48 0 48 32
piv 80 16

Plant
loc 96 0 16 32

LongTableUp
loc 0 32 64 48
piv 16 48
LongTableRight
loc 64 32 64 48
piv 112 48";
            using (var file = new FileInfo("texture.atlas").CreateText())
                file.Write(data);

            var content = new StubContent(new Dictionary<string, object> {
                {"texture", new Texture2D(new StubGraphics(), 1, 1)}
            });
            var atlas = content.LoadTextureAtlas("texture");
            Assert.AreEqual(atlas.Regions.Count(), 5);

            var table = atlas["LongTableUp"];
            Assert.AreEqual(table.Area, new Rectangle(0, 32, 64, 48));
            Assert.AreEqual(table.PivotPixels, new Vector2(16, 48 - 32));
        }

    }
}