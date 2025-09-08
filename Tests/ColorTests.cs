using Microsoft.Xna.Framework;
using MLEM.Graphics;
using NUnit.Framework;

namespace Tests;

public class ColorTests {

    [Test]
    public void TestConversionIdentity() {
        foreach (var c in typeof(Color).GetProperties()) {
            if (!c.GetGetMethod().IsStatic)
                continue;
            var value = (Color) c.GetValue(null);
            if (value == Color.Transparent)
                continue;
            ColorTests.AssertColorsEqual(value, ColorHelper.FromHsv(value.ToHsv()));
            ColorTests.AssertColorsEqual(value, ColorHelper.FromHsl(value.ToHsl()));
            ColorTests.AssertColorsEqual(value, ColorHelper.FromHexString(value.ToHexStringRgb()));
            ColorTests.AssertColorsEqual(value, ColorHelper.FromHexString(value.ToHexStringRgba()));
            ColorTests.AssertColorsEqual(value, value.Invert().Invert());
        }
    }

    [Test]
    public void TestConversionSample() {
        ColorTests.CompareColors(new Color(0, 0, 0), (0, 0, 0), (0, 0, 0));
        ColorTests.CompareColors(new Color(255, 0, 0), (0, 1, 1), (0, 1, 0.5F));
        ColorTests.CompareColors(new Color(0, 255, 0), (120, 1, 1), (120, 1, 0.5F));
        ColorTests.CompareColors(new Color(0, 0, 255), (240, 1, 1), (240, 1, 0.5F));

        ColorTests.CompareColors(new Color(50, 168, 82), (136, 0.7F, 0.66F), (136, 0.54F, 0.43F));
        ColorTests.CompareColors(new Color(252, 186, 3), (44, 0.99F, 0.99F), (44, 0.98F, 0.5F));
        ColorTests.CompareColors(new Color(116, 86, 122), (290, 0.3F, 0.48F), (290, 0.17F, 0.41F));
    }

    private static void CompareColors(Color color, (float H, float S, float V) hsv, (float H, float S, float L) hsl) {
        // hues > 1 are interpreted as degrees
        hsv.H = hsv.H > 1 ? hsv.H / 360 : hsv.H;
        hsl.H = hsl.H > 1 ? hsl.H / 360 : hsl.H;
        ColorTests.AssertColorsEqual(hsv, color.ToHsv());
        ColorTests.AssertColorsEqual(hsl, color.ToHsl());
    }

    private static void AssertColorsEqual(Color expected, Color actual, int tolerance = 1) {
        Assert.AreEqual(expected.R, actual.R, tolerance);
        Assert.AreEqual(expected.G, actual.G, tolerance);
        Assert.AreEqual(expected.B, actual.B, tolerance);
        Assert.AreEqual(expected.A, actual.A, tolerance);
    }

    private static void AssertColorsEqual((float, float, float) expected, (float, float, float) actual, float tolerance = 0.01F) {
        Assert.AreEqual(expected.Item1, actual.Item1, tolerance);
        Assert.AreEqual(expected.Item2, actual.Item2, tolerance);
        Assert.AreEqual(expected.Item3, actual.Item3, tolerance);
    }

}
