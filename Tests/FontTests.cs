using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests;

public class FontTests : GameTestFixture {

    private GenericFont Font => this.Game.UiSystem.Style.Font;
    private TextFormatter Formatter => this.Game.UiSystem.TextFormatter;

    [Test]
    public void TestRegularSplit() {
        Assert.AreEqual(this.Font.SplitStringSeparate(
            "Note that the default style does not contain any textures or font files and, as such, is quite bland. However, the default style is quite easy to override, as can be seen in the code for this demo.",
            65, 0.1F), new[] {"Note that the default style does ", "not contain any textures or font ", "files and, as such, is quite ", "bland. However, the default ", "style is quite easy to override, ", "as can be seen in the code for ", "this demo."});

        var formatted = this.Formatter.Tokenize(this.Font,
            "Select the demo you want to see below using your mouse, touch input, your keyboard or a controller. Check the demos' <c CornflowerBlue><l https://github.com/Ellpeck/MLEM/tree/main/Demos>source code</l></c> for more in-depth explanations of their functionality or the <c CornflowerBlue><l https://mlem.ellpeck.de/>website</l></c> for tutorials and API documentation.");
        formatted.Split(this.Font, 90, 0.1F);
        Assert.AreEqual(formatted.DisplayString, "Select the demo you want to see below using \nyour mouse, touch input, your keyboard or a \ncontroller. Check the demos' source code for \nmore in-depth explanations of their \nfunctionality or the website for tutorials and \nAPI documentation.");

        var tokens = new[] {
            "Select the demo you want to see below using \nyour mouse, touch input, your keyboard or a \ncontroller. Check the demos' ",
            string.Empty,
            "source code",
            string.Empty,
            " for \nmore in-depth explanations of their \nfunctionality or the ",
            string.Empty,
            "website",
            string.Empty,
            " for tutorials and \nAPI documentation."
        };
        for (var i = 0; i < tokens.Length; i++)
            Assert.AreEqual(formatted.Tokens[i].DisplayString, tokens[i]);
    }

    [Test]
    public void TestLongLineSplit() {
        var expectedDisplay = new[] {"This_is_a_really_long_line_to_s", "ee_if_splitting_without_spaces_", "works_properly._I_also_want_to_", "see_if_it_works_across_multiple", "_lines_or_just_on_the_first_one. ", "But after this, I want the text to ", "continue normally before ", "changing_back_to_being_really_", "long_oh_yes"};

        Assert.AreEqual(this.Font.SplitStringSeparate(
            "This_is_a_really_long_line_to_see_if_splitting_without_spaces_works_properly._I_also_want_to_see_if_it_works_across_multiple_lines_or_just_on_the_first_one. But after this, I want the text to continue normally before changing_back_to_being_really_long_oh_yes",
            65, 0.1F), expectedDisplay);

        var formatted = this.Formatter.Tokenize(this.Font,
            "This_is_a_really_long_line_to_see_if_<c Blue>splitting</c>_without_spaces_works_properly._I_also_want_to_see_if_it_works_across_multiple_<c Yellow>lines</c>_or_just_on_the_first_one. But after this, I want the <b>text</b> to continue normally before changing_back_<i>to</i>_being_really_long_oh_yes");
        formatted.Split(this.Font, 65, 0.1F);
        Assert.AreEqual(formatted.DisplayString, string.Join('\n', expectedDisplay));

        var tokens = new[] {
            "This_is_a_really_long_line_to_s\nee_if_",
            "splitting",
            "_without_spaces_\nworks_properly._I_also_want_to_\nsee_if_it_works_across_multiple\n_",
            "lines",
            "_or_just_on_the_first_one. \nBut after this, I want the ",
            "text",
            " to \ncontinue normally before \nchanging_back_",
            "to",
            "_being_really_\nlong_oh_yes"
        };
        for (var i = 0; i < tokens.Length; i++)
            Assert.AreEqual(formatted.Tokens[i].DisplayString, tokens[i]);
    }

    [Test]
    public void TestNewlineSplit() {
        var formatted = this.Formatter.Tokenize(this.Font,
            "This is a pretty long line with regular <c Blue>content</c> that will be split.\nNow this is a new line with additional regular <c Blue>content</c> that is forced into a new line.");
        formatted.Split(this.Font, 65, 0.1F);
        Assert.AreEqual(formatted.DisplayString, "This is a pretty long line with \nregular content that will be \nsplit.\nNow this is a new line with \nadditional regular content that \nis forced into a new line.");

        var tokens = new[] {
            "This is a pretty long line with \nregular ",
            "content",
            " that will be \nsplit.\nNow this is a new line with \nadditional regular ",
            "content",
            " that \nis forced into a new line."
        };
        for (var i = 0; i < tokens.Length; i++)
            Assert.AreEqual(formatted.Tokens[i].DisplayString, tokens[i]);
    }

