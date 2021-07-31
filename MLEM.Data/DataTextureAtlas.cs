using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Textures;

namespace MLEM.Data {
    /// <summary>
    /// This class represents an atlas of <see cref="TextureRegion"/> objects which are loaded from a special texture atlas file. 
    /// To load a data texture atlas, you can use <see cref="DataTextureAtlasExtensions.LoadTextureAtlas"/>.
    /// To see the structure of a Data Texture Atlas, you can check out the sandbox project: <see href="https://github.com/Ellpeck/MLEM/blob/main/Sandbox/Content/Textures/Furniture.atlas"/>.
    /// Additionally, if you are using Aseprite, there is a script to automatically populate it: <see href="https://github.com/Ellpeck/MLEM/blob/main/Utilities/Populate%20Data%20Texture%20Atlas.lua"/>
    /// </summary>
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
        /// Returns an enumerable of all of the <see cref="TextureRegion"/>s in this atlas.
        /// </summary>
        public IEnumerable<TextureRegion> Regions => this.regions.Values;

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
            var info = new FileInfo(Path.Combine(content.RootDirectory, infoPath));
            string text;
            using (var reader = info.OpenText())
                text = reader.ReadToEnd();
            var atlas = new DataTextureAtlas(texture);

            // parse each texture region: "<name> loc <u> <v> <w> <h> [piv <px> <py>] [off <ox> <oy>]"
            foreach (Match match in Regex.Matches(text, @"(.+)\W+loc\W+([0-9]+)\W+([0-9]+)\W+([0-9]+)\W+([0-9]+)\W*(?:piv\W+([0-9.]+)\W+([0-9.]+))?\W*(?:off\W+([0-9.]+)\W+([0-9.]+))?")) {
                var name = match.Groups[1].Value.Trim();
                // offset
                var off = !match.Groups[8].Success ? Vector2.Zero : new Vector2(
                    float.Parse(match.Groups[8].Value, CultureInfo.InvariantCulture),
                    float.Parse(match.Groups[9].Value, CultureInfo.InvariantCulture));

                // location
                var loc = new Rectangle(
                    int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value));
                loc.Offset(off);

                // pivot
                var piv = !match.Groups[6].Success ? Vector2.Zero : off + new Vector2(
                    float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture) - (pivotRelative ? 0 : loc.X),
                    float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture) - (pivotRelative ? 0 : loc.Y));

                var region = new TextureRegion(texture, loc) {
                    PivotPixels = piv,
                    Name = name
                };
                atlas.regions.Add(name, region);
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