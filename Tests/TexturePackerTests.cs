using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Data;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests;

public class TexturePackerTests : GameTestFixture {

    private Texture2D testTexture;
    private Texture2D disposedTestTexture;

    [SetUp]
    public void SetUp() {
        this.testTexture = new Texture2D(this.Game.GraphicsDevice, 2048, 2048);
        this.disposedTestTexture = new Texture2D(this.Game.GraphicsDevice, 16, 16);
    }

    [TearDown]
    public void TearDown() {
        this.testTexture?.Dispose();
        this.disposedTestTexture?.Dispose();
    }

    [Test]
    public void TestPacking() {
        using var packer = new RuntimeTexturePacker();
        for (var i = 0; i < 5; i++) {
            var width = 16 * (i + 1);
            packer.Add(new TextureRegion(this.testTexture, 0, 0, width, 64), r => {
                Assert.AreEqual(r.Width, width);
                Assert.AreEqual(r.Height, 64);
            });
        }
        packer.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(packer.PackedTexture.Width, 16 + 32 + 48 + 64 + 80);
        Assert.AreEqual(packer.PackedTexture.Height, 64);
    }

    [Test]
    public void TestOverlap() {
        var packed = new List<TextureRegion>();
        using (var packer = new RuntimeTexturePacker(8192)) {
            for (var i = 1; i <= 1000; i++)
                packer.Add(new TextureRegion(this.testTexture, 0, 0, i % 239, i % 673), packed.Add);
            packer.Pack(this.Game.GraphicsDevice);
        }

        foreach (var r1 in packed) {
            foreach (var r2 in packed) {
                if (r1 == r2)
                    continue;
                Assert.False(r1.Area.Intersects(r2.Area));
            }
        }
    }

    [Test]
    public void TestDisposal() {
        using var packer = new RuntimeTexturePacker(128, disposeTextures: true);
        packer.Add(new TextureRegion(this.disposedTestTexture), TexturePackerTests.StubResult);
        packer.Add(new TextureRegion(this.disposedTestTexture, 0, 0, 8, 8), TexturePackerTests.StubResult);
        packer.Pack(this.Game.GraphicsDevice);
        Assert.True(this.disposedTestTexture.IsDisposed);
        Assert.False(packer.PackedTexture.IsDisposed);
    }

    [Test]
    public void TestBounds() {
        // test forced max width
        using var packer = new RuntimeTexturePacker(128);
        Assert.Throws<InvalidOperationException>(() => {
            packer.Add(new TextureRegion(this.testTexture, 0, 0, 256, 128), TexturePackerTests.StubResult);
        });

        // test auto-expanding width
        using var packer2 = new RuntimeTexturePacker(128, true);
        Assert.DoesNotThrow(() => {
            packer2.Add(new TextureRegion(this.testTexture, 0, 0, 256, 128), TexturePackerTests.StubResult);
        });
        packer2.Pack(this.Game.GraphicsDevice);

        // test power of two forcing
        using var packer3 = new RuntimeTexturePacker(128, forcePowerOfTwo: true);
        packer3.Add(new TextureRegion(this.testTexture, 0, 0, 37, 170), TexturePackerTests.StubResult);
        packer3.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(64, packer3.PackedTexture.Width);
        Assert.AreEqual(256, packer3.PackedTexture.Height);

        // test square forcing
        using var packer4 = new RuntimeTexturePacker(128, forceSquare: true);
        packer4.Add(new TextureRegion(this.testTexture, 0, 0, 37, 170), TexturePackerTests.StubResult);
        packer4.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(170, packer4.PackedTexture.Width);
        Assert.AreEqual(170, packer4.PackedTexture.Height);
    }

    [Test]
    public void TestPackMultipleTimes() {
        using var packer = new RuntimeTexturePacker(1024);

        // pack the first time
        var results = 0;
        for (var i = 0; i < 10; i++)
            packer.Add(new TextureRegion(this.testTexture, 0, 0, 64, 64), _ => results++);
        packer.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(10, results);

        // pack without resizing
        packer.Add(new TextureRegion(this.testTexture, 0, 0, 0, 0), _ => results++);
        packer.Pack(this.Game.GraphicsDevice);
        Assert.AreEqual(11, results);

        // pack and force a resize
        packer.Add(new TextureRegion(this.testTexture, 0, 0, 64, 64), _ => results++);
        packer.Pack(this.Game.GraphicsDevice);
        // all callbacks are called again, so we add 11 again, as well as the callback we just added
        Assert.AreEqual(2 * 11 + 1, results);
    }

    [Test]
    public void TestPackTimes() {
        for (var total = 1; total <= 10001; total += 1000) {
            using var sameSizePacker = new RuntimeTexturePacker();
            using var diffSizePacker = new RuntimeTexturePacker();
            for (var i = 0; i < total; i++) {
                sameSizePacker.Add(new TextureRegion(this.testTexture, 0, 0, 10, 10), TexturePackerTests.StubResult);
                diffSizePacker.Add(new TextureRegion(this.testTexture, 0, 0, 10 + i % 129, 10 * (i % 5 + 1)), TexturePackerTests.StubResult);
            }
            sameSizePacker.Pack(this.Game.GraphicsDevice);
            diffSizePacker.Pack(this.Game.GraphicsDevice);

            TestContext.WriteLine($"""
                {total} regions,
                same-size {sameSizePacker.LastCalculationTime.TotalMilliseconds} calc, {sameSizePacker.LastPackTime.TotalMilliseconds} pack, {sameSizePacker.LastTotalTime.TotalMilliseconds} total,
                diff-size {diffSizePacker.LastCalculationTime.TotalMilliseconds} calc, {diffSizePacker.LastPackTime.TotalMilliseconds} pack, {diffSizePacker.LastTotalTime.TotalMilliseconds} total
                """);
        }
    }

    private static void StubResult(TextureRegion region) {}

}
