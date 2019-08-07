using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MLEM.Input;
using MLEM.Startup;

namespace Tests {
    public class GameImpl : MlemGame {

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            // Input tests
            if (Input.IsKeyPressed(Keys.A))
                Console.WriteLine("A was pressed");
            if (Input.IsMouseButtonPressed(MouseButton.Left))
                Console.WriteLine("Left was pressed");
            if (Input.IsGamepadButtonPressed(0, Buttons.A))
                Console.WriteLine("Gamepad A was pressed");
        }

    }
}