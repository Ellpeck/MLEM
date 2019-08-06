using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Extensions;

namespace MLEM.Cameras {
    public class Camera {

        public Vector2 Position;
        public float Scale;
        public Matrix ViewMatrix {
            get {
                var pos = -this.Position * this.Scale;
                if (this.roundPosition)
                    pos = pos.Floor();
                return Matrix.CreateScale(this.Scale, this.Scale, 1) * Matrix.CreateTranslation(new Vector3(pos, 0));
            }
        }
        public Vector2 LookingPosition => this.Position + new Vector2(this.Viewport.Width / 2, this.Viewport.Height / 2) / this.Scale;
        public Viewport Viewport => this.graphicsDevice.Viewport;

        private readonly bool roundPosition;
        private readonly GraphicsDevice graphicsDevice;

        public Camera(GraphicsDevice graphicsDevice, bool roundPosition = true) {
            this.graphicsDevice = graphicsDevice;
            this.roundPosition = roundPosition;
        }

        public void LookAt(Vector2 worldPos) {
            this.Position = worldPos - new Vector2(this.Viewport.Width / 2, this.Viewport.Height / 2) / this.Scale;
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

    }
}