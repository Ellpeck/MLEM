using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Textures;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class Texture2DReader : RawContentReader<Texture2D> {

        /// <inheritdoc />
        protected override Texture2D Read(RawContentManager manager, string assetPath, Stream stream, Texture2D existing) {
#if !FNA && !KNI
            if (existing != null) {
                existing.Reload(stream);
                return existing;
            }
#endif

            // premultiply the texture's color to be in line with the pipeline's texture reader
            using (var texture = Texture2D.FromStream(manager.GraphicsDevice, stream))
                return texture.PremultipliedCopy();
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"png", "bmp", "gif", "jpg", "tif", "dds"};
        }

    }
}
