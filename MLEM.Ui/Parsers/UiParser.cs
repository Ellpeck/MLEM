using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Formatting;
using MLEM.Textures;
using MLEM.Ui.Elements;
using MLEM.Ui.Style;

#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
using System.Net.Http;
#else
using System.Net;
#endif

namespace MLEM.Ui.Parsers {
    /// <summary>
    /// A base class for parsing various types of formatted strings into a set of MLEM.Ui elements with styling for each individual <see cref="ElementType"/>.
    /// The only parser currently implemented is <see cref="UiMarkdownParser"/>.
    /// </summary>
    public abstract class UiParser {

        /// <summary>
        /// An array containing all of the <see cref="ElementType"/> enum values.
        /// </summary>
        public static readonly ElementType[] ElementTypes =
#if NET6_0_OR_GREATER
            Enum.GetValues<ElementType>();
#else
            (ElementType[]) Enum.GetValues(typeof(ElementType));
#endif

        /// <summary>
        /// The base path for images, which is prepended to the image link.
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
        /// This defaults to "Monospaced" if default styling is applied in <see cref="UiParser(bool)"/>.
        /// </summary>
        public string CodeFont;

        private readonly Dictionary<ElementType, Action<Element>> elementStyles = new Dictionary<ElementType, Action<Element>>();

        /// <summary>
        /// Creates a new UI parser and optionally initializes some default style settings.
        /// </summary>
        /// <param name="applyDefaultStyling">Whether default style settings should be applied.</param>
        protected UiParser(bool applyDefaultStyling) {
            if (applyDefaultStyling) {
                this.CodeFont = "Monospaced";
                this.Style<VerticalSpace>(ElementType.VerticalSpace, v => v.Size = new Vector2(1, 5));
                for (var i = 0; i < 6; i++) {
                    var level = i;
                    this.Style<Paragraph>(UiParser.ElementTypes[Array.IndexOf(UiParser.ElementTypes, UiParser.ElementType.Header1) + i], p => {
                        p.Alignment = TextAlignment.Center;
                        p.TextScaleMultiplier = 2 - level * 0.15F;
                    });
                }
            }
        }

        /// <summary>
        /// Parses the given raw formatted string into a set of elements and returns them along with their <see cref="ElementType"/>.
        /// This method is used by implementors to parse specific text, and it is used by <see cref="Parse"/> and <see cref="ParseInto"/>.
        /// </summary>
        /// <param name="raw">The raw string to parse.</param>
        /// <returns>The parsed elements, without styling.</returns>
        protected abstract IEnumerable<(ElementType, Element)> ParseUnstyled(string raw);

        /// <summary>
        /// Parses the given raw formatted string into a set of elements and returns them along with their <see cref="ElementType"/>.
        /// During this process, the element stylings specified using <see cref="Style{T}"/> are also applied.
        /// </summary>
        /// <param name="raw">The raw string to parse.</param>
        /// <returns>The parsed elements.</returns>
        public IEnumerable<(ElementType, Element)> Parse(string raw) {
            foreach (var (t, e) in this.ParseUnstyled(raw)) {
                if (this.elementStyles.TryGetValue(t, out var style))
                    style.Invoke(e);
                yield return (t, e);
            }
        }

        /// <summary>
        /// Parses the given raw formatted string into a set of elements (using <see cref="Parse"/>) and adds them as children to the givem <paramref name="element"/>.
        /// During this process, the element stylings specified using <see cref="Style{T}"/> are also applied.
        /// </summary>
        /// <param name="raw">The raw string to parse.</param>
        /// <param name="element">The element to add the parsed elements to.</param>
        /// <returns>The <paramref name="element"/>, for chaining.</returns>
        public Element ParseInto(string raw, Element element) {
            foreach (var (_, e) in this.Parse(raw))
                element.AddChild(e);
            return element;
        }

