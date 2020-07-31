using System.Text.RegularExpressions;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;
using MLEM.Formatting;
using MLEM.Formatting.Codes;
using MLEM.Textures;
using NUnit.Framework;
using Tests.Stub;

namespace Tests {
    public class FormattingTests {

        [Test]
        public void TestMacros() {
            var formatter = new TextFormatter();
            formatter.Macros.Add(new Regex("<testmacro>"), (f, m, r) => "<test1>");
            formatter.Macros.Add(new Regex("<test1>"), (f, m, r) => "<test2>blue");
            formatter.Macros.Add(new Regex("<test2>"), (f, m, r) => "<c Blue>");
            const string strg = "This text uses a bunch of non-breaking~spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <testmacro> text</c>.";
            const string goal = "This text uses a bunch of non-breaking\u00A0spaces to see if macros work. Additionally, it uses a macro that resolves into a bunch of other macros and then, at the end, into <c Blue>blue text</c>.";
            Assert.AreEqual(formatter.ResolveMacros(strg), goal);
        }

        [Test]
        public void TestFormatting() {
            var formatter = new TextFormatter();
            formatter.AddImage("Test", new TextureRegion(new Texture2D(new StubGraphics(), 1, 1), 0, 8, 24, 24));
            const string strg = "Lorem Ipsum <i Test> is simply dummy text of the <i Test> printing and typesetting <i Test> industry. Lorem Ipsum has been the industry's standard dummy text <i Test> ever since the <i Test> 1500s, when <i Test><i Test><i Test><i Test><i Test><i Test><i Test> an unknown printer took a galley of type and scrambled it to make a type specimen book.";
            var ret = formatter.Tokenize(new StubFont(), strg);
            Assert.AreEqual(ret.Tokens.Length, 13);
            Assert.AreEqual(ret.DisplayString, "Lorem Ipsum \uF8FF is simply dummy text of the \uF8FF printing and typesetting \uF8FF industry. Lorem Ipsum has been the industry's standard dummy text \uF8FF ever since the \uF8FF 1500s, when \uF8FF\uF8FF\uF8FF\uF8FF\uF8FF\uF8FF\uF8FF an unknown printer took a galley of type and scrambled it to make a type specimen book.");
            Assert.AreEqual(ret.AllCodes.Length, 12);
        }

    }
}