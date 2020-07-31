using Microsoft.Xna.Framework.Graphics;

namespace Tests.Stub {
    public class StubGraphics : GraphicsDevice {

        public StubGraphics() :
            base(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, new PresentationParameters()) {
        }
    }
}