using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;
using MLEM.Misc;

namespace MLEM.Cameras {
    public class Camera {

        public Vector2 Position;
        public float Scale {
            get => this.scale;
            set => this.scale = MathHelper.Clamp(value, this.MinScale, this.MaxScale);
        }
        private float scale = 1;
        public float MinScale = 0;
        public float MaxScale = float.MaxValue;
        public bool AutoScaleWithScreen;
        public Point AutoScaleReferenceSize;
        public float ActualScale {
            get {
                if (!this.AutoScaleWithScreen)
                    return this.Scale;
                return Math.Min(this.Viewport.Width / (float) this.AutoScaleReferenceSize.X, this.Viewport.Height / (float) this.AutoScaleReferenceSize.Y) * this.Scale;
            }
        }
        public Matrix ViewMatrix {
            get {
                var sc = this.ActualScale;
                var pos = -this.Position * sc;
                if (this.roundPosition)
                    pos = pos.Floor();
                return Matrix.CreateScale(sc, sc, 1) * Matrix.CreateTranslation(new Vector3(pos, 0));
            }
        }
        public Vector2 Max {
            get => this.Position + this.ScaledViewport;
            set => this.Position = value - this.ScaledViewport;
        }
        public Vector2 LookingPosition {
            get => this.Position + this.ScaledViewport / 2;
            set => this.Position = value - this.ScaledViewport / 2;
        }
        public Rectangle Viewport => this.graphicsDevice.Viewport.Bounds;
        public Vector2 ScaledViewport => new Vector2(this.Viewport.Width, this.Viewport.Height) / this.ActualScale;

        private readonly bool roundPosition;
        private readonly GraphicsDevice graphicsDevice;

        public Camera(GraphicsDevice graphicsDevice, bool roundPosition = true) {
            this.graphicsDevice = graphicsDevice;
            this.AutoScaleReferenceSize = this.Viewport.Size;
            this.roundPosition = roundPosition;
        }

        public Vector2 ToWorldPos(Vector2 pos) {
            return Vector2.Transform(pos, Matrix.Invert(this.ViewMatrix));
        }

        public Vector2 ToCameraPos(Vector2 pos) {
            return Vector2.Transform(pos, this.ViewMatrix);
        }

        public (Vector2 topLeft, Vector2 bottomRight) GetVisibleArea() {
            return (this.ToWorldPos(Vector2.Zero), this.ToWorldPos(new Vector2(this.Viewport.Width, this.Viewport.Height)));
        }

        public RectangleF GetVisibleRectangle() {
            var start = this.ToWorldPos(Vector2.Zero);
            return new RectangleF(start, this.ToWorldPos(new Vector2(this.Viewport.Width, this.Viewport.Height)) - start);
        }

        public void ConstrainWorldBounds(Vector2 min, Vector2 max) {
            if (this.Position.X < min.X && this.Max.X > max.X) {
                this.LookingPosition = new Vector2((max.X - min.X) / 2, this.LookingPosition.Y);
            } else {
                if (this.Position.X < min.X)
                    this.Position.X = min.X;
                if (this.Max.X > max.X)
                    this.Max = new Vector2(max.X, this.Max.Y);
            }

            if (this.Position.Y < min.Y && this.Max.Y > max.Y) {
                this.LookingPosition = new Vector2(this.LookingPosition.X, (max.Y - min.Y) / 2);
            } else {
                if (this.Position.Y < min.Y)
                    this.Position.Y = min.Y;
                if (this.Max.Y > max.Y)
                    this.Max = new Vector2(this.Max.X, max.Y);
            }
        }

        public void Zoom(float delta, Vector2? zoomCenter = null) {
            var center = (zoomCenter ?? this.Viewport.Size.ToVector2() / 2) / this.ActualScale;
            var lastScale = this.Scale;
            this.Scale += delta;
            this.Position += center * ((this.Scale - lastScale) / this.Scale);
        }

    }
}