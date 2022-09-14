using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests {
    public class TestDataTextureAtlas {

        [Test]
        public void Test() {
            using var game = TestGame.Create();
            using var texture = new Texture2D(game.GraphicsDevice, 1, 1);
            var atlas = DataTextureAtlas.LoadAtlasData(new TextureRegion(texture), game.RawContent, "Texture.atlas");
            Assert.AreEqual(11, atlas.Regions.Count());

            // no added offset
            var table = atlas["LongTableUp"];
            Assert.AreEqual(table.Area, new Rectangle(0, 32, 64, 48));
            Assert.AreEqual(table.PivotPixels, new Vector2(16, 48 - 32));

            // added offset
            var table2 = atlas["LongTableLeft"];
            Assert.AreEqual(table2.Area, new Rectangle(64, 32, 64, 48));
            Assert.AreEqual(table2.PivotPixels, new Vector2(112 - 64, 48 - 32));

            // negative pivot
            var negativePivot = atlas["TestRegionNegativePivot"];
            Assert.AreEqual(negativePivot.Area, new Rectangle(0, 32, 16, 16));
            Assert.AreEqual(negativePivot.PivotPixels, new Vector2(-32, 46 - 32));

            // copies
            var copy1 = atlas["Copy1"];
            Assert.AreEqual(copy1.Area, new Rectangle(0 + 16, 32, 64, 48));
            Assert.AreEqual(copy1.PivotPixels, new Vector2(16 + 16, 48 - 32));
            var copy2 = atlas["Copy2"];
            Assert.AreEqual(copy2.Area, new Rectangle(0 + 32, 32 + 4, 64, 48));
            Assert.AreEqual(copy2.PivotPixels, new Vector2(16 + 32, 48 - 32 + 4));

            // data
            var data = atlas["DataTest"];
            Assert.AreEqual("ThisIsSomeData", data.GetData<string>("DataPoint1"));
            Assert.AreEqual("3.5", data.GetData<string>("DataPoint2"));
            Assert.AreEqual("---", data.GetData<string>("DataPoint3"));
        }

    }
}
