using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MLEM.Maths;
using NUnit.Framework;

namespace Tests;

public class SingleRandomTests {

    [Test]
    public void TestEquality() {
        for (var i = 0; i < 1000000; i++) {
            Assert.AreEqual(SingleRandom.Single(i), SingleRandom.Single(new SeedSource().Add(i)));
            Assert.AreEqual(SingleRandom.Int(i), SingleRandom.Int(new SeedSource().Add(i)));

            // test if all methods that accept mins and max are identical
            Assert.AreEqual(SingleRandom.Int(i), SingleRandom.Int(int.MaxValue, i));
            Assert.AreEqual(SingleRandom.Int(i), SingleRandom.Int(0, int.MaxValue, i));
            Assert.AreEqual(SingleRandom.Single(i), SingleRandom.Single(1F, i));
            Assert.AreEqual(SingleRandom.Single(i), SingleRandom.Single(0F, 1F, i));
        }
    }

    [Test]
    public void TestBounds() {
        for (var i = 0; i < 1000000; i++) {
            Assert.That(SingleRandom.Single(i), Is.LessThan(1).And.GreaterThanOrEqualTo(0));
            Assert.That(SingleRandom.Single(127F, i), Is.LessThan(127).And.GreaterThanOrEqualTo(0));
            Assert.That(SingleRandom.Single(12920F, 1203919023F, i), Is.LessThan(1203919023).And.GreaterThanOrEqualTo(12920));

            Assert.That(SingleRandom.Int(i), Is.LessThan(int.MaxValue).And.GreaterThanOrEqualTo(0));
            Assert.That(SingleRandom.Int(17, i), Is.LessThan(17).And.GreaterThanOrEqualTo(0));
            Assert.That(SingleRandom.Int(19283, 832498394, i), Is.LessThan(832498394).And.GreaterThanOrEqualTo(19283));
        }
    }

    [Test]
    public void TestAverages() {
        var ints = new List<int>();
        var flts = new List<float>();
        for (var i = 0; i < 1000000; i++) {
            ints.Add(SingleRandom.Int(i));
            flts.Add(SingleRandom.Single(i));
        }
        Assert.AreEqual(0.5, ints.Average() / int.MaxValue, 0.001);
        Assert.AreEqual(0.5, flts.Average(), 0.001);
    }

    [Test]
    public void TestExpectedValues() {
        var distributed = File.ReadAllLines("Content/DistributedSingleRandoms.txt");
        for (var i = 0; i < 10000; i++)
            Assert.AreEqual(SingleRandom.Single(i * 10000).ToString(CultureInfo.InvariantCulture), distributed[i]);

        var consecutive = File.ReadAllLines("Content/ConsecutiveSingleRandoms.txt");
        for (var i = 0; i < 10000; i++)
            Assert.AreEqual(SingleRandom.Single(i).ToString(CultureInfo.InvariantCulture), consecutive[i]);
    }

}
