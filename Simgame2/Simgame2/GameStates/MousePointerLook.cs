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
        public MousePointerLook(GameSession.GameSession RunningGameSession)
            : base(RunningGameSession)
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


            Entity previousEntityWithFocus = entityWithFocus;

            mouseCursorRay = this.RunningGameSession.PlayerCamera.UnProjectScreenPoint(currentMouseState.X, currentMouseState.Y, this.RunningGameSession.device.Viewport);
                
            
            //entityWithFocus = this.game.worldMap.FindEntityAt(mouseCursorRay);
            entityWithFocus = this.RunningGameSession.LODMap.FindEntityAt(mouseCursorRay);


            if (previousEntityWithFocus != null && entityWithFocus != null && previousEntityWithFocus != entityWithFocus)
            {
                this.RunningGameSession.simulator.AddEvent(new Simulation.Events.EntityLostFocusEvent(previousEntityWithFocus));
                this.RunningGameSession.simulator.AddEvent(new Simulation.Events.EntityHasFocusEvent(entityWithFocus));
            }
            else if (previousEntityWithFocus != null && entityWithFocus == null)
            {
                this.RunningGameSession.simulator.AddEvent(new Simulation.Events.EntityLostFocusEvent(previousEntityWithFocus));
            }
            else if (previousEntityWithFocus == null && entityWithFocus != null)
            {
                this.RunningGameSession.simulator.AddEvent(new Simulation.Events.EntityHasFocusEvent(entityWithFocus));
            }



            // EntityHasFocusEvent

            if (keyState.IsKeyUp(Keys.Space) && ButtonSpaceDown == true)
            {
                this.RunningGameSession.ChangeGameState(this.RunningGameSession.PlaceBuildingState);
                ButtonSpaceDown = false;
            }

            




            if (currentMouseState.LeftButton == ButtonState.Released && mouseLeftButtonDown == true)
            {
                // do nothing in this state
                mouseLeftButtonDown = false;


                if (entityWithFocus != null)
                {
                    if (SelectedEntity == null ) 
                    {
                        SelectedEntity = entityWithFocus;
                    }
                    else
                    {
                        if (SelectedEntity.CanBeCommanded && SelectedEntity.IsMover)
                        {
                            ((Entities.MoverEntity)SelectedEntity).ManualControl = true;
                            //((Entities.MoverEntity)SelectedEntity).MoveTo(entityWithFocus.location);
                            ((Entities.MoverEntity)SelectedEntity).MoveTo(entityWithFocus);
                            SelectedEntity = null;
                        }
                        else
                        {
                            SelectedEntity = null;
                        }
                    }

                    
                }



            }


            if (currentMouseState.RightButton == ButtonState.Released && mouseRightButtonDown == true)
            {
                mouseRightButtonDown = false;
                this.RunningGameSession.ChangeGameState(this.RunningGameSession.FreeLookState);
                
            }

            this.RunningGameSession.HUD_overlay.Update(currentMouseState.X, currentMouseState.Y, currentMouseState.LeftButton == ButtonState.Pressed, false);



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
                    this.formDebug = new Tools.FormDebug(this.RunningGameSession);
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
            this.RunningGameSession.game.IsMouseVisible = true;
        }

        public override void ExitState()
        {
            if (entityWithFocus != null)
            {
                entityWithFocus.HasMouseFocus = false;
            }
            base.ExitState();
            this.RunningGameSession.game.IsMouseVisible = false;
        }

        public override string GetShortName() { return "M"; }


        protected bool ButtonDDown = false;



        protected Entity SelectedEntity;


        Tools.FormDebug formDebug;
    }
}
