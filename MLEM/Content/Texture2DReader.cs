using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Content {
    public class Texture2DReader : RawContentReader<Texture2D> {

        protected override Texture2D Read(RawContentManager manager, string assetPath, Stream stream, Texture2D existing) {
            if (existing != null) {
                existing.Reload(stream);
                return existing;
            } else {
                return Texture2D.FromStream(manager.GraphicsDevice, stream);
            }
        }

        public override string[] GetFileExtensions() {
            return new[] {"png", "bmp", "gif", "jpg", "tif", "dds"};
        }

    }
}