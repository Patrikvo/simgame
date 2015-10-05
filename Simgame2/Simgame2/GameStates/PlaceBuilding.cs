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
            LastSelectedEntityType = Entity.EntityTypes.NONE;
        }


        public override void Draw(GameTime gameTime) 
        {
            base.Draw(gameTime);
        }

        int mouseRotation = 0;
        public override void Update(GameTime gameTime) 
        {
            base.Update(gameTime);

            


            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                game.ChangeGameState(game.MousePointerLookState);
                ButtonSpaceDown = false;
            }




            markerLocation = game.PlayerCamera.UnProjectScreenPointLoc(currentMouseState.X, currentMouseState.Y, this.game.GraphicsDevice.Viewport);


           // Vector3 offset = Vector3.Transform(new Vector3(0, 0, -50), Matrix.CreateRotationY(game.PlayerCamera.leftrightRot));

            int rotAmout = currentMouseState.ScrollWheelValue - mouseRotation;
            mouseRotation = currentMouseState.ScrollWheelValue;

           


            if (game.selBuilding == null)
            {
                game.selBuilding = (EntityBuilding)game.entityFactory.CreateEnity(LastSelectedEntityType, markerLocation, false);
            }

            game.selBuilding.location = markerLocation;

            // convert rotation to radials, add to current rotation then limit to 0 - 2*PI
            game.selBuilding.rotation.Y = (float)(game.selBuilding.rotation.Y + (rotAmout * Math.PI / 180) % 2* Math.PI);

            game.selBuilding.IsTransparant = true;
            
            
            // does the building collide with other buildings?
            game.selBuilding.CanPlace = !game.worldMap.Collides(game.selBuilding.boundingBox);
            
            // are we placing in on water?
            if (game.selBuilding.getAltitude() < 1)
            
         //   if (game.worldMap.getAltitude(game.selBuilding.location.X, game.selBuilding.location.Z) < WorldMap.waterHeight) 
            {
                game.selBuilding.CanPlace = false;
            }

            
            
            game.selBuilding.PlaceBuilding(game.worldMap, false);



            if (currentMouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown == true)
            {
                mouseLeftButtonDown = false;
                if (game.selBuilding.CanPlace)
                {
                    game.selBuilding.RemoveBuilding(game.worldMap);
                    game.selBuilding.IsTransparant = false;
                    game.selBuilding.PlaceBuilding(game.worldMap, true);
                    game.simulator.AddEntity(game.selBuilding.GetSimEntity());


                 //   game.selBuilding = new EntityBuilding(game.selBuilding);
                    game.selBuilding = (EntityBuilding)game.entityFactory.CreateEnity(LastSelectedEntityType, markerLocation, false);
                    game.ChangeGameState(game.MousePointerLookState);
                }
            }

            if (currentMouseState.RightButton == ButtonState.Released && mouseRightButtonDown == true)
            {
                mouseRightButtonDown = false;
                game.ChangeGameState(game.FreeLookState);

            }

            //game.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, currentMouseState.LeftButton == ButtonState.Pressed);
            game.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, false, true);


        }

        public override void EnterState()
        {
            game.IsMouseVisible = true;
            base.EnterState();
            if (game.selBuilding != null)
            {
                game.selBuilding.RemoveBuilding(game.worldMap);
            }
         //   game.selBuilding = null;
            game.selBuilding = (EntityBuilding)game.entityFactory.CreateEnity(LastSelectedEntityType, markerLocation, false);
        }

        public override void ExitState()
        {
            game.IsMouseVisible = false;
            base.ExitState();
            if (game.selBuilding != null)
            {
                game.selBuilding.RemoveBuilding(game.worldMap);
            }
        }

        public override string GetShortName() { return "P"; }
        private Vector3 markerLocation;
        public Entity.EntityTypes LastSelectedEntityType { get; set; }
    }
}
