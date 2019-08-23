using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Cameras {
    public class Camera {

        public Vector2 Position;
        public float Scale;
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

        public void ConstrainWorldBounds(Vector2 min, Vector2 max) {
            if (this.Position.X < min.X)
                this.Position.X = min.X;
            if (this.Position.Y < min.Y)
                this.Position.Y = min.Y;

            if (this.Max.X > max.X)
                this.Max = new Vector2(max.X, this.Max.Y);
            if (this.Max.Y > max.Y)
                this.Max = new Vector2(this.Max.X, max.Y);
        }

    }
}