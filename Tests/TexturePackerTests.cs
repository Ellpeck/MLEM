using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Maths;
using MLEM.Textures;
using NUnit.Framework;
using Color = Microsoft.Xna.Framework.Color;

namespace Tests;

public class TexturePackerTests : GameTestFixture {

    private readonly List<TextureRegion> generatedTextures = [];

    [TearDown]
    public void TearDown() {
        foreach (var tex in this.generatedTextures)
            tex.Texture.Dispose();
        this.generatedTextures.Clear();
    }

    [Test]
    public void TestPacking() {
        using var packer = new RuntimeTexturePacker();
        for (var i = 0; i < 5; i++) {
            var width = 16 * (i + 1);
            packer.Add(this.MakeTextureRegion(width, 64), r => {
                Assert.AreEqual(r.Width, width);
                Assert.AreEqual(r.Height, 64);
            });
        }
        packer.Pack(this.Game.GraphicsDevice);
        TexturePackerTests.SaveTexture(packer);
        Assert.AreEqual(packer.PackedTexture.Width, 128);
        Assert.AreEqual(packer.PackedTexture.Height, 128);
    }

    [Test]
    public void TestOverlap([Values(0, 1, 5, 10)] int padding) {
        var packed = new List<TextureRegion>();
        using var packer = new RuntimeTexturePacker();
        for (var i = 1; i <= 1000; i++)
            packer.Add(this.MakeTextureRegion(i % 239, i % 673), packed.Add, padding);
        packer.Pack(this.Game.GraphicsDevice);

        TexturePackerTests.SaveTexture(packer, padding.ToString());

        foreach (var r1 in packed) {
            var r1Padded = r1.Area;
            r1Padded.Inflate(padding, padding);

            foreach (var r2 in packed) {
                if (r1 == r2)
                    continue;

                Assert.False(r1.Area.Intersects(r2.Area), $"Regions {r1.Area} and {r2.Area} intersect");

                var r2Padded = r2.Area;
                r2Padded.Inflate(padding, padding);
                Assert.False(r1Padded.Intersects(r2Padded), $"Padded regions {r1Padded} and {r2Padded} intersect");
            }
        }
    }

    [Test]
    public void TestDisposal() {
        using var packer = new RuntimeTexturePacker(disposeTextures: true);
        var disposeLater = this.MakeTextureRegion(16, 16);
        packer.Add(disposeLater, TexturePackerTests.StubResult);
        packer.Add(new TextureRegion(disposeLater, 0, 0, 8, 8), TexturePackerTests.StubResult);
        packer.Pack(this.Game.GraphicsDevice);
        Assert.True(disposeLater.Texture.IsDisposed);
        Assert.False(packer.PackedTexture.IsDisposed);
    }

    [Test]
    public void TestBounds() {
        // test power of two forcing
        using var packer3 = new RuntimeTexturePacker(true);
        packer3.Add(this.MakeTextureRegion(37, 170), TexturePackerTests.StubResult);
        packer3.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(64, packer3.PackedTexture.Width);
        Assert.AreEqual(256, packer3.PackedTexture.Height);

        // test square forcing
        using var packer4 = new RuntimeTexturePacker(forceSquare: true);
        packer4.Add(this.MakeTextureRegion(37, 170), TexturePackerTests.StubResult);
        packer4.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(170, packer4.PackedTexture.Width);
        Assert.AreEqual(170, packer4.PackedTexture.Height);
    }

    [Test]
    public void TestPackMultipleTimes() {
        using var packer = new RuntimeTexturePacker();

        // pack the first time
        var results = 0;
        for (var i = 0; i < 10; i++)
            packer.Add(this.MakeTextureRegion(64, 64), _ => results++);
        packer.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(10, results);

        // pack again
        packer.Add(this.MakeTextureRegion(64, 64), _ => results++);
        packer.Pack(this.Game.GraphicsDevice);
        // all callbacks are called again, so we add 10 again, as well as the callback we just added
        Assert.AreEqual(10 + 10 + 1, results);
    }

    [Test]
    public void TestPackTimes([Values(1, 100, 1000, 5000, 10000)] int total) {
        var random = new Random(1238492384);
        using var sameSizePacker = new RuntimeTexturePacker();
        using var diffSizePacker = new RuntimeTexturePacker();
        for (var i = 0; i < total; i++) {
            sameSizePacker.Add(this.MakeTextureRegion(10, 10), TexturePackerTests.StubResult);
            diffSizePacker.Add(this.MakeTextureRegion(random.Next(10, 200), random.Next(10, 200)), TexturePackerTests.StubResult);
        }
        sameSizePacker.Pack(this.Game.GraphicsDevice);
        diffSizePacker.Pack(this.Game.GraphicsDevice);

        TexturePackerTests.SaveTexture(sameSizePacker, "SameSize");
        TexturePackerTests.SaveTexture(diffSizePacker, "DiffSize");

        TestContext.WriteLine($"""
            {total} regions,
            same-size {sameSizePacker.LastCalculationTime.TotalMilliseconds}ms calc, {sameSizePacker.LastPackTime.TotalMilliseconds}ms pack, {sameSizePacker.LastTotalTime.TotalMilliseconds}ms total,
            diff-size {diffSizePacker.LastCalculationTime.TotalMilliseconds}ms calc, {diffSizePacker.LastPackTime.TotalMilliseconds}ms pack, {diffSizePacker.LastTotalTime.TotalMilliseconds}ms total
            """);
    }

    private TextureRegion MakeTextureRegion(int width, int height) {
        var color = new Color((uint) SingleRandom.Int(this.generatedTextures.Count)) {A = 255};
        var texture = new Texture2D(this.Game.GraphicsDevice, Math.Max(width, 1), Math.Max(height, 1));
        using (var data = texture.GetTextureData()) {
            for (var x = 0; x < texture.Width; x++) {
                for (var y = 0; y < texture.Height; y++)
                    data[x, y] = color;
            }
        }
        var region = new TextureRegion(texture, 0, 0, width, height);
        this.generatedTextures.Add(region);
        return region;
    }

    private static void SaveTexture(RuntimeTexturePacker packer, string append = "") {
        var caller = new System.Diagnostics.StackTrace(1).GetFrame(0).GetMethod().Name + append;
        var file = Path.GetFullPath(Path.Combine(TestContext.CurrentContext.WorkDirectory, "PackedTextures", caller + ".png"));
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        using (var stream = File.Create(file))
            packer.PackedTexture.SaveAsPng(stream, packer.PackedTexture.Width, packer.PackedTexture.Height);
        TestContext.WriteLine($"Saving texture generated by {caller} to {file}");
        TestContext.AddTestAttachment(file);
    }

    private static void StubResult(TextureRegion region) {}

}
