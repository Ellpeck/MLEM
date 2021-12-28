using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MLEM.Input;
using MLEM.Startup;
using MLEM.Ui;
using MLEM.Ui.Elements;

namespace Demos {
    public class Demo {

        public readonly MlemGame Game;
        public SpriteBatch SpriteBatch => this.Game.SpriteBatch;
        public GraphicsDevice GraphicsDevice => this.Game.GraphicsDevice;
        public InputHandler InputHandler => this.Game.InputHandler;
        public UiSystem UiSystem => this.Game.UiSystem;
        public Element UiRoot => this.UiSystem.Get("DemoUi").Element;

        public Demo(MlemGame game) {
            this.Game = game;
        }

        public virtual void LoadContent() {}

        public virtual void Update(GameTime time) {}

        public virtual void DoDraw(GameTime time) {}

        public virtual void Clear() {}

        public static T LoadContent<T>(string name) {
            return MlemGame.LoadContent<T>(name);
        }

    }
}