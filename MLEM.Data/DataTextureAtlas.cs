using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Maths;
using MLEM.Misc;
using MLEM.Textures;

namespace MLEM.Data {
    /// <summary>
    /// <para>
    /// This class represents an atlas of <see cref="TextureRegion"/> objects which are loaded from a special texture atlas file.
    /// To load a data texture atlas, you can use <see cref="DataTextureAtlasExtensions.LoadTextureAtlas"/>.
    /// </para>
    /// <para>
    /// Data texture atlases are designed to be easy to write by hand. Because of this, their structure is very simple.
    /// Each texture region defined in the atlas consists of its names (where multiple names can be separated by whitespace), followed by a set of possible instructions and their arguments, also separated by whitespace.
    /// <list type="bullet">
    /// <item><description>The <c>loc</c> (or <c>location</c>) instruction defines the <see cref="TextureRegion.Area"/> of the texture region as a rectangle whose origin is its top-left corner. It requires four arguments: x, y, width and height of the rectangle.</description></item>
    /// <item><description>The (optional) <c>piv</c> (or <c>pivot</c>) instruction defines the <see cref="TextureRegion.PivotPixels"/> of the texture region. It requires two arguments: x and y. If it is not supplied, the pivot defaults to the top-left corner of the texture region.</description></item>
    /// <item><description>The (optional) <c>off</c> (of <c>offset</c>) instruction defines an offset that is added onto the location and pivot of this texture region. This is useful when duplicating a previously defined texture region to create a second region that has a constant offset. It requires two arguments: The x and y offset.</description></item>
    /// <item><description>The (optional and repeatable) <c>cpy</c> (or <c>copy</c>) instruction defines an additional texture region that should also be generated from the same data, but with a given offset that will be applied to the location and pivot. It requires three arguments: the copy region's name and the x and y offsets.</description></item>
    /// <item><description>The (optional and repeatable) <c>dat</c> (or <c>data</c>) instruction defines a custom data point that can be added to the resulting <see cref="TextureRegion"/>'s <see cref="GenericDataHolder"/> data. It requires two arguments: the data point's name and the data point's value, the latter of which is also stored as a string value.</description></item>
    /// <item><description>The (optional) <c>frm</c> (or <c>from</c>) instruction defines a texture region (defined before the current region) whose data should be copied. All data from the region will be copied, but adding additional instructions afterwards modifies the data. It requires one argument: the name of the region whose data to copy. If this instruction is used, the <c>loc</c> instruction is not required.</description></item>
    /// </list>
    /// </para>
    /// <example>
    /// The following entry defines a texture region with the names <c>LongTableRight</c> and <c>LongTableUp</c>, whose <see cref="TextureRegion.Area"/> will be a rectangle with X=32, Y=30, Width=64, Height=48, and whose <see cref="TextureRegion.PivotPixels"/> will be a vector with X=80, Y=46.
    /// <code>
    /// LongTableRight LongTableUp
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
        /// Converts this data texture atlas to a <see cref="Dictionary{TKey,TValue}"/> and returns the result.
        /// The resulting dictionary will contain all named regions that this data texture atlas contains.
        /// </summary>
        /// <returns>The dictionary representation of this data texture atlas.</returns>
        public Dictionary<string, TextureRegion> ToDictionary() {
            return new Dictionary<string, TextureRegion>(this.regions);
        }

