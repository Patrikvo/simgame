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

namespace Simgame2.Buildings
{
    public class SolarPlant : EntityBuilding
    {
        public SolarPlant(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.SOLAR;
            this.CurrentState = States.UNDER_CONSTRUCTION;
        }


        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            base.OnEvent(ReceivedEvent);
            // TODO IMPLEMENT


            return false;
        }

        public override void Reset()
        {
            if (!IsGhost)
            {
                CurrentAnimation = ConstructionAnimation;
                CurrentAnimation.Reset();
            }
        }

        // Parts
        const int BaseIndex = 0;

        const int Left_downIndex = 1;
        const int Left_upIndex = 2;
        const int Right_downIndex = 3;
        const int Right_upIndex = 4;
        const int TopIndex = 5;

        private string[] BoneNames = { "Base", "Left_down", "Left_up", "Right_down", "Right_up", "Top" };

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.scale = scale;
            this.Rotation = rotation;
            this.location = location;

            this.LoadModel("Models/Solarplant/solarplant");
            this.AddTexture("Models/Solarplant/Base");
            this.AddTexture("Models/Solarplant/Panel");
            this.AddTexture("Models/Solarplant/Panel");
            this.AddTexture("Models/Solarplant/Panel");
            this.AddTexture("Models/Solarplant/Panel");
            this.AddTexture("Models/Solarplant/Top");

            ConstructionAnimation = new AnimConstruction(this.model, this.BoneNames);
            ActiveAnimation = new AnimActive(this.model, this.BoneNames);
            IdleAnimation = new AnimIdle(this.model, this.BoneNames);

            ConstructionAnimation.NextAnimation = ActiveAnimation;