    [Test]
    public void TestMacros() {
        this.Formatter.Macros.Add(new Regex("<testmacro>"), (_, _, _) => "<test1>");
        this.Formatter.Macros.Add(new Regex("<test1>"), (_, _, _) => "<test2>blue");
        this.Formatter.Macros.Add(new Regex("<test2>"), (_, _, _) => "<c Blue>");
        const string strg = "This text uses a bunch of non-breaking~spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <testmacro> text</c>.";
        const string goal = "This text uses a bunch of non-breaking\u00A0spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <c Blue>blue text</c>.";
        Assert.AreEqual(this.Formatter.ResolveMacros(strg), goal);

        // test recursive macros
        this.Formatter.Macros.Add(new Regex("<rec1>"), (_, _, _) => "<rec2>");
        this.Formatter.Macros.Add(new Regex("<rec2>"), (_, _, _) => "<rec1>");
        Assert.Throws<ArithmeticException>(() => this.Formatter.ResolveMacros("Test <rec1> string"));
    }

    [Test]
    public void TestFormatting() {
        this.Formatter.AddImage("Test", new TextureRegion((Texture2D) null, 0, 8, 24, 24));
        const string strg = "<b>Lorem</b> Ipsum <i Test> is simply dummy text of the <i Test> printing and typesetting <i Test> industry. Lorem Ipsum has been the industry's standard dummy text <i Test> ever since the <i Test> 1500s, when <i Test><i Test><i Test><i Test><i Test><i Test><i Test> an unknown printer took a galley of type and scrambled it to make a type specimen <b></b>book.";
        var ret = this.Formatter.Tokenize(this.Font, strg);
        Assert.AreEqual(ret.Tokens.Length, 16);
        Assert.AreEqual(ret.DisplayString, "Lorem Ipsum  is simply dummy text of the  printing and typesetting  industry. Lorem Ipsum has been the industry's standard dummy text  ever since the  1500s, when  an unknown printer took a galley of type and scrambled it to make a type specimen book.");
        Assert.AreEqual(ret.AllCodes.Length, 16);
    }

    [Test]
    public void TestStripping() {
        var stripped = this.Formatter.StripAllFormatting("This is a <b>test string</b></b><b></b> <i>with a lot of</i>content</b><i></b></i> and an <k> invalid code</b> as well<ü>.");
        Assert.AreEqual("This is a test string with a lot ofcontent and an <k> invalid code as well<ü>.", stripped);
    }

    [Test]
    public void TestConsistency() {
        void CompareSizes(string s) {
            var spriteFont = ((GenericSpriteFont) this.Font).Font.MeasureString(s);
            var genericFont = this.Font.MeasureString(s);
            Assert.AreEqual(spriteFont.X, genericFont.X);
            Assert.AreEqual(spriteFont.Y, genericFont.Y
#if KNI
                // we leave a bit of room for the Y value since KNI's sprite fonts sometimes increase line height for specific characters, which generic fonts don't
                , spriteFont.Y / 10
#endif
            );
        }

        CompareSizes("This is a very simple test string");
        CompareSizes("This\n is a very\nsimple test string");
        CompareSizes("\nThis is a very simple test string");
        CompareSizes("This is a very simple test string\n");
        CompareSizes("This is a very simple test string\n\n\n\n\n");
        CompareSizes("This    is a very    simple   test   string");
        CompareSizes("This is a very    simple   \n   test string");
    }

    [Test]
    public void TestSpecialCharacters() {
        void CompareSizes(string s) {
            var spriteFont = ((GenericSpriteFont) this.Font).Font;
            Assert.AreNotEqual(spriteFont.MeasureString(s), this.Font.MeasureString(s));
        }

        CompareSizes($"This is a very simple{GenericFont.Nbsp}test string");
        CompareSizes($"This is a very simple{GenericFont.Emsp}test string");
        CompareSizes($"This is a very simple{GenericFont.Zwsp}test string");

        Assert.AreEqual(new Vector2(this.Font.LineHeight, this.Font.LineHeight), this.Font.MeasureString(GenericFont.Emsp.ToString()));
        Assert.AreEqual(new Vector2(0, this.Font.LineHeight), this.Font.MeasureString(GenericFont.Zwsp.ToString()));
    }

    // empty strings cause issues when tokenizing with alignment set
    [Test]
    public void TestIssue42([Values(TextAlignment.Left, TextAlignment.Center, TextAlignment.Right)] TextAlignment alignment) {
        var result = this.Formatter.Tokenize(this.Font, "", alignment);
        Assert.AreEqual(0, result.AllCodes.Length);
        Assert.AreEqual("", result.RawString);
        Assert.AreEqual("", result.String);
    }

}
