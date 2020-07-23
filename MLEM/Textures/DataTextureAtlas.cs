using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Textures {
    /// <summary>
    /// This class represents an atlas of <see cref="TextureRegion"/> objects which are loaded from a special texture atlas file. 
    /// To load a data texture atlas, you can use <see cref="DataTextureAtlasExtensions.LoadTextureAtlas"/>.
    /// </summary>
    public class DataTextureAtlas {

        /// <summary>
        /// The texture to use for this atlas
        /// </summary>
        public readonly Texture2D Texture;
        /// <summary>
        /// Returns the texture region with the given name.
        /// </summary>
        /// <param name="name">The region's name</param>
        public TextureRegion this[string name] => this.regions[name];
        /// <summary>
        /// Returns an enumerable of all of the <see cref="TextureRegion"/>s in this atlas.
        /// </summary>
        public IEnumerable<TextureRegion> Regions => this.regions.Values;

        private readonly Dictionary<string, TextureRegion> regions = new Dictionary<string, TextureRegion>();

        /// <summary>
        /// Creates a new data texture atlas with the given texture and region amount.
        /// </summary>
        /// <param name="texture">The texture to use for this atlas</param>
        public DataTextureAtlas(Texture2D texture) {
            this.Texture = texture;
        }

        internal static DataTextureAtlas Load(ContentManager content, string texturePath, string infoPath, bool pivotRelative) {
            var info = new FileInfo(Path.Combine(content.RootDirectory, infoPath ?? $"{texturePath}.atlas"));
            string text;
            using (var reader = info.OpenText())
                text = reader.ReadToEnd();

            var texture = content.Load<Texture2D>(texturePath);
            var atlas = new DataTextureAtlas(texture);

            // parse each texture region: "<name> loc <u> <v> <w> <h> piv <px> <py>"
            const string regex = @"(.+)\W+loc\W+([0-9]+)\W+([0-9]+)\W+([0-9]+)\W+([0-9]+)\W+(?:piv\W+([0-9.]+)\W+([0-9.]+))?";
            foreach (Match match in Regex.Matches(text, regex)) {
                var name = match.Groups[1].Value.Trim();
                var loc = new Rectangle(
                    int.Parse(match.Groups[2].Value), int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value), int.Parse(match.Groups[5].Value));
                var piv = Vector2.Zero;
                if (match.Groups[6].Success) {
                    piv = new Vector2(
                        float.Parse(match.Groups[6].Value, CultureInfo.InvariantCulture) - (pivotRelative ? 0 : loc.X),
                        float.Parse(match.Groups[7].Value, CultureInfo.InvariantCulture) - (pivotRelative ? 0 : loc.Y));
                }
                atlas.regions.Add(name, new TextureRegion(texture, loc) {
                    PivotPixels = piv,
                    Name = name
                });
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
            return DataTextureAtlas.Load(content, texturePath, infoPath, pivotRelative);
        }

    }
}