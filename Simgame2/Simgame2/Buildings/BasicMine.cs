﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Simgame2.Buildings
{
    public class BasicMine : EntityBuilding, Simulation.Events.EventReceiver
    {
        public BasicMine(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.BASIC_MINE;
            
        }


        public override void Reset()
        {
            if (!IsGhost)
            {
                CurrentAnimation = ConstructionAnimation;
                CurrentAnimation.Reset();
            }
        }

        public override bool OnEvent(Simulation.Event ReceivedEvent)
        {
            base.OnEvent(ReceivedEvent);
            // TODO IMPLEMENT
/*
            if (ReceivedEvent is Simulation.Events.BuildingContructionDoneEvent)
            {
                Simulation.Events.BuildingContructionDoneEvent e = (Simulation.Events.BuildingContructionDoneEvent)ReceivedEvent;

                Console.WriteLine("Saw New Building");

                return true;
            }

            if (ReceivedEvent is Simulation.Events.EntityLostFocus)
            {
                Simulation.Events.EntityLostFocus e = (Simulation.Events.EntityLostFocus)ReceivedEvent;

                if (e.SourceEntity == this)
                {
                    this.HasMouseFocus = false;
                }

            }

            if (ReceivedEvent is Simulation.Events.EntityHasFocusEvent)
            {
                Simulation.Events.EntityHasFocusEvent e = (Simulation.Events.EntityHasFocusEvent)ReceivedEvent;

                if (e.SourceEntity == this)
                {
                    this.HasMouseFocus = true;
                }

            }
            */
            return false;
        }

        // Parts
        const int BaseIndex = 0;
        const int DrillIndex = 1;
        const int TopIndex = 2;
        const int WheelIndex = 3;

  

        private string[] BoneNames = { "Base", "Drill", "Top", "Wheel" };

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.AddTexture("Models/Mine/Base");
            this.AddTexture("Models/Mine/Drill");
            this.AddTexture("Models/Mine/Top");
            this.AddTexture("Models/Mine/Wheel");

            this.scale = scale;
            this.Rotation = rotation;
            this.location = location;

            this.LoadModel("Models/Mine/Mine");
            ConstructionAnimation = new AnimConstruction(this.model, this.BoneNames);
            ActiveMineAnimation = new AnimActive(this.model, this.BoneNames);
            IdleAnimation = new AnimIdle(this.model, this.BoneNames);

            ConstructionAnimation.NextAnimation = ActiveMineAnimation;

            CurrentAnimation = ConstructionAnimation;

            
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (this.IsGhost) { CurrentAnimation = IdleAnimation; }

            CurrentAnimation.Update(gameTime);
            if (CurrentAnimation.HasFinished) { CurrentAnimation = CurrentAnimation.NextAnimation;  }


            


            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }

            if (!IsGhost)
            {
                GetSimEntity().Update(gameTime);
            }
        }

 
        ResourceCell resourceCell;

        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {

            CurrentAnimation.SetModelTransforms();
 
            base.Draw(currentViewMatrix, cameraPosition);

            if (this.HasMouseFocus && !HideBillboard)
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
            if (this.resourceCell == null)
            {
                resourceCell = this.getResourceCell(location.X, location.Z);
            }
            // TODO modifyvto allow custom string and images.
            if (statusBillboard.BillboardBackGroundTexture != null)
            {
               // float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.SimulationEnity.Resource.ORE);
               // float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.SimulationEnity.Resource.ORE);

                spriteBatch = new SpriteBatch(this.device);


                RenderTarget2D target = new RenderTarget2D(this.device, 400, 600);
                this.device.SetRenderTarget(target);// Now the spriteBatch will render to the RenderTarget2D

                this.device.Clear(Color.Transparent);

                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
                spriteBatch.Draw(statusBillboard.BillboardBackGroundTexture, new Rectangle(0, 0, 400, 600), Color.White);

            //    StringBuilder sb = new StringBuilder();
             //   sb.Append("Ore: ");
              //  sb.Append(electicAvail.ToString("0.0"));
               // sb.Append("/");
                //sb.Append(electricMax);

                spriteBatch.DrawString(this.font, GetSimEntity().ToString(), new Vector2(20, 20), Color.Black);

                spriteBatch.End();

                this.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }

        public override Simulation.SimulationBuildingEnity GetSimEntity()
        {
            if (basicMineSim == null)
            {
                if (this.resourceCell == null)
                {
                    resourceCell = this.getResourceCell(location.X, location.Z);
                }
                basicMineSim = new BasicMineSim(this);
                
            }

            return basicMineSim;
        }


        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);
        
 

        private BasicMineSim basicMineSim;



        private class BasicMineSim : Simulation.SimulationBuildingEnity
        {
            private BasicMine mineParent;

            public BasicMineSim(BasicMine mineParent)
            {
                this.mineParent = mineParent;
                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreAluminium] = this.mineParent.resourceCell.Aluminium;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreAluminium] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreCopper] = this.mineParent.resourceCell.Copper;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreCopper] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreGold] = this.mineParent.resourceCell.Gold;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreGold] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreIron] = this.mineParent.resourceCell.Iron;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreIron] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreLead] = this.mineParent.resourceCell.Lead;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreLead] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreLithium] = this.mineParent.resourceCell.Lithium;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreLithium] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreNickel] = this.mineParent.resourceCell.Nickel;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreNickel] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OrePlatinum] = this.mineParent.resourceCell.Platinum;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OrePlatinum] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreSilver] = this.mineParent.resourceCell.Silver;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreSilver] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreTitanium] = this.mineParent.resourceCell.Titanium;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreTitanium] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreTungsten] = this.mineParent.resourceCell.Tungsten;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreTungsten] = 1000;

                ProduceRate[(int)Simulation.ResourceStorage.Resource.OreUranium] = this.mineParent.resourceCell.Uranium;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.OreUranium] = 1000;
            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = 0; i < Simulation.ResourceStorage.ResourceCount; i++)
                {
                 //   ResourceOutput[i] = clamp(ResourceOutput[i] +
                  //      (ProduceRate[i]/100 * timeDelta), ResourceMaxAmount[i]);
                    mineParent.RunningGameSession.AvailableResources.AddResourceAmount((Simulation.ResourceStorage.Resource)i, (ProduceRate[i] / 100 * timeDelta));
                   
                   
                }
            }


            public override string ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                for (int i = 2; i < 14; i++)
                {
                    sb.Append((Simulation.ResourceStorage.Resource)(i)); sb.Append(": ");  //sb.Append((int)ResourceOutput[i]);
                    //sb.Append("/"); sb.Append(ResourceMaxAmount[i]); sb.Append(" @ ");
                    sb.Append(ProduceRate[i]);
                    sb.AppendLine();
                }


                return sb.ToString();
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
        AnimActive ActiveMineAnimation;
        AnimIdle IdleAnimation;

        Entities.Animation CurrentAnimation;


        private class AnimConstruction : Entities.Animation
        {
            public AnimConstruction(Model model, string[] boneNames):base(model, boneNames)
            {
                this.Reset();
                
            }

            public override void Reset()
            {
                base.Reset();
                this.currentposition = new float[] { 0.0f, 11.0f, 12.0f, 13.0f };
                this.currentSpeed = new float[] { 0.0f, 10.0f, 10.0f, 10.0f };

             //   this.currentposition = new float[] { 10.0f, 11.0f, 12.0f, 13.0f };
              //  this.currentSpeed = new float[] { 10.0f, 10.0f, 10.0f, 10.0f };
            }

            public override void SetModelTransforms()
            {
                this.model.Bones["Base"].Transform = this.InitialBoneTransforms[BaseIndex] * Matrix.CreateTranslation(0, 0, currentposition[BaseIndex]);
                this.model.Bones["Drill"].Transform = this.InitialBoneTransforms[DrillIndex] * Matrix.CreateTranslation(0, 0, currentposition[DrillIndex]);
                this.model.Bones["Top"].Transform = this.InitialBoneTransforms[TopIndex] * Matrix.CreateTranslation(0, 0, currentposition[TopIndex]);
                this.model.Bones["Wheel"].Transform = this.InitialBoneTransforms[WheelIndex] * Matrix.CreateTranslation(0, 0, currentposition[WheelIndex]); 
            }

            public override void Update(GameTime gameTime)
            {
                for (int i = 0; i < this.InitialBoneTransforms.Length; i++)
                {
                    currentposition[i] -= (float)(currentSpeed[i] * gameTime.ElapsedGameTime.TotalSeconds);
                    if (currentposition[i] <= 0)
                    {
                        currentposition[i] = 0;
                    }
                }

                // has this animation finished?
                this.HasFinished = true;
                for (int i = 0; i < this.InitialBoneTransforms.Length; i++)
                {
                    if (currentposition[i] > 0)
                    {
                        this.HasFinished = false;
                        break;
                    }
                }
            }



            // construction animation

            float[] currentposition;
            float[] currentSpeed;


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
                this.model.Bones["Wheel"].Transform = Matrix.CreateRotationZ(WheelRotation) * this.InitialBoneTransforms[WheelIndex];
                this.model.Bones["Drill"].Transform = Matrix.CreateRotationZ(DrillRotation) * this.InitialBoneTransforms[DrillIndex] *
                    Matrix.CreateTranslation(0, 0, DrillPosition);
            }

            public override void Update(GameTime gameTime)
            {
                WheelRotation += (float)((WheelRotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);
                DrillRotation += (float)((DrillRotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2 * Math.PI);

                DrillPosition += (float)((DrillSpeed));
                if (DrillPosition > DrillMaxPosition || DrillPosition < DrillMinPosition) { DrillSpeed = -DrillSpeed; }
            }


            // Wheel

            private float WheelRotation = 0;
            private float WheelRotationSpeed = 1;

            // Drill

            private float DrillRotation = 0;
            private float DrillRotationSpeed = 1;

            private float DrillPosition = 0;
            private float DrillMinPosition = 0;
            private float DrillMaxPosition = 1;
            private float DrillSpeed = 0.01f;
 


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
                this.model.Bones["Drill"].Transform = this.InitialBoneTransforms[DrillIndex];
                this.model.Bones["Top"].Transform = this.InitialBoneTransforms[TopIndex];
                this.model.Bones["Wheel"].Transform = this.InitialBoneTransforms[WheelIndex];
            }

            public override void Update(GameTime gameTime)
            {
                ; // do nothing
            }


           



        }



    }
}
