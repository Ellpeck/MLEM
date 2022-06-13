using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Formatting;
using MLEM.Misc;
using MLEM.Textures;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

namespace MLEM.Ui.Parsers {
    /// <summary>
    /// A class for parsing Markdown strings into a set of MLEM.Ui elements with styling for each individual <see cref="ElementType"/>.
    /// To parse, use <see cref="Parse"/> or <see cref="ParseInto"/>. To style the parsed output, use <see cref="Style{T}"/> before parsing.
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
    public class UiMarkdownParser {

        private static readonly ElementType[] ElementTypes = EnumHelper.GetValues<ElementType>().ToArray();

        /// <summary>
        /// The base path for markdown images, which is prepended to the image link.
        /// </summary>
        public string ImageBasePath;
        /// <summary>
        /// An action that is invoked when an image fails to load while parsing.
        /// This action receives the expected location of the image, as well as the <see cref="Exception"/> that occured.
        /// </summary>
        public Action<string, Exception> ImageExceptionHandler;
        /// <summary>
        /// The graphics device that should be used when loading images and other graphics-dependent content.
        /// </summary>
        public GraphicsDevice GraphicsDevice;
        /// <summary>
        /// The name of the font used for inline code as well as code blocks.
        /// This only has an effect if a font with this name is added to the used <see cref="UiStyle"/>'s <see cref="UiStyle.AdditionalFonts"/>.
        /// This defaults to "Monospaced" if default styling is applied in <see cref="UiMarkdownParser(bool)"/>.
        /// </summary>
        public string CodeFont;

        private readonly Dictionary<ElementType, Action<Element>> elementStyles = new Dictionary<ElementType, Action<Element>>();

        /// <summary>
        /// Creates a new UI markdown parser and optionally initializes some default style settings.
        /// </summary>
        /// <param name="applyDefaultStyling">Whether default style settings should be applied.</param>
        public UiMarkdownParser(bool applyDefaultStyling = true) {
            if (applyDefaultStyling) {
                this.CodeFont = "Monospaced";
                this.Style<VerticalSpace>(ElementType.VerticalSpace, v => v.Size = new Vector2(1, 5));
                for (var i = 0; i < 6; i++) {
                    var level = i;
                    this.Style<Paragraph>(ElementTypes[Array.IndexOf(ElementTypes, ElementType.Header1) + i], p => {
                        p.Alignment = TextAlignment.Center;
                        p.TextScaleMultiplier = 2 - level * 0.15F;
                    });
                }
            }
        }

        /// <summary>
        /// Parses the given markdown string into a set of elements (using <see cref="Parse"/>) and adds them as children to the givem <paramref name="element"/>.
        /// During this process, the element stylings specified using <see cref="Style{T}"/> are also applied.
        /// </summary>
        /// <param name="markdown">The markdown to parse.</param>
        /// <param name="element">The element to add the parsed elements to.</param>
        /// <returns>The <paramref name="element"/>, for chaining.</returns>
        public Element ParseInto(string markdown, Element element) {
            foreach (var el in this.Parse(markdown))
                element.AddChild(el);
            return element;
        }

        /// <summary>
        /// Parses the given markdown string into a set of elements and returns them.
        /// During this process, the element stylings specified using <see cref="Style{T}"/> are also applied.
        /// </summary>
        /// <param name="markdown">The markdown to parse.</param>
        /// <returns>The parsed elements.</returns>
        public IEnumerable<Element> Parse(string markdown) {
            foreach (var (type, element) in this.ParseUnstyled(markdown)) {
                if (this.elementStyles.TryGetValue(type, out var style))
                    style.Invoke(element);
                yield return element;
            }
        }

        /// <summary>
        /// Specifies an action to be invoked when a new element with the given <see cref="ElementType"/> is parsed in <see cref="Parse"/> or <see cref="ParseInto"/>.
        /// These actions can be used to modify the style properties of the created elements.
        /// </summary>
        /// <param name="types">The element types that should be styled. Can be a combined flag.</param>
        /// <param name="style">The action that styles the elements with the given element type.</param>
        /// <param name="add">Whether the <paramref name="style"/> function should be added to the existing style settings, or replace them.</param>
        /// <typeparam name="T">The type of elements that the given <see cref="ElementType"/> flags are expected to be.</typeparam>
        /// <returns>This parser, for chaining.</returns>
        public UiMarkdownParser Style<T>(ElementType types, Action<T> style, bool add = false) where T : Element {
            foreach (var type in ElementTypes) {
                if (types.HasFlag(type)) {
                    if (add && this.elementStyles.ContainsKey(type)) {
                        this.elementStyles[type] += Action;
                    } else {
                        this.elementStyles[type] = Action;
                    }
                }
            }
            return this;

            void Action(Element e) {
                style.Invoke(e as T ?? throw new ArgumentException($"Expected {typeof(T)} for style action but got {e.GetType()}"));
            }
        }

