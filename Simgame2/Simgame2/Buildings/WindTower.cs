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
    public class WindTower : EntityBuilding
    {
        public WindTower(GameSession.GameSession RuningGameSession)
            : base(RuningGameSession)
        {
            this.Type = EntityTypes.WIND_TOWER;
            this.CurrentState = States.UNDER_CONSTRUCTION;

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
            */

            return false;
        }

        public void Initialize(Vector3 scale, Vector3 rotation)
        {
            this.AddTexture("WindTower_bTex");  // base texture
            this.AddTexture("WindTower_rTex");  // rotor texture
          

            this.scale = scale;
            this.Rotation = rotation;
            this.location = location;

            this.LoadModel("Models/WindTower");

            rotorTransform = this.model.Bones["Rotor"].Transform;  // Rotor start position
        }

        public override void Update(GameTime gameTime)
        {
            if (CurrentState == States.UNDER_CONSTRUCTION)
            {
                if (this.rotationSpeed >= rotationMaxSpeed)
                {
                    this.rotationSpeed = this.rotationMaxSpeed;
                    this.CurrentState = States.IDLE;
                }
                else
                {
                    this.rotationSpeed += (float)(rotationAcceleration * gameTime.ElapsedGameTime.TotalSeconds);
                }
            }


            rotorRotation += (float)((rotationSpeed * gameTime.ElapsedGameTime.TotalSeconds) % 2*Math.PI);

      //      this.model.Bones["Rotor"].Transform = Matrix.CreateRotationZ(rotorRotation) * rotorTransform; 

            if (HasMouseFocus)
            {
                UpdateStatusScreen();
            }
            base.Update(gameTime);
        }



        public override void Draw(Matrix currentViewMatrix, Vector3 cameraPosition)
        {
            this.model.Bones["Rotor"].Transform = Matrix.CreateRotationZ(rotorRotation) * rotorTransform; 

              /*  
            if (ShowBoundingBox)
            {
                DrawBoundingBox(currentViewMatrix, cameraPosition);
            }
            */
            base.Draw(currentViewMatrix, cameraPosition);

            if (HasMouseFocus && !HideBillboard)
            {
                statusBillboard.Draw(this.playerCamera);
            }
        }


        SpriteBatch spriteBatch;
        public void UpdateStatusScreen()
        {
            // TODO modifyvto allow custom string and images.
            if (statusBillboard.BillboardBackGroundTexture != null)
            {
                float electicAvail = GetSimEntity().GetAvailableOutResource(Simulation.ResourceStorage.Resource.ELECTRICITY);
                float electricMax = GetSimEntity().GetMaxResourceAmount(Simulation.ResourceStorage.Resource.ELECTRICITY);

                spriteBatch = new SpriteBatch(this.device);


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
                sb.AppendLine(this.CurrentState.ToString());

                spriteBatch.DrawString(this.font, sb.ToString(), new Vector2(20, 20), Color.Black);

                spriteBatch.End();

                this.device.SetRenderTarget(null);//This will set the spriteBatch to render to the screen again.

                this.statusBillboard.SetTexture((Texture2D)target);

            }

        }


        public override Simulation.SimulationBuildingEnity GetSimEntity()
        {
            if (windTowerSim == null)
            {
                windTowerSim = new WindTowerSim();
            }

            return windTowerSim;
        }


    //    public Effect effect { get; set; }

        public static Vector3 StandardScale = new Vector3(5, 5, 5);
        public static Vector3 StandardRotation = new Vector3(0, MathHelper.Pi, 0);

        private float rotorRotation = 0;
        private float rotationSpeed = 0;
        private float rotationAcceleration = 0.01f;
        private float rotationMaxSpeed = 1;
        private Matrix rotorTransform;

        private WindTowerSim windTowerSim;

        private class WindTowerSim : Simulation.SimulationBuildingEnity
        {
            public WindTowerSim()
            {
                ProduceRate[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = 0.01f;
                ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = 1000;
            }


            public override void Update(GameTime gameTime)
            {
                float timeDelta = gameTime.ElapsedGameTime.Milliseconds;
                for (int i = 0; i < Simulation.ResourceStorage.ResourceCount; i++)
                {
                    ResourceOutput[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] = clamp(ResourceOutput[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] +
                        (ProduceRate[(int)Simulation.ResourceStorage.Resource.ELECTRICITY] * timeDelta), ResourceMaxAmount[(int)Simulation.ResourceStorage.Resource.ELECTRICITY]);
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

    }
}
