using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Font;

namespace Tests.Stub {
    public class StubFont : GenericFont {

        public override GenericFont Bold => this;
        public override GenericFont Italic => this;
        public override float LineHeight => 1;

        protected override Vector2 MeasureChar(char c) {
            return Vector2.Zero;
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color) {
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
        }

        public override void DrawString(SpriteBatch batch, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
        }

        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color) {
        }

        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth) {
        }

        public override void DrawString(SpriteBatch batch, StringBuilder text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth) {
        }

    }
}