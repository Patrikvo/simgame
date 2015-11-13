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

        public GameState(Game1 game)
        {
            this.game = game;
        }

        public virtual void Draw(GameTime gameTime) 
        { 
        }

        public virtual void Update(GameTime gameTime) 
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.game.Exit();


            

            game.PlayerCamera.AdjustCameraAltitude(gameTime);


            this.currentMouseState =  Mouse.GetState();
            this.keyState = Keyboard.GetState();

            game.entityFactory.Update(gameTime);

            // place building code
            if (game.selBuilding != null)
            {
                game.selBuilding.RemoveBuilding(game.worldMap);
            }

            float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
            Vector3 moveVector = new Vector3(0, 0, 0);

            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))
                moveVector += new Vector3(0, 0, -1);
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))
                moveVector += new Vector3(0, 0, 1);
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))
                moveVector += new Vector3(1, 0, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))
                moveVector += new Vector3(-1, 0, 0);


            game.PlayerCamera.AddToCameraPosition(moveVector * timeDifference);


            if (keyState.IsKeyDown(Keys.Escape))
                game.Exit();


            if (keyState.IsKeyDown(Keys.X))
                ButtonXDown = true;

            if (keyState.IsKeyUp(Keys.X) && ButtonXDown == true)
            {
                game.showDebugImg = ! game.showDebugImg;
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
                Entities.MoverEntity mover = this.game.entityFactory.CreateMover(game.PlayerCamera.GetCameraPostion());
                this.game.worldMap.AddEntity(mover);
                this.game.simulator.AddEntity(mover.GetSimEntity());
                ButtonInsertDown = false;
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

        protected Game1 game;
    }
}