        private IEnumerable<(ElementType, Element)> ParseUnstyled(string markdown) {
            var inCodeBlock = false;
            foreach (var line in markdown.Split('\n')) {
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
                    yield return (ElementType.Blockquote, new Paragraph(Anchor.AutoLeft, 1, line.Substring(1).Trim()));
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
                    if (this.GraphicsDevice == null)
                        throw new NullReferenceException("A markdown parser requires a GraphicsDevice for parsing images");

                    TextureRegion image = null;
                    LoadImageAsync();
                    yield return (ElementType.Image, new Image(Anchor.AutoLeft, new Vector2(1, -1), _ => image) {
                        OnDisposed = e => image?.Texture.Dispose()
                    });

                    async void LoadImageAsync() {
                        var loc = imageMatch.Groups[1].Value;
                        // only apply the base path for relative files
                        if (this.ImageBasePath != null && !loc.StartsWith("http") && !Path.IsPathRooted(loc))
                            loc = $"{this.ImageBasePath}/{loc}";
                        try {
                            Texture2D tex;
                            if (loc.StartsWith("http")) {
                                using (var client = new HttpClient()) {
                                    using (var src = await client.GetStreamAsync(loc))
                                        tex = Texture2D.FromStream(this.GraphicsDevice, src);
                                }
                            } else {
                                using (var stream = Path.IsPathRooted(loc) ? File.OpenRead(loc) : TitleContainer.OpenStream(loc))
                                    tex = Texture2D.FromStream(this.GraphicsDevice, stream);
                            }
                            image = new TextureRegion(tex);
                        } catch (Exception e) {
                            if (this.ImageExceptionHandler != null) {
                                this.ImageExceptionHandler.Invoke(loc, e);
                            } else {
                                throw new Exception($"Couldn't parse image {loc}, and no ImageExceptionHandler was set", e);
                            }
                        }
                    }
                    continue;
                }

                // headers
                var parsedHeader = false;
                for (var h = 6; h >= 1; h--) {
                    if (line.StartsWith(new string('#', h))) {
                        var type = ElementTypes[Array.IndexOf(ElementTypes, ElementType.Header1) + h - 1];
                        yield return (type, new Paragraph(Anchor.AutoLeft, 1, line.Substring(h).Trim()));
                        parsedHeader = true;
                        break;
                    }
                }
                if (parsedHeader)
                    continue;

                // parse everything else as a paragraph (with formatting)
                var par = line;
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
                yield return (ElementType.Paragraph, new Paragraph(Anchor.AutoLeft, 1, par));
            }
        }

        /// <summary>
        /// A flags enumeration used by <see cref="UiMarkdownParser"/> that contains the types of elements that can be parsed and returned in <see cref="Parse"/> or <see cref="UiMarkdownParser.ParseInto"/>.
        /// This is a flags enumeration so that <see cref="UiMarkdownParser.Style{T}"/> can have multiple element types being styled at the same time.
        /// </summary>
        [Flags]
        public enum ElementType {

            /// <summary>
            /// A blockquote.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Blockquote = 1,
            /// <summary>
            /// A vertical space, which is a gap between multiple markdown paragraphs.
            /// This element type is a <see cref="VerticalSpace"/>.
            /// </summary>
            VerticalSpace = 2,
            /// <summary>
            /// An image.
            /// This element type is an <see cref="Image"/>.
            /// </summary>
            Image = 4,
            /// <summary>
            /// A header with header level 1.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Header1 = 8,
            /// <summary>
            /// A header with header level 2.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Header2 = 16,
            /// <summary>
            /// A header with header level 3.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Header3 = 32,
            /// <summary>
            /// A header with header level 4.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Header4 = 64,
            /// <summary>
            /// A header with header level 5.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Header5 = 128,
            /// <summary>
            /// A header with header level 6.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Header6 = 256,
            /// <summary>
            /// A combined flag that contains <see cref="Header1"/> through <see cref="Header6"/>.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Headers = Header1 | Header2 | Header3 | Header4 | Header5 | Header6,
            /// <summary>
            /// A paragraph, which is one line of markdown text.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Paragraph = 512,
            /// <summary>
            /// A single line of a code block.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            CodeBlock = 1024

        }

    }
}