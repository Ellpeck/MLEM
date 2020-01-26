using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class GraphicsExtensions {

        private static int lastWidth;
        private static int lastHeight;

        public static void SetFullscreen(this GraphicsDeviceManager manager, bool fullscreen) {
            if (fullscreen || lastWidth == 0 || lastHeight == 0) {
                var view = manager.GraphicsDevice.Viewport;
                lastWidth = view.Width;
                lastHeight = view.Height;
            }
            manager.IsFullScreen = fullscreen;
            if (fullscreen) {
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