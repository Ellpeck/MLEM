using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests;

public class TestDataTextureAtlas : GameTestFixture {

    [Test]
    public void Test([Values(0, 4)] int regionX, [Values(0, 4)] int regionY) {
        using var texture = new Texture2D(this.Game.GraphicsDevice, 1, 1);
        var region = new TextureRegion(texture, regionX, regionY, 1, 1);
        var atlas = DataTextureAtlas.LoadAtlasData(region, this.Game.RawContent, "Texture.atlas");
        Assert.AreEqual(13, atlas.Regions.Count());

        // no pivot
        var plant = atlas["Plant"];
        Assert.AreEqual(plant.Area, new Rectangle(96 + regionX, 0 + regionY, 16, 32));
        Assert.AreEqual(plant.PivotPixels, Vector2.Zero);

        // no added offset
        var table = atlas["LongTableUp"];
        Assert.AreEqual(table.Area, new Rectangle(0 + regionX, 32 + regionY, 64, 48));
        Assert.AreEqual(table.PivotPixels, new Vector2(16, 48 - 32));

        // added offset
        var table2 = atlas["LongTableDown"];
        Assert.AreEqual(table2.Area, new Rectangle(64 + regionX, 32 + regionY, 64, 48));
        Assert.AreEqual(table2.PivotPixels, new Vector2(112 - 64, 48 - 32));

        // negative pivot
        var negativePivot = atlas["TestRegionNegativePivot"];
        Assert.AreEqual(negativePivot.Area, new Rectangle(0 + regionX, 32 + regionY, 16, 16));
        Assert.AreEqual(negativePivot.PivotPixels, new Vector2(-32, 46 - 32));

        // cpy (pivot pixels should be identical to LongTableUp because they're region-internal)
        var copy1 = atlas["Copy1"];
        Assert.AreEqual(copy1.Area, new Rectangle(0 + 16 + regionX, 32 + regionY, 64, 48));
        Assert.AreEqual(copy1.PivotPixels, new Vector2(16, 48 - 32));
        var copy2 = atlas["Copy2"];
        Assert.AreEqual(copy2.Area, new Rectangle(0 + 32 + regionX, 32 + 4 + regionY, 64, 48));
        Assert.AreEqual(copy2.PivotPixels, new Vector2(16, 48 - 32));

        // frm
        var copy3 = atlas["Copy3"];
        Assert.AreEqual(copy3.Area, new Rectangle(0 + 2 + regionX, 32 + 4 + regionY, 64, 48));
        Assert.AreEqual(copy3.PivotPixels, new Vector2(16, 48 - 32));

        // data
        var data = atlas["DataTest"];
        Assert.AreEqual("ThisIsSomeData", data.GetData<string>("DataPoint1"));
        Assert.AreEqual("3.5", data.GetData<string>("DataPoint2"));
        Assert.AreEqual("---", data.GetData<string>("DataPoint3"));

        // spaces
        var spaces = atlas["Region with Spaces"];
        Assert.AreEqual(spaces.Area, new Rectangle(1 + regionX, 2 + regionY, 3, 4));
        Assert.AreEqual(spaces.GetDataKeys().Count(), 2);
        Assert.AreEqual(spaces.GetData<string>("SomeData"), "Some Value with Spaces");
        Assert.AreEqual(spaces.GetData<string>("Some Data with Spaces"), "SomeValue");
    }

}
