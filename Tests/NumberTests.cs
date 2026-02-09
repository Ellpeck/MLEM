using Microsoft.Xna.Framework;
using MLEM.Extended.Maths;
using NUnit.Framework;
using RectangleF = MLEM.Maths.RectangleF;
using MLEM.Maths;
using MonoGame.Extended;
namespace MLEM.Tests;

public class NumberTests {

    [Test]
    public void TestRounding() {
        Assert.AreEqual(1.25F.Floor(), 1);
        Assert.AreEqual(-1.25F.Floor(), -1);

        Assert.AreEqual(1.25F.Ceil(), 2);
        Assert.AreEqual(-1.25F.Ceil(), -2);

        Assert.AreEqual(new Vector2(5, 2.5F).FloorCopy(), new Vector2(5, 2));
        Assert.AreEqual(new Vector2(5, 2.5F).CeilCopy(), new Vector2(5, 3));
        Assert.AreEqual(new Vector2(5.25F, 2).FloorCopy(), new Vector2(5, 2));
        Assert.AreEqual(new Vector2(5.25F, 2).CeilCopy(), new Vector2(6, 2));
    }

    [Test]
    public void TestEquals() {
        Assert.IsTrue(0.25F.Equals(0.26F, 0.01F));
        Assert.IsFalse(0.25F.Equals(0.26F, 0.009F));

        Assert.IsTrue(new Vector2(0.25F, 0).Equals(new Vector2(0.3F, 0), 0.1F));
        Assert.IsFalse(new Vector2(0.25F, 0.5F).Equals(new Vector2(0.3F, 0.25F), 0.1F));

        Assert.IsTrue(new Vector3(0.25F, 0, 3.5F).Equals(new Vector3(0.3F, 0, 3.45F), 0.1F));
        Assert.IsFalse(new Vector3(0.25F, 0.5F, 0).Equals(new Vector3(0.3F, 0.25F, 0), 0.1F));

        Assert.IsTrue(new Vector4(0.25F, 0, 3.5F, 0.75F).Equals(new Vector4(0.3F, 0, 3.45F, 0.7F), 0.1F));
        Assert.IsFalse(new Vector4(0.25F, 0.5F, 0, 1).Equals(new Vector4(0.3F, 0.25F, 0, 0.9F), 0.1F));
    }

    [Test]
    public void TestPointExtensions() {
        Assert.AreEqual(new Point(4, 3).Multiply(3), new Point(12, 9));
        Assert.AreEqual(new Point(17, 12).Divide(3), new Point(5, 4));
        Assert.AreEqual(new Point(4, 12).Transform(Matrix.CreateTranslation(2, 0, 0) * Matrix.CreateScale(2, 2, 2)), new Point(12, 24));
    }

    [Test]
    public void TestMatrixOps([Range(0.5F, 2, 0.5F)] float scale, [Range(-0.5F, 0.5F, 1)] float rotationX, [Range(-0.5F, 0.5F, 1)] float rotationY, [Range(-0.5F, 0.5F, 1)] float rotationZ) {
        var rotation = Matrix.CreateRotationX(rotationX) * Matrix.CreateRotationY(rotationY) * Matrix.CreateRotationZ(rotationZ);
        var matrix = rotation * Matrix.CreateScale(scale, scale, scale);
        Assert.IsTrue(matrix.Scale().Equals(new Vector3(scale), 0.001F), $"{matrix.Scale()} does not equal {new Vector2(scale)}");
        Assert.IsTrue(matrix.Rotation().Equals(Quaternion.CreateFromRotationMatrix(rotation), 0.001F), $"{matrix.Rotation()} does not equal {Quaternion.CreateFromRotationMatrix(rotation)}");
        Assert.IsTrue(matrix.RotationVector().Equals(new Vector3(rotationX, rotationY, rotationZ), 0.001F), $"{matrix.RotationVector()} does not equal {new Vector3(rotationX, rotationY, rotationZ)}");

        // check against decomposed results
        matrix.Decompose(out var sc, out var rot, out _);
        Assert.AreEqual(matrix.Rotation(), rot);
        Assert.AreEqual(matrix.Scale(), sc);
    }

    [Test]
    public void TestPenetrate() {
        new RectangleF(2, 2, 4, 4).Penetrate(new RectangleF(3, 2, 4, 4), out var normal, out var penetration);
        Assert.AreEqual(normal, new Vector2(1, 0));
        Assert.AreEqual(penetration, 3);

        new RectangleF(-10, 10, 5, 5).Penetrate(new RectangleF(25, 25, 10, 10), out normal, out penetration);
        Assert.AreEqual(normal, Vector2.Zero);
        Assert.AreEqual(penetration, 0);
    }

    [Test]
    public void TestRangePercentage() {
        Assert.AreEqual(0.5F, new Range<int>(1, 7).GetPercentage(4));
        Assert.AreEqual(1, new Range<int>(1, 7).GetPercentage(7));
        Assert.AreEqual(0, new Range<int>(1, 7).GetPercentage(1));
        Assert.AreEqual(4, new Range<int>(1, 7).FromPercentage(0.5F));
        Assert.AreEqual(7, new Range<int>(1, 7).FromPercentage(1));
        Assert.AreEqual(1, new Range<int>(1, 7).FromPercentage(0));

        Assert.AreEqual(0.5F, new Range<float>(-1, 1).GetPercentage(0));
        Assert.AreEqual(0.25F, new Range<float>(-1, 1).GetPercentage(-0.5F));
        Assert.AreEqual(0.75F, new Range<float>(-1, 1).GetPercentage(0.5F));
        Assert.AreEqual(0, new Range<float>(-1, 1).FromPercentage(0.5F));
        Assert.AreEqual(-0.5F, new Range<float>(-1, 1).FromPercentage(0.25F));
        Assert.AreEqual(0.5F, new Range<float>(-1, 1).FromPercentage(0.75F));

        Assert.AreEqual(1.5F, new Range<float>(8, 10).GetPercentage(11));
        Assert.AreEqual(-0.5F, new Range<float>(8, 10).GetPercentage(7));
        Assert.AreEqual(11, new Range<float>(8, 10).FromPercentage(1.5F));
        Assert.AreEqual(7, new Range<float>(8, 10).FromPercentage(-0.5F));
    }

}
