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
    public class GameState
    {
        public const string Name = "BaseState";

        public GameState(GameSession.GameSession RunningGameSession)
        {
            this.RunningGameSession = RunningGameSession;
        }

        public virtual void Draw(GameTime gameTime) 
        { 
        }

        public virtual void Update(GameTime gameTime) 
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.RunningGameSession.game.Exit();




            this.RunningGameSession.PlayerCamera.AdjustCameraAltitude(gameTime);
            


            this.currentMouseState =  Mouse.GetState();
            this.keyState = Keyboard.GetState();

            this.RunningGameSession.entityFactory.Update(gameTime);

            //this.game.worldMap.Update(gameTime);
            this.RunningGameSession.LODMap.Update(gameTime);

            // place building code
            if (this.RunningGameSession.selBuilding != null)
            {
              //  game.selBuilding.RemoveBuilding(game.worldMap);
                this.RunningGameSession.selBuilding.RemoveBuilding(this.RunningGameSession.LODMap);
            }

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Vector3 moveVector = new Vector3(0, 0, 0);

            if (keyState.IsKeyDown(Keys.Up))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left))
                moveVector += new Vector3(-1, 0, 0);


            if (keyState.IsKeyDown(Keys.R))
                this.RunningGameSession.PlayerCamera.cameraHeight = -this.RunningGameSession.PlayerCamera.cameraHeight;
            if (keyState.IsKeyDown(Keys.F))
                moveVector += new Vector3(0, -1, 0);


            this.RunningGameSession.PlayerCamera.AddToCameraPosition(moveVector * timeDifference);


            if (keyState.IsKeyDown(Keys.Escape))
                this.RunningGameSession.game.Exit();


            if (keyState.IsKeyDown(Keys.X))
                ButtonXDown = true;

            if (keyState.IsKeyUp(Keys.X) && ButtonXDown == true)
            {
                this.RunningGameSession.showDebugImg = ! this.RunningGameSession.showDebugImg;
                ButtonXDown = false;
            }


            if (keyState.IsKeyDown(Keys.Space))
                ButtonSpaceDown = true;

            if (currentMouseState.LeftButton == ButtonState.Pressed)
            {
                mouseLeftButtonDown = true;
            }


            if (currentMouseState.RightButton == ButtonState.Pressed)
            {
                mouseRightButtonDown = true;
            }


            if (keyState.IsKeyDown(Keys.Insert))
            {
                ButtonInsertDown = true;
            }

            if (keyState.IsKeyUp(Keys.Insert) && ButtonInsertDown == true)
            {
                Entities.MoverEntity mover = this.RunningGameSession.entityFactory.CreateMover(this.RunningGameSession.PlayerCamera.GetCameraPostion());
                //this.game.worldMap.AddEntity(mover);
                this.RunningGameSession.LODMap.AddEntity(mover);
                this.RunningGameSession.simulator.AddEntity(mover.movement.GetSimEntity());


                    ButtonInsertDown = false;
            }


            if (keyState.IsKeyDown(Keys.S))
            {
                ButtonSDown = true;
            }
            if (keyState.IsKeyUp(Keys.S) && ButtonSDown == true)
            {
                this.RunningGameSession.SaveGame("savegame.dat");
                ButtonSDown = false;
            }


            if (keyState.IsKeyDown(Keys.L))
            {
                ButtonLDown = true;
            }
            if (keyState.IsKeyUp(Keys.L) && ButtonLDown == true)
            {
                this.RunningGameSession.LoadGame("savegame.dat");
                ButtonLDown = false;
            }

            if (keyState.IsKeyDown(Keys.N))
            {
                ButtonNDown = true;
            }
            if (keyState.IsKeyUp(Keys.N) && ButtonNDown == true)
            {
                //this.game.worldMap.CreateNewMap(this.game.worldMap.mapNumCellsPerRow, this.game.worldMap.mapNumCellPerColumn);
                this.RunningGameSession.LODMap.CreateNewMap(this.RunningGameSession.LODMap.mapNumCellsPerRow, this.RunningGameSession.LODMap.mapNumCellPerColumn, this.RunningGameSession.device, null);
                ButtonNDown = false;
            }


            

            

        }


        


        public virtual void EnterState() { }

        public virtual void ExitState() { }

        public virtual string GetShortName() { return "B"; }


        public MouseState currentMouseState;
        public MouseState originalMouseState;

        public KeyboardState keyState;

        protected bool ButtonXDown = false;
        protected bool mouseLeftButtonDown = false;
        protected bool mouseRightButtonDown = false;
        protected bool ButtonSpaceDown = false;
        protected bool ButtonInsertDown = false;


        protected bool ButtonSDown = false;
        protected bool ButtonLDown = false;
        protected bool ButtonNDown = false;


        protected GameSession.GameSession RunningGameSession;
    }
}
