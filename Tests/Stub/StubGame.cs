using Microsoft.Xna.Framework;

namespace Tests.Stub {
    public class StubGame : Game {

        public StubGame() {
            new GraphicsDeviceManager(this);
            this.RunOneFrame();
        }

    }
}