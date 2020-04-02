using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    public static class GraphicsExtensions {

        private static int lastWidth;
        private static int lastHeight;

        public static void SetFullscreen(this GraphicsDeviceManager manager, bool fullscreen) {
            manager.IsFullScreen = fullscreen;
            if (fullscreen) {
                var view = manager.GraphicsDevice.Viewport;
                lastWidth = view.Width;
                lastHeight = view.Height;

                var curr = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                manager.PreferredBackBufferWidth = curr.Width;
                manager.PreferredBackBufferHeight = curr.Height;
            } else {
                if (lastWidth <= 0 || lastHeight <= 0)
                    throw new InvalidOperationException("Can't call SetFullscreen to change out of fullscreen mode without going into fullscreen mode first");

                manager.PreferredBackBufferWidth = lastWidth;
                manager.PreferredBackBufferHeight = lastHeight;
            }
            manager.ApplyChanges();
        }

        public static void ApplyChangesSafely(this GraphicsDeviceManager manager) {
            // If we don't do this, then applying changes will cause the
            // graphics device manager to reset the window size to the
            // size set when starting the game :V
            var view = manager.GraphicsDevice.Viewport;
            manager.PreferredBackBufferWidth = view.Width;
            manager.PreferredBackBufferHeight = view.Height;
            manager.ApplyChanges();
        }

        public static TargetContext WithRenderTarget(this GraphicsDevice device, RenderTarget2D target) {
            return new TargetContext(device, target);
        }

        public struct TargetContext : IDisposable {

            private readonly GraphicsDevice device;
            private readonly RenderTargetBinding[] lastTargets;

            public TargetContext(GraphicsDevice device, RenderTarget2D target) {
                this.device = device;
                this.lastTargets = device.RenderTargetCount <= 0 ? null : device.GetRenderTargets();
                device.SetRenderTarget(target);
            }

            public void Dispose() {
                if (this.lastTargets != null) {
                    this.device.SetRenderTargets(this.lastTargets);
                } else {
                    this.device.SetRenderTarget(null);
                }
            }

        }

    }
}