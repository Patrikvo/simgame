using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Simgame2.GameStates
{
    public class FreeLook : GameState
    {
        public FreeLook(Game1 game): base(game)
        {
            
        }


        public override void Draw(GameTime gameTime) 
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime) 
        {
            base.Update(gameTime);

            if (currentMouseState != originalMouseState)
            {
                float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
                float yDifference = 0;
                float xDifference = currentMouseState.X - originalMouseState.X;
                yDifference = currentMouseState.Y - originalMouseState.Y;
                game.PlayerCamera.leftrightRot -= Camera.rotationSpeed * xDifference * timeDifference;
                game.PlayerCamera.updownRot -= Camera.rotationSpeed * yDifference * timeDifference;
                Mouse.SetPosition(game.device.Viewport.Width / 2, game.device.Viewport.Height / 2);
                game.PlayerCamera.UpdateViewMatrix();
            }

            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                // do nothing in this state
                ButtonSpaceDown = false;
            }


            if (currentMouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown == true)
            {
                // do nothing in this state
                mouseLeftButtonDown = false;
            }


            if (currentMouseState.RightButton == ButtonState.Released && mouseRightButtonDown == true)
            {
                mouseRightButtonDown = false;
                game.ChangeGameState(game.MousePointerLookState);
            }


            if (keyState.IsKeyUp(Keys.PageUp) && button_PageUp_pressed == true)
            { 
                button_PageUp_pressed = false;
                game.worldMap.GetRenderer().IncreaseAmbientLightLevel();
            }
            if (keyState.IsKeyDown(Keys.PageUp))
            {
                button_PageUp_pressed = true;
            }


            if (keyState.IsKeyUp(Keys.PageDown) && button_PageDown_pressed == true)
            {
                button_PageDown_pressed = false;
                game.worldMap.GetRenderer().DecreaseAmbientLightLevel();
            }
            if (keyState.IsKeyDown(Keys.PageDown))
            {
                button_PageDown_pressed = true;
            }





            game.HUD_overlay.Update(0, 0, false, true);


        }

        private bool button_PageUp_pressed;
        private bool button_PageDown_pressed;



        public override void EnterState()
        {
            base.EnterState();
            Mouse.SetPosition(game.device.Viewport.Width / 2, game.device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();
            game.IsMouseVisible = false;
            button_PageUp_pressed = false;
            button_PageDown_pressed = false;
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public override string GetShortName() { return "F"; }

    }
}
