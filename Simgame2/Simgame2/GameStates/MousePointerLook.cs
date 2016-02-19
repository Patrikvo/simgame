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
      //  private Vector3 markerLocation;
        public MousePointerLook(Game1 game)
            : base(game)
        {
            
        }

        short[] bBoxIndices = { 0, 1};

        public override void Draw(GameTime gameTime) 
        {
            
            base.Draw(gameTime);

                        

        }

        Entity entityWithFocus;
     //   private BasicEffect effect;



     //   private bool hold;

        private Ray mouseCursorRay;

        public override void Update(GameTime gameTime) 
        {
            base.Update(gameTime);

          //  if (keyState.IsKeyDown(Keys.F1))
         //   {
         //       hold = true;
         //   }


            if (entityWithFocus != null) 
            {
                entityWithFocus.HasMouseFocus = false;
            }



            mouseCursorRay = game.PlayerCamera.UnProjectScreenPoint(currentMouseState.X, currentMouseState.Y, this.game.GraphicsDevice.Viewport);
                
            
            //entityWithFocus = this.game.worldMap.FindEntityAt(mouseCursorRay);
            entityWithFocus = this.game.LODMap.FindEntityAt(mouseCursorRay);

            if (entityWithFocus != null)
            {
                entityWithFocus.HasMouseFocus = true;
            }


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

            game.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, currentMouseState.LeftButton == ButtonState.Pressed, false);



            if (keyState.IsKeyDown(Keys.P))
            {
                int x = 5;
                x++;
                
            }


            if (keyState.IsKeyDown(Keys.D))
            {
                ButtonDDown = true;
            }
            if (keyState.IsKeyUp(Keys.D) && ButtonDDown == true)
            {
                ButtonDDown = false;
                if (this.formDebug == null)
                {
                    this.formDebug = new Tools.FormDebug(this.game);
                    this.formDebug.Show();
                }
                else 
                {
                    this.formDebug.Visible = !this.formDebug.Visible;
                }
                
            }


            if (this.formDebug != null)
            {
                this.formDebug.Update();
            }

        }

        public override void EnterState()
        {
            base.EnterState();
            game.IsMouseVisible = true;
        }

        public override void ExitState()
        {
            if (entityWithFocus != null)
            {
                entityWithFocus.HasMouseFocus = false;
            }
            base.ExitState();
            game.IsMouseVisible = false;
        }

        public override string GetShortName() { return "M"; }


        protected bool ButtonDDown = false;


        Tools.FormDebug formDebug;
    }
}
