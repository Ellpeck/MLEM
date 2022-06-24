using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class Texture2DReader : RawContentReader<Texture2D> {

        /// <inheritdoc />
        protected override Texture2D Read(RawContentManager manager, string assetPath, Stream stream, Texture2D existing) {
            #if !FNA
            if (existing != null) {
                existing.Reload(stream);
                return existing;
            } else
            #endif
            {
                // premultiply the texture's color to be in line with the pipeline's texture reader
                // TODO this can be converted to use https://github.com/MonoGame/MonoGame/pull/7369 in the future
                using (var texture = Texture2D.FromStream(manager.GraphicsDevice, stream)) {
                    var ret = new Texture2D(manager.GraphicsDevice, texture.Width, texture.Height);
                    using (var textureData = texture.GetTextureData()) {
                        using (var retData = ret.GetTextureData()) {
                            for (var x = 0; x < ret.Width; x++) {
                                for (var y = 0; y < ret.Height; y++)
                                    retData[x, y] = Color.FromNonPremultiplied(textureData[x, y].ToVector4());
                            }
                        }
                    }
                    return ret;
                }
            }
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"png", "bmp", "gif", "jpg", "tif", "dds"};
        }

    }
}