        /// <summary>
        /// Specifies an action to be invoked when a new element with the given <see cref="ElementType"/> is parsed in <see cref="Parse"/> or <see cref="ParseInto"/>.
        /// These actions can be used to modify the style properties of the created elements similarly to <see cref="UiStyle.AddCustomStyle{T}"/>.
        /// </summary>
        /// <param name="types">The element types that should be styled. Can be a combined flag.</param>
        /// <param name="style">The action that styles the elements with the given element type.</param>
        /// <param name="add">Whether the <paramref name="style"/> function should be added to the existing style settings rather than replacing them.</param>
        /// <typeparam name="T">The type of elements that the given <see cref="ElementType"/> flags are expected to be.</typeparam>
        /// <returns>This parser, for chaining.</returns>
        public UiParser Style<T>(ElementType types, Action<T> style, bool add = false) where T : Element {
            foreach (var type in UiParser.ElementTypes) {
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

        /// <summary>
        /// Parses the given path into a <see cref="Image"/> element by loading it from disk or downloading it from the internet.
        /// Note that, for a <paramref name="path"/> that doesn't start with <c>http</c> and isn't rooted, the <see cref="ImageBasePath"/> is prepended automatically.
        /// This method invokes an asynchronouns action, meaning the <see cref="Image"/>'s <see cref="Image.Texture"/> will likely not have loaded in when this method returns.
        /// </summary>
        /// <param name="path">The absolute, relative or web path to the image.</param>
        /// <returns>The loaded image.</returns>
        /// <exception cref="NullReferenceException">Thrown if <see cref="GraphicsDevice"/> is null, or if there is an <see cref="Exception"/> loading the image and <see cref="ImageExceptionHandler"/> is unset.</exception>
        protected Image ParseImage(string path) {
            if (this.GraphicsDevice == null)
                throw new NullReferenceException("A UI parser requires a GraphicsDevice for parsing images");

            TextureRegion image = null;
            return new Image(Anchor.AutoLeft, Vector2.One, _ => image) {
                SetHeightBasedOnAspect = true,
                OnAddedToUi = e => {
                    if (image == null)
                        LoadImageAsync();
                },
                OnRemovedFromUi = e => {
                    image?.Texture.Dispose();
                    image = null;
                }
            };

            async void LoadImageAsync() {
                // only apply the base path for relative files
                if (this.ImageBasePath != null && !path.StartsWith("http") && !Path.IsPathRooted(path))
                    path = $"{this.ImageBasePath}/{path}";
                try {
                    Texture2D tex;
                    if (path.StartsWith("http")) {
                        byte[] src;
#if NETSTANDARD2_0_OR_GREATER || NET6_0_OR_GREATER
                        using (var client = new HttpClient())
                            src = await client.GetByteArrayAsync(path);
#else
                        using (var client = new WebClient())
                            src = await client.DownloadDataTaskAsync(path);
#endif
                        using (var memory = new MemoryStream(src))
                            tex = Texture2D.FromStream(this.GraphicsDevice, memory);
                    } else {
                        using (var stream = Path.IsPathRooted(path) ? File.OpenRead(path) : TitleContainer.OpenStream(path))
                            tex = Texture2D.FromStream(this.GraphicsDevice, stream);
                    }
                    image = new TextureRegion(tex);
                } catch (Exception e) {
                    if (this.ImageExceptionHandler != null) {
                        this.ImageExceptionHandler.Invoke(path, e);
                    } else {
                        throw new NullReferenceException($"Couldn't parse image {path}, and no ImageExceptionHandler was set", e);
                    }
                }
            }
        }

        /// <summary>
        /// A flags enumeration used by <see cref="UiParser"/> that contains the types of elements that can be parsed and returned in <see cref="Parse"/> or <see cref="ParseInto"/>.
        /// This is a flags enumeration so that <see cref="Style{T}"/> can have multiple element types being styled at the same time.
        /// </summary>
        [Flags]
        public enum ElementType {

            /// <summary>
            /// A blockquote.
            /// This element type is a <see cref="Paragraph"/>.
            /// </summary>
            Blockquote = 1,
            /// <summary>
            /// A vertical space, which is a gap between multiple paragraphs.
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
            Headers = ElementType.Header1 | ElementType.Header2 | ElementType.Header3 | ElementType.Header4 | ElementType.Header5 | ElementType.Header6,
            /// <summary>
            /// A paragraph, which is one line (or non-vertically spaced section) of text.
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