        /// <summary>
        /// Loads a <see cref="DataTextureAtlas"/> from the given loaded texture and texture data file.
        /// For more information on data texture atlases, see the <see cref="DataTextureAtlas"/> type documentation.
        /// </summary>
        /// <param name="texture">The texture to use for this data texture atlas</param>
        /// <param name="content">The content manager to use for loading</param>
        /// <param name="infoPath">The path, including extension, to the atlas info file</param>
        /// <param name="pivotRelative">If this value is true, then the pivot points passed in the info file will be relative to the coordinates of the texture region, not relative to the entire texture's origin.</param>
        /// <returns>A new data texture atlas with the given settings</returns>
        public static DataTextureAtlas LoadAtlasData(TextureRegion texture, ContentManager content, string infoPath, bool pivotRelative = false) {
            var info = Path.Combine(content.RootDirectory, infoPath);
            string text;
            try {
                if (Path.IsPathRooted(info)) {
                    text = File.ReadAllText(info);
                } else {
                    using (var reader = new StreamReader(TitleContainer.OpenStream(info)))
                        text = reader.ReadToEnd();
                }
            } catch (Exception e) {
                throw new ContentLoadException($"Couldn't load data texture atlas data from {info}", e);
            }
            var atlas = new DataTextureAtlas(texture);
            var words = Regex.Split(text, @"\s+");

            var namesOffsets = new List<(string, Vector2)>();
            var customData = new Dictionary<string, string>();
            var location = Rectangle.Empty;
            var pivot = Vector2.Zero;
            var offset = Vector2.Zero;
            for (var i = 0; i < words.Length; i++) {
                var word = words[i];
                try {
                    switch (word) {
                        case "loc":
                        case "location":
                            location = new Rectangle(
                                int.Parse(words[i + 1], CultureInfo.InvariantCulture), int.Parse(words[i + 2], CultureInfo.InvariantCulture),
                                int.Parse(words[i + 3], CultureInfo.InvariantCulture), int.Parse(words[i + 4], CultureInfo.InvariantCulture));
                            i += 4;
                            break;
                        case "piv":
                        case "pivot":
                            pivot = new Vector2(
                                float.Parse(words[i + 1], CultureInfo.InvariantCulture),
                                float.Parse(words[i + 2], CultureInfo.InvariantCulture));
                            i += 2;
                            break;
                        case "off":
                        case "offset":
                            offset = new Vector2(
                                float.Parse(words[i + 1], CultureInfo.InvariantCulture),
                                float.Parse(words[i + 2], CultureInfo.InvariantCulture));
                            i += 2;
                            break;
                        case "cpy":
                        case "copy":
                            var copyOffset = new Vector2(
                                float.Parse(words[i + 2], CultureInfo.InvariantCulture),
                                float.Parse(words[i + 3], CultureInfo.InvariantCulture));
                            namesOffsets.Add((words[i + 1], copyOffset));
                            i += 3;
                            break;
                        case "dat":
                        case "data":
                            customData.Add(words[i + 1], words[i + 2]);
                            i += 2;
                            break;
                        case "frm":
                        case "from":
                            var fromRegion = atlas[words[i + 1]];
                            customData.Clear();
                            foreach (var key in fromRegion.GetDataKeys())
                                customData.Add(key, fromRegion.GetData<string>(key));
                            // our main texture might be a sub-region already, so we have to take that into account
                            location = fromRegion.Area.OffsetCopy(new Point(-texture.U, -texture.V));
                            pivot = fromRegion.PivotPixels;
                            if (pivot != Vector2.Zero && !pivotRelative)
                                pivot += location.Location.ToVector2();
                            offset = Vector2.Zero;
                            i += 1;
                            break;
                        default:
                            // if we have data for the previous regions, they're valid so we add them
                            AddCurrentRegions();

                            // we're starting a new region (or adding another name for a new region), so clear old data
                            namesOffsets.Add((word.Trim(), Vector2.Zero));
                            customData.Clear();
                            location = Rectangle.Empty;
                            pivot = Vector2.Zero;
                            offset = Vector2.Zero;
                            break;
                    }
                } catch (Exception e) {
                    throw new ContentLoadException($"Couldn't parse data texture atlas instruction {word} for region(s) {string.Join(", ", namesOffsets)}", e);
                }
            }

            // add the last region that was started on
            AddCurrentRegions();
            return atlas;

            void AddCurrentRegions() {
                // the location is the only mandatory information, which is why we check it here
                if (location == Rectangle.Empty || namesOffsets.Count <= 0)
                    return;
                foreach (var (name, addedOff) in namesOffsets) {
                    var loc = location;
                    var piv = pivot;
                    var off = offset + addedOff;

                    loc.Offset(off.ToPoint());
                    if (piv != Vector2.Zero) {
                        piv += off;
                        if (!pivotRelative)
                            piv -= loc.Location.ToVector2();
                    }

                    var region = new TextureRegion(texture, loc) {
                        PivotPixels = piv,
                        Name = name
                    };
                    foreach (var kv in customData)
                        region.SetData(kv.Key, kv.Value);
                    atlas.regions.Add(name, region);
                }
                // we only clear names offsets if the location was valid, otherwise we ignore multiple names for a region
                namesOffsets.Clear();
            }
        }

    }

    /// <summary>
    /// A set of extension methods for dealing with <see cref="DataTextureAtlas"/>.
    /// </summary>
    public static class DataTextureAtlasExtensions {

        /// <summary>
        /// Loads a <see cref="DataTextureAtlas"/> from the given texture and texture data file.
        /// For more information on data texture atlases, see the <see cref="DataTextureAtlas"/> type documentation.
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
