using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MLEM.Ui.Elements;

namespace MLEM.Ui.Parsers {
    /// <summary>
    /// A class for parsing Markdown strings into a set of MLEM.Ui elements with styling for each individual <see cref="UiParser.ElementType"/>.
    /// To parse, use <see cref="UiParser.Parse"/> or <see cref="UiParser.ParseInto"/>. To style the parsed output, use <see cref="UiParser.Style{T}"/> before parsing.
    /// </summary>
    /// <remarks>
    /// Note that this parser is rather rudimentary and doesn't deal well with very complex Markdown documents. Missing features are as follows:
    /// <list type="bullet">
    /// <item><description>Lines that end without a double space are still converted to distinct lines rather than being merged with the next line</description></item>
    /// <item><description>Better list handling, including nested lists</description></item>
    /// <item><description>Horizontal rules</description></item>
    /// <item><description>Tables</description></item>
    /// </list>
    /// </remarks>
    public class UiMarkdownParser : UiParser {

        /// <summary>
        /// Creates a new UI markdown parser and optionally initializes some default style settings.
        /// </summary>
        /// <param name="applyDefaultStyling">Whether default style settings should be applied.</param>
        public UiMarkdownParser(bool applyDefaultStyling = true) : base(applyDefaultStyling) {}

        /// <inheritdoc />
        protected override IEnumerable<(ElementType, Element)> ParseUnstyled(string raw) {
            var inCodeBlock = false;
            foreach (var line in raw.Split('\n')) {
                // code blocks
                if (line.Trim().StartsWith("```")) {
                    inCodeBlock = !inCodeBlock;
                    continue;
                }
                // code block content
                if (inCodeBlock) {
                    yield return (ElementType.CodeBlock, new Paragraph(Anchor.AutoLeft, 1, $"<f {this.CodeFont}>{line}</f>"));
                    continue;
                }

                // quotes
                if (line.StartsWith(">")) {
                    yield return (ElementType.Blockquote, new Paragraph(Anchor.AutoLeft, 1, this.ParseParagraph(line.Substring(1).Trim())));
                    continue;
                }

                // vertical space (empty lines)
                if (line.Trim().Length <= 0) {
                    yield return (ElementType.VerticalSpace, new VerticalSpace(0));
                    continue;
                }

                // images
                var imageMatch = Regex.Match(line, @"!\[\]\(([^)]+)\)");
                if (imageMatch.Success) {
                    yield return (ElementType.Image, this.ParseImage(imageMatch.Groups[1].Value));
                    continue;
                }

                // headers
                var parsedHeader = false;
                for (var h = 6; h >= 1; h--) {
                    if (line.StartsWith(new string('#', h))) {
                        var type = UiParser.ElementTypes[Array.IndexOf(UiParser.ElementTypes, ElementType.Header1) + h - 1];
                        yield return (type, new Paragraph(Anchor.AutoLeft, 1, this.ParseParagraph(line.Substring(h).Trim())));
                        parsedHeader = true;
                        break;
                    }
                }
                if (parsedHeader)
                    continue;

                // parse everything else as a paragraph (with formatting)
                yield return (ElementType.Paragraph, new Paragraph(Anchor.AutoLeft, 1, this.ParseParagraph(line)));
            }
        }

        private string ParseParagraph(string par) {
            // replace links
            par = Regex.Replace(par, @"<([^>]+)>", "<l $1>$1</l>");
            par = Regex.Replace(par, @"\[([^\]]+)\]\(([^)]+)\)", "<l $2>$1</l>");
            // replace formatting
            par = Regex.Replace(par, @"\*\*([^\*]+)\*\*", "<b>$1</b>");
            par = Regex.Replace(par, @"__([^_]+)__", "<b>$1</b>");
            par = Regex.Replace(par, @"\*([^\*]+)\*", "<i>$1</i>");
            par = Regex.Replace(par, @"_([^_]+)_", "<i>$1</i>");
            par = Regex.Replace(par, @"~~([^~]+)~~", "<st>$1</st>");
            // replace inline code with custom code font
            par = Regex.Replace(par, @"`([^`]+)`", $"<f {this.CodeFont}>$1</f>");
            return par;
        }

    }
}
