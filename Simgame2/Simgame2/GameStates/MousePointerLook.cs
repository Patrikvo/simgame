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
    public class MousePointerLook : GameState
    {
        public MousePointerLook(Game1 game)
            : base(game)
        {

        }


        public override void Draw(GameTime gameTime) 
        {
            base.Draw(gameTime);

        }

        
        public override void Update(GameTime gameTime) 
        {
            base.Update(gameTime);

            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                game.ChangeGameState(game.PlaceBuildingState);
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
                game.ChangeGameState(game.FreeLookState);
                
            }

            game.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, currentMouseState.LeftButton == ButtonState.Pressed);

 

        }

        public override void EnterState()
        {
            base.EnterState();
            game.IsMouseVisible = true;
        }

        public override void ExitState()
        {
            base.ExitState();
            game.IsMouseVisible = false;
        }

        public override string GetShortName() { return "M"; }

        
    }
}
