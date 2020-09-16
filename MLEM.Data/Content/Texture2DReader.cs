using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Data.Content {
    /// <inheritdoc />
    public class Texture2DReader : RawContentReader<Texture2D> {

        /// <inheritdoc />
        protected override Texture2D Read(RawContentManager manager, string assetPath, Stream stream, Texture2D existing) {
            if (existing != null) {
                existing.Reload(stream);
                return existing;
            } else {
                return Texture2D.FromStream(manager.GraphicsDevice, stream);
            }
        }

        /// <inheritdoc />
        public override string[] GetFileExtensions() {
            return new[] {"png", "bmp", "gif", "jpg", "tif", "dds"};
        }

    }
}