            CurrentAnimation = ConstructionAnimation;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsGhost) { CurrentAnimation = IdleAnimation; }

            CurrentAnimation.Update(gameTime);
            if (CurrentAnimation.HasFinished) { CurrentAnimation = CurrentAnimation.NextAnimation; }


            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }
            if (!IsGhost)
            {
                GetSimEntity();
            }
        }



        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            CurrentAnimation.SetModelTransforms();
            
            base.Draw(currentViewMatrix, cameraPosition);




            /*
            Matrix worldMatrix = GetWorldMatrix();
            Matrix[] transforms = GetBoneTransforms();

            int meshNum = 0;
            foreach (ModelMesh mesh in model.Meshes)
            {

                foreach (Effect currentEffect in mesh.Effects)
                {

                    currentEffect.Parameters["View"].SetValue(currentViewMatrix);
                    currentEffect.Parameters["Projection"].SetValue(projectionMatrix);
                    currentEffect.CurrentTechnique = currentEffect.Techniques["Textured"];

                    currentEffect.Parameters["xTexture"].SetValue(this.texture[meshNum]);
                    currentEffect.Parameters["World"].SetValue(transforms[mesh.ParentBone.Index] * worldMatrix);

                    currentEffect.Parameters["xIsTransparant"].SetValue(this.IsTransparant);
                    if (CanPlace)
                    {
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(0.0f, 1.0f, 0.0f, 0.5f));
                    }
                    else
                    {
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(1.0f, 0.0f, 0.0f, 0.5f));
                    }

                    if (HasMouseFocus)
                    {
                        currentEffect.Parameters["xIsTransparant"].SetValue(true);
                        currentEffect.Parameters["xTransparantColor"].SetValue(new Vector4(1.0f, 1.0f, 0.0f, 0.7f));
                    }

                    currentEffect.Parameters["xEnableLighting"].SetValue(true);
                    currentEffect.Parameters["xAmbient"].SetValue(this.RunningGameSession.LODMap.GetRenderer().AmbientLightLevel);
                    currentEffect.Parameters["LightDirection"].SetValue(this.RunningGameSession.LODMap.GetRenderer().SunLight.GetInvertedLightDirection());

                    currentEffect.Parameters["cameraPos"].SetValue(cameraPosition);
                    currentEffect.Parameters["FogColor"].SetValue(FOGCOLOR.ToVector4());
                    currentEffect.Parameters["FogNear"].SetValue(FOGNEAR);
                    currentEffect.Parameters["FogFar"].SetValue(FOGFAR);


                }
                mesh.Draw();
                meshNum++;
            }
             */ 
              
              
              
            if (ShowBoundingBox)
            {
                DrawBoundingBox(currentViewMatrix, cameraPosition);
            }

            if (HasMouseFocus && !HideBillboard)
            {
                statusBillboard.Draw(this.playerCamera);
            }


            // reset model
            CurrentAnimation.ResetModelTransforms();

        }

        public override void DrawShadow(Matrix currentViewMatrix, Matrix projectionMatrix, Vector3 cameraPosition)
        {
            CurrentAnimation.SetModelTransforms();
            base.DrawShadow(currentViewMatrix, projectionMatrix, cameraPosition);
            CurrentAnimation.ResetModelTransforms();
        }



        SpriteBatch spriteBatch;
        public void UpdateStatusScreen()
        {
            // TODO modifyvto allow custom string and images.
            if (!IsGhost && statusBillboard.BillboardBackGroundTexture != null)
            {
                float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.ResourceStorage.Resource.ELECTRICITY);
                float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.ResourceStorage.Resource.ELECTRICITY);
                
                spriteBatch = new SpriteBatch(this.RunningGameSession.device);


                RenderTarget2D target = new RenderTarget2D(this.device, 400, 400);
                this.device.SetRenderTarget(target);// Now the spriteBatch will render to the RenderTarget2D

                this.device.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(statusBillboard.BillboardBackGroundTexture, new Rectangle(0, 0, 400, 400), Color.White);

                StringBuilder sb = new StringBuilder();
                sb.Append("Power: ");
                sb.Append(electicAvail.ToString("0.0"));
                sb.Append("/");
                sb.Append(electricMax);
                sb.AppendLine();
                sb.Append(this.getAltitude());
                spriteBatch.DrawString(this.font, sb.ToString(), new Vector2(20, 20), Color.Black);//Do your stuff here

                spriteBatch.End();

                this.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }

        public override Simulation.SimulationBuildingEnity GetSimEntity()
        {
            if (solarSimEntity == null)
            {
                solarSimEntity = new SolorPlantSim(this);
            }

            return solarSimEntity;
        }

        SolorPlantSim solarSimEntity;

        public static Vector3 StandardScale = new Vector3(10, 10, 10);    // (5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);





        private class SolorPlantSim : Simulation.SimulationBuildingEnity
        {
            private SolarPlant Parent;

            public SolorPlantSim(SolarPlant parent)
            {
                this.Parent = parent;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = 0.01f;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = 1000;

                if (!parent.IsGhost)
                {
                    this.Parent.RunningGameSession.AvailableResources.AddResourceAmount(Simulation.ResourceStorage.Resource.ELECTRICITY, 100);
                }

            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = 0; i < Simulation.ResourceStorage.ResourceCount; i++)
                {
                 //   ResourceOutput[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = clamp(ResourceOutput[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] +
                   //     (ProduceRate[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] * timeDelta), ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.ELECTRICITY]);




                    
                }
            }




          //  protected float[] ResouceAmount;
         //   protected float[] ResourceConsumptionPerSecond;
          //  protected float[] ResourceMaxAmount;

