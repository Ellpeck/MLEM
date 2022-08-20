using System;
using MLEM.Ui.Elements;

namespace MLEM.Ui {
    /// <summary>
    /// A snapshot of update and rendering statistics from <see cref="UiSystem.Metrics"/> to be used for runtime debugging and profiling.
    /// </summary>
    public struct UiMetrics {

        /// <summary>
        /// The amount of time that <see cref="Element.ForceUpdateArea"/> took.
        /// Can be divided by <see cref="ForceAreaUpdates"/> to get an average per area update.
        /// </summary>
        public TimeSpan ForceAreaUpdateTime { get; internal set; }
        /// <summary>
        /// The amount of time that <see cref="Element.Update"/> took.
        /// Can be divided by <see cref="Updates"/> to get an average per update.
        /// </summary>
        public TimeSpan UpdateTime { get; internal set; }
        /// <summary>
        /// The amount of times that <see cref="Element.ForceUpdateArea"/> was called.
        /// </summary>
        public uint ForceAreaUpdates { get; internal set; }
        /// <summary>
        /// The amount of times that <see cref="Element.SetAreaAndUpdateChildren"/> was called.
        /// </summary>
        public uint ActualAreaUpdates { get; internal set; }
        /// <summary>
        /// The amount of times that <see cref="Element.Update"/> was called.
        /// </summary>
        public uint Updates { get; internal set; }

        /// <summary>
        /// The amount of time that <see cref="Element.Draw(Microsoft.Xna.Framework.GameTime,Microsoft.Xna.Framework.Graphics.SpriteBatch,float,MLEM.Graphics.SpriteBatchContext)"/> took.
        /// Can be divided by <see cref="Draws"/> to get an average per draw.
        /// </summary>
        public TimeSpan DrawTime { get; internal set; }
        /// <summary>
        /// The amount of times that <see cref="Element.Draw(Microsoft.Xna.Framework.GameTime,Microsoft.Xna.Framework.Graphics.SpriteBatch,float,MLEM.Graphics.SpriteBatchContext)"/> was called.
        /// </summary>
        public uint Draws { get; internal set; }

        /// <summary>
        /// Resets all update-related metrics to 0.
        /// </summary>
        public void ResetUpdates() {
            this.ForceAreaUpdateTime = TimeSpan.Zero;
            this.UpdateTime = TimeSpan.Zero;
            this.ForceAreaUpdates = 0;
            this.ActualAreaUpdates = 0;
            this.Updates = 0;
        }

        /// <summary>
        /// Resets all rendering-related metrics to 0.
        /// </summary>
        public void ResetDraws() {
            this.DrawTime = TimeSpan.Zero;
            this.Draws = 0;
        }

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString() {
            return $"{nameof(this.ForceAreaUpdateTime)}: {this.ForceAreaUpdateTime}, {nameof(this.UpdateTime)}: {this.UpdateTime}, {nameof(this.ForceAreaUpdates)}: {this.ForceAreaUpdates}, {nameof(this.ActualAreaUpdates)}: {this.ActualAreaUpdates}, {nameof(this.Updates)}: {this.Updates}, {nameof(this.DrawTime)}: {this.DrawTime}, {nameof(this.Draws)}: {this.Draws}";
        }

        /// <summary>
        /// Adds two ui metrics together, causing all of their values to be combined.
        /// </summary>
        /// <param name="left">The left metrics</param>
        /// <param name="right">The right metrics</param>
        /// <returns>The sum of both metrics</returns>
        public static UiMetrics operator +(UiMetrics left, UiMetrics right) {
            return new UiMetrics {
                ForceAreaUpdateTime = left.ForceAreaUpdateTime + right.ForceAreaUpdateTime,
                UpdateTime = left.UpdateTime + right.UpdateTime,
                ForceAreaUpdates = left.ForceAreaUpdates + right.ForceAreaUpdates,
                ActualAreaUpdates = left.ActualAreaUpdates + right.ActualAreaUpdates,
                Updates = left.Updates + right.Updates,
                DrawTime = left.DrawTime + right.DrawTime,
                Draws = left.Draws + right.Draws
            };
        }

        /// <summary>
        /// Subtracts two ui metrics from each other, causing their values to be subtracted.
        /// </summary>
        /// <param name="left">The left metrics</param>
        /// <param name="right">The right metrics</param>
        /// <returns>The difference of both metrics</returns>
        public static UiMetrics operator -(UiMetrics left, UiMetrics right) {
            return new UiMetrics {
                ForceAreaUpdateTime = left.ForceAreaUpdateTime - right.ForceAreaUpdateTime,
                UpdateTime = left.UpdateTime - right.UpdateTime,
                ForceAreaUpdates = left.ForceAreaUpdates - right.ForceAreaUpdates,
                ActualAreaUpdates = left.ActualAreaUpdates - right.ActualAreaUpdates,
                Updates = left.Updates - right.Updates,
                DrawTime = left.DrawTime - right.DrawTime,
                Draws = left.Draws - right.Draws
            };
        }

    }
}
