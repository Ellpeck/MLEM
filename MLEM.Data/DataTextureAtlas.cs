using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Textures;

namespace MLEM.Data {
    /// <summary>
    /// <para>
    /// This class represents an atlas of <see cref="TextureRegion"/> objects which are loaded from a special texture atlas file.
    /// To load a data texture atlas, you can use <see cref="DataTextureAtlasExtensions.LoadTextureAtlas"/>.
    /// </para>
    /// <para>
    /// Data texture atlases are designed to be easy to write by hand. Because of this, their structure is very simple.
    /// Each texture region defined in the atlas consists of its name, followed by a set of possible keywords and their arguments, separated by spaces.
    /// The <c>loc</c> keyword defines the <see cref="TextureRegion.Area"/> of the texture region as a rectangle whose origin is its top-left corner. It requires four arguments: x, y, width and height of the rectangle.
    /// The (optional) <c>piv</c> keyword defines the <see cref="TextureRegion.PivotPixels"/> of the texture region. It requires two arguments: x and y. If it is not supplied, the pivot defaults to the top-left corner of the texture region.
    /// The (optional) <c>off</c> keyword defines an offset that is added onto the location and pivot of this texture region. This is useful when copying and pasting a previously defined texture region to create a second region that has a constant offset. It requires two arguments: The x and y offset.
    /// </para>
    /// <example>
    /// The following entry defines a texture region with the name <c>LongTableRight</c>, whose <see cref="TextureRegion.Area"/> will be a rectangle with X=32, Y=30, Width=64, Height=48, and whose <see cref="TextureRegion.PivotPixels"/> will be a vector with X=80, Y=46.
    /// <code>
    /// LongTableRight
    /// loc 32 30 64 48
    /// piv 80 46
    /// </code>
    /// </example>
    /// </summary>
    /// <remarks>
    /// To see a Data Texture Atlas in action, you can check out the sandbox project: https://github.com/Ellpeck/MLEM/blob/main/Sandbox/Content/Textures/Furniture.atlas.
    /// Additionally, if you are using Aseprite, there is a script to automatically populate it: https://gist.github.com/Ellpeck/e597c1412465c10f41a42050ec117ea2.
    /// </remarks>
    public class DataTextureAtlas {

        /// <summary>
        /// The texture to use for this atlas
        /// </summary>
        public readonly TextureRegion Texture;
        /// <summary>
        /// Returns the texture region with the given name, or null if it does not exist.
        /// </summary>
        /// <param name="name">The region's name</param>
        public TextureRegion this[string name] {
            get {
                this.regions.TryGetValue(name, out var ret);
                return ret;
            }
        }
        /// <summary>
        /// Returns an enumerable of all of the <see cref="TextureRegion"/> values in this atlas.
        /// </summary>
        public IEnumerable<TextureRegion> Regions => this.regions.Values;
        /// <summary>
        /// Returns an enumerable of all of the <see cref="TextureRegion"/> names in this atlas.
        /// </summary>
        public IEnumerable<string> RegionNames => this.regions.Keys;

        private readonly Dictionary<string, TextureRegion> regions = new Dictionary<string, TextureRegion>();

        private DataTextureAtlas(TextureRegion texture) {
            this.Texture = texture;
        }

        /// <summary>
        /// Loads a <see cref="DataTextureAtlas"/> from the given loaded texture and texture data file.
        /// </summary>
        /// <param name="texture">The texture to use for this data texture atlas</param>
        /// <param name="content">The content manager to use for loading</param>
        /// <param name="infoPath">The path, including extension, to the atlas info file</param>
        /// <param name="pivotRelative">If this value is true, then the pivot points passed in the info file will be relative to the coordinates of the texture region, not relative to the entire texture's origin.</param>
        /// <returns>A new data texture atlas with the given settings</returns>
        public static DataTextureAtlas LoadAtlasData(TextureRegion texture, ContentManager content, string infoPath, bool pivotRelative = false) {
            var info = Path.Combine(content.RootDirectory, infoPath);
            string text;
            if (Path.IsPathRooted(info)) {
                text = File.ReadAllText(info);
            } else {
                using (var reader = new StreamReader(TitleContainer.OpenStream(info)))
                    text = reader.ReadToEnd();
            }
            var atlas = new DataTextureAtlas(texture);

            // parse each texture region: "<names> loc <u> <v> <w> <h> [piv <px> <py>] [off <ox> <oy>]"
            foreach (Match match in Regex.Matches(text, @"(.+)\s+loc\s+([0-9+]+)\s+([0-9+]+)\s+([0-9+]+)\s+([0-9+]+)\s*(?:piv\s+([0-9.+-]+)\s+([0-9.+-]+))?\s*(?:off\s+([0-9.+-]+)\s+([0-9.+-]+))?")) {
                // offset
                var off = !match.Groups[8].Success ? Vector2.Zero : new Vector2(
                    float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                    float.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture));

                // location
                var loc = new Rectangle(
                    int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value));
                loc.Offset(off.ToPoint());

                // pivot
                var piv = !match.Groups[6].Success ? Vector2.Zero : off + new Vector2(
                    float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture) - (pivotRelative ? 0 : loc.X),
                    float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture) - (pivotRelative ? 0 : loc.Y));

                foreach (var name in Regex.Split(match.Groups[1].Value, @"\s")) {
                    var trimmed = name.Trim();
                    if (trimmed.Length <= 0)
                        continue;
                    var region = new TextureRegion(texture, loc) {
                        PivotPixels = piv,
                        Name = trimmed
                    };
                    atlas.regions.Add(trimmed, region);
                }
            }

            return atlas;
        }

    }

    /// <summary>
    /// A set of extension methods for dealing with <see cref="DataTextureAtlas"/>.
    /// </summary>
    public static class DataTextureAtlasExtensions {

        /// <summary>
        /// Loads a <see cref="DataTextureAtlas"/> from the given texture and texture data file.
        /// </summary>
        /// <param name="content">The content manager to use for loading</param>
        /// <param name="texturePath">The path to the texture file</param>
        /// <param name="infoPath">The path, including extension, to the atlas info file, or null if "<paramref name="texturePath"/>.atlas" should be used</param>
        /// <param name="pivotRelative">If this value is true, then the pivot points passed in the info file will be relative to the coordinates of the texture region, not relative to the entire texture's origin.</param>
        /// <returns>A new data texture atlas with the given settings</returns>
        public static DataTextureAtlas LoadTextureAtlas(this ContentManager content, string texturePath, string infoPath = null, bool pivotRelative = false) {
            var texture = new TextureRegion(content.Load<Texture2D>(texturePath));
            return DataTextureAtlas.LoadAtlasData(texture, content, infoPath ?? $"{texturePath}.atlas", pivotRelative);
        }

    }
}
