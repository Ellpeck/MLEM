using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Camera = MLEM.Cameras.Camera;

namespace MLEM.Extended.Extensions {
    public static class CameraExtensions {

        public static RectangleF GetVisibleRectangle(this Camera camera) {
            var start = camera.ToWorldPos(Vector2.Zero);
            return new RectangleF(start, camera.ToWorldPos(new Vector2(camera.Viewport.Width, camera.Viewport.Height) - start));
        }

    }
}