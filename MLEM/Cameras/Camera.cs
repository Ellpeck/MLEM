using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Cameras {
    /// <summary>
    /// Represents a simple, orthographic 2-dimensional camera that can be moved, scaled and that supports automatic viewport sizing.
    /// To draw with the camera's positioning and scaling applied, use <see cref="ViewMatrix"/>.
    /// </summary>
    public class Camera {

        /// <summary>
        /// This field holds an epsilon value used in some camera calculations to mitigate floating point rounding inaccuracies.
        /// If camera <see cref="Position"/> or <see cref="Viewport"/> size are extremely small or extremely big, this value can be reduced or increased.
        /// </summary>
        public static float Epsilon = 0.01F;

        /// <summary>
        /// The top-left corner of the camera's viewport.
        /// <seealso cref="LookingPosition"/>
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The scale that this camera's <see cref="ViewMatrix"/> should have.
        /// </summary>
        public float Scale {
            get => this.scale;
            set => this.scale = MathHelper.Clamp(value, this.MinScale, this.MaxScale);
        }
        /// <summary>
        /// The minimum <see cref="Scale"/> that the camera can have
        /// </summary>
        public float MinScale = 0;
        /// <summary>
        /// The maximum <see cref="Scale"/> that the camera can have
        /// </summary>
        public float MaxScale = float.MaxValue;
        /// <summary>
        /// If this is true, the camera will automatically adapt to changed screen sizes.
        /// You can use <see cref="AutoScaleReferenceSize"/> to determine the initial screen size that this camera should base its calculations on.
        /// </summary>
        public bool AutoScaleWithScreen;
        /// <summary>
        /// <seealso cref="AutoScaleWithScreen"/>
        /// </summary>
        public Point AutoScaleReferenceSize;
        /// <summary>
        /// The scale that this camera currently has, based on <see cref="Scale"/> and <see cref="AutoScaleReferenceSize"/> if <see cref="AutoScaleWithScreen"/> is true.
        /// </summary>
        public float ActualScale {
            get {
                if (!this.AutoScaleWithScreen)
                    return this.Scale;
                return Math.Min(this.Viewport.Width / (float) this.AutoScaleReferenceSize.X, this.Viewport.Height / (float) this.AutoScaleReferenceSize.Y) * this.Scale;
            }
        }
        /// <summary>
        /// The matrix that this camera "sees", based on its position and scale.
        /// Use this in your <see cref="SpriteBatch.Begin"/> calls to render based on the camera's viewport.
        /// </summary>
        public Matrix ViewMatrix {
            get {
                var sc = this.ActualScale;
                var pos = -this.Position * sc;
                if (this.RoundPosition)
                    pos = pos.FloorCopy();
                return Matrix.CreateScale(sc, sc, 1) * Matrix.CreateTranslation(new Vector3(pos, 0));
            }
        }
        /// <summary>
        /// The bottom-right corner of the camera's viewport
        /// <seealso cref="LookingPosition"/>
        /// </summary>
        public Vector2 Max {
            get => this.Position + this.ScaledViewport;
            set => this.Position = value - this.ScaledViewport;
        }
        /// <summary>
        /// The center of the camera's viewport, or the position that the camera is looking at.
        /// </summary>
        public Vector2 LookingPosition {
            get => this.Position + this.ScaledViewport / 2;
            set => this.Position = value - this.ScaledViewport / 2;
        }
        /// <summary>
        /// The viewport of this camera, based on the game's <see cref="GraphicsDevice.Viewport"/> and this camera's <see cref="ActualScale"/>
        /// </summary>
        public Vector2 ScaledViewport => new Vector2(this.Viewport.Width, this.Viewport.Height) / this.ActualScale;
        /// <summary>
        /// Whether the camera's <see cref="Position"/> should be rounded to full integers when calculating the <see cref="ViewMatrix"/>.
        /// If this value is true, the occurence of rendering fragments due to floating point rounding might be reduced.
        /// </summary>
        public bool RoundPosition;

        private Rectangle Viewport => this.graphicsDevice.Viewport.Bounds;
        private readonly GraphicsDevice graphicsDevice;
        private float scale = 1;

        /// <summary>
        /// Creates a new camera.
        /// </summary>
        /// <param name="graphicsDevice">The game's graphics device</param>
        /// <param name="roundPosition">Whether the camera's <see cref="Position"/> should be rounded to full integers when calculating the <see cref="ViewMatrix"/></param>
        public Camera(GraphicsDevice graphicsDevice, bool roundPosition = true) {
            this.graphicsDevice = graphicsDevice;
            this.AutoScaleReferenceSize = this.Viewport.Size;
            this.RoundPosition = roundPosition;
        }

        /// <summary>
        /// Converts a given position in screen space to world space
        /// </summary>
        /// <param name="pos">The position in screen space</param>
        /// <returns>The position in world space</returns>
        public Vector2 ToWorldPos(Vector2 pos) {
            return Vector2.Transform(pos, Matrix.Invert(this.ViewMatrix));
        }

        /// <summary>
        /// Converts a given position in world space to screen space
        /// </summary>
        /// <param name="pos">The position in world space</param>
        /// <returns>The position in camera space</returns>
        public Vector2 ToCameraPos(Vector2 pos) {
            return Vector2.Transform(pos, this.ViewMatrix);
        }

        /// <summary>
        /// Returns the area that this camera can see, in world space.
        /// This can be useful for culling of tile and other entity renderers.
        /// </summary>
        /// <returns>A rectangle that represents the camera's visible area in world space</returns>
        public RectangleF GetVisibleRectangle() {
            var start = this.ToWorldPos(Vector2.Zero);
            return new RectangleF(start, this.ToWorldPos(new Vector2(this.Viewport.Width, this.Viewport.Height)) - start);
        }

        /// <summary>
        /// Forces the camera's bounds into the given min and max positions in world space.
        /// If the space represented by the given values is smaller than what the camera can see, its position will be forced into the center of the area.
        /// </summary>
        /// <param name="min">The top left bound, in world space</param>
        /// <param name="max">The bottom right bound, in world space</param>
        /// <returns>Whether or not the camera's position changed as a result of the constraint</returns>
        public bool ConstrainWorldBounds(Vector2 min, Vector2 max) {
            var lastPos = this.Position;
            var visible = this.GetVisibleRectangle();
            if (max.X - min.X < visible.Width) {
                this.LookingPosition = new Vector2((max.X + min.X) / 2, this.LookingPosition.Y);
            } else {
                if (this.Position.X < min.X)
                    this.Position.X = min.X;
                if (this.Max.X > max.X)
                    this.Max = new Vector2(max.X, this.Max.Y);
            }

            if (max.Y - min.Y < visible.Height) {
                this.LookingPosition = new Vector2(this.LookingPosition.X, (max.Y + min.Y) / 2);
            } else {
                if (this.Position.Y < min.Y)
                    this.Position.Y = min.Y;
                if (this.Max.Y > max.Y)
                    this.Max = new Vector2(this.Max.X, max.Y);
            }
            return !this.Position.Equals(lastPos, Camera.Epsilon);
        }

        /// <summary>
        /// Zoom in the camera's view by a given amount, optionally focusing on a given center point.
        /// </summary>
        /// <param name="delta">The amount to zoom in or out by</param>
        /// <param name="zoomCenter">The position that should be regarded as the zoom's center, in screen space. The default value is the center.</param>
        public void Zoom(float delta, Vector2? zoomCenter = null) {
            var center = (zoomCenter ?? this.Viewport.Size.ToVector2() / 2) / this.ActualScale;
            var lastScale = this.Scale;
            this.Scale += delta;
            this.Position += center * ((this.Scale - lastScale) / this.Scale);
        }

    }
}
