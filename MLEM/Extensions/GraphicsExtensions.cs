using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class GraphicsExtensions {

        private static int lastWidth;
        private static int lastHeight;

        public static void SetFullscreen(this GraphicsDeviceManager manager, bool fullscreen) {
            manager.IsFullScreen = fullscreen;
            if (fullscreen) {
                lastWidth = manager.GraphicsDevice.Viewport.Width;
                lastHeight = manager.GraphicsDevice.Viewport.Height;

                var curr = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                manager.PreferredBackBufferWidth = curr.Width;
                manager.PreferredBackBufferHeight = curr.Height;
            } else {
                manager.PreferredBackBufferWidth = lastWidth;
                manager.PreferredBackBufferHeight = lastHeight;
            }
            manager.ApplyChanges();
        }

    }
}