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
        public PlaceBuilding(GameSession.GameSession RunningGameSession)
            : base(RunningGameSession)
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
                this.RunningGameSession.ChangeGameState(this.RunningGameSession.MousePointerLookState);
                ButtonSpaceDown = false;
            }




            markerLocation = this.RunningGameSession.PlayerCamera.UnProjectScreenPointLoc(currentMouseState.X, currentMouseState.Y, this.RunningGameSession.device.Viewport);


           // Vector3 offset = Vector3.Transform(new Vector3(0, 0, -50), Matrix.CreateRotationY(game.PlayerCamera.leftrightRot));

            int rotAmout = currentMouseState.ScrollWheelValue - mouseRotation;
            mouseRotation = currentMouseState.ScrollWheelValue;




            if (this.RunningGameSession.selBuilding == null)
            {
                this.RunningGameSession.selBuilding = (EntityBuilding)this.RunningGameSession.entityFactory.CreateEnity(LastSelectedEntityType, markerLocation, false);
                this.RunningGameSession.selBuilding.IsGhost = true;
            }

            this.RunningGameSession.selBuilding.location = markerLocation;

            // convert rotation to radials, add to current rotation then limit to 0 - 2*PI
            this.RunningGameSession.selBuilding.rotation.Y = (float)(this.RunningGameSession.selBuilding.rotation.Y + (rotAmout * Math.PI / 180) % 2 * Math.PI);


            //game.selBuilding.PlaceBuilding(game.worldMap, false);
            this.RunningGameSession.selBuilding.PlaceBuilding(this.RunningGameSession.LODMap, false);

            this.RunningGameSession.selBuilding.UpdateBoundingBox();

            this.RunningGameSession.selBuilding.IsTransparant = true;
            
            
            // does the building collide with other buildings?
            //game.selBuilding.CanPlace = !game.worldMap.Collides(game.selBuilding.boundingBox);
            this.RunningGameSession.selBuilding.CanPlace = !this.RunningGameSession.LODMap.Collides(this.RunningGameSession.selBuilding.boundingBox);
            
            // are we placing in on water?
            if (this.RunningGameSession.selBuilding.getAltitude() < 1)
            
         //   if (game.worldMap.getAltitude(game.selBuilding.location.X, game.selBuilding.location.Z) < WorldMap.waterHeight) 
            {
                this.RunningGameSession.selBuilding.CanPlace = false;
            }

            
            
          //  game.selBuilding.PlaceBuilding(game.worldMap, false);



            if (currentMouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown == true)
            {
                mouseLeftButtonDown = false;
                if (this.RunningGameSession.selBuilding.CanPlace)
                {
                    //this.RunningGameSession.selBuilding.RemoveBuilding(game.worldMap);
                    this.RunningGameSession.selBuilding.RemoveBuilding(this.RunningGameSession.LODMap);
                    this.RunningGameSession.selBuilding.IsTransparant = false;
                    this.RunningGameSession.selBuilding.IsGhost = false;
                    //this.RunningGameSession.selBuilding.PlaceBuilding(game.worldMap, true);
                    this.RunningGameSession.selBuilding.PlaceBuilding(this.RunningGameSession.LODMap, true);
                    this.RunningGameSession.simulator.AddEntity(this.RunningGameSession.selBuilding.GetSimEntity());
                    this.RunningGameSession.simulator.MapModified = true;

                 //   game.selBuilding = new EntityBuilding(game.selBuilding);
                    this.RunningGameSession.selBuilding = (EntityBuilding)this.RunningGameSession.entityFactory.CreateEnity(LastSelectedEntityType, markerLocation, false);
                    this.RunningGameSession.selBuilding.IsGhost = true;
                    this.RunningGameSession.ChangeGameState(this.RunningGameSession.MousePointerLookState);
                }
            }

            if (currentMouseState.RightButton == ButtonState.Released && mouseRightButtonDown == true)
            {
                mouseRightButtonDown = false;
                this.RunningGameSession.ChangeGameState(this.RunningGameSession.FreeLookState);

            }

            //game.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, currentMouseState.LeftButton == ButtonState.Pressed);
            this.RunningGameSession.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, false, true);


        }

        public override void EnterState()
        {
            this.RunningGameSession.game.IsMouseVisible = true;
            base.EnterState();
            if (this.RunningGameSession.selBuilding != null)
            {
                //game.selBuilding.RemoveBuilding(game.worldMap);
                this.RunningGameSession.selBuilding.RemoveBuilding(this.RunningGameSession.LODMap);
            }
         //   game.selBuilding = null;
            this.RunningGameSession.selBuilding = (EntityBuilding)this.RunningGameSession.entityFactory.CreateEnity(LastSelectedEntityType, markerLocation, false);
            this.RunningGameSession.selBuilding.IsGhost = true;
        }

        public override void ExitState()
        {
            this.RunningGameSession.game.IsMouseVisible = false;
            base.ExitState();
            if (this.RunningGameSession.selBuilding != null)
            {
                //game.selBuilding.RemoveBuilding(game.worldMap);
                this.RunningGameSession.selBuilding.RemoveBuilding(this.RunningGameSession.LODMap);
            }
        }

        public override string GetShortName() { return "P"; }
        private Vector3 markerLocation;
        public Entity.EntityTypes LastSelectedEntityType { get; set; }
    }
}
