using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MLEM.Extensions {
    /// <summary>
    /// A set of extensions for dealing with <see cref="GraphicsDevice"/> and <see cref="GraphicsDeviceManager"/>
    /// </summary>
    public static class GraphicsExtensions {

        private static int lastWidth;
        private static int lastHeight;

        /// <summary>
        /// Sets the graphics device manager to fullscreen, properly taking into account the preferred backbuffer width and height to avoid lower resolutions for higher resolution screens.
        /// </summary>
        /// <param name="manager">The graphics device manager</param>
        /// <param name="fullscreen">True if fullscreen should be enabled, false if disabled</param>
        /// <exception cref="InvalidOperationException">Thrown when changing out of fullscreen mode before changing into fullscreen mode using this method</exception>
        public static void SetFullscreen(this GraphicsDeviceManager manager, bool fullscreen) {
            manager.IsFullScreen = fullscreen;
            if (fullscreen) {
                var view = manager.GraphicsDevice.Viewport;
                GraphicsExtensions.lastWidth = view.Width;
                GraphicsExtensions.lastHeight = view.Height;

                var curr = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                manager.PreferredBackBufferWidth = curr.Width;
                manager.PreferredBackBufferHeight = curr.Height;
            } else {
                if (GraphicsExtensions.lastWidth <= 0 || GraphicsExtensions.lastHeight <= 0)
                    throw new InvalidOperationException("Can't call SetFullscreen to change out of fullscreen mode without going into fullscreen mode first");

                manager.PreferredBackBufferWidth = GraphicsExtensions.lastWidth;
                manager.PreferredBackBufferHeight = GraphicsExtensions.lastHeight;
            }
            manager.ApplyChanges();
        }

        /// <summary>
        /// Save version of <see cref="GraphicsDeviceManager.ApplyChanges"/> that doesn't reset window size to defaults
        /// </summary>
        /// <param name="manager">The graphics device manager</param>
        public static void ApplyChangesSafely(this GraphicsDeviceManager manager) {
            // If we don't do this, then applying changes will cause the
            // graphics device manager to reset the window size to the
            // size set when starting the game :V
            var view = manager.GraphicsDevice.Viewport;
            manager.PreferredBackBufferWidth = view.Width;
            manager.PreferredBackBufferHeight = view.Height;
            manager.ApplyChanges();
        }

        /// <summary>
        /// Resets preferred width and height back to the window's default bound values.
        /// </summary>
        /// <param name="manager">The graphics device manager</param>
        /// <param name="window">The window whose bounds to use</param>
        public static void ResetWidthAndHeight(this GraphicsDeviceManager manager, GameWindow window) {
            var (width, height) = (window.ClientBounds.Width, window.ClientBounds.Height);
            manager.PreferredBackBufferWidth = Math.Max(height, width);
            manager.PreferredBackBufferHeight = Math.Min(height, width);
            manager.ApplyChanges();
        }

        /// <summary>
        /// Starts a new <see cref="TargetContext"/> using the specified render target.
        /// The returned context automatically disposes when used in a <c>using</c> statement, which causes any previously applied render targets to be reapplied automatically.
        /// </summary>
        /// <param name="device">The graphics device</param>
        /// <param name="target">The render target to apply</param>
        /// <returns></returns>
        public static TargetContext WithRenderTarget(this GraphicsDevice device, RenderTarget2D target) {
            return new TargetContext(device, target);
        }

        /// <summary>
        /// Represents a context in which a <see cref="RenderTarget2D"/> is applied.
        /// This class should be used with <see cref="GraphicsExtensions.WithRenderTarget"/>.
        /// </summary>
        public readonly struct TargetContext : IDisposable {

            private readonly GraphicsDevice device;
            private readonly RenderTargetBinding[] lastTargets;

            /// <summary>
            /// Creates a new target context with the given settings.
            /// </summary>
            /// <param name="device">The graphics device to apply the target on</param>
            /// <param name="target">The target to apply</param>
            public TargetContext(GraphicsDevice device, RenderTarget2D target) {
                this.device = device;
                #if FNA
                // RenderTargetCount doesn't exist in FNA but we still want the optimization in MG
                this.lastTargets = device.GetRenderTargets();
                #else
                this.lastTargets = device.RenderTargetCount <= 0 ? null : device.GetRenderTargets();
                #endif
                device.SetRenderTarget(target);
            }

            /// <summary>
            /// Disposes this target context, which causes the graphics device's previous render targets to be re-applied.
            /// </summary>
            public void Dispose() {
                this.device.SetRenderTargets(this.lastTargets);
            }

        }

    }
}
