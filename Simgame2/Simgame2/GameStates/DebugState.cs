﻿using System;
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
    public class DebugState: GameState
    {
        public DebugState(GameSession.GameSession RunningGameSession)
            : base(RunningGameSession)
        {
            
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
          //  base.Update(gameTime);

            this.keyState = Keyboard.GetState();
            this.RunningGameSession.entityFactory.Update(gameTime);
            this.RunningGameSession.LODMap.Update(gameTime);

            if (keyState.IsKeyDown(Keys.Escape))
                this.RunningGameSession.game.Exit();

            if (currentMouseState != originalMouseState)
            {
         //       float timeDifference = (float)gameTime.ElapsedGameTime.TotalMilliseconds / 1000.0f;
          //      float yDifference = 0;
           //     float xDifference = currentMouseState.X - originalMouseState.X;
            //    yDifference = currentMouseState.Y - originalMouseState.Y;
            //    game.PlayerCamera.leftrightRot -= Camera.rotationSpeed * xDifference * timeDifference;
            //    game.PlayerCamera.updownRot -= Camera.rotationSpeed * yDifference * timeDifference;
           //     Mouse.SetPosition(game.device.Viewport.Width / 2, game.device.Viewport.Height / 2);
           //     game.PlayerCamera.UpdateViewMatrix();
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
                this.RunningGameSession.ChangeGameState(this.RunningGameSession.MousePointerLookState);
            }


            if (keyState.IsKeyUp(Keys.PageUp) && button_PageUp_pressed == true)
            {
                button_PageUp_pressed = false;
                //game.worldMap.GetRenderer().IncreaseAmbientLightLevel();
                this.RunningGameSession.LODMap.GetRenderer().IncreaseAmbientLightLevel();
            }
            if (keyState.IsKeyDown(Keys.PageUp))
            {
                button_PageUp_pressed = true;
            }


            if (keyState.IsKeyUp(Keys.PageDown) && button_PageDown_pressed == true)
            {
                button_PageDown_pressed = false;
                //   game.worldMap.GetRenderer().DecreaseAmbientLightLevel();
                this.RunningGameSession.LODMap.GetRenderer().DecreaseAmbientLightLevel();
            }
            if (keyState.IsKeyDown(Keys.PageDown))
            {
                button_PageDown_pressed = true;
            }

            if (keyState.IsKeyUp(Keys.D) && button_D_pressed == true)
            {
                button_D_pressed = false;
                this.RunningGameSession.ChangeGameState(this.RunningGameSession.FreeLookState);

            }
            if (keyState.IsKeyDown(Keys.D) && button_D_Reseted == true)
            {
                button_D_pressed = true;
            }

            if (keyState.IsKeyUp(Keys.D) && button_D_pressed == false && button_D_Reseted == false)
            {
                button_D_Reseted = true;
            }


            if (keyState.IsKeyDown(Keys.X))
                ButtonXDown = true;

            if (keyState.IsKeyUp(Keys.X) && ButtonXDown == true)
            {
                this.RunningGameSession.showDebugImg = !this.RunningGameSession.showDebugImg;
                ButtonXDown = false;
            }


            this.RunningGameSession.HUD_overlay.Update(0, 0, false, true);


        }

        private bool button_PageUp_pressed;
        private bool button_PageDown_pressed;
        private bool button_D_pressed;

        private bool button_D_Reseted;

//        private bool ButtonXDown;



        public override void EnterState()
        {
            
            base.EnterState();
            Mouse.SetPosition(RunningGameSession.device.Viewport.Width / 2, RunningGameSession.device.Viewport.Height / 2);
            originalMouseState = Mouse.GetState();
            this.RunningGameSession.game.IsMouseVisible = false;
            button_PageUp_pressed = false;
            button_PageDown_pressed = false;
            button_D_pressed = false;
            Vector3 ppos = this.RunningGameSession.PlayerCamera.GetCameraPostion();
            this.RunningGameSession.PlayerCamera.ForceViewMatrix(ppos + this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightPosition, ppos+  this.RunningGameSession.LODMap.GetRenderer().SunLight.ShadowLightTarget, Vector3.Up);
            button_D_Reseted = false;
        }

        public override void ExitState()
        {
            base.ExitState();
        }

        public override string GetShortName() { return "D"; }




    }
}