//           protected Resource[] ConvertsTo;
  //          protected float[] ProduceRate;

    //        protected bool[] ConsumingResource;

    //        protected float[] ResourceOutput;

      //      public const int ResourceCount = 3;
        //    public enum Resource { NOTHING, ELECTRICITY, ORE, METAL };
            
        }








        AnimConstruction ConstructionAnimation;
        AnimActive ActiveAnimation;
        AnimIdle IdleAnimation;

        Entities.Animation CurrentAnimation;


        private class AnimConstruction : Entities.Animation
        {
            public AnimConstruction(Model model, string[] boneNames)
                : base(model, boneNames)
            {
                this.Reset();
                Transform = Matrix.CreateTranslation(0.4899f, -1.19249f, 1.02549f) + 
                    Matrix.CreateRotationZ(0.02041f) + 
                    Matrix.CreateRotationY(-1.338318f);

                transformBack = Matrix.CreateRotationY(1.338318f) +
                    Matrix.CreateRotationZ(-0.02041f) + 
                    Matrix.CreateTranslation(-0.4899f, +1.19249f, -1.02549f);
            }

            Matrix Transform;
            Matrix transformBack;

            public override void Reset()
            {
                base.Reset();
   //             this.currentposition = new float[] { 0.0f, 11.0f, 12.0f, 13.0f };
    //            this.currentSpeed = new float[] { 0.0f, 10.0f, 10.0f, 10.0f };


            }

            public override void SetModelTransforms()
            {
                //  "Left_down", "Left_up", "Right_down", "Right_up"

                this.model.Bones["Base"].Transform = this.InitialBoneTransforms[BaseIndex];


                float shift = 1.308f;
                float angle = MathHelper.ToRadians(24.85f);
                float angle2 = MathHelper.ToRadians(-0.49f);


                // TODO find correct rotation axis
                //this.model.Bones["Left_down"].Transform = Matrix.CreateTranslation(0, -1.5f, 0) * Matrix.CreateRotationX(PanelLeftDownRotation) * Matrix.CreateTranslation(0, 1.5f, 0) * this.InitialBoneTransforms[Left_downIndex];
                this.model.Bones["Left_down"].Transform =
                    //Matrix.CreateTranslation(0, -shift, 0) *
                    //Matrix.CreateRotationZ(angle2) *
                    //Matrix.CreateRotationY(angle) *
                    
                    Transform *


           //         Matrix.CreateRotationX(PanelLeftDownRotation) *
                    
        //            transformBack *

                    //Matrix.CreateRotationY(-angle) *
                    //Matrix.CreateRotationZ(-angle2) *
                    //Matrix.CreateTranslation(0, shift, 0) * 
                    this.InitialBoneTransforms[Left_downIndex];
                
                
                this.model.Bones["Left_up"].Transform = this.InitialBoneTransforms[Left_upIndex];
                this.model.Bones["Right_down"].Transform = this.InitialBoneTransforms[Right_downIndex];
                this.model.Bones["Right_up"].Transform = this.InitialBoneTransforms[Right_upIndex];
                this.model.Bones["Top"].Transform = this.InitialBoneTransforms[TopIndex];


           //     this.model.Bones["Base"].Transform = this.InitialBoneTransforms[BaseIndex] * Matrix.CreateTranslation(0, 0, currentposition[BaseIndex]);
            //    this.model.Bones["Drill"].Transform = this.InitialBoneTransforms[DrillIndex] * Matrix.CreateTranslation(0, 0, currentposition[DrillIndex]);
             //   this.model.Bones["Top"].Transform = this.InitialBoneTransforms[TopIndex] * Matrix.CreateTranslation(0, 0, currentposition[TopIndex]);
              //  this.model.Bones["Wheel"].Transform = this.InitialBoneTransforms[WheelIndex] * Matrix.CreateTranslation(0, 0, currentposition[WheelIndex]);
            }

            public override void Update(GameTime gameTime)
            {
                PanelLeftDownRotation = PanelLeftDownRotation + (float)(PanelSpeed * gameTime.ElapsedGameTime.TotalSeconds);
                if (PanelLeftDownRotation > PanelLeftDownMaxRotation) { PanelLeftDownRotation = PanelLeftDownMaxRotation; }


    //            for (int i = 0; i < this.InitialBoneTransforms.Length; i++)
     //           {
      //              currentposition[i] -= (float)(currentSpeed[i] * gameTime.ElapsedGameTime.TotalSeconds);
       //             if (currentposition[i] <= 0)
        //            {
         //               currentposition[i] = 0;
          //          }
           //     }

                // has this animation finished?
      //          this.HasFinished = true;
       //         for (int i = 0; i < this.InitialBoneTransforms.Length; i++)
        //        {
         //           if (currentposition[i] > 0)
          //          {
           //             this.HasFinished = false;
            //            break;
             //       }
              //  }
            }



            // construction animation

//            float[] currentposition;
  //          float[] currentSpeed;


            private float PanelLeftDownRotation = 0;
            private float PanelLeftDownMaxRotation = (float)Math.PI;
            private float PanelSpeed = 0.2f;
        }






        private class AnimActive : Entities.Animation
        {
            public AnimActive(Model model, string[] boneNames)
                : base(model, boneNames)
            {
                this.Reset();
            }

            public override void Reset()
            {
                base.Reset();

            }

            public override void SetModelTransforms()
            {
                this.model.Bones["Base"].Transform = this.InitialBoneTransforms[BaseIndex];
                this.model.Bones["Left_down"].Transform = this.InitialBoneTransforms[Left_downIndex];
                this.model.Bones["Left_up"].Transform = this.InitialBoneTransforms[Left_upIndex];
                this.model.Bones["Right_down"].Transform = this.InitialBoneTransforms[Right_downIndex];
                this.model.Bones["Right_up"].Transform = this.InitialBoneTransforms[Right_upIndex];
                this.model.Bones["Top"].Transform = this.InitialBoneTransforms[TopIndex];


          //      this.model.Bones["Wheel"].Transform = Matrix.CreateRotationZ(WheelRotation) * this.InitialBoneTransforms[WheelIndex];
           //     this.model.Bones["Drill"].Transform = Matrix.CreateRotationZ(DrillRotation) * this.InitialBoneTransforms[DrillIndex] *
            //        Matrix.CreateTranslation(0, 0, DrillPosition);
            }

            public override void Update(GameTime gameTime)
            {
    //            WheelRotation += (float)((WheelRotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);
     //           DrillRotation += (float)((DrillRotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);

       //         DrillPosition += (float)((DrillSpeed));
        //        if (DrillPosition > DrillMaxPosition || DrillPosition < DrillMinPosition) { DrillSpeed = -DrillSpeed; }
            }


            // Wheel

    //        private float WheelRotation = 0;
     //       private float WheelRotationSpeed = 1;

            // Drill

     //       private float DrillRotation = 0;
    //        private float DrillRotationSpeed = 1;

     //       private float DrillPosition = 0;
    //        private float DrillMinPosition = 0;
     //       private float DrillMaxPosition = 1;
     //       private float DrillSpeed = 0.01f;


            


        }



        private class AnimIdle : Entities.Animation
        {
            public AnimIdle(Model model, string[] boneNames)
                : base(model, boneNames)
            {
                this.Reset();
            }

            public override void Reset()
            {
                base.Reset();

            }

            public override void SetModelTransforms()
            {
                this.model.Bones["Base"].Transform = this.InitialBoneTransforms[BaseIndex];
                this.model.Bones["Left_down"].Transform = this.InitialBoneTransforms[Left_downIndex];
                this.model.Bones["Left_up"].Transform = this.InitialBoneTransforms[Left_upIndex];
                this.model.Bones["Right_down"].Transform = this.InitialBoneTransforms[Right_downIndex];
                this.model.Bones["Right_up"].Transform = this.InitialBoneTransforms[Right_upIndex];
                this.model.Bones["Top"].Transform = this.InitialBoneTransforms[TopIndex];
            }

            public override void Update(GameTime gameTime)
            {
                ; // do nothing
            }






        }

    }
}
