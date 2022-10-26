using System;
using System.Linq;
using System.Numerics;
using MLEM.Data;
using MLEM.Misc;
using NUnit.Framework;

namespace Tests;

public class EnumTests {

    [Test]
    public void TestRegularEnums() {
        Assert.AreEqual(
            new[] {TestEnum.One, TestEnum.Two, TestEnum.Eight, TestEnum.Sixteen, TestEnum.EightSixteen},
            EnumHelper.GetFlags(TestEnum.One | TestEnum.Sixteen | TestEnum.Eight | TestEnum.Two));

        Assert.AreEqual(
            new[] {TestEnum.One, TestEnum.Two, TestEnum.Eight, TestEnum.Sixteen},
            EnumHelper.GetUniqueFlags(TestEnum.One | TestEnum.Sixteen | TestEnum.Eight | TestEnum.Two));
    }

    [Test]
    public void TestDynamicEnums() {
        var flags = new TestDynamicEnum[100];
        for (var i = 0; i < flags.Length; i++)
            flags[i] = DynamicEnum.AddFlag<TestDynamicEnum>("Flag" + i);
        var combined = DynamicEnum.Add<TestDynamicEnum>("Combined", DynamicEnum.GetValue(DynamicEnum.Or(flags[7], flags[13])));

        Assert.AreEqual(DynamicEnum.GetValue(flags[7]), BigInteger.One << 7);
        Assert.AreEqual(DynamicEnum.GetEnumValue<TestDynamicEnum>(BigInteger.One << 75), flags[75]);

        Assert.AreEqual(DynamicEnum.GetValue(DynamicEnum.Or(flags[2], flags[17])), BigInteger.One << 2 | BigInteger.One << 17);
        Assert.AreEqual(DynamicEnum.GetValue(DynamicEnum.And(flags[2], flags[3])), BigInteger.Zero);
        Assert.AreEqual(DynamicEnum.And(DynamicEnum.Or(flags[24], flags[52]), DynamicEnum.Or(flags[52], flags[75])), flags[52]);
        Assert.AreEqual(DynamicEnum.Xor(DynamicEnum.Or(flags[85], flags[73]), flags[73]), flags[85]);
        Assert.AreEqual(DynamicEnum.Xor(DynamicEnum.Or(flags[85], DynamicEnum.Or(flags[73], flags[12])), flags[73]), DynamicEnum.Or(flags[85], flags[12]));
        Assert.AreEqual(DynamicEnum.GetValue(DynamicEnum.Neg(flags[74])), ~(BigInteger.One << 74));

        Assert.AreEqual(DynamicEnum.Or(flags[24], flags[52]).HasFlag(flags[24]), true);
        Assert.AreEqual(DynamicEnum.Or(flags[24], flags[52]).HasAnyFlag(flags[24]), true);
        Assert.AreEqual(DynamicEnum.Or(flags[24], flags[52]).HasFlag(DynamicEnum.Or(flags[24], flags[26])), false);
        Assert.AreEqual(DynamicEnum.Or(flags[24], flags[52]).HasAnyFlag(DynamicEnum.Or(flags[24], flags[26])), true);

        Assert.AreEqual(DynamicEnum.Parse<TestDynamicEnum>("Flag24"), flags[24]);
        Assert.AreEqual(DynamicEnum.Parse<TestDynamicEnum>("Flag24 | Flag43"), DynamicEnum.Or(flags[24], flags[43]));
        Assert.AreEqual(flags[24].ToString(), "Flag24");
        Assert.AreEqual(DynamicEnum.Or(flags[24], flags[43]).ToString(), "Flag24 | Flag43");

        Assert.True(DynamicEnum.IsDefined(flags[27]));
        Assert.True(DynamicEnum.IsDefined(combined));
        Assert.False(DynamicEnum.IsDefined(DynamicEnum.Or(flags[17], flags[49])));
        Assert.False(DynamicEnum.IsDefined(DynamicEnum.Or(combined, flags[49])));

        Assert.AreEqual(
            new[] {flags[0], flags[7], flags[13], combined},
            DynamicEnum.GetFlags(DynamicEnum.Or(DynamicEnum.Or(flags[0], flags[13]), flags[7])));

        Assert.AreEqual(
            new[] {flags[0], flags[7], flags[13]},
            DynamicEnum.GetUniqueFlags(DynamicEnum.Or(DynamicEnum.Or(flags[0], flags[13]), flags[7])));
    }

    [Flags]
    private enum TestEnum {

        One = 1,
        Two = 2,
        Eight = 8,
        Sixteen = 16,
        EightSixteen = TestEnum.Eight | TestEnum.Sixteen,
        ThirtyTwo = 32,
        OneTwentyEight = 128,
        OneTwentyEightTwoOne = TestEnum.OneTwentyEight | TestEnum.Two | TestEnum.One

    }

    private class TestDynamicEnum : DynamicEnum {

        public TestDynamicEnum(string name, BigInteger value) : base(name, value) {}

    }

}
