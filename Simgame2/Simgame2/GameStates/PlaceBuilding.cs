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
    public class PlaceBuilding : GameState
    {
        public PlaceBuilding(Game1 game)
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

            Vector3 offset = Vector3.Transform(new Vector3(0, 0, -50), Matrix.CreateRotationY(game.PlayerCamera.leftrightRot));
            game.selBuilding.location = game.PlayerCamera.GetCameraPostion() + offset;

            game.selBuilding.PlaceBuilding(game.worldMap, false);


            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                game.ChangeGameState(game.MousePointerLookState);
                ButtonSpaceDown = false;
            }


            if (currentMouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown == true)
            {
                mouseLeftButtonDown = false;
                game.selBuilding.RemoveBuilding(game.worldMap);
                game.selBuilding.PlaceBuilding(game.worldMap, true);

                game.selBuilding = new EntityBuilding(game.selBuilding);
                game.ChangeGameState(game.MousePointerLookState);
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
        }

        public override void ExitState()
        {
            base.ExitState();

        }

        public override string GetShortName() { return "P"; }

    }
}
