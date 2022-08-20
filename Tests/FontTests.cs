using System;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Textures;
using NUnit.Framework;

namespace Tests {
    public class FontTests {

        private TestGame game;
        private GenericFont font;
        private TextFormatter formatter;

        [SetUp]
        public void SetUp() {
            this.game = TestGame.Create();
            this.font = this.game.UiSystem.Style.Font;
            this.formatter = this.game.UiSystem.TextFormatter;
        }

        [TearDown]
        public void TearDown() {
            this.game?.Dispose();
        }

        [Test]
        public void TestRegularSplit() {
            Assert.AreEqual(this.font.SplitStringSeparate(
                "Note that the default style does not contain any textures or font files and, as such, is quite bland. However, the default style is quite easy to override, as can be seen in the code for this demo.",
                65, 0.1F), new[] {"Note that the default style does ", "not contain any textures or font ", "files and, as such, is quite ", "bland. However, the default ", "style is quite easy to override, ", "as can be seen in the code for ", "this demo."});

            var formatted = this.formatter.Tokenize(this.font,
                "Select the demo you want to see below using your mouse, touch input, your keyboard or a controller. Check the demos' <c CornflowerBlue><l https://github.com/Ellpeck/MLEM/tree/main/Demos>source code</l></c> for more in-depth explanations of their functionality or the <c CornflowerBlue><l https://mlem.ellpeck.de/>website</l></c> for tutorials and API documentation.");
            formatted.Split(this.font, 90, 0.1F);
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

            Assert.AreEqual(this.font.SplitStringSeparate(
                "This_is_a_really_long_line_to_see_if_splitting_without_spaces_works_properly._I_also_want_to_see_if_it_works_across_multiple_lines_or_just_on_the_first_one. But after this, I want the text to continue normally before changing_back_to_being_really_long_oh_yes",
                65, 0.1F), expectedDisplay);

            var formatted = this.formatter.Tokenize(this.font,
                "This_is_a_really_long_line_to_see_if_<c Blue>splitting</c>_without_spaces_works_properly._I_also_want_to_see_if_it_works_across_multiple_<c Yellow>lines</c>_or_just_on_the_first_one. But after this, I want the <b>text</b> to continue normally before changing_back_<i>to</i>_being_really_long_oh_yes");
            formatted.Split(this.font, 65, 0.1F);
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
            var formatted = this.formatter.Tokenize(this.font,
                "This is a pretty long line with regular <c Blue>content</c> that will be split.\nNow this is a new line with additional regular <c Blue>content</c> that is forced into a new line.");
            formatted.Split(this.font, 65, 0.1F);
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
            this.formatter.Macros.Add(new Regex("<testmacro>"), (_, _, _) => "<test1>");
            this.formatter.Macros.Add(new Regex("<test1>"), (_, _, _) => "<test2>blue");
            this.formatter.Macros.Add(new Regex("<test2>"), (_, _, _) => "<c Blue>");
            const string strg = "This text uses a bunch of non-breaking~spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <testmacro> text</c>.";
            const string goal = "This text uses a bunch of non-breaking\u00A0spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <c Blue>blue text</c>.";
            Assert.AreEqual(this.formatter.ResolveMacros(strg), goal);

            // test recursive macros
            this.formatter.Macros.Add(new Regex("<rec1>"), (_, _, _) => "<rec2>");
            this.formatter.Macros.Add(new Regex("<rec2>"), (_, _, _) => "<rec1>");
            Assert.Throws<ArithmeticException>(() => this.formatter.ResolveMacros("Test <rec1> string"));
        }

        [Test]
        public void TestFormatting() {
            this.formatter.AddImage("Test", new TextureRegion((Texture2D) null, 0, 8, 24, 24));
            const string strg = "Lorem Ipsum <i Test> is simply dummy text of the <i Test> printing and typesetting <i Test> industry. Lorem Ipsum has been the industry's standard dummy text <i Test> ever since the <i Test> 1500s, when <i Test><i Test><i Test><i Test><i Test><i Test><i Test> an unknown printer took a galley of type and scrambled it to make a type specimen book.";
            var ret = this.formatter.Tokenize(this.font, strg);
            Assert.AreEqual(ret.Tokens.Length, 13);
            Assert.AreEqual(ret.DisplayString, "Lorem Ipsum \u2003 is simply dummy text of the \u2003 printing and typesetting \u2003 industry. Lorem Ipsum has been the industry's standard dummy text \u2003 ever since the \u2003 1500s, when \u2003\u2003\u2003\u2003\u2003\u2003\u2003 an unknown printer took a galley of type and scrambled it to make a type specimen book.");
            Assert.AreEqual(ret.AllCodes.Length, 12);
        }

        [Test]
        public void TestConsistency() {
            void CompareSizes(string s) {
                var spriteFont = ((GenericSpriteFont) this.font).Font;
                Assert.AreEqual(spriteFont.MeasureString(s), this.font.MeasureString(s));
            }

            CompareSizes("This is a very simple test string");
            CompareSizes("This\n is a very\nsimple test string");
            CompareSizes("\nThis is a very simple test string");
            CompareSizes("This is a very simple test string\n");
            CompareSizes("This is a very simple test string\n\n\n\n\n");
        }

        [Test]
        public void TestSpecialCharacters() {
            void CompareSizes(string s) {
                var spriteFont = ((GenericSpriteFont) this.font).Font;
                Assert.AreNotEqual(spriteFont.MeasureString(s), this.font.MeasureString(s));
            }

            CompareSizes($"This is a very simple{GenericFont.Nbsp}test string");
            CompareSizes($"This is a very simple{GenericFont.Emsp}test string");
            CompareSizes($"This is a very simple{GenericFont.Zwsp}test string");

            Assert.AreEqual(new Vector2(this.font.LineHeight, this.font.LineHeight), this.font.MeasureString(GenericFont.Emsp.ToCachedString()));
            Assert.AreEqual(new Vector2(0, this.font.LineHeight), this.font.MeasureString(GenericFont.Zwsp.ToCachedString()));
        }

    }
}